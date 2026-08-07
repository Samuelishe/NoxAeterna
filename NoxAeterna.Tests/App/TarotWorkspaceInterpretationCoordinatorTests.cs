using NodaTime;
using NoxAeterna.App.Tarot;
using NoxAeterna.Domain.Tarot;
using NoxAeterna.Interpretation.Tarot.Contracts;
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
        using var coordinator = Coordinator(resolvedResolver, viewModel);
        viewModel.Draw(Instant.FromUnixTimeTicks(7));
        var resolved = Assert.IsType<ResolvedTarotInterpretation<TarotSingleCardEntry>>(coordinator.Current.SingleCard);
        Assert.Equal("classic", resolved.PackId.Value);
        Assert.Equal(7, resolved.ContentVersion);
        Assert.Equal("ru", resolved.RequestedLocale.Value);
        Assert.Equal("en", resolved.ResolvedLocale.Value);
        Assert.True(coordinator.Current.HasResolvedContent);

        var noContentResolver = new RecordingResolver(
            diagnostic: new TarotResolutionDiagnostic("manifest.sha256", "Technical trust-chain detail."));
        coordinator.ReplaceResolver(noContentResolver);

        var absent = Assert.IsType<NoTarotInterpretationContent<TarotSingleCardEntry>>(coordinator.Current.SingleCard);
        Assert.Equal(TarotNoContentReason.BrokenReadyModule, absent.Reason);
        Assert.False(coordinator.Current.HasResolvedContent);
        Assert.DoesNotContain("Technical", coordinator.Current.GetType().GetProperties().Select(static property => property.Name));
        Assert.Single(noContentResolver.SingleCalls);
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
        TarotWorkspaceViewModel viewModel) =>
        new(resolver, viewModel, new InterpretationLanguagePreference(new LanguageCode("ru")));

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
        TarotResolutionDiagnostic? diagnostic = null) : ITarotWorkspaceInterpretationResolver
    {
        public List<SingleCall> SingleCalls { get; } = [];
        public List<PositionCall> PositionCalls { get; } = [];

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

            var document = TarotInterpretationResolverTestSource.SyntheticSingleCard(cardId.Value, orientation);
            var content = TarotInterpretationValidator.ValidateSingleCard(document, StandardTarotCatalog.Deck).Value!;
            return new ResolvedTarotInterpretation<TarotSingleCardEntry>(
                packId,
                7,
                TarotInterpretationMode.SingleCard,
                requestedLocale,
                new TarotInterpretationLocale("en"),
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
            return new NoTarotInterpretationContent<TarotThreeCardPositionEntry>(TarotNoContentReason.NoReadyLocale);
        }
    }

    private sealed record SingleCall(
        TarotInterpretationPackId PackId,
        TarotInterpretationLocale Locale,
        TarotCardId CardId,
        TarotCardOrientation Orientation);

    private sealed record PositionCall(
        TarotInterpretationPackId PackId,
        TarotInterpretationLocale Locale,
        TarotThreeCardPosition Position,
        TarotCardId CardId,
        TarotCardOrientation Orientation);

    private sealed class SequenceRandomSource(params int[] values) : ITarotRandomSource
    {
        private readonly Queue<int> values = new(values);

        public int NextIndex(int exclusiveUpperBound) => values.Dequeue();
    }
}
