using Avalonia;
using NoxAeterna.Geometry.Charts;

namespace NoxAeterna.Rendering.Charts;

/// <summary>
/// Represents a centered, clipped, render-safe chart viewport.
/// </summary>
public readonly record struct ChartViewport
{
    private ChartViewport(
        Rect controlBounds,
        Rect chartBounds,
        Rect safeDrawingBounds,
        double effectiveRadius,
        ChartVisualMetrics visualMetrics)
    {
        ControlBounds = controlBounds;
        ChartBounds = chartBounds;
        SafeDrawingBounds = safeDrawingBounds;
        Center = chartBounds.Center;
        EffectiveRadius = effectiveRadius;
        VisualMetrics = visualMetrics;
    }

    /// <summary>
    /// Gets the complete control bounds supplied to rendering.
    /// </summary>
    public Rect ControlBounds { get; }

    /// <summary>
    /// Gets the centered square bounds used as the final clip.
    /// </summary>
    public Rect ChartBounds { get; }

    /// <summary>
    /// Gets the square bounds after border and safety insets.
    /// </summary>
    public Rect SafeDrawingBounds { get; }

    /// <summary>
    /// Gets the chart center.
    /// </summary>
    public Point Center { get; }

    /// <summary>
    /// Gets the effective radius that keeps strokes and known glyph bounds inside the safe area.
    /// </summary>
    public double EffectiveRadius { get; }

    /// <summary>
    /// Gets the radius-responsive visual metrics reserved by this viewport.
    /// </summary>
    public ChartVisualMetrics VisualMetrics { get; }

    /// <summary>
    /// Attempts to create a safe viewport for the available control bounds.
    /// </summary>
    public static bool TryCreate(
        Rect controlBounds,
        ChartRadialLanes radialLanes,
        ChartRenderOptions options,
        out ChartViewport viewport)
    {
        ArgumentNullException.ThrowIfNull(radialLanes);
        ArgumentNullException.ThrowIfNull(options);

        viewport = default;

        if (!IsFinite(controlBounds) || controlBounds.Width <= 0d || controlBounds.Height <= 0d)
        {
            return false;
        }

        var squareSize = Math.Min(controlBounds.Width, controlBounds.Height);
        var squareBounds = new Rect(
            controlBounds.X + ((controlBounds.Width - squareSize) / 2d),
            controlBounds.Y + ((controlBounds.Height - squareSize) / 2d),
            squareSize,
            squareSize);
        var chartBounds = squareBounds.Deflate(options.ControlBorderInset);
        var safeBounds = chartBounds.Deflate(options.SafetyMargin);

        if (safeBounds.Width <= 0d || safeBounds.Height <= 0d)
        {
            return false;
        }

        var availableHalfSize = safeBounds.Width / 2d;
        var effectiveRadius = availableHalfSize / radialLanes.OuterBoundaryRadiusRatio;
        var visualMetrics = ChartVisualMetrics.Calculate(effectiveRadius, options);

        for (var iteration = 0; iteration < 4; iteration++)
        {
            var outerBoundaryRadius =
                (availableHalfSize - (visualMetrics.OuterRingStrokeThickness / 2d)) /
                radialLanes.OuterBoundaryRadiusRatio;
            var zodiacGlyphRadius =
                (availableHalfSize - (visualMetrics.ZodiacGlyphSize / 2d) - (visualMetrics.GlyphStrokeThickness / 2d)) /
                radialLanes.ZodiacGlyphLane.MidpointRadiusRatio;
            var planetGlyphRadius =
                (availableHalfSize -
                 (visualMetrics.PlanetGlyphSize / 2d) -
                 visualMetrics.PlanetAnnotationFontSize -
                 (visualMetrics.GlyphStrokeThickness / 2d)) /
                radialLanes.PlanetGlyphLane.OuterRadiusRatio;
            var angleLabelMargin = Math.Clamp(effectiveRadius * 0.01d, 3d, 5d);
            var angleLabelRadius =
                (availableHalfSize -
                 (visualMetrics.AngleLabelFontSize * 1.75d) -
                 angleLabelMargin) /
                radialLanes.OuterBoundaryRadiusRatio;
            var safeRadius = Math.Min(
                Math.Min(outerBoundaryRadius, zodiacGlyphRadius),
                Math.Min(planetGlyphRadius, angleLabelRadius));

            if (safeRadius >= effectiveRadius)
            {
                break;
            }

            effectiveRadius = safeRadius;
            visualMetrics = ChartVisualMetrics.Calculate(effectiveRadius, options);
        }

        if (!double.IsFinite(effectiveRadius) || effectiveRadius < options.MinimumEffectiveRadius)
        {
            return false;
        }

        viewport = new ChartViewport(controlBounds, chartBounds, safeBounds, effectiveRadius, visualMetrics);
        return true;
    }

    private static bool IsFinite(Rect bounds) =>
        double.IsFinite(bounds.X) &&
        double.IsFinite(bounds.Y) &&
        double.IsFinite(bounds.Width) &&
        double.IsFinite(bounds.Height);
}
