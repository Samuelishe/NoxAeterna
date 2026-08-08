using System.Text.Json;
using NoxAeterna.Domain.Tarot;
using NoxAeterna.Presentation.Localization;
using NoxAeterna.Presentation.Preferences;
using NoxAeterna.Presentation.Tarot;
using NoxAeterna.Presentation.Theming;

namespace NoxAeterna.App.Preferences;

/// <summary>Loads and atomically saves the versioned AppData settings document.</summary>
public sealed class JsonUserPreferencesStore : IUserPreferencesStore
{
    public const int CurrentSchemaVersion = 2;
    private static readonly TarotInterpretationPackId CompiledDefaultInterpretationPackId = new("classic");

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    private static readonly HashSet<string> SupportedLanguages = new(["ru", "en"], StringComparer.Ordinal);
    private static readonly HashSet<string> SupportedThemes = new(["dark", "light"], StringComparer.Ordinal);
    private static readonly HashSet<string> SupportedSpreads = new(
        [
            StandardTarotSpreads.SingleCard.Id.Value,
            StandardTarotSpreads.TwoCards.Id.Value,
            StandardTarotSpreads.ThreeCards.Id.Value
        ],
        StringComparer.Ordinal);
    private static readonly HashSet<string> SupportedArtworkPacks = new(
        [TarotPrototypeSelections.LupusNoctisArtworkPackId.Value],
        StringComparer.Ordinal);
    private static readonly HashSet<string> SupportedBackVariants = new(["black-sun", "lunar-seal"], StringComparer.Ordinal);
    private readonly IReadOnlyList<TarotInterpretationPackId> availableInterpretationPackIds;

    public JsonUserPreferencesStore(
        string settingsPath,
        IEnumerable<TarotInterpretationPackId>? availableInterpretationPackIds = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(settingsPath);
        SettingsPath = Path.GetFullPath(settingsPath);
        var ids = (availableInterpretationPackIds ?? [CompiledDefaultInterpretationPackId]).ToArray();
        if (ids.Distinct().Count() != ids.Length)
        {
            throw new ArgumentException("Available interpretation pack IDs must be unique.", nameof(availableInterpretationPackIds));
        }

        this.availableInterpretationPackIds = Array.AsReadOnly(ids.ToArray());
    }

    public string SettingsPath { get; }

