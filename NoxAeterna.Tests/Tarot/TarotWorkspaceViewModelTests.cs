using NodaTime;
using NoxAeterna.Domain.Tarot;
using NoxAeterna.Presentation.Tarot;

namespace NoxAeterna.Tests.Tarot;

public sealed class TarotWorkspaceViewModelTests
{
    [Fact]
    public void Foundation_ExposesRealSingleAndThreeCardSpreadsAndIndependentSelections()
    {
        var viewModel = TarotWorkspaceViewModel.CreateFoundation(
            new TarotDrawEngine(new SequenceRandomSource(0)));

        Assert.Equal(
            new[] { StandardTarotSpreads.SingleCard.Id, StandardTarotSpreads.ThreeCards.Id },
            viewModel.SpreadOptions.Select(option => option.Definition.Id));
        Assert.Same(StandardTarotSpreads.SingleCard, viewModel.SpreadOptions[0].Definition);
        Assert.Same(StandardTarotSpreads.ThreeCards, viewModel.SpreadOptions[1].Definition);
        Assert.Equal("prototype-symbolic", viewModel.ArtworkPackId.Value);
        Assert.Equal("astral-archive-prototype", viewModel.PresentationSkinId.Value);
        Assert.Equal("foundation", viewModel.InterpretationSetId.Value);
    }

    [Fact]
    public void Draw_UsesDomainEngineAndPassesUprightOnlyPreference()
    {
        var random = new SequenceRandomSource(0);
        var viewModel = TarotWorkspaceViewModel.CreateFoundation(new TarotDrawEngine(random));
        var timestamp = Instant.FromUtc(2026, 8, 3, 12, 0);

        viewModel.Draw(timestamp);

        var reading = Assert.IsType<TarotReading>(viewModel.CurrentReading);
        Assert.Equal(StandardTarotCatalog.Deck.Id, reading.DeckId);
        Assert.Equal(StandardTarotSpreads.SingleCard.Id, reading.SpreadId);
        Assert.Equal(timestamp, reading.DrawnAt);
        Assert.Equal(TarotCardOrientation.Upright, Assert.Single(reading.Cards).Orientation);
        Assert.Equal(new[] { 78 }, random.RequestedUpperBounds);
        Assert.Null(viewModel.CurrentFailure);
    }

    [Fact]
    public void Draw_PassesUprightAndReversedPreferenceToDomainEngine()
    {
        var random = new SequenceRandomSource(0, 1);
        var viewModel = TarotWorkspaceViewModel.CreateFoundation(new TarotDrawEngine(random));
        viewModel.SetAllowReversed(true);

        viewModel.Draw(Instant.FromUnixTimeTicks(10));

        Assert.Equal(
            TarotCardOrientation.Reversed,
            Assert.Single(Assert.IsType<TarotReading>(viewModel.CurrentReading).Cards).Orientation);
        Assert.Equal(new[] { 78, 2 }, random.RequestedUpperBounds);
    }

    [Fact]
    public void Redraw_ReplacesCurrentReadingWithNewDomainResult()
    {
        var viewModel = TarotWorkspaceViewModel.CreateFoundation(
            new TarotDrawEngine(new SequenceRandomSource(0, 1)));
        viewModel.Draw(Instant.FromUnixTimeTicks(20));
        var firstReading = Assert.IsType<TarotReading>(viewModel.CurrentReading);

        viewModel.Draw(Instant.FromUnixTimeTicks(21));

        var secondReading = Assert.IsType<TarotReading>(viewModel.CurrentReading);
        Assert.NotSame(firstReading, secondReading);
        Assert.NotEqual(firstReading.Cards[0].Card.Id, secondReading.Cards[0].Card.Id);
        Assert.Equal(Instant.FromUnixTimeTicks(21), secondReading.DrawnAt);
    }

