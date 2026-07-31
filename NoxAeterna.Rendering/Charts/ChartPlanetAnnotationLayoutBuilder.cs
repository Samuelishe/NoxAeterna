using Avalonia;
using NoxAeterna.Domain.Astrology;
using NoxAeterna.Geometry.Charts;

namespace NoxAeterna.Rendering.Charts;

/// <summary>
/// Builds deterministic viewport-specific bounds for complete planet annotation visuals.
/// </summary>
public static class ChartPlanetAnnotationLayoutBuilder
{
    private const double DisplacementThreshold = 0.01d;
    private const double MaximumAngularAdjustmentDegrees = 14d;
    private const double AngularAdjustmentStepDegrees = 2d;

    /// <summary>
    /// Builds the final glyph, label, protected-envelope, and connector geometry.
    /// </summary>
    public static IReadOnlyList<ChartPlanetAnnotationLayout> Build(
        ChartRenderScene scene,
        ChartViewport viewport,
        Func<string, double, Size> measureText)
    {
        ArgumentNullException.ThrowIfNull(scene);
        ArgumentNullException.ThrowIfNull(measureText);

        var slotsByBody = scene.PlanetGlyphSlots.ToDictionary(static slot => slot.Body);
        var accepted = new List<ChartPlanetAnnotationLayout>(scene.PlanetAnnotations.Count);

        foreach (var annotation in scene.PlanetAnnotations.OrderBy(static item => item.Body))
        {
            var slot = slotsByBody[annotation.Body];
            var labelText = GetLabelText(annotation);
            var labelSize = measureText(labelText, viewport.VisualMetrics.PlanetAnnotationFontSize);
            ValidateTextSize(labelSize);

            var selected = FindPlacement(
                annotation,
                slot,
                labelSize,
                viewport,
                scene.Layout.RadialLanes,
                accepted);
            accepted.Add(selected);
        }

        return accepted.AsReadOnly();
    }

    /// <summary>
    /// Gets the exact degree and optional retrograde label rendered for an annotation.
    /// </summary>
    public static string GetLabelText(ChartPlanetAnnotationPlacement annotation)
    {
        ArgumentNullException.ThrowIfNull(annotation);
        return annotation.IsRetrograde
            ? $"{annotation.DegreeText} R"
            : annotation.DegreeText;
    }

    private static ChartPlanetAnnotationLayout FindPlacement(
        ChartPlanetAnnotationPlacement annotation,
        PlanetGlyphSlot slot,
        Size labelSize,
        ChartViewport viewport,
        ChartRadialLanes lanes,
        IReadOnlyCollection<ChartPlanetAnnotationLayout> accepted)
    {
        foreach (var candidate in EnumerateCandidates(slot, lanes))
        {
            var layout = CreateLayout(annotation, slot, candidate, labelSize, viewport, lanes);
            if (Contains(viewport.SafeDrawingBounds, layout.ProtectedBounds) &&
                IsInsidePlanetAnnotationCircle(layout, viewport, lanes) &&
                accepted.All(existing => !Overlaps(existing.ProtectedBounds, layout.ProtectedBounds)))
            {
                return layout;
            }
        }

        var fallback = CreateLayout(
            annotation,
            slot,
            slot.AnchorPoint,
            labelSize,
            viewport,
            lanes);
        return TranslateInside(fallback, viewport.SafeDrawingBounds);
    }

    private static IEnumerable<RadialPoint> EnumerateCandidates(
        PlanetGlyphSlot slot,
        ChartRadialLanes lanes)
    {
        var radii = new[] { slot.AnchorPoint.RadiusRatio }
            .Concat(lanes.PlanetSubLaneRadiusRatios.Where(radius =>
                Math.Abs(radius - slot.AnchorPoint.RadiusRatio) > 1e-9))
            .ToArray();

        foreach (var radius in radii)
        {
            yield return new RadialPoint(slot.DisplayAngle, radius);
        }

        for (var adjustment = AngularAdjustmentStepDegrees;
             adjustment <= MaximumAngularAdjustmentDegrees;
             adjustment += AngularAdjustmentStepDegrees)
        {
            foreach (var direction in new[] { -1d, 1d })
            {
                var angle = new AngularPosition(slot.DisplayAngle.Degrees + (adjustment * direction));
                foreach (var radius in radii)
                {
                    yield return new RadialPoint(angle, radius);
                }
            }
        }
    }

