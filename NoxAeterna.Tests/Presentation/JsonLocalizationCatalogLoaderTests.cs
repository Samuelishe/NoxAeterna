using NoxAeterna.Presentation.Localization;
using NoxAeterna.Domain.Tarot;
using NoxAeterna.Presentation.Tarot;

namespace NoxAeterna.Tests.Presentation;

public sealed class JsonLocalizationCatalogLoaderTests
{
    [Fact]
    public void LoadFromJson_LoadsFlatKeyValueCatalog()
    {
        const string json = """
                            {
                              "ui.shell.window_title": "Nox Aeterna",
                              "ui.shell.section.settings": "Settings"
                            }
                            """;

        var catalog = JsonLocalizationCatalogLoader.LoadFromJson(
            LocalizationScope.Ui,
            new LanguageCode("en"),
            json);

        Assert.Equal(LocalizationScope.Ui, catalog.Scope);
        Assert.Equal(new LanguageCode("en"), catalog.Language);
        Assert.Equal(2, catalog.Entries.Count);
        Assert.True(catalog.TryGetText(new LocalizationKey("ui.shell.window_title"), out var text));
        Assert.Equal("Nox Aeterna", text);
    }

    [Fact]
    public void LoadFromJson_RejectsMalformedJson()
    {
        Assert.ThrowsAny<Exception>(() =>
            JsonLocalizationCatalogLoader.LoadFromJson(
                LocalizationScope.Ui,
                new LanguageCode("ru"),
                "{"));
    }

    [Fact]
    public void LoadFromJson_RejectsNonStringValues()
    {
        const string json = """
                            {
                              "ui.shell.window_title": 42
                            }
                            """;

        Assert.Throws<InvalidOperationException>(() =>
            JsonLocalizationCatalogLoader.LoadFromJson(
                LocalizationScope.Ui,
                new LanguageCode("ru"),
                json));
    }

    [Fact]
    public void LoadFromJson_RejectsDuplicateKeys()
    {
        const string json = """
                            {
                              "ui.shell.window_title": "A",
                              "ui.shell.window_title": "B"
                            }
                            """;

        Assert.Throws<InvalidOperationException>(() =>
            JsonLocalizationCatalogLoader.LoadFromJson(
                LocalizationScope.Ui,
                new LanguageCode("ru"),
                json));
    }

