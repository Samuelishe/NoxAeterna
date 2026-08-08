using NodaTime;
using NoxAeterna.App.Tarot;
using NoxAeterna.Domain.Tarot;
using NoxAeterna.Interpretation.Tarot.Contracts;
using NoxAeterna.Interpretation.Tarot.Keys;
using NoxAeterna.Interpretation.Tarot.Resolution;
using NoxAeterna.Interpretation.Tarot.Validation;
using NoxAeterna.Presentation.Localization;
using NoxAeterna.Presentation.Preferences;
using NoxAeterna.Presentation.Tarot;
using NoxAeterna.Tests.Interpretation.Tarot;

namespace NoxAeterna.Tests.App;

public sealed class TarotWorkspaceInterpretationCoordinatorTests
{
    [Fact]
    public void NoReadingAndHiddenSingleCard_DoNotCallResolver()
    {
        var resolver = new RecordingResolver();
        var viewModel = Workspace(autoRevealCards: false, new SequenceRandomSource(0));
        using var coordinator = Coordinator(resolver, viewModel);

        Assert.Empty(resolver.SingleCalls);
        Assert.Empty(resolver.PositionCalls);

        viewModel.Draw(Instant.FromUnixTimeTicks(1));

        Assert.Empty(resolver.SingleCalls);
        Assert.Empty(resolver.PositionCalls);
        Assert.Same(TarotWorkspaceInterpretationSnapshot.Empty, coordinator.Current);
    }

    [Fact]
    public void RevealedSingleCard_RequestsExactPackLocaleCardAndOrientationOnce()
    {
        var resolver = new RecordingResolver();
        var viewModel = Workspace(autoRevealCards: false, new SequenceRandomSource(0));
        using var coordinator = Coordinator(resolver, viewModel);
        viewModel.Draw(Instant.FromUnixTimeTicks(2));
        var card = Assert.Single(Assert.IsType<TarotReading>(viewModel.CurrentReading).Cards);

        viewModel.RevealAndSelect(card.PositionId);

        var call = Assert.Single(resolver.SingleCalls);
        Assert.Equal(TarotPrototypeSelections.InterpretationPackId, call.PackId);
        Assert.Equal(new TarotInterpretationLocale("ru"), call.Locale);
        Assert.Equal(card.Card.Id, call.CardId);
        Assert.Equal(card.Orientation, call.Orientation);
        Assert.IsType<NoTarotInterpretationContent<TarotSingleCardEntry>>(coordinator.Current.SingleCard);
    }

    [Fact]
    public void AutoRevealedSingleCard_ResolvesImmediatelyAfterSuccessfulDraw()
    {
        var resolver = new RecordingResolver();
        var viewModel = Workspace(autoRevealCards: true, new SequenceRandomSource(0));
        using var coordinator = Coordinator(resolver, viewModel);

        viewModel.Draw(Instant.FromUnixTimeTicks(3));

        Assert.Single(resolver.SingleCalls);
        Assert.True(viewModel.AreAllCardsRevealed);
    }

    [Fact]
    public void TwoCards_WaitsForBothRevealsThenResolvesExactlyOneCombinedPair()
    {
        var resolver = new RecordingResolver();
        var viewModel = Workspace(autoRevealCards: false, new SequenceRandomSource(0, 0));
        viewModel.SelectSpread(StandardTarotSpreads.TwoCards.Id);
        using var coordinator = Coordinator(resolver, viewModel);
        viewModel.Draw(Instant.FromUnixTimeTicks(31));
        var reading = Assert.IsType<TarotReading>(viewModel.CurrentReading);

        Assert.Empty(resolver.PairCalls);
        Assert.Null(coordinator.Current.TwoCardPair);
        viewModel.RevealAndSelect(reading.Cards[0].PositionId);

        Assert.Empty(resolver.PairCalls);
        Assert.Null(coordinator.Current.TwoCardPair);
        Assert.Empty(resolver.SingleCalls);
        Assert.Empty(resolver.PositionCalls);
        viewModel.RevealAndSelect(reading.Cards[1].PositionId);

        var call = Assert.Single(resolver.PairCalls);
        Assert.Equal(reading.Cards[0].Card.Id, call.FirstCardId);
        Assert.Equal(reading.Cards[0].Orientation, call.FirstOrientation);
        Assert.Equal(reading.Cards[1].Card.Id, call.SecondCardId);
        Assert.Equal(reading.Cards[1].Orientation, call.SecondOrientation);
        Assert.IsType<NoTarotInterpretationContent<TarotOrientedPairEntry>>(coordinator.Current.TwoCardPair);
        Assert.Null(coordinator.Current.TwoCardPresentation);
        Assert.False(coordinator.Current.HasResolvedContent);
        Assert.Null(coordinator.Current.SingleCard);
        Assert.Empty(coordinator.Current.ThreeCardPositions);
    }

