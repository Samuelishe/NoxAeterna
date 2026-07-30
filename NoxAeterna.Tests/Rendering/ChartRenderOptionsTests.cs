using NoxAeterna.Rendering.Charts;

namespace NoxAeterna.Tests.Rendering;

public sealed class ChartRenderOptionsTests
{
    [Theory]
    [InlineData(-0.01)]
    [InlineData(double.NaN)]
    public void Constructor_RejectsInvalidSafetyMargin(double safetyMargin)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ChartRenderOptions(safetyMargin: safetyMargin));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_RejectsInvalidPlanetGlyphSize(double glyphSize)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ChartRenderOptions(planetGlyphSize: glyphSize));
    }
}
