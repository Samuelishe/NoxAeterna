using NoxAeterna.Domain.Astrology;
using NoxAeterna.Presentation.Localization;

namespace NoxAeterna.Presentation.Astrology;

/// <summary>
/// Builds compact, localizable principal-angle rows from a chart snapshot.
/// </summary>
public static class ChartAngleSummaryBuilder
{
    private static readonly LocalizationKey UnknownTimeStatusKey =
        new("ui.chart.angles.unavailable.unknown_time");
    private static readonly LocalizationKey CalculationStatusKey =
        new("ui.chart.angles.unavailable.calculation");

    /// <summary>
    /// Builds ASC and MC rows with degree-and-minute precision, or an unavailable status.
    /// </summary>
    public static ChartAngleSummary Build(NatalChart chart)
    {
        ArgumentNullException.ThrowIfNull(chart);

        if (chart.Houses is not { IsAvailable: true, Angles: not null } houses)
        {
            var statusKey = chart.Houses?.Availability == NatalHousesAvailability.UnavailableUnknownTime
                ? UnknownTimeStatusKey
                : CalculationStatusKey;
            return new ChartAngleSummary([], statusKey);
        }

        return new ChartAngleSummary(
        [
            BuildRow(
                "ui.chart.angles.ascendant",
                houses.Angles.Ascendant),
            BuildRow(
                "ui.chart.angles.midheaven",
                houses.Angles.Midheaven)
        ]);
    }

    private static ChartAngleSummaryRow BuildRow(
        string angleLabelKey,
        ZodiacLongitude longitude) =>
        new(
            new LocalizationKey(angleLabelKey),
            AstrologySymbolCatalog.GetSignLabelKey(longitude.Sign),
            PlanetPositionSummaryBuilder.FormatDegree(longitude.Degrees % 30d));
}