    [Fact]
    public void AutoRevealedTwoCards_ResolvesOnePairAndBuildsOneCombinedPresentation()
    {
        var resolver = new RecordingResolver(resolvePair: true);
        var viewModel = Workspace(autoRevealCards: true, new SequenceRandomSource(0, 0));
        viewModel.SelectSpread(StandardTarotSpreads.TwoCards.Id);
        using var coordinator = Coordinator(resolver, viewModel, new RecordingLabelSource());

        viewModel.Draw(Instant.FromUnixTimeTicks(32));

        Assert.True(viewModel.AreAllCardsRevealed);
        Assert.Single(resolver.PairCalls);
        Assert.IsType<ResolvedTarotInterpretation<TarotOrientedPairEntry>>(coordinator.Current.TwoCardPair);
        var presentation = Assert.IsType<TarotOrientedPairInterpretationPresentation>(
            coordinator.Current.TwoCardPresentation);
        Assert.Equal("Synthetic pair interaction", presentation.Interaction);
        Assert.Equal("Synthetic pair direction", presentation.Direction);
        Assert.Equal(3, presentation.Tags.Count);
        Assert.True(coordinator.Current.HasResolvedContent);
        Assert.Null(coordinator.Current.SingleCardPresentation);
    }

    [Fact]
    public void TwoCardPackLocaleAndRedrawRefreshesReplaceSnapshotWithoutStaleContent()
    {
        var resolver = new RecordingResolver(resolvePair: true, resolvedLocaleFollowsRequest: true);
        var secondPack = new TarotInterpretationPackOption(new("second-pack"));
        var viewModel = Workspace(
            autoRevealCards: true,
            new SequenceRandomSource(0, 0, 1, 0),
            [new(TarotPrototypeSelections.InterpretationPackId), secondPack]);
        viewModel.SelectSpread(StandardTarotSpreads.TwoCards.Id);
        using var coordinator = Coordinator(resolver, viewModel, new RecordingLabelSource());
        viewModel.Draw(Instant.FromUnixTimeTicks(33));
        var firstReading = Assert.IsType<TarotReading>(viewModel.CurrentReading);
        var firstPair = Assert.IsType<ResolvedTarotInterpretation<TarotOrientedPairEntry>>(
            coordinator.Current.TwoCardPair);

        viewModel.SelectInterpretationPack(secondPack.Id);
        Assert.Equal(secondPack.Id, Assert.IsType<ResolvedTarotInterpretation<TarotOrientedPairEntry>>(
            coordinator.Current.TwoCardPair).PackId);
        coordinator.SetInterpretationLanguage(new(new LanguageCode("en")));
        var english = Assert.IsType<TarotOrientedPairInterpretationPresentation>(
            coordinator.Current.TwoCardPresentation);
        Assert.Equal("en", english.ResolvedLocale.Value);
        Assert.All(english.Tags, static tag => Assert.StartsWith("en ", tag.Label, StringComparison.Ordinal));

        viewModel.Draw(Instant.FromUnixTimeTicks(34));
        var secondReading = Assert.IsType<TarotReading>(viewModel.CurrentReading);
        var secondPair = Assert.IsType<ResolvedTarotInterpretation<TarotOrientedPairEntry>>(
            coordinator.Current.TwoCardPair);

        Assert.NotSame(firstReading, secondReading);
        Assert.NotEqual(
            (firstPair.Content.CardAId, firstPair.Content.CardBId),
            (secondPair.Content.CardAId, secondPair.Content.CardBId));
        Assert.Equal(4, resolver.PairCalls.Count);
        Assert.Null(coordinator.Current.SingleCard);
        Assert.Empty(coordinator.Current.ThreeCardPositions);
    }

    [Fact]
    public void ThreeCards_ResolveOnlyPositionsRevealedAtEachRefresh()
    {
        var resolver = new RecordingResolver();
        var viewModel = Workspace(autoRevealCards: false, new SequenceRandomSource(0, 0, 0));
        viewModel.SelectSpread(StandardTarotSpreads.ThreeCards.Id);
        using var coordinator = Coordinator(resolver, viewModel);
        viewModel.Draw(Instant.FromUnixTimeTicks(4));
        var reading = Assert.IsType<TarotReading>(viewModel.CurrentReading);

        viewModel.RevealAndSelect(reading.Cards[0].PositionId);

        var first = Assert.Single(resolver.PositionCalls);
        Assert.Equal(reading.Cards[0].Card.Id, first.CardId);
        Assert.DoesNotContain(resolver.PositionCalls, call => call.CardId == reading.Cards[1].Card.Id);
        Assert.DoesNotContain(resolver.PositionCalls, call => call.CardId == reading.Cards[2].Card.Id);

        resolver.PositionCalls.Clear();
        viewModel.RevealAndSelect(reading.Cards[1].PositionId);

        Assert.Equal(2, resolver.PositionCalls.Count);
        Assert.Equal(
            reading.Cards.Take(2).Select(static card => card.Card.Id).OrderBy(static id => id.Value),
            resolver.PositionCalls.Select(static call => call.CardId).OrderBy(static id => id.Value));
        Assert.DoesNotContain(resolver.PositionCalls, call => call.CardId == reading.Cards[2].Card.Id);
        Assert.Equal(2, coordinator.Current.ThreeCardPositions.Count);
    }

