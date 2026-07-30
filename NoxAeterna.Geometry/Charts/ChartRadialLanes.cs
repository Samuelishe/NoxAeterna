namespace NoxAeterna.Geometry.Charts;

/// <summary>
/// Defines the named, render-independent radial zones of a circular chart.
/// </summary>
public sealed record ChartRadialLanes
{
    private static readonly double[] DefaultPlanetSubLaneRadii = [0.715d, 0.665d, 0.615d, 0.575d];

    /// <summary>
    /// Initializes a new instance of the <see cref="ChartRadialLanes"/> class.
    /// </summary>
    public ChartRadialLanes(
        double outerBoundaryRadiusRatio,
        RadialLaneBounds zodiacRing,
        RadialLaneBounds zodiacGlyphLane,
        RadialLaneBounds planetGlyphLane,
        IEnumerable<double> planetSubLaneRadiusRatios,
        double aspectInteriorRadiusRatio,
        RadialLaneBounds futureHouseRing)
    {
        if (!double.IsFinite(outerBoundaryRadiusRatio) ||
            outerBoundaryRadiusRatio <= 0d ||
            outerBoundaryRadiusRatio > 1d)
        {
            throw new ArgumentOutOfRangeException(
                nameof(outerBoundaryRadiusRatio),
                "Outer boundary radius must be a finite number in the range (0, 1].");
        }

        if (!double.IsFinite(aspectInteriorRadiusRatio) ||
            aspectInteriorRadiusRatio <= 0d ||
            aspectInteriorRadiusRatio > 1d)
        {
            throw new ArgumentOutOfRangeException(
                nameof(aspectInteriorRadiusRatio),
                "Aspect interior radius must be a finite number in the range (0, 1].");
        }

        var copiedSubLanes = (planetSubLaneRadiusRatios ??
                              throw new ArgumentNullException(nameof(planetSubLaneRadiusRatios)))
            .ToArray();

        if (copiedSubLanes.Length == 0 ||
            copiedSubLanes.Any(radius => !double.IsFinite(radius) || !planetGlyphLane.Contains(radius)))
        {
            throw new ArgumentException(
                "Planet sub-lanes must contain at least one finite radius inside the planet glyph lane.",
                nameof(planetSubLaneRadiusRatios));
        }

        if (copiedSubLanes.Distinct().Count() != copiedSubLanes.Length ||
            !copiedSubLanes.SequenceEqual(copiedSubLanes.OrderDescending()))
        {
            throw new ArgumentException(
                "Planet sub-lanes must be unique and ordered from outermost to innermost.",
                nameof(planetSubLaneRadiusRatios));
        }

        if (zodiacRing.OuterRadiusRatio != outerBoundaryRadiusRatio ||
            zodiacGlyphLane.InnerRadiusRatio < zodiacRing.InnerRadiusRatio ||
            zodiacGlyphLane.OuterRadiusRatio > zodiacRing.OuterRadiusRatio ||
            planetGlyphLane.OuterRadiusRatio >= zodiacGlyphLane.InnerRadiusRatio ||
            futureHouseRing.OuterRadiusRatio >= planetGlyphLane.InnerRadiusRatio ||
            aspectInteriorRadiusRatio >= futureHouseRing.InnerRadiusRatio)
        {
            throw new ArgumentException("Chart radial zones must be ordered, contained, and non-overlapping.");
        }

        OuterBoundaryRadiusRatio = outerBoundaryRadiusRatio;
        ZodiacRing = zodiacRing;
        ZodiacGlyphLane = zodiacGlyphLane;
        PlanetGlyphLane = planetGlyphLane;
        PlanetSubLaneRadiusRatios = Array.AsReadOnly(copiedSubLanes);
        AspectInteriorRadiusRatio = aspectInteriorRadiusRatio;
        FutureHouseRing = futureHouseRing;
    }

    /// <summary>
    /// Gets the default radial-zone definition for the current chart.
    /// </summary>
    public static ChartRadialLanes Default { get; } = new(
        outerBoundaryRadiusRatio: 0.94d,
        zodiacRing: new RadialLaneBounds(0.78d, 0.94d),
        zodiacGlyphLane: new RadialLaneBounds(0.835d, 0.895d),
        planetGlyphLane: new RadialLaneBounds(0.57d, 0.72d),
        planetSubLaneRadiusRatios: DefaultPlanetSubLaneRadii,
        aspectInteriorRadiusRatio: 0.47d,
        futureHouseRing: new RadialLaneBounds(0.495d, 0.545d));

    /// <summary>
    /// Gets the normalized outer chart boundary.
    /// </summary>
    public double OuterBoundaryRadiusRatio { get; }

    /// <summary>
    /// Gets the zodiac ring bounds.
    /// </summary>
    public RadialLaneBounds ZodiacRing { get; }

    /// <summary>
    /// Gets the zodiac vector-glyph lane bounds.
    /// </summary>
    public RadialLaneBounds ZodiacGlyphLane { get; }

    /// <summary>
    /// Gets the planet vector-glyph lane bounds.
    /// </summary>
    public RadialLaneBounds PlanetGlyphLane { get; }

    /// <summary>
    /// Gets ordered planet anchor radii from outermost to innermost.
    /// </summary>
    public IReadOnlyList<double> PlanetSubLaneRadiusRatios { get; }

    /// <summary>
    /// Gets the maximum radius available to aspect endpoints.
    /// </summary>
    public double AspectInteriorRadiusRatio { get; }

    /// <summary>
    /// Gets geometry reserved for a possible future house ring.
    /// </summary>
    /// <remarks>No houses or house cusps are produced by the current layout.</remarks>
    public RadialLaneBounds FutureHouseRing { get; }
}
