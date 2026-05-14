using NodaTime;
using NoxAeterna.Domain.Astrology;
using NoxAeterna.Domain.Birth;
using NoxAeterna.Geometry.Charts;

namespace NoxAeterna.Tests.Geometry;

public sealed class CircularChartLayoutBuilderTests
{
    [Theory]
    [InlineData(-1d, 359d)]
    [InlineData(360d, 0d)]
    [InlineData(721d, 1d)]
    public void AngularPosition_NormalizesInput(double input, double expected)
    {
        var position = new AngularPosition(input);

        Assert.Equal(expected, position.Degrees, precision: 10);
    }

    [Theory]
    [InlineData(0d, 0d, -0.88d)]
    [InlineData(90d, 0.88d, 0d)]
    [InlineData(180d, 0d, 0.88d)]
    public void RadialPoint_ProducesDeterministicCircularCoordinates(double degrees, double expectedX, double expectedY)
    {
        var point = new RadialPoint(new AngularPosition(degrees), 0.88d);

        Assert.Equal(expectedX, point.X, precision: 10);
        Assert.Equal(expectedY, point.Y, precision: 10);
    }

    [Fact]
    public void Build_CreatesDeterministicCircularLayout()
    {
        var chart = CreateChart();
        var builder = new CircularChartLayoutBuilder();

        var layout = builder.Build(chart);

        Assert.Equal(12, layout.ZodiacSectors.Count);
        Assert.Equal(ZodiacSign.Aries, layout.ZodiacSectors[0].Sign);
        Assert.Equal(0d, layout.ZodiacSectors[0].StartAngle.Degrees, precision: 10);
        Assert.Equal(30d, layout.ZodiacSectors[0].EndAngle.Degrees, precision: 10);
        Assert.Equal(ZodiacSign.Pisces, layout.ZodiacSectors[^1].Sign);
        Assert.Equal(330d, layout.ZodiacSectors[^1].StartAngle.Degrees, precision: 10);
        Assert.Equal(0d, layout.ZodiacSectors[^1].EndAngle.Degrees, precision: 10);

        Assert.Equal(3, layout.PlanetGlyphSlots.Count);
        Assert.Equal(CelestialBody.Sun, layout.PlanetGlyphSlots[0].Body);
        Assert.Equal(0, layout.PlanetGlyphSlots[0].SlotIndex);
        Assert.Equal(0, layout.PlanetGlyphSlots[0].RadialBandIndex);
        Assert.Equal(10d, layout.PlanetGlyphSlots[0].Angle.Degrees, precision: 10);

        Assert.Equal(CelestialBody.Mars, layout.PlanetGlyphSlots[1].Body);
        Assert.Equal(220d, layout.PlanetGlyphSlots[1].Angle.Degrees, precision: 10);

        Assert.Equal(CelestialBody.Neptune, layout.PlanetGlyphSlots[2].Body);
        Assert.Equal(100d, layout.PlanetGlyphSlots[2].Angle.Degrees, precision: 10);

        Assert.Equal(2, layout.AspectLines.Count);
        Assert.Equal(AspectType.Square, layout.AspectLines[0].AspectType);
        Assert.Equal(CelestialBody.Sun, layout.AspectLines[0].SourceBody);
        Assert.Equal(CelestialBody.Neptune, layout.AspectLines[0].TargetBody);
        Assert.Equal(AspectType.Trine, layout.AspectLines[1].AspectType);
        Assert.Equal(CelestialBody.Mars, layout.AspectLines[1].SourceBody);
        Assert.Equal(CelestialBody.Neptune, layout.AspectLines[1].TargetBody);
    }

    [Fact]
    public void Build_HandlesWrapAroundNearZeroDegrees()
    {
        var chart = NatalChart.Create(
            CreateBirthMoment(),
            new[]
            {
                new PlanetPosition(CelestialBody.Sun, new ZodiacLongitude(359d), false),
                new PlanetPosition(CelestialBody.Moon, new ZodiacLongitude(1d), false)
            });

        var layout = new CircularChartLayoutBuilder().Build(chart);

        Assert.Equal(359d, layout.PlanetGlyphSlots[0].Angle.Degrees, precision: 10);
        Assert.Equal(1d, layout.PlanetGlyphSlots[1].Angle.Degrees, precision: 10);
        Assert.Single(layout.AspectLines);
        Assert.Equal(359d, layout.AspectLines[0].SourceAngle.Degrees, precision: 10);
        Assert.Equal(1d, layout.AspectLines[0].TargetAngle.Degrees, precision: 10);
    }

    [Fact]
    public void Build_IsStableForSameInput()
    {
        var chart = CreateChart();
        var builder = new CircularChartLayoutBuilder();

        var firstLayout = builder.Build(chart);
        var secondLayout = builder.Build(chart);

        Assert.Equal(firstLayout.ZodiacSectors, secondLayout.ZodiacSectors);
        Assert.Equal(firstLayout.PlanetGlyphSlots, secondLayout.PlanetGlyphSlots);
        Assert.Equal(firstLayout.AspectLines, secondLayout.AspectLines);
    }

    private static NatalChart CreateChart() =>
        NatalChart.Create(
            CreateBirthMoment(),
            new[]
            {
                new PlanetPosition(CelestialBody.Neptune, new ZodiacLongitude(100d), false),
                new PlanetPosition(CelestialBody.Sun, new ZodiacLongitude(10d), false),
                new PlanetPosition(CelestialBody.Mars, new ZodiacLongitude(220d), true)
            });

    private static BirthMoment CreateBirthMoment() =>
        new(
            new LocalDateTime(1990, 7, 14, 13, 45),
            new TimezoneId("Europe/Moscow"),
            Instant.FromUtc(1990, 7, 14, 9, 45),
            TimeResolutionStatus.Resolved,
            BirthTimeAccuracy.ExactTime,
            "Geometry fixture");
}
