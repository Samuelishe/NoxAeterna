using NoxAeterna.Domain.Tarot;
using NoxAeterna.Interpretation.Tarot.Contracts;
using NoxAeterna.Interpretation.Tarot.Serialization;
using NoxAeterna.Interpretation.Tarot.Validation;

namespace NoxAeterna.Interpretation.Tarot.Keys;

public sealed record TarotSingleCardKey
{
    internal TarotSingleCardKey(TarotCardId cardId, TarotCardOrientation orientation) =>
        (CardId, Orientation) = (cardId, orientation);

    public TarotCardId CardId { get; }
    public TarotCardOrientation Orientation { get; }
}

public sealed record TarotOrientedPairKey
{
    internal TarotOrientedPairKey(TarotCardId cardAId, TarotCardId cardBId, TarotOrientedPairState orientationState) =>
        (CardAId, CardBId, OrientationState) = (cardAId, cardBId, orientationState);

    public TarotCardId CardAId { get; }
    public TarotCardId CardBId { get; }
    public TarotOrientedPairState OrientationState { get; }
}

public sealed record TarotThreeCardPositionKey
{
    internal TarotThreeCardPositionKey(
        TarotThreeCardPosition position,
        TarotCardId cardId,
        TarotCardOrientation orientation) => (Position, CardId, Orientation) = (position, cardId, orientation);

    public TarotThreeCardPosition Position { get; }
    public TarotCardId CardId { get; }
    public TarotCardOrientation Orientation { get; }
}

public sealed record TarotSynthesisResourceKey
{
    internal TarotSynthesisResourceKey(TarotSynthesisResourceType resourceType, TarotSynthesisResourceId resourceId) =>
        (ResourceType, ResourceId) = (resourceType, resourceId);

    public TarotSynthesisResourceType ResourceType { get; }
    public TarotSynthesisResourceId ResourceId { get; }
}

public sealed record TarotCanonicalPair
{
    internal TarotCanonicalPair(
        TarotCardId cardAId,
        TarotCardId cardBId,
        TarotOrientedPairState orientationState) =>
        (CardAId, CardBId, OrientationState) = (cardAId, cardBId, orientationState);

    public TarotCardId CardAId { get; }
    public TarotCardId CardBId { get; }
    public TarotOrientedPairState OrientationState { get; }
}

/// <summary>Builds and parses exact canonical interpretation keys without normalization.</summary>
public static class TarotInterpretationKeys
{
    public static string CreateSingleCard(TarotCardId cardId, TarotCardOrientation orientation)
    {
        ArgumentNullException.ThrowIfNull(cardId);
        EnsureDefined(orientation, nameof(orientation));
        return $"{cardId.Value}|{TarotSchemaText.Get(orientation, TarotSchemaText.CardOrientations)}";
    }

    public static string CreateOrientedPair(
        TarotCardId cardAId,
        TarotCardId cardBId,
        TarotOrientedPairState orientationState)
    {
        ArgumentNullException.ThrowIfNull(cardAId);
        ArgumentNullException.ThrowIfNull(cardBId);
        EnsureDefined(orientationState, nameof(orientationState));
        if (StringComparer.Ordinal.Compare(cardAId.Value, cardBId.Value) >= 0)
        {
            throw new ArgumentException("Oriented-pair keys require distinct canonical card order.", nameof(cardBId));
        }

        return $"{cardAId.Value}__{cardBId.Value}|{TarotSchemaText.Get(orientationState, TarotSchemaText.PairStates)}";
    }

    public static string CreateThreeCardPosition(
        TarotThreeCardPosition position,
        TarotCardId cardId,
        TarotCardOrientation orientation)
    {
        EnsureDefined(position, nameof(position));
        ArgumentNullException.ThrowIfNull(cardId);
        EnsureDefined(orientation, nameof(orientation));
        return $"position|{TarotSchemaText.Get(position, TarotSchemaText.Positions)}|{cardId.Value}|{TarotSchemaText.Get(orientation, TarotSchemaText.CardOrientations)}";
    }

    public static string CreateSynthesisResource(
        TarotSynthesisResourceType resourceType,
        TarotSynthesisResourceId resourceId)
    {
        EnsureDefined(resourceType, nameof(resourceType));
        ArgumentNullException.ThrowIfNull(resourceId);
        return $"synthesis|{TarotSchemaText.Get(resourceType, TarotSchemaText.SynthesisResourceTypes)}|{resourceId.Value}";
    }

