using Avalonia;
using NoxAeterna.App.Samples;
using NoxAeterna.Domain.Astrology;
using NoxAeterna.Geometry.Charts;
using NoxAeterna.Rendering.Charts;

namespace NoxAeterna.Tests.Rendering;

public sealed class ChartPlanetAnnotationLayoutTests
{
    private static readonly Func<string, double, Size> DeterministicTextMeasure =
        static (text, fontSize) => new Size(text.Length * fontSize * 0.56d, fontSize * 1.18d);

    [Theory]
    [InlineData(480d)]
    [InlineData(620d)]
    [InlineData(900d)]
    public void PragueAnnotationEnvelopesAreFiniteProtectedDeterministicAndInsideSafeBounds(double side)
    {
        var scene = CreatePragueScene();
        var viewport = CreateViewport(scene, side);

        var first = ChartPlanetAnnotationLayoutBuilder.Build(scene, viewport, DeterministicTextMeasure);
        var second = ChartPlanetAnnotationLayoutBuilder.Build(scene, viewport, DeterministicTextMeasure);

        Assert.Equal(first, second);
        Assert.Equal(10, first.Count);
        Assert.All(
            first,
            layout =>
            {
                AssertFinite(layout.GlyphBounds);
                AssertFinite(layout.LabelBounds);
                AssertFinite(layout.GlyphProtectedBounds);
                AssertFinite(layout.LabelProtectedBounds);
                AssertFinite(layout.VisualBounds);
                AssertFinite(layout.ProtectedBounds);
                AssertContains(layout.VisualBounds, layout.GlyphBounds);
                AssertContains(layout.VisualBounds, layout.LabelBounds);
                AssertContains(layout.GlyphProtectedBounds, layout.GlyphBounds);
                AssertContains(layout.LabelProtectedBounds, layout.LabelBounds);
                AssertContains(layout.ProtectedBounds, layout.VisualBounds);
                Assert.True(layout.ProtectedBounds.Width > layout.VisualBounds.Width);
                Assert.True(layout.ProtectedBounds.Height > layout.VisualBounds.Height);
                AssertContains(viewport.SafeDrawingBounds, layout.ProtectedBounds);
                AssertInsidePlanetCircle(layout.LabelProtectedBounds, viewport, scene.Layout.RadialLanes);
            });

        AssertNoOverlaps(first);
    }

    [Fact]
    public void ConnectorsAreConditionalAndTerminateOnProtectedEnvelopeBoundary()
    {
        var scene = CreatePragueScene();
        var viewport = CreateViewport(scene, 760d);
        var layouts = ChartPlanetAnnotationLayoutBuilder.Build(scene, viewport, DeterministicTextMeasure);

        Assert.Contains(layouts, static layout => !layout.HasDisplacement && layout.ConnectorEndpoint is null);
        var displaced = layouts.Where(static layout => layout.HasDisplacement).ToArray();
        Assert.NotEmpty(displaced);

        Assert.All(
            displaced,
            layout =>
            {
                var endpoint = Assert.IsType<Point>(layout.ConnectorEndpoint);
                Assert.True(IsOnBoundary(endpoint, layout.ProtectedBounds));
                Assert.NotEqual(layout.FinalAnchor, endpoint);
                Assert.False(IsInside(layout.GlyphBounds, endpoint));
                Assert.False(SegmentIntersectsInterior(
                    layout.ConnectorStart,
                    endpoint,
                    layout.LabelBounds));
                Assert.True(double.IsFinite(endpoint.X));
                Assert.True(double.IsFinite(endpoint.Y));
            });
    }

