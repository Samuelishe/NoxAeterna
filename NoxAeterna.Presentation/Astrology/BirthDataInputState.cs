using NoxAeterna.Domain.Birth;

namespace NoxAeterna.Presentation.Astrology;

/// <summary>
/// Represents editable birth-data input state in the astrology workspace.
/// </summary>
public sealed record BirthDataInputState(
    string BirthDateText,
    string BirthTimeText,
    BirthTimeAccuracy BirthTimeAccuracy,
    string BirthPlaceDisplayName,
    string LatitudeText,
    string LongitudeText,
    string TimezoneIdText);
