using NoxAeterna.Domain.Astrology;

namespace NoxAeterna.Geometry.Charts;

/// <summary>
/// Represents a zodiac sector within a circular chart layout.
/// </summary>
public readonly record struct ZodiacSectorGeometry
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ZodiacSectorGeometry"/> struct.
    /// </summary>
    /// <param name="sign">The zodiac sign represented by the sector.</param>
    /// <param name="startAngle">The inclusive start angle.</param>
    /// <param name="endAngle">The exclusive end angle.</param>
    /// <param name="innerRadiusRatio">The normalized inner radius.</param>
    /// <param name="outerRadiusRatio">The normalized outer radius.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the radii are invalid.</exception>
    public ZodiacSectorGeometry(
        ZodiacSign sign,
        AngularPosition startAngle,
        AngularPosition endAngle,
        double innerRadiusRatio,
        double outerRadiusRatio)
    {
        if (double.IsNaN(innerRadiusRatio) || double.IsInfinity(innerRadiusRatio) || innerRadiusRatio < 0d)
        {
            throw new ArgumentOutOfRangeException(nameof(innerRadiusRatio), "Inner radius ratio must be a finite non-negative number.");
        }

        if (double.IsNaN(outerRadiusRatio) || double.IsInfinity(outerRadiusRatio) || outerRadiusRatio <= 0d)
        {
            throw new ArgumentOutOfRangeException(nameof(outerRadiusRatio), "Outer radius ratio must be a finite positive number.");
        }

        if (innerRadiusRatio >= outerRadiusRatio)
        {
            throw new ArgumentOutOfRangeException(nameof(innerRadiusRatio), "Inner radius ratio must be smaller than the outer radius ratio.");
        }

        Sign = sign;
        StartAngle = startAngle;
        EndAngle = endAngle;
        InnerRadiusRatio = innerRadiusRatio;
        OuterRadiusRatio = outerRadiusRatio;
    }

    /// <summary>
    /// Gets the zodiac sign represented by the sector.
    /// </summary>
    public ZodiacSign Sign { get; }

    /// <summary>
    /// Gets the inclusive start angle.
    /// </summary>
    public AngularPosition StartAngle { get; }

    /// <summary>
    /// Gets the exclusive end angle.
    /// </summary>
    public AngularPosition EndAngle { get; }

    /// <summary>
    /// Gets the normalized inner radius.
    /// </summary>
    public double InnerRadiusRatio { get; }

    /// <summary>
    /// Gets the normalized outer radius.
    /// </summary>
    public double OuterRadiusRatio { get; }
}