    [Fact]
    public void RevealAndSelect_PreservesExactSpreadAssignment()
    {
        var viewModel = TarotWorkspaceViewModel.CreateFoundation(
            new TarotDrawEngine(new SequenceRandomSource(2, 0, 0)));
        viewModel.SelectSpread(StandardTarotSpreads.ThreeCards.Id);
        viewModel.Draw(Instant.FromUnixTimeTicks(30));
        var reading = Assert.IsType<TarotReading>(viewModel.CurrentReading);
        var expected = reading.Cards[1];

        viewModel.RevealAndSelect(expected.PositionId);

        Assert.Same(expected, viewModel.SelectedCard);
        Assert.Equal("present", viewModel.SelectedCard!.PositionId.Value);
        Assert.True(viewModel.IsRevealed(expected.PositionId));
        Assert.False(viewModel.IsRevealed(reading.Cards[0].PositionId));
    }

    [Fact]
    public void ChangingSpread_ClearsIncompatibleReadingAndSelection()
    {
        var viewModel = TarotWorkspaceViewModel.CreateFoundation(
            new TarotDrawEngine(new SequenceRandomSource(0)));
        viewModel.Draw(Instant.FromUnixTimeTicks(40));
        var positionId = Assert.Single(viewModel.CurrentReading!.Cards).PositionId;
        viewModel.RevealAndSelect(positionId);

        viewModel.SelectSpread(StandardTarotSpreads.ThreeCards.Id);

        Assert.Null(viewModel.CurrentReading);
        Assert.Null(viewModel.SelectedCard);
        Assert.Null(viewModel.CurrentFailure);
        Assert.False(viewModel.IsRevealed(positionId));
    }

    [Fact]
    public void ControlledDrawFailure_ReplacesReadingWithTypedFailureState()
    {
        var deck = new TarotDeckDefinition(
            new TarotDeckId("small-deck"),
            [TarotCardDefinition.CreateMajor(new TarotCardId("major.only"))]);
        var viewModel = new TarotWorkspaceViewModel(
            new TarotDrawEngine(new SequenceRandomSource()),
            deck,
            [new TarotSpreadOption(StandardTarotSpreads.ThreeCards, new("ui.test.spread"))],
            TarotPrototypeSelections.BackVariants,
            TarotPrototypeSelections.ArtworkPackId,
            TarotPrototypeSelections.PresentationSkinId,
            TarotPrototypeSelections.InterpretationSetId);

        viewModel.Draw(Instant.FromUnixTimeTicks(50));

        Assert.Null(viewModel.CurrentReading);
        Assert.Null(viewModel.SelectedCard);
        Assert.Equal(TarotDrawFailureReason.InsufficientDeckSize, viewModel.CurrentFailure?.Reason);
        Assert.Equal("ui.tarot.failure.insufficient-deck", viewModel.FailureStateKey.Value);
    }

    [Fact]
    public void PrototypeBackVariants_HaveTwoUniqueIdsAndSelectionDoesNotChangeReading()
    {
        var viewModel = TarotWorkspaceViewModel.CreateFoundation(
            new TarotDrawEngine(new SequenceRandomSource(0)));
        viewModel.Draw(Instant.FromUnixTimeTicks(60));
        var reading = viewModel.CurrentReading;

        Assert.Equal(2, viewModel.BackVariants.Count);
        Assert.Equal(2, viewModel.BackVariants.Select(option => option.Id).Distinct().Count());
        Assert.Equal(new[] { "black-sun", "lunar-seal" }, viewModel.BackVariants.Select(option => option.Id.Value));

        viewModel.SelectBackVariant(new TarotBackVariantId("lunar-seal"));

        Assert.Equal("lunar-seal", viewModel.SelectedBackVariant.Id.Value);
        Assert.Same(reading, viewModel.CurrentReading);
    }

    private sealed class SequenceRandomSource(params int[] values) : ITarotRandomSource
    {
        private readonly Queue<int> values = new(values);

        public List<int> RequestedUpperBounds { get; } = [];

        public int NextIndex(int exclusiveUpperBound)
        {
            RequestedUpperBounds.Add(exclusiveUpperBound);
            return values.Dequeue();
        }
    }
}
