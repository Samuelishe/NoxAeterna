using NoxAeterna.Presentation.Localization;

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
    public void RealRussianUiCatalog_UsesLocalizedBirthInputLabelsWithoutKnownMixedEnglishTerms()
    {
        var ruCatalog = LoadRealUiCatalog("ru");

        Assert.Equal("Часовой пояс", GetRequiredText(ruCatalog, "ui.birth_data.timezone"));
        Assert.Equal("Prague, Czechia", GetRequiredText(ruCatalog, "ui.birth_data.birth_city_or_settlement_placeholder"));
        Assert.DoesNotContain("Timezone ID", GetRequiredText(ruCatalog, "ui.birth_data.timezone"));
        Assert.DoesNotContain("workspace", GetRequiredText(ruCatalog, "ui.astrology.workspace.hint"), StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("sample", GetRequiredText(ruCatalog, "ui.astrology.panel.chart.description"), StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("chart pipeline", GetRequiredText(ruCatalog, "ui.astrology.workspace.hint"), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RealUiCatalogs_ContainVisibleShellWorkspaceAndSettingsKeys()
    {
        var requiredKeys = new[]
        {
            "ui.shell.window_title",
            "ui.shell.navigation_title",
            "ui.shell.section.astrology",
            "ui.shell.section.settings",
            "ui.astrology.workspace.hint",
            "ui.astrology.demo_calculation_notice",
            "ui.astrology.panel.chart.title",
            "ui.astrology.panel.birth_data.title",
            "ui.birth_data.birth_date",
            "ui.birth_data.birth_date_helper",
            "ui.birth_data.birth_time",
            "ui.birth_data.birth_time_helper",
            "ui.birth_data.birth_time_accuracy",
            "ui.birth_data.birth_city_or_settlement",
            "ui.birth_data.birth_city_or_settlement_placeholder",
            "ui.birth_data.birth_city_or_settlement_helper",
            "ui.birth_data.latitude",
            "ui.birth_data.longitude",
            "ui.birth_data.timezone",
            "ui.birth_data.timezone_helper",
            "ui.birth_data.validate",
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
    public void RealUiCatalogs_KeepWorkspaceSubtitleSeparateFromDemoWarning()
    {
        foreach (var language in new[] { "ru", "en" })
        {
            var catalog = LoadRealUiCatalog(language);
            var subtitle = GetRequiredText(catalog, "ui.astrology.workspace.hint");
            var demoWarning = GetRequiredText(catalog, "ui.astrology.demo_calculation_notice");

            Assert.NotEqual(subtitle, demoWarning);
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
