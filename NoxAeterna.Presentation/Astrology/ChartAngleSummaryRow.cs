using NoxAeterna.Presentation.Localization;

namespace NoxAeterna.Presentation.Astrology;

/// <summary>
/// Represents one localized principal chart-angle summary row.
/// </summary>
public sealed record ChartAngleSummaryRow(
    LocalizationKey AngleLabelKey,
    LocalizationKey SignLabelKey,
    string PositionText);
