namespace NoxAeterna.Domain.Astrology;

/// <summary>
/// Provides core angular calculations for major astrological aspects.
/// </summary>
public static class AspectMath
{
    /// <summary>
    /// Gets the default orb in degrees for the supported major aspects.
    /// </summary>
    public const double DefaultOrbDegrees = 6d;

    /// <summary>
    /// Calculates the smallest angular distance between two zodiac longitudes.
    /// </summary>
    /// <param name="first">The first longitude.</param>
    /// <param name="second">The second longitude.</param>
    /// <returns>The smallest angular distance in degrees.</returns>
    public static double CalculateAngularDelta(ZodiacLongitude first, ZodiacLongitude second)
    {
        var absoluteDifference = Math.Abs(first.Degrees - second.Degrees);
        return Math.Min(absoluteDifference, 360d - absoluteDifference);
    }

    /// <summary>
    /// Determines whether two zodiac longitudes match the specified aspect within the provided orb.
    /// </summary>
    /// <param name="first">The first longitude.</param>
    /// <param name="second">The second longitude.</param>
    /// <param name="aspectType">The aspect to test.</param>
    /// <param name="orbDegrees">The allowed orb in degrees.</param>
    /// <returns><see langword="true"/> when the aspect is matched within orb; otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="orbDegrees"/> is not a finite non-negative number.</exception>
    public static bool IsMatch(
        ZodiacLongitude first,
        ZodiacLongitude second,
        AspectType aspectType,
        double orbDegrees = DefaultOrbDegrees)
    {
        if (double.IsNaN(orbDegrees) || double.IsInfinity(orbDegrees) || orbDegrees < 0d)
        {
            throw new ArgumentOutOfRangeException(nameof(orbDegrees), "Orb must be a finite non-negative number.");
        }

        var delta = CalculateAngularDelta(first, second);
        var exactAngle = (double)aspectType;

        return Math.Abs(delta - exactAngle) <= orbDegrees;
    }
}
