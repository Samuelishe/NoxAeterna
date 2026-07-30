using NoxAeterna.Geometry.Charts;

namespace NoxAeterna.Rendering.Charts;

/// <summary>
/// Represents one render-ready principal-angle label.
/// </summary>
public sealed record ChartAngleLabelPlacement(string Text, RadialPoint AnchorPoint);
