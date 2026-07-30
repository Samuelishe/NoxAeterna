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
    double AnchorStrokeThickness,
    double ConnectorStrokeThickness,
    double AspectScale)
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
            Math.Max(options.ZodiacGlyphSize, Math.Clamp(radius * 0.12d, 24d, 46d)),
            Math.Max(options.PlanetGlyphSize, Math.Clamp(radius * 0.09d, 18d, 36d)),
            Math.Max(options.GlyphStrokeThickness, Math.Clamp(radius * 0.006d, 1.45d, 2.6d)),
            Math.Max(options.OuterCircleStrokeThickness, Math.Clamp(radius * 0.0055d, 1.5d, 2.4d)),
            Math.Max(options.SectorLineThickness, Math.Clamp(radius * 0.0035d, 0.9d, 1.5d)),
            Math.Clamp(radius * 0.0032d, 0.9d, 1.4d),
            Math.Clamp(radius * 0.0024d, 0.65d, 1.05d),
            Math.Clamp(radius / 230d, 0.9d, 1.45d));
    }
}
