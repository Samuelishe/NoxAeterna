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
    private readonly ChartRenderScene _chartScene;

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
        ChartRenderScene chartScene)
    {
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _localizationProvider = localizationProvider ?? throw new ArgumentNullException(nameof(localizationProvider));
        _applicationLanguage = applicationLanguage;
        _chartScene = chartScene ?? throw new ArgumentNullException(nameof(chartScene));

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
                        _applicationLanguage)),
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
            new AstrologyChartSurfaceControl(_chartScene)
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                MinHeight = 460
            });

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

    private IBrush ResolveBrush(string resourceKey, IBrush fallbackBrush) =>
        Application.Current is { } application &&
        application.TryGetResource(resourceKey, ActualThemeVariant, out var resource) &&
        resource is IBrush brush
            ? brush
            : fallbackBrush;
}