    [Fact]
    public void PragueLowerAndUpperClustersRemainSeparatedAndMidheavenAxisIsKnockedOutAtSaturn()
    {
        var scene = CreatePragueScene();
        var viewport = CreateViewport(scene, 760d);
        var first = ChartPlanetAnnotationLayoutBuilder.Build(scene, viewport, DeterministicTextMeasure);
        var second = ChartPlanetAnnotationLayoutBuilder.Build(scene, viewport, DeterministicTextMeasure);

        Assert.Equal(first, second);
        AssertNoOverlaps(Get(first, CelestialBody.Uranus, CelestialBody.Neptune, CelestialBody.Saturn));
        AssertNoOverlaps(Get(
            first,
            CelestialBody.Sun,
            CelestialBody.Jupiter,
            CelestialBody.Venus,
            CelestialBody.Mercury));

        var saturn = first.Single(layout => layout.Annotation.Body == CelestialBody.Saturn);
        var midheavenAxis = scene.AngleAxes.Single(
            axis => axis.AxisType == ChartAngleAxisType.MidheavenImumCoeli);
        var axisStart = ToPoint(viewport, midheavenAxis.PrimaryPoint);
        var axisEnd = ToPoint(viewport, midheavenAxis.OppositePoint);
        Assert.True(SegmentIntersectsInterior(axisStart, axisEnd, saturn.ProtectedBounds));
    }

    [Theory]
    [InlineData(480d)]
    [InlineData(620d)]
    [InlineData(900d)]
    public void PrincipalAngleLabelsStayInsideSafeBoundsAndClearTheRimAndZodiacGlyphs(double side)
    {
        var scene = CreatePragueScene();
        var viewport = CreateViewport(scene, side);
        var layouts = ChartAngleLabelLayoutBuilder.Build(scene, viewport, DeterministicTextMeasure);
        var rimRadius = viewport.EffectiveRadius * scene.Layout.RadialLanes.OuterBoundaryRadiusRatio;

        Assert.Equal(new[] { "ASC", "DSC", "MC", "IC" }, layouts.Select(layout => layout.Label.Text));
        Assert.All(
            layouts,
            layout =>
            {
                AssertContains(viewport.SafeDrawingBounds, layout.Bounds);
                var direction = Normalize(layout.Anchor - viewport.Center);
                var nearestProjection = Corners(layout.Bounds)
                    .Min(point => Dot(point - viewport.Center, direction));
                Assert.True(nearestProjection >= rimRadius + 2.9d);

                Assert.DoesNotContain(
                    scene.ZodiacGlyphs,
                    glyph => Overlaps(
                        layout.Bounds,
                        GetGlyphBounds(glyph, viewport)));
            });
    }

    private static ChartRenderScene CreatePragueScene() =>
        DevelopmentSampleChartBuildResultFactory.Create().RenderScene;

    private static ChartViewport CreateViewport(ChartRenderScene scene, double side)
    {
        Assert.True(
            ChartViewport.TryCreate(
                new Rect(0d, 0d, side, side),
                scene.Layout.RadialLanes,
                new ChartRenderOptions(ChartRenderPalette.Light),
                out var viewport));
        return viewport;
    }

    private static IReadOnlyList<ChartPlanetAnnotationLayout> Get(
        IEnumerable<ChartPlanetAnnotationLayout> layouts,
        params CelestialBody[] bodies) =>
        layouts.Where(layout => bodies.Contains(layout.Annotation.Body)).ToArray();

    private static void AssertNoOverlaps(IReadOnlyList<ChartPlanetAnnotationLayout> layouts)
    {
        for (var first = 0; first < layouts.Count; first++)
        {
            for (var second = first + 1; second < layouts.Count; second++)
            {
                Assert.False(
                    Overlaps(layouts[first].ProtectedBounds, layouts[second].ProtectedBounds),
                    $"{layouts[first].Annotation.Body} overlaps {layouts[second].Annotation.Body}.");
            }
        }
    }

    private static Rect GetGlyphBounds(ChartGlyphPlacement glyph, ChartViewport viewport)
    {
        var anchor = ToPoint(viewport, glyph.AnchorPoint);
        var size = viewport.VisualMetrics.ZodiacGlyphSize;
        var scale = size / Math.Max(glyph.Glyph.UnitBounds.Width, glyph.Glyph.UnitBounds.Height);
        var width = (glyph.Glyph.UnitBounds.Width * scale) + viewport.VisualMetrics.GlyphStrokeThickness;
        var height = (glyph.Glyph.UnitBounds.Height * scale) + viewport.VisualMetrics.GlyphStrokeThickness;
        return new Rect(anchor.X - (width / 2d), anchor.Y - (height / 2d), width, height);
    }

