using System.Collections.ObjectModel;
using NoxAeterna.Domain.Tarot;
using NoxAeterna.Interpretation.Tarot.Contracts;
using NoxAeterna.Interpretation.Tarot.Resolution;
using NoxAeterna.Interpretation.Tarot.Storage;
using NoxAeterna.Presentation.Preferences;
using NoxAeterna.Presentation.Tarot;

namespace NoxAeterna.App.Tarot;

/// <summary>Narrow App adapter over the Interpretation-owned semantic resolver.</summary>
public interface ITarotWorkspaceInterpretationResolver
{
    TarotInterpretationResolution<TarotSingleCardEntry> ResolveSingleCard(
        TarotInterpretationPackId packId,
        TarotInterpretationLocale requestedLocale,
        TarotCardId cardId,
        TarotCardOrientation orientation);

    TarotInterpretationResolution<TarotOrientedPairEntry> ResolveOrientedPair(
        TarotInterpretationPackId packId,
        TarotInterpretationLocale requestedLocale,
        TarotCardId firstCardId,
        TarotCardOrientation firstOrientation,
        TarotCardId secondCardId,
        TarotCardOrientation secondOrientation);

    TarotInterpretationResolution<TarotThreeCardPositionEntry> ResolveThreeCardPosition(
        TarotInterpretationPackId packId,
        TarotInterpretationLocale requestedLocale,
        TarotThreeCardPosition position,
        TarotCardId cardId,
        TarotCardOrientation orientation);

    TarotInterpretationResolution<TarotOrientedPairEntry> ResolveThreeCardRelation(
        TarotInterpretationPackId packId,
        TarotInterpretationLocale requestedLocale,
        TarotCardId firstCardId,
        TarotCardOrientation firstOrientation,
        TarotCardId secondCardId,
        TarotCardOrientation secondOrientation);

    TarotInterpretationResolution<TarotSynthesisResource> ResolveThreeCardSynthesisResource(
        TarotInterpretationPackId packId,
        TarotInterpretationLocale requestedLocale,
        TarotSynthesisResourceType resourceType,
        TarotSynthesisResourceId resourceId);
}

/// <summary>Forwards exact entry requests without duplicating fallback or readiness policy.</summary>
public sealed class TarotWorkspaceInterpretationResolverAdapter(TarotInterpretationPackResolver resolver)
    : ITarotWorkspaceInterpretationResolver
{
    private readonly TarotInterpretationPackResolver resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));

    public TarotInterpretationResolution<TarotSingleCardEntry> ResolveSingleCard(
        TarotInterpretationPackId packId,
        TarotInterpretationLocale requestedLocale,
        TarotCardId cardId,
        TarotCardOrientation orientation) =>
        resolver.ResolveSingleCard(packId, requestedLocale, cardId, orientation);

    public TarotInterpretationResolution<TarotOrientedPairEntry> ResolveOrientedPair(
        TarotInterpretationPackId packId,
        TarotInterpretationLocale requestedLocale,
        TarotCardId firstCardId,
        TarotCardOrientation firstOrientation,
        TarotCardId secondCardId,
        TarotCardOrientation secondOrientation) =>
        resolver.ResolveOrientedPair(
            packId,
            TarotInterpretationMode.TwoCards,
            requestedLocale,
            firstCardId,
            firstOrientation,
            secondCardId,
            secondOrientation);

    public TarotInterpretationResolution<TarotThreeCardPositionEntry> ResolveThreeCardPosition(
        TarotInterpretationPackId packId,
        TarotInterpretationLocale requestedLocale,
        TarotThreeCardPosition position,
        TarotCardId cardId,
        TarotCardOrientation orientation) =>
        resolver.ResolveThreeCardPosition(packId, requestedLocale, position, cardId, orientation);

    public TarotInterpretationResolution<TarotOrientedPairEntry> ResolveThreeCardRelation(
        TarotInterpretationPackId packId,
        TarotInterpretationLocale requestedLocale,
        TarotCardId firstCardId,
        TarotCardOrientation firstOrientation,
        TarotCardId secondCardId,
        TarotCardOrientation secondOrientation) =>
        resolver.ResolveOrientedPair(
            packId,
            TarotInterpretationMode.ThreeCards,
            requestedLocale,
            firstCardId,
            firstOrientation,
            secondCardId,
            secondOrientation);

    public TarotInterpretationResolution<TarotSynthesisResource> ResolveThreeCardSynthesisResource(
        TarotInterpretationPackId packId,
        TarotInterpretationLocale requestedLocale,
        TarotSynthesisResourceType resourceType,
        TarotSynthesisResourceId resourceId) =>
        resolver.ResolveThreeCardSynthesisResource(packId, requestedLocale, resourceType, resourceId);
}

