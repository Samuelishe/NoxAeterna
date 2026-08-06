using NodaTime;
using NoxAeterna.Domain.Tarot;
using NoxAeterna.Presentation.Preferences;
using NoxAeterna.Presentation.Tarot;

namespace NoxAeterna.Tests.Tarot;

public sealed class TarotWorkspaceViewModelTests
{
    [Fact]
    public void ClassicWorkspace_ExposesRealSpreadsAndDefaultsToSoleLupusNoctisArtworkPack()
    {
        var viewModel = TarotWorkspaceViewModel.CreateClassic(
            new TarotDrawEngine(new SequenceRandomSource(0)));

        Assert.Equal(
            new[] { StandardTarotSpreads.SingleCard.Id, StandardTarotSpreads.ThreeCards.Id },
            viewModel.SpreadOptions.Select(option => option.Definition.Id));
        Assert.Same(StandardTarotSpreads.SingleCard, viewModel.SpreadOptions[0].Definition);
        Assert.Same(StandardTarotSpreads.ThreeCards, viewModel.SpreadOptions[1].Definition);
        Assert.Equal("lupus-noctis", viewModel.ArtworkPackId.Value);
        Assert.Equal("lupus-noctis", Assert.Single(viewModel.ArtworkPacks).Id.Value);
        Assert.Same(Assert.Single(viewModel.ArtworkPacks), viewModel.SelectedArtworkPack);
        Assert.True(viewModel.AutoRevealCards);
        Assert.Equal(TarotWorkspacePreferences.CreateDefault(), viewModel.Preferences);
        Assert.Equal("astral-archive-prototype", viewModel.PresentationSkinId.Value);
        Assert.Equal("classic", viewModel.InterpretationPackId.Value);
    }

    [Fact]
    public void Draw_UsesDomainEngineAndPassesUprightOnlyPreference()
    {
        var random = new SequenceRandomSource(0);
        var viewModel = TarotWorkspaceViewModel.CreateClassic(new TarotDrawEngine(random));
        var timestamp = Instant.FromUtc(2026, 8, 3, 12, 0);

        viewModel.Draw(timestamp);

        var reading = Assert.IsType<TarotReading>(viewModel.CurrentReading);
        Assert.Equal(StandardTarotCatalog.Deck.Id, reading.DeckId);
        Assert.Equal(StandardTarotSpreads.SingleCard.Id, reading.SpreadId);
        Assert.Equal(timestamp, reading.DrawnAt);
        Assert.Equal(TarotCardOrientation.Upright, Assert.Single(reading.Cards).Orientation);
        Assert.Equal(new[] { 78 }, random.RequestedUpperBounds);
        Assert.Null(viewModel.CurrentFailure);
        Assert.Equal(1, viewModel.RevealedCardCount);
        Assert.True(viewModel.HasRevealedCards);
        Assert.True(viewModel.AreAllCardsRevealed);
        Assert.True(viewModel.IsRevealed(Assert.Single(reading.Cards).PositionId));
        Assert.Null(viewModel.SelectedCard);
    }