    private static Point ToPoint(ChartViewport viewport, RadialPoint radialPoint) =>
        new(
            viewport.Center.X + (radialPoint.X * viewport.EffectiveRadius),
            viewport.Center.Y + (radialPoint.Y * viewport.EffectiveRadius));

    private static bool IsOnBoundary(Point point, Rect bounds)
    {
        const double tolerance = 1e-7;
        var onVertical =
            (Math.Abs(point.X - bounds.Left) <= tolerance ||
             Math.Abs(point.X - bounds.Right) <= tolerance) &&
            point.Y >= bounds.Top - tolerance &&
            point.Y <= bounds.Bottom + tolerance;
        var onHorizontal =
            (Math.Abs(point.Y - bounds.Top) <= tolerance ||
             Math.Abs(point.Y - bounds.Bottom) <= tolerance) &&
            point.X >= bounds.Left - tolerance &&
            point.X <= bounds.Right + tolerance;
        return onVertical || onHorizontal;
    }

    private static bool SegmentIntersectsInterior(Point start, Point end, Rect bounds)
    {
        const int samples = 200;
        for (var index = 1; index < samples; index++)
        {
            var ratio = index / (double)samples;
            var point = new Point(
                start.X + ((end.X - start.X) * ratio),
                start.Y + ((end.Y - start.Y) * ratio));
            if (IsInside(bounds, point))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsInside(Rect bounds, Point point) =>
        point.X > bounds.Left + 1e-7 &&
        point.X < bounds.Right - 1e-7 &&
        point.Y > bounds.Top + 1e-7 &&
        point.Y < bounds.Bottom - 1e-7;

    private static bool Overlaps(Rect first, Rect second) =>
        Math.Min(first.Right, second.Right) - Math.Max(first.Left, second.Left) > 0.5d &&
        Math.Min(first.Bottom, second.Bottom) - Math.Max(first.Top, second.Top) > 0.5d;

    private static void AssertContains(Rect outer, Rect inner)
    {
        Assert.True(inner.Left >= outer.Left - 1e-8);
        Assert.True(inner.Top >= outer.Top - 1e-8);
        Assert.True(inner.Right <= outer.Right + 1e-8);
        Assert.True(inner.Bottom <= outer.Bottom + 1e-8);
    }

    private static void AssertFinite(Rect bounds)
    {
        Assert.True(double.IsFinite(bounds.X));
        Assert.True(double.IsFinite(bounds.Y));
        Assert.True(double.IsFinite(bounds.Width));
        Assert.True(double.IsFinite(bounds.Height));
        Assert.True(bounds.Width > 0d);
        Assert.True(bounds.Height > 0d);
    }

    private static void AssertInsidePlanetCircle(
        Rect bounds,
        ChartViewport viewport,
        ChartRadialLanes lanes)
    {
        var maximumRadius =
            (viewport.EffectiveRadius * lanes.ZodiacRing.InnerRadiusRatio) -
            (viewport.VisualMetrics.StructuralStrokeThickness / 2d) -
            1d;
        Assert.All(
            Corners(bounds),
            point =>
            {
                var distance = point - viewport.Center;
                Assert.True(
                    ((distance.X * distance.X) + (distance.Y * distance.Y)) <=
                    (maximumRadius * maximumRadius) + 1e-8);
            });
    }

    private static IEnumerable<Point> Corners(Rect bounds)
    {
        yield return bounds.TopLeft;
        yield return bounds.TopRight;
        yield return bounds.BottomRight;
        yield return bounds.BottomLeft;
    }

    private static Vector Normalize(Vector vector)
    {
        var length = Math.Sqrt((vector.X * vector.X) + (vector.Y * vector.Y));
        return new Vector(vector.X / length, vector.Y / length);
    }

    private static double Dot(Vector first, Vector second) =>
        (first.X * second.X) + (first.Y * second.Y);
}
