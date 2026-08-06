using System.Text.Json;
using NoxAeterna.App.Preferences;
using NoxAeterna.Domain.Tarot;
using NoxAeterna.Presentation.Localization;
using NoxAeterna.Presentation.Preferences;
using NoxAeterna.Presentation.Tarot;
using NoxAeterna.Presentation.Theming;

namespace NoxAeterna.Tests.App;

public sealed class JsonUserPreferencesStoreTests
{
    [Fact]
    public void DefaultPath_IsLocalApplicationDataNoxAeternaSettingsJson()
    {
        var localApplicationData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        var path = UserPreferencesPathResolver.GetSettingsPath();

        Assert.Equal(
            Path.GetFullPath(Path.Combine(localApplicationData, "NoxAeterna", "settings.json")),
            path);
        Assert.False(path.StartsWith(Path.GetFullPath(AppContext.BaseDirectory), StringComparison.OrdinalIgnoreCase));
        Assert.NotEqual(Path.GetFullPath("settings.json"), path);
    }

    [Fact]
    public void InjectedPath_IsResolvedBelowSuppliedUserDataRoot()
    {
        var root = Path.Combine(Path.GetTempPath(), $"NoxAeterna-settings-root-{Guid.NewGuid():N}");

        var path = UserPreferencesPathResolver.GetSettingsPath(root);

        Assert.Equal(Path.Combine(Path.GetFullPath(root), "NoxAeterna", "settings.json"), path);
        Assert.StartsWith(Path.GetFullPath(root), path, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Load_MissingFile_ReturnsDefaultsWithoutDiagnosticOrCreatingFile()
    {
        using var fixture = new SettingsFixture();
        var store = fixture.CreateStore();

        var result = store.Load();

        Assert.Equal(UserPreferencesDefaults.Create(), result.Preferences);
        Assert.Null(result.Diagnostic);
        Assert.False(File.Exists(fixture.SettingsPath));
        Assert.False(Directory.Exists(Path.GetDirectoryName(fixture.SettingsPath)));
    }

    [Fact]
    public void SaveAndLoad_RoundTripsEveryPreferenceAndWritesSchemaVersionOne()
    {
        using var fixture = new SettingsFixture();
        var store = fixture.CreateStore();
        var expected = CreateDistinctPreferences();

        var save = store.Save(expected);
        var load = store.Load();

        Assert.True(save.IsSuccess);
        Assert.Null(save.Diagnostic);
        Assert.Null(load.Diagnostic);
        Assert.Equal(expected, load.Preferences);
        using var json = JsonDocument.Parse(File.ReadAllText(fixture.SettingsPath));
        Assert.Equal(1, json.RootElement.GetProperty("schemaVersion").GetInt32());
    }

    [Fact]
    public void Save_WritesOnlyVersionedPrimitivePreferenceDocument()
    {
        using var fixture = new SettingsFixture();
        var store = fixture.CreateStore();

        Assert.True(store.Save(CreateDistinctPreferences()).IsSuccess);

        using var json = JsonDocument.Parse(File.ReadAllText(fixture.SettingsPath));
        Assert.Equal(
            new[] { "schemaVersion", "applicationLanguage", "interpretationLanguage", "theme", "tarot" },
            json.RootElement.EnumerateObject().Select(property => property.Name));
        var tarot = json.RootElement.GetProperty("tarot");
        Assert.Equal(
            new[] { "spreadId", "artworkPackId", "backVariantId", "allowReversed", "autoRevealCards" },
            tarot.EnumerateObject().Select(property => property.Name));
        Assert.Equal(JsonValueKind.String, json.RootElement.GetProperty("applicationLanguage").ValueKind);
        Assert.Equal(JsonValueKind.String, tarot.GetProperty("spreadId").ValueKind);
        Assert.Equal(JsonValueKind.True, tarot.GetProperty("allowReversed").ValueKind);
        Assert.Equal(JsonValueKind.False, tarot.GetProperty("autoRevealCards").ValueKind);

        var serialized = File.ReadAllText(fixture.SettingsPath);
        foreach (var forbidden in new[]
                 {
                     "currentReading", "cards", "drawnAt", "random", "revealedPositions",
                     "selectedCard", "interpretationResult", "bitmap", "scrollOffset", "failure"
                 })
        {
            Assert.DoesNotContain($"\"{forbidden}\"", serialized, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void Save_CreatesParentDirectoryAndHumanReadableValidJson()
    {
        using var fixture = new SettingsFixture();
        var store = fixture.CreateStore();
        Assert.False(Directory.Exists(Path.GetDirectoryName(fixture.SettingsPath)));

        var result = store.Save(UserPreferencesDefaults.Create());

        Assert.True(result.IsSuccess);
        Assert.True(Directory.Exists(Path.GetDirectoryName(fixture.SettingsPath)));
        var content = File.ReadAllText(fixture.SettingsPath);
        Assert.Contains(Environment.NewLine, content, StringComparison.Ordinal);
        Assert.Contains("  \"schemaVersion\": 1", content, StringComparison.Ordinal);
        using var json = JsonDocument.Parse(content);
        Assert.Equal(JsonValueKind.Object, json.RootElement.ValueKind);
    }

    [Fact]
    public void Save_SuccessLeavesNoTemporaryFile()
    {
        using var fixture = new SettingsFixture();
        var store = fixture.CreateStore();

        var result = store.Save(UserPreferencesDefaults.Create());

        Assert.True(result.IsSuccess);
        Assert.Equal(
            new[] { "settings.json" },
            Directory.GetFiles(Path.GetDirectoryName(fixture.SettingsPath)!)
                .Select(Path.GetFileName)
                .Order(StringComparer.Ordinal));
    }

    [Fact]
    public void Load_FalseAutoRevealValueIsPreserved()
    {
        using var fixture = new SettingsFixture();
        var store = fixture.CreateStore();
        var expected = UserPreferencesDefaults.Create() with
        {
            Tarot = TarotWorkspacePreferences.CreateDefault() with { AutoRevealCards = false }
        };

        Assert.True(store.Save(expected).IsSuccess);
        var result = store.Load();

        Assert.Null(result.Diagnostic);
        Assert.False(result.Preferences.Tarot.AutoRevealCards);
    }

    [Fact]
    public void Load_MalformedJson_ReturnsControlledDefaultsAndStructuredDiagnostic()
    {
        using var fixture = new SettingsFixture();
        fixture.WriteRaw("{ definitely-not-json }");

        var result = fixture.CreateStore().Load();

        Assert.Equal(UserPreferencesDefaults.Create(), result.Preferences);
        Assert.Equal(UserPreferencesDiagnosticCode.MalformedJson, result.Diagnostic?.Code);
        Assert.False(string.IsNullOrWhiteSpace(result.Diagnostic?.Message));
        Assert.DoesNotContain("System.Text.Json", result.Diagnostic!.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Load_UnsupportedSchemaVersion_ReturnsControlledDefaultsAndStructuredDiagnostic()
    {
        using var fixture = new SettingsFixture();
        fixture.WriteRaw(ValidJson(schemaVersion: 2));

        var result = fixture.CreateStore().Load();

        Assert.Equal(UserPreferencesDefaults.Create(), result.Preferences);
        Assert.Equal(UserPreferencesDiagnosticCode.UnsupportedSchemaVersion, result.Diagnostic?.Code);
        Assert.Contains("2", result.Diagnostic!.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("applicationLanguage", "unknown")]
    [InlineData("interpretationLanguage", "unknown")]
    [InlineData("theme", "archive")]
    [InlineData("spreadId", "unknown-spread")]
    [InlineData("artworkPackId", "classic")]
    [InlineData("backVariantId", "unknown-back")]
    public void Load_InvalidStringField_DefaultsOnlyThatField(string field, string invalidValue)
    {
        using var fixture = new SettingsFixture();
        fixture.WriteRaw(ValidJson(overrides: new Dictionary<string, string> { [field] = invalidValue }));

        var result = fixture.CreateStore().Load();

        Assert.Null(result.Diagnostic);
        var expected = CreateDistinctPreferences();
        expected = field switch
        {
            "applicationLanguage" => expected with
            {
                ApplicationLanguage = UserPreferencesDefaults.Create().ApplicationLanguage
            },
            "interpretationLanguage" => expected with
            {
                InterpretationLanguage = UserPreferencesDefaults.Create().InterpretationLanguage
            },
            "theme" => expected with { ThemeId = UserPreferencesDefaults.Create().ThemeId },
            "spreadId" => expected with
            {
                Tarot = expected.Tarot with { SpreadId = UserPreferencesDefaults.Create().Tarot.SpreadId }
            },
            "artworkPackId" => expected with
            {
                Tarot = expected.Tarot with { ArtworkPackId = UserPreferencesDefaults.Create().Tarot.ArtworkPackId }
            },
            "backVariantId" => expected with
            {
                Tarot = expected.Tarot with { BackVariantId = UserPreferencesDefaults.Create().Tarot.BackVariantId }
            },
            _ => throw new ArgumentOutOfRangeException(nameof(field))
        };
        Assert.Equal(expected, result.Preferences);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Load_UsesBooleanValuesAsRead(bool allowReversed, bool autoRevealCards)
    {
        using var fixture = new SettingsFixture();
        fixture.WriteRaw(ValidJson(allowReversed: allowReversed, autoRevealCards: autoRevealCards));

        var result = fixture.CreateStore().Load();

        Assert.Null(result.Diagnostic);
        Assert.Equal(allowReversed, result.Preferences.Tarot.AllowReversed);
        Assert.Equal(autoRevealCards, result.Preferences.Tarot.AutoRevealCards);
    }

    [Fact]
    public void Load_UnknownJsonFields_PreservesEveryKnownValue()
    {
        using var fixture = new SettingsFixture();
        fixture.WriteRaw(
            """
            {
              "schemaVersion": 1,
              "futureRoot": "ignored",
              "applicationLanguage": "en",
              "interpretationLanguage": "en",
              "theme": "light",
              "tarot": {
                "spreadId": "three-cards",
                "artworkPackId": "lupus-noctis",
                "backVariantId": "lunar-seal",
                "allowReversed": true,
                "autoRevealCards": false,
                "futureTarot": 42
              }
            }
            """);

        var result = fixture.CreateStore().Load();

        Assert.Null(result.Diagnostic);
        Assert.Equal(CreateDistinctPreferences(), result.Preferences);
    }

    [Fact]
    public void Load_MissingTarotObject_DefaultsOnlyTarotPreferences()
    {
        using var fixture = new SettingsFixture();
        fixture.WriteRaw(
            """
            {
              "schemaVersion": 1,
              "applicationLanguage": "en",
              "interpretationLanguage": "en",
              "theme": "light"
            }
            """);

        var result = fixture.CreateStore().Load();

        Assert.Null(result.Diagnostic);
        Assert.Equal(new LanguageCode("en"), result.Preferences.ApplicationLanguage.Language);
        Assert.Equal(new LanguageCode("en"), result.Preferences.InterpretationLanguage.Language);
        Assert.Equal(new ThemeId("light"), result.Preferences.ThemeId);
        Assert.Equal(TarotWorkspacePreferences.CreateDefault(), result.Preferences.Tarot);
    }

    [Fact]
    public void Load_MissingTarotBooleanFields_UsesBooleanDefaultsAndPreservesIds()
    {
        using var fixture = new SettingsFixture();
        fixture.WriteRaw(
            """
            {
              "schemaVersion": 1,
              "applicationLanguage": "en",
              "interpretationLanguage": "en",
              "theme": "light",
              "tarot": {
                "spreadId": "three-cards",
                "artworkPackId": "lupus-noctis",
                "backVariantId": "lunar-seal"
              }
            }
            """);

        var result = fixture.CreateStore().Load();

        Assert.Null(result.Diagnostic);
        Assert.Equal(StandardTarotSpreads.ThreeCards.Id, result.Preferences.Tarot.SpreadId);
        Assert.Equal(TarotPrototypeSelections.LupusNoctisArtworkPackId, result.Preferences.Tarot.ArtworkPackId);
        Assert.Equal(new TarotBackVariantId("lunar-seal"), result.Preferences.Tarot.BackVariantId);
        Assert.False(result.Preferences.Tarot.AllowReversed);
        Assert.True(result.Preferences.Tarot.AutoRevealCards);
    }

    [Fact]
    public void SaveFailure_CleansTemporaryFileAndLeavesExistingFinalTargetUntouched()
    {
        using var fixture = new SettingsFixture();
        Directory.CreateDirectory(fixture.SettingsPath);
        var sentinel = Path.Combine(fixture.SettingsPath, "existing-target.txt");
        File.WriteAllText(sentinel, "preserve");

        var result = fixture.CreateStore().Save(UserPreferencesDefaults.Create());

        Assert.False(result.IsSuccess);
        Assert.Equal(UserPreferencesDiagnosticCode.SaveFailure, result.Diagnostic?.Code);
        Assert.True(Directory.Exists(fixture.SettingsPath));
        Assert.Equal("preserve", File.ReadAllText(sentinel));
        Assert.Equal(new[] { "existing-target.txt" }, Directory.GetFiles(fixture.SettingsPath).Select(Path.GetFileName));
        Assert.Empty(Directory.GetFiles(fixture.Root, "*.tmp", SearchOption.AllDirectories));
    }

    private static UserPreferences CreateDistinctPreferences() => new(
        new ApplicationLanguagePreference(new LanguageCode("en")),
        new InterpretationLanguagePreference(new LanguageCode("en")),
        new ThemeId("light"),
        new TarotWorkspacePreferences(
            StandardTarotSpreads.ThreeCards.Id,
            TarotPrototypeSelections.LupusNoctisArtworkPackId,
            new TarotBackVariantId("lunar-seal"),
            AllowReversed: true,
            AutoRevealCards: false));

    private static string ValidJson(
        int schemaVersion = 1,
        IReadOnlyDictionary<string, string>? overrides = null,
        bool allowReversed = true,
        bool autoRevealCards = false)
    {
        string Value(string field, string fallback) => overrides?.TryGetValue(field, out var value) == true
            ? value
            : fallback;

        return $$"""
                 {
                   "schemaVersion": {{schemaVersion}},
                   "applicationLanguage": "{{Value("applicationLanguage", "en")}}",
                   "interpretationLanguage": "{{Value("interpretationLanguage", "en")}}",
                   "theme": "{{Value("theme", "light")}}",
                   "tarot": {
                     "spreadId": "{{Value("spreadId", "three-cards")}}",
                     "artworkPackId": "{{Value("artworkPackId", "lupus-noctis")}}",
                     "backVariantId": "{{Value("backVariantId", "lunar-seal")}}",
                     "allowReversed": {{allowReversed.ToString().ToLowerInvariant()}},
                     "autoRevealCards": {{autoRevealCards.ToString().ToLowerInvariant()}}
                   }
                 }
                 """;
    }

    private sealed class SettingsFixture : IDisposable
    {
        public SettingsFixture()
        {
            Root = Path.Combine(Path.GetTempPath(), $"NoxAeterna-settings-tests-{Guid.NewGuid():N}");
            SettingsPath = Path.Combine(Root, "nested", "settings.json");
        }

        public string Root { get; }

        public string SettingsPath { get; }

        public JsonUserPreferencesStore CreateStore() => new(SettingsPath);

        public void WriteRaw(string content)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
            File.WriteAllText(SettingsPath, content);
        }

        public void Dispose()
        {
            if (Directory.Exists(Root))
            {
                Directory.Delete(Root, recursive: true);
            }
        }
    }
}
