namespace NoxAeterna.Presentation.Localization;

/// <summary>
/// Represents an explicit language code used for localization selection.
/// </summary>
public readonly record struct LanguageCode
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LanguageCode"/> struct.
    /// </summary>
    /// <param name="value">The language code value.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is empty or whitespace.</exception>
    public LanguageCode(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Language code must not be empty.", nameof(value));
        }

        Value = value.Trim().ToLowerInvariant();
    }

    /// <summary>
    /// Gets the normalized language code value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Gets the neutral parent language code when the current value contains a region suffix.
    /// </summary>
    public LanguageCode? NeutralParent =>
        Value.Contains('-', StringComparison.Ordinal)
            ? new LanguageCode(Value[..Value.IndexOf('-', StringComparison.Ordinal)])
            : null;

    /// <inheritdoc />
    public override string ToString() => Value;
}
