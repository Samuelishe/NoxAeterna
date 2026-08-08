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
            ? new TarotInterpretationPresentationLabels(labels.Labels.SingleCardSections,labels.TagLabels)
            : null;
    }
}

/// <summary>Captures typed entry results for only the currently revealed workspace inputs.</summary>
public sealed class TarotWorkspaceInterpretationSnapshot
{
    public static TarotWorkspaceInterpretationSnapshot Empty { get; } = new(
        null,
        null,
        new Dictionary<TarotSpreadPositionId, TarotInterpretationResolution<TarotThreeCardPositionEntry>>(),
        null,
        null);

    public TarotWorkspaceInterpretationSnapshot(
        TarotInterpretationResolution<TarotSingleCardEntry>? singleCard,
        TarotInterpretationResolution<TarotOrientedPairEntry>? twoCardPair,
        IReadOnlyDictionary<TarotSpreadPositionId, TarotInterpretationResolution<TarotThreeCardPositionEntry>> threeCardPositions,
        TarotSingleCardInterpretationPresentation? singleCardPresentation = null,
        TarotOrientedPairInterpretationPresentation? twoCardPresentation = null)
    {
        SingleCard = singleCard;
        TwoCardPair = twoCardPair;
        ThreeCardPositions = new ReadOnlyDictionary<TarotSpreadPositionId, TarotInterpretationResolution<TarotThreeCardPositionEntry>>(
            threeCardPositions.ToDictionary(static pair => pair.Key, static pair => pair.Value));
        SingleCardPresentation = singleCardPresentation;
        TwoCardPresentation = twoCardPresentation;
    }

    public TarotInterpretationResolution<TarotSingleCardEntry>? SingleCard { get; }

    public TarotInterpretationResolution<TarotOrientedPairEntry>? TwoCardPair { get; }

    public IReadOnlyDictionary<TarotSpreadPositionId, TarotInterpretationResolution<TarotThreeCardPositionEntry>> ThreeCardPositions { get; }

    public TarotSingleCardInterpretationPresentation? SingleCardPresentation { get; }

    public TarotOrientedPairInterpretationPresentation? TwoCardPresentation { get; }

    public bool HasResolvedContent =>
        SingleCard is ResolvedTarotInterpretation<TarotSingleCardEntry> ||
        TwoCardPair is ResolvedTarotInterpretation<TarotOrientedPairEntry> ||
        ThreeCardPositions.Values.Any(static item => item is ResolvedTarotInterpretation<TarotThreeCardPositionEntry>);
}

/// <summary>Re-resolves current revealed cards whenever workspace or interpretation-language inputs change.</summary>
public sealed class TarotWorkspaceInterpretationCoordinator : IDisposable
{
    private ITarotWorkspaceInterpretationResolver resolver;
    private readonly TarotWorkspaceViewModel viewModel;
    private readonly ITarotInterpretationPresentationLabelSource labelSource;
    private readonly TarotSingleCardInterpretationPresentationBuilder presentationBuilder;
    private readonly TarotOrientedPairInterpretationPresentationBuilder pairPresentationBuilder;
    private InterpretationLanguagePreference language;

    public TarotWorkspaceInterpretationCoordinator(
        ITarotWorkspaceInterpretationResolver resolver,
        TarotWorkspaceViewModel viewModel,
        InterpretationLanguagePreference language,
        ITarotInterpretationPresentationLabelSource? labelSource = null,
        TarotSingleCardInterpretationPresentationBuilder? presentationBuilder = null,
        TarotOrientedPairInterpretationPresentationBuilder? pairPresentationBuilder = null)
    {
        this.resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
        this.viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        this.language = language ?? throw new ArgumentNullException(nameof(language));
        this.labelSource = labelSource ?? EmptyTarotInterpretationPresentationLabelSource.Instance;
        this.presentationBuilder = presentationBuilder ?? new TarotSingleCardInterpretationPresentationBuilder();
        this.pairPresentationBuilder = pairPresentationBuilder ?? new TarotOrientedPairInterpretationPresentationBuilder();
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

            Publish(new TarotWorkspaceInterpretationSnapshot(null, null, positions));
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