    public static TarotValidationResult<TarotSingleCardKey> ParseSingleCard(string key)
    {
        var diagnostics = new TarotDiagnosticBag();
        var parts = Split(key, '|', 2, diagnostics);
        var cardId = parts is null ? null : ParseCardId(parts[0], "key.cardId", diagnostics);
        var orientation = parts is null
            ? null
            : ParseEnum(parts[1], TarotSchemaText.CardOrientations, "key.orientation", diagnostics);
        var value = diagnostics.HasErrors || cardId is null || orientation is null
            ? null
            : new TarotSingleCardKey(cardId, orientation.Value);
        return TarotValidationResult<TarotSingleCardKey>.Create(value, diagnostics.Items);
    }

    public static TarotValidationResult<TarotOrientedPairKey> ParseOrientedPair(string key)
    {
        var diagnostics = new TarotDiagnosticBag();
        var sections = Split(key, '|', 2, diagnostics);
        string[]? cards = null;
        if (sections is not null)
        {
            cards = sections[0].Split("__", StringSplitOptions.None);
            if (cards.Length != 2 || cards.Any(string.IsNullOrEmpty))
            {
                diagnostics.Error("key.format", "key", "An oriented-pair key requires two card IDs separated by '__'.");
                cards = null;
            }
        }

        var cardA = cards is null ? null : ParseCardId(cards[0], "key.cardAId", diagnostics);
        var cardB = cards is null ? null : ParseCardId(cards[1], "key.cardBId", diagnostics);
        if (cardA is not null && cardB is not null && StringComparer.Ordinal.Compare(cardA.Value, cardB.Value) >= 0)
        {
            diagnostics.Error("pair.noncanonical", "key", "Pair card IDs are not in distinct ordinal order.");
        }

        var state = sections is null
            ? null
            : ParseEnum(sections[1], TarotSchemaText.PairStates, "key.orientationState", diagnostics);
        var value = diagnostics.HasErrors || cardA is null || cardB is null || state is null
            ? null
            : new TarotOrientedPairKey(cardA, cardB, state.Value);
        return TarotValidationResult<TarotOrientedPairKey>.Create(value, diagnostics.Items);
    }

    public static TarotValidationResult<TarotThreeCardPositionKey> ParseThreeCardPosition(string key)
    {
        var diagnostics = new TarotDiagnosticBag();
        var parts = Split(key, '|', 4, diagnostics);
        if (parts is not null && !string.Equals(parts[0], "position", StringComparison.Ordinal))
        {
            diagnostics.Error("key.prefix", "key", "A three-card position key must start with 'position'.");
        }

        var position = parts is null ? null : ParseEnum(parts[1], TarotSchemaText.Positions, "key.position", diagnostics);
        var cardId = parts is null ? null : ParseCardId(parts[2], "key.cardId", diagnostics);
        var orientation = parts is null ? null : ParseEnum(parts[3], TarotSchemaText.CardOrientations, "key.orientation", diagnostics);
        var value = diagnostics.HasErrors || position is null || cardId is null || orientation is null
            ? null
            : new TarotThreeCardPositionKey(position.Value, cardId, orientation.Value);
        return TarotValidationResult<TarotThreeCardPositionKey>.Create(value, diagnostics.Items);
    }

    public static TarotValidationResult<TarotSynthesisResourceKey> ParseSynthesisResource(string key)
    {
        var diagnostics = new TarotDiagnosticBag();
        var parts = Split(key, '|', 3, diagnostics);
        if (parts is not null && !string.Equals(parts[0], "synthesis", StringComparison.Ordinal))
        {
            diagnostics.Error("key.prefix", "key", "A synthesis key must start with 'synthesis'.");
        }

        var type = parts is null
            ? null
            : ParseEnum(parts[1], TarotSchemaText.SynthesisResourceTypes, "key.resourceType", diagnostics);
        TarotSynthesisResourceId? resourceId = null;
        if (parts is not null)
        {
            try
            {
                resourceId = new TarotSynthesisResourceId(parts[2]);
            }
            catch (ArgumentException exception)
            {
                diagnostics.Error("key.resource-id", "key.resourceId", exception.Message);
            }
        }

        var value = diagnostics.HasErrors || type is null || resourceId is null
            ? null
            : new TarotSynthesisResourceKey(type.Value, resourceId);
        return TarotValidationResult<TarotSynthesisResourceKey>.Create(value, diagnostics.Items);
    }

