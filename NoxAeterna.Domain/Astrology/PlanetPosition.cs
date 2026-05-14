namespace NoxAeterna.Domain.Astrology;

/// <summary>
/// Represents a calculated ecliptic position for a celestial body.
/// </summary>
public readonly record struct PlanetPosition
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PlanetPosition"/> struct.
    /// </summary>
    /// <param name="body">The celestial body.</param>
    /// <param name="eclipticLongitude">The normalized ecliptic longitude.</param>
    /// <param name="isRetrograde">A value indicating whether the body is in retrograde motion.</param>
    /// <param name="eclipticLatitude">The optional ecliptic latitude in degrees.</param>
    /// <param name="speed">The optional ecliptic speed.</param>
    public PlanetPosition(
        CelestialBody body,
        ZodiacLongitude eclipticLongitude,
        bool isRetrograde,
        double? eclipticLatitude = null,
        double? speed = null)
    {
        Body = body;
        EclipticLongitude = eclipticLongitude;
        IsRetrograde = isRetrograde;
        EclipticLatitude = eclipticLatitude;
        Speed = speed;
    }

    /// <summary>
    /// Gets the celestial body.
    /// </summary>
    public CelestialBody Body { get; }

    /// <summary>
    /// Gets the normalized ecliptic longitude.
    /// </summary>
    public ZodiacLongitude EclipticLongitude { get; }

    /// <summary>
    /// Gets the zodiac sign derived from the ecliptic longitude.
    /// </summary>
    public ZodiacSign Sign => EclipticLongitude.Sign;

    /// <summary>
    /// Gets the degree within the derived zodiac sign in the range [0, 30).
    /// </summary>
    public double DegreeWithinSign => EclipticLongitude.Degrees % 30d;

    /// <summary>
    /// Gets a value indicating whether the body is in retrograde motion.
    /// </summary>
    public bool IsRetrograde { get; }

    /// <summary>
    /// Gets the optional ecliptic latitude in degrees.
    /// </summary>
    public double? EclipticLatitude { get; }

    /// <summary>
    /// Gets the optional ecliptic speed.
    /// </summary>
    public double? Speed { get; }
}
