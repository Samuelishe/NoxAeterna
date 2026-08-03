using NodaTime;
using NoxAeterna.Domain.Tarot;

namespace NoxAeterna.Tests.Tarot;

public sealed class TarotDrawEngineTests
{
    [Fact]
    public void Draw_InjectedSequenceProducesDeterministicWithoutReplacementAssignments()
    {
        var random = new RecordingRandomSource(2, 1, 0, 0, 1, 1);
        var timestamp = Instant.FromUtc(2026, 8, 3, 9, 30);

        var result = new TarotDrawEngine(random).Draw(
            CreateFourCardDeck(),
            StandardTarotSpreads.ThreeCards,
            TarotOrientationPolicy.UprightAndReversed,
            timestamp);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Failure);
        var reading = Assert.IsType<TarotReading>(result.Reading);
        Assert.Equal("test-deck", reading.DeckId.Value);
        Assert.Equal("three-cards", reading.SpreadId.Value);
        Assert.Equal(timestamp, reading.DrawnAt);
        Assert.Equal(new[] { "past", "present", "future" }, reading.Cards.Select(card => card.PositionId.Value));
        Assert.Equal(new[] { "major.gamma", "major.alpha", "major.beta" }, reading.Cards.Select(card => card.Card.Id.Value));
        Assert.Equal(
            new[] { TarotCardOrientation.Reversed, TarotCardOrientation.Upright, TarotCardOrientation.Reversed },
            reading.Cards.Select(card => card.Orientation));
        Assert.Equal(reading.Cards.Count, reading.Cards.Select(card => card.Card.Id).Distinct().Count());
        Assert.Equal(new[] { 4, 2, 3, 2, 2, 2 }, random.RequestedUpperBounds);
        Assert.Throws<NotSupportedException>(() =>
            ((ICollection<TarotDrawnCard>)reading.Cards).Add(reading.Cards[0]));
    }

    [Fact]
    public void Draw_UprightOnlyNeverConsumesOrientationRandomnessOrProducesReversedCards()
    {
        var random = new RecordingRandomSource(1, 0, 0);

        var result = new TarotDrawEngine(random).Draw(
            CreateFourCardDeck(),
            StandardTarotSpreads.ThreeCards,
            TarotOrientationPolicy.UprightOnly,
            Instant.FromUnixTimeTicks(123));

        var reading = Assert.IsType<TarotReading>(result.Reading);
        Assert.All(reading.Cards, card => Assert.Equal(TarotCardOrientation.Upright, card.Orientation));
        Assert.Equal(new[] { 4, 3, 2 }, random.RequestedUpperBounds);
        Assert.Equal(0, random.RemainingValues);
    }

    [Fact]
    public void Draw_InsufficientDeckReturnsTypedFailureWithoutUsingRandomness()
    {
        var random = new RecordingRandomSource();
        var deck = new TarotDeckDefinition(
            new TarotDeckId("small-deck"),
            [TarotCardDefinition.CreateMajor(new TarotCardId("major.only"))]);

        var result = new TarotDrawEngine(random).Draw(
            deck,
            StandardTarotSpreads.ThreeCards,
            TarotOrientationPolicy.UprightOnly,
            Instant.FromUnixTimeTicks(456));

        Assert.False(result.IsSuccess);
        Assert.Null(result.Reading);
        var failure = Assert.IsType<TarotDrawFailure>(result.Failure);
        Assert.Equal(TarotDrawFailureReason.InsufficientDeckSize, failure.Reason);
        Assert.Equal(3, failure.RequiredCardCount);
        Assert.Equal(1, failure.AvailableCardCount);
        Assert.Empty(random.RequestedUpperBounds);
    }

    [Fact]
    public void Draw_DeckExactlyMatchingSpreadSizeSucceeds()
    {
        var deck = new TarotDeckDefinition(
            new TarotDeckId("exact-deck"),
            [
                TarotCardDefinition.CreateMajor(new TarotCardId("major.alpha")),
                TarotCardDefinition.CreateMajor(new TarotCardId("major.beta")),
                TarotCardDefinition.CreateMajor(new TarotCardId("major.gamma"))
            ]);

        var result = new TarotDrawEngine(new RecordingRandomSource(0, 0, 0)).Draw(
            deck,
            StandardTarotSpreads.ThreeCards,
            TarotOrientationPolicy.UprightOnly,
            Instant.FromUnixTimeTicks(457));

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Reading);
        Assert.Null(result.Failure);
        Assert.Equal(3, result.Reading!.Cards.Count);
    }

    [Fact]
    public void DrawFailure_RejectsInconsistentInsufficientDeckCounts()
    {
        Assert.Throws<ArgumentException>(() => new TarotDrawFailure(
            TarotDrawFailureReason.InsufficientDeckSize,
            requiredCardCount: 3,
            availableCardCount: 3));
    }

    [Fact]
    public void Draw_ReplayingSameFakeSequenceReproducesResult()
    {
        var timestamp = Instant.FromUtc(2026, 8, 3, 10, 15);
        var first = new TarotDrawEngine(new RecordingRandomSource(3, 0, 1, 1, 0, 0)).Draw(
            CreateFourCardDeck(),
            StandardTarotSpreads.ThreeCards,
            TarotOrientationPolicy.UprightAndReversed,
            timestamp);
        var second = new TarotDrawEngine(new RecordingRandomSource(3, 0, 1, 1, 0, 0)).Draw(
            CreateFourCardDeck(),
            StandardTarotSpreads.ThreeCards,
            TarotOrientationPolicy.UprightAndReversed,
            timestamp);

        Assert.Equal(CreateSnapshot(first), CreateSnapshot(second));
    }

    [Fact]
    public void Draw_InvalidRandomIndexIsNotSilentlyMasked()
    {
        var engine = new TarotDrawEngine(new RecordingRandomSource(4));

        Assert.Throws<InvalidOperationException>(() => engine.Draw(
            CreateFourCardDeck(),
            StandardTarotSpreads.SingleCard,
            TarotOrientationPolicy.UprightOnly,
            Instant.FromUnixTimeTicks(789)));
    }

    private static TarotDeckDefinition CreateFourCardDeck() => new(
        new TarotDeckId("test-deck"),
        [
            TarotCardDefinition.CreateMajor(new TarotCardId("major.alpha")),
            TarotCardDefinition.CreateMajor(new TarotCardId("major.beta")),
            TarotCardDefinition.CreateMajor(new TarotCardId("major.gamma")),
            TarotCardDefinition.CreateMajor(new TarotCardId("major.delta"))
        ]);

    private static string[] CreateSnapshot(TarotDrawResult result)
    {
        var reading = Assert.IsType<TarotReading>(result.Reading);
        return reading.Cards
            .Select(card => $"{card.PositionId.Value}|{card.Card.Id.Value}|{card.Orientation}|{reading.DrawnAt.ToUnixTimeTicks()}")
            .ToArray();
    }

    private sealed class RecordingRandomSource(params int[] values) : ITarotRandomSource
    {
        private readonly Queue<int> values = new(values);

        public List<int> RequestedUpperBounds { get; } = [];

        public int RemainingValues => values.Count;

        public int NextIndex(int exclusiveUpperBound)
        {
            RequestedUpperBounds.Add(exclusiveUpperBound);
            return values.Dequeue();
        }
    }
}
