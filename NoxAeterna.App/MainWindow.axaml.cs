using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using NoxAeterna.Astronomy.Calculation;
using NoxAeterna.Astronomy.Time;
using NoxAeterna.App.Astrology;
using NoxAeterna.App.Debug;
using NoxAeterna.App.Localization;
using NoxAeterna.App.Preferences;
using NoxAeterna.App.Shell;
using NoxAeterna.App.Tarot;
using NoxAeterna.Infrastructure.Ephemeris;
using NoxAeterna.Infrastructure.Tarot;
using NoxAeterna.Domain.Tarot;
using NoxAeterna.Presentation.Astrology;
using NoxAeterna.Presentation.Localization;
using NoxAeterna.Presentation.Preferences;
using NoxAeterna.Presentation.Settings;
using NoxAeterna.Presentation.Shell;
using NoxAeterna.Presentation.Theming;
using NoxAeterna.Presentation.Tarot;
using NodaTime;
using ShapePath = Avalonia.Controls.Shapes.Path;

namespace NoxAeterna.App;

public partial class MainWindow : Window
{
    private ILocalizationProvider _localizationProvider;
    private UserPreferences _userPreferences;
    private readonly UserPreferencesCoordinator _preferencesCoordinator;
    private readonly ShellViewModel _shellViewModel;
    private readonly AstrologyWorkspaceViewModel _astrologyWorkspaceViewModel;
    private readonly TarotWorkspaceViewModel _tarotWorkspaceViewModel;
    private readonly TarotArtworkPackCatalog _tarotArtworkPackCatalog;
    private readonly SettingsViewModel _settingsViewModel;
    private readonly DevelopmentAstrologyChartCoordinator _astrologyChartCoordinator;
    private readonly SplitView _shellSplitView;
    private readonly TextBlock _navigationTitleTextBlock;
    private readonly Button _navigationToggleButton;
    private readonly ShapePath _navigationToggleIcon;
    private readonly ListBox _navigationListBox;
    private readonly TextBlock _sectionTitleTextBlock;
    private readonly TextBlock _sectionHintTextBlock;
    private readonly ContentControl _sectionContentHost;
    private ShellNavigationItemView[] _navigationItemViews = [];

    public MainWindow()
        : this(CreatePreferencesCoordinator())
    {
    }

