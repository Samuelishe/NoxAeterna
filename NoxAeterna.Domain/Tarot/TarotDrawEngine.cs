using NodaTime;

namespace NoxAeterna.Domain.Tarot;

/// <summary>Supplies bounded random indices without exposing a framework RNG to domain logic.</summary>
public interface ITarotRandomSource
{
    /// <summary>Returns an index in the range from zero through <paramref name="exclusiveUpperBound"/> minus one.</summary>
    int NextIndex(int exclusiveUpperBound);
}

/// <summary>Identifies a controlled Tarot draw failure.</summary>
public enum TarotDrawFailureReason
{
    /// <summary>The semantic deck contains fewer cards than the spread requires.</summary>
    InsufficientDeckSize
}

/// <summary>Describes a controlled Tarot draw failure.</summary>
public sealed record TarotDrawFailure
{
    /// <summary>Initializes a typed Tarot draw failure.</summary>
    public TarotDrawFailure(TarotDrawFailureReason reason, int requiredCardCount, int availableCardCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(requiredCardCount);
        ArgumentOutOfRangeException.ThrowIfNegative(availableCardCount);
        ArgumentOutOfRangeException.ThrowIfNotEqual(Enum.IsDefined(reason), true, nameof(reason));
        if (reason == TarotDrawFailureReason.InsufficientDeckSize && requiredCardCount <= availableCardCount)
        {
            throw new ArgumentException(
                "An insufficient-deck failure requires fewer available cards than required cards.",
                nameof(availableCardCount));
        }

        Reason = reason;
        RequiredCardCount = requiredCardCount;
        AvailableCardCount = availableCardCount;
    }

    /// <summary>Gets the typed failure reason.</summary>
    public TarotDrawFailureReason Reason { get; }

    /// <summary>Gets the number of cards required by the spread.</summary>
    public int RequiredCardCount { get; }

    /// <summary>Gets the number of cards available in the deck.</summary>
    public int AvailableCardCount { get; }
}

/// <summary>Represents either a completed reading or a controlled draw failure.</summary>
public sealed record TarotDrawResult
{
    private TarotDrawResult(TarotReading? reading, TarotDrawFailure? failure)
    {
        Reading = reading;
        Failure = failure;
    }

    /// <summary>Gets whether the draw completed successfully.</summary>
    public bool IsSuccess => Reading is not null;

    /// <summary>Gets the completed reading, when successful.</summary>
    public TarotReading? Reading { get; }

    /// <summary>Gets the controlled failure, when unsuccessful.</summary>
    public TarotDrawFailure? Failure { get; }

    internal static TarotDrawResult Succeeded(TarotReading reading) => new(reading, failure: null);

    internal static TarotDrawResult Failed(TarotDrawFailure failure) => new(reading: null, failure);
}

/// <summary>Draws an in-memory Tarot reading using injected randomness and no ambient clock.</summary>
public sealed class TarotDrawEngine
{
    private readonly ITarotRandomSource randomSource;

    /// <summary>Initializes the draw engine with a project-owned randomness source.</summary>
    public TarotDrawEngine(ITarotRandomSource randomSource)
    {
        ArgumentNullException.ThrowIfNull(randomSource);
        this.randomSource = randomSource;
    }

    /// <summary>Draws cards without replacement and assigns them in spread-position order.</summary>
    public TarotDrawResult Draw(
        TarotDeckDefinition deck,
        TarotSpreadDefinition spread,
        TarotOrientationPolicy orientationPolicy,
        Instant drawnAt)
    {
        ArgumentNullException.ThrowIfNull(deck);
        ArgumentNullException.ThrowIfNull(spread);
        ArgumentOutOfRangeException.ThrowIfNotEqual(Enum.IsDefined(orientationPolicy), true, nameof(orientationPolicy));

        if (deck.Cards.Count < spread.Positions.Count)
        {
            return TarotDrawResult.Failed(new TarotDrawFailure(
                TarotDrawFailureReason.InsufficientDeckSize,
                spread.Positions.Count,
                deck.Cards.Count));
        }

        var availableCards = deck.Cards.ToArray();
        var remainingCount = availableCards.Length;
        var assignments = new TarotDrawnCard[spread.Positions.Count];

        for (var positionIndex = 0; positionIndex < spread.Positions.Count; positionIndex++)
        {
            var selectedIndex = NextValidatedIndex(remainingCount);
            var selectedCard = availableCards[selectedIndex];

            remainingCount--;
            availableCards[selectedIndex] = availableCards[remainingCount];

            var orientation = orientationPolicy == TarotOrientationPolicy.UprightOnly
                ? TarotCardOrientation.Upright
                : NextValidatedIndex(2) == 0
                    ? TarotCardOrientation.Upright
                    : TarotCardOrientation.Reversed;

            assignments[positionIndex] = new TarotDrawnCard(
                spread.Positions[positionIndex].Id,
                selectedCard,
                orientation);
        }

        return TarotDrawResult.Succeeded(new TarotReading(
            deck.Id,
            spread.Id,
            drawnAt,
            assignments));
    }

    private int NextValidatedIndex(int exclusiveUpperBound)
    {
        var index = randomSource.NextIndex(exclusiveUpperBound);
        if (index < 0 || index >= exclusiveUpperBound)
        {
            throw new InvalidOperationException(
                $"The Tarot random source returned {index} for an exclusive upper bound of {exclusiveUpperBound}.");
        }

        return index;
    }
}
