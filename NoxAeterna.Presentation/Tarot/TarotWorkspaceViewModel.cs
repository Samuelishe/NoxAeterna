using NodaTime;
using NoxAeterna.Domain.Tarot;
using NoxAeterna.Presentation.Localization;

namespace NoxAeterna.Presentation.Tarot;

/// <summary>Coordinates the first in-memory Tarot workspace without owning draw rules.</summary>
public sealed class TarotWorkspaceViewModel
{
    private readonly TarotDrawEngine drawEngine;
    private readonly TarotDeckDefinition deck;
    private readonly HashSet<TarotSpreadPositionId> revealedPositions = [];

    /// <summary>Initializes the workspace with explicit domain and prototype selection contracts.</summary>
    public TarotWorkspaceViewModel(
        TarotDrawEngine drawEngine,
        TarotDeckDefinition deck,
        IEnumerable<TarotSpreadOption> spreadOptions,
        IEnumerable<TarotBackVariantOption> backVariants,
        TarotArtworkPackId artworkPackId,
        TarotPresentationSkinId presentationSkinId,
        TarotInterpretationSetId interpretationSetId)
    {
        this.drawEngine = drawEngine ?? throw new ArgumentNullException(nameof(drawEngine));
        this.deck = deck ?? throw new ArgumentNullException(nameof(deck));

        var copiedSpreads = (spreadOptions ?? throw new ArgumentNullException(nameof(spreadOptions))).ToArray();
        var copiedBacks = (backVariants ?? throw new ArgumentNullException(nameof(backVariants))).ToArray();
        if (copiedSpreads.Length == 0)
        {
            throw new ArgumentException("A Tarot workspace requires at least one spread option.", nameof(spreadOptions));
        }

        if (copiedBacks.Length == 0 || copiedBacks.Select(static option => option.Id).Distinct().Count() != copiedBacks.Length)
        {
            throw new ArgumentException("Tarot back variants must contain unique options.", nameof(backVariants));
        }

        ArtworkPackId = artworkPackId ?? throw new ArgumentNullException(nameof(artworkPackId));
        PresentationSkinId = presentationSkinId ?? throw new ArgumentNullException(nameof(presentationSkinId));
        InterpretationSetId = interpretationSetId ?? throw new ArgumentNullException(nameof(interpretationSetId));
        SpreadOptions = Array.AsReadOnly(copiedSpreads);
        BackVariants = Array.AsReadOnly(copiedBacks);
        SelectedSpread = SpreadOptions[0];
        SelectedBackVariant = BackVariants[0];
    }

    /// <summary>Raised after visible workspace state changes.</summary>
    public event EventHandler? StateChanged;

    /// <summary>Gets the available built-in spread options.</summary>
    public IReadOnlyList<TarotSpreadOption> SpreadOptions { get; }

    /// <summary>Gets the selected spread option.</summary>
    public TarotSpreadOption SelectedSpread { get; private set; }

    /// <summary>Gets whether reversed cards are allowed.</summary>
    public bool AllowReversed { get; private set; }

    /// <summary>Gets the available card-back variants.</summary>
    public IReadOnlyList<TarotBackVariantOption> BackVariants { get; }

    /// <summary>Gets the selected back variant.</summary>
    public TarotBackVariantOption SelectedBackVariant { get; private set; }

    /// <summary>Gets the active prototype artwork-pack identity.</summary>
    public TarotArtworkPackId ArtworkPackId { get; }

    /// <summary>Gets the active prototype presentation-skin identity.</summary>
    public TarotPresentationSkinId PresentationSkinId { get; }

    /// <summary>Gets the foundation interpretation-set identity.</summary>
    public TarotInterpretationSetId InterpretationSetId { get; }

    /// <summary>Gets the current successful in-memory reading.</summary>
    public TarotReading? CurrentReading { get; private set; }

    /// <summary>Gets the selected revealed card assignment.</summary>
    public TarotDrawnCard? SelectedCard { get; private set; }

    /// <summary>Gets the current controlled draw failure.</summary>
    public TarotDrawFailure? CurrentFailure { get; private set; }

    /// <summary>Gets the empty-state localization key.</summary>
    public LocalizationKey EmptyStateKey { get; } = new("ui.tarot.empty-state");

