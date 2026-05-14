using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
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
    private ILocalizationProvider _localizationProvider;
    private UserPreferences _userPreferences;
    private readonly ShellViewModel _shellViewModel;
    private readonly SettingsViewModel _settingsViewModel;
    private readonly TextBlock _navigationTitleTextBlock;
    private readonly ListBox _navigationListBox;
    private readonly TextBlock _sectionTitleTextBlock;
    private readonly TextBlock _sectionHintTextBlock;
    private readonly ContentControl _sectionContentHost;

    public MainWindow()
    {
        InitializeComponent();

        _navigationTitleTextBlock = this.FindControl<TextBlock>("NavigationTitleTextBlock")
            ?? throw new InvalidOperationException("NavigationTitleTextBlock was not found.");
        _navigationListBox = this.FindControl<ListBox>("NavigationListBox")
            ?? throw new InvalidOperationException("NavigationListBox was not found.");
        _sectionTitleTextBlock = this.FindControl<TextBlock>("SectionTitleTextBlock")
            ?? throw new InvalidOperationException("SectionTitleTextBlock was not found.");
        _sectionHintTextBlock = this.FindControl<TextBlock>("SectionHintTextBlock")
            ?? throw new InvalidOperationException("SectionHintTextBlock was not found.");
        _sectionContentHost = this.FindControl<ContentControl>("SectionContentHost")
            ?? throw new InvalidOperationException("SectionContentHost was not found.");

        _userPreferences = new UserPreferences(
            new ApplicationLanguagePreference(new LanguageCode("ru")),
            new InterpretationLanguagePreference(new LanguageCode("ru")),
            new ThemeId("dark"));
        _localizationProvider = DebugShellLocalizationProviderFactory.Create(_userPreferences.ApplicationLanguage.Language);
        _shellViewModel = ShellViewModel.CreateDefault();
        _settingsViewModel = SettingsViewModel.CreateDefault(_userPreferences);

        RefreshShell();
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    private void OnNavigationSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_navigationListBox.SelectedItem is not LocalizedShellNavigationItem selectedItem)
        {
            return;
        }

        _shellViewModel.SelectedSectionId = selectedItem.Item.Id;
        UpdateShellSection();
    }

    private void UpdateShellSection()
    {
        var currentItem = _shellViewModel.NavigationItems.First(item => item.Id == _shellViewModel.SelectedSectionId);

        _sectionTitleTextBlock.Text = Localize(currentItem.LabelKey);

        if (currentItem.Id == ShellSectionId.DebugPreview)
        {
            _sectionHintTextBlock.Text = $"{Localize("ui.shell.preview_caption")} • {Localize("ui.shell.preview_hint")}";
            _sectionContentHost.Content = new DebugChartPreviewControl();
            return;
        }

        if (currentItem.Id == ShellSectionId.Settings)
        {
            _sectionHintTextBlock.Text = Localize("ui.settings.hint");
            _sectionContentHost.Content = new DebugSettingsControl(
                _settingsViewModel,
                _localizationProvider,
                _userPreferences.ApplicationLanguage.Language,
                ApplyUserPreferences);
            return;
        }

        _sectionHintTextBlock.Text = $"{Localize("ui.shell.placeholder.caption")} • {Localize("ui.shell.placeholder.hint")}";
        _sectionContentHost.Content = new TextBlock
        {
            Text = Localize("ui.shell.placeholder.caption"),
            FontSize = 18
        };
    }

    private void ApplyUserPreferences(UserPreferences updatedPreferences)
    {
        _userPreferences = updatedPreferences;
        _localizationProvider = DebugShellLocalizationProviderFactory.Create(_userPreferences.ApplicationLanguage.Language);
        if (Application.Current is App app)
        {
            app.ApplyTheme(_userPreferences.ThemeId);
        }

        RefreshShell();
    }

    private void RefreshShell()
    {
        Title = Localize(_shellViewModel.WindowTitleKey);
        _navigationTitleTextBlock.Text = Localize("ui.shell.navigation_title");

        var navigationItems = _shellViewModel.NavigationItems
            .Select(item => new LocalizedShellNavigationItem(item, Localize(item.LabelKey)))
            .ToArray();

        _navigationListBox.ItemsSource = navigationItems;
        _navigationListBox.SelectedItem = navigationItems.First(item => item.Item.Id == _shellViewModel.SelectedSectionId);

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
