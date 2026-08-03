namespace NoxAeterna.Domain.Tarot;

/// <summary>Defines one language-neutral position in a Tarot spread.</summary>
public sealed record TarotSpreadPositionDefinition
{
    /// <summary>Initializes a spread position.</summary>
    public TarotSpreadPositionDefinition(TarotSpreadPositionId id)
    {
        ArgumentNullException.ThrowIfNull(id);
        Id = id;
    }

    /// <summary>Gets the stable internal position identity.</summary>
    public TarotSpreadPositionId Id { get; }
}

/// <summary>Defines an immutable ordered Tarot spread without presentation geometry.</summary>
public sealed record TarotSpreadDefinition
{
    /// <summary>Initializes a spread from an explicitly ordered position set.</summary>
    public TarotSpreadDefinition(
        TarotSpreadId id,
        IEnumerable<TarotSpreadPositionDefinition> positions)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(positions);

        var copiedPositions = positions.ToArray();
        if (copiedPositions.Length == 0)
        {
            throw new ArgumentException("A Tarot spread must contain at least one position.", nameof(positions));
        }

        if (copiedPositions.Any(static position => position is null))
        {
            throw new ArgumentException("A Tarot spread must not contain null positions.", nameof(positions));
        }

        var duplicatePosition = copiedPositions
            .GroupBy(static position => position.Id)
            .FirstOrDefault(static group => group.Count() > 1);
        if (duplicatePosition is not null)
        {
            throw new ArgumentException($"Duplicate Tarot spread position ID '{duplicatePosition.Key}'.", nameof(positions));
        }

        Id = id;
        Positions = Array.AsReadOnly(copiedPositions);
    }

    /// <summary>Gets the stable spread identity.</summary>
    public TarotSpreadId Id { get; }

    /// <summary>Gets the positions in deterministic draw order.</summary>
    public IReadOnlyList<TarotSpreadPositionDefinition> Positions { get; }
}