    public UserPreferencesLoadResult Load()
    {
        var defaults = UserPreferencesDefaults.Create();
        if (!File.Exists(SettingsPath))
        {
            return new UserPreferencesLoadResult(defaults, null);
        }

        try
        {
            using var stream = File.Open(SettingsPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var document = JsonSerializer.Deserialize<UserPreferencesDocument>(stream, SerializerOptions);
            if (document is null)
            {
                return Malformed(defaults, "The settings document is empty.");
            }

            if (document.SchemaVersion is not (1 or CurrentSchemaVersion))
            {
                return new UserPreferencesLoadResult(
                    defaults,
                    new UserPreferencesDiagnostic(
                        UserPreferencesDiagnosticCode.UnsupportedSchemaVersion,
                        $"Settings schema version {document.SchemaVersion} is not supported."));
            }

            return new UserPreferencesLoadResult(Normalize(document, defaults), null);
        }
        catch (JsonException exception)
        {
            return Malformed(defaults, exception.Message);
        }
        catch (IOException exception)
        {
            return ReadFailure(defaults, exception.Message);
        }
        catch (UnauthorizedAccessException exception)
        {
            return ReadFailure(defaults, exception.Message);
        }
        catch (NotSupportedException exception)
        {
            return ReadFailure(defaults, exception.Message);
        }
    }

    public UserPreferencesSaveResult Save(UserPreferences preferences)
    {
        ArgumentNullException.ThrowIfNull(preferences);
        var directory = Path.GetDirectoryName(SettingsPath)
            ?? throw new InvalidOperationException("The settings path has no parent directory.");
        var temporaryPath = Path.Combine(
            directory,
            $"{Path.GetFileName(SettingsPath)}.{Guid.NewGuid():N}.tmp");

        try
        {
            Directory.CreateDirectory(directory);
            using (var stream = new FileStream(
                       temporaryPath,
                       FileMode.CreateNew,
                       FileAccess.Write,
                       FileShare.None,
                       bufferSize: 4096,
                       FileOptions.WriteThrough))
            {
                JsonSerializer.Serialize(stream, CreateDocument(preferences), SerializerOptions);
                stream.Flush(flushToDisk: true);
            }

            File.Move(temporaryPath, SettingsPath, overwrite: true);
            return UserPreferencesSaveResult.Success;
        }
        catch (IOException exception)
        {
            return SaveFailure(exception.Message, temporaryPath);
        }
        catch (UnauthorizedAccessException exception)
        {
            return SaveFailure(exception.Message, temporaryPath);
        }
        catch (NotSupportedException exception)
        {
            return SaveFailure(exception.Message, temporaryPath);
        }
    }

    private UserPreferences Normalize(UserPreferencesDocument document, UserPreferences defaults)
    {
        var tarot = document.Tarot;
        return new UserPreferences(
            new ApplicationLanguagePreference(new LanguageCode(
                Normalize(document.ApplicationLanguage, SupportedLanguages, defaults.ApplicationLanguage.Language.Value))),
            new InterpretationLanguagePreference(new LanguageCode(
                Normalize(document.InterpretationLanguage, SupportedLanguages, defaults.InterpretationLanguage.Language.Value))),
            new ThemeId(Normalize(document.Theme, SupportedThemes, defaults.ThemeId.Value)),
            new TarotWorkspacePreferences(
                new TarotSpreadId(Normalize(tarot?.SpreadId, SupportedSpreads, defaults.Tarot.SpreadId.Value)),
                new TarotArtworkPackId(Normalize(tarot?.ArtworkPackId, SupportedArtworkPacks, defaults.Tarot.ArtworkPackId.Value)),
                NormalizeInterpretationPackId(
                    document.SchemaVersion == 1 ? null : tarot?.SelectedInterpretationPackId),
                new TarotBackVariantId(Normalize(tarot?.BackVariantId, SupportedBackVariants, defaults.Tarot.BackVariantId.Value)),
                tarot?.AllowReversed ?? defaults.Tarot.AllowReversed,
                tarot?.AutoRevealCards ?? defaults.Tarot.AutoRevealCards));
    }

    private static string Normalize(string? value, HashSet<string> supported, string fallback) =>
        value is not null && supported.Contains(value) ? value : fallback;

    private TarotInterpretationPackId NormalizeInterpretationPackId(string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            try
            {
                var stored = new TarotInterpretationPackId(value);
                if (availableInterpretationPackIds.Contains(stored))
                {
                    return stored;
                }
            }
            catch (ArgumentException)
            {
                // Invalid persisted IDs follow the same silent normalization policy as unknown IDs.
            }
        }

        if (availableInterpretationPackIds.Contains(CompiledDefaultInterpretationPackId))
        {
            return CompiledDefaultInterpretationPackId;
        }

        return availableInterpretationPackIds.FirstOrDefault() ?? CompiledDefaultInterpretationPackId;
    }

    private static UserPreferencesDocument CreateDocument(UserPreferences preferences) => new()
    {
        SchemaVersion = CurrentSchemaVersion,
        ApplicationLanguage = preferences.ApplicationLanguage.Language.Value,
        InterpretationLanguage = preferences.InterpretationLanguage.Language.Value,
        Theme = preferences.ThemeId.Value,
        Tarot = new TarotWorkspacePreferencesDocument
        {
            SpreadId = preferences.Tarot.SpreadId.Value,
            ArtworkPackId = preferences.Tarot.ArtworkPackId.Value,
            SelectedInterpretationPackId = preferences.Tarot.InterpretationPackId.Value,
            BackVariantId = preferences.Tarot.BackVariantId.Value,
            AllowReversed = preferences.Tarot.AllowReversed,
            AutoRevealCards = preferences.Tarot.AutoRevealCards
        }
    };

    private static UserPreferencesLoadResult Malformed(UserPreferences defaults, string message) => new(
        defaults,
        new UserPreferencesDiagnostic(UserPreferencesDiagnosticCode.MalformedJson, message));

    private static UserPreferencesLoadResult ReadFailure(UserPreferences defaults, string message) => new(
        defaults,
        new UserPreferencesDiagnostic(UserPreferencesDiagnosticCode.ReadFailure, message));

    private static UserPreferencesSaveResult SaveFailure(string message, string temporaryPath)
    {
        try
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
        catch (IOException)
        {
            // The original save failure remains authoritative; cleanup is best effort.
        }
        catch (UnauthorizedAccessException)
        {
            // The original save failure remains authoritative; cleanup is best effort.
        }

        return new UserPreferencesSaveResult(
            new UserPreferencesDiagnostic(UserPreferencesDiagnosticCode.SaveFailure, message));
    }
}
