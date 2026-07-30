using NoxAeterna.Domain.Astrology;

namespace NoxAeterna.Astronomy.Calculation;

/// <summary>
/// Represents either a complete house calculation or a typed unavailable result.
/// </summary>
public sealed record HouseCalculationResult
{
    private HouseCalculationResult(
        HouseSystem houseSystem,
        NatalHouses? houses,
        HouseCalculationFailureReason failureReason,
        string? sourceMetadata)
    {
        HouseSystem = houseSystem;
        Houses = houses;
        FailureReason = failureReason;
        SourceMetadata = string.IsNullOrWhiteSpace(sourceMetadata) ? null : sourceMetadata.Trim();
    }

    /// <summary>
    /// Gets the requested house system.
    /// </summary>
    public HouseSystem HouseSystem { get; }

    /// <summary>
    /// Gets whether a complete result is available.
    /// </summary>
    public bool IsAvailable => Houses is not null;

    /// <summary>
    /// Gets the complete houses when available.
    /// </summary>
    public NatalHouses? Houses { get; }

    /// <summary>
    /// Gets the typed failure reason when the result is unavailable.
    /// </summary>
    public HouseCalculationFailureReason FailureReason { get; }

    /// <summary>
    /// Gets optional provider-neutral source metadata.
    /// </summary>
    public string? SourceMetadata { get; }

    /// <summary>
    /// Creates a successful calculation result.
    /// </summary>
    public static HouseCalculationResult Available(NatalHouses houses)
    {
        ArgumentNullException.ThrowIfNull(houses);

        if (!houses.IsAvailable)
        {
            throw new ArgumentException("A successful result requires complete house data.", nameof(houses));
        }

        return new HouseCalculationResult(
            houses.HouseSystem,
            houses,
            HouseCalculationFailureReason.None,
            houses.SourceMetadata);
    }

    /// <summary>
    /// Creates a typed unavailable calculation result without fake house data.
    /// </summary>
    public static HouseCalculationResult Unavailable(
        HouseSystem houseSystem,
        HouseCalculationFailureReason failureReason,
        string? sourceMetadata = null)
    {
        if (failureReason == HouseCalculationFailureReason.None)
        {
            throw new ArgumentException("An unavailable result requires a failure reason.", nameof(failureReason));
        }

        return new HouseCalculationResult(houseSystem, null, failureReason, sourceMetadata);
    }
}
