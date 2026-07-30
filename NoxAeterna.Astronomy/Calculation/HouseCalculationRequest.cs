using NoxAeterna.Domain.Astrology;
using NoxAeterna.Domain.Birth;

namespace NoxAeterna.Astronomy.Calculation;

/// <summary>
/// Represents a provider-independent request for natal houses.
/// </summary>
public sealed record HouseCalculationRequest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HouseCalculationRequest"/> class.
    /// </summary>
    public HouseCalculationRequest(
        BirthMoment calculationMoment,
        BirthLocation? location,
        HouseSystem houseSystem)
    {
        if (location is null)
        {
            throw new ArgumentNullException(nameof(location), "House calculation requires a birth location.");
        }

        if (calculationMoment.BirthTimeAccuracy == BirthTimeAccuracy.UnknownTime)
        {
            throw new ArgumentException(
                "House calculation requires a known or approximate birth time.",
                nameof(calculationMoment));
        }

        CalculationMoment = calculationMoment;
        Location = location.Value;
        HouseSystem = houseSystem;
    }

    /// <summary>
    /// Gets the resolved calculation moment.
    /// </summary>
    public BirthMoment CalculationMoment { get; }

    /// <summary>
    /// Gets the validated geographic location.
    /// </summary>
    public BirthLocation Location { get; }

    /// <summary>
    /// Gets the explicitly selected house system.
    /// </summary>
    public HouseSystem HouseSystem { get; }
}
