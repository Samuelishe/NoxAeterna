using NodaTime;
using NoxAeterna.Astronomy.Calculation;
using NoxAeterna.Domain.Astrology;
using NoxAeterna.Domain.Birth;

namespace NoxAeterna.Tests.Astronomy;

public sealed class ChartCalculationContractsTests
{
    [Fact]
    public void ChartCalculationRequest_PreservesRequestedBodies()
    {
        var request = new ChartCalculationRequest(
            CreateBirthMoment(),
            new[] { CelestialBody.Sun, CelestialBody.Moon, CelestialBody.Mars });

        Assert.Equal(
            new[] { CelestialBody.Sun, CelestialBody.Moon, CelestialBody.Mars },
            request.RequestedBodies);
    }

    [Fact]
    public void ChartCalculationRequest_RejectsEmptyBodySet()
    {
        Assert.Throws<ArgumentException>(() =>
            new ChartCalculationRequest(CreateBirthMoment(), Array.Empty<CelestialBody>()));
    }

    [Fact]
    public void IEphemerisCalculator_CanReturnDeterministicPositionsThroughTestFake()
    {
        var request = new ChartCalculationRequest(
            CreateBirthMoment(),
            new[] { CelestialBody.Sun, CelestialBody.Moon });

        IEphemerisCalculator calculator = new FakeEphemerisCalculator();

        var result = calculator.Calculate(request);

        Assert.Equal(request.CalculationMoment, result.CalculationMoment);
        Assert.Equal(2, result.Positions.Count);
        Assert.Equal(CelestialBody.Sun, result.Positions[0].Body);
        Assert.Equal(15d, result.Positions[0].EclipticLongitude.Degrees, precision: 10);
        Assert.Equal(CelestialBody.Moon, result.Positions[1].Body);
        Assert.Equal(123.5d, result.Positions[1].EclipticLongitude.Degrees, precision: 10);
        Assert.Equal("test-fake", result.EphemerisSourceVersion);
    }

    private static BirthMoment CreateBirthMoment() =>
        new(
            new LocalDateTime(1990, 7, 14, 13, 45),
            new TimezoneId("Europe/Moscow"),
            Instant.FromUtc(1990, 7, 14, 9, 45),
            TimeResolutionStatus.Resolved,
            BirthTimeAccuracy.ExactTime,
            "Test fixture");

    private sealed class FakeEphemerisCalculator : IEphemerisCalculator
    {
        public ChartCalculationResult Calculate(ChartCalculationRequest request)
        {
            var positions = new[]
            {
                new PlanetPosition(CelestialBody.Sun, new ZodiacLongitude(15d), false, speed: 1d),
                new PlanetPosition(CelestialBody.Moon, new ZodiacLongitude(123.5d), false, speed: 12d)
            };

            return new ChartCalculationResult(request.CalculationMoment, positions, "test-fake");
        }
    }
}
