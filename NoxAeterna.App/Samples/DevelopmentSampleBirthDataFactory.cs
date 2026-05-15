using NodaTime;
using NoxAeterna.Domain.Birth;

namespace NoxAeterna.App.Samples;

/// <summary>
/// Provides deterministic birth data for initial real-chart startup and non-user-driven verification.
/// </summary>
public static class DevelopmentSampleBirthDataFactory
{
    /// <summary>
    /// Creates deterministic birth data that can be resolved and calculated through the live ephemeris pipeline.
    /// </summary>
    public static BirthData Create() =>
        new(
            new LocalBirthDateTime(new LocalDate(1990, 7, 14), new LocalTime(13, 45)),
            BirthTimeAccuracy.ExactTime,
            new BirthLocation("Prague, Czechia", 50.0755d, 14.4378d),
            new TimezoneId("Europe/Prague"),
            "Development sample birth data");
}
