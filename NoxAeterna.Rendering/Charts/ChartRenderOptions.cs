namespace NoxAeterna.Rendering.Charts;

/// <summary>
/// Represents numeric rendering options for a circular chart preview.
/// </summary>
public sealed record ChartRenderOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChartRenderOptions"/> class.
    /// </summary>
    /// <param name="palette">The chart-local palette.</param>
    /// <param name="controlBorderInset">The control-border inset in device-independent pixels.</param>
    /// <param name="safetyMargin">The final safety margin inside the clip.</param>
    /// <param name="minimumEffectiveRadius">The minimum useful effective radius.</param>
    /// <param name="outerCircleStrokeThickness">The outer circle stroke thickness.</param>
    /// <param name="sectorLineThickness">The zodiac sector separator thickness.</param>
    /// <param name="glyphStrokeThickness">The vector glyph stroke thickness.</param>
    /// <param name="zodiacGlyphSize">The zodiac vector glyph size.</param>
    /// <param name="planetGlyphSize">The planet vector glyph size.</param>
    public ChartRenderOptions(
        ChartRenderPalette? palette = null,
        double controlBorderInset = 1d,
        double safetyMargin = 3d,
        double minimumEffectiveRadius = 24d,
        double outerCircleStrokeThickness = 2d,
        double sectorLineThickness = 0.8d,
        double glyphStrokeThickness = 1.55d,
        double zodiacGlyphSize = 24d,
        double planetGlyphSize = 17d)
    {
        ValidateFiniteNonNegative(nameof(controlBorderInset), controlBorderInset);
        ValidateFiniteNonNegative(nameof(safetyMargin), safetyMargin);
        ValidateFinitePositive(nameof(outerCircleStrokeThickness), outerCircleStrokeThickness);
        ValidateFinitePositive(nameof(sectorLineThickness), sectorLineThickness);
        ValidateFinitePositive(nameof(minimumEffectiveRadius), minimumEffectiveRadius);
        ValidateFinitePositive(nameof(glyphStrokeThickness), glyphStrokeThickness);
        ValidateFinitePositive(nameof(zodiacGlyphSize), zodiacGlyphSize);
        ValidateFinitePositive(nameof(planetGlyphSize), planetGlyphSize);

        Palette = palette ?? ChartRenderPalette.Dark;
        ControlBorderInset = controlBorderInset;
        SafetyMargin = safetyMargin;
        MinimumEffectiveRadius = minimumEffectiveRadius;
        OuterCircleStrokeThickness = outerCircleStrokeThickness;
        SectorLineThickness = sectorLineThickness;
        GlyphStrokeThickness = glyphStrokeThickness;
        ZodiacGlyphSize = zodiacGlyphSize;
        PlanetGlyphSize = planetGlyphSize;
    }

    /// <summary>
    /// Gets the chart-local palette.
    /// </summary>
    public ChartRenderPalette Palette { get; }

    /// <summary>
    /// Gets the control-border inset.
    /// </summary>
    public double ControlBorderInset { get; }

    /// <summary>
    /// Gets the final safety margin.
    /// </summary>
    public double SafetyMargin { get; }

    /// <summary>
    /// Gets the minimum useful effective radius.
    /// </summary>
    public double MinimumEffectiveRadius { get; }

    /// <summary>
    /// Gets the outer circle stroke thickness.
    /// </summary>
    public double OuterCircleStrokeThickness { get; }

    /// <summary>
    /// Gets the zodiac sector separator thickness.
    /// </summary>
    public double SectorLineThickness { get; }

    /// <summary>
    /// Gets the vector glyph stroke thickness.
    /// </summary>
    public double GlyphStrokeThickness { get; }

    /// <summary>
    /// Gets the zodiac vector glyph size.
    /// </summary>
    public double ZodiacGlyphSize { get; }

    /// <summary>
    /// Gets the planet vector glyph size.
    /// </summary>
    public double PlanetGlyphSize { get; }

    private static void ValidateFinitePositive(string parameterName, double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value) || value <= 0d)
        {
            throw new ArgumentOutOfRangeException(parameterName, "Value must be a finite positive number.");
        }
    }

    private static void ValidateFiniteNonNegative(string parameterName, double value)
    {
        if (!double.IsFinite(value) || value < 0d)
        {
            throw new ArgumentOutOfRangeException(parameterName, "Value must be a finite non-negative number.");
        }
    }
}
