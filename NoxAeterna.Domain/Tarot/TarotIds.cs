namespace NoxAeterna.Domain.Tarot;

/// <summary>
/// Identifies a semantic Tarot deck independently from its visual assets.
/// </summary>
public sealed record TarotDeckId
{
    /// <summary>Initializes a validated stable deck identifier.</summary>
    public TarotDeckId(string value) => Value = StableTarotId.Normalize(value, nameof(value));

    /// <summary>Gets the stable identifier value.</summary>
    public string Value { get; }

    /// <inheritdoc />
    public override string ToString() => Value;
}

/// <summary>
/// Identifies one semantic Tarot card independently from artwork, numbering, and display order.
/// </summary>
public sealed record TarotCardId
{
    /// <summary>Initializes a validated stable card identifier.</summary>
    public TarotCardId(string value) => Value = StableTarotId.Normalize(value, nameof(value));

    /// <summary>Gets the stable identifier value.</summary>
    public string Value { get; }

    /// <inheritdoc />
    public override string ToString() => Value;
}

/// <summary>Identifies a Tarot spread definition.</summary>
public sealed record TarotSpreadId
{
    /// <summary>Initializes a validated stable spread identifier.</summary>
    public TarotSpreadId(string value) => Value = StableTarotId.Normalize(value, nameof(value));

    /// <summary>Gets the stable identifier value.</summary>
    public string Value { get; }

    /// <inheritdoc />
    public override string ToString() => Value;
}

/// <summary>Identifies a semantic position within a Tarot spread.</summary>
public sealed record TarotSpreadPositionId
{
    /// <summary>Initializes a validated stable spread-position identifier.</summary>
    public TarotSpreadPositionId(string value) => Value = StableTarotId.Normalize(value, nameof(value));

    /// <summary>Gets the stable identifier value.</summary>
    public string Value { get; }

    /// <inheritdoc />
    public override string ToString() => Value;
}

internal static class StableTarotId
{
    public static string Normalize(string value, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(value, parameterName);

        var normalized = value.Trim();
        if (normalized.Length == 0)
        {
            throw new ArgumentException("A stable Tarot identifier must not be empty.", parameterName);
        }

        var previousWasSeparator = true;
        foreach (var character in normalized)
        {
            var isLowerAsciiLetter = character is >= 'a' and <= 'z';
            var isDigit = character is >= '0' and <= '9';
            var isSeparator = character is '.' or '-';

            if ((!isLowerAsciiLetter && !isDigit && !isSeparator) || (isSeparator && previousWasSeparator))
            {
                throw new ArgumentException(
                    "A stable Tarot identifier may contain lowercase ASCII letters, digits, and single '.' or '-' separators.",
                    parameterName);
            }

            previousWasSeparator = isSeparator;
        }

        if (previousWasSeparator)
        {
            throw new ArgumentException("A stable Tarot identifier must not end with a separator.", parameterName);
        }

        return normalized;
    }
}
