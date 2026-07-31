using NoxAeterna.Domain.Astrology;

namespace NoxAeterna.Geometry.Charts;

/// <summary>
/// Resolves exact zodiac longitudes against an ordered set of house cusps.
/// </summary>
public static class ChartHouseMembership
{
    /// <summary>
    /// Finds the house whose start-inclusive circular span contains the longitude.
    /// </summary>
    public static HouseNumber? Find(
        ZodiacLongitude longitude,
        IReadOnlyList<HouseCusp> cusps)
    {
        ArgumentNullException.ThrowIfNull(cusps);

        for (var index = 0; index < cusps.Count; index++)
        {
            var current = cusps[index];
            var next = cusps[(index + 1) % cusps.Count];
            var span = ZodiacLongitude.Normalize(next.Longitude.Degrees - current.Longitude.Degrees);
            var offset = ZodiacLongitude.Normalize(longitude.Degrees - current.Longitude.Degrees);

            if (offset < span || Math.Abs(offset) <= 1e-9)
            {
                return current.HouseNumber;
            }
        }

        return null;
    }

    /// <summary>
    /// Finds the house from prepared cusp geometry.
    /// </summary>
    public static HouseNumber? Find(
        ZodiacLongitude longitude,
        IReadOnlyList<HouseCuspGeometry> cusps)
    {
        ArgumentNullException.ThrowIfNull(cusps);

        for (var index = 0; index < cusps.Count; index++)
        {
            var current = cusps[index];
            var next = cusps[(index + 1) % cusps.Count];
            var span = ZodiacLongitude.Normalize(next.Longitude.Degrees - current.Longitude.Degrees);
            var offset = ZodiacLongitude.Normalize(longitude.Degrees - current.Longitude.Degrees);

            if (offset < span || Math.Abs(offset) <= 1e-9)
            {
                return current.HouseNumber;
            }
        }

        return null;
    }
}
