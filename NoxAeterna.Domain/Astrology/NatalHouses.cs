namespace NoxAeterna.Domain.Astrology;

/// <summary>
/// Represents an explicitly calculated or explicitly unavailable natal house structure.
/// </summary>
public sealed record NatalHouses
{
    private const double AngleToleranceDegrees = 0.000001d;

    private NatalHouses(
        HouseSystem houseSystem,
        NatalHousesAvailability availability,
        IReadOnlyList<HouseCusp> cusps,
        ChartAngles? angles,
        string? sourceMetadata)
    {
        HouseSystem = houseSystem;
        Availability = availability;
        Cusps = cusps;
        Angles = angles;
        SourceMetadata = string.IsNullOrWhiteSpace(sourceMetadata) ? null : sourceMetadata.Trim();
    }

    /// <summary>
    /// Gets the explicitly selected house system.
    /// </summary>
    public HouseSystem HouseSystem { get; }

    /// <summary>
    /// Gets the calculation availability.
    /// </summary>
    public NatalHousesAvailability Availability { get; }

    /// <summary>
    /// Gets whether complete house data is available.
    /// </summary>
    public bool IsAvailable => Availability == NatalHousesAvailability.Available;

    /// <summary>
    /// Gets the cusps in deterministic house-number order.
    /// </summary>
    public IReadOnlyList<HouseCusp> Cusps { get; }

    /// <summary>
    /// Gets the chart angles when houses are available.
    /// </summary>
    public ChartAngles? Angles { get; }

    /// <summary>
    /// Gets optional provider-neutral source metadata.
    /// </summary>
    public string? SourceMetadata { get; }

    /// <summary>
    /// Creates a complete natal house structure.
    /// </summary>
    public static NatalHouses CreateAvailable(
        HouseSystem houseSystem,
        IEnumerable<HouseCusp> cusps,
        ChartAngles angles,
        string? sourceMetadata = null)
    {
        ArgumentNullException.ThrowIfNull(cusps);
        ArgumentNullException.ThrowIfNull(angles);

        var orderedCusps = cusps.OrderBy(static cusp => cusp.HouseNumber).ToArray();

        if (orderedCusps.Length != 12)
        {
            throw new ArgumentException("A complete natal house structure must contain exactly twelve cusps.", nameof(cusps));
        }

        var duplicateHouse = orderedCusps
            .GroupBy(static cusp => cusp.HouseNumber)
            .FirstOrDefault(static group => group.Count() > 1);

        if (duplicateHouse is not null)
        {
            throw new ArgumentException($"Duplicate cusp detected for house {duplicateHouse.Key}.", nameof(cusps));
        }

        for (var index = 0; index < orderedCusps.Length; index++)
        {
            if (orderedCusps[index].HouseNumber.Value != index + 1)
            {
                throw new ArgumentException("House cusps must contain each house number from 1 through 12.", nameof(cusps));
            }
        }

        var ascendantDifference = CircularDifference(
            orderedCusps[0].Longitude.Degrees,
            angles.Ascendant.Degrees);

        if (ascendantDifference > AngleToleranceDegrees)
        {
            throw new ArgumentException("The first-house cusp must match the Ascendant.", nameof(cusps));
        }

        var midheavenDifference = CircularDifference(
            orderedCusps[9].Longitude.Degrees,
            angles.Midheaven.Degrees);

        if (midheavenDifference > AngleToleranceDegrees)
        {
            throw new ArgumentException("The tenth-house cusp must match the Midheaven.", nameof(cusps));
        }

        return new NatalHouses(
            houseSystem,
            NatalHousesAvailability.Available,
            Array.AsReadOnly(orderedCusps),
            angles,
            sourceMetadata);
    }

    /// <summary>
    /// Creates an explicit unavailable house result without fake cusps or angles.
    /// </summary>
    public static NatalHouses CreateUnavailable(
        HouseSystem houseSystem,
        NatalHousesAvailability availability,
        string? sourceMetadata = null)
    {
        if (availability == NatalHousesAvailability.Available)
        {
            throw new ArgumentException("Use CreateAvailable when house data is available.", nameof(availability));
        }

        return new NatalHouses(
            houseSystem,
            availability,
            Array.Empty<HouseCusp>(),
            angles: null,
            sourceMetadata);
    }

    private static double CircularDifference(double left, double right)
    {
        var difference = Math.Abs(left - right) % 360d;
        return Math.Min(difference, 360d - difference);
    }
}