    [Fact]
    public void ThreeCards_NonLinearRevealAddsOnlyPermittedBlocksAndNeverRequestsPastFuture()
    {
        var resolver = new RecordingResolver(resolveThreeCards: true);
        var viewModel = Workspace(autoRevealCards: false, new SequenceRandomSource(0, 0, 0));
        viewModel.SelectSpread(StandardTarotSpreads.ThreeCards.Id);
        using var coordinator = Coordinator(resolver, viewModel, new RecordingLabelSource());
        viewModel.Draw(Instant.FromUnixTimeTicks(41));
        var reading = Assert.IsType<TarotReading>(viewModel.CurrentReading);
        var past = reading.Cards.Single(static card => card.PositionId.Value == "past");
        var present = reading.Cards.Single(static card => card.PositionId.Value == "present");
        var future = reading.Cards.Single(static card => card.PositionId.Value == "future");

        Assert.Same(TarotWorkspaceInterpretationSnapshot.Empty, coordinator.Current);
        viewModel.RevealAndSelect(future.PositionId);

        Assert.Equal(["future"], Assert.IsType<TarotThreeCardInterpretationPresentation>(
            coordinator.Current.ThreeCardPresentation).Blocks.Select(static block => block.BlockId));
        Assert.Empty(resolver.ThreeCardRelationCalls);
        Assert.Empty(resolver.SynthesisCalls);

        resolver.ClearThreeCardCalls();
        viewModel.RevealAndSelect(past.PositionId);

        Assert.Equal(["past", "future"], Assert.IsType<TarotThreeCardInterpretationPresentation>(
            coordinator.Current.ThreeCardPresentation).Blocks.Select(static block => block.BlockId));
        Assert.Equal(2, resolver.PositionCalls.Count);
        Assert.Empty(resolver.ThreeCardRelationCalls);
        Assert.Empty(resolver.SynthesisCalls);
        Assert.Null(coordinator.Current.ThreeCardSynthesis);

        resolver.ClearThreeCardCalls();
        viewModel.RevealAndSelect(present.PositionId);

        Assert.Equal(
            ["past", "past-present", "present", "present-future", "future", "overall"],
            Assert.IsType<TarotThreeCardInterpretationPresentation>(
                coordinator.Current.ThreeCardPresentation).Blocks.Select(static block => block.BlockId));
        Assert.Equal(3, resolver.PositionCalls.Count);
        Assert.Collection(
            resolver.ThreeCardRelationCalls,
            call => Assert.Equal(
                (past.Card.Id, past.Orientation, present.Card.Id, present.Orientation),
                (call.FirstCardId, call.FirstOrientation, call.SecondCardId, call.SecondOrientation)),
            call => Assert.Equal(
                (present.Card.Id, present.Orientation, future.Card.Id, future.Orientation),
                (call.FirstCardId, call.FirstOrientation, call.SecondCardId, call.SecondOrientation)));
        Assert.DoesNotContain(
            resolver.ThreeCardRelationCalls,
            call =>
                (call.FirstCardId == past.Card.Id && call.SecondCardId == future.Card.Id) ||
                (call.FirstCardId == future.Card.Id && call.SecondCardId == past.Card.Id));
        Assert.Equal(
            [TarotThreeCardRelationId.PastPresent, TarotThreeCardRelationId.PresentFuture],
            coordinator.Current.ThreeCardRelations.Select(static item => item.RelationId));
        Assert.Collection(
            resolver.SynthesisCalls,
            call =>
            {
                Assert.Equal(TarotSynthesisResourceType.TrajectoryProfile, call.ResourceType);
                Assert.Equal(TarotThreeCardSynthesisContract.Improving, call.ResourceId.Value);
            },
            call =>
            {
                Assert.Equal(TarotSynthesisResourceType.SynthesisFragment, call.ResourceType);
                Assert.Equal(TarotThreeCardSynthesisContract.UnevenInfluence, call.ResourceId.Value);
            });
        var synthesis = Assert.IsType<TarotThreeCardSynthesisSelection>(coordinator.Current.ThreeCardSynthesis);
        Assert.Equal(TarotThreeCardSynthesisContract.Improving, synthesis.Plan.TrajectoryProfileId.Value);
        Assert.Equal(TarotThreeCardSynthesisContract.UnevenInfluence, synthesis.Plan.SynthesisFragmentId.Value);
        Assert.True(coordinator.Current.HasResolvedContent);
    }

