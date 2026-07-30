using Avalonia.Media;

namespace NoxAeterna.Rendering.Charts;

/// <summary>
/// Defines restrained visual hierarchy for one aspect type.
/// </summary>
public sealed record ChartAspectVisualStyle(
    Color Color,
    double Thickness,
    double Opacity,
    IReadOnlyList<double>? DashPattern);
