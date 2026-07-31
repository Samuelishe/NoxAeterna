using Avalonia;
using NoxAeterna.Rendering.Charts;

namespace NoxAeterna.Tests.Rendering;

public sealed class ChartLineOcclusionTests
{
    [Fact]
    public void SegmentWithoutIntersectionRemainsUnchanged()
    {
        var source = new Point(0d, 0d);
        var target = new Point(10d, 0d);

        var visible = ChartLineOcclusion.GetVisibleSegments(
            source,
            target,
            [new Rect(2d, 3d, 4d, 2d)]);

        Assert.Equal([new ChartLineSegment(source, target)], visible);
    }

    [Fact]
    public void SegmentThroughOneRectangleSplitsIntoTwoVisibleSegments()
    {
        var visible = ChartLineOcclusion.GetVisibleSegments(
            new Point(-10d, 0d),
            new Point(10d, 0d),
            [new Rect(-2d, -2d, 4d, 4d)]);

        Assert.Equal(
            [
                new ChartLineSegment(new Point(-10d, 0d), new Point(-2d, 0d)),
                new ChartLineSegment(new Point(2d, 0d), new Point(10d, 0d))
            ],
            visible);
    }

    [Fact]
    public void SegmentEntirelyInsideRectangleDisappears()
    {
        var visible = ChartLineOcclusion.GetVisibleSegments(
            new Point(-1d, 0d),
            new Point(1d, 0d),
            [new Rect(-2d, -2d, 4d, 4d)]);

        Assert.Empty(visible);
    }

    [Fact]
    public void BoundaryPointTouchLeavesTheFiniteSegmentUnchanged()
    {
        var source = new Point(-5d, -5d);
        var target = new Point(0d, 0d);
        var visible = ChartLineOcclusion.GetVisibleSegments(
            source,
            target,
            [new Rect(0d, 0d, 2d, 2d)]);

        Assert.Equal([new ChartLineSegment(source, target)], visible);
        Assert.All(visible, AssertFiniteNonNegative);
    }

    [Fact]
    public void OverlappingRectanglesMergeIntoOneOccludedInterval()
    {
        var visible = ChartLineOcclusion.GetVisibleSegments(
            new Point(0d, 0d),
            new Point(10d, 0d),
            [
                new Rect(2d, -1d, 4d, 2d),
                new Rect(4d, -1d, 4d, 2d)
            ]);

        Assert.Equal(
            [
                new ChartLineSegment(new Point(0d, 0d), new Point(2d, 0d)),
                new ChartLineSegment(new Point(8d, 0d), new Point(10d, 0d))
            ],
            visible);
    }

    [Fact]
    public void SeparatedRectanglesCreateThreeVisibleSegments()
    {
        var visible = ChartLineOcclusion.GetVisibleSegments(
            new Point(0d, 0d),
            new Point(12d, 0d),
            [
                new Rect(2d, -1d, 2d, 2d),
                new Rect(8d, -1d, 2d, 2d)
            ]);

        Assert.Equal(
            [
                new ChartLineSegment(new Point(0d, 0d), new Point(2d, 0d)),
                new ChartLineSegment(new Point(4d, 0d), new Point(8d, 0d)),
                new ChartLineSegment(new Point(10d, 0d), new Point(12d, 0d))
            ],
            visible);
    }

    [Fact]
    public void RectangleOrderDoesNotAffectVisibleSegments()
    {
        var bounds = new[]
        {
            new Rect(2d, -1d, 2d, 2d),
            new Rect(8d, -1d, 2d, 2d),
            new Rect(3d, -1d, 3d, 2d)
        };

        var first = ChartLineOcclusion.GetVisibleSegments(
            new Point(0d, 0d),
            new Point(12d, 0d),
            bounds);
        var second = ChartLineOcclusion.GetVisibleSegments(
            new Point(0d, 0d),
            new Point(12d, 0d),
            bounds.Reverse());

        Assert.Equal(first, second);
    }

    [Theory]
    [MemberData(nameof(VerticalHorizontalAndDiagonalCases))]
    public void VerticalHorizontalAndDiagonalSegmentsAreOccludedDeterministically(
        Point source,
        Point target,
        Rect bounds)
    {
        var first = ChartLineOcclusion.GetVisibleSegments(source, target, [bounds], 0.5d);
        var second = ChartLineOcclusion.GetVisibleSegments(source, target, [bounds], 0.5d);

        Assert.Equal(first, second);
        Assert.Equal(2, first.Count);
        Assert.All(first, AssertFiniteNonNegative);
    }

    [Fact]
    public void ZeroLengthSegmentIsVisibleOutsideAndHiddenInside()
    {
        var point = new Point(1d, 1d);
        var bounds = new Rect(0d, 0d, 2d, 2d);

        Assert.Empty(ChartLineOcclusion.GetVisibleSegments(point, point, [bounds]));
        Assert.Equal(
            [new ChartLineSegment(point, point)],
            ChartLineOcclusion.GetVisibleSegments(
                point,
                point,
                [new Rect(5d, 5d, 2d, 2d)]));
    }

    public static TheoryData<Point, Point, Rect> VerticalHorizontalAndDiagonalCases =>
        new()
        {
            {
                new Point(0d, -10d),
                new Point(0d, 10d),
                new Rect(-2d, -2d, 4d, 4d)
            },
            {
                new Point(-10d, 0d),
                new Point(10d, 0d),
                new Rect(-2d, -2d, 4d, 4d)
            },
            {
                new Point(-10d, -10d),
                new Point(10d, 10d),
                new Rect(-2d, -2d, 4d, 4d)
            }
        };

    private static void AssertFiniteNonNegative(ChartLineSegment segment)
    {
        Assert.True(double.IsFinite(segment.Source.X));
        Assert.True(double.IsFinite(segment.Source.Y));
        Assert.True(double.IsFinite(segment.Target.X));
        Assert.True(double.IsFinite(segment.Target.Y));
        var delta = segment.Target - segment.Source;
        Assert.True((delta.X * delta.X) + (delta.Y * delta.Y) >= 0d);
    }
}
