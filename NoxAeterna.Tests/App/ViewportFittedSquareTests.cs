using NoxAeterna.App.Astrology;

namespace NoxAeterna.Tests.App;

public sealed class ViewportFittedSquareTests
{
    [Fact]
    public void SquareSide_IsConstrainedByBothWidthAndViewportHeight()
    {
        Assert.Equal(620d, ViewportFittedSquare.CalculateSide(620d, 900d, 96d));
        Assert.Equal(704d, ViewportFittedSquare.CalculateSide(1200d, 800d, 96d));
    }

    [Fact]
    public void MaximizedWidthCannotProduceSquareTallerThanChartViewport()
    {
        var side = ViewportFittedSquare.CalculateSide(1600d, 980d, 96d);

        Assert.Equal(884d, side);
        Assert.True(side + 96d <= 980d);
    }

    [Fact]
    public void SquareSide_IsDeterministicAndCappedOnLargeMonitors()
    {
        Assert.Equal(
            ViewportFittedSquare.CalculateSide(1400d, 1400d),
            ViewportFittedSquare.CalculateSide(1400d, 1400d));
        Assert.Equal(
            ViewportFittedSquare.MaximumSide,
            ViewportFittedSquare.CalculateSide(2000d, 2000d));
    }

    [Theory]
    [InlineData(0d, 500d)]
    [InlineData(500d, 0d)]
    [InlineData(double.NaN, 500d)]
    [InlineData(500d, double.PositiveInfinity)]
    [InlineData(double.PositiveInfinity, 500d)]
    public void SquareSide_RejectsInvalidOrUnboundedConstraints(double width, double height)
    {
        Assert.Equal(0d, ViewportFittedSquare.CalculateSide(width, height));
    }

    [Fact]
    public void SquareSide_CanUseFiniteFallbackWidth()
    {
        Assert.Equal(
            480d,
            ViewportFittedSquare.CalculateSide(
                double.PositiveInfinity,
                700d,
                reservedVerticalSpace: 0d,
                fallbackWidth: 480d));
    }
}