    [Fact]
    public void ThreeCards_TwoAdjacentRevealsAddOnlyTheirRelationWithoutOverall()
    {
        var resolver = new RecordingResolver(resolveThreeCards: true);
        var viewModel = Workspace(autoRevealCards: false, new SequenceRandomSource(0, 0, 0));
        viewModel.SelectSpread(StandardTarotSpreads.ThreeCards.Id);
        using var coordinator = Coordinator(resolver, viewModel, new RecordingLabelSource());
        viewModel.Draw(Instant.FromUnixTimeTicks(42));
        var reading = Assert.IsType<TarotReading>(viewModel.CurrentReading);

        viewModel.RevealAndSelect(reading.Cards[0].PositionId);
        resolver.ClearThreeCardCalls();
        viewModel.RevealAndSelect(reading.Cards[1].PositionId);

        Assert.Equal(
            ["past", "past-present", "present"],
            Assert.IsType<TarotThreeCardInterpretationPresentation>(
                coordinator.Current.ThreeCardPresentation).Blocks.Select(static block => block.BlockId));
        Assert.Equal(TarotThreeCardRelationId.PastPresent, Assert.Single(coordinator.Current.ThreeCardRelations).RelationId);
        Assert.Single(resolver.ThreeCardRelationCalls);
        Assert.Empty(resolver.SynthesisCalls);
        Assert.Null(coordinator.Current.ThreeCardSynthesis);
    }

    [Fact]
    public void ThreeCards_AutoRevealBuildsCompleteSnapshotOnlyAfterAllFiveSemanticEntriesResolve()
    {
        var resolvedResolver = new RecordingResolver(resolveThreeCards: true);
        var viewModel = Workspace(autoRevealCards: true, new SequenceRandomSource(0, 0, 0));
        viewModel.SelectSpread(StandardTarotSpreads.ThreeCards.Id);
        using var coordinator = Coordinator(resolvedResolver, viewModel, new RecordingLabelSource());

        viewModel.Draw(Instant.FromUnixTimeTicks(43));

        Assert.True(viewModel.AreAllCardsRevealed);
        Assert.Equal(3, coordinator.Current.ThreeCardPositions.Count);
        Assert.Equal(2, coordinator.Current.ThreeCardRelations.Count);
        Assert.NotNull(coordinator.Current.ThreeCardSynthesis);
        Assert.Equal(6, Assert.IsType<TarotThreeCardInterpretationPresentation>(
            coordinator.Current.ThreeCardPresentation).Blocks.Count);

        var unresolvedResolver = new RecordingResolver();
        coordinator.ReplaceResolver(unresolvedResolver);

        Assert.Equal(3, unresolvedResolver.PositionCalls.Count);
        Assert.Equal(2, unresolvedResolver.ThreeCardRelationCalls.Count);
        Assert.Empty(unresolvedResolver.SynthesisCalls);
        Assert.Null(coordinator.Current.ThreeCardSynthesis);
        Assert.Null(coordinator.Current.ThreeCardPresentation);
        Assert.False(coordinator.Current.HasResolvedContent);
    }

    [Fact]
    public void ThreeCards_RedrawLanguageAndSpreadSwitchReplaceSnapshotWithoutStaleBlocks()
    {
        var resolver = new RecordingResolver(resolveThreeCards: true, resolvedLocaleFollowsRequest: true);
        var viewModel = Workspace(autoRevealCards: true, new SequenceRandomSource(0, 0, 0, 1, 0, 0, 0));
        viewModel.SelectSpread(StandardTarotSpreads.ThreeCards.Id);
        using var coordinator = Coordinator(resolver, viewModel, new RecordingLabelSource());
        viewModel.Draw(Instant.FromUnixTimeTicks(44));
        var first = Assert.IsType<TarotThreeCardInterpretationPresentation>(coordinator.Current.ThreeCardPresentation);

        coordinator.SetInterpretationLanguage(new(new LanguageCode("en")));
        var english = Assert.IsType<TarotThreeCardInterpretationPresentation>(coordinator.Current.ThreeCardPresentation);
        Assert.Equal("en", english.ResolvedLocale.Value);
        Assert.All(english.Positions, static block => Assert.StartsWith("en ", block.Label, StringComparison.Ordinal));
        Assert.All(english.Relations, static block => Assert.StartsWith("en ", block.Label, StringComparison.Ordinal));

        viewModel.Draw(Instant.FromUnixTimeTicks(45));
        var redrawn = Assert.IsType<TarotThreeCardInterpretationPresentation>(coordinator.Current.ThreeCardPresentation);
        Assert.NotSame(first, redrawn);
        Assert.Equal(6, redrawn.Blocks.Count);

        viewModel.SelectSpread(StandardTarotSpreads.SingleCard.Id);

        Assert.Same(TarotWorkspaceInterpretationSnapshot.Empty, coordinator.Current);
        Assert.Empty(coordinator.Current.ThreeCardPositions);
        Assert.Empty(coordinator.Current.ThreeCardRelations);
        Assert.Null(coordinator.Current.ThreeCardSynthesis);
        Assert.Null(coordinator.Current.ThreeCardPresentation);
    }

