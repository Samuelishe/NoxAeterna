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
        ZodiacLabels = Array.AsReadOnly(BuildZodiacLabels(Layout.ZodiacSectors));
        PlanetLabels = Array.AsReadOnly(BuildPlanetLabels(Layout.PlanetGlyphSlots));
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
    /// Gets the zodiac labels to render around the chart ring.
    /// </summary>
    public IReadOnlyList<ChartTextLabel> ZodiacLabels { get; }

    /// <summary>
    /// Gets the planetary labels to render at planetary marker slots.
    /// </summary>
    public IReadOnlyList<ChartTextLabel> PlanetLabels { get; }

    /// <summary>
    /// Creates a chart render scene from prepared circular chart geometry.
    /// </summary>
    /// <param name="layout">The source circular chart layout.</param>
    /// <returns>A render-ready chart scene.</returns>
    public static ChartRenderScene FromLayout(CircularChartLayout layout) => new(layout);

    private static ChartTextLabel[] BuildZodiacLabels(IEnumerable<ZodiacSectorGeometry> sectors) =>
        sectors
            .Select(sector =>
            {
                var midAngle = new AngularPosition((sector.StartAngle.Degrees + sector.EndAngle.Degrees) / 2d);
                var radius = sector.InnerRadiusRatio + ((sector.OuterRadiusRatio - sector.InnerRadiusRatio) * 0.48d);
                return new ChartTextLabel(
                    ChartGlyphCatalog.GetSignGlyph(sector.Sign),
                    new RadialPoint(midAngle, radius),
                    22d,
                    ChartTextLabelStyle.Zodiac);
            })
            .ToArray();

    private static ChartTextLabel[] BuildPlanetLabels(IEnumerable<PlanetGlyphSlot> glyphSlots) =>
        glyphSlots
            .Select(slot => new ChartTextLabel(
                ChartGlyphCatalog.GetBodyGlyph(slot.Body),
                new RadialPoint(slot.Angle, Math.Min(0.98d, slot.AnchorPoint.RadiusRatio + 0.03d)),
                18d,
                ChartTextLabelStyle.Planet))
            .ToArray();
}