/// <summary>Supplies already trusted pack-local labels without placing filesystem work in Presentation.</summary>
public interface ITarotInterpretationPresentationLabelSource
{
    TarotInterpretationPresentationLabels? Resolve(
        TarotInterpretationPackId packId,
        int contentVersion,
        TarotInterpretationLocale resolvedLocale);
}

/// <summary>Keeps production presentation silent until trusted pack-local label routing is available.</summary>
public sealed class EmptyTarotInterpretationPresentationLabelSource : ITarotInterpretationPresentationLabelSource
{
    public static EmptyTarotInterpretationPresentationLabelSource Instance { get; } = new();

    private EmptyTarotInterpretationPresentationLabelSource()
    {
    }

    public TarotInterpretationPresentationLabels? Resolve(
        TarotInterpretationPackId packId,
        int contentVersion,
        TarotInterpretationLocale resolvedLocale) => null;
}

/// <summary>Maps trusted package-local labels and vocabulary into the pure Presentation model.</summary>
public sealed class TarotPackagePresentationLabelSource(ITarotInterpretationPackStoreCatalog catalog) : ITarotInterpretationPresentationLabelSource
{
    private readonly ITarotInterpretationPackStoreCatalog catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
    public TarotInterpretationPresentationLabels? Resolve(TarotInterpretationPackId packId,int contentVersion,TarotInterpretationLocale resolvedLocale)
    {
        if(!catalog.TryGetStore(packId,out var store)||store is null||store.Manifest.ContentVersion!=contentVersion)return null;
        var result=store.GetLabels(resolvedLocale);return result.Status==TarotInterpretationStoreStatus.Found&&result.Value is { } labels
            ? new TarotInterpretationPresentationLabels(
                labels.Labels.SingleCardSections,
                labels.Labels.ThreeCardPositions,
                labels.Labels.Relations,
                labels.TagLabels)
            : null;
    }
}

/// <summary>One explicitly adjacent Three Cards relation resolution.</summary>
public sealed record TarotThreeCardRelationResolution(
    TarotThreeCardRelationId RelationId,
    TarotInterpretationResolution<TarotOrientedPairEntry> Resolution);

/// <summary>The deterministic plan and its two exact package resource lookups.</summary>
public sealed record TarotThreeCardSynthesisSelection(
    TarotThreeCardSynthesisPlan Plan,
    TarotInterpretationResolution<TarotSynthesisResource> TrajectoryProfile,
    TarotInterpretationResolution<TarotSynthesisResource> SynthesisFragment);

/// <summary>Captures typed entry results for only the currently revealed workspace inputs.</summary>
public sealed class TarotWorkspaceInterpretationSnapshot
{
    public static TarotWorkspaceInterpretationSnapshot Empty { get; } = new(
        null,
        null,
        new Dictionary<TarotSpreadPositionId, TarotInterpretationResolution<TarotThreeCardPositionEntry>>(),
        null,
        null,
        [],
        null,
        null);

    public TarotWorkspaceInterpretationSnapshot(
        TarotInterpretationResolution<TarotSingleCardEntry>? singleCard,
        TarotInterpretationResolution<TarotOrientedPairEntry>? twoCardPair,
        IReadOnlyDictionary<TarotSpreadPositionId, TarotInterpretationResolution<TarotThreeCardPositionEntry>> threeCardPositions,
        TarotSingleCardInterpretationPresentation? singleCardPresentation = null,
        TarotOrientedPairInterpretationPresentation? twoCardPresentation = null,
        IEnumerable<TarotThreeCardRelationResolution>? threeCardRelations = null,
        TarotThreeCardSynthesisSelection? threeCardSynthesis = null,
        TarotThreeCardInterpretationPresentation? threeCardPresentation = null)
    {
        SingleCard = singleCard;
        TwoCardPair = twoCardPair;
        ThreeCardPositions = new ReadOnlyDictionary<TarotSpreadPositionId, TarotInterpretationResolution<TarotThreeCardPositionEntry>>(
            threeCardPositions.ToDictionary(static pair => pair.Key, static pair => pair.Value));
        SingleCardPresentation = singleCardPresentation;
        TwoCardPresentation = twoCardPresentation;
        ThreeCardRelations = Array.AsReadOnly((threeCardRelations ?? []).ToArray());
        ThreeCardSynthesis = threeCardSynthesis;
        ThreeCardPresentation = threeCardPresentation;
    }

