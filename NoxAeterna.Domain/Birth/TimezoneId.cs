namespace NoxAeterna.Domain.Birth;

/// <summary>
/// Represents an explicit IANA timezone identifier.
/// </summary>
public readonly record struct TimezoneId
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TimezoneId"/> struct.
    /// </summary>
    /// <param name="value">The IANA timezone identifier.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is empty or whitespace.</exception>
    public TimezoneId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Timezone ID must not be empty.", nameof(value));
        }

        Value = value.Trim();
    }

    /// <summary>
    /// Gets the IANA timezone identifier.
    /// </summary>
    public string Value { get; }

    /// <inheritdoc />
    public override string ToString() => Value;
}
