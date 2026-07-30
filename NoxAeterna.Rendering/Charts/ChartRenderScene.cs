using NoxAeterna.Geometry.Charts;

namespace NoxAeterna.Rendering.Charts;

/// <summary>
/// Represents a minimal render-ready chart scene derived from geometry output.
/// </summary>
public sealed record ChartRenderScene
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChartRenderScene"/> class.
    /// </summary>
    /// <param name="layout">The source circular chart layout.</param>
    public ChartRenderScene(CircularChartLayout layout)
    {
        Layout = layout ?? throw new ArgumentNullException(nameof(layout));
        ZodiacSectors = Layout.ZodiacSectors;
        PlanetGlyphSlots = Layout.PlanetGlyphSlots;
        AspectLines = Layout.AspectLines;
        ZodiacGlyphs = Array.AsReadOnly(BuildZodiacGlyphs(Layout));
        PlanetGlyphs = Array.AsReadOnly(BuildPlanetGlyphs(Layout.PlanetGlyphSlots));
    }

    /// <summary>
    /// Gets the source circular chart layout.
    /// </summary>
    public CircularChartLayout Layout { get; }

    /// <summary>
    /// Gets the zodiac sectors to render.
    /// </summary>
    public IReadOnlyList<ZodiacSectorGeometry> ZodiacSectors { get; }

    /// <summary>
    /// Gets the planetary glyph slots to render.
    /// </summary>
    public IReadOnlyList<PlanetGlyphSlot> PlanetGlyphSlots { get; }

    /// <summary>
    /// Gets the aspect lines to render.
    /// </summary>
    public IReadOnlyList<AspectLineGeometry> AspectLines { get; }

    /// <summary>
    /// Gets the zodiac vector glyphs to render around the chart ring.
    /// </summary>
    public IReadOnlyList<ChartGlyphPlacement> ZodiacGlyphs { get; }

    /// <summary>
    /// Gets the planetary vector glyphs to render at geometry-owned anchors.
    /// </summary>
    public IReadOnlyList<ChartGlyphPlacement> PlanetGlyphs { get; }

    /// <summary>
    /// Creates a chart render scene from prepared circular chart geometry.
    /// </summary>
    /// <param name="layout">The source circular chart layout.</param>
    /// <returns>A render-ready chart scene.</returns>
    public static ChartRenderScene FromLayout(CircularChartLayout layout) => new(layout);

    private static ChartGlyphPlacement[] BuildZodiacGlyphs(CircularChartLayout layout) =>
        layout.ZodiacSectors
            .Select(sector =>
            {
                var midAngle = new AngularPosition(sector.StartAngle.Degrees + 15d);
                return new ChartGlyphPlacement(
                    ChartGlyphCatalog.GetSignGlyph(sector.Sign),
                    new RadialPoint(midAngle, layout.RadialLanes.ZodiacGlyphLane.MidpointRadiusRatio),
                    24d,
                    ChartGlyphStyle.Zodiac);
            })
            .ToArray();

    private static ChartGlyphPlacement[] BuildPlanetGlyphs(IEnumerable<PlanetGlyphSlot> glyphSlots) =>
        glyphSlots
            .Select(slot => new ChartGlyphPlacement(
                ChartGlyphCatalog.GetBodyGlyph(slot.Body),
                slot.AnchorPoint,
                17d,
                ChartGlyphStyle.Planet))
            .ToArray();
}
