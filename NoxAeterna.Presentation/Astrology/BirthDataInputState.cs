using NoxAeterna.Domain.Birth;

namespace NoxAeterna.Presentation.Astrology;

/// <summary>
/// Represents editable birth-data input state in the astrology workspace.
/// </summary>
public sealed record BirthDataInputState(
    DateTimeOffset? BirthDate,
    TimeSpan? BirthTime,
    BirthTimeAccuracy BirthTimeAccuracy,
    string BirthPlaceDisplayName,
    string LatitudeText,
    string LongitudeText,
    string TimezoneId,
    LocationSource LocationSource);
