using Avalonia.Media;
using NoxAeterna.Domain.Astrology;

namespace NoxAeterna.Rendering.Charts;

/// <summary>
/// Supplies deterministic visual styles for all currently supported aspects.
/// </summary>
public static class ChartAspectStyleCatalog
{
    private static readonly ChartAspectVisualStyle Conjunction =
        new(Color.FromRgb(185, 174, 156), 1d, 0.82d, null);

    private static readonly ChartAspectVisualStyle Sextile =
        new(Color.FromRgb(72, 139, 119), 1.15d, 0.74d, null);

    private static readonly ChartAspectVisualStyle Square =
        new(Color.FromRgb(177, 76, 72), 1.4d, 0.84d, null);

    private static readonly ChartAspectVisualStyle Trine =
        new(Color.FromRgb(67, 119, 160), 1.2d, 0.76d, null);

    private static readonly ChartAspectVisualStyle Opposition =
        new(Color.FromRgb(186, 91, 73), 1.45d, 0.86d, null);

    /// <summary>
    /// Gets the visual style for a supported aspect type.
    /// </summary>
    public static ChartAspectVisualStyle Get(AspectType aspectType) =>
        aspectType switch
        {
            AspectType.Conjunction => Conjunction,
            AspectType.Sextile => Sextile,
            AspectType.Square => Square,
            AspectType.Trine => Trine,
            AspectType.Opposition => Opposition,
            _ => throw new ArgumentOutOfRangeException(nameof(aspectType), aspectType, "Unsupported aspect type.")
        };
}