    public TarotInterpretationResolution<TarotSingleCardEntry>? SingleCard { get; }

    public TarotInterpretationResolution<TarotOrientedPairEntry>? TwoCardPair { get; }

    public IReadOnlyDictionary<TarotSpreadPositionId, TarotInterpretationResolution<TarotThreeCardPositionEntry>> ThreeCardPositions { get; }

    public TarotSingleCardInterpretationPresentation? SingleCardPresentation { get; }

    public TarotOrientedPairInterpretationPresentation? TwoCardPresentation { get; }

    public IReadOnlyList<TarotThreeCardRelationResolution> ThreeCardRelations { get; }

    public TarotThreeCardSynthesisSelection? ThreeCardSynthesis { get; }

    public TarotThreeCardInterpretationPresentation? ThreeCardPresentation { get; }

    public bool HasResolvedContent =>
        SingleCard is ResolvedTarotInterpretation<TarotSingleCardEntry> ||
        TwoCardPair is ResolvedTarotInterpretation<TarotOrientedPairEntry> ||
        ThreeCardPresentation is { Blocks.Count: > 0 } ||
        ThreeCardPositions.Values.Any(static item => item is ResolvedTarotInterpretation<TarotThreeCardPositionEntry>) ||
        ThreeCardRelations.Any(static item => item.Resolution is ResolvedTarotInterpretation<TarotOrientedPairEntry>);
}

/// <summary>Re-resolves current revealed cards whenever workspace or interpretation-language inputs change.</summary>
public sealed class TarotWorkspaceInterpretationCoordinator : IDisposable
{
    private ITarotWorkspaceInterpretationResolver resolver;
    private readonly TarotWorkspaceViewModel viewModel;
    private readonly ITarotInterpretationPresentationLabelSource labelSource;
    private readonly TarotSingleCardInterpretationPresentationBuilder presentationBuilder;
    private readonly TarotOrientedPairInterpretationPresentationBuilder pairPresentationBuilder;
    private readonly TarotThreeCardInterpretationPresentationBuilder threeCardPresentationBuilder;
    private InterpretationLanguagePreference language;

    public TarotWorkspaceInterpretationCoordinator(
        ITarotWorkspaceInterpretationResolver resolver,
        TarotWorkspaceViewModel viewModel,
        InterpretationLanguagePreference language,
        ITarotInterpretationPresentationLabelSource? labelSource = null,
        TarotSingleCardInterpretationPresentationBuilder? presentationBuilder = null,
        TarotOrientedPairInterpretationPresentationBuilder? pairPresentationBuilder = null,
        TarotThreeCardInterpretationPresentationBuilder? threeCardPresentationBuilder = null)
    {
        this.resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
        this.viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        this.language = language ?? throw new ArgumentNullException(nameof(language));
        this.labelSource = labelSource ?? EmptyTarotInterpretationPresentationLabelSource.Instance;
        this.presentationBuilder = presentationBuilder ?? new TarotSingleCardInterpretationPresentationBuilder();
        this.pairPresentationBuilder = pairPresentationBuilder ?? new TarotOrientedPairInterpretationPresentationBuilder();
        this.threeCardPresentationBuilder = threeCardPresentationBuilder ??
                                            new TarotThreeCardInterpretationPresentationBuilder();
        viewModel.StateChanged += OnWorkspaceStateChanged;
        Refresh();
    }

    public event EventHandler? SnapshotChanged;

    public TarotWorkspaceInterpretationSnapshot Current { get; private set; } = TarotWorkspaceInterpretationSnapshot.Empty;

    public void SetInterpretationLanguage(InterpretationLanguagePreference value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (language == value)
        {
            return;
        }

        language = value;
        Refresh();
    }

