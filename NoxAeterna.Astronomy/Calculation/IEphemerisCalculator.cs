namespace NoxAeterna.Astronomy.Calculation;

/// <summary>
/// Defines a calculator that produces celestial positions without exposing provider-specific ephemeris types.
/// </summary>
public interface IEphemerisCalculator
{
    /// <summary>
    /// Calculates celestial positions for the supplied request.
    /// </summary>
    /// <param name="request">The calculation request.</param>
    /// <returns>The calculation result.</returns>
    ChartCalculationResult Calculate(ChartCalculationRequest request);
}
