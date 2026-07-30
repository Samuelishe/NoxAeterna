using NoxAeterna.App.Astrology;

namespace NoxAeterna.Tests.App;

public sealed class WidthDrivenSquareTests
{
    [Fact]
    public void SquareSide_GrowsContinuouslyWithAvailableWidth()
    {
        var standard = WidthDrivenSquare.CalculateSide(520d);
        var wide = WidthDrivenSquare.CalculateSide(820d);

        Assert.Equal(520d, standard);
        Assert.Equal(820d, wide);
        Assert.True(wide > standard);
    }

    [Fact]
    public void SquareSide_IsDeterministicForSameWidth()
    {
        Assert.Equal(
            WidthDrivenSquare.CalculateSide(640d),
            WidthDrivenSquare.CalculateSide(640d));
    }

    [Theory]
    [InlineData(0d, 0d, 0d)]
    [InlineData(double.PositiveInfinity, 480d, 480d)]
    [InlineData(double.PositiveInfinity, 0d, 0d)]
    [InlineData(1400d, 0d, WidthDrivenSquare.MaximumSide)]
    public void SquareSide_HandlesBoundaryConstraints(
        double availableWidth,
        double fallbackWidth,
        double expected)
    {
        Assert.Equal(expected, WidthDrivenSquare.CalculateSide(availableWidth, fallbackWidth));
    }
}
