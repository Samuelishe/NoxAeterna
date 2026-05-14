using NoxAeterna.Domain.Astrology;

namespace NoxAeterna.Geometry.Charts;

/// <summary>
/// Represents a normalized chart-space angle in degrees.
/// </summary>
public readonly record struct AngularPosition
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AngularPosition"/> struct.
    /// </summary>
    /// <param name="degrees">The chart-space angle in degrees.</param>
    public AngularPosition(double degrees)
    {
        Degrees = ZodiacLongitude.Normalize(degrees);
    }

    /// <summary>
    /// Gets the normalized chart-space angle in degrees in the range [0, 360).
    /// </summary>
    public double Degrees { get; }

    /// <summary>
    /// Creates a chart-space angle from a normalized ecliptic longitude.
    /// </summary>
    /// <param name="longitude">The source longitude.</param>
    /// <returns>The corresponding chart-space angle.</returns>
    public static AngularPosition FromLongitude(ZodiacLongitude longitude) => new(longitude.Degrees);
}
