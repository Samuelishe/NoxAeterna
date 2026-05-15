using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using NoxAeterna.Presentation.Astrology;
using NoxAeterna.Presentation.Localization;
using NoxAeterna.Rendering.Charts;

namespace NoxAeterna.App.Astrology;

/// <summary>
/// Hosts the first reusable astrology workspace foundation.
/// </summary>
public sealed class AstrologyWorkspaceControl : UserControl
{
    private readonly AstrologyWorkspaceViewModel _viewModel;
    private readonly ILocalizationProvider _localizationProvider;
    private readonly LanguageCode _applicationLanguage;
    private readonly DevelopmentAstrologyChartCoordinator _chartCoordinator;
    private AstrologyChartSurfaceControl? _chartSurfaceControl;

    /// <summary>
    /// Initializes a new instance of the <see cref="AstrologyWorkspaceControl"/> class.
    /// </summary>
    /// <param name="viewModel">The astrology workspace view model.</param>
    /// <param name="localizationProvider">The UI localization provider.</param>
    /// <param name="applicationLanguage">The current application language.</param>
    /// <param name="chartScene">The prepared chart render scene.</param>
    public AstrologyWorkspaceControl(
        AstrologyWorkspaceViewModel viewModel,
        ILocalizationProvider localizationProvider,
        LanguageCode applicationLanguage,
        DevelopmentAstrologyChartCoordinator chartCoordinator)
    {
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _localizationProvider = localizationProvider ?? throw new ArgumentNullException(nameof(localizationProvider));
        _applicationLanguage = applicationLanguage;
        _chartCoordinator = chartCoordinator ?? throw new ArgumentNullException(nameof(chartCoordinator));

        Content = BuildContent();
    }

    private Control BuildContent()
    {
        var chartPanel = _viewModel.Panels.First(panel => panel.Id == AstrologyWorkspacePanelId.Chart);
        var birthDataPanel = _viewModel.Panels.First(panel => panel.Id == AstrologyWorkspacePanelId.BirthData);
        var interpretationPanel = _viewModel.Panels.First(panel => panel.Id == AstrologyWorkspacePanelId.Interpretation);
        var sidePanelStack = new StackPanel
        {
            Spacing = 16,
            Children =
            {
                CreatePanelContainer(
                    birthDataPanel,
                    new BirthDataInputControl(
                        _viewModel.BirthDataInput,
                        _localizationProvider,
                        _applicationLanguage,
                        TryBuildChartFromInput)),
                CreatePlaceholderPanel(interpretationPanel)
            }
        };

        var sidePanelScrollViewer = new ScrollViewer
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
            Content = sidePanelStack
        };
        Grid.SetColumn(sidePanelScrollViewer, 1);

        return new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("2.2*,1.15*"),
            ColumnSpacing = 16,
            Children =
            {
                CreateChartPanel(chartPanel),
                sidePanelScrollViewer
            }
        };
    }

    private Control CreateChartPanel(AstrologyWorkspacePanel panel) =>
        CreatePanelContainer(
            panel,
            CreateChartPanelBody());

    private Control CreateChartPanelBody()
    {
        _chartSurfaceControl = new AstrologyChartSurfaceControl(_chartCoordinator.CurrentScene)
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            MinHeight = 460
        };

        return new StackPanel
        {
            Spacing = 10,
            Children =
            {
                new TextBlock
                {
                    Text = Localize("ui.astrology.demo_calculation_notice"),
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = ResolveBrush("WorkspacePanelSubtleForegroundBrush", new SolidColorBrush(Color.FromRgb(128, 128, 132)))
                },
                _chartSurfaceControl
            }
        };
    }

    private Control CreatePlaceholderPanel(AstrologyWorkspacePanel panel) =>
        CreatePanelContainer(
            panel,
            new TextBlock
            {
                Text = Localize(panel.DescriptionKey),
                TextWrapping = TextWrapping.Wrap,
                Foreground = ResolveBrush("WorkspacePanelSubtleForegroundBrush", new SolidColorBrush(Color.FromRgb(128, 128, 132)))
            });

    private Control CreatePanelContainer(AstrologyWorkspacePanel panel, Control content) =>
        CreatePanelContainerCore(panel, content);

    private Control CreatePanelContainerCore(AstrologyWorkspacePanel panel, Control content)
    {
        var bodyHost = new ContentControl
        {
            Content = content
        };
        Grid.SetRow(bodyHost, 1);

        return new Border
        {
            Background = ResolveBrush("WorkspacePanelBackgroundBrush", new SolidColorBrush(Color.FromRgb(20, 20, 24))),
            BorderBrush = ResolveBrush("WorkspacePanelBorderBrush", new SolidColorBrush(Color.FromRgb(56, 56, 62))),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(14),
            Child = new Grid
            {
                RowDefinitions = new RowDefinitions("Auto,*"),
                RowSpacing = 10,
                Children =
                {
                    new TextBlock
                    {
                        Text = Localize(panel.TitleKey),
                        FontSize = 15,
                        FontWeight = FontWeight.SemiBold
                    },
                    bodyHost
                }
            }
        };
    }

    private string Localize(LocalizationKey key) =>
        _localizationProvider.Get(LocalizationScope.Ui, _applicationLanguage, key).Text;

    private string Localize(string key) => Localize(new LocalizationKey(key));

    private bool TryBuildChartFromInput(BirthDataInputViewModel birthDataInput)
    {
        var rebuilt = _chartCoordinator.TryBuild(birthDataInput);
        if (rebuilt)
        {
            _chartSurfaceControl?.SetScene(_chartCoordinator.CurrentScene);
        }

        return rebuilt;
    }

    private IBrush ResolveBrush(string resourceKey, IBrush fallbackBrush) =>
        Application.Current is { } application &&
        application.TryGetResource(resourceKey, ActualThemeVariant, out var resource) &&
        resource is IBrush brush
            ? brush
            : fallbackBrush;
}
