using NoxAeterna.Presentation.Localization;

namespace NoxAeterna.Presentation.Astrology;

/// <summary>
/// Represents one readable planet-position summary row for the current chart.
/// </summary>
public sealed record PlanetPositionSummaryRow(
    LocalizationKey PlanetLabelKey,
    LocalizationKey SignLabelKey,
    string PositionText,
    bool IsRetrograde);
