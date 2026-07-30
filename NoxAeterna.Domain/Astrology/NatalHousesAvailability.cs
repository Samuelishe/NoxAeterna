namespace NoxAeterna.Domain.Astrology;

/// <summary>
/// Describes whether natal houses and chart angles are available.
/// </summary>
public enum NatalHousesAvailability
{
    /// <summary>
    /// All twelve cusps and chart angles are available.
    /// </summary>
    Available,

    /// <summary>
    /// Houses were deliberately not calculated because the birth time is unknown.
    /// </summary>
    UnavailableUnknownTime,

    /// <summary>
    /// The requested house system could not be calculated for the supplied data.
    /// </summary>
    UnavailableCalculation
}
