using Avalonia.Media;

namespace NoxAeterna.Rendering.Charts;

/// <summary>
/// Defines chart-local structural and glyph colors without coupling rendering to shell themes.
/// </summary>
public sealed record ChartRenderPalette(
    Color StructureColor,
    Color SubtleStructureColor,
    Color ZodiacGlyphColor,
    Color PlanetGlyphColor,
    Color PlanetAnchorColor)
{
    /// <summary>
    /// Gets the restrained palette for a dark preview surface.
    /// </summary>
    public static ChartRenderPalette Dark { get; } = new(
        Color.FromRgb(184, 157, 103),
        Color.FromRgb(116, 105, 88),
        Color.FromRgb(211, 179, 116),
        Color.FromRgb(238, 222, 190),
        Color.FromRgb(169, 156, 132));

    /// <summary>
    /// Gets the restrained palette for a light preview surface.
    /// </summary>
    public static ChartRenderPalette Light { get; } = new(
        Color.FromRgb(112, 83, 48),
        Color.FromRgb(158, 139, 112),
        Color.FromRgb(132, 91, 39),
        Color.FromRgb(67, 52, 34),
        Color.FromRgb(123, 102, 75));
}
