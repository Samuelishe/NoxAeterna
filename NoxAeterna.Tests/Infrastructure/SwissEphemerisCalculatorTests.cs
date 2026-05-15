using NodaTime;
using NoxAeterna.Astronomy.Calculation;
using NoxAeterna.Domain.Astrology;
using NoxAeterna.Domain.Birth;
using NoxAeterna.Infrastructure.Ephemeris;

namespace NoxAeterna.Tests.Infrastructure;

public sealed class SwissEphemerisCalculatorTests
{
    [Fact]
    public void Calculate_ReturnsRequestedBodiesWithNormalizedLongitudes()
    {
        IEphemerisCalculator calculator = new SwissEphemerisCalculator();
        var request = new ChartCalculationRequest(
            CreateBirthMoment(),
            new[] { CelestialBody.Sun, CelestialBody.Moon, CelestialBody.Mars });

        var result = calculator.Calculate(request);

        Assert.Equal(request.RequestedBodies, result.Positions.Select(position => position.Body));
        Assert.All(result.Positions, position =>
        {
            Assert.InRange(position.EclipticLongitude.Degrees, 0d, 360d);
            Assert.Equal(position.EclipticLongitude.Sign, position.Sign);
        });
    }

    [Fact]
    public void Calculate_IsDeterministicForTheSameRequest()
    {
        IEphemerisCalculator calculator = new SwissEphemerisCalculator();
        var request = new ChartCalculationRequest(
            CreateBirthMoment(),
            Enum.GetValues<CelestialBody>());

        var first = calculator.Calculate(request);
        var second = calculator.Calculate(request);

        Assert.Equal(first.Positions, second.Positions);
        Assert.Equal(first.EphemerisSourceVersion, second.EphemerisSourceVersion);
    }

    [Fact]
    public void Calculate_UsesSwissEphNetMetadataAndCanFallbackWithoutExternalFiles()
    {
        IEphemerisCalculator calculator = new SwissEphemerisCalculator();
        var request = new ChartCalculationRequest(
            CreateBirthMoment(),
            new[] { CelestialBody.Sun });

        var result = calculator.Calculate(request);

        Assert.Contains("SwissEphNet", result.EphemerisSourceVersion, StringComparison.Ordinal);
        Assert.Contains("Moshier", result.EphemerisSourceVersion, StringComparison.OrdinalIgnoreCase);
    }

    private static BirthMoment CreateBirthMoment() =>
        new(
            new LocalDateTime(1990, 7, 14, 13, 45),
            new TimezoneId("Europe/Prague"),
            Instant.FromUtc(1990, 7, 14, 11, 45),
            TimeResolutionStatus.Resolved,
            BirthTimeAccuracy.ExactTime,
            "Swiss ephemeris test fixture");
}