    public static TarotValidationResult<TarotCanonicalPair> CanonicalizePair(
        TarotCardId firstCardId,
        TarotCardOrientation firstOrientation,
        TarotCardId secondCardId,
        TarotCardOrientation secondOrientation)
    {
        ArgumentNullException.ThrowIfNull(firstCardId);
        ArgumentNullException.ThrowIfNull(secondCardId);
        var diagnostics = new TarotDiagnosticBag();
        if (!Enum.IsDefined(firstOrientation) || !Enum.IsDefined(secondOrientation))
        {
            diagnostics.Error("orientation.unknown", "orientation", "Both card orientations must be defined.");
        }

        var comparison = StringComparer.Ordinal.Compare(firstCardId.Value, secondCardId.Value);
        if (comparison == 0)
        {
            diagnostics.Error("pair.self", "cards", "A canonical pair requires two distinct cards.");
        }

        TarotCanonicalPair? pair = null;
        if (!diagnostics.HasErrors)
        {
            pair = comparison < 0
                ? new TarotCanonicalPair(firstCardId, secondCardId, ToPairState(firstOrientation, secondOrientation))
                : new TarotCanonicalPair(secondCardId, firstCardId, ToPairState(secondOrientation, firstOrientation));
        }

        return TarotValidationResult<TarotCanonicalPair>.Create(pair, diagnostics.Items);
    }

    private static TarotOrientedPairState ToPairState(TarotCardOrientation a, TarotCardOrientation b) => (a, b) switch
    {
        (TarotCardOrientation.Upright, TarotCardOrientation.Upright) => TarotOrientedPairState.UprightUpright,
        (TarotCardOrientation.Upright, TarotCardOrientation.Reversed) => TarotOrientedPairState.UprightReversed,
        (TarotCardOrientation.Reversed, TarotCardOrientation.Upright) => TarotOrientedPairState.ReversedUpright,
        (TarotCardOrientation.Reversed, TarotCardOrientation.Reversed) => TarotOrientedPairState.ReversedReversed,
        _ => throw new ArgumentOutOfRangeException(nameof(a))
    };

    private static string[]? Split(string key, char separator, int expectedCount, TarotDiagnosticBag diagnostics)
    {
        if (string.IsNullOrEmpty(key) || key != key.Trim())
        {
            diagnostics.Error("key.empty", "key", "A canonical key must not be empty or padded.");
            return null;
        }

        var parts = key.Split(separator, StringSplitOptions.None);
        if (parts.Length != expectedCount || parts.Any(string.IsNullOrEmpty))
        {
            diagnostics.Error("key.format", "key", $"A canonical key requires exactly {expectedCount} non-empty segments.");
            return null;
        }

        return parts;
    }

    private static TarotCardId? ParseCardId(string value, string field, TarotDiagnosticBag diagnostics)
    {
        try
        {
            var id = new TarotCardId(value);
            if (!string.Equals(id.Value, value, StringComparison.Ordinal))
            {
                diagnostics.Error("key.normalized", field, "Canonical keys do not normalize card IDs.");
                return null;
            }

            return id;
        }
        catch (ArgumentException exception)
        {
            diagnostics.Error("key.card-id", field, exception.Message);
            return null;
        }
    }

    private static TEnum? ParseEnum<TEnum>(
        string value,
        IReadOnlyDictionary<TEnum, string> mapping,
        string field,
        TarotDiagnosticBag diagnostics)
        where TEnum : struct, Enum
    {
        if (TarotSchemaText.TryParse(value, mapping, out var result))
        {
            return result;
        }

        diagnostics.Error("key.enum", field, $"Unknown {typeof(TEnum).Name} value.");
        return null;
    }

    private static void EnsureDefined<TEnum>(TEnum value, string parameterName)
        where TEnum : struct, Enum
    {
        if (!Enum.IsDefined(value))
        {
            throw new ArgumentOutOfRangeException(parameterName, value, "Unknown schema enum value.");
        }
    }
}
