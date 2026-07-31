using Avalonia;

namespace NoxAeterna.Rendering.Charts;

/// <summary>
/// Represents a viewport-safe principal-angle label.
/// </summary>
public sealed record ChartAngleLabelLayout(
    ChartAngleLabelPlacement Label,
    Point Anchor,
    Rect Bounds);
