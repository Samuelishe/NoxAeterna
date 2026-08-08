using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
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
    private readonly TarotInterpretationPackCatalog _tarotInterpretationPackCatalog;
    private readonly TarotWorkspaceInterpretationCoordinator _tarotInterpretationCoordinator;
    private readonly WindowPlacementCoordinator _windowPlacementCoordinator;
    private readonly WindowsSystemMenuCoordinator? _windowsSystemMenuCoordinator;
    private readonly SettingsViewModel _settingsViewModel;
    private readonly DevelopmentAstrologyChartCoordinator _astrologyChartCoordinator;
    private readonly SplitView _shellSplitView;
    private readonly Border _windowCaptionSpacer;
    private readonly TextBlock _navigationTitleTextBlock;
    private readonly Button _navigationToggleButton;
    private readonly ShapePath _navigationToggleIcon;
    private readonly ListBox _navigationListBox;
    private readonly StackPanel _sectionHeaderPanel;
    private readonly TextBlock _sectionTitleTextBlock;
    private readonly TextBlock _sectionHintTextBlock;
    private readonly ContentControl _sectionContentHost;
    private readonly bool _usesProjectChrome;
    private ShellNavigationItemView[] _navigationItemViews = [];

    public MainWindow()
        : this(CreateDefaultDependencies())
    {
    }

    public MainWindow(UserPreferencesCoordinator preferencesCoordinator)
        : this(preferencesCoordinator, TarotInterpretationComposition.CreateBuiltIn())
    {
    }

    public MainWindow(
        UserPreferencesCoordinator preferencesCoordinator,
        TarotInterpretationComposition interpretationComposition)
    {
        _preferencesCoordinator = preferencesCoordinator ?? throw new ArgumentNullException(nameof(preferencesCoordinator));
        ArgumentNullException.ThrowIfNull(interpretationComposition);
        InitializeComponent();

        _windowCaptionSpacer = this.FindControl<Border>("WindowCaptionSpacer")
            ?? throw new InvalidOperationException("WindowCaptionSpacer was not found.");
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
        _sectionHeaderPanel = this.FindControl<StackPanel>("SectionHeaderPanel")
            ?? throw new InvalidOperationException("SectionHeaderPanel was not found.");
        _sectionTitleTextBlock = this.FindControl<TextBlock>("SectionTitleTextBlock")
            ?? throw new InvalidOperationException("SectionTitleTextBlock was not found.");
        _sectionHintTextBlock = this.FindControl<TextBlock>("SectionHintTextBlock")
            ?? throw new InvalidOperationException("SectionHintTextBlock was not found.");
        _sectionContentHost = this.FindControl<ContentControl>("SectionContentHost")
            ?? throw new InvalidOperationException("SectionContentHost was not found.");

        _userPreferences = _preferencesCoordinator.Current;
        ApplicationCultureController.Apply(_userPreferences.ApplicationLanguage.Language);
        _localizationProvider = DebugShellLocalizationProviderFactory.Create(_userPreferences.ApplicationLanguage.Language);
        _usesProjectChrome = WindowChromePolicy.Apply(this);
        _windowCaptionSpacer.IsVisible = _usesProjectChrome;
        _windowsSystemMenuCoordinator = _usesProjectChrome
            ? new WindowsSystemMenuCoordinator(this)
            : null;
        _windowPlacementCoordinator = new WindowPlacementCoordinator(this, _preferencesCoordinator);
        _shellViewModel = ShellViewModel.CreateDefault();
        _shellViewModel.NavigationState.UpdateViewportWidth(
            double.IsFinite(Width) && Width > 0d
                ? Width
                : ShellNavigationLayout.CompactViewportThreshold);
        _astrologyWorkspaceViewModel = AstrologyWorkspaceViewModel.CreateFoundation();
        _tarotArtworkPackCatalog = TarotArtworkPackCatalog.CreateBuiltIn();
        _tarotInterpretationPackCatalog = interpretationComposition.PackCatalog;
        ITarotRandomSource tarotRandomSource = new SystemTarotRandomSource();
#if DEBUG
        tarotRandomSource = DebugTarotSmokeRandomSource.CreateFromEnvironment() ?? tarotRandomSource;
#endif
        _tarotWorkspaceViewModel = TarotWorkspaceViewModel.CreateClassic(
            new TarotDrawEngine(tarotRandomSource),
            _tarotArtworkPackCatalog.AvailableOptions,
            _tarotInterpretationPackCatalog.Options,
            _userPreferences.Tarot);
        _tarotInterpretationCoordinator = new TarotWorkspaceInterpretationCoordinator(
            interpretationComposition.Resolver,
            _tarotWorkspaceViewModel,
            _userPreferences.InterpretationLanguage,
            interpretationComposition.PresentationLabels);
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
        Opened += OnWindowOpened;
        PropertyChanged += OnMainWindowPropertyChanged;

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

        _sectionHeaderPanel.IsVisible = currentItem.ShowHeader;
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
                _tarotInterpretationPackCatalog,
                _tarotInterpretationCoordinator,
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
        _tarotInterpretationCoordinator.SetInterpretationLanguage(_userPreferences.InterpretationLanguage);
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
        RefreshCaptionLocalization();
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

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);
        if (!e.Cancel)
        {
            _windowPlacementCoordinator.PersistAtClose();
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        Opened -= OnWindowOpened;
        PropertyChanged -= OnMainWindowPropertyChanged;
        _windowPlacementCoordinator.Dispose();
        _windowsSystemMenuCoordinator?.Dispose();
        _tarotInterpretationCoordinator.Dispose();
        base.OnClosed(e);
    }

    private void OnWindowOpened(object? sender, EventArgs e) =>
        Dispatcher.UIThread.Post(UpdateCaptionAccessibility);

    private void OnMainWindowPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property != WindowStateProperty || !_usesProjectChrome)
        {
            return;
        }

        RefreshCaptionLocalization();
        Dispatcher.UIThread.Post(UpdateCaptionAccessibility);
    }

    private void RefreshCaptionLocalization()
    {
        Resources["WindowCaptionMinimizeText"] = Localize("ui.shell.window.minimize");
        Resources["WindowCaptionMaximizeText"] = Localize(
            WindowState == WindowState.Maximized
                ? "ui.shell.window.restore"
                : "ui.shell.window.maximize");
        Resources["WindowCaptionCloseText"] = Localize("ui.shell.window.close");
        if (_usesProjectChrome && IsVisible)
        {
            Dispatcher.UIThread.Post(UpdateCaptionAccessibility);
        }
    }

    private void UpdateCaptionAccessibility()
    {
        if (!_usesProjectChrome || VisualRoot is not Visual visualRoot)
        {
            return;
        }

        SetCaptionButtonText(
            visualRoot,
            "PART_MinimizeButton",
            Localize("ui.shell.window.minimize"));
        SetCaptionButtonText(
            visualRoot,
            "PART_MaximizeButton",
            Localize(WindowState == WindowState.Maximized
                ? "ui.shell.window.restore"
                : "ui.shell.window.maximize"));
        SetCaptionButtonText(
            visualRoot,
            "PART_CloseButton",
            Localize("ui.shell.window.close"));
    }

    private static void SetCaptionButtonText(Visual visualRoot, string name, string text)
    {
        var button = visualRoot
            .GetVisualDescendants()
            .OfType<Button>()
            .FirstOrDefault(candidate => candidate.Name == name);
        if (button is null)
        {
            return;
        }

        ToolTip.SetTip(button, text);
        AutomationProperties.SetName(button, text);
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

    private MainWindow(DefaultDependencies dependencies)
        : this(dependencies.PreferencesCoordinator, dependencies.InterpretationComposition)
    {
    }

    private static DefaultDependencies CreateDefaultDependencies()
    {
        var interpretation = TarotInterpretationComposition.CreateBuiltIn();
        var store = new JsonUserPreferencesStore(
            UserPreferencesPathResolver.GetSettingsPath(),
            interpretation.PackCatalog.AvailablePackIds);
        return new DefaultDependencies(
            new UserPreferencesCoordinator(store, store.Load()),
            interpretation);
    }

    private sealed record DefaultDependencies(
        UserPreferencesCoordinator PreferencesCoordinator,
        TarotInterpretationComposition InterpretationComposition);

}
