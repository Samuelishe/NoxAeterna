using NoxAeterna.Presentation.Localization;
using NoxAeterna.Presentation.Preferences;
using NoxAeterna.Presentation.Settings;
using NoxAeterna.Presentation.Theming;

namespace NoxAeterna.Tests.Presentation;

public sealed class SettingsViewModelTests
{
    [Fact]
    public void CreateDefault_ExposesAvailableLanguages()
    {
        var viewModel = SettingsViewModel.CreateDefault(CreatePreferences());

        Assert.Equal(
            new[] { new LanguageCode("ru"), new LanguageCode("en") },
            viewModel.AvailableApplicationLanguages.Select(option => option.Code));
        Assert.Equal(
            new[] { new LanguageCode("ru"), new LanguageCode("en") },
            viewModel.AvailableInterpretationLanguages.Select(option => option.Code));
    }

    [Fact]
    public void CreateDefault_ExposesAvailableThemes()
    {
        var viewModel = SettingsViewModel.CreateDefault(CreatePreferences());

        Assert.Equal(
            new[] { new ThemeId("dark"), new ThemeId("light") },
            viewModel.AvailableThemes.Select(option => option.ThemeId));
    }

    [Fact]
    public void ApplicationAndInterpretationLanguages_AreSeparateSelections()
    {
        var viewModel = SettingsViewModel.CreateDefault(CreatePreferences());

        viewModel.SetApplicationLanguage(new LanguageCode("en"));

        Assert.Equal(new LanguageCode("en"), viewModel.CurrentPreferences.ApplicationLanguage.Language);
        Assert.Equal(new LanguageCode("ru"), viewModel.CurrentPreferences.InterpretationLanguage.Language);

        viewModel.SetInterpretationLanguage(new LanguageCode("en"));

        Assert.Equal(new LanguageCode("en"), viewModel.CurrentPreferences.InterpretationLanguage.Language);
    }

    [Fact]
    public void ThemeSelection_UsesThemeId()
    {
        var viewModel = SettingsViewModel.CreateDefault(CreatePreferences());

        viewModel.SetTheme(new ThemeId("light"));

        Assert.Equal(new ThemeId("light"), viewModel.CurrentPreferences.ThemeId);
    }

    [Fact]
    public void UserPreferencesUpdates_AreDeterministic()
    {
        var viewModel = SettingsViewModel.CreateDefault(CreatePreferences());

        viewModel.SetApplicationLanguage(new LanguageCode("en"));
        viewModel.SetInterpretationLanguage(new LanguageCode("ru"));
        viewModel.SetTheme(new ThemeId("light"));

        Assert.Equal(
            new UserPreferences(
                new ApplicationLanguagePreference(new LanguageCode("en")),
                new InterpretationLanguagePreference(new LanguageCode("ru")),
                new ThemeId("light")),
            viewModel.CurrentPreferences);
    }

    [Fact]
    public void SettingsLabels_AreLocalizationKeyBased()
    {
        var viewModel = SettingsViewModel.CreateDefault(CreatePreferences());

        Assert.Equal(new LocalizationKey("ui.settings.application_language"), viewModel.ApplicationLanguageLabelKey);
        Assert.Equal(new LocalizationKey("ui.settings.interpretation_language"), viewModel.InterpretationLanguageLabelKey);
        Assert.Equal(new LocalizationKey("ui.settings.theme"), viewModel.ThemeLabelKey);
        Assert.All(viewModel.AvailableApplicationLanguages, option => Assert.False(string.IsNullOrWhiteSpace(option.LabelKey.Value)));
        Assert.All(viewModel.AvailableThemes, option => Assert.False(string.IsNullOrWhiteSpace(option.LabelKey.Value)));
    }

    private static UserPreferences CreatePreferences() =>
        new(
            new ApplicationLanguagePreference(new LanguageCode("ru")),
            new InterpretationLanguagePreference(new LanguageCode("ru")),
            new ThemeId("dark"));
}
