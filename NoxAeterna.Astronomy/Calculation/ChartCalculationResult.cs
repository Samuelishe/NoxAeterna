using NoxAeterna.Domain.Astrology;
using NoxAeterna.Domain.Birth;

namespace NoxAeterna.Astronomy.Calculation;

/// <summary>
/// Represents calculated celestial positions for a single calculation moment.
/// </summary>
public sealed record ChartCalculationResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChartCalculationResult"/> class.
    /// </summary>
    /// <param name="calculationMoment">The calculation moment used for the result.</param>
    /// <param name="positions">The calculated body positions.</param>
    /// <param name="ephemerisSourceVersion">The optional source or ephemeris version metadata.</param>
    public ChartCalculationResult(
        BirthMoment calculationMoment,
        IEnumerable<PlanetPosition> positions,
        string? ephemerisSourceVersion = null)
    {
        CalculationMoment = calculationMoment;
        Positions = Array.AsReadOnly((positions ?? throw new ArgumentNullException(nameof(positions))).ToArray());
        EphemerisSourceVersion = string.IsNullOrWhiteSpace(ephemerisSourceVersion) ? null : ephemerisSourceVersion.Trim();
    }

    /// <summary>
    /// Gets the calculation moment used for the result.
    /// </summary>
    public BirthMoment CalculationMoment { get; }

    /// <summary>
    /// Gets the calculated positions.
    /// </summary>
    public IReadOnlyList<PlanetPosition> Positions { get; }

    /// <summary>
    /// Gets the optional source or ephemeris version metadata.
    /// </summary>
    public string? EphemerisSourceVersion { get; }
}
