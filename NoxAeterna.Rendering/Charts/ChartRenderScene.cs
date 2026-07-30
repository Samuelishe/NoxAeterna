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
        HouseCusps = Layout.HouseCusps;
        HouseNumberAnchors = Layout.HouseNumberAnchors;
        AngleAxes = Layout.AngleAxes;
        AngleLabels = Array.AsReadOnly(BuildAngleLabels(Layout.AngleAxes));
        ZodiacGlyphs = Array.AsReadOnly(BuildZodiacGlyphs(Layout));
        PlanetGlyphs = Array.AsReadOnly(BuildPlanetGlyphs(Layout.PlanetGlyphSlots));
        PlanetAnnotations = Array.AsReadOnly(BuildPlanetAnnotations(Layout.PlanetGlyphSlots));
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
    /// Gets the house cusp geometry without any ephemeris-provider dependency.
    /// </summary>
    public IReadOnlyList<HouseCuspGeometry> HouseCusps { get; }

    /// <summary>
    /// Gets the house-number anchors.
    /// </summary>
    public IReadOnlyList<HouseNumberAnchor> HouseNumberAnchors { get; }

    /// <summary>
    /// Gets the principal chart axes.
    /// </summary>
    public IReadOnlyList<ChartAngleAxisGeometry> AngleAxes { get; }

    /// <summary>
    /// Gets ASC, DSC, MC, and IC label placements when houses are available.
    /// </summary>
    public IReadOnlyList<ChartAngleLabelPlacement> AngleLabels { get; }

    /// <summary>
    /// Gets the zodiac vector glyphs to render around the chart ring.
    /// </summary>
    public IReadOnlyList<ChartGlyphPlacement> ZodiacGlyphs { get; }

    /// <summary>
    /// Gets the planetary vector glyphs to render at geometry-owned anchors.
    /// </summary>
    public IReadOnlyList<ChartGlyphPlacement> PlanetGlyphs { get; }

    /// <summary>
    /// Gets render-ready planet annotation groups with degrees, retrograde state, and displacement state.
    /// </summary>
    public IReadOnlyList<ChartPlanetAnnotationPlacement> PlanetAnnotations { get; }

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
                var midAngle = layout.Orientation.TransformDegrees(((int)sector.Sign * 30d) + 15d);
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

    private static ChartPlanetAnnotationPlacement[] BuildPlanetAnnotations(
        IEnumerable<PlanetGlyphSlot> glyphSlots) =>
        glyphSlots
            .Select(slot => new ChartPlanetAnnotationPlacement(
                slot.Body,
                ChartGlyphCatalog.GetBodyGlyph(slot.Body),
                slot.AnchorPoint,
                $"{(int)Math.Floor(slot.Longitude.Degrees % 30d):00}°",
                slot.IsRetrograde,
                CircularDelta(slot.SourceAngle.Degrees, slot.DisplayAngle.Degrees) > 0.01d))
            .ToArray();

    private static ChartAngleLabelPlacement[] BuildAngleLabels(
        IEnumerable<ChartAngleAxisGeometry> axes) =>
        axes.SelectMany(axis => axis.AxisType switch
            {
                ChartAngleAxisType.AscendantDescendant =>
                new[]
                {
                    new ChartAngleLabelPlacement("ASC", axis.PrimaryLabelAnchor),
                    new ChartAngleLabelPlacement("DSC", axis.OppositeLabelAnchor)
                },
                ChartAngleAxisType.MidheavenImumCoeli =>
                new[]
                {
                    new ChartAngleLabelPlacement("MC", axis.PrimaryLabelAnchor),
                    new ChartAngleLabelPlacement("IC", axis.OppositeLabelAnchor)
                },
                _ => throw new ArgumentOutOfRangeException(
                    nameof(axis.AxisType),
                    axis.AxisType,
                    "Unsupported chart angle axis.")
            })
            .ToArray();

    private static double CircularDelta(double first, double second)
    {
        var delta = Math.Abs(first - second);
        return Math.Min(delta, 360d - delta);
    }
}