    [Fact]
    public void LoadFromFile_CanLoadRealUiCatalog()
    {
        var filePath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..",
            "resources", "localization", "ui", "ru.json"));

        var catalog = JsonLocalizationCatalogLoader.LoadFromFile(
            LocalizationScope.Ui,
            new LanguageCode("ru"),
            filePath);

        Assert.True(catalog.TryGetText(new LocalizationKey("ui.settings.title"), out var text));
        Assert.Equal("Настройки", text);
    }

    [Fact]
    public void FallbackProvider_UsesRealRussianCatalogBeforeKeyFallback()
    {
        var ruCatalog = JsonLocalizationCatalogLoader.LoadFromFile(
            LocalizationScope.Ui,
            new LanguageCode("ru"),
            Path.GetFullPath(Path.Combine(
                AppContext.BaseDirectory,
                "..", "..", "..", "..",
                "resources", "localization", "ui", "ru.json")));

        var provider = new FallbackLocalizationProvider([ruCatalog]);

        var fallbackToRu = provider.Get(
            LocalizationScope.Ui,
            new LanguageCode("de"),
            new LocalizationKey("ui.settings.title"));

        var fallbackToKey = provider.Get(
            LocalizationScope.Ui,
            new LanguageCode("de"),
            new LocalizationKey("ui.missing.key"));

        Assert.Equal("Настройки", fallbackToRu.Text);
        Assert.Equal(new LanguageCode("ru"), fallbackToRu.ResolvedLanguage);
        Assert.Equal("ui.missing.key", fallbackToKey.Text);
        Assert.Null(fallbackToKey.ResolvedLanguage);
    }

    [Fact]
    public void RealUiCatalogs_PreserveProductNameAcrossLanguages()
    {
        var ruCatalog = LoadRealUiCatalog("ru");
        var enCatalog = LoadRealUiCatalog("en");

        Assert.True(ruCatalog.TryGetText(new LocalizationKey("ui.shell.window_title"), out var ruTitle));
        Assert.True(enCatalog.TryGetText(new LocalizationKey("ui.shell.window_title"), out var enTitle));
        Assert.Equal("Nox Aeterna", ruTitle);
        Assert.Equal("Nox Aeterna", enTitle);
    }

    [Fact]
    public void RealUiCatalogs_LocalizeEveryProjectOwnedWindowCaptionAction()
    {
        var ruCatalog = LoadRealUiCatalog("ru");
        var enCatalog = LoadRealUiCatalog("en");
        var expected = new Dictionary<string, (string Ru, string En)>
        {
            ["ui.shell.window.minimize"] = ("Свернуть", "Minimize"),
            ["ui.shell.window.maximize"] = ("Развернуть", "Maximize"),
            ["ui.shell.window.restore"] = ("Восстановить", "Restore"),
            ["ui.shell.window.close"] = ("Закрыть", "Close")
        };

        foreach (var (key, value) in expected)
        {
            Assert.Equal(value.Ru, GetRequiredText(ruCatalog, key));
            Assert.Equal(value.En, GetRequiredText(enCatalog, key));
        }
    }

    [Fact]
    public void RealRussianUiCatalog_UsesLocalizedBirthInputLabelsWithoutKnownMixedEnglishTerms()
    {
        var ruCatalog = LoadRealUiCatalog("ru");

        Assert.Equal("Часовой пояс", GetRequiredText(ruCatalog, "ui.birth_data.timezone"));
        Assert.Equal("Prague, Czechia", GetRequiredText(ruCatalog, "ui.birth_data.birth_city_or_settlement_placeholder"));
        Assert.DoesNotContain("Timezone ID", GetRequiredText(ruCatalog, "ui.birth_data.timezone"));
        Assert.Equal("Ввод времени отключён.", GetRequiredText(ruCatalog, "ui.birth_data.unknown_time_status"));
    }

    [Fact]
    public void RealUiCatalogs_ContainVisibleShellWorkspaceAndSettingsKeys()
    {
        var requiredKeys = new[]
        {
            "ui.shell.window_title",
            "ui.shell.navigation_title",
            "ui.shell.navigation.expand",
            "ui.shell.navigation.collapse",
            "ui.shell.section.astrology",
            "ui.shell.section.settings",
            "ui.astrology.panel.chart.title",
            "ui.astrology.panel.birth_data.title",
            "ui.chart.empty_state",
            "ui.birth_data.birth_date",
            "ui.birth_data.birth_time",
            "ui.birth_data.birth_time_accuracy",
            "ui.birth_data.birth_city_or_settlement",
            "ui.birth_data.birth_city_or_settlement_placeholder",
            "ui.birth_data.latitude",
            "ui.birth_data.longitude",
            "ui.birth_data.timezone",
            "ui.birth_data.unknown_time_status",
            "ui.birth_data.validate",
            "ui.chart.positions.title",
            "ui.chart.positions.header.planet",
            "ui.chart.positions.header.sign",
            "ui.chart.positions.header.position",
            "ui.chart.positions.header.retrograde",
            "ui.chart.positions.retrograde_marker",
            "ui.chart.angles.title",
            "ui.chart.angles.ascendant",
            "ui.chart.angles.midheaven",
            "ui.chart.angles.unavailable.unknown_time",
            "ui.chart.angles.unavailable.calculation",
            "ui.planet.sun",
            "ui.planet.moon",
            "ui.planet.mercury",
            "ui.planet.venus",
            "ui.planet.mars",
            "ui.planet.jupiter",
            "ui.planet.saturn",
            "ui.planet.uranus",
            "ui.planet.neptune",
            "ui.planet.pluto",
            "ui.zodiac.aries",
            "ui.zodiac.taurus",
            "ui.zodiac.gemini",
            "ui.zodiac.cancer",
            "ui.zodiac.leo",
            "ui.zodiac.virgo",
            "ui.zodiac.libra",
            "ui.zodiac.scorpio",
            "ui.zodiac.sagittarius",
            "ui.zodiac.capricorn",
            "ui.zodiac.aquarius",
            "ui.zodiac.pisces",
            "ui.settings.title",
            "ui.settings.application_language",
            "ui.settings.interpretation_language",
            "ui.settings.theme"
        };

        foreach (var language in new[] { "ru", "en" })
        {
            var catalog = LoadRealUiCatalog(language);

            foreach (var key in requiredKeys)
            {
                Assert.False(string.IsNullOrWhiteSpace(GetRequiredText(catalog, key)));
            }
        }
    }

    [Fact]
    public void RealUiCatalogs_ContainEveryVisibleTarotWorkspaceKey()
    {
        var requiredKeys = new[]
        {
            "ui.tarot.control.spread", "ui.tarot.control.artwork", "ui.tarot.control.interpretation-pack",
            "ui.tarot.control.back", "ui.tarot.control.allow-reversed",
            "ui.tarot.control.auto-reveal",
            "ui.tarot.control.draw", "ui.tarot.control.redraw", "ui.tarot.tableau.title",
            "ui.tarot.empty-state", "ui.tarot.failure.insufficient-deck", "ui.tarot.artwork.unavailable",
            "ui.tarot.spread.single-card", "ui.tarot.spread.two-cards", "ui.tarot.spread.three-cards",
            "ui.tarot.interpretation.pair.interaction", "ui.tarot.interpretation.pair.direction",
            "ui.tarot.position.card",
            "ui.tarot.position.past", "ui.tarot.position.present", "ui.tarot.position.future",
            "ui.tarot.orientation.upright", "ui.tarot.orientation.reversed", "ui.tarot.arcana.major",
            "ui.tarot.arcana.minor", "ui.tarot.back.black-sun", "ui.tarot.back.lunar-seal",
            "ui.tarot.artwork.lupus-noctis",
            "ui.tarot.skin.astral-archive-prototype", "ui.tarot.inspector.title", "ui.tarot.inspector.card",
            "ui.tarot.inspector.position", "ui.tarot.inspector.orientation", "ui.tarot.inspector.arcana",
            "ui.tarot.inspector.suit", "ui.tarot.inspector.rank"
        };

        foreach (var language in new[] { "ru", "en" })
        {
            var catalog = LoadRealUiCatalog(language);
            Assert.All(requiredKeys, key => Assert.False(string.IsNullOrWhiteSpace(GetRequiredText(catalog, key))));
        }
    }

    [Fact]
    public void RealUiCatalogs_PreserveLupusNoctisProperNameAndLocalizeUnavailableDiagnostic()
    {
        var english = LoadRealUiCatalog("en");
        var russian = LoadRealUiCatalog("ru");

        Assert.Equal("Lupus Noctis", GetRequiredText(english, "ui.tarot.artwork.lupus-noctis"));
        Assert.Equal("Lupus Noctis", GetRequiredText(russian, "ui.tarot.artwork.lupus-noctis"));
        Assert.False(string.IsNullOrWhiteSpace(GetRequiredText(english, "ui.tarot.artwork.unavailable")));
        Assert.False(string.IsNullOrWhiteSpace(GetRequiredText(russian, "ui.tarot.artwork.unavailable")));
    }

    [Fact]
    public void RealUiCatalogs_ContainAutoRevealWithExactRussianAndEnglishCopy()
    {
        var english = LoadRealUiCatalog("en");
        var russian = LoadRealUiCatalog("ru");

        Assert.Equal(
            "Reveal cards automatically",
            GetRequiredText(english, "ui.tarot.control.auto-reveal"));
        Assert.Equal(
            "Открывать карты автоматически",
            GetRequiredText(russian, "ui.tarot.control.auto-reveal"));
    }

    [Fact]
    public void RealUiCatalogs_UseSpreadNeutralTarotNoReadingGuidance()
    {
        var english = LoadRealUiCatalog("en");
        var russian = LoadRealUiCatalog("ru");

        Assert.Equal("Draw the cards to begin", GetRequiredText(english, "ui.tarot.empty-state"));
        Assert.Equal("Вытяните карты, чтобы начать", GetRequiredText(russian, "ui.tarot.empty-state"));
    }

    [Fact]
    public void RealUiCatalogs_ContainTwoCardSpreadAndPairHeadingsWithExactCopy()
    {
        var english = LoadRealUiCatalog("en");
        var russian = LoadRealUiCatalog("ru");

        Assert.Equal("Two cards", GetRequiredText(english, "ui.tarot.spread.two-cards"));
        Assert.Equal("Interaction", GetRequiredText(english, "ui.tarot.interpretation.pair.interaction"));
        Assert.Equal("Direction", GetRequiredText(english, "ui.tarot.interpretation.pair.direction"));
        Assert.Equal("Две карты", GetRequiredText(russian, "ui.tarot.spread.two-cards"));
        Assert.Equal("Взаимодействие", GetRequiredText(russian, "ui.tarot.interpretation.pair.interaction"));
        Assert.Equal("Направление", GetRequiredText(russian, "ui.tarot.interpretation.pair.direction"));
    }

    [Fact]
    public void RussianAndEnglishUiCatalogs_HaveIdenticalKeySets()
    {
        var english = LoadRealUiCatalog("en");
        var russian = LoadRealUiCatalog("ru");

        Assert.Equal(
            russian.Entries.Select(entry => entry.Key.Value).Order(StringComparer.Ordinal),
            english.Entries.Select(entry => entry.Key.Value).Order(StringComparer.Ordinal));
    }

    [Fact]
    public void EveryStandardTarotCard_HasLocalizedDisplayNameInRussianAndEnglish()
    {
        foreach (var language in new[] { "ru", "en" })
        {
            var languageCode = new LanguageCode(language);
            var provider = new FallbackLocalizationProvider([LoadRealUiCatalog(language)]);
            var displayNames = StandardTarotCatalog.Deck.Cards
                .Select(card => TarotCardTextResolver.GetCardName(card, provider, languageCode))
                .ToArray();

            Assert.Equal(78, displayNames.Length);
            Assert.All(displayNames, name =>
            {
                Assert.False(string.IsNullOrWhiteSpace(name));
                Assert.DoesNotContain("ui.tarot", name, StringComparison.Ordinal);
            });
            Assert.Equal(78, displayNames.Distinct(StringComparer.Ordinal).Count());
        }
    }

    private static LocalizationCatalog LoadRealUiCatalog(string languageCode) =>
        JsonLocalizationCatalogLoader.LoadFromFile(
            LocalizationScope.Ui,
            new LanguageCode(languageCode),
            Path.GetFullPath(Path.Combine(
                AppContext.BaseDirectory,
                "..", "..", "..", "..",
                "resources", "localization", "ui", $"{languageCode}.json")));

    private static string GetRequiredText(LocalizationCatalog catalog, string key)
    {
        Assert.True(catalog.TryGetText(new LocalizationKey(key), out var text));
        return text!;
    }
}
