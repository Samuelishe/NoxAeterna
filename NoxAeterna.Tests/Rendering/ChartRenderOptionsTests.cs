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

    [Fact]
    public void ResponsiveMetrics_GrowWithRadiusAndStayWithinCaps()
    {
        var options = new ChartRenderOptions();
        var small = ChartVisualMetrics.Calculate(140d, options);
        var large = ChartVisualMetrics.Calculate(520d, options);
        var huge = ChartVisualMetrics.Calculate(5000d, options);

        Assert.True(large.ZodiacGlyphSize > small.ZodiacGlyphSize);
        Assert.True(large.PlanetGlyphSize > small.PlanetGlyphSize);
        Assert.True(large.GlyphStrokeThickness > small.GlyphStrokeThickness);
        Assert.Equal(46d, huge.ZodiacGlyphSize);
        Assert.Equal(36d, huge.PlanetGlyphSize);
        Assert.Equal(2.4d, huge.OuterRingStrokeThickness);
        Assert.Equal(1.5d, huge.StructuralStrokeThickness);
    }

    [Fact]
    public void ResponsiveMetrics_AreDeterministic()
    {
        var options = new ChartRenderOptions();

        Assert.Equal(
            ChartVisualMetrics.Calculate(320d, options),
            ChartVisualMetrics.Calculate(320d, options));
    }
}
