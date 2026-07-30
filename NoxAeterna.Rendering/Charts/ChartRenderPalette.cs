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
        Color.FromRgb(130, 132, 140),
        Color.FromRgb(86, 88, 96),
        Color.FromRgb(202, 184, 145),
        Color.FromRgb(224, 211, 180),
        Color.FromRgb(139, 137, 132));

    /// <summary>
    /// Gets the restrained palette for a light preview surface.
    /// </summary>
    public static ChartRenderPalette Light { get; } = new(
        Color.FromRgb(93, 88, 82),
        Color.FromRgb(157, 147, 135),
        Color.FromRgb(119, 91, 54),
        Color.FromRgb(70, 61, 50),
        Color.FromRgb(116, 107, 97));
}
