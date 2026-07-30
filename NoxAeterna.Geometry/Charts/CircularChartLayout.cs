namespace NoxAeterna.Geometry.Charts;

/// <summary>
/// Represents a render-independent circular chart layout.
/// </summary>
public sealed record CircularChartLayout
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CircularChartLayout"/> class.
    /// </summary>
    /// <param name="zodiacSectors">The zodiac sector geometry.</param>
    /// <param name="planetGlyphSlots">The planetary glyph slots.</param>
    /// <param name="aspectLines">The aspect line geometry.</param>
    public CircularChartLayout(
        ChartRadialLanes radialLanes,
        ChartOrientation orientation,
        IEnumerable<ZodiacSectorGeometry> zodiacSectors,
        IEnumerable<PlanetGlyphSlot> planetGlyphSlots,
        IEnumerable<AspectLineGeometry> aspectLines,
        IEnumerable<HouseCuspGeometry> houseCusps,
        IEnumerable<HouseNumberAnchor> houseNumberAnchors,
        IEnumerable<ChartAngleAxisGeometry> angleAxes)
    {
        RadialLanes = radialLanes ?? throw new ArgumentNullException(nameof(radialLanes));
        Orientation = orientation;
        ZodiacSectors = Array.AsReadOnly((zodiacSectors ?? throw new ArgumentNullException(nameof(zodiacSectors))).ToArray());
        PlanetGlyphSlots = Array.AsReadOnly((planetGlyphSlots ?? throw new ArgumentNullException(nameof(planetGlyphSlots))).ToArray());
        AspectLines = Array.AsReadOnly((aspectLines ?? throw new ArgumentNullException(nameof(aspectLines))).ToArray());
        HouseCusps = Array.AsReadOnly((houseCusps ?? throw new ArgumentNullException(nameof(houseCusps))).ToArray());
        HouseNumberAnchors = Array.AsReadOnly((houseNumberAnchors ?? throw new ArgumentNullException(nameof(houseNumberAnchors))).ToArray());
        AngleAxes = Array.AsReadOnly((angleAxes ?? throw new ArgumentNullException(nameof(angleAxes))).ToArray());
    }

    /// <summary>
    /// Gets the named radial zones used by this layout.
    /// </summary>
    public ChartRadialLanes RadialLanes { get; }

    /// <summary>
    /// Gets the shared source-longitude to chart-space transform.
    /// </summary>
    public ChartOrientation Orientation { get; }

    /// <summary>
    /// Gets the zodiac sector geometry.
    /// </summary>
    public IReadOnlyList<ZodiacSectorGeometry> ZodiacSectors { get; }

    /// <summary>
    /// Gets the planetary glyph slots.
    /// </summary>
    public IReadOnlyList<PlanetGlyphSlot> PlanetGlyphSlots { get; }

    /// <summary>
    /// Gets the aspect line geometry.
    /// </summary>
    public IReadOnlyList<AspectLineGeometry> AspectLines { get; }

    /// <summary>
    /// Gets the transformed house cusp lines, or an empty collection when houses are unavailable.
    /// </summary>
    public IReadOnlyList<HouseCuspGeometry> HouseCusps { get; }

    /// <summary>
    /// Gets the house-number anchors, or an empty collection when houses are unavailable.
    /// </summary>
    public IReadOnlyList<HouseNumberAnchor> HouseNumberAnchors { get; }

    /// <summary>
    /// Gets the ASC–DSC and MC–IC axes, or an empty collection when houses are unavailable.
    /// </summary>
    public IReadOnlyList<ChartAngleAxisGeometry> AngleAxes { get; }
}
