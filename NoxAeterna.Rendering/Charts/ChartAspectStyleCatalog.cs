using Avalonia.Media;
using NoxAeterna.Domain.Astrology;

namespace NoxAeterna.Rendering.Charts;

/// <summary>
/// Supplies deterministic visual styles for all currently supported aspects.
/// </summary>
public static class ChartAspectStyleCatalog
{
    private static readonly ChartAspectVisualStyle Conjunction =
        new(Color.FromRgb(132, 127, 123), 0.8d, 0.48d, null);

    private static readonly ChartAspectVisualStyle Sextile =
        new(Color.FromRgb(104, 128, 114), 0.65d, 0.32d, [3d, 4d]);

    private static readonly ChartAspectVisualStyle Square =
        new(Color.FromRgb(145, 91, 88), 0.9d, 0.52d, null);

    private static readonly ChartAspectVisualStyle Trine =
        new(Color.FromRgb(101, 119, 139), 0.7d, 0.36d, [6d, 3d]);

    private static readonly ChartAspectVisualStyle Opposition =
        new(Color.FromRgb(143, 111, 82), 1d, 0.56d, [8d, 3d]);

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