    [Fact]
    public void PackAndInterpretationLanguageSwitchesReResolveWithoutChangingReadingOrReveal()
    {
        var resolver = new RecordingResolver();
        var second = new TarotInterpretationPackOption(new("second-pack"));
        var viewModel = Workspace(
            autoRevealCards: false,
            new SequenceRandomSource(0),
            [new(TarotPrototypeSelections.InterpretationPackId), second]);
        using var coordinator = Coordinator(resolver, viewModel);
        viewModel.Draw(Instant.FromUnixTimeTicks(5));
        var reading = Assert.IsType<TarotReading>(viewModel.CurrentReading);
        var card = Assert.Single(reading.Cards);
        viewModel.RevealAndSelect(card.PositionId);
        resolver.SingleCalls.Clear();

        viewModel.SelectInterpretationPack(second.Id);

        var packCall = Assert.Single(resolver.SingleCalls);
        Assert.Equal(second.Id, packCall.PackId);
        Assert.Equal(new TarotInterpretationLocale("ru"), packCall.Locale);
        Assert.Same(reading, viewModel.CurrentReading);
        Assert.True(viewModel.IsRevealed(card.PositionId));

        resolver.SingleCalls.Clear();
        coordinator.SetInterpretationLanguage(new(new LanguageCode("en")));

        var languageCall = Assert.Single(resolver.SingleCalls);
        Assert.Equal(second.Id, languageCall.PackId);
        Assert.Equal(new TarotInterpretationLocale("en"), languageCall.Locale);
        Assert.Same(reading, viewModel.CurrentReading);
        Assert.True(viewModel.IsRevealed(card.PositionId));
    }

    [Fact]
    public void UiLanguageDisplayChange_DoesNotChangeRequestedInterpretationLocale()
    {
        var composition = TarotInterpretationComposition.CreateBuiltIn();
        var resolver = new RecordingResolver();
        var viewModel = Workspace(autoRevealCards: true, new SequenceRandomSource(0));
        using var coordinator = Coordinator(resolver, viewModel);
        viewModel.Draw(Instant.FromUnixTimeTicks(6));
        resolver.SingleCalls.Clear();

        var english = composition.PackCatalog.ResolveDisplayName(viewModel.InterpretationPackId, new("en"));

        Assert.Equal("Classic", english);
        Assert.Empty(resolver.SingleCalls);
        Assert.IsType<NoTarotInterpretationContent<TarotSingleCardEntry>>(coordinator.Current.SingleCard);
    }

    [Fact]
    public void ResolverReplacementAndNoContent_ClearPreviousTypedResolvedSnapshot()
    {
        var resolvedResolver = new RecordingResolver(resolveSingle: true);
        var viewModel = Workspace(autoRevealCards: true, new SequenceRandomSource(0));
        using var coordinator = Coordinator(resolvedResolver, viewModel, new RecordingLabelSource());
        viewModel.Draw(Instant.FromUnixTimeTicks(7));
        var resolved = Assert.IsType<ResolvedTarotInterpretation<TarotSingleCardEntry>>(coordinator.Current.SingleCard);
        Assert.Equal("classic", resolved.PackId.Value);
        Assert.Equal(7, resolved.ContentVersion);
        Assert.Equal("ru", resolved.RequestedLocale.Value);
        Assert.Equal("en", resolved.ResolvedLocale.Value);
        Assert.True(coordinator.Current.HasResolvedContent);
        Assert.NotNull(coordinator.Current.SingleCardPresentation);

        var noContentResolver = new RecordingResolver(
            diagnostic: new TarotResolutionDiagnostic("manifest.sha256", "Technical trust-chain detail."));
        coordinator.ReplaceResolver(noContentResolver);

        var absent = Assert.IsType<NoTarotInterpretationContent<TarotSingleCardEntry>>(coordinator.Current.SingleCard);
        Assert.Equal(TarotNoContentReason.BrokenReadyModule, absent.Reason);
        Assert.False(coordinator.Current.HasResolvedContent);
        Assert.Null(coordinator.Current.SingleCardPresentation);
        Assert.DoesNotContain("Technical", coordinator.Current.GetType().GetProperties().Select(static property => property.Name));
        Assert.Single(noContentResolver.SingleCalls);
    }

