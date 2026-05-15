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

        var totalMinutes = (int)Math.Floor((degreeWithinSign * 60d) + 0.0000001d);
        var degrees = totalMinutes / 60;
        var minutes = totalMinutes % 60;

        return $"{degrees:00}°{minutes:00}'";
    }
}
