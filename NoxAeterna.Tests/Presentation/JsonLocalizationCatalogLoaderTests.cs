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
}