    public MainWindow(UserPreferencesCoordinator preferencesCoordinator)
    {
        _preferencesCoordinator = preferencesCoordinator ?? throw new ArgumentNullException(nameof(preferencesCoordinator));
        InitializeComponent();

        _shellSplitView = this.FindControl<SplitView>("ShellSplitView")
            ?? throw new InvalidOperationException("ShellSplitView was not found.");
        _navigationTitleTextBlock = this.FindControl<TextBlock>("NavigationTitleTextBlock")
            ?? throw new InvalidOperationException("NavigationTitleTextBlock was not found.");
        _navigationToggleButton = this.FindControl<Button>("NavigationToggleButton")
            ?? throw new InvalidOperationException("NavigationToggleButton was not found.");
        _navigationToggleIcon = this.FindControl<ShapePath>("NavigationToggleIcon")
            ?? throw new InvalidOperationException("NavigationToggleIcon was not found.");
        _navigationListBox = this.FindControl<ListBox>("NavigationListBox")
            ?? throw new InvalidOperationException("NavigationListBox was not found.");
        _sectionTitleTextBlock = this.FindControl<TextBlock>("SectionTitleTextBlock")
            ?? throw new InvalidOperationException("SectionTitleTextBlock was not found.");
        _sectionHintTextBlock = this.FindControl<TextBlock>("SectionHintTextBlock")
            ?? throw new InvalidOperationException("SectionHintTextBlock was not found.");
        _sectionContentHost = this.FindControl<ContentControl>("SectionContentHost")
            ?? throw new InvalidOperationException("SectionContentHost was not found.");

        _userPreferences = _preferencesCoordinator.Current;
        ApplicationCultureController.Apply(_userPreferences.ApplicationLanguage.Language);
        _localizationProvider = DebugShellLocalizationProviderFactory.Create(_userPreferences.ApplicationLanguage.Language);
        _shellViewModel = ShellViewModel.CreateDefault();
        _shellViewModel.NavigationState.UpdateViewportWidth(
            double.IsFinite(Width) && Width > 0d
                ? Width
                : ShellNavigationLayout.CompactViewportThreshold);
        _astrologyWorkspaceViewModel = AstrologyWorkspaceViewModel.CreateFoundation();
        _tarotArtworkPackCatalog = TarotArtworkPackCatalog.CreateBuiltIn();
        ITarotRandomSource tarotRandomSource = new SystemTarotRandomSource();
#if DEBUG
        tarotRandomSource = DebugTarotSmokeRandomSource.CreateFromEnvironment() ?? tarotRandomSource;
#endif
        _tarotWorkspaceViewModel = TarotWorkspaceViewModel.CreateClassic(
            new TarotDrawEngine(tarotRandomSource),
            _tarotArtworkPackCatalog.AvailableOptions,
            _userPreferences.Tarot);
        _tarotWorkspaceViewModel.PreferencesChanged += OnTarotPreferencesChanged;
        _settingsViewModel = SettingsViewModel.CreateDefault(_userPreferences);
        _astrologyChartCoordinator = new DevelopmentAstrologyChartCoordinator(
            new DevelopmentAstrologyChartPipeline(
                new TzdbBirthMomentResolver(),
                new SwissEphemerisCalculator(),
                new SwissEphemerisHouseCalculator()));

        _shellSplitView.OpenPaneLength = ShellNavigationLayout.ExpandedPaneLength;
        _shellSplitView.CompactPaneLength = ShellNavigationLayout.CompactPaneLength;
        SizeChanged += OnWindowSizeChanged;

        RefreshShell();
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    private void OnNavigationSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_navigationListBox.SelectedItem is not ShellNavigationItemView selectedItem)
        {
            return;
        }

