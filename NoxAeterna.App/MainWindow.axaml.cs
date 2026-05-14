using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using NoxAeterna.App.Debug;
using NoxAeterna.Presentation.Localization;
using NoxAeterna.Presentation.Preferences;
using NoxAeterna.Presentation.Settings;
using NoxAeterna.Presentation.Shell;
using NoxAeterna.Presentation.Theming;

namespace NoxAeterna.App;

public partial class MainWindow : Window
{
    private readonly ILocalizationProvider _localizationProvider;
    private UserPreferences _userPreferences;
    private readonly ShellViewModel _shellViewModel;
    private readonly SettingsViewModel _settingsViewModel;

    public MainWindow()
    {
        InitializeComponent();

        _userPreferences = new UserPreferences(
            new ApplicationLanguagePreference(new LanguageCode("ru")),
            new InterpretationLanguagePreference(new LanguageCode("ru")),
            new ThemeId("dark"));
        _localizationProvider = DebugShellLocalizationFactory.Create();
        _shellViewModel = ShellViewModel.CreateDefault();
        _settingsViewModel = SettingsViewModel.CreateDefault(_userPreferences);

        RefreshShell();
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    private void OnNavigationSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (NavigationListBox.SelectedItem is not LocalizedShellNavigationItem selectedItem)
        {
            return;
        }

        _shellViewModel.SelectedSectionId = selectedItem.Item.Id;
        UpdateShellSection();
    }

    private void UpdateShellSection()
    {
        var currentItem = _shellViewModel.NavigationItems.First(item => item.Id == _shellViewModel.SelectedSectionId);

        SectionTitleTextBlock.Text = Localize(currentItem.LabelKey);

        if (currentItem.Id == ShellSectionId.DebugPreview)
        {
            SectionHintTextBlock.Text = $"{Localize("ui.shell.preview_caption")} • {Localize("ui.shell.preview_hint")}";
            SectionContentHost.Content = new DebugChartPreviewControl();
            return;
        }

        if (currentItem.Id == ShellSectionId.Settings)
        {
            SectionHintTextBlock.Text = Localize("ui.settings.hint");
            SectionContentHost.Content = new DebugSettingsControl(
                _settingsViewModel,
                _localizationProvider,
                _userPreferences.ApplicationLanguage.Language,
                ApplyUserPreferences);
            return;
        }

        SectionHintTextBlock.Text = $"{Localize("ui.shell.placeholder.caption")} • {Localize("ui.shell.placeholder.hint")}";
        SectionContentHost.Content = new TextBlock
        {
            Text = Localize("ui.shell.placeholder.caption"),
            FontSize = 18
        };
    }

    private void ApplyUserPreferences(UserPreferences updatedPreferences)
    {
        _userPreferences = updatedPreferences;
        RefreshShell();
    }

    private void RefreshShell()
    {
        Title = Localize(_shellViewModel.WindowTitleKey);
        NavigationTitleTextBlock.Text = Localize("ui.shell.navigation_title");

        var navigationItems = _shellViewModel.NavigationItems
            .Select(item => new LocalizedShellNavigationItem(item, Localize(item.LabelKey)))
            .ToArray();

        NavigationListBox.ItemsSource = navigationItems;
        NavigationListBox.SelectedItem = navigationItems.First(item => item.Item.Id == _shellViewModel.SelectedSectionId);

        UpdateShellSection();
    }

    private string Localize(string key) => Localize(new LocalizationKey(key));

    private string Localize(LocalizationKey key) =>
        _localizationProvider.Get(LocalizationScope.Ui, _userPreferences.ApplicationLanguage.Language, key).Text;

    private sealed record LocalizedShellNavigationItem(ShellNavigationItem Item, string Label)
    {
        public override string ToString() => Item.IsTemporary ? $"{Label} [temp]" : Label;
    }
}
