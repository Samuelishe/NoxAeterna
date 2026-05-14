using NoxAeterna.App.Samples;
using NoxAeterna.Domain.Astrology;
using NoxAeterna.Rendering.Charts;

namespace NoxAeterna.Tests.App;

public sealed class DevelopmentSampleFactoriesTests
{
    [Fact]
    public void NatalChartFactory_ProducesDeterministicNatalChart()
    {
        var firstChart = DevelopmentSampleNatalChartFactory.Create();
        var secondChart = DevelopmentSampleNatalChartFactory.Create();

        Assert.Equal(firstChart.BirthMoment, secondChart.BirthMoment);
        Assert.Equal(firstChart.Positions, secondChart.Positions);
        Assert.Equal(firstChart.Aspects, secondChart.Aspects);
        Assert.Equal("development-sample", firstChart.EphemerisSourceVersion);
        Assert.Equal(10, firstChart.Positions.Count);
        Assert.Contains(firstChart.Positions, position => position.Body == CelestialBody.Sun && position.EclipticLongitude.Degrees == 10d);
        Assert.Contains(firstChart.Aspects, aspect => aspect.AspectType == AspectType.Conjunction);
    }

    [Fact]
    public void ChartSceneFactory_ProducesDeterministicScene()
    {
        var firstScene = DevelopmentSampleChartSceneFactory.Create();
        var secondScene = DevelopmentSampleChartSceneFactory.Create();

        Assert.IsType<ChartRenderScene>(firstScene);
        Assert.Equal(firstScene.ZodiacSectors, secondScene.ZodiacSectors);
        Assert.Equal(firstScene.PlanetGlyphSlots, secondScene.PlanetGlyphSlots);
        Assert.Equal(firstScene.AspectLines, secondScene.AspectLines);
    }
}
