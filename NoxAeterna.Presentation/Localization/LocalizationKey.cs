namespace NoxAeterna.Presentation.Localization;

/// <summary>
/// Represents a stable localization key for human-facing text.
/// </summary>
public readonly record struct LocalizationKey
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LocalizationKey"/> struct.
    /// </summary>
    /// <param name="value">The localization key value.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is empty or whitespace.</exception>
    public LocalizationKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Localization key must not be empty.", nameof(value));
        }

        Value = value.Trim();
    }

    /// <summary>
    /// Gets the localization key value.
    /// </summary>
    public string Value { get; }

    /// <inheritdoc />
    public override string ToString() => Value;
}
