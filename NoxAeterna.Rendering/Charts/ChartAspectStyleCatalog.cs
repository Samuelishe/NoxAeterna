using Avalonia.Media;
using NoxAeterna.Domain.Astrology;

namespace NoxAeterna.Rendering.Charts;

/// <summary>
/// Supplies deterministic visual styles for all currently supported aspects.
/// </summary>
public static class ChartAspectStyleCatalog
{
    /// <summary>
    /// Gets the theme-aware visual style for a supported aspect type.
    /// </summary>
    public static ChartAspectVisualStyle Get(
        AspectType aspectType,
        ChartRenderPalette palette)
    {
        ArgumentNullException.ThrowIfNull(palette);

        return
        aspectType switch
        {
            AspectType.Conjunction =>
                new ChartAspectVisualStyle(palette.ConjunctionAspectColor, 1d, 0.90d, null),
            AspectType.Sextile =>
                new ChartAspectVisualStyle(palette.HarmoniousTealAspectColor, 1.15d, 0.88d, null),
            AspectType.Square =>
                new ChartAspectVisualStyle(palette.HardAspectColor, 1.4d, 0.92d, null),
            AspectType.Trine =>
                new ChartAspectVisualStyle(palette.HarmoniousBlueAspectColor, 1.2d, 0.88d, null),
            AspectType.Opposition =>
                new ChartAspectVisualStyle(palette.HardAspectColor, 1.45d, 0.94d, null),
            _ => throw new ArgumentOutOfRangeException(nameof(aspectType), aspectType, "Unsupported aspect type.")
        };
    }
}
