namespace NoxAeterna.Domain.Tarot;

/// <summary>Defines an immutable semantic Tarot deck.</summary>
public sealed record TarotDeckDefinition
{
    /// <summary>Initializes a semantic deck from an explicitly ordered card set.</summary>
    public TarotDeckDefinition(TarotDeckId id, IEnumerable<TarotCardDefinition> cards)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(cards);

        var copiedCards = cards.ToArray();
        if (copiedCards.Length == 0)
        {
            throw new ArgumentException("A Tarot deck must contain at least one card.", nameof(cards));
        }

        if (copiedCards.Any(static card => card is null))
        {
            throw new ArgumentException("A Tarot deck must not contain null card definitions.", nameof(cards));
        }

        var duplicateId = copiedCards
            .GroupBy(static card => card.Id)
            .FirstOrDefault(static group => group.Count() > 1);
        if (duplicateId is not null)
        {
            throw new ArgumentException($"Duplicate Tarot card ID '{duplicateId.Key}'.", nameof(cards));
        }

        var duplicateMinorIdentity = copiedCards
            .Where(static card => card.Arcana == TarotArcana.Minor)
            .GroupBy(static card => (card.Suit, card.Rank))
            .FirstOrDefault(static group => group.Count() > 1);
        if (duplicateMinorIdentity is not null)
        {
            throw new ArgumentException("Duplicate Minor Arcana suit/rank identity.", nameof(cards));
        }

        Id = id;
        Cards = Array.AsReadOnly(copiedCards);
    }

    /// <summary>Gets the stable semantic deck identity.</summary>
    public TarotDeckId Id { get; }

    /// <summary>Gets the cards in deterministic deck-definition order.</summary>
    public IReadOnlyList<TarotCardDefinition> Cards { get; }
}
