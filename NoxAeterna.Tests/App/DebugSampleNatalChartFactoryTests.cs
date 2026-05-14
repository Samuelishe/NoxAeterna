using NoxAeterna.App.Debug;
using NoxAeterna.Domain.Astrology;

namespace NoxAeterna.Tests.App;

public sealed class DebugSampleNatalChartFactoryTests
{
    [Fact]
    public void Create_ProducesDeterministicNatalChart()
    {
        var firstChart = DebugSampleNatalChartFactory.Create();
        var secondChart = DebugSampleNatalChartFactory.Create();

        Assert.Equal(firstChart.BirthMoment, secondChart.BirthMoment);
        Assert.Equal(firstChart.Positions, secondChart.Positions);
        Assert.Equal(firstChart.Aspects, secondChart.Aspects);
        Assert.Equal("debug-sample", firstChart.EphemerisSourceVersion);
        Assert.Equal(10, firstChart.Positions.Count);
        Assert.Contains(firstChart.Positions, position => position.Body == CelestialBody.Sun && position.EclipticLongitude.Degrees == 10d);
        Assert.Contains(firstChart.Aspects, aspect => aspect.AspectType == AspectType.Conjunction);
    }
}
