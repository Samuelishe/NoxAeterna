using NoxAeterna.Rendering.Charts;

namespace NoxAeterna.Tests.Rendering;

public sealed class ChartRenderOptionsTests
{
    [Theory]
    [InlineData(-0.01)]
    [InlineData(0.5)]
    [InlineData(1)]
    public void Constructor_RejectsInvalidPaddingRatio(double paddingRatio)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ChartRenderOptions(paddingRatio: paddingRatio));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_RejectsInvalidMarkerRadius(double markerRadius)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ChartRenderOptions(planetMarkerRadius: markerRadius));
    }
}
