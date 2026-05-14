namespace NoxAeterna.Presentation.Theming;

/// <summary>
/// Represents a stable theme identifier.
/// </summary>
public readonly record struct ThemeId
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ThemeId"/> struct.
    /// </summary>
    /// <param name="value">The theme identifier.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is empty or whitespace.</exception>
    public ThemeId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Theme identifier must not be empty.", nameof(value));
        }

        Value = value.Trim().ToLowerInvariant();
    }

    /// <summary>
    /// Gets the normalized theme identifier.
    /// </summary>
    public string Value { get; }

    /// <inheritdoc />
    public override string ToString() => Value;
}
