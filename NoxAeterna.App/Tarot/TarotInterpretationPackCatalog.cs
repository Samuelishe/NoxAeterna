using System.Collections.ObjectModel;
using NoxAeterna.Domain.Tarot;
using NoxAeterna.Interpretation.Tarot.Contracts;
using NoxAeterna.Interpretation.Tarot.Serialization;
using NoxAeterna.Interpretation.Tarot.Sources;
using NoxAeterna.Interpretation.Tarot.Validation;
using NoxAeterna.Presentation.Localization;
using NoxAeterna.Presentation.Tarot;

namespace NoxAeterna.App.Tarot;

/// <summary>Describes an internal package-catalog loading diagnostic.</summary>
public sealed record TarotInterpretationPackCatalogDiagnostic(
    TarotInterpretationPackId PackId,
    string Code,
    string Message);

/// <summary>Materializes stable user-facing options from validated interpretation manifests.</summary>
public sealed class TarotInterpretationPackCatalog
{
    private static readonly TarotInterpretationLocale English = new("en");
    private static readonly TarotInterpretationLocale Russian = new("ru");
    private readonly IReadOnlyDictionary<TarotInterpretationPackId, IReadOnlyDictionary<TarotInterpretationLocale, string>> displayNames;

    public TarotInterpretationPackCatalog(
        ITarotInterpretationPackSourceCatalog sourceCatalog,
        IEnumerable<TarotInterpretationPackId> sourceIds)
    {
        ArgumentNullException.ThrowIfNull(sourceCatalog);
        ArgumentNullException.ThrowIfNull(sourceIds);
        var ids = sourceIds.ToArray();
        if (ids.Distinct().Count() != ids.Length)
        {
            throw new ArgumentException("Interpretation pack IDs must be unique.", nameof(sourceIds));
        }

        var loaded = new Dictionary<TarotInterpretationPackId, TarotInterpretationPackManifest>();
        var loadedDisplayNames = new Dictionary<
            TarotInterpretationPackId,
            IReadOnlyDictionary<TarotInterpretationLocale, string>>();
        var diagnostics = new List<TarotInterpretationPackCatalogDiagnostic>();
        foreach (var packId in ids)
        {
            ArgumentNullException.ThrowIfNull(packId);
            if (!sourceCatalog.TryGetSource(packId, out var source) || source is null)
            {
                diagnostics.Add(new(packId, "source.missing", "The interpretation pack source is unavailable."));
                continue;
            }

            var read = source.ReadManifest();
            if (read.Status != TarotInterpretationSourceReadStatus.Found)
            {
                diagnostics.Add(new(
                    packId,
                    read.Diagnostic?.Code ?? "manifest.missing",
                    read.Diagnostic?.Message ?? "The interpretation pack manifest is unavailable."));
                continue;
            }

            var parsed = TarotInterpretationJson.Parse<TarotInterpretationPackDocument>(read.Bytes.Span);
            if (!parsed.IsSuccess || parsed.Document is null)
            {
                diagnostics.Add(new(packId, "manifest.json", parsed.Failure?.Message ?? "The manifest is malformed."));
                continue;
            }

            var originalDisplayNames = CopyUsableDisplayNames(parsed.Document);
            var validated = TarotInterpretationValidator.ValidateManifest(parsed.Document);
            if (!validated.IsValid && validated.Diagnostics
                    .Where(static item => item.Severity == TarotValidationSeverity.Error)
                    .All(static item => item.Code.StartsWith("manifest.display-name", StringComparison.Ordinal)))
            {
                parsed.Document.DisplayNames = parsed.Document.DeclaredLocales!
                    .Cast<string>()
                    .ToDictionary(
                        static locale => locale,
                        _ => (string?)packId.Value,
                        StringComparer.Ordinal);
                validated = TarotInterpretationValidator.ValidateManifest(parsed.Document);
                diagnostics.Add(new(
                    packId,
                    "manifest.display-name-fallback",
                    "One or more manifest display names were unavailable; UI fallback remains silent."));
            }

            if (!validated.IsValid || validated.Value is null || validated.Value.PackId != packId)
            {
                diagnostics.Add(new(
                    packId,
                    "manifest.validation",
                    validated.Diagnostics.FirstOrDefault()?.Message ?? "The manifest identity is invalid."));
                continue;
            }

            loaded.Add(packId, validated.Value);
            loadedDisplayNames.Add(
                packId,
                originalDisplayNames.Count > 0 ? originalDisplayNames : validated.Value.DisplayNames);
        }

        displayNames = new ReadOnlyDictionary<
            TarotInterpretationPackId,
            IReadOnlyDictionary<TarotInterpretationLocale, string>>(loadedDisplayNames);
        Options = Array.AsReadOnly(loaded.Keys
            .Select(static id => new TarotInterpretationPackOption(id))
            .ToArray());
        AvailablePackIds = Array.AsReadOnly(loaded.Keys.ToArray());
        Diagnostics = Array.AsReadOnly(diagnostics.ToArray());
    }

    public IReadOnlyList<TarotInterpretationPackOption> Options { get; }

    public IReadOnlyList<TarotInterpretationPackId> AvailablePackIds { get; }

    /// <summary>Gets technical diagnostics that are never materialized as user-facing text.</summary>
    public IReadOnlyList<TarotInterpretationPackCatalogDiagnostic> Diagnostics { get; }

    public string ResolveDisplayName(TarotInterpretationPackId packId, LanguageCode uiLanguage)
    {
        ArgumentNullException.ThrowIfNull(packId);
        if (!displayNames.TryGetValue(packId, out var names))
        {
            return packId.Value;
        }

        var requested = names.Keys.FirstOrDefault(
            locale => locale.Value == uiLanguage.Value);
        foreach (var locale in new[] { requested, English, Russian }.Where(static item => item is not null).Distinct())
        {
            if (names.TryGetValue(locale!, out var name) && !string.IsNullOrWhiteSpace(name))
            {
                return name;
            }
        }

        return packId.Value;
    }

    private static IReadOnlyDictionary<TarotInterpretationLocale, string> CopyUsableDisplayNames(
        TarotInterpretationPackDocument document)
    {
        var declared = document.DeclaredLocales?
            .Where(static locale => !string.IsNullOrWhiteSpace(locale))
            .Cast<string>()
            .ToHashSet(StringComparer.Ordinal) ?? [];
        var result = new Dictionary<TarotInterpretationLocale, string>();
        foreach (var (locale, name) in document.DisplayNames ?? new Dictionary<string, string?>())
        {
            if (!declared.Contains(locale) || string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            try
            {
                result.Add(new TarotInterpretationLocale(locale), name);
            }
            catch (ArgumentException)
            {
                // The manifest validator retains the technical locale diagnostic.
            }
        }

        return new ReadOnlyDictionary<TarotInterpretationLocale, string>(result);
    }
}
