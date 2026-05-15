using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using NoxAeterna.Rendering.Charts;

namespace NoxAeterna.App.Astrology;

/// <summary>
/// Hosts the current chart rendering surface for the astrology workspace foundation.
/// </summary>
public sealed class AstrologyChartSurfaceControl : Control
{
    private ChartRenderScene _scene;
    private readonly CircularChartRenderer _renderer;
    private readonly ChartRenderOptions _renderOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="AstrologyChartSurfaceControl"/> class.
    /// </summary>
    /// <param name="scene">The prepared chart render scene.</param>
    public AstrologyChartSurfaceControl(ChartRenderScene scene)
    {
        _scene = scene ?? throw new ArgumentNullException(nameof(scene));
        _renderer = new CircularChartRenderer();
        _renderOptions = new ChartRenderOptions();
    }

    /// <summary>
    /// Replaces the current prepared render scene and redraws the surface.
    /// </summary>
    /// <param name="scene">The prepared render scene.</param>
    public void SetScene(ChartRenderScene scene)
    {
        _scene = scene ?? throw new ArgumentNullException(nameof(scene));
        InvalidateVisual();
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
