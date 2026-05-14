namespace NoxAeterna.Domain.Birth;

/// <summary>
/// Represents user-provided birth location input.
/// </summary>
public readonly record struct BirthLocation
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BirthLocation"/> struct.
    /// </summary>
    /// <param name="displayName">The user-facing location name.</param>
    /// <param name="latitude">The latitude in degrees in the range [-90, 90].</param>
    /// <param name="longitude">The longitude in degrees in the range [-180, 180].</param>
    /// <param name="region">The optional country or region label.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="displayName"/> is empty or whitespace.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when latitude or longitude is outside the supported range or is not finite.</exception>
    public BirthLocation(string displayName, double latitude, double longitude, string? region = null)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException("Display name must not be empty.", nameof(displayName));
        }

        if (double.IsNaN(latitude) || double.IsInfinity(latitude) || latitude < -90d || latitude > 90d)
        {
            throw new ArgumentOutOfRangeException(nameof(latitude), "Latitude must be a finite number in the range [-90, 90].");
        }

        if (double.IsNaN(longitude) || double.IsInfinity(longitude) || longitude < -180d || longitude > 180d)
        {
            throw new ArgumentOutOfRangeException(nameof(longitude), "Longitude must be a finite number in the range [-180, 180].");
        }

        DisplayName = displayName.Trim();
        Latitude = latitude;
        Longitude = longitude;
        Region = string.IsNullOrWhiteSpace(region) ? null : region.Trim();
    }

    /// <summary>
    /// Gets the user-facing location name.
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// Gets the latitude in degrees.
    /// </summary>
    public double Latitude { get; }

    /// <summary>
    /// Gets the longitude in degrees.
    /// </summary>
    public double Longitude { get; }

    /// <summary>
    /// Gets the optional country or region string.
    /// </summary>
    public string? Region { get; }
}
