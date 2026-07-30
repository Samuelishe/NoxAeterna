using NoxAeterna.Domain.Astrology;

namespace NoxAeterna.Geometry.Charts;

/// <summary>
/// Defines one deterministic transform from source zodiac longitude to chart-space angle.
/// </summary>
public readonly record struct ChartOrientation
{
    private const double AscendantAtLeftChartAngle = 270d;

    private ChartOrientation(double rotationDegrees, ZodiacLongitude? ascendant)
    {
        RotationDegrees = ZodiacLongitude.Normalize(rotationDegrees);
        Ascendant = ascendant;
    }

    /// <summary>
    /// Gets the Aries-at-top orientation used when houses are unavailable.
    /// </summary>
    public static ChartOrientation AriesAtTop { get; } = new(0d, null);

    /// <summary>
    /// Gets the clockwise rotation added to every source longitude.
    /// </summary>
    public double RotationDegrees { get; }

    /// <summary>
    /// Gets the Ascendant that determined the orientation, when present.
    /// </summary>
    public ZodiacLongitude? Ascendant { get; }

    /// <summary>
    /// Gets whether the orientation is derived from an available Ascendant.
    /// </summary>
    public bool IsAscendantOriented => Ascendant is not null;

    /// <summary>
    /// Creates an orientation that places the Ascendant at the left (9 o'clock).
    /// </summary>
    public static ChartOrientation AscendantAtLeft(ZodiacLongitude ascendant) =>
        new(AscendantAtLeftChartAngle - ascendant.Degrees, ascendant);

    /// <summary>
    /// Transforms an immutable source longitude to a display angle.
    /// </summary>
    public AngularPosition Transform(ZodiacLongitude longitude) =>
        TransformDegrees(longitude.Degrees);

    /// <summary>
    /// Transforms a longitude-like degree value, including collision-adjusted display values.
    /// </summary>
    public AngularPosition TransformDegrees(double degrees) =>
        new(degrees + RotationDegrees);
}
