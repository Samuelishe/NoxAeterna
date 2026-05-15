using NoxAeterna.Domain.Astrology;

namespace NoxAeterna.Astronomy.Calculation;

/// <summary>
/// Produces deterministic development-only chart positions.
/// This calculator is not astronomically accurate and exists only until real ephemeris integration is added.
/// </summary>
public sealed class DevelopmentEphemerisCalculator : IEphemerisCalculator
{
    private static readonly IReadOnlyDictionary<CelestialBody, BodyProfile> Profiles = new Dictionary<CelestialBody, BodyProfile>
    {
        [CelestialBody.Sun] = new(18.0d, 0.985d, 0.2d, 0d),
        [CelestialBody.Moon] = new(143.0d, 13.176d, 0.9d, 1d),
        [CelestialBody.Mercury] = new(72.0d, 1.38d, 1.4d, 2d),
        [CelestialBody.Venus] = new(196.0d, 1.12d, 1.0d, 3d),
        [CelestialBody.Mars] = new(233.0d, 0.52d, 0.7d, 4d),
        [CelestialBody.Jupiter] = new(287.0d, 0.08d, 0.18d, 5d),
        [CelestialBody.Saturn] = new(319.0d, 0.03d, 0.15d, 6d),
        [CelestialBody.Uranus] = new(41.0d, 0.011d, 0.1d, 7d),
        [CelestialBody.Neptune] = new(102.0d, 0.006d, 0.08d, 8d),
        [CelestialBody.Pluto] = new(258.0d, 0.004d, 0.07d, 9d)
    };

    /// <inheritdoc />
    public ChartCalculationResult Calculate(ChartCalculationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var daysSinceUnixEpoch = request.CalculationMoment.Instant.ToUnixTimeSeconds() / 86400d;
        var locationOffset = request.LocationContext is { } locationContext
            ? (locationContext.Latitude * 0.17d) + (locationContext.Longitude * 0.11d)
            : 0d;

        var positions = request.RequestedBodies
            .OrderBy(static body => body)
            .Select(body => BuildPosition(body, daysSinceUnixEpoch, locationOffset))
            .ToArray();

        return new ChartCalculationResult(
            request.CalculationMoment,
            positions,
            ephemerisSourceVersion: "development-demo");
    }

    private static PlanetPosition BuildPosition(CelestialBody body, double daysSinceUnixEpoch, double locationOffset)
    {
        var profile = Profiles.TryGetValue(body, out var resolvedProfile)
            ? resolvedProfile
            : throw new ArgumentOutOfRangeException(nameof(body), body, "Unsupported celestial body for development ephemeris calculation.");

        var phase = daysSinceUnixEpoch / (11d + profile.PhaseOffset) + profile.PhaseOffset;
        var longitude = new ZodiacLongitude(
            profile.BaseLongitude +
            (daysSinceUnixEpoch * profile.FakeDailyRate) +
            (locationOffset * (0.35d + (profile.PhaseOffset * 0.03d))));

        var speed = body is CelestialBody.Sun or CelestialBody.Moon
            ? Math.Abs(profile.FakeDailyRate)
            : profile.FakeDailyRate * Math.Cos(phase) * (0.75d + profile.SpeedVariation);
        var isRetrograde = body is not (CelestialBody.Sun or CelestialBody.Moon) && speed < 0d;
        var latitude = Math.Sin(phase * 0.7d) * (1.2d + (profile.PhaseOffset * 0.15d));

        return new PlanetPosition(
            body,
            longitude,
            isRetrograde,
            eclipticLatitude: latitude,
            speed: speed);
    }

    private sealed record BodyProfile(
        double BaseLongitude,
        double FakeDailyRate,
        double SpeedVariation,
        double PhaseOffset);
}
