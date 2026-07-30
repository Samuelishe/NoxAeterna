namespace NoxAeterna.Domain.Astrology;

/// <summary>
/// Represents the zodiac longitude at which a house begins.
/// </summary>
public sealed record HouseCusp
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HouseCusp"/> class.
    /// </summary>
    public HouseCusp(HouseNumber houseNumber, ZodiacLongitude longitude)
    {
        HouseNumber = houseNumber ?? throw new ArgumentNullException(nameof(houseNumber));
        Longitude = longitude;
    }

    /// <summary>
    /// Gets the validated house number.
    /// </summary>
    public HouseNumber HouseNumber { get; }

    /// <summary>
    /// Gets the normalized cusp longitude.
    /// </summary>
    public ZodiacLongitude Longitude { get; }
}