    /// <summary>Gets the controlled-failure localization key.</summary>
    public LocalizationKey FailureStateKey { get; } = new("ui.tarot.failure.insufficient-deck");

    /// <summary>Gets the honest unavailable-interpretation localization key.</summary>
    public LocalizationKey InterpretationUnavailableKey { get; } = new("ui.tarot.interpretation.unavailable");

    /// <summary>Creates the default workspace over the real standard deck and spreads.</summary>
    public static TarotWorkspaceViewModel CreateFoundation(TarotDrawEngine drawEngine) => new(
        drawEngine,
        StandardTarotCatalog.Deck,
        [
            new TarotSpreadOption(StandardTarotSpreads.SingleCard, new LocalizationKey("ui.tarot.spread.single-card")),
            new TarotSpreadOption(StandardTarotSpreads.ThreeCards, new LocalizationKey("ui.tarot.spread.three-cards"))
        ],
        TarotPrototypeSelections.BackVariants,
        TarotPrototypeSelections.ArtworkPackId,
        TarotPrototypeSelections.PresentationSkinId,
        TarotPrototypeSelections.InterpretationSetId);

    /// <summary>Selects a spread and clears a reading that belongs to another spread.</summary>
    public void SelectSpread(TarotSpreadId spreadId)
    {
        ArgumentNullException.ThrowIfNull(spreadId);
        var option = SpreadOptions.FirstOrDefault(candidate => candidate.Definition.Id == spreadId)
            ?? throw new ArgumentException("The spread is not available in this workspace.", nameof(spreadId));
        if (SelectedSpread == option)
        {
            return;
        }

        SelectedSpread = option;
        if (CurrentReading?.SpreadId != spreadId)
        {
            ClearReading();
        }

        OnStateChanged();
    }

    /// <summary>Updates whether reversed cards may be drawn.</summary>
    public void SetAllowReversed(bool allowReversed)
    {
        if (AllowReversed == allowReversed)
        {
            return;
        }

        AllowReversed = allowReversed;
        OnStateChanged();
    }

    /// <summary>Selects one of the available programmatic back variants.</summary>
    public void SelectBackVariant(TarotBackVariantId backVariantId)
    {
        ArgumentNullException.ThrowIfNull(backVariantId);
        var option = BackVariants.FirstOrDefault(candidate => candidate.Id == backVariantId)
            ?? throw new ArgumentException("The back variant is not available in this workspace.", nameof(backVariantId));
        if (SelectedBackVariant == option)
        {
            return;
        }

        SelectedBackVariant = option;
        OnStateChanged();
    }

    /// <summary>Draws or redraws the selected spread through the injected domain engine.</summary>
    public void Draw(Instant drawnAt)
    {
        var result = drawEngine.Draw(
            deck,
            SelectedSpread.Definition,
            AllowReversed
                ? TarotOrientationPolicy.UprightAndReversed
                : TarotOrientationPolicy.UprightOnly,
            drawnAt);

        CurrentReading = result.Reading;
        CurrentFailure = result.Failure;
        SelectedCard = null;
        revealedPositions.Clear();
        OnStateChanged();
    }

    /// <summary>Reveals and selects the exact assignment at a spread position.</summary>
    public void RevealAndSelect(TarotSpreadPositionId positionId)
    {
        ArgumentNullException.ThrowIfNull(positionId);
        var assignment = CurrentReading?.Cards.FirstOrDefault(card => card.PositionId == positionId)
            ?? throw new InvalidOperationException("The requested position is not part of the current reading.");

        revealedPositions.Add(positionId);
        SelectedCard = assignment;
        OnStateChanged();
    }

    /// <summary>Gets whether a drawn position has been revealed.</summary>
    public bool IsRevealed(TarotSpreadPositionId positionId)
    {
        ArgumentNullException.ThrowIfNull(positionId);
        return revealedPositions.Contains(positionId);
    }

    private void ClearReading()
    {
        CurrentReading = null;
        CurrentFailure = null;
        SelectedCard = null;
        revealedPositions.Clear();
    }

    private void OnStateChanged() => StateChanged?.Invoke(this, EventArgs.Empty);
}
