using NodaTime;

namespace NoxAeterna.Domain.Tarot;

/// <summary>Identifies how a drawn Tarot card is oriented.</summary>
public enum TarotCardOrientation
{
    /// <summary>The card is upright.</summary>
    Upright,

    /// <summary>The card is reversed.</summary>
    Reversed
}

/// <summary>Controls which orientations a draw may produce.</summary>
public enum TarotOrientationPolicy
{
    /// <summary>Every card is upright.</summary>
    UprightOnly,

    /// <summary>Each card may be upright or reversed.</summary>
    UprightAndReversed
}

/// <summary>Assigns one drawn semantic card and orientation to one spread position.</summary>
public sealed record TarotDrawnCard
{
    /// <summary>Initializes one drawn-card assignment.</summary>
    public TarotDrawnCard(
        TarotSpreadPositionId positionId,
        TarotCardDefinition card,
        TarotCardOrientation orientation)
    {
        ArgumentNullException.ThrowIfNull(positionId);
        ArgumentNullException.ThrowIfNull(card);
        ArgumentOutOfRangeException.ThrowIfNotEqual(Enum.IsDefined(orientation), true, nameof(orientation));

        PositionId = positionId;
        Card = card;
        Orientation = orientation;
    }

    /// <summary>Gets the assigned spread position.</summary>
    public TarotSpreadPositionId PositionId { get; }

    /// <summary>Gets the drawn semantic card.</summary>
    public TarotCardDefinition Card { get; }

    /// <summary>Gets the drawn orientation.</summary>
    public TarotCardOrientation Orientation { get; }
}

/// <summary>Represents an immutable in-memory Tarot reading.</summary>
public sealed record TarotReading
{
    /// <summary>Initializes a completed reading.</summary>
    public TarotReading(
        TarotDeckId deckId,
        TarotSpreadId spreadId,
        Instant drawnAt,
        IEnumerable<TarotDrawnCard> cards)
    {
        ArgumentNullException.ThrowIfNull(deckId);
        ArgumentNullException.ThrowIfNull(spreadId);
        ArgumentNullException.ThrowIfNull(cards);

        var copiedCards = cards.ToArray();
        if (copiedCards.Length == 0)
        {
            throw new ArgumentException("A Tarot reading must contain at least one drawn card.", nameof(cards));
        }

        if (copiedCards.Any(static card => card is null))
        {
            throw new ArgumentException("A Tarot reading must not contain null assignments.", nameof(cards));
        }

        if (copiedCards.Select(static card => card.PositionId).Distinct().Count() != copiedCards.Length)
        {
            throw new ArgumentException("A Tarot reading must not repeat spread positions.", nameof(cards));
        }

        if (copiedCards.Select(static card => card.Card.Id).Distinct().Count() != copiedCards.Length)
        {
            throw new ArgumentException("A Tarot reading must not repeat semantic cards.", nameof(cards));
        }

        DeckId = deckId;
        SpreadId = spreadId;
        DrawnAt = drawnAt;
        Cards = Array.AsReadOnly(copiedCards);
    }

    /// <summary>Gets the semantic deck identity.</summary>
    public TarotDeckId DeckId { get; }

    /// <summary>Gets the spread identity.</summary>
    public TarotSpreadId SpreadId { get; }

    /// <summary>Gets the timestamp supplied explicitly by the caller.</summary>
    public Instant DrawnAt { get; }

    /// <summary>Gets drawn assignments in spread-position order.</summary>
    public IReadOnlyList<TarotDrawnCard> Cards { get; }
}
