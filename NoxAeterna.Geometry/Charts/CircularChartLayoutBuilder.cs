using NoxAeterna.Domain.Astrology;

namespace NoxAeterna.Geometry.Charts;

/// <summary>
/// Builds a minimal render-independent circular layout for a natal chart snapshot.
/// </summary>
public sealed class CircularChartLayoutBuilder
{
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

        var glyphSlots = chart.Positions
            .OrderBy(static position => position.Body)
            .Select((position, index) =>
            {
                var angle = AngularPosition.FromLongitude(position.EclipticLongitude);
                return new PlanetGlyphSlot(
                    position.Body,
                    position.EclipticLongitude,
                    angle,
                    new RadialPoint(angle, PlanetRadiusRatio),
                    index,
                    radialBandIndex: 0);
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

    private static void ValidateRadius(string parameterName, double radiusRatio, bool requirePositive)
    {
        if (double.IsNaN(radiusRatio) || double.IsInfinity(radiusRatio) || radiusRatio < 0d || (requirePositive && radiusRatio == 0d))
        {
            throw new ArgumentOutOfRangeException(parameterName, "Radius ratio must be a finite non-negative number.");
        }
    }
}
