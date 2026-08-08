using NoxAeterna.Domain.Tarot;

namespace NoxAeterna.Tests.Tarot;

public sealed class TarotSpreadDefinitionTests
{
    [Fact]
    public void SingleCardSpread_ContainsOneStablePosition()
    {
        Assert.Collection(
            StandardTarotSpreads.SingleCard.Positions,
            position => Assert.Equal("card", position.Id.Value));
    }

    [Fact]
    public void TwoCardSpread_ContainsExactlyTwoNeutralStableTechnicalPositions()
    {
        Assert.Equal("two-cards", StandardTarotSpreads.TwoCards.Id.Value);
        var positionIds = StandardTarotSpreads.TwoCards.Positions
            .Select(static position => position.Id.Value)
            .ToArray();

        Assert.Equal(new[] { "slot-a", "slot-b" }, positionIds);
        Assert.Equal(2, positionIds.Distinct(StringComparer.Ordinal).Count());
        Assert.DoesNotContain(
            positionIds,
            id => new[]
            {
                "past", "future", "cause", "effect", "problem", "solution", "me", "partner",
                "question", "answer", "left", "right", "first", "second"
            }.Contains(id, StringComparer.Ordinal));
    }

    [Fact]
    public void ThreeCardSpread_ContainsUniquePastPresentFuturePositionsInOrder()
    {
        var positionIds = StandardTarotSpreads.ThreeCards.Positions
            .Select(position => position.Id.Value)
            .ToArray();

        Assert.Equal(new[] { "past", "present", "future" }, positionIds);
        Assert.Equal(positionIds.Length, positionIds.Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void SpreadDefinition_DefensivelyCopiesAndExposesReadOnlyPositions()
    {
        var source = new List<TarotSpreadPositionDefinition>
        {
            new(new TarotSpreadPositionId("first"))
        };
        var spread = new TarotSpreadDefinition(new TarotSpreadId("test-spread"), source);

        source.Clear();

        Assert.Single(spread.Positions);
        Assert.Throws<NotSupportedException>(() =>
            ((ICollection<TarotSpreadPositionDefinition>)spread.Positions).Add(
                new TarotSpreadPositionDefinition(new TarotSpreadPositionId("second"))));
    }

    [Fact]
    public void SpreadDefinition_RejectsDuplicatePositionIds()
    {
        Assert.Throws<ArgumentException>(() => new TarotSpreadDefinition(
            new TarotSpreadId("test-spread"),
            [
                new TarotSpreadPositionDefinition(new TarotSpreadPositionId("same")),
                new TarotSpreadPositionDefinition(new TarotSpreadPositionId("same"))
            ]));
    }

    [Fact]
    public void SpreadDefinition_RejectsEmptyPositionSet()
    {
        Assert.Throws<ArgumentException>(() => new TarotSpreadDefinition(
            new TarotSpreadId("test-spread"),
            Array.Empty<TarotSpreadPositionDefinition>()));
    }
}
