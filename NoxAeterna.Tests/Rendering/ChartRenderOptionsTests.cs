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
        Assert.Equal(3.4d, huge.OuterRingStrokeThickness);
        Assert.Equal(2.2d, huge.StructuralStrokeThickness);
        Assert.Equal(1.8d, huge.HouseCuspStrokeThickness);
        Assert.Equal(3.2d, huge.AngleAxisStrokeThickness);
    }

    [Theory]
    [InlineData(140d)]
    [InlineData(300d)]
    [InlineData(520d)]
    public void ResponsiveMetrics_KeepPrimaryLinesReadableAtRepresentativeRadii(double radius)
    {
        var metrics = ChartVisualMetrics.Calculate(radius, new ChartRenderOptions());

        Assert.True(metrics.OuterRingStrokeThickness >= 2.4d);
        Assert.True(metrics.StructuralStrokeThickness >= 1.5d);
        Assert.True(metrics.HouseCuspStrokeThickness >= 1.2d);
        Assert.True(metrics.AngleAxisStrokeThickness >= 2.2d);
        Assert.True(metrics.PlanetGlyphSize <= 36d);
        Assert.True(metrics.ZodiacGlyphSize <= 46d);
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
