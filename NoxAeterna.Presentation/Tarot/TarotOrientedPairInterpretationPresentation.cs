using NoxAeterna.Domain.Tarot;
using NoxAeterna.Interpretation.Tarot.Contracts;
using NoxAeterna.Interpretation.Tarot.Resolution;

namespace NoxAeterna.Presentation.Tarot;

/// <summary>Immutable display-ready combined oriented-pair interpretation.</summary>
public sealed class TarotOrientedPairInterpretationPresentation
{
    internal TarotOrientedPairInterpretationPresentation(
        TarotInterpretationPackId packId,
        int contentVersion,
        TarotInterpretationLocale requestedLocale,
        TarotInterpretationLocale resolvedLocale,
        string interaction,
        string direction,
        IEnumerable<TarotInterpretationTagPresentation> tags,
        int overallValence,
        int overallIntensity)
    {
        PackId = packId;
        ContentVersion = contentVersion;
        RequestedLocale = requestedLocale;
        ResolvedLocale = resolvedLocale;
        Interaction = interaction;
        Direction = direction;
        Tags = Array.AsReadOnly(tags.ToArray());
        OverallValence = overallValence;
        OverallIntensity = overallIntensity;
    }

    public TarotInterpretationPackId PackId { get; }
    public int ContentVersion { get; }
    public TarotInterpretationLocale RequestedLocale { get; }
    public TarotInterpretationLocale ResolvedLocale { get; }
    public string Interaction { get; }
    public string Direction { get; }
    public IReadOnlyList<TarotInterpretationTagPresentation> Tags { get; }
    public int OverallValence { get; }
    public int OverallIntensity { get; }
}

/// <summary>Builds one combined pair presentation from resolved content and pack-local vocabulary.</summary>
public sealed class TarotOrientedPairInterpretationPresentationBuilder
{
    public TarotOrientedPairInterpretationPresentation? Build(
        TarotReading reading,
        ResolvedTarotInterpretation<TarotOrientedPairEntry> resolved,
        TarotInterpretationPresentationLabels labels)
    {
        ArgumentNullException.ThrowIfNull(reading);
        ArgumentNullException.ThrowIfNull(resolved);
        ArgumentNullException.ThrowIfNull(labels);
        if (reading.SpreadId != StandardTarotSpreads.TwoCards.Id ||
            reading.Cards.Count != 2 ||
            resolved.ModeId != TarotInterpretationMode.TwoCards)
        {
            throw new ArgumentException("Pair presentation requires a two-card reading and resolution.");
        }

        var assignments = reading.Cards.ToDictionary(static card => card.Card.Id);
        if (!assignments.TryGetValue(resolved.Content.CardAId, out var cardA) ||
            !assignments.TryGetValue(resolved.Content.CardBId, out var cardB) ||
            !Matches(resolved.Content.OrientationState, cardA.Orientation, cardB.Orientation))
        {
            throw new ArgumentException("The resolved pair must match the exact drawn assignments.", nameof(resolved));
        }

        var tags = resolved.Content.Tags
            .Where(tag => labels.TagLabels.TryGetValue(tag.ConceptId, out var label) &&
                          !string.IsNullOrWhiteSpace(label))
            .Select(tag => new TarotInterpretationTagPresentation(
                tag.ConceptId,
                labels.TagLabels[tag.ConceptId],
                tag.Valence,
                tag.Intensity))
            .ToArray();

        return new TarotOrientedPairInterpretationPresentation(
            resolved.PackId,
            resolved.ContentVersion,
            resolved.RequestedLocale,
            resolved.ResolvedLocale,
            resolved.Content.Interaction,
            resolved.Content.Direction,
            tags,
            resolved.Content.OverallValence,
            resolved.Content.OverallIntensity);
    }

    private static bool Matches(
        TarotOrientedPairState state,
        TarotCardOrientation cardA,
        TarotCardOrientation cardB) => (state, cardA, cardB) switch
    {
        (TarotOrientedPairState.UprightUpright, TarotCardOrientation.Upright, TarotCardOrientation.Upright) => true,
        (TarotOrientedPairState.UprightReversed, TarotCardOrientation.Upright, TarotCardOrientation.Reversed) => true,
        (TarotOrientedPairState.ReversedUpright, TarotCardOrientation.Reversed, TarotCardOrientation.Upright) => true,
        (TarotOrientedPairState.ReversedReversed, TarotCardOrientation.Reversed, TarotCardOrientation.Reversed) => true,
        _ => false
    };
}