        _shellViewModel.SelectedSectionId = selectedItem.Item.Id;
        UpdateShellSection();
    }

    private void OnNavigationToggleClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _shellViewModel.NavigationState.Toggle();
        ApplyNavigationVisualState();
    }

    private void OnWindowSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        _shellViewModel.NavigationState.UpdateViewportWidth(e.NewSize.Width);
        ApplyNavigationVisualState();
    }

    private void UpdateShellSection()
    {
        var currentItem = _shellViewModel.NavigationItems.First(item => item.Id == _shellViewModel.SelectedSectionId);

        _sectionTitleTextBlock.Text = Localize(currentItem.LabelKey);

        if (currentItem.Id == ShellSectionId.Astrology)
        {
            _sectionHintTextBlock.Text = string.Empty;
            _sectionHintTextBlock.IsVisible = false;
            _sectionContentHost.Content = new AstrologyWorkspaceControl(
                _astrologyWorkspaceViewModel,
                _localizationProvider,
                _userPreferences.ApplicationLanguage.Language,
                _astrologyChartCoordinator);
            return;
        }

        if (currentItem.Id == ShellSectionId.Settings)
        {
            _sectionHintTextBlock.IsVisible = true;
            _sectionHintTextBlock.Text = Localize("ui.settings.hint");
            _sectionContentHost.Content = new DebugSettingsControl(
                _settingsViewModel,
                _localizationProvider,
                _userPreferences.ApplicationLanguage.Language,
                ApplyUserPreferences);
            return;
        }

        if (currentItem.Id == ShellSectionId.Tarot)
        {
            _sectionHintTextBlock.Text = string.Empty;
            _sectionHintTextBlock.IsVisible = false;
            _sectionContentHost.Content = new TarotWorkspaceControl(
                _tarotWorkspaceViewModel,
                _tarotArtworkPackCatalog,
                _localizationProvider,
                _userPreferences.ApplicationLanguage.Language,
                SystemClock.Instance.GetCurrentInstant);
            return;
        }

        _sectionHintTextBlock.IsVisible = true;
        _sectionHintTextBlock.Text = $"{Localize("ui.shell.placeholder.caption")} • {Localize("ui.shell.placeholder.hint")}";
        _sectionContentHost.Content = new TextBlock
        {
            Text = Localize("ui.shell.placeholder.caption"),
            FontSize = 18
        };
    }

    private void ApplyUserPreferences(UserPreferences updatedPreferences)
    {
        if (!_preferencesCoordinator.ApplyApplicationPreferences(updatedPreferences))
        {
            return;
        }

        _userPreferences = _preferencesCoordinator.Current;
        _settingsViewModel.ReplaceCurrentPreferences(_userPreferences);
        ApplicationCultureController.Apply(_userPreferences.ApplicationLanguage.Language);
        _localizationProvider = DebugShellLocalizationProviderFactory.Create(_userPreferences.ApplicationLanguage.Language);
        if (Application.Current is App app)
        {
            app.ApplyTheme(_userPreferences.ThemeId);
        }

        RefreshShell();
    }

    private void OnTarotPreferencesChanged(object? sender, TarotWorkspacePreferences preferences)
    {
        if (!_preferencesCoordinator.ApplyTarotPreferences(preferences))
        {
            return;
        }

        _userPreferences = _preferencesCoordinator.Current;
        _settingsViewModel.ReplaceCurrentPreferences(_userPreferences);
    }

    private void RefreshShell()
    {
        Title = Localize(_shellViewModel.WindowTitleKey);
        _navigationTitleTextBlock.Text = Localize("ui.shell.navigation_title");

        _navigationItemViews = _shellViewModel.NavigationItems
            .Select(item => new ShellNavigationItemView(
                item,
                Localize(item.LabelKey),
                _shellViewModel.NavigationState.IsExpanded))
            .ToArray();

        _navigationListBox.ItemsSource = _navigationItemViews;
        _navigationListBox.SelectedItem = _navigationItemViews.First(
            item => item.Item.Id == _shellViewModel.SelectedSectionId);

        ApplyNavigationVisualState();
        UpdateShellSection();
    }

    private void ApplyNavigationVisualState()
    {
        var navigationState = _shellViewModel.NavigationState;
        var isExpanded = navigationState.IsExpanded;

        _shellSplitView.IsPaneOpen = isExpanded;
        _navigationTitleTextBlock.IsVisible = isExpanded;
        _navigationToggleButton.HorizontalAlignment = isExpanded
            ? Avalonia.Layout.HorizontalAlignment.Right
            : Avalonia.Layout.HorizontalAlignment.Center;
        _navigationToggleButton.IsEnabled = !navigationState.IsCompactViewport;
        _navigationListBox.Classes.Set("compact", !isExpanded);

        foreach (var item in _navigationItemViews)
        {
            item.IsLabelVisible = isExpanded;
        }

        var toggleIcon = isExpanded
            ? ShellNavigationIconId.Collapse
            : ShellNavigationIconId.Expand;
        var toggleLabel = Localize(isExpanded
            ? "ui.shell.navigation.collapse"
            : "ui.shell.navigation.expand");
        _navigationToggleIcon.Data = ShellNavigationIconCatalog.CreateGeometry(toggleIcon);
        ToolTip.SetTip(_navigationToggleButton, toggleLabel);
        AutomationProperties.SetName(_navigationToggleButton, toggleLabel);
    }

    private string Localize(string key) => Localize(new LocalizationKey(key));

    private string Localize(LocalizationKey key) =>
        _localizationProvider.Get(LocalizationScope.Ui, _userPreferences.ApplicationLanguage.Language, key).Text;

    private static UserPreferencesCoordinator CreatePreferencesCoordinator()
    {
        var store = new JsonUserPreferencesStore(UserPreferencesPathResolver.GetSettingsPath());
        return new UserPreferencesCoordinator(store, store.Load());
    }

}
