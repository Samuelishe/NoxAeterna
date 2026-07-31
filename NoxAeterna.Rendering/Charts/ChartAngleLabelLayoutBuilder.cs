using Avalonia;

namespace NoxAeterna.Rendering.Charts;

/// <summary>
/// Places principal-angle labels outside the rim with a deterministic safe inset.
/// </summary>
public static class ChartAngleLabelLayoutBuilder
{
    /// <summary>
    /// Builds viewport-safe ASC, DSC, MC, and IC label bounds.
    /// </summary>
    public static IReadOnlyList<ChartAngleLabelLayout> Build(
        ChartRenderScene scene,
        ChartViewport viewport,
        Func<string, double, Size> measureText)
    {
        ArgumentNullException.ThrowIfNull(scene);
        ArgumentNullException.ThrowIfNull(measureText);

        var layouts = new List<ChartAngleLabelLayout>(scene.AngleLabels.Count);
        var rimRadius = viewport.EffectiveRadius * scene.Layout.RadialLanes.OuterBoundaryRadiusRatio;
        var margin = Math.Clamp(viewport.EffectiveRadius * 0.01d, 3d, 5d);

        foreach (var label in scene.AngleLabels)
        {
            var size = measureText(label.Text, viewport.VisualMetrics.AngleLabelFontSize);
            if (!double.IsFinite(size.Width) || !double.IsFinite(size.Height) || size.Width <= 0d || size.Height <= 0d)
            {
                throw new ArgumentException("Measured angle-label size must be finite and positive.");
            }

            var direction = new Vector(label.AnchorPoint.X, label.AnchorPoint.Y);
            var directionLength = Math.Sqrt((direction.X * direction.X) + (direction.Y * direction.Y));
            direction = directionLength > 1e-9
                ? new Vector(direction.X / directionLength, direction.Y / directionLength)
                : new Vector(0d, -1d);
            var projectedHalfExtent =
                (Math.Abs(direction.X) * size.Width / 2d) +
                (Math.Abs(direction.Y) * size.Height / 2d);
            var geometryRadius = label.AnchorPoint.RadiusRatio * viewport.EffectiveRadius;
            var safeRadius = Math.Max(
                geometryRadius,
                rimRadius + margin + projectedHalfExtent);
            var anchor = new Point(
                viewport.Center.X + (direction.X * safeRadius),
                viewport.Center.Y + (direction.Y * safeRadius));
            anchor = ClampAnchor(anchor, size, viewport.SafeDrawingBounds);
            layouts.Add(new ChartAngleLabelLayout(label, anchor, CenteredRect(anchor, size)));
        }

        return layouts.AsReadOnly();
    }

    private static Point ClampAnchor(Point anchor, Size size, Rect safeBounds)
    {
        var halfWidth = size.Width / 2d;
        var halfHeight = size.Height / 2d;
        return new Point(
            Math.Clamp(anchor.X, safeBounds.Left + halfWidth, safeBounds.Right - halfWidth),
            Math.Clamp(anchor.Y, safeBounds.Top + halfHeight, safeBounds.Bottom - halfHeight));
    }

    private static Rect CenteredRect(Point center, Size size) =>
        new(
            center.X - (size.Width / 2d),
            center.Y - (size.Height / 2d),
            size.Width,
            size.Height);
}
