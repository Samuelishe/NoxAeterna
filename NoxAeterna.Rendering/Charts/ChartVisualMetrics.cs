namespace NoxAeterna.Rendering.Charts;

/// <summary>
/// Represents render-owned visual measurements derived from the effective chart radius.
/// </summary>
public readonly record struct ChartVisualMetrics(
    double ZodiacGlyphSize,
    double PlanetGlyphSize,
    double GlyphStrokeThickness,
    double OuterRingStrokeThickness,
    double StructuralStrokeThickness,
    double SectorSeparatorStrokeThickness,
    double AnchorStrokeThickness,
    double ConnectorStrokeThickness,
    double LabelLeaderStrokeThickness,
    double PlanetSourceMarkerRadius,
    double AspectScale,
    double AspectCircleStrokeThickness,
    double AspectEndpointRadius,
    double HouseNumberFontSize,
    double HouseNumberOpacity,
    double AngleLabelFontSize,
    double PlanetAnnotationFontSize,
    double HouseCuspStrokeThickness,
    double HouseCuspMarkerStrokeThickness,
    double AngleAxisStrokeThickness)
{
    /// <summary>
    /// Calculates bounded, deterministic visual measurements for the supplied radius.
    /// </summary>
    public static ChartVisualMetrics Calculate(double effectiveRadius, ChartRenderOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var radius = double.IsFinite(effectiveRadius)
            ? Math.Max(0d, effectiveRadius)
            : 0d;

        return new ChartVisualMetrics(
            Math.Max(options.ZodiacGlyphSize, Math.Clamp(radius * 0.12d, 26d, 46d)),
            Math.Max(options.PlanetGlyphSize, Math.Clamp(radius * 0.09d, 20d, 36d)),
            Math.Max(options.GlyphStrokeThickness, Math.Clamp(radius * 0.0062d, 1.6d, 2.8d)),
            Math.Max(options.OuterCircleStrokeThickness, Math.Clamp(radius * 0.007d, 2.4d, 3.4d)),
            Math.Max(options.SectorLineThickness, Math.Clamp(radius * 0.0045d, 1.5d, 2.2d)),
            Math.Clamp(radius * 0.0038d, 1.2d, 1.8d),
            Math.Clamp(radius * 0.0038d, 1.2d, 1.8d),
            Math.Clamp(radius * 0.0038d, 1.2d, 1.7d),
            Math.Clamp(radius * 0.0025d, 0.8d, 1.15d),
            Math.Clamp(radius * 0.0085d, 2.5d, 3.8d),
            Math.Clamp(radius / 230d, 1d, 1.5d),
            Math.Clamp(radius * 0.0035d, 1.1d, 1.6d),
            Math.Clamp(radius * 0.008d, 2.1d, 3.5d),
            Math.Clamp(radius * 0.044d, 12d, 18d),
            0.70d,
            Math.Clamp(radius * 0.05d, 13d, 20d),
            Math.Clamp(radius * 0.039d, 11d, 16d),
            Math.Clamp(radius * 0.0043d, 1.2d, 1.8d),
            Math.Clamp(radius * 0.006d, 1.8d, 2.5d),
            Math.Clamp(radius * 0.007d, 2.2d, 3.2d));
    }
}
