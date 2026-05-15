namespace NoxAeterna.Presentation.Astrology;

/// <summary>
/// Describes where the current location details came from in the birth-data input flow.
/// </summary>
public enum LocationSource
{
    NameOnly = 0,
    ManualCoordinates = 1,
    OnlineLookup = 2,
    SavedPlace = 3
}
