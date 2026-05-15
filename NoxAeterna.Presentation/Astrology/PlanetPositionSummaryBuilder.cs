using NoxAeterna.Domain.Astrology;

namespace NoxAeterna.Presentation.Astrology;

/// <summary>
/// Builds readable planet-position summary rows from a natal chart snapshot.
/// </summary>
public static class PlanetPositionSummaryBuilder
{
    /// <summary>
    /// Builds summary rows in deterministic body order.
    /// </summary>
    public static IReadOnlyList<PlanetPositionSummaryRow> Build(NatalChart chart)
    {
        ArgumentNullException.ThrowIfNull(chart);

        return chart.Positions
            .OrderBy(static position => position.Body)
            .Select(position => new PlanetPositionSummaryRow(
                AstrologySymbolCatalog.GetBodyGlyph(position.Body),
                AstrologySymbolCatalog.GetBodyLabelKey(position.Body),
                AstrologySymbolCatalog.GetSignGlyph(position.Sign),
                AstrologySymbolCatalog.GetSignLabelKey(position.Sign),
                FormatDegree(position.DegreeWithinSign),
                position.IsRetrograde))
            .ToArray();
    }

    /// <summary>
    /// Formats a within-sign degree value as degrees and minutes.
    /// </summary>
    public static string FormatDegree(double degreeWithinSign)
    {
        if (double.IsNaN(degreeWithinSign) || double.IsInfinity(degreeWithinSign) || degreeWithinSign < 0d || degreeWithinSign >= 30d)
        {
            throw new ArgumentOutOfRangeException(nameof(degreeWithinSign), "Degree within sign must be in the range [0, 30).");
        }

        var roundedTotalMinutes = (int)Math.Round(degreeWithinSign * 60d, MidpointRounding.AwayFromZero);
        var boundedTotalMinutes = Math.Min(roundedTotalMinutes, (30 * 60) - 1);
        var degrees = boundedTotalMinutes / 60;
        var minutes = boundedTotalMinutes % 60;

        return $"{degrees:00}°{minutes:00}'";
    }
}
