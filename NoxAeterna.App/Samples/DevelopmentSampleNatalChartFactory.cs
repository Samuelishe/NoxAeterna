using NodaTime;
using NoxAeterna.Domain.Astrology;
using NoxAeterna.Domain.Birth;

namespace NoxAeterna.App.Samples;

/// <summary>
/// Creates deterministic development-only chart data for workspace and rendering verification.
/// This factory is not product data flow.
/// </summary>
public static class DevelopmentSampleNatalChartFactory
{
    /// <summary>
    /// Creates a deterministic natal chart snapshot for development-only use.
    /// </summary>
    /// <returns>A sample natal chart snapshot.</returns>
    public static NatalChart Create()
    {
        var birthMoment = new BirthMoment(
            new LocalDateTime(1990, 7, 14, 13, 45),
            new TimezoneId("Europe/Moscow"),
            Instant.FromUtc(1990, 7, 14, 9, 45),
            TimeResolutionStatus.Resolved,
            BirthTimeAccuracy.ExactTime,
            "Development sample");

        var positions = new[]
        {
            new PlanetPosition(CelestialBody.Sun, new ZodiacLongitude(10d), false, speed: 0.985d),
            new PlanetPosition(CelestialBody.Moon, new ZodiacLongitude(100d), false, speed: 13.2d),
            new PlanetPosition(CelestialBody.Mercury, new ZodiacLongitude(42d), false, speed: 1.1d),
            new PlanetPosition(CelestialBody.Venus, new ZodiacLongitude(144d), false, speed: 1.0d),
            new PlanetPosition(CelestialBody.Mars, new ZodiacLongitude(220d), true, speed: -0.4d),
            new PlanetPosition(CelestialBody.Jupiter, new ZodiacLongitude(278d), true, speed: -0.08d),
            new PlanetPosition(CelestialBody.Saturn, new ZodiacLongitude(300d), true, speed: -0.03d),
            new PlanetPosition(CelestialBody.Uranus, new ZodiacLongitude(305d), true, speed: -0.01d),
            new PlanetPosition(CelestialBody.Neptune, new ZodiacLongitude(14d), true, speed: -0.01d),
            new PlanetPosition(CelestialBody.Pluto, new ZodiacLongitude(225d), true, speed: -0.02d)
        };

        return NatalChart.Create(
            birthMoment,
            positions,
            ephemerisSourceVersion: "development-sample");
    }
}
