using Avalonia.Media;

namespace NoxAeterna.Rendering.Charts;

/// <summary>
/// Defines chart-local structural and glyph colors without coupling rendering to shell themes.
/// </summary>
public sealed record ChartRenderPalette(
    Color StructureColor,
    Color SubtleStructureColor,
    Color FireSectorColor,
    Color EarthSectorColor,
    Color AirSectorColor,
    Color WaterSectorColor,
    double ZodiacSectorOpacity,
    Color ZodiacGlyphColor,
    Color PlanetGlyphColor,
    Color PlanetAnchorColor,
    Color HouseCuspColor,
    Color AngleAxisColor,
    Color HouseLabelColor)
{
    /// <summary>
    /// Gets the restrained palette for a dark preview surface.
    /// </summary>
    public static ChartRenderPalette Dark { get; } = new(
        Color.FromRgb(207, 174, 108),
        Color.FromRgb(133, 119, 94),
        Color.FromRgb(132, 65, 52),
        Color.FromRgb(101, 96, 51),
        Color.FromRgb(54, 97, 108),
        Color.FromRgb(59, 66, 103),
        0.48d,
        Color.FromRgb(229, 194, 122),
        Color.FromRgb(250, 237, 210),
        Color.FromRgb(205, 188, 151),
        Color.FromRgb(174, 157, 123),
        Color.FromRgb(224, 190, 122),
        Color.FromRgb(224, 202, 158));

    /// <summary>
    /// Gets the restrained palette for a light preview surface.
    /// </summary>
    public static ChartRenderPalette Light { get; } = new(
        Color.FromRgb(103, 70, 35),
        Color.FromRgb(139, 116, 84),
        Color.FromRgb(177, 106, 88),
        Color.FromRgb(143, 137, 75),
        Color.FromRgb(86, 133, 143),
        Color.FromRgb(96, 104, 151),
        0.42d,
        Color.FromRgb(111, 70, 25),
        Color.FromRgb(48, 37, 25),
        Color.FromRgb(104, 80, 51),
        Color.FromRgb(118, 96, 65),
        Color.FromRgb(91, 57, 25),
        Color.FromRgb(75, 54, 31));
}