    [Fact]
    public void InterpretationLanguageRefresh_RebuildsLabelsAndPreservesDeterministicConcepts()
    {
        var resolver = new RecordingResolver(resolveSingle: true, resolvedLocaleFollowsRequest: true);
        var viewModel = Workspace(autoRevealCards: true, new SequenceRandomSource(0));
        using var coordinator = Coordinator(resolver, viewModel, new RecordingLabelSource());
        viewModel.Draw(Instant.FromUnixTimeTicks(71));
        var reading = Assert.IsType<TarotReading>(viewModel.CurrentReading);
        var russian = Assert.IsType<TarotSingleCardInterpretationPresentation>(
            coordinator.Current.SingleCardPresentation);

        coordinator.SetInterpretationLanguage(new(new LanguageCode("en")));

        var english = Assert.IsType<TarotSingleCardInterpretationPresentation>(
            coordinator.Current.SingleCardPresentation);
        Assert.Equal(3, russian.Tags.Count);
        Assert.Equal(
            russian.Tags.Select(static tag => tag.ConceptId),
            english.Tags.Select(static tag => tag.ConceptId));
        Assert.All(russian.Tags, tag => Assert.StartsWith("ru ", tag.Label, StringComparison.Ordinal));
        Assert.All(english.Tags, tag => Assert.StartsWith("en ", tag.Label, StringComparison.Ordinal));
        Assert.Same(reading, viewModel.CurrentReading);
        Assert.True(viewModel.AreAllCardsRevealed);
    }

    [Fact]
    public void SpreadChangeClearsReadingAndInterpretationWithoutAnotherEntryCall()
    {
        var resolver = new RecordingResolver();
        var viewModel = Workspace(autoRevealCards: true, new SequenceRandomSource(0));
        using var coordinator = Coordinator(resolver, viewModel);
        viewModel.Draw(Instant.FromUnixTimeTicks(8));
        Assert.Single(resolver.SingleCalls);

        viewModel.SelectSpread(StandardTarotSpreads.ThreeCards.Id);

        Assert.Null(viewModel.CurrentReading);
        Assert.Same(TarotWorkspaceInterpretationSnapshot.Empty, coordinator.Current);
        Assert.Single(resolver.SingleCalls);
        Assert.Empty(resolver.PositionCalls);
    }

    private static TarotWorkspaceInterpretationCoordinator Coordinator(
        ITarotWorkspaceInterpretationResolver resolver,
        TarotWorkspaceViewModel viewModel,
        ITarotInterpretationPresentationLabelSource? labelSource = null) =>
        new(resolver, viewModel, new InterpretationLanguagePreference(new LanguageCode("ru")), labelSource);

    private static TarotWorkspaceViewModel Workspace(
        bool autoRevealCards,
        ITarotRandomSource random,
        IEnumerable<TarotInterpretationPackOption>? packs = null) =>
        TarotWorkspaceViewModel.CreateClassic(
            new TarotDrawEngine(random),
            interpretationPacks: packs,
            initialPreferences: TarotWorkspacePreferences.CreateDefault() with
            {
                AutoRevealCards = autoRevealCards
            });

