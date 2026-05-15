using NoxAeterna.Presentation.Localization;
using NoxAeterna.Presentation.Preferences;
using NoxAeterna.Presentation.Theming;

namespace NoxAeterna.Tests.Presentation;

public sealed class LocalizationContractsTests
{
    [Fact]
    public void FallbackLocalizationProvider_UsesSelectedLanguageWhenAvailable()
    {
        ILocalizationProvider provider = new FallbackLocalizationProvider(
        [
            new LocalizationCatalog(
                LocalizationScope.Ui,
                new LanguageCode("en"),
                [new LocalizationEntry(new LocalizationKey("ui.app.title"), "Nox Aeterna")]),
            new LocalizationCatalog(
                LocalizationScope.Ui,
                new LanguageCode("ru"),
                [new LocalizationEntry(new LocalizationKey("ui.app.title"), "Nox Aeterna")])
        ]);

        var result = provider.Get(LocalizationScope.Ui, new LanguageCode("en"), new LocalizationKey("ui.app.title"));

        Assert.Equal("Nox Aeterna", result.Text);
        Assert.Equal(new LanguageCode("en"), result.ResolvedLanguage);
        Assert.False(result.UsedFallback);
    }

    [Fact]
    public void FallbackLocalizationProvider_FallsBackToRussianCatalog()
    {
        ILocalizationProvider provider = new FallbackLocalizationProvider(
        [
            new LocalizationCatalog(
                LocalizationScope.Ui,
                new LanguageCode("ru"),
                [new LocalizationEntry(new LocalizationKey("ui.app.title"), "Nox Aeterna")])
        ]);

        var result = provider.Get(LocalizationScope.Ui, new LanguageCode("de"), new LocalizationKey("ui.app.title"));

        Assert.Equal("Nox Aeterna", result.Text);
        Assert.Equal(new LanguageCode("ru"), result.ResolvedLanguage);
        Assert.True(result.UsedFallback);
    }

    [Fact]
    public void FallbackLocalizationProvider_FallsBackToKeyWhenNoCatalogEntryExists()
    {
        ILocalizationProvider provider = new FallbackLocalizationProvider(Array.Empty<LocalizationCatalog>());

        var result = provider.Get(LocalizationScope.Interpretation, new LanguageCode("en"), new LocalizationKey("interpretation.aspect.square"));

        Assert.Equal("interpretation.aspect.square", result.Text);
        Assert.Null(result.ResolvedLanguage);
        Assert.True(result.UsedFallback);
    }

    [Fact]
    public void FallbackLocalizationProvider_UsesNeutralParentBeforeRussianFallback()
    {
        ILocalizationProvider provider = new FallbackLocalizationProvider(
        [
            new LocalizationCatalog(
                LocalizationScope.Ui,
                new LanguageCode("en"),
                [new LocalizationEntry(new LocalizationKey("ui.chart.title"), "Chart")]),
            new LocalizationCatalog(
                LocalizationScope.Ui,
                new LanguageCode("ru"),
                [new LocalizationEntry(new LocalizationKey("ui.chart.title"), "Карта")])
        ]);

        var result = provider.Get(LocalizationScope.Ui, new LanguageCode("en-us"), new LocalizationKey("ui.chart.title"));

        Assert.Equal("Chart", result.Text);
        Assert.Equal(new LanguageCode("en"), result.ResolvedLanguage);
        Assert.True(result.UsedFallback);
    }

    [Fact]
    public void UserPreferences_SeparateApplicationAndInterpretationLanguages()
    {
        var preferences = new UserPreferences(
            new ApplicationLanguagePreference(new LanguageCode("ru")),
            new InterpretationLanguagePreference(new LanguageCode("en")),
            new ThemeId("dark"));

        Assert.Equal(new LanguageCode("ru"), preferences.ApplicationLanguage.Language);
        Assert.Equal(new LanguageCode("en"), preferences.InterpretationLanguage.Language);
        Assert.Equal(new ThemeId("dark"), preferences.ThemeId);
    }

    [Fact]
    public void ThemeRegistry_ResolvesRegisteredThemes()
    {
        var registry = new ThemeRegistry(
        [
            new ThemeDefinition(new ThemeId("dark"), new LocalizationKey("theme.dark")),
            new ThemeDefinition(new ThemeId("light"), new LocalizationKey("theme.light"))
        ]);

        var found = registry.TryGet(new ThemeId("light"), out var theme);

        Assert.True(found);
        Assert.NotNull(theme);
        Assert.Equal(new LocalizationKey("theme.light"), theme!.DisplayNameKey);
    }
}
