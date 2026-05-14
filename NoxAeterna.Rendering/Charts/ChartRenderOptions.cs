namespace NoxAeterna.Rendering.Charts;

/// <summary>
/// Represents numeric rendering options for a circular chart preview.
/// </summary>
public sealed record ChartRenderOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChartRenderOptions"/> class.
    /// </summary>
    /// <param name="paddingRatio">The normalized padding ratio applied to the chart radius.</param>
    /// <param name="outerCircleStrokeThickness">The outer circle stroke thickness.</param>
    /// <param name="sectorLineThickness">The zodiac sector separator thickness.</param>
    /// <param name="aspectLineThickness">The aspect line thickness.</param>
    /// <param name="planetMarkerRadius">The planet marker radius in device-independent pixels.</param>
    public ChartRenderOptions(
        double paddingRatio = 0.06d,
        double outerCircleStrokeThickness = 2d,
        double sectorLineThickness = 1d,
        double aspectLineThickness = 1d,
        double planetMarkerRadius = 6d)
    {
        ValidateFinitePositive(nameof(outerCircleStrokeThickness), outerCircleStrokeThickness);
        ValidateFinitePositive(nameof(sectorLineThickness), sectorLineThickness);
        ValidateFinitePositive(nameof(aspectLineThickness), aspectLineThickness);
        ValidateFinitePositive(nameof(planetMarkerRadius), planetMarkerRadius);

        if (double.IsNaN(paddingRatio) || double.IsInfinity(paddingRatio) || paddingRatio < 0d || paddingRatio >= 0.5d)
        {
            throw new ArgumentOutOfRangeException(nameof(paddingRatio), "Padding ratio must be a finite number in the range [0, 0.5).");
        }

        PaddingRatio = paddingRatio;
        OuterCircleStrokeThickness = outerCircleStrokeThickness;
        SectorLineThickness = sectorLineThickness;
        AspectLineThickness = aspectLineThickness;
        PlanetMarkerRadius = planetMarkerRadius;
    }

    /// <summary>
    /// Gets the normalized padding ratio applied to the chart radius.
    /// </summary>
    public double PaddingRatio { get; }

    /// <summary>
    /// Gets the outer circle stroke thickness.
    /// </summary>
    public double OuterCircleStrokeThickness { get; }

    /// <summary>
    /// Gets the zodiac sector separator thickness.
    /// </summary>
    public double SectorLineThickness { get; }

    /// <summary>
    /// Gets the aspect line thickness.
    /// </summary>
    public double AspectLineThickness { get; }

    /// <summary>
    /// Gets the planet marker radius in device-independent pixels.
    /// </summary>
    public double PlanetMarkerRadius { get; }

    private static void ValidateFinitePositive(string parameterName, double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value) || value <= 0d)
        {
            throw new ArgumentOutOfRangeException(parameterName, "Value must be a finite positive number.");
        }
    }
}
