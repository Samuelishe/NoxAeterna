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
        IEnumerable<ZodiacSectorGeometry> zodiacSectors,
        IEnumerable<PlanetGlyphSlot> planetGlyphSlots,
        IEnumerable<AspectLineGeometry> aspectLines)
    {
        ZodiacSectors = Array.AsReadOnly((zodiacSectors ?? throw new ArgumentNullException(nameof(zodiacSectors))).ToArray());
        PlanetGlyphSlots = Array.AsReadOnly((planetGlyphSlots ?? throw new ArgumentNullException(nameof(planetGlyphSlots))).ToArray());
        AspectLines = Array.AsReadOnly((aspectLines ?? throw new ArgumentNullException(nameof(aspectLines))).ToArray());
    }

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
}
