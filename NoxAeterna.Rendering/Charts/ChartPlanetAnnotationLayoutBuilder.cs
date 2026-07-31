using Avalonia;
using NoxAeterna.Domain.Astrology;
using NoxAeterna.Geometry.Charts;

namespace NoxAeterna.Rendering.Charts;

/// <summary>
/// Builds deterministic viewport-specific source, glyph, and label visuals for planet annotations.
/// </summary>
public static class ChartPlanetAnnotationLayoutBuilder
{
    /// <summary>
    /// Gets the largest longitude adjustment allowed for a glyph annotation.
    /// </summary>
    public const double MaximumGlyphAngularAdjustmentDegrees = 8d;

    private const double AngularAdjustmentStepDegrees = 1d;
    private const double DisplacementThreshold = 0.01d;

    /// <summary>
    /// Builds exact source markers, bounded glyph placements, and independent label placements.
    /// </summary>
    public static IReadOnlyList<ChartPlanetAnnotationLayout> Build(
        ChartRenderScene scene,
        ChartViewport viewport,
        Func<string, double, Size> measureText)
    {
        ArgumentNullException.ThrowIfNull(scene);
        ArgumentNullException.ThrowIfNull(measureText);

        var slotsByBody = scene.PlanetGlyphSlots.ToDictionary(static slot => slot.Body);
        var glyphs = new List<GlyphLayout>(scene.PlanetAnnotations.Count);

        foreach (var annotation in scene.PlanetAnnotations.OrderBy(static item => item.Body))
        {
            var slot = slotsByBody[annotation.Body];
            glyphs.Add(FindGlyphLayout(annotation, slot, scene, viewport, glyphs));
        }

        var labels = new List<LabelLayout>(glyphs.Count);
        foreach (var glyph in glyphs)
        {
            var labelText = GetLabelText(glyph.Annotation);
            var labelSize = measureText(labelText, viewport.VisualMetrics.PlanetAnnotationFontSize);
            ValidateTextSize(labelSize);
            labels.Add(FindLabelLayout(glyph, labelSize, viewport, scene.Layout.RadialLanes, glyphs, labels));
        }

        return glyphs
            .Zip(labels, (glyph, label) => CreateLayout(glyph, label, viewport, scene.Layout.RadialLanes))
            .ToArray();
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

    private static GlyphLayout FindGlyphLayout(
        ChartPlanetAnnotationPlacement annotation,
        PlanetGlyphSlot slot,
        ChartRenderScene scene,
        ChartViewport viewport,
        IReadOnlyCollection<GlyphLayout> accepted)
    {
        var candidates = EnumerateGlyphCandidates(slot, scene.Layout)
            .Select((candidate, index) => CreateGlyphCandidate(
                annotation,
                slot,
                candidate,
                index,
                viewport,
                scene.Layout.RadialLanes))
            .ToArray();

        foreach (var candidate in candidates)
        {
            if (candidate.IsSafe && accepted.All(existing => !Overlaps(existing.CollisionBounds, candidate.CollisionBounds)))
            {
                return candidate.ToLayout(isCrowded: false, overlapArea: 0d);
            }
        }

        var fallback = candidates
            .Select(candidate => new
            {
                Candidate = candidate,
                OverlapArea = accepted.Sum(existing => IntersectionArea(existing.CollisionBounds, candidate.CollisionBounds))
            })
            .OrderByDescending(static item => item.Candidate.IsSafe)
            .ThenBy(static item => item.Candidate.AngularDisplacement)
            .ThenBy(static item => item.OverlapArea)
            .ThenBy(static item => item.Candidate.RadialRank)
            .ThenBy(static item => item.Candidate.Sequence)
            .First();

        return fallback.Candidate.ToLayout(
            isCrowded: fallback.OverlapArea > 0.5d || !fallback.Candidate.IsSafe,
            overlapArea: fallback.OverlapArea);
    }

    private static IEnumerable<GlyphCandidateDefinition> EnumerateGlyphCandidates(
        PlanetGlyphSlot slot,
        CircularChartLayout layout)
    {
        var radii = BuildRadialCandidates(slot, layout.RadialLanes);

        foreach (var radius in radii)
        {
            yield return CreateDefinition(slot, layout, slot.Longitude, radius.Radius, 0d, radius.Rank);
        }

        for (var adjustment = AngularAdjustmentStepDegrees;
             adjustment <= MaximumGlyphAngularAdjustmentDegrees;
             adjustment += AngularAdjustmentStepDegrees)
        {
            foreach (var direction in new[] { -1d, 1d })
            {
                var longitude = new ZodiacLongitude(slot.Longitude.Degrees + (adjustment * direction));
                if (longitude.Sign != slot.Longitude.Sign ||
                    !RemainsInSourceHouse(longitude, slot, layout.HouseCusps))
                {
                    continue;
                }

                foreach (var radius in radii)
                {
                    yield return CreateDefinition(
                        slot,
                        layout,
                        longitude,
                        radius.Radius,
                        adjustment,
                        radius.Rank);
                }
            }
        }
    }

    private static GlyphCandidateDefinition CreateDefinition(
        PlanetGlyphSlot slot,
        CircularChartLayout layout,
        ZodiacLongitude longitude,
        double radius,
        double angularDisplacement,
        int radialRank) =>
        new(
            longitude,
            new RadialPoint(layout.Orientation.Transform(longitude), radius),
            angularDisplacement,
            radialRank);

    private static IReadOnlyList<(double Radius, int Rank)> BuildRadialCandidates(
        PlanetGlyphSlot slot,
        ChartRadialLanes lanes)
    {
        var ordered = new List<double> { slot.PreferredGlyphAnchor.RadiusRatio };
        ordered.AddRange(lanes.PlanetSubLaneRadiusRatios);

        var sorted = lanes.PlanetSubLaneRadiusRatios.OrderByDescending(static radius => radius).ToArray();
        for (var index = 0; index < sorted.Length - 1; index++)
        {
            ordered.Add((sorted[index] + sorted[index + 1]) / 2d);
        }

        ordered.Add(lanes.PlanetGlyphLane.OuterRadiusRatio - 0.012d);
        ordered.Add(lanes.PlanetGlyphLane.InnerRadiusRatio + 0.012d);

        return ordered
            .Where(lanes.PlanetGlyphLane.Contains)
            .DistinctBy(static radius => Math.Round(radius, 9))
            .Select((radius, index) => (radius, index))
            .ToArray();
    }

    private static bool RemainsInSourceHouse(
        ZodiacLongitude candidate,
        PlanetGlyphSlot slot,
        IReadOnlyList<HouseCuspGeometry> cusps)
    {
        if (slot.SourceHouseNumber is null)
        {
            return true;
        }

        return ChartHouseMembership.Find(candidate, cusps)?.Value == slot.SourceHouseNumber.Value;
    }

    private static GlyphCandidate CreateGlyphCandidate(
        ChartPlanetAnnotationPlacement annotation,
        PlanetGlyphSlot slot,
        GlyphCandidateDefinition definition,
        int sequence,
        ChartViewport viewport,
        ChartRadialLanes lanes)
    {
        var anchor = ToPoint(viewport, definition.RadialPoint);
        var metrics = viewport.VisualMetrics;
        var glyphScale = metrics.PlanetGlyphSize /
                         Math.Max(annotation.Glyph.UnitBounds.Width, annotation.Glyph.UnitBounds.Height);
        var glyphSize = new Size(
            (annotation.Glyph.UnitBounds.Width * glyphScale) + metrics.GlyphStrokeThickness,
            (annotation.Glyph.UnitBounds.Height * glyphScale) + metrics.GlyphStrokeThickness);
        var bounds = CenteredRect(anchor, glyphSize);
        var padding = AnnotationPadding(viewport);
        var protectedBounds = Inflate(bounds, padding);
        var collisionBounds = Inflate(bounds, 0.5d);
        var maximumRadius =
            (viewport.EffectiveRadius * lanes.ZodiacRing.InnerRadiusRatio) -
            (viewport.VisualMetrics.StructuralStrokeThickness / 2d) - 1d;
        var isSafe = Contains(viewport.SafeDrawingBounds, protectedBounds) &&
                     IsInsideCircle(protectedBounds, viewport, maximumRadius);

        return new GlyphCandidate(
            annotation,
            slot,
            definition.Longitude,
            definition.RadialPoint,
            anchor,
            bounds,
            protectedBounds,
            collisionBounds,
            definition.AngularDisplacement,
            definition.RadialRank,
            sequence,
            isSafe);
    }

    private static LabelLayout FindLabelLayout(
        GlyphLayout glyph,
        Size labelSize,
        ChartViewport viewport,
        ChartRadialLanes lanes,
        IReadOnlyList<GlyphLayout> glyphs,
        IReadOnlyCollection<LabelLayout> accepted)
    {
        var candidates = EnumerateLabelAnchors(glyph, labelSize, viewport)
            .Select((anchor, index) => CreateLabelCandidate(
                glyph,
                anchor,
                index,
                labelSize,
                viewport,
                lanes))
            .ToArray();

        foreach (var candidate in candidates)
        {
            if (candidate.IsSafe && !candidate.IntersectsSourceLeader &&
                glyphs.All(other => other.Annotation.Body == glyph.Annotation.Body ||
                                    !Overlaps(other.ProtectedBounds, candidate.ProtectedBounds)) &&
                !Overlaps(glyph.ProtectedBounds, candidate.ProtectedBounds) &&
                accepted.All(existing => !Overlaps(existing.ProtectedBounds, candidate.ProtectedBounds)))
            {
                return candidate.ToLayout();
            }
        }

        return candidates
            .Select(candidate => new
            {
                Candidate = candidate,
                Overlap = glyphs.Sum(other => IntersectionArea(other.ProtectedBounds, candidate.ProtectedBounds)) +
                          accepted.Sum(other => IntersectionArea(other.ProtectedBounds, candidate.ProtectedBounds))
            })
            .OrderByDescending(static item => item.Candidate.IsSafe)
            .ThenBy(static item => item.Candidate.IntersectsSourceLeader)
            .ThenBy(static item => item.Overlap)
            .ThenBy(static item => item.Candidate.Sequence)
            .First()
            .Candidate
            .ToLayout();
    }

    private static IEnumerable<Point> EnumerateLabelAnchors(
        GlyphLayout glyph,
        Size labelSize,
        ChartViewport viewport)
    {
        var gap = Math.Clamp(viewport.EffectiveRadius * 0.010d, 3d, 5d);
        var vertical = (glyph.Bounds.Height / 2d) + (labelSize.Height / 2d) + gap;
        var horizontal = (glyph.Bounds.Width / 2d) + (labelSize.Width / 2d) + gap;
        var radial = UnitVector(viewport.Center, glyph.Anchor);
        var tangent = new Vector(-radial.Y, radial.X);
        var radialDistance = Math.Max(vertical, horizontal * 0.72d);
        var tangentDistance = Math.Max(horizontal, vertical * 0.72d);

        yield return glyph.Anchor + new Vector(0d, vertical);
        yield return glyph.Anchor + new Vector(0d, -vertical);
        yield return glyph.Anchor - (radial * radialDistance);
        yield return glyph.Anchor + (radial * radialDistance);
        yield return glyph.Anchor + (tangent * tangentDistance);
        yield return glyph.Anchor - (tangent * tangentDistance);
        yield return glyph.Anchor - (radial * radialDistance) + (tangent * (tangentDistance * 0.55d));
        yield return glyph.Anchor - (radial * radialDistance) - (tangent * (tangentDistance * 0.55d));
        yield return glyph.Anchor + (radial * radialDistance) + (tangent * (tangentDistance * 0.55d));
        yield return glyph.Anchor + (radial * radialDistance) - (tangent * (tangentDistance * 0.55d));
    }

    private static LabelCandidate CreateLabelCandidate(
        GlyphLayout glyph,
        Point anchor,
        int sequence,
        Size labelSize,
        ChartViewport viewport,
        ChartRadialLanes lanes)
    {
        var bounds = CenteredRect(anchor, labelSize);
        var protectedBounds = Inflate(bounds, AnnotationPadding(viewport));
        var maximumRadius =
            (viewport.EffectiveRadius * lanes.ZodiacRing.InnerRadiusRatio) -
            (viewport.VisualMetrics.StructuralStrokeThickness / 2d) - 1d;
        var isSafe = Contains(viewport.SafeDrawingBounds, protectedBounds) &&
                     IsInsideCircle(protectedBounds, viewport, maximumRadius);
        var sourceRadius = Math.Min(
            lanes.ZodiacRing.InnerRadiusRatio - 0.014d,
            lanes.PlanetGlyphLane.OuterRadiusRatio + 0.040d);
        var sourceAnchor = ToPoint(viewport, new RadialPoint(glyph.Slot.SourceAngle, sourceRadius));
        var leaderStart = MoveTowards(
            sourceAnchor,
            glyph.Anchor,
            viewport.VisualMetrics.PlanetSourceMarkerRadius + 1d);
        var leaderEndpoint = IntersectRayWithBounds(sourceAnchor, glyph.Bounds);
        var intersectsSourceLeader = SegmentIntersectsBounds(
            leaderStart,
            leaderEndpoint,
            protectedBounds);
        return new LabelCandidate(
            anchor,
            bounds,
            protectedBounds,
            sequence,
            isSafe,
            intersectsSourceLeader);
    }

    private static ChartPlanetAnnotationLayout CreateLayout(
        GlyphLayout glyph,
        LabelLayout label,
        ChartViewport viewport,
        ChartRadialLanes lanes)
    {
        var sourceRadius = Math.Min(
            lanes.ZodiacRing.InnerRadiusRatio - 0.014d,
            lanes.PlanetGlyphLane.OuterRadiusRatio + 0.040d);
        var sourceRadialPoint = new RadialPoint(glyph.Slot.SourceAngle, sourceRadius);
        var sourceAnchor = ToPoint(viewport, sourceRadialPoint);
        var markerRadius = viewport.VisualMetrics.PlanetSourceMarkerRadius;
        var sourceMarkerBounds = CenteredRect(sourceAnchor, new Size(markerRadius * 2d, markerRadius * 2d));
        var sourceLeaderStart = MoveTowards(sourceAnchor, glyph.Anchor, markerRadius + 1d);
        var sourceLeaderEndpoint = IntersectRayWithBounds(sourceAnchor, glyph.Bounds);
        var labelGap = DistanceBetween(glyph.Bounds, label.Bounds);
        var hasLabelDisplacement = label.Sequence > 0;
        var labelLeaderThreshold = Math.Clamp(viewport.EffectiveRadius * 0.010d, 3d, 5d);
        var labelOffset = label.Anchor - glyph.Anchor;
        var hasLabelLeader = hasLabelDisplacement &&
                             labelGap >= labelLeaderThreshold - 1e-6 &&
                             Math.Abs(labelOffset.Y) >= Math.Abs(labelOffset.X) * 0.5d;
        var labelLeaderStart = hasLabelLeader
            ? IntersectRayWithBounds(label.Anchor, glyph.Bounds)
            : (Point?)null;
        var labelLeaderEndpoint = hasLabelLeader
            ? IntersectRayWithBounds(glyph.Anchor, label.Bounds)
            : (Point?)null;
        var visualBounds = Union(Union(sourceMarkerBounds, glyph.Bounds), label.Bounds);
        var protectedBounds = Union(Union(Inflate(sourceMarkerBounds, 1d), glyph.ProtectedBounds), label.ProtectedBounds);
        var hasGlyphDisplacement =
            CircularDelta(glyph.Slot.SourceAngle.Degrees, glyph.RadialPoint.Angle.Degrees) > DisplacementThreshold ||
            Math.Abs(glyph.Slot.PreferredGlyphAnchor.RadiusRatio - glyph.RadialPoint.RadiusRatio) > 1e-9;

        return new ChartPlanetAnnotationLayout(
            glyph.Annotation,
            sourceRadialPoint,
            sourceAnchor,
            sourceMarkerBounds,
            glyph.Longitude,
            glyph.RadialPoint,
            glyph.Anchor,
            glyph.Bounds,
            glyph.ProtectedBounds,
            label.Anchor,
            label.Bounds,
            label.ProtectedBounds,
            visualBounds,
            protectedBounds,
            sourceLeaderStart,
            sourceLeaderEndpoint,
            labelLeaderStart,
            labelLeaderEndpoint,
            hasGlyphDisplacement,
            hasLabelDisplacement,
            glyph.IsCrowded,
            glyph.OverlapArea,
            glyph.Slot.Longitude.Sign,
            glyph.Slot.SourceHouseNumber);
    }

    private static Point IntersectRayWithBounds(Point source, Rect bounds)
    {
        var center = bounds.Center;
        var deltaX = source.X - center.X;
        var deltaY = source.Y - center.Y;
        var scaleX = Math.Abs(deltaX) > 1e-9 ? (bounds.Width / 2d) / Math.Abs(deltaX) : double.PositiveInfinity;
        var scaleY = Math.Abs(deltaY) > 1e-9 ? (bounds.Height / 2d) / Math.Abs(deltaY) : double.PositiveInfinity;
        var scale = Math.Min(scaleX, scaleY);
        return double.IsFinite(scale)
            ? new Point(center.X + (deltaX * scale), center.Y + (deltaY * scale))
            : new Point(center.X, bounds.Top);
    }

    private static Point MoveTowards(Point source, Point target, double distance)
    {
        var direction = target - source;
        var length = Math.Sqrt((direction.X * direction.X) + (direction.Y * direction.Y));
        return length <= 1e-9
            ? source
            : source + new Vector(direction.X * distance / length, direction.Y * distance / length);
    }

    private static Vector UnitVector(Point source, Point target)
    {
        var direction = target - source;
        var length = Math.Sqrt((direction.X * direction.X) + (direction.Y * direction.Y));
        return length <= 1e-9 ? new Vector(0d, -1d) : direction / length;
    }

    private static double AnnotationPadding(ChartViewport viewport) =>
        Math.Clamp(viewport.EffectiveRadius * 0.006d, 1.5d, 2.75d);

    private static Rect CenteredRect(Point center, Size size) =>
        new(center.X - (size.Width / 2d), center.Y - (size.Height / 2d), size.Width, size.Height);

    private static Rect Inflate(Rect bounds, double padding) =>
        new(bounds.X - padding, bounds.Y - padding, bounds.Width + (padding * 2d), bounds.Height + (padding * 2d));

    private static Rect Union(Rect first, Rect second)
    {
        var left = Math.Min(first.Left, second.Left);
        var top = Math.Min(first.Top, second.Top);
        var right = Math.Max(first.Right, second.Right);
        var bottom = Math.Max(first.Bottom, second.Bottom);
        return new Rect(left, top, right - left, bottom - top);
    }

    private static bool Contains(Rect outer, Rect inner) =>
        inner.Left >= outer.Left - 1e-9 && inner.Top >= outer.Top - 1e-9 &&
        inner.Right <= outer.Right + 1e-9 && inner.Bottom <= outer.Bottom + 1e-9;

    private static bool IsInsideCircle(Rect bounds, ChartViewport viewport, double maximumRadius)
    {
        var maximumRadiusSquared = maximumRadius * maximumRadius;
        return new[] { bounds.TopLeft, bounds.TopRight, bounds.BottomRight, bounds.BottomLeft }
            .All(point =>
            {
                var deltaX = point.X - viewport.Center.X;
                var deltaY = point.Y - viewport.Center.Y;
                return ((deltaX * deltaX) + (deltaY * deltaY)) <= maximumRadiusSquared + 1e-8;
            });
    }

    private static bool Overlaps(Rect first, Rect second) => IntersectionArea(first, second) > 0.5d;

    private static double IntersectionArea(Rect first, Rect second) =>
        Math.Max(0d, Math.Min(first.Right, second.Right) - Math.Max(first.Left, second.Left)) *
        Math.Max(0d, Math.Min(first.Bottom, second.Bottom) - Math.Max(first.Top, second.Top));

    private static double DistanceBetween(Rect first, Rect second)
    {
        var deltaX = Math.Max(0d, Math.Max(first.Left - second.Right, second.Left - first.Right));
        var deltaY = Math.Max(0d, Math.Max(first.Top - second.Bottom, second.Top - first.Bottom));
        return Math.Sqrt((deltaX * deltaX) + (deltaY * deltaY));
    }

    private static bool SegmentIntersectsBounds(Point source, Point target, Rect bounds)
    {
        var originalLength = Distance(source, target);
        var visibleLength = ChartLineOcclusion
            .GetVisibleSegments(source, target, [bounds], 0d)
            .Sum(segment => Distance(segment.Source, segment.Target));
        return visibleLength < originalLength - 0.01d;
    }

    private static double Distance(Point first, Point second)
    {
        var deltaX = first.X - second.X;
        var deltaY = first.Y - second.Y;
        return Math.Sqrt((deltaX * deltaX) + (deltaY * deltaY));
    }

    private static double CircularDelta(double first, double second)
    {
        var delta = Math.Abs(first - second);
        return Math.Min(delta, 360d - delta);
    }

    private static Point ToPoint(ChartViewport viewport, RadialPoint radialPoint) =>
        new(
            viewport.Center.X + (radialPoint.X * viewport.EffectiveRadius),
            viewport.Center.Y + (radialPoint.Y * viewport.EffectiveRadius));

    private static void ValidateTextSize(Size size)
    {
        if (!double.IsFinite(size.Width) || !double.IsFinite(size.Height) || size.Width < 0d || size.Height <= 0d)
        {
            throw new ArgumentException("Measured annotation text size must be finite and non-negative.");
        }
    }

    private readonly record struct GlyphCandidateDefinition(
        ZodiacLongitude Longitude,
        RadialPoint RadialPoint,
        double AngularDisplacement,
        int RadialRank);

    private sealed record GlyphCandidate(
        ChartPlanetAnnotationPlacement Annotation,
        PlanetGlyphSlot Slot,
        ZodiacLongitude Longitude,
        RadialPoint RadialPoint,
        Point Anchor,
        Rect Bounds,
        Rect ProtectedBounds,
        Rect CollisionBounds,
        double AngularDisplacement,
        int RadialRank,
        int Sequence,
        bool IsSafe)
    {
        public GlyphLayout ToLayout(bool isCrowded, double overlapArea) =>
            new(Annotation, Slot, Longitude, RadialPoint, Anchor, Bounds, ProtectedBounds, CollisionBounds, isCrowded, overlapArea);
    }

    private sealed record GlyphLayout(
        ChartPlanetAnnotationPlacement Annotation,
        PlanetGlyphSlot Slot,
        ZodiacLongitude Longitude,
        RadialPoint RadialPoint,
        Point Anchor,
        Rect Bounds,
        Rect ProtectedBounds,
        Rect CollisionBounds,
        bool IsCrowded,
        double OverlapArea);

    private sealed record LabelCandidate(
        Point Anchor,
        Rect Bounds,
        Rect ProtectedBounds,
        int Sequence,
        bool IsSafe,
        bool IntersectsSourceLeader)
    {
        public LabelLayout ToLayout() => new(Anchor, Bounds, ProtectedBounds, Sequence);
    }

    private sealed record LabelLayout(Point Anchor, Rect Bounds, Rect ProtectedBounds, int Sequence);
}
