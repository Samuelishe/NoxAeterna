using NoxAeterna.Domain.Tarot;

namespace NoxAeterna.Tests.Tarot;

public sealed class StandardTarotCatalogTests
{
    [Theory]
    [InlineData("")]
    [InlineData("UPPERCASE")]
    [InlineData("leading.separator.")]
    [InlineData("double..separator")]
    [InlineData("contains whitespace")]
    public void StableIds_RejectNonCanonicalValues(string value)
    {
        Assert.Throws<ArgumentException>(() => new TarotCardId(value));
        Assert.Throws<ArgumentException>(() => new TarotDeckId(value));
        Assert.Throws<ArgumentException>(() => new TarotSpreadId(value));
        Assert.Throws<ArgumentException>(() => new TarotSpreadPositionId(value));
    }

    [Fact]
    public void StandardDeck_ContainsExactlySeventyEightCardsWithStandardArcanaCounts()
    {
        var cards = StandardTarotCatalog.Deck.Cards;

        Assert.Equal(78, cards.Count);
        Assert.Equal(22, cards.Count(card => card.Arcana == TarotArcana.Major));
        Assert.Equal(56, cards.Count(card => card.Arcana == TarotArcana.Minor));
        Assert.All(
            Enum.GetValues<TarotSuit>(),
            suit => Assert.Equal(14, cards.Count(card => card.Suit == suit)));
    }

    [Fact]
    public void StandardDeck_HasUniqueCardIdsAndMinorSuitRankPairs()
    {
        var cards = StandardTarotCatalog.Deck.Cards;
        var minorCards = cards.Where(card => card.Arcana == TarotArcana.Minor).ToArray();

        Assert.Equal(cards.Count, cards.Select(card => card.Id).Distinct().Count());
        Assert.Equal(
            minorCards.Length,
            minorCards.Select(card => (card.Suit, card.Rank)).Distinct().Count());
        Assert.All(cards.Where(card => card.Arcana == TarotArcana.Major), card =>
        {
            Assert.Null(card.Suit);
            Assert.Null(card.Rank);
        });
        Assert.All(minorCards, card =>
        {
            Assert.NotNull(card.Suit);
            Assert.NotNull(card.Rank);
        });
    }

    [Fact]
    public void MajorCardIdentity_IsExplicitStableTextRatherThanEnumOrdinal()
    {
        var expectedIds = new[]
        {
            "major.chariot", "major.death", "major.devil", "major.emperor", "major.empress", "major.fool",
            "major.hanged-man", "major.hermit", "major.hierophant", "major.high-priestess", "major.judgement",
            "major.justice", "major.lovers", "major.magician", "major.moon", "major.star", "major.strength",
            "major.sun", "major.temperance", "major.tower", "major.wheel-of-fortune", "major.world"
        };

        var actualIds = StandardTarotCatalog.Deck.Cards
            .Where(card => card.Arcana == TarotArcana.Major)
            .Select(card => card.Id.Value)
            .ToArray();

        Assert.False(typeof(TarotCardId).IsEnum);
        Assert.Equal(expectedIds, actualIds);
    }

    [Fact]
    public void DeckDefinition_DefensivelyCopiesAndExposesReadOnlyCards()
    {
        var source = new List<TarotCardDefinition>
        {
            TarotCardDefinition.CreateMajor(new TarotCardId("major.alpha"))
        };
        var deck = new TarotDeckDefinition(new TarotDeckId("test-deck"), source);

        source.Clear();

        Assert.Single(deck.Cards);
        Assert.Throws<NotSupportedException>(() =>
            ((ICollection<TarotCardDefinition>)deck.Cards).Add(
                TarotCardDefinition.CreateMajor(new TarotCardId("major.beta"))));
    }

    [Fact]
    public void DeckDefinition_RejectsDuplicateCardIds()
    {
        var repeatedId = new TarotCardId("major.repeated");

        Assert.Throws<ArgumentException>(() => new TarotDeckDefinition(
            new TarotDeckId("test-deck"),
            [
                TarotCardDefinition.CreateMajor(repeatedId),
                TarotCardDefinition.CreateMajor(repeatedId)
            ]));
    }

    [Fact]
    public void DeckDefinition_RejectsDuplicateMinorSuitRankPairs()
    {
        Assert.Throws<ArgumentException>(() => new TarotDeckDefinition(
            new TarotDeckId("test-deck"),
            [
                TarotCardDefinition.CreateMinor(new TarotCardId("minor.first"), TarotSuit.Cups, TarotRank.Ace),
                TarotCardDefinition.CreateMinor(new TarotCardId("minor.second"), TarotSuit.Cups, TarotRank.Ace)
            ]));
    }
}