    private static ChartPlanetAnnotationLayout CreateLayout(
        ChartPlanetAnnotationPlacement annotation,
        PlanetGlyphSlot slot,
        RadialPoint finalRadialPoint,
        Size labelSize,
        ChartViewport viewport,
        ChartRadialLanes lanes)
    {
        var anchor = ToPoint(viewport, finalRadialPoint);
        var metrics = viewport.VisualMetrics;
        var glyphScale = metrics.PlanetGlyphSize /
                         Math.Max(annotation.Glyph.UnitBounds.Width, annotation.Glyph.UnitBounds.Height);
        var glyphWidth = (annotation.Glyph.UnitBounds.Width * glyphScale) + metrics.GlyphStrokeThickness;
        var glyphHeight = (annotation.Glyph.UnitBounds.Height * glyphScale) + metrics.GlyphStrokeThickness;
        var glyphBounds = CenteredRect(anchor, new Size(glyphWidth, glyphHeight));
        var labelAnchor = new Point(
            anchor.X,
            anchor.Y +
            (metrics.PlanetGlyphSize / 2d) +
            (metrics.PlanetAnnotationFontSize * 0.72d));
        var labelBounds = CenteredRect(labelAnchor, labelSize);
        var visualBounds = Union(glyphBounds, labelBounds);
        var padding = Math.Clamp(viewport.EffectiveRadius * 0.008d, 2d, 3.5d);
        var glyphProtectedBounds = Inflate(glyphBounds, padding);
        var labelProtectedBounds = Inflate(labelBounds, padding);
        var protectedBounds = Union(glyphProtectedBounds, labelProtectedBounds);
        var tickInnerRadius = lanes.PlanetGlyphLane.OuterRadiusRatio + 0.012d;
        var tickOuterRadius = Math.Min(
            lanes.ZodiacRing.InnerRadiusRatio - 0.012d,
            tickInnerRadius + 0.018d);
        var rawConnectorStart = ToPoint(
            viewport,
            new RadialPoint(slot.SourceAngle, tickOuterRadius));
        var renderAdjustment = Distance(anchor, ToPoint(viewport, slot.AnchorPoint)) > 0.25d;
        var hasDisplacement = annotation.HasDisplacement || renderAdjustment;
        var connector = hasDisplacement
            ? CreateConnector(rawConnectorStart, protectedBounds, padding + 2d)
            : (Start: rawConnectorStart, Endpoint: (Point?)null);

        return new ChartPlanetAnnotationLayout(
            annotation,
            anchor,
            glyphBounds,
            labelBounds,
            glyphProtectedBounds,
            labelProtectedBounds,
            visualBounds,
            protectedBounds,
            connector.Start,
            connector.Endpoint,
            hasDisplacement);
    }

    private static ChartPlanetAnnotationLayout TranslateInside(
        ChartPlanetAnnotationLayout layout,
        Rect safeBounds)
    {
        var offsetX = layout.ProtectedBounds.Left < safeBounds.Left
            ? safeBounds.Left - layout.ProtectedBounds.Left
            : layout.ProtectedBounds.Right > safeBounds.Right
                ? safeBounds.Right - layout.ProtectedBounds.Right
                : 0d;
        var offsetY = layout.ProtectedBounds.Top < safeBounds.Top
            ? safeBounds.Top - layout.ProtectedBounds.Top
            : layout.ProtectedBounds.Bottom > safeBounds.Bottom
                ? safeBounds.Bottom - layout.ProtectedBounds.Bottom
                : 0d;

        if (Math.Abs(offsetX) <= 1e-9 && Math.Abs(offsetY) <= 1e-9)
        {
            return layout;
        }

        var offset = new Vector(offsetX, offsetY);
        var protectedBounds = Translate(layout.ProtectedBounds, offset);
        var connector = CreateConnector(layout.ConnectorStart, protectedBounds, 5d);

        return layout with
        {
            FinalAnchor = layout.FinalAnchor + offset,
            GlyphBounds = Translate(layout.GlyphBounds, offset),
            LabelBounds = Translate(layout.LabelBounds, offset),
            GlyphProtectedBounds = Translate(layout.GlyphProtectedBounds, offset),
            LabelProtectedBounds = Translate(layout.LabelProtectedBounds, offset),
            VisualBounds = Translate(layout.VisualBounds, offset),
            ProtectedBounds = protectedBounds,
            ConnectorStart = connector.Start,
            ConnectorEndpoint = connector.Endpoint,
            HasDisplacement = true
        };
    }

    private static (Point Start, Point? Endpoint) CreateConnector(
        Point source,
        Rect protectedBounds,
        double outsideOffset)
    {
        var endpoint = IntersectRayWithBounds(source, protectedBounds);
        if (!ContainsInclusive(protectedBounds, source))
        {
            return (source, endpoint);
        }

        var direction = endpoint - protectedBounds.Center;
        var length = Math.Sqrt((direction.X * direction.X) + (direction.Y * direction.Y));
        if (length <= 1e-9)
        {
            return (source, endpoint);
        }

        var start = endpoint + new Vector(
            direction.X * outsideOffset / length,
            direction.Y * outsideOffset / length);
        return (start, endpoint);
    }

