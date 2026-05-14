namespace NoxAeterna.Domain.Astrology;

/// <summary>
/// Represents a normalized ecliptic longitude in degrees.
/// </summary>
public readonly record struct ZodiacLongitude
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ZodiacLongitude"/> struct.
    /// </summary>
    /// <param name="degrees">The longitude in degrees. The value is normalized to the range [0, 360).</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="degrees"/> is not a finite number.</exception>
    public ZodiacLongitude(double degrees)
    {
        Degrees = Normalize(degrees);
    }

    /// <summary>
    /// Gets the normalized ecliptic longitude in degrees.
    /// </summary>
    public double Degrees { get; }

    /// <summary>
    /// Gets the zodiac sign derived from the normalized longitude.
    /// </summary>
    public ZodiacSign Sign => (ZodiacSign)(int)(Degrees / 30d);

    /// <summary>
    /// Normalizes a longitude to the range [0, 360).
    /// </summary>
    /// <param name="degrees">The longitude in degrees.</param>
    /// <returns>The normalized longitude.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="degrees"/> is not a finite number.</exception>
    public static double Normalize(double degrees)
    {
        if (double.IsNaN(degrees) || double.IsInfinity(degrees))
        {
            throw new ArgumentOutOfRangeException(nameof(degrees), "Longitude must be a finite number.");
        }

        var normalized = degrees % 360d;

        if (normalized < 0d)
        {
            normalized += 360d;
        }

        return normalized == 360d ? 0d : normalized;
    }

    /// <inheritdoc />
    public override string ToString() => Degrees.ToString("0.###");
}
