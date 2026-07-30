namespace NoxAeterna.Astronomy.Calculation;

/// <summary>
/// Describes a provider-neutral reason why a requested house calculation is unavailable.
/// </summary>
public enum HouseCalculationFailureReason
{
    /// <summary>
    /// No failure occurred.
    /// </summary>
    None,

    /// <summary>
    /// The selected system cannot produce a result for the supplied geographic data.
    /// </summary>
    UnsupportedGeographicConditions,

    /// <summary>
    /// The provider returned incomplete, non-finite, or internally inconsistent output.
    /// </summary>
    InvalidProviderOutput
}
