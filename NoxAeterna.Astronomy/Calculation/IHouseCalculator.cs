namespace NoxAeterna.Astronomy.Calculation;

/// <summary>
/// Calculates natal house cusps and principal chart angles.
/// </summary>
public interface IHouseCalculator
{
    /// <summary>
    /// Calculates houses for a resolved birth moment, location, and explicit house system.
    /// </summary>
    HouseCalculationResult Calculate(HouseCalculationRequest request);
}