    private sealed class RecordingResolver(
        bool resolveSingle = false,
        bool resolvePair = false,
        bool resolveThreeCards = false,
        TarotResolutionDiagnostic? diagnostic = null,
        bool resolvedLocaleFollowsRequest = false) : ITarotWorkspaceInterpretationResolver
    {
        public List<SingleCall> SingleCalls { get; } = [];
        public List<PairCall> PairCalls { get; } = [];
        public List<PositionCall> PositionCalls { get; } = [];
        public List<PairCall> ThreeCardRelationCalls { get; } = [];
        public List<SynthesisCall> SynthesisCalls { get; } = [];

        public void ClearThreeCardCalls()
        {
            PositionCalls.Clear();
            ThreeCardRelationCalls.Clear();
            SynthesisCalls.Clear();
        }

        public TarotInterpretationResolution<TarotSingleCardEntry> ResolveSingleCard(
            TarotInterpretationPackId packId,
            TarotInterpretationLocale requestedLocale,
            TarotCardId cardId,
            TarotCardOrientation orientation)
        {
            SingleCalls.Add(new(packId, requestedLocale, cardId, orientation));
            if (!resolveSingle)
            {
                return new NoTarotInterpretationContent<TarotSingleCardEntry>(
                    diagnostic is null ? TarotNoContentReason.NoReadyLocale : TarotNoContentReason.BrokenReadyModule,
                    diagnostic);
            }

            var content = new TarotSingleCardEntry(
                cardId,
                orientation,
                new Dictionary<string,string>(StringComparer.Ordinal)
                {
                    ["situation"]="Synthetic situation",["development"]="Synthetic development",["risk"]="Synthetic risk",["outcome"]="Synthetic outcome",["advice"]="Synthetic advice"
                },
                Enumerable.Range(1,5).Select(index=>new TarotTagAssignment(new($"synthetic-{index}"),index%3-1,index%3+1)),
                0,
                2,
                orientation == TarotCardOrientation.Reversed ? [TarotReversalMechanism.Blocked] : []);
            return new ResolvedTarotInterpretation<TarotSingleCardEntry>(
                packId,
                7,
                TarotInterpretationMode.SingleCard,
                requestedLocale,
                resolvedLocaleFollowsRequest ? requestedLocale : new TarotInterpretationLocale("en"),
                content);
        }

        public TarotInterpretationResolution<TarotThreeCardPositionEntry> ResolveThreeCardPosition(
            TarotInterpretationPackId packId,
            TarotInterpretationLocale requestedLocale,
            TarotThreeCardPosition position,
            TarotCardId cardId,
            TarotCardOrientation orientation)
        {
            PositionCalls.Add(new(packId, requestedLocale, position, cardId, orientation));
            if (!resolveThreeCards)
            {
                return new NoTarotInterpretationContent<TarotThreeCardPositionEntry>(TarotNoContentReason.NoReadyLocale);
            }

            var (valence, intensity) = position switch
            {
                TarotThreeCardPosition.Past => (-2, 1),
                TarotThreeCardPosition.Present => (0, 2),
                TarotThreeCardPosition.Future => (2, 3),
                _ => throw new ArgumentOutOfRangeException(nameof(position))
            };
            return new ResolvedTarotInterpretation<TarotThreeCardPositionEntry>(
                packId,
                7,
                TarotInterpretationMode.ThreeCards,
                requestedLocale,
                resolvedLocaleFollowsRequest ? requestedLocale : new("ru"),
                new(
                    position,
                    cardId,
                    orientation,
                    $"Synthetic {position} position",
                    [new(new($"position-{position.ToString().ToLowerInvariant()}"), valence, intensity)],
                    valence,
                    intensity));
        }

        public TarotInterpretationResolution<TarotOrientedPairEntry> ResolveThreeCardRelation(
            TarotInterpretationPackId packId,
            TarotInterpretationLocale requestedLocale,
            TarotCardId firstCardId,
            TarotCardOrientation firstOrientation,
            TarotCardId secondCardId,
            TarotCardOrientation secondOrientation)
        {
            ThreeCardRelationCalls.Add(new(
                packId,
                requestedLocale,
                firstCardId,
                firstOrientation,
                secondCardId,
                secondOrientation));
            if (!resolveThreeCards)
            {
                return new NoTarotInterpretationContent<TarotOrientedPairEntry>(TarotNoContentReason.NoReadyLocale);
            }

            var isPastPresent = ThreeCardRelationCalls.Count % 2 == 1;
            var (valence, intensity, tagId) = isPastPresent
                ? (-1, 3, "relation-past-present")
                : (1, 1, "relation-present-future");
            var pair = Assert.IsType<TarotCanonicalPair>(TarotInterpretationKeys.CanonicalizePair(
                firstCardId,
                firstOrientation,
                secondCardId,
                secondOrientation).Value);
            return new ResolvedTarotInterpretation<TarotOrientedPairEntry>(
                packId,
                7,
                TarotInterpretationMode.ThreeCards,
                requestedLocale,
                resolvedLocaleFollowsRequest ? requestedLocale : new("ru"),
                new(
                    pair.CardAId,
                    pair.CardBId,
                    pair.OrientationState,
                    isPastPresent ? "Synthetic past-present interaction" : "Synthetic present-future interaction",
                    isPastPresent ? "Synthetic past-present direction" : "Synthetic present-future direction",
                    [new(new(tagId), valence, intensity)],
                    valence,
                    intensity));
        }

        public TarotInterpretationResolution<TarotSynthesisResource> ResolveThreeCardSynthesisResource(
            TarotInterpretationPackId packId,
            TarotInterpretationLocale requestedLocale,
            TarotSynthesisResourceType resourceType,
            TarotSynthesisResourceId resourceId)
        {
            SynthesisCalls.Add(new(packId, requestedLocale, resourceType, resourceId));
            if (!resolveThreeCards)
            {
                return new NoTarotInterpretationContent<TarotSynthesisResource>(TarotNoContentReason.NoReadyLocale);
            }

            return new ResolvedTarotInterpretation<TarotSynthesisResource>(
                packId,
                7,
                TarotInterpretationMode.ThreeCards,
                requestedLocale,
                resolvedLocaleFollowsRequest ? requestedLocale : new("ru"),
                new(resourceType, resourceId, $"Synthetic synthesis {resourceId.Value}", "{\"text\":\"synthetic\"}\n"));
        }

        public TarotInterpretationResolution<TarotOrientedPairEntry> ResolveOrientedPair(
            TarotInterpretationPackId packId,
            TarotInterpretationLocale requestedLocale,
            TarotCardId firstCardId,
            TarotCardOrientation firstOrientation,
            TarotCardId secondCardId,
            TarotCardOrientation secondOrientation)
        {
            PairCalls.Add(new(
                packId,
                requestedLocale,
                firstCardId,
                firstOrientation,
                secondCardId,
                secondOrientation));
            if (!resolvePair)
            {
                return new NoTarotInterpretationContent<TarotOrientedPairEntry>(
                    diagnostic is null ? TarotNoContentReason.NoReadyLocale : TarotNoContentReason.BrokenReadyModule,
                    diagnostic);
            }

            var canonical = TarotInterpretationKeys.CanonicalizePair(
                firstCardId,
                firstOrientation,
                secondCardId,
                secondOrientation);
            var pair = Assert.IsType<TarotCanonicalPair>(canonical.Value);
            var content = new TarotOrientedPairEntry(
                pair.CardAId,
                pair.CardBId,
                pair.OrientationState,
                "Synthetic pair interaction",
                "Synthetic pair direction",
                Enumerable.Range(1, 3).Select(index =>
                    new TarotTagAssignment(new($"synthetic-{index}"), index - 2, index)),
                1,
                3);
            return new ResolvedTarotInterpretation<TarotOrientedPairEntry>(
                packId,
                7,
                TarotInterpretationMode.TwoCards,
                requestedLocale,
                resolvedLocaleFollowsRequest ? requestedLocale : new TarotInterpretationLocale("en"),
                content);
        }
    }

