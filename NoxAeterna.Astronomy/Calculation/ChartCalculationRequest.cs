using NoxAeterna.Domain.Astrology;
using NoxAeterna.Domain.Birth;

namespace NoxAeterna.Astronomy.Calculation;

/// <summary>
/// Represents a request to calculate celestial positions for a specific birth moment.
/// </summary>
public sealed record ChartCalculationRequest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChartCalculationRequest"/> class.
    /// </summary>
    /// <param name="calculationMoment">The calculation moment.</param>
    /// <param name="requestedBodies">The celestial bodies to calculate.</param>
    /// <param name="ephemerisSourceVersion">The optional source or ephemeris version hint.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="requestedBodies"/> is empty.</exception>
    public ChartCalculationRequest(
        BirthMoment calculationMoment,
        IEnumerable<CelestialBody> requestedBodies,
        string? ephemerisSourceVersion = null)
    {
        var bodies = requestedBodies?.Distinct().ToArray() ?? throw new ArgumentNullException(nameof(requestedBodies));

        if (bodies.Length == 0)
        {
            throw new ArgumentException("At least one celestial body must be requested.", nameof(requestedBodies));
        }

        CalculationMoment = calculationMoment;
        RequestedBodies = Array.AsReadOnly(bodies);
        EphemerisSourceVersion = string.IsNullOrWhiteSpace(ephemerisSourceVersion) ? null : ephemerisSourceVersion.Trim();
    }

    /// <summary>
    /// Gets the calculation moment.
    /// </summary>
    public BirthMoment CalculationMoment { get; }

    /// <summary>
    /// Gets the celestial bodies requested for calculation.
    /// </summary>
    public IReadOnlyList<CelestialBody> RequestedBodies { get; }

    /// <summary>
    /// Gets the optional source or ephemeris version hint.
    /// </summary>
    public string? EphemerisSourceVersion { get; }
}
