using NoxAeterna.Domain.Astrology;

namespace NoxAeterna.Geometry.Charts;

/// <summary>
/// Builds a minimal render-independent circular layout for a natal chart snapshot.
/// </summary>
public sealed class CircularChartLayoutBuilder
{
    private const double ClusterThresholdDegrees = 7d;
    private static readonly double[] PlanetBandOffsets = [0d, 0.035d, 0.07d];

    /// <summary>
    /// Initializes a new instance of the <see cref="CircularChartLayoutBuilder"/> class.
    /// </summary>
    /// <param name="zodiacInnerRadiusRatio">The normalized inner radius for zodiac sectors.</param>
    /// <param name="zodiacOuterRadiusRatio">The normalized outer radius for zodiac sectors.</param>
    /// <param name="planetRadiusRatio">The normalized radius for planet glyph slots.</param>
    /// <param name="aspectRadiusRatio">The normalized radius for aspect line endpoints.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when radius arguments are invalid.</exception>
    public CircularChartLayoutBuilder(
        double zodiacInnerRadiusRatio = 0.76d,
        double zodiacOuterRadiusRatio = 1d,
        double planetRadiusRatio = 0.88d,
        double aspectRadiusRatio = 0.58d)
    {
        ValidateRadius(nameof(zodiacInnerRadiusRatio), zodiacInnerRadiusRatio, requirePositive: false);
        ValidateRadius(nameof(zodiacOuterRadiusRatio), zodiacOuterRadiusRatio, requirePositive: true);
        ValidateRadius(nameof(planetRadiusRatio), planetRadiusRatio, requirePositive: true);
        ValidateRadius(nameof(aspectRadiusRatio), aspectRadiusRatio, requirePositive: true);

        if (zodiacInnerRadiusRatio >= zodiacOuterRadiusRatio)
        {
            throw new ArgumentOutOfRangeException(
                nameof(zodiacInnerRadiusRatio),
                "Zodiac inner radius ratio must be smaller than the zodiac outer radius ratio.");
        }

        ZodiacInnerRadiusRatio = zodiacInnerRadiusRatio;
        ZodiacOuterRadiusRatio = zodiacOuterRadiusRatio;
        PlanetRadiusRatio = planetRadiusRatio;
        AspectRadiusRatio = aspectRadiusRatio;
    }

    /// <summary>
    /// Gets the normalized inner radius for zodiac sectors.
    /// </summary>
    public double ZodiacInnerRadiusRatio { get; }

    /// <summary>
    /// Gets the normalized outer radius for zodiac sectors.
    /// </summary>
    public double ZodiacOuterRadiusRatio { get; }

    /// <summary>
    /// Gets the normalized radius for planet glyph slots.
    /// </summary>
    public double PlanetRadiusRatio { get; }

    /// <summary>
    /// Gets the normalized radius for aspect line endpoints.
    /// </summary>
    public double AspectRadiusRatio { get; }

    /// <summary>
    /// Builds a render-independent circular chart layout for the supplied natal chart snapshot.
    /// </summary>
    /// <param name="chart">The source chart snapshot.</param>
    /// <returns>The prepared circular chart layout.</returns>
    public CircularChartLayout Build(NatalChart chart)
    {
        ArgumentNullException.ThrowIfNull(chart);

        var zodiacSectors = Enum
            .GetValues<ZodiacSign>()
            .Select(sign => new ZodiacSectorGeometry(
                sign,
                new AngularPosition((int)sign * 30d),
                new AngularPosition(((int)sign + 1) * 30d),
                ZodiacInnerRadiusRatio,
                ZodiacOuterRadiusRatio))
            .ToArray();

        var radialBandsByBody = BuildRadialBands(chart.Positions);
        var glyphSlots = chart.Positions
            .OrderBy(static position => position.Body)
            .Select((position, index) =>
            {
                var angle = AngularPosition.FromLongitude(position.EclipticLongitude);
                var radialBandIndex = radialBandsByBody[position.Body];
                var radiusRatio = Math.Min(0.98d, PlanetRadiusRatio + PlanetBandOffsets[radialBandIndex]);
                return new PlanetGlyphSlot(
                    position.Body,
                    position.EclipticLongitude,
                    angle,
                    new RadialPoint(angle, radiusRatio),
                    index,
                    radialBandIndex);
            })
            .ToArray();

        var glyphSlotsByBody = glyphSlots.ToDictionary(static slot => slot.Body);

        var aspectLines = chart.Aspects
            .OrderBy(static aspect => aspect.SourceBody)
            .ThenBy(static aspect => aspect.TargetBody)
            .ThenBy(static aspect => aspect.AspectType)
            .Select(aspect =>
            {
                var sourceSlot = glyphSlotsByBody[aspect.SourceBody];
                var targetSlot = glyphSlotsByBody[aspect.TargetBody];

                return new AspectLineGeometry(
                    aspect.SourceBody,
                    aspect.TargetBody,
                    sourceSlot.Angle,
                    targetSlot.Angle,
                    new RadialPoint(sourceSlot.Angle, AspectRadiusRatio),
                    new RadialPoint(targetSlot.Angle, AspectRadiusRatio),
                    aspect.AspectType);
            })
            .ToArray();

        return new CircularChartLayout(zodiacSectors, glyphSlots, aspectLines);
    }

    private static IReadOnlyDictionary<CelestialBody, int> BuildRadialBands(IEnumerable<PlanetPosition> positions)
    {
        var ordered = positions
            .OrderBy(static position => position.EclipticLongitude.Degrees)
            .ThenBy(static position => position.Body)
            .ToArray();

        var radialBandsByBody = new Dictionary<CelestialBody, int>(ordered.Length);
        var currentBandIndex = 0;

        for (var index = 0; index < ordered.Length; index++)
        {
            if (index > 0)
            {
                var previousLongitude = ordered[index - 1].EclipticLongitude.Degrees;
                var currentLongitude = ordered[index].EclipticLongitude.Degrees;
                var delta = AspectMath.CalculateAngularDelta(new ZodiacLongitude(previousLongitude), new ZodiacLongitude(currentLongitude));
                currentBandIndex = delta < ClusterThresholdDegrees
                    ? (currentBandIndex + 1) % PlanetBandOffsets.Length
                    : 0;
            }

            radialBandsByBody[ordered[index].Body] = currentBandIndex;
        }

        return radialBandsByBody;
    }

    private static void ValidateRadius(string parameterName, double radiusRatio, bool requirePositive)
    {
        if (double.IsNaN(radiusRatio) || double.IsInfinity(radiusRatio) || radiusRatio < 0d || (requirePositive && radiusRatio == 0d))
        {
            throw new ArgumentOutOfRangeException(parameterName, "Radius ratio must be a finite non-negative number.");
        }
    }
}