    public void ReplaceResolver(ITarotWorkspaceInterpretationResolver value)
    {
        resolver = value ?? throw new ArgumentNullException(nameof(value));
        Refresh();
    }

    public void Refresh()
    {
        var reading = viewModel.CurrentReading;
        if (reading is null || !viewModel.HasRevealedCards)
        {
            Publish(TarotWorkspaceInterpretationSnapshot.Empty);
            return;
        }

        var locale = new TarotInterpretationLocale(language.Language.Value);
        if (reading.SpreadId == StandardTarotSpreads.SingleCard.Id)
        {
            var card = reading.Cards.Single();
            if (!viewModel.IsRevealed(card.PositionId))
            {
                Publish(TarotWorkspaceInterpretationSnapshot.Empty);
                return;
            }

            var resolution = resolver.ResolveSingleCard(
                viewModel.InterpretationPackId,
                locale,
                card.Card.Id,
                card.Orientation);
            var presentation = resolution is ResolvedTarotInterpretation<TarotSingleCardEntry> resolved &&
                               labelSource.Resolve(
                                   resolved.PackId,
                                   resolved.ContentVersion,
                                   resolved.ResolvedLocale) is { } labels
                ? presentationBuilder.Build(reading, resolved, labels)
                : null;
            Publish(new TarotWorkspaceInterpretationSnapshot(
                resolution,
                null,
                new Dictionary<TarotSpreadPositionId, TarotInterpretationResolution<TarotThreeCardPositionEntry>>(),
                presentation));
            return;
        }

        if (reading.SpreadId == StandardTarotSpreads.TwoCards.Id)
        {
            if (!viewModel.AreAllCardsRevealed)
            {
                Publish(TarotWorkspaceInterpretationSnapshot.Empty);
                return;
            }

            var first = reading.Cards[0];
            var second = reading.Cards[1];
            var resolution = resolver.ResolveOrientedPair(
                viewModel.InterpretationPackId,
                locale,
                first.Card.Id,
                first.Orientation,
                second.Card.Id,
                second.Orientation);
            var presentation = resolution is ResolvedTarotInterpretation<TarotOrientedPairEntry> resolved &&
                               labelSource.Resolve(
                                   resolved.PackId,
                                   resolved.ContentVersion,
                                   resolved.ResolvedLocale) is { } labels
                ? pairPresentationBuilder.Build(reading, resolved, labels)
                : null;
            Publish(new TarotWorkspaceInterpretationSnapshot(
                null,
                resolution,
                new Dictionary<TarotSpreadPositionId, TarotInterpretationResolution<TarotThreeCardPositionEntry>>(),
                null,
                presentation));
            return;
        }

        if (reading.SpreadId == StandardTarotSpreads.ThreeCards.Id)
        {
            var positions = new Dictionary<TarotSpreadPositionId, TarotInterpretationResolution<TarotThreeCardPositionEntry>>();
            foreach (var card in reading.Cards.Where(card => viewModel.IsRevealed(card.PositionId)))
            {
                positions.Add(
                    card.PositionId,
                    resolver.ResolveThreeCardPosition(
                        viewModel.InterpretationPackId,
                        locale,
                        ToThreeCardPosition(card.PositionId),
                        card.Card.Id,
                        card.Orientation));
            }

            var relations = new List<TarotThreeCardRelationResolution>(2);
            var past = reading.Cards.Single(static card => card.PositionId.Value == "past");
            var present = reading.Cards.Single(static card => card.PositionId.Value == "present");
            var future = reading.Cards.Single(static card => card.PositionId.Value == "future");

            ResolvedTarotInterpretation<TarotOrientedPairEntry>? pastPresent = null;
            if (viewModel.IsRevealed(past.PositionId) && viewModel.IsRevealed(present.PositionId))
            {
                var resolution = resolver.ResolveThreeCardRelation(
                    viewModel.InterpretationPackId,
                    locale,
                    past.Card.Id,
                    past.Orientation,
                    present.Card.Id,
                    present.Orientation);
                relations.Add(new(TarotThreeCardRelationId.PastPresent, resolution));
                pastPresent = resolution as ResolvedTarotInterpretation<TarotOrientedPairEntry>;
            }

            ResolvedTarotInterpretation<TarotOrientedPairEntry>? presentFuture = null;
            if (viewModel.IsRevealed(present.PositionId) && viewModel.IsRevealed(future.PositionId))
            {
                var resolution = resolver.ResolveThreeCardRelation(
                    viewModel.InterpretationPackId,
                    locale,
                    present.Card.Id,
                    present.Orientation,
                    future.Card.Id,
                    future.Orientation);
                relations.Add(new(TarotThreeCardRelationId.PresentFuture, resolution));
                presentFuture = resolution as ResolvedTarotInterpretation<TarotOrientedPairEntry>;
            }

            TarotThreeCardSynthesisSelection? synthesis = null;
            TarotThreeCardResolvedSynthesis? resolvedSynthesis = null;
            var resolvedPositions = positions.Values
                .OfType<ResolvedTarotInterpretation<TarotThreeCardPositionEntry>>()
                .ToArray();
            if (viewModel.AreAllCardsRevealed &&
                resolvedPositions.Length == 3 &&
                pastPresent is not null &&
                presentFuture is not null)
            {
                var byPosition = resolvedPositions.ToDictionary(static item => item.Content.Position);
                var plan = TarotThreeCardSynthesisPlanner.Plan(new TarotThreeCardSynthesisInput(
                    byPosition[TarotThreeCardPosition.Past].Content.OverallValence,
                    byPosition[TarotThreeCardPosition.Present].Content.OverallValence,
                    byPosition[TarotThreeCardPosition.Future].Content.OverallValence,
                    pastPresent.Content.OverallValence,
                    pastPresent.Content.OverallIntensity,
                    presentFuture.Content.OverallValence,
                    presentFuture.Content.OverallIntensity));
                var trajectory = resolver.ResolveThreeCardSynthesisResource(
                    viewModel.InterpretationPackId,
                    locale,
                    TarotSynthesisResourceType.TrajectoryProfile,
                    plan.TrajectoryProfileId);
                var transition = resolver.ResolveThreeCardSynthesisResource(
                    viewModel.InterpretationPackId,
                    locale,
                    TarotSynthesisResourceType.SynthesisFragment,
                    plan.SynthesisFragmentId);
                synthesis = new(plan, trajectory, transition);
                if (trajectory is ResolvedTarotInterpretation<TarotSynthesisResource> resolvedTrajectory &&
                    transition is ResolvedTarotInterpretation<TarotSynthesisResource> resolvedTransition)
                {
                    resolvedSynthesis = new(plan, resolvedTrajectory, resolvedTransition);
                }
            }

            TarotThreeCardInterpretationPresentation? presentation = null;
            if (resolvedPositions.Length == positions.Count &&
                relations.All(static item =>
                    item.Resolution is ResolvedTarotInterpretation<TarotOrientedPairEntry>) &&
                (!viewModel.AreAllCardsRevealed || resolvedSynthesis is not null) &&
                resolvedPositions.FirstOrDefault() is { } firstResolved &&
                labelSource.Resolve(
                    firstResolved.PackId,
                    firstResolved.ContentVersion,
                    firstResolved.ResolvedLocale) is { } labels)
            {
                try
                {
                    presentation = threeCardPresentationBuilder.Build(
                        reading,
                        resolvedPositions,
                        pastPresent,
                        presentFuture,
                        resolvedSynthesis,
                        labels);
                }
                catch (ArgumentException)
                {
                    presentation = null;
                }
            }

            Publish(new TarotWorkspaceInterpretationSnapshot(
                null,
                null,
                positions,
                null,
                null,
                relations,
                synthesis,
                presentation));
            return;
        }

        Publish(TarotWorkspaceInterpretationSnapshot.Empty);
    }

    public void Dispose() => viewModel.StateChanged -= OnWorkspaceStateChanged;

    private void OnWorkspaceStateChanged(object? sender, EventArgs e) => Refresh();

    private void Publish(TarotWorkspaceInterpretationSnapshot value)
    {
        Current = value;
        SnapshotChanged?.Invoke(this, EventArgs.Empty);
    }

    private static TarotThreeCardPosition ToThreeCardPosition(TarotSpreadPositionId positionId) => positionId.Value switch
    {
        "past" => TarotThreeCardPosition.Past,
        "present" => TarotThreeCardPosition.Present,
        "future" => TarotThreeCardPosition.Future,
        _ => throw new InvalidOperationException("The three-card reading contains an unsupported position.")
    };
}
