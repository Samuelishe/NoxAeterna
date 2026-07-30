namespace NoxAeterna.Domain.Astrology;

/// <summary>
/// Represents a validated astrological house number.
/// </summary>
public sealed record HouseNumber : IComparable<HouseNumber>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HouseNumber"/> struct.
    /// </summary>
    /// <param name="value">The house number in the inclusive range 1 through 12.</param>
    public HouseNumber(int value)
    {
        if (value is < 1 or > 12)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "House number must be in the range [1, 12].");
        }

        Value = value;
    }

    /// <summary>
    /// Gets the numeric house identifier.
    /// </summary>
    public int Value { get; }

    /// <inheritdoc />
    public int CompareTo(HouseNumber? other) =>
        other is null ? 1 : Value.CompareTo(other.Value);

    /// <inheritdoc />
    public override string ToString() => Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
}
