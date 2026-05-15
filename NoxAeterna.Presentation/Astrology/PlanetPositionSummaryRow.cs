using NoxAeterna.Presentation.Localization;

namespace NoxAeterna.Presentation.Astrology;

/// <summary>
/// Represents one readable planet-position summary row for the current chart.
/// </summary>
public sealed record PlanetPositionSummaryRow(
    string Glyph,
    LocalizationKey BodyLabelKey,
    string SignGlyph,
    LocalizationKey SignLabelKey,
    string DegreeText,
    bool IsRetrograde);