    [Fact]
    public void Draw_PassesUprightAndReversedPreferenceToDomainEngine()
    {
        var random = new SequenceRandomSource(0, 1);
        var viewModel = TarotWorkspaceViewModel.CreateClassic(new TarotDrawEngine(random));
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
        var viewModel = TarotWorkspaceViewModel.CreateClassic(
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
        var viewModel = TarotWorkspaceViewModel.CreateClassic(
            new TarotDrawEngine(new SequenceRandomSource(2, 0, 0)));
        viewModel.SelectSpread(StandardTarotSpreads.ThreeCards.Id);
        viewModel.SetAutoRevealCards(false);
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
    public void Draw_WithAutoRevealDisabled_StartsWithEveryPositionHidden()
    {
        var viewModel = CreateThreeCardWorkspace(autoRevealCards: false, new SequenceRandomSource(0, 0, 0));

        viewModel.Draw(Instant.FromUnixTimeTicks(31));

        var reading = Assert.IsType<TarotReading>(viewModel.CurrentReading);
        Assert.Equal(3, reading.Cards.Count);
        Assert.Equal(0, viewModel.RevealedCardCount);
        Assert.False(viewModel.HasRevealedCards);
        Assert.False(viewModel.AreAllCardsRevealed);
        Assert.All(reading.Cards, assignment => Assert.False(viewModel.IsRevealed(assignment.PositionId)));
        Assert.Null(viewModel.SelectedCard);
    }

    [Fact]
    public void ManualReveal_WithAutoRevealDisabled_RevealsOnlyRequestedPosition()
    {
        var viewModel = CreateThreeCardWorkspace(autoRevealCards: false, new SequenceRandomSource(0, 0, 0));
        viewModel.Draw(Instant.FromUnixTimeTicks(32));
        var reading = Assert.IsType<TarotReading>(viewModel.CurrentReading);

        viewModel.RevealAndSelect(reading.Cards[1].PositionId);

        Assert.Same(reading.Cards[1], viewModel.SelectedCard);
        Assert.Equal(1, viewModel.RevealedCardCount);
        Assert.True(viewModel.HasRevealedCards);
        Assert.False(viewModel.AreAllCardsRevealed);
        Assert.False(viewModel.IsRevealed(reading.Cards[0].PositionId));
        Assert.True(viewModel.IsRevealed(reading.Cards[1].PositionId));
        Assert.False(viewModel.IsRevealed(reading.Cards[2].PositionId));
    }

    [Fact]
    public void RevealStateProperties_TrackEmptyPartialAndCompleteReading()
    {
        var viewModel = CreateThreeCardWorkspace(autoRevealCards: false, new SequenceRandomSource(0, 0, 0));
        Assert.Equal(0, viewModel.RevealedCardCount);
        Assert.False(viewModel.HasRevealedCards);
        Assert.False(viewModel.AreAllCardsRevealed);
        viewModel.Draw(Instant.FromUnixTimeTicks(33));
        var reading = Assert.IsType<TarotReading>(viewModel.CurrentReading);

        viewModel.RevealAndSelect(reading.Cards[0].PositionId);

        Assert.Equal(1, viewModel.RevealedCardCount);
        Assert.True(viewModel.HasRevealedCards);
        Assert.False(viewModel.AreAllCardsRevealed);

        viewModel.RevealAndSelect(reading.Cards[1].PositionId);
        viewModel.RevealAndSelect(reading.Cards[2].PositionId);

        Assert.Equal(3, viewModel.RevealedCardCount);
        Assert.True(viewModel.HasRevealedCards);
        Assert.True(viewModel.AreAllCardsRevealed);
    }

    [Fact]
    public void EnablingAutoRevealAfterDraw_DoesNotRevealCurrentReadingRetroactively()
    {
        var viewModel = CreateThreeCardWorkspace(autoRevealCards: false, new SequenceRandomSource(0, 0, 0));
        viewModel.Draw(Instant.FromUnixTimeTicks(34));
        var reading = Assert.IsType<TarotReading>(viewModel.CurrentReading);
        viewModel.RevealAndSelect(reading.Cards[1].PositionId);

        viewModel.SetAutoRevealCards(true);

        Assert.Same(reading, viewModel.CurrentReading);
        Assert.Same(reading.Cards[1], viewModel.SelectedCard);
        Assert.Equal(1, viewModel.RevealedCardCount);
        Assert.True(viewModel.IsRevealed(reading.Cards[1].PositionId));
        Assert.False(viewModel.IsRevealed(reading.Cards[0].PositionId));
        Assert.False(viewModel.IsRevealed(reading.Cards[2].PositionId));
    }

    [Fact]
    public void DisablingAutoRevealAfterDraw_DoesNotHideCurrentReading()
    {
        var viewModel = CreateThreeCardWorkspace(autoRevealCards: true, new SequenceRandomSource(0, 0, 0));
        viewModel.Draw(Instant.FromUnixTimeTicks(35));
        var reading = Assert.IsType<TarotReading>(viewModel.CurrentReading);
        viewModel.RevealAndSelect(reading.Cards[1].PositionId);

        viewModel.SetAutoRevealCards(false);

        Assert.Same(reading, viewModel.CurrentReading);
        Assert.Same(reading.Cards[1], viewModel.SelectedCard);
        Assert.Equal(3, viewModel.RevealedCardCount);
        Assert.True(viewModel.AreAllCardsRevealed);
        Assert.All(reading.Cards, assignment => Assert.True(viewModel.IsRevealed(assignment.PositionId)));
    }

    [Fact]
    public void SubsequentDraw_UsesLatestAutoRevealPreference()
    {
        var viewModel = CreateThreeCardWorkspace(
            autoRevealCards: true,
            new SequenceRandomSource(0, 0, 0, 0, 0, 0));
        viewModel.Draw(Instant.FromUnixTimeTicks(36));
        Assert.True(viewModel.AreAllCardsRevealed);
        var firstReading = viewModel.CurrentReading;

        viewModel.SetAutoRevealCards(false);
        viewModel.Draw(Instant.FromUnixTimeTicks(37));

        Assert.NotSame(firstReading, viewModel.CurrentReading);
        Assert.Equal(0, viewModel.RevealedCardCount);
        Assert.False(viewModel.HasRevealedCards);
        Assert.False(viewModel.AreAllCardsRevealed);
    }

    [Fact]
    public void ChangingSpread_ClearsIncompatibleReadingAndSelection()
    {
        var viewModel = TarotWorkspaceViewModel.CreateClassic(
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
    public void ControlledDrawFailure_ClearsReadingSelectionAndRevealState()
    {
        var deck = new TarotDeckDefinition(
            new TarotDeckId("small-deck"),
            [TarotCardDefinition.CreateMajor(new TarotCardId("major.only"))]);
        var viewModel = new TarotWorkspaceViewModel(
            new TarotDrawEngine(new SequenceRandomSource()),
            deck,
            [new TarotSpreadOption(StandardTarotSpreads.ThreeCards, new("ui.test.spread"))],
            TarotPrototypeSelections.BackVariants,
            TarotPrototypeSelections.ArtworkPacks,
            TarotPrototypeSelections.PresentationSkinId,
            TarotPrototypeSelections.InterpretationPackId,
            new TarotWorkspacePreferences(
                StandardTarotSpreads.ThreeCards.Id,
                TarotPrototypeSelections.DefaultArtworkPackId,
                new TarotBackVariantId("black-sun"),
                AllowReversed: false,
                AutoRevealCards: true));

        viewModel.Draw(Instant.FromUnixTimeTicks(50));

        Assert.Null(viewModel.CurrentReading);
        Assert.Null(viewModel.SelectedCard);
        Assert.Equal(0, viewModel.RevealedCardCount);
        Assert.False(viewModel.HasRevealedCards);
        Assert.False(viewModel.AreAllCardsRevealed);
        Assert.Equal(TarotDrawFailureReason.InsufficientDeckSize, viewModel.CurrentFailure?.Reason);
        Assert.Equal("ui.tarot.failure.insufficient-deck", viewModel.FailureStateKey.Value);
    }

    [Fact]
    public void CreateClassic_AppliesPersistedTarotPreferencesByExplicitIds()
    {
        var initial = new TarotWorkspacePreferences(
            StandardTarotSpreads.ThreeCards.Id,
            TarotPrototypeSelections.LupusNoctisArtworkPackId,
            new TarotBackVariantId("lunar-seal"),
            AllowReversed: true,
            AutoRevealCards: false);

        var viewModel = TarotWorkspaceViewModel.CreateClassic(
            new TarotDrawEngine(new SequenceRandomSource()),
            initialPreferences: initial);

        Assert.Same(StandardTarotSpreads.ThreeCards, viewModel.SelectedSpread.Definition);
        Assert.Equal("lupus-noctis", viewModel.ArtworkPackId.Value);
        Assert.Equal("lunar-seal", viewModel.SelectedBackVariant.Id.Value);
        Assert.True(viewModel.AllowReversed);
        Assert.False(viewModel.AutoRevealCards);
        Assert.Equal(initial, viewModel.Preferences);
    }

    [Theory]
    [InlineData("spread")]
    [InlineData("artwork")]
    [InlineData("back")]
    public void Constructor_RejectsUnavailableInitialPreferenceIds(string field)
    {
        var initial = TarotWorkspacePreferences.CreateDefault();
        initial = field switch
        {
            "spread" => initial with { SpreadId = new TarotSpreadId("missing-spread") },
            "artwork" => initial with { ArtworkPackId = new TarotArtworkPackId("missing-artwork") },
            "back" => initial with { BackVariantId = new TarotBackVariantId("missing-back") },
            _ => throw new ArgumentOutOfRangeException(nameof(field))
        };

        var exception = Assert.Throws<ArgumentException>(() => new TarotWorkspaceViewModel(
            new TarotDrawEngine(new SequenceRandomSource()),
            StandardTarotCatalog.Deck,
            [
                new TarotSpreadOption(StandardTarotSpreads.SingleCard, new("ui.test.single")),
                new TarotSpreadOption(StandardTarotSpreads.ThreeCards, new("ui.test.three"))
            ],
            TarotPrototypeSelections.BackVariants,
            TarotPrototypeSelections.ArtworkPacks,
            TarotPrototypeSelections.PresentationSkinId,
            TarotPrototypeSelections.InterpretationPackId,
            initial));

        Assert.Equal("initialPreferences", exception.ParamName);
    }

    [Fact]
    public void ActualTarotPreferenceChanges_RaiseTypedEventExactlyOnceEach()
    {
        var alternate = new TarotArtworkPackOption(new("alternate-pack"), new("ui.test.alternate"));
        var viewModel = CreateWorkspaceWithArtworkOptions([.. TarotPrototypeSelections.ArtworkPacks, alternate]);
        var observed = new List<TarotWorkspacePreferences>();
        viewModel.PreferencesChanged += (_, preferences) => observed.Add(preferences);

        viewModel.SelectSpread(StandardTarotSpreads.ThreeCards.Id);
        viewModel.SelectArtworkPack(alternate.Id);
        viewModel.SelectBackVariant(new TarotBackVariantId("lunar-seal"));
        viewModel.SetAllowReversed(true);
        viewModel.SetAutoRevealCards(false);

        Assert.Equal(5, observed.Count);
        Assert.Equal(StandardTarotSpreads.ThreeCards.Id, observed[0].SpreadId);
        Assert.Equal(alternate.Id, observed[1].ArtworkPackId);
        Assert.Equal(new TarotBackVariantId("lunar-seal"), observed[2].BackVariantId);
        Assert.True(observed[3].AllowReversed);
        Assert.False(observed[4].AutoRevealCards);
        Assert.Equal(viewModel.Preferences, observed[^1]);
    }

    [Fact]
    public void SettingSameTarotPreferences_DoesNotRaiseTypedEvent()
    {
        var viewModel = TarotWorkspaceViewModel.CreateClassic(
            new TarotDrawEngine(new SequenceRandomSource()));
        var count = 0;
        viewModel.PreferencesChanged += (_, _) => count++;

        viewModel.SelectSpread(StandardTarotSpreads.SingleCard.Id);
        viewModel.SelectArtworkPack(TarotPrototypeSelections.LupusNoctisArtworkPackId);
        viewModel.SelectBackVariant(new TarotBackVariantId("black-sun"));
        viewModel.SetAllowReversed(false);
        viewModel.SetAutoRevealCards(true);

        Assert.Equal(0, count);
    }

    [Fact]
    public void DrawRevealAndSelection_DoNotRaisePreferenceEvent()
    {
        var viewModel = CreateThreeCardWorkspace(autoRevealCards: false, new SequenceRandomSource(0, 0, 0));
        var count = 0;
        viewModel.PreferencesChanged += (_, _) => count++;

        viewModel.Draw(Instant.FromUnixTimeTicks(51));
        var assignment = Assert.IsType<TarotReading>(viewModel.CurrentReading).Cards[1];
        viewModel.RevealAndSelect(assignment.PositionId);
        viewModel.RevealAndSelect(assignment.PositionId);

        Assert.Same(assignment, viewModel.SelectedCard);
        Assert.Equal(1, viewModel.RevealedCardCount);
        Assert.Equal(0, count);
    }

    [Fact]
    public void PrototypeBackVariants_HaveTwoUniqueIdsAndSelectionDoesNotChangeReading()
    {
        var viewModel = TarotWorkspaceViewModel.CreateClassic(
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

    [Fact]
    public void SelectingCurrentLupusNoctisArtworkPack_PreservesCurrentReadingRevealAndSelection()
    {
        var viewModel = TarotWorkspaceViewModel.CreateClassic(
            new TarotDrawEngine(new SequenceRandomSource(0)));
        viewModel.Draw(Instant.FromUnixTimeTicks(70));
        var reading = Assert.IsType<TarotReading>(viewModel.CurrentReading);
        var assignment = Assert.Single(reading.Cards);
        viewModel.RevealAndSelect(assignment.PositionId);

        viewModel.SelectArtworkPack(new TarotArtworkPackId("lupus-noctis"));

        Assert.Equal("lupus-noctis", viewModel.SelectedArtworkPack.Id.Value);
        Assert.Same(reading, viewModel.CurrentReading);
        Assert.Same(assignment, viewModel.SelectedCard);
        Assert.True(viewModel.IsRevealed(assignment.PositionId));
        Assert.Equal(Instant.FromUnixTimeTicks(70), viewModel.CurrentReading!.DrawnAt);
        Assert.Equal(assignment.Card.Id, viewModel.CurrentReading.Cards[0].Card.Id);
    }

    [Fact]
    public void SelectArtworkPack_RejectsUnavailableIdWithoutChangingState()
    {
        var viewModel = TarotWorkspaceViewModel.CreateClassic(
            new TarotDrawEngine(new SequenceRandomSource(0)));

        var exception = Assert.Throws<ArgumentException>(() =>
            viewModel.SelectArtworkPack(new TarotArtworkPackId("not-installed")));

        Assert.Equal("artworkPackId", exception.ParamName);
        Assert.Equal("lupus-noctis", viewModel.ArtworkPackId.Value);
        Assert.Null(viewModel.CurrentReading);
    }

    [Fact]
    public void Constructor_UsesExplicitDefaultArtworkPackInsteadOfListPosition()
    {
        var alternate = new TarotArtworkPackOption(new("alternate-pack"), new("ui.test.alternate"));
        var lupus = Assert.Single(TarotPrototypeSelections.ArtworkPacks);

        var viewModel = new TarotWorkspaceViewModel(
            new TarotDrawEngine(new SequenceRandomSource()),
            StandardTarotCatalog.Deck,
            [new TarotSpreadOption(StandardTarotSpreads.SingleCard, new("ui.test.spread"))],
            TarotPrototypeSelections.BackVariants,
            [alternate, lupus],
            TarotPrototypeSelections.PresentationSkinId,
            TarotPrototypeSelections.InterpretationPackId,
            TarotWorkspacePreferences.CreateDefault());

        Assert.Same(lupus, viewModel.SelectedArtworkPack);
        Assert.Equal("lupus-noctis", viewModel.ArtworkPackId.Value);
    }

    [Fact]
    public void Constructor_RejectsDuplicateArtworkPackOptions()
    {
        var duplicateId = new TarotArtworkPackId("duplicate-pack");

        var exception = Assert.Throws<ArgumentException>(() => new TarotWorkspaceViewModel(
            new TarotDrawEngine(new SequenceRandomSource()),
            StandardTarotCatalog.Deck,
            [new TarotSpreadOption(StandardTarotSpreads.SingleCard, new("ui.test.spread"))],
            TarotPrototypeSelections.BackVariants,
            [
                new TarotArtworkPackOption(duplicateId, new("ui.test.first")),
                new TarotArtworkPackOption(duplicateId, new("ui.test.second"))
            ],
            TarotPrototypeSelections.PresentationSkinId,
            TarotPrototypeSelections.InterpretationPackId,
            TarotWorkspacePreferences.CreateDefault()));

        Assert.Equal("artworkPacks", exception.ParamName);
    }

    private static TarotWorkspaceViewModel CreateThreeCardWorkspace(
        bool autoRevealCards,
        ITarotRandomSource randomSource) => TarotWorkspaceViewModel.CreateClassic(
        new TarotDrawEngine(randomSource),
        initialPreferences: new TarotWorkspacePreferences(
            StandardTarotSpreads.ThreeCards.Id,
            TarotPrototypeSelections.DefaultArtworkPackId,
            new TarotBackVariantId("black-sun"),
            AllowReversed: false,
            AutoRevealCards: autoRevealCards));

    private static TarotWorkspaceViewModel CreateWorkspaceWithArtworkOptions(
        IReadOnlyList<TarotArtworkPackOption> artworkPacks) => new(
        new TarotDrawEngine(new SequenceRandomSource()),
        StandardTarotCatalog.Deck,
        [
            new TarotSpreadOption(StandardTarotSpreads.SingleCard, new("ui.test.single")),
            new TarotSpreadOption(StandardTarotSpreads.ThreeCards, new("ui.test.three"))
        ],
        TarotPrototypeSelections.BackVariants,
        artworkPacks,
        TarotPrototypeSelections.PresentationSkinId,
        TarotPrototypeSelections.InterpretationPackId,
        TarotWorkspacePreferences.CreateDefault());

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
