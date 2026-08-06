using System.Text.RegularExpressions;

namespace NoxAeterna.Interpretation.Tarot.Contracts;

/// <summary>Identifies a frozen Tarot interpretation mode.</summary>
public enum TarotInterpretationMode
{
    SingleCard,
    TwoCards,
    ThreeCards,
    CelticCross
}

/// <summary>Attaches two orientations to canonical card A and card B.</summary>
public enum TarotOrientedPairState
{
    UprightUpright,
    UprightReversed,
    ReversedUpright,
    ReversedReversed
}

/// <summary>Identifies an ordered three-card position.</summary>
public enum TarotThreeCardPosition
{
    Past,
    Present,
    Future
}

/// <summary>Describes a non-visible reversed-card mechanism.</summary>
public enum TarotReversalMechanism
{
    Blocked,
    Delayed,
    Internalized,
    Excessive,
    Distorted,
    Resisted,
    Depleted
}

/// <summary>Identifies a typed three-card synthesis resource.</summary>
public enum TarotSynthesisResourceType
{
    ThreeCardPosition,
    TrajectoryProfile,
    SynthesisFragment,
    RelationLabel
}

/// <summary>Identifies a same-locale mode dependency.</summary>
public enum TarotModuleDependency
{
    OrientedPairs,
    ThreeCardPositions,
    ThreeCardSynthesis
}

/// <summary>Identifies a generated interpretation corpus.</summary>
public enum TarotInterpretationCorpus
{
    SingleCard,
    OrientedPairs,
    ThreeCards
}

/// <summary>Identifies a future internal no-content reason without user-facing wording.</summary>
public enum TarotNoContentReason
{
    PackUnavailable,
    NoReadyLocale,
    BrokenReadyModule,
    UnsupportedMode,
    ValidationFailed
}

/// <summary>Represents a narrow lowercase ASCII locale tag used by pack schemas.</summary>
public sealed record TarotInterpretationLocale
{
    private static readonly Regex Pattern = new(
        "^[a-z]{2,8}(?:-[a-z0-9]{1,8})*$",
        RegexOptions.CultureInvariant | RegexOptions.NonBacktracking);

    public TarotInterpretationLocale(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (!Pattern.IsMatch(value))
        {
            throw new ArgumentException(
                "An interpretation locale must be a lowercase ASCII language tag.",
                nameof(value));
        }

        Value = value;
    }

    public string Value { get; }

    public override string ToString() => Value;
}

/// <summary>Identifies one language-neutral tag concept.</summary>
public sealed record TarotTagConceptId
{
    public TarotTagConceptId(string value) => Value = TarotSchemaId.Validate(value, nameof(value));

    public string Value { get; }

    public override string ToString() => Value;
}

/// <summary>Identifies one language-neutral synthesis resource.</summary>
public sealed record TarotSynthesisResourceId
{
    public TarotSynthesisResourceId(string value) => Value = TarotSchemaId.Validate(value, nameof(value));

    public string Value { get; }

    public override string ToString() => Value;
}

/// <summary>Represents a validated package-relative path using forward slashes.</summary>
public sealed record TarotPackageRelativePath
{
    public TarotPackageRelativePath(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value.Length == 0 || value != value.Trim() || value.Contains('\\') || value.StartsWith('/') ||
            value.EndsWith('/') || value.Contains("//", StringComparison.Ordinal) ||
            value.Split('/').Any(segment => segment is "" or "." or "..") ||
            Uri.TryCreate(value, UriKind.Absolute, out _))
        {
            throw new ArgumentException("A package path must be a safe relative '/' path.", nameof(value));
        }

        Value = value;
    }

    public string Value { get; }

    public override string ToString() => Value;
}

/// <summary>Represents an exact lowercase SHA-256 value.</summary>
public sealed record TarotSha256
{
    public TarotSha256(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value.Length != 64 || value.Any(character =>
                character is not (>= '0' and <= '9') and not (>= 'a' and <= 'f')))
        {
            throw new ArgumentException("A SHA-256 value must contain exactly 64 lowercase hexadecimal characters.", nameof(value));
        }

        Value = value;
    }

    public string Value { get; }

    public override string ToString() => Value;
}

internal static class TarotSchemaId
{
    public static string Validate(string value, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(value, parameterName);
        if (value.Length == 0 || value != value.Trim())
        {
            throw new ArgumentException("A schema identifier must not be empty or padded.", parameterName);
        }

        var previousWasSeparator = true;
        foreach (var character in value)
        {
            var valid = character is >= 'a' and <= 'z' or >= '0' and <= '9';
            var separator = character is '.' or '-';
            if ((!valid && !separator) || (separator && previousWasSeparator))
            {
                throw new ArgumentException("A schema identifier must use lowercase ASCII segments.", parameterName);
            }

            previousWasSeparator = separator;
        }

        if (previousWasSeparator)
        {
            throw new ArgumentException("A schema identifier must not end with a separator.", parameterName);
        }

        return value;
    }
}
