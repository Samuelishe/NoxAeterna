using Avalonia;

namespace NoxAeterna.Rendering.Charts;

/// <summary>
/// Represents one visible portion of a render-owned line segment.
/// </summary>
public readonly record struct ChartLineSegment(Point Source, Point Target);