    private static Point IntersectRayWithBounds(Point source, Rect bounds)
    {
        var center = bounds.Center;
        var deltaX = source.X - center.X;
        var deltaY = source.Y - center.Y;
        var halfWidth = bounds.Width / 2d;
        var halfHeight = bounds.Height / 2d;
        var scaleX = Math.Abs(deltaX) > 1e-9 ? halfWidth / Math.Abs(deltaX) : double.PositiveInfinity;
        var scaleY = Math.Abs(deltaY) > 1e-9 ? halfHeight / Math.Abs(deltaY) : double.PositiveInfinity;
        var scale = Math.Min(scaleX, scaleY);

        if (!double.IsFinite(scale))
        {
            return new Point(center.X, bounds.Top);
        }

        return new Point(
            center.X + (deltaX * scale),
            center.Y + (deltaY * scale));
    }

    private static Rect CenteredRect(Point center, Size size) =>
        new(
            center.X - (size.Width / 2d),
            center.Y - (size.Height / 2d),
            size.Width,
            size.Height);

    private static Rect Union(Rect first, Rect second)
    {
        var left = Math.Min(first.Left, second.Left);
        var top = Math.Min(first.Top, second.Top);
        var right = Math.Max(first.Right, second.Right);
        var bottom = Math.Max(first.Bottom, second.Bottom);
        return new Rect(left, top, right - left, bottom - top);
    }

    private static Rect Inflate(Rect bounds, double padding) =>
        new(
            bounds.X - padding,
            bounds.Y - padding,
            bounds.Width + (padding * 2d),
            bounds.Height + (padding * 2d));

    private static Rect Translate(Rect bounds, Vector offset) =>
        new(bounds.X + offset.X, bounds.Y + offset.Y, bounds.Width, bounds.Height);

    private static bool Contains(Rect outer, Rect inner) =>
        inner.Left >= outer.Left - 1e-9 &&
        inner.Top >= outer.Top - 1e-9 &&
        inner.Right <= outer.Right + 1e-9 &&
        inner.Bottom <= outer.Bottom + 1e-9;

    private static bool ContainsInclusive(Rect bounds, Point point) =>
        point.X >= bounds.Left &&
        point.X <= bounds.Right &&
        point.Y >= bounds.Top &&
        point.Y <= bounds.Bottom;

    private static bool IsInsidePlanetAnnotationCircle(
        ChartPlanetAnnotationLayout layout,
        ChartViewport viewport,
        ChartRadialLanes lanes)
    {
        var maximumRadius =
            (viewport.EffectiveRadius * lanes.ZodiacRing.InnerRadiusRatio) -
            (viewport.VisualMetrics.StructuralStrokeThickness / 2d) -
            1d;
        var maximumRadiusSquared = maximumRadius * maximumRadius;

        return GetCorners(layout.LabelProtectedBounds).All(point =>
        {
            var deltaX = point.X - viewport.Center.X;
            var deltaY = point.Y - viewport.Center.Y;
            return ((deltaX * deltaX) + (deltaY * deltaY)) <= maximumRadiusSquared + 1e-8;
        });
    }

    private static IEnumerable<Point> GetCorners(Rect bounds)
    {
        yield return bounds.TopLeft;
        yield return bounds.TopRight;
        yield return bounds.BottomRight;
        yield return bounds.BottomLeft;
    }

    private static bool Overlaps(Rect first, Rect second) =>
        Math.Min(first.Right, second.Right) - Math.Max(first.Left, second.Left) > 0.5d &&
        Math.Min(first.Bottom, second.Bottom) - Math.Max(first.Top, second.Top) > 0.5d;

    private static double Distance(Point first, Point second)
    {
        var deltaX = first.X - second.X;
        var deltaY = first.Y - second.Y;
        return Math.Sqrt((deltaX * deltaX) + (deltaY * deltaY));
    }

    private static Point ToPoint(ChartViewport viewport, RadialPoint radialPoint) =>
        new(
            viewport.Center.X + (radialPoint.X * viewport.EffectiveRadius),
            viewport.Center.Y + (radialPoint.Y * viewport.EffectiveRadius));

    private static void ValidateTextSize(Size size)
    {
        if (!double.IsFinite(size.Width) ||
            !double.IsFinite(size.Height) ||
            size.Width < 0d ||
            size.Height <= 0d)
        {
            throw new ArgumentException("Measured annotation text size must be finite and non-negative.");
        }
    }
}
