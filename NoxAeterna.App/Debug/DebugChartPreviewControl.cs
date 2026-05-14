using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using NoxAeterna.Geometry.Charts;
using NoxAeterna.Rendering.Charts;

namespace NoxAeterna.App.Debug;

/// <summary>
/// Hosts a temporary debug-only chart preview for visual verification of the current pipeline.
/// This control is not product UI.
/// </summary>
public sealed class DebugChartPreviewControl : Control
{
    private readonly ChartRenderScene _scene;
    private readonly CircularChartRenderer _renderer;
    private readonly ChartRenderOptions _renderOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="DebugChartPreviewControl"/> class.
    /// </summary>
    public DebugChartPreviewControl()
    {
        var sampleChart = DebugSampleNatalChartFactory.Create();
        var layout = new CircularChartLayoutBuilder().Build(sampleChart);

        _scene = ChartRenderScene.FromLayout(layout);
        _renderer = new CircularChartRenderer();
        _renderOptions = new ChartRenderOptions();
    }

    /// <inheritdoc />
    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var bounds = Bounds;
        var backgroundBrush = ResolveBrush("PreviewSurfaceBackgroundBrush", new SolidColorBrush(Color.FromRgb(18, 18, 20)));
        var borderBrush = ResolveBrush("PreviewSurfaceBorderBrush", new SolidColorBrush(Color.FromRgb(64, 64, 70)));

        context.FillRectangle(backgroundBrush, bounds);
        context.DrawRectangle(new Pen(borderBrush, 1d), bounds.Deflate(0.5d));

        _renderer.Render(context, bounds, _scene, _renderOptions);
    }

    private IBrush ResolveBrush(string resourceKey, IBrush fallbackBrush) =>
        Application.Current is { } application &&
        application.TryGetResource(resourceKey, ActualThemeVariant, out var resource) &&
        resource is IBrush brush
            ? brush
            : fallbackBrush;
}