    private sealed class RecordingLabelSource : ITarotInterpretationPresentationLabelSource
    {
        public TarotInterpretationPresentationLabels Resolve(
            TarotInterpretationPackId packId,
            int contentVersion,
            TarotInterpretationLocale resolvedLocale) => new(
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["situation"] = $"{resolvedLocale.Value} situation",
                ["development"] = $"{resolvedLocale.Value} development",
                ["risk"] = $"{resolvedLocale.Value} risk",
                ["outcome"] = $"{resolvedLocale.Value} outcome",
                ["advice"] = $"{resolvedLocale.Value} advice"
            },
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["past"] = $"{resolvedLocale.Value} past",
                ["present"] = $"{resolvedLocale.Value} present",
                ["future"] = $"{resolvedLocale.Value} future"
            },
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["past-present"] = $"{resolvedLocale.Value} past-present",
                ["present-future"] = $"{resolvedLocale.Value} present-future",
                ["overall"] = $"{resolvedLocale.Value} overall"
            },
            Enumerable.Range(1, 5).ToDictionary(
                static index => new TarotTagConceptId($"synthetic-{index}"),
                index => $"{resolvedLocale.Value} tag {index}")
                .Concat(new Dictionary<TarotTagConceptId, string>
                {
                    [new("position-past")] = $"{resolvedLocale.Value} position past",
                    [new("position-present")] = $"{resolvedLocale.Value} position present",
                    [new("position-future")] = $"{resolvedLocale.Value} position future",
                    [new("relation-past-present")] = $"{resolvedLocale.Value} relation past-present",
                    [new("relation-present-future")] = $"{resolvedLocale.Value} relation present-future"
                })
                .ToDictionary(static pair => pair.Key, static pair => pair.Value));
    }

    private sealed record SingleCall(
        TarotInterpretationPackId PackId,
        TarotInterpretationLocale Locale,
        TarotCardId CardId,
        TarotCardOrientation Orientation);

    private sealed record PairCall(
        TarotInterpretationPackId PackId,
        TarotInterpretationLocale Locale,
        TarotCardId FirstCardId,
        TarotCardOrientation FirstOrientation,
        TarotCardId SecondCardId,
        TarotCardOrientation SecondOrientation);

    private sealed record PositionCall(
        TarotInterpretationPackId PackId,
        TarotInterpretationLocale Locale,
        TarotThreeCardPosition Position,
        TarotCardId CardId,
        TarotCardOrientation Orientation);

    private sealed record SynthesisCall(
        TarotInterpretationPackId PackId,
        TarotInterpretationLocale Locale,
        TarotSynthesisResourceType ResourceType,
        TarotSynthesisResourceId ResourceId);

    private sealed class SequenceRandomSource(params int[] values) : ITarotRandomSource
    {
        private readonly Queue<int> values = new(values);

        public int NextIndex(int exclusiveUpperBound) => values.Dequeue();
    }
}
