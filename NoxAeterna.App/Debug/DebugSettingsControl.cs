using Avalonia.Controls;
using NoxAeterna.Presentation.Localization;
using NoxAeterna.Presentation.Preferences;
using NoxAeterna.Presentation.Settings;
using NoxAeterna.Presentation.Theming;

namespace NoxAeterna.App.Debug;

/// <summary>
/// Hosts the first temporary settings foundation using in-memory presentation state only.
/// This control is bootstrap infrastructure and does not implement persistence.
/// </summary>
public sealed class DebugSettingsControl : UserControl
{
    private readonly ILocalizationProvider _localizationProvider;
    private readonly LanguageCode _applicationLanguage;
    private readonly SettingsViewModel _viewModel;
    private readonly Action<UserPreferences> _preferencesChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="DebugSettingsControl"/> class.
    /// </summary>
    public DebugSettingsControl(
        SettingsViewModel viewModel,
        ILocalizationProvider localizationProvider,
        LanguageCode applicationLanguage,
        Action<UserPreferences> preferencesChanged)
    {
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _localizationProvider = localizationProvider ?? throw new ArgumentNullException(nameof(localizationProvider));
        _applicationLanguage = applicationLanguage;
        _preferencesChanged = preferencesChanged ?? throw new ArgumentNullException(nameof(preferencesChanged));

        Content = BuildContent();
    }

    private Control BuildContent()
    {
        var applicationLanguageComboBox = new ComboBox
        {
            ItemsSource = _viewModel.AvailableApplicationLanguages
                .Select(option => new LocalizedLanguageOption(option, Localize(option.LabelKey)))
                .ToArray(),
            SelectedItem = _viewModel.AvailableApplicationLanguages
                .Select(option => new LocalizedLanguageOption(option, Localize(option.LabelKey)))
                .First(option => option.Option.Code == _viewModel.CurrentPreferences.ApplicationLanguage.Language)
        };
        applicationLanguageComboBox.SelectionChanged += (_, _) =>
        {
            if (applicationLanguageComboBox.SelectedItem is not LocalizedLanguageOption selected)
            {
                return;
            }

            _viewModel.SetApplicationLanguage(selected.Option.Code);
            _preferencesChanged(_viewModel.CurrentPreferences);
        };

        var interpretationLanguageComboBox = new ComboBox
        {
            ItemsSource = _viewModel.AvailableInterpretationLanguages
                .Select(option => new LocalizedLanguageOption(option, Localize(option.LabelKey)))
                .ToArray(),
            SelectedItem = _viewModel.AvailableInterpretationLanguages
                .Select(option => new LocalizedLanguageOption(option, Localize(option.LabelKey)))
                .First(option => option.Option.Code == _viewModel.CurrentPreferences.InterpretationLanguage.Language)
        };
        interpretationLanguageComboBox.SelectionChanged += (_, _) =>
        {
            if (interpretationLanguageComboBox.SelectedItem is not LocalizedLanguageOption selected)
            {
                return;
            }

            _viewModel.SetInterpretationLanguage(selected.Option.Code);
            _preferencesChanged(_viewModel.CurrentPreferences);
        };

        var themeComboBox = new ComboBox
        {
            ItemsSource = _viewModel.AvailableThemes
                .Select(option => new LocalizedThemeOption(option, Localize(option.LabelKey)))
                .ToArray(),
            SelectedItem = _viewModel.AvailableThemes
                .Select(option => new LocalizedThemeOption(option, Localize(option.LabelKey)))
                .First(option => option.Option.ThemeId == _viewModel.CurrentPreferences.ThemeId)
        };
        themeComboBox.SelectionChanged += (_, _) =>
        {
            if (themeComboBox.SelectedItem is not LocalizedThemeOption selected)
            {
                return;
            }

            _viewModel.SetTheme(selected.Option.ThemeId);
            _preferencesChanged(_viewModel.CurrentPreferences);
        };

        return new StackPanel
        {
            Spacing = 16,
            Children =
            {
                CreateSettingRow(Localize(_viewModel.ApplicationLanguageLabelKey), applicationLanguageComboBox),
                CreateSettingRow(Localize(_viewModel.InterpretationLanguageLabelKey), interpretationLanguageComboBox),
                CreateSettingRow(Localize(_viewModel.ThemeLabelKey), themeComboBox)
            }
        };
    }

    private static Control CreateSettingRow(string labelText, Control editor) =>
        new StackPanel
        {
            Spacing = 6,
            Children =
            {
                new TextBlock
                {
                    Text = labelText,
                    FontSize = 14
                },
                editor
            }
        };

    private string Localize(LocalizationKey key) =>
        _localizationProvider.Get(LocalizationScope.Ui, _applicationLanguage, key).Text;

    private sealed record LocalizedLanguageOption(LanguageOption Option, string Label)
    {
        public override string ToString() => Label;
    }

    private sealed record LocalizedThemeOption(ThemeOption Option, string Label)
    {
        public override string ToString() => Label;
    }
}
