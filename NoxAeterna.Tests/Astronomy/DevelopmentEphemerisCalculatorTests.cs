using NodaTime;
using NoxAeterna.Astronomy.Calculation;
using NoxAeterna.Domain.Astrology;
using NoxAeterna.Domain.Birth;

namespace NoxAeterna.Tests.Astronomy;

public sealed class DevelopmentEphemerisCalculatorTests
{
    [Fact]
    public void Calculate_ReturnsDeterministicPositionsForSameRequest()
    {
        var calculator = new DevelopmentEphemerisCalculator();
        var request = CreateRequest(
            Instant.FromUtc(1990, 7, 14, 9, 45),
            new BirthLocation("Prague, Czechia", 50.0755d, 14.4378d));

        var first = calculator.Calculate(request);
        var second = calculator.Calculate(request);

        Assert.Equal(first.Positions, second.Positions);
        Assert.Equal("development-demo", first.EphemerisSourceVersion);
    }

    [Fact]
    public void Calculate_ReturnsStableButDifferentPositionsForDifferentInputs()
    {
        var calculator = new DevelopmentEphemerisCalculator();
        var firstRequest = CreateRequest(
            Instant.FromUtc(1990, 7, 14, 9, 45),
            new BirthLocation("Prague, Czechia", 50.0755d, 14.4378d));
        var secondRequest = CreateRequest(
            Instant.FromUtc(1990, 7, 15, 9, 45),
            new BirthLocation("Rome, Italy", 41.9028d, 12.4964d));

        var first = calculator.Calculate(firstRequest);
        var second = calculator.Calculate(secondRequest);

        Assert.NotEqual(first.Positions, second.Positions);
    }

    private static ChartCalculationRequest CreateRequest(Instant instant, BirthLocation location) =>
        new(
            new BirthMoment(
                instant.InUtc().LocalDateTime,
                new TimezoneId("Europe/Prague"),
                instant,
                TimeResolutionStatus.Resolved,
                BirthTimeAccuracy.ExactTime,
                "Test request"),
            new[] { CelestialBody.Sun, CelestialBody.Moon, CelestialBody.Mars },
            location);
}
