using Avalonia.Media;
using NoxAeterna.Domain.Astrology;

namespace NoxAeterna.Rendering.Charts;

/// <summary>
/// Supplies deterministic visual styles for all currently supported aspects.
/// </summary>
public static class ChartAspectStyleCatalog
{
    private static readonly ChartAspectVisualStyle Conjunction =
        new(Color.FromRgb(128, 119, 109), 0.8d, 0.65d, null);

    private static readonly ChartAspectVisualStyle Sextile =
        new(Color.FromRgb(82, 116, 99), 0.68d, 0.54d, [3d, 4d]);

    private static readonly ChartAspectVisualStyle Square =
        new(Color.FromRgb(145, 75, 70), 0.92d, 0.70d, null);

    private static readonly ChartAspectVisualStyle Trine =
        new(Color.FromRgb(70, 98, 127), 0.72d, 0.56d, [6d, 3d]);

    private static readonly ChartAspectVisualStyle Opposition =
        new(Color.FromRgb(139, 91, 51), 1d, 0.74d, [8d, 3d]);

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
