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
    private ContentControl? _chartStateHost;
    private ViewportFittedSquare? _chartSquareHost;
    private AstrologyChartSurfaceControl? _chartSurfaceControl;
    private PlanetPositionSummaryControl? _positionSummaryControl;
    private ChartAngleSummaryControl? _angleSummaryControl;

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
        var sidePanelStack = new StackPanel
        {
            Children =
            {
                CreatePanelContainer(
                    birthDataPanel,
                    new BirthDataInputControl(
                        _viewModel.BirthDataInput,
                        _localizationProvider,
                        _applicationLanguage,
                        TryBuildChartFromInput))
            }
        };

        var sidePanelScrollViewer = new ScrollViewer
        {
            Width = 428,
            MinWidth = 408,
            MaxWidth = 456,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
            Padding = new Thickness(0, 4, 18, 4),
            Content = sidePanelStack
        };
        Grid.SetColumn(sidePanelScrollViewer, 1);

        var chartPanelScrollViewer = new ScrollViewer
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
            Padding = new Thickness(0, 4, 18, 4),
            Content = CreateChartPanel(chartPanel)
        };

        return new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto"),
            ColumnSpacing = 20,
            Children =
            {
                chartPanelScrollViewer,
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
        _chartStateHost = new ContentControl
        {
            HorizontalContentAlignment = HorizontalAlignment.Stretch
        };
        RefreshChartStateContent();
        return _chartStateHost;
    }

    private Control CreateReadyChartContent(
        DevelopmentChartBuildResult buildResult,
        ChartRenderScene scene)
    {
        _chartSurfaceControl = new AstrologyChartSurfaceControl(scene)
        {
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        _positionSummaryControl = new PlanetPositionSummaryControl(
            _localizationProvider,
            _applicationLanguage,
            PlanetPositionSummaryBuilder.Build(buildResult.NatalChart));
        _angleSummaryControl = new ChartAngleSummaryControl(
            _localizationProvider,
            _applicationLanguage,
            ChartAngleSummaryBuilder.Build(buildResult.NatalChart));
        _chartSquareHost = new ViewportFittedSquare
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Child = _chartSurfaceControl
        };

        return new StackPanel
        {
            Spacing = 18,
            Children =
            {
                _chartSquareHost,
                _positionSummaryControl,
                _angleSummaryControl
            }
        };
    }

    private void RefreshChartStateContent()
    {
        if (_chartStateHost is null)
        {
            return;
        }

        if (_chartCoordinator.CurrentBuildResult is { } buildResult &&
            _chartCoordinator.CurrentScene is { } scene)
        {
            _chartStateHost.Content = CreateReadyChartContent(buildResult, scene);
            _chartSquareHost?.SetViewportHeightConstraint(Bounds.Height);
            return;
        }

        _chartSquareHost = null;
        _chartSurfaceControl = null;
        _positionSummaryControl = null;
        _angleSummaryControl = null;
        var emptyState = new TextBlock
        {
            Text = Localize("ui.chart.empty_state"),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 36)
        };
        emptyState.Classes.Add("subtle");
        _chartStateHost.Content = emptyState;
    }

    private Control CreatePanelContainer(AstrologyWorkspacePanel panel, Control content) =>
        CreatePanelContainerCore(panel, content);

    private Control CreatePanelContainerCore(AstrologyWorkspacePanel panel, Control content)
    {
        var bodyHost = new ContentControl
        {
            Content = content
        };
        Grid.SetRow(bodyHost, 1);

        var panelContainer = new Border
        {
            Padding = new Thickness(20, 18, 20, 20),
            Child = new Grid
            {
                RowDefinitions = new RowDefinitions("Auto,*"),
                RowSpacing = 14,
                Children =
                {
                    new TextBlock
                    {
                        Text = Localize(panel.TitleKey),
                        FontSize = 16,
                        FontWeight = FontWeight.SemiBold
                    },
                    bodyHost
                }
            }
        };
        panelContainer.Classes.Add("surface-card");
        return panelContainer;
    }

    private string Localize(LocalizationKey key) =>
        _localizationProvider.Get(LocalizationScope.Ui, _applicationLanguage, key).Text;

    private string Localize(string key) => Localize(new LocalizationKey(key));

    private bool TryBuildChartFromInput(BirthDataInputViewModel birthDataInput)
    {
        var rebuilt = _chartCoordinator.TryBuild(birthDataInput);
        if (rebuilt)
        {
            RefreshChartStateContent();
        }

        return rebuilt;
    }

    /// <inheritdoc />
    protected override Size MeasureOverride(Size availableSize)
    {
        _chartSquareHost?.SetViewportHeightConstraint(availableSize.Height);
        return base.MeasureOverride(availableSize);
    }

}
