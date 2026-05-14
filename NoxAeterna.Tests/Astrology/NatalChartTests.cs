using NodaTime;
using NoxAeterna.Domain.Astrology;
using NoxAeterna.Domain.Birth;

namespace NoxAeterna.Tests.Astrology;

public sealed class NatalChartTests
{
    [Fact]
    public void NatalChart_StoresBirthMomentPositionsAndDetectedAspects()
    {
        var birthMoment = CreateBirthMoment();
        var positions = new[]
        {
            new PlanetPosition(CelestialBody.Sun, new ZodiacLongitude(10d), false),
            new PlanetPosition(CelestialBody.Moon, new ZodiacLongitude(100d), false)
        };

        var chart = NatalChart.Create(birthMoment, positions, ephemerisSourceVersion: "test-source");

        Assert.Equal(birthMoment, chart.BirthMoment);
        Assert.Equal(2, chart.Positions.Count);
        Assert.Single(chart.Aspects);
        Assert.Equal(AspectType.Square, chart.Aspects[0].AspectType);
        Assert.Equal("test-source", chart.EphemerisSourceVersion);
    }

    [Theory]
    [InlineData(0d, AspectType.Conjunction)]
    [InlineData(60d, AspectType.Sextile)]
    [InlineData(90d, AspectType.Square)]
    [InlineData(120d, AspectType.Trine)]
    [InlineData(180d, AspectType.Opposition)]
    public void AspectDetection_FindsExactMajorAspect(double targetLongitude, AspectType expectedAspectType)
    {
        var positions = new[]
        {
            new PlanetPosition(CelestialBody.Sun, new ZodiacLongitude(0d), false),
            new PlanetPosition(CelestialBody.Moon, new ZodiacLongitude(targetLongitude), false)
        };

        var aspects = PlanetaryAspectCalculator.Calculate(positions);

        var aspect = Assert.Single(aspects);
        Assert.Equal(expectedAspectType, aspect.AspectType);
        Assert.Equal(Math.Abs(targetLongitude), aspect.AngularDeltaDegrees, precision: 10);
        Assert.Equal(0d, aspect.OrbDistanceDegrees, precision: 10);
        Assert.Equal(CelestialBody.Sun, aspect.SourceBody);
        Assert.Equal(CelestialBody.Moon, aspect.TargetBody);
    }

    [Fact]
    public void AspectDetection_HandlesWrapAroundAcrossZeroDegrees()
    {
        var positions = new[]
        {
            new PlanetPosition(CelestialBody.Moon, new ZodiacLongitude(1d), false),
            new PlanetPosition(CelestialBody.Sun, new ZodiacLongitude(359d), false)
        };

        var aspect = Assert.Single(PlanetaryAspectCalculator.Calculate(positions));

        Assert.Equal(AspectType.Conjunction, aspect.AspectType);
        Assert.Equal(2d, aspect.AngularDeltaDegrees, precision: 10);
        Assert.Equal(2d, aspect.OrbDistanceDegrees, precision: 10);
        Assert.Equal(CelestialBody.Sun, aspect.SourceBody);
        Assert.Equal(CelestialBody.Moon, aspect.TargetBody);
    }

    [Fact]
    public void AspectDetection_AvoidsDuplicateReversedAspects()
    {
        var positions = new[]
        {
            new PlanetPosition(CelestialBody.Moon, new ZodiacLongitude(90d), false),
            new PlanetPosition(CelestialBody.Sun, new ZodiacLongitude(0d), false)
        };

        var aspects = PlanetaryAspectCalculator.Calculate(positions);

        var aspect = Assert.Single(aspects);
        Assert.Equal(CelestialBody.Sun, aspect.SourceBody);
        Assert.Equal(CelestialBody.Moon, aspect.TargetBody);
    }

    [Fact]
    public void AspectDetection_AvoidsSelfAspects()
    {
        var positions = new[]
        {
            new PlanetPosition(CelestialBody.Sun, new ZodiacLongitude(0d), false)
        };

        var aspects = PlanetaryAspectCalculator.Calculate(positions);

        Assert.Empty(aspects);
    }

    [Fact]
    public void AspectDetection_RespectsOrb()
    {
        var positions = new[]
        {
            new PlanetPosition(CelestialBody.Sun, new ZodiacLongitude(0d), false),
            new PlanetPosition(CelestialBody.Moon, new ZodiacLongitude(67d), false)
        };

        Assert.Empty(PlanetaryAspectCalculator.Calculate(positions));

        var expandedOrbAspect = Assert.Single(PlanetaryAspectCalculator.Calculate(positions, orbDegrees: 7d));
        Assert.Equal(AspectType.Sextile, expandedOrbAspect.AspectType);
        Assert.Equal(7d, expandedOrbAspect.OrbDistanceDegrees, precision: 10);
    }

    [Fact]
    public void NatalChart_PublicCollectionsRemainReadOnly()
    {
        var sourcePositions = new List<PlanetPosition>
        {
            new(CelestialBody.Sun, new ZodiacLongitude(0d), false),
            new(CelestialBody.Moon, new ZodiacLongitude(90d), false)
        };

        var chart = NatalChart.Create(CreateBirthMoment(), sourcePositions);

        sourcePositions.Clear();

        Assert.Equal(2, chart.Positions.Count);
        Assert.Single(chart.Aspects);
        Assert.Throws<NotSupportedException>(() =>
            ((ICollection<PlanetPosition>)chart.Positions).Add(new PlanetPosition(CelestialBody.Mars, new ZodiacLongitude(120d), false)));
        Assert.Throws<NotSupportedException>(() =>
            ((ICollection<CalculatedAspect>)chart.Aspects).Add(new CalculatedAspect(
                CelestialBody.Sun,
                CelestialBody.Mars,
                AspectType.Trine,
                new ZodiacLongitude(0d),
                new ZodiacLongitude(120d))));
    }

    private static BirthMoment CreateBirthMoment() =>
        new(
            new LocalDateTime(1990, 7, 14, 13, 45),
            new TimezoneId("Europe/Moscow"),
            Instant.FromUtc(1990, 7, 14, 9, 45),
            TimeResolutionStatus.Resolved,
            BirthTimeAccuracy.ExactTime,
            "Test fixture");
}
