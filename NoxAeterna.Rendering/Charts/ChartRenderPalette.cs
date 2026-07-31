using Avalonia.Media;

namespace NoxAeterna.Rendering.Charts;

/// <summary>
/// Defines chart-local structural and glyph colors without coupling rendering to shell themes.
/// </summary>
public sealed record ChartRenderPalette(
    Color InteriorBackgroundColor,
    Color PrimaryStructureColor,
    Color SecondaryStructureColor,
    Color FireSectorColor,
    Color EarthSectorColor,
    Color AirSectorColor,
    Color WaterSectorColor,
    double ZodiacSectorOpacity,
    Color ZodiacGlyphColor,
    Color PlanetGlyphColor,
    Color PlanetDegreeColor,
    Color PlanetAnchorColor,
    Color HouseCuspColor,
    Color HouseLabelColor,
    Color AngleAxisColor,
    Color AspectCircleColor,
    Color ConjunctionAspectColor,
    Color HardAspectColor,
    Color HarmoniousBlueAspectColor,
    Color HarmoniousTealAspectColor)
{
    /// <summary>
    /// Gets the Obsidian chart palette for a dark preview surface.
    /// </summary>
    public static ChartRenderPalette Dark { get; } = new(
        Color.Parse("#0B1020"),
        Color.Parse("#7F8AA8"),
        Color.Parse("#3E4865"),
        Color.Parse("#7A2F43"),
        Color.Parse("#285E4B"),
        Color.Parse("#245A73"),
        Color.Parse("#403777"),
        1d,
        Color.Parse("#FFF2C6"),
        Color.Parse("#F8E8FF"),
        Color.Parse("#C9B8EA"),
        Color.Parse("#8FA4C7"),
        Color.Parse("#66718F"),
        Color.Parse("#D8D3E8"),
        Color.Parse("#F2C14E"),
        Color.Parse("#48516D"),
        Color.Parse("#D8D3E8"),
        Color.Parse("#FF6275"),
        Color.Parse("#4EB7E8"),
        Color.Parse("#45D1A6"));

    /// <summary>
    /// Gets the Porcelain chart palette for a light preview surface.
    /// </summary>
    public static ChartRenderPalette Light { get; } = new(
        Color.Parse("#FAF9FE"),
        Color.Parse("#5B5870"),
        Color.Parse("#C8C3D5"),
        Color.Parse("#E77886"),
        Color.Parse("#5BB187"),
        Color.Parse("#58A9D8"),
        Color.Parse("#8574D8"),
        1d,
        Color.Parse("#2C213C"),
        Color.Parse("#2A1D3D"),
        Color.Parse("#615374"),
        Color.Parse("#6A7088"),
        Color.Parse("#8A8398"),
        Color.Parse("#4A425D"),
        Color.Parse("#8C5A00"),
        Color.Parse("#C1BBCD"),
        Color.Parse("#514A60"),
        Color.Parse("#C43F5A"),
        Color.Parse("#1E8DB4"),
        Color.Parse("#20866A"));
}
