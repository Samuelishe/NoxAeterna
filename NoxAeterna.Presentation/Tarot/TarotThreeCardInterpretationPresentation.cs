using NoxAeterna.Domain.Tarot;
using NoxAeterna.Interpretation.Tarot.Contracts;
using NoxAeterna.Interpretation.Tarot.Keys;
using NoxAeterna.Interpretation.Tarot.Resolution;

namespace NoxAeterna.Presentation.Tarot;

/// <summary>The two semantic adjacent transitions available to a Three Cards reading.</summary>
public enum TarotThreeCardRelationId
{
    PastPresent = 0,
    PresentFuture = 1
}

/// <summary>Base contract for one visible block in canonical Three Cards trajectory order.</summary>
public abstract class TarotThreeCardInterpretationBlock
{
    private protected TarotThreeCardInterpretationBlock(string blockId) => BlockId = blockId;

    public string BlockId { get; }
}

/// <summary>One exact revealed position meaning with its pack-local label and authored metadata.</summary>
public sealed class TarotThreeCardPositionPresentation : TarotThreeCardInterpretationBlock
{
    internal TarotThreeCardPositionPresentation(
        string positionId,
        TarotThreeCardPosition position,
        string label,
        string text,
        IEnumerable<TarotInterpretationTagPresentation> tags,
        int overallValence,
        int overallIntensity)
        : base(positionId)
    {
        PositionId = positionId;
        Position = position;
        Label = label;
        Text = text;
        Tags = Array.AsReadOnly(tags.ToArray());
        OverallValence = overallValence;
        OverallIntensity = overallIntensity;
    }

    public string PositionId { get; }
    public TarotThreeCardPosition Position { get; }
    public string Label { get; }
    public string Text { get; }
    public IReadOnlyList<TarotInterpretationTagPresentation> Tags { get; }
    public int OverallValence { get; }
    public int OverallIntensity { get; }
}

/// <summary>One exact adjacent oriented-pair meaning under its trajectory relation identity.</summary>
public sealed class TarotThreeCardRelationPresentation : TarotThreeCardInterpretationBlock
{
    internal TarotThreeCardRelationPresentation(
        string relationId,
        TarotThreeCardRelationId relation,
        string label,
        string interaction,
        string direction,
        IEnumerable<TarotInterpretationTagPresentation> tags,
        int overallValence,
        int overallIntensity)
        : base(relationId)
    {
        RelationId = relationId;
        Relation = relation;
        Label = label;
        Interaction = interaction;
        Direction = direction;
        Tags = Array.AsReadOnly(tags.ToArray());
        OverallValence = overallValence;
        OverallIntensity = overallIntensity;
    }

    public string RelationId { get; }
    public TarotThreeCardRelationId Relation { get; }
    public string Label { get; }
    public string Interaction { get; }
    public string Direction { get; }
    public IReadOnlyList<TarotInterpretationTagPresentation> Tags { get; }
    public int OverallValence { get; }
    public int OverallIntensity { get; }
}

/// <summary>The exact two curated resource fragments selected for the complete reading.</summary>
public sealed class TarotThreeCardOverallPresentation : TarotThreeCardInterpretationBlock
{
    internal TarotThreeCardOverallPresentation(
        string relationId,
        string label,
        TarotSynthesisResourceId trajectoryProfileId,
        string trajectoryText,
        TarotSynthesisResourceId synthesisFragmentId,
        string transitionText)
        : base(relationId)
    {
        RelationId = relationId;
        Label = label;
        TrajectoryProfileId = trajectoryProfileId;
        TrajectoryText = trajectoryText;
        SynthesisFragmentId = synthesisFragmentId;
        TransitionText = transitionText;
    }

    public string RelationId { get; }
    public string Label { get; }
    public TarotSynthesisResourceId TrajectoryProfileId { get; }
    public string TrajectoryText { get; }
    public TarotSynthesisResourceId SynthesisFragmentId { get; }
    public string TransitionText { get; }
}

/// <summary>Resolver-owned synthesis results selected only for a complete Three Cards reading.</summary>
public sealed record TarotThreeCardResolvedSynthesis(
    TarotThreeCardSynthesisPlan Plan,
    ResolvedTarotInterpretation<TarotSynthesisResource> TrajectoryProfile,
    ResolvedTarotInterpretation<TarotSynthesisResource> SynthesisFragment);

/// <summary>Immutable display-ready Three Cards meaning in canonical progressive order.</summary>
public sealed class TarotThreeCardInterpretationPresentation
{
    internal TarotThreeCardInterpretationPresentation(
        TarotInterpretationPackId packId,
        int contentVersion,
        TarotInterpretationLocale requestedLocale,
        TarotInterpretationLocale resolvedLocale,
        IEnumerable<TarotThreeCardPositionPresentation> positions,
        IEnumerable<TarotThreeCardRelationPresentation> relations,
        TarotThreeCardOverallPresentation? overall,
        IEnumerable<TarotThreeCardInterpretationBlock> blocks)
    {
        PackId = packId;
        ContentVersion = contentVersion;
        RequestedLocale = requestedLocale;
        ResolvedLocale = resolvedLocale;
        Positions = Array.AsReadOnly(positions.ToArray());
        Relations = Array.AsReadOnly(relations.ToArray());
        Overall = overall;
        Blocks = Array.AsReadOnly(blocks.ToArray());
    }

    public TarotInterpretationPackId PackId { get; }
    public int ContentVersion { get; }
    public TarotInterpretationLocale RequestedLocale { get; }
    public TarotInterpretationLocale ResolvedLocale { get; }
    public IReadOnlyList<TarotThreeCardPositionPresentation> Positions { get; }
    public IReadOnlyList<TarotThreeCardRelationPresentation> Relations { get; }
    public TarotThreeCardOverallPresentation? Overall { get; }
    public IReadOnlyList<TarotThreeCardInterpretationBlock> Blocks { get; }
}

/// <summary>Builds a validated progressive Three Cards presentation without resolving or composing meaning.</summary>
public sealed class TarotThreeCardInterpretationPresentationBuilder
{
    public TarotThreeCardInterpretationPresentation? Build(
        TarotReading reading,
        IEnumerable<ResolvedTarotInterpretation<TarotThreeCardPositionEntry>> positionResolutions,
        ResolvedTarotInterpretation<TarotOrientedPairEntry>? pastPresentRelation,
        ResolvedTarotInterpretation<TarotOrientedPairEntry>? presentFutureRelation,
        TarotThreeCardResolvedSynthesis? synthesis,
        TarotInterpretationPresentationLabels labels)
    {
        ArgumentNullException.ThrowIfNull(reading);
        ArgumentNullException.ThrowIfNull(positionResolutions);
        ArgumentNullException.ThrowIfNull(labels);
        if (reading.SpreadId != StandardTarotSpreads.ThreeCards.Id || reading.Cards.Count != 3)
        {
            throw new ArgumentException("Three Cards presentation requires an exact three-card reading.", nameof(reading));
        }

        var resolvedPositions = positionResolutions.ToArray();
        if (resolvedPositions.Length == 0)
        {
            return null;
        }

        var provenance = resolvedPositions.Cast<object>()
            .Concat(pastPresentRelation is null ? [] : [pastPresentRelation])
            .Concat(presentFutureRelation is null ? [] : [presentFutureRelation])
            .Concat(synthesis is null ? [] : [synthesis.TrajectoryProfile, synthesis.SynthesisFragment])
            .Select(Describe)
            .ToArray();
        if (provenance.Any(static item => item.Mode != TarotInterpretationMode.ThreeCards) ||
            provenance.Any(item => item != provenance[0]))
        {
            throw new ArgumentException("Every Three Cards input must share mode and resolver provenance.");
        }

        var assignments = reading.Cards.ToDictionary(
            card => ToPosition(card.PositionId),
            static card => card);
        if (assignments.Count != 3 ||
            resolvedPositions.Select(static item => item.Content.Position).Distinct().Count() != resolvedPositions.Length)
        {
            throw new ArgumentException("Three Cards positions must be unique and supported.", nameof(positionResolutions));
        }

        var positions = new Dictionary<TarotThreeCardPosition, TarotThreeCardPositionPresentation>();
        foreach (var resolved in resolvedPositions)
        {
            if (!assignments.TryGetValue(resolved.Content.Position, out var assignment) ||
                assignment.Card.Id != resolved.Content.CardId ||
                assignment.Orientation != resolved.Content.Orientation)
            {
                throw new ArgumentException("Each resolved position must match its exact drawn assignment.", nameof(positionResolutions));
            }

            var id = PositionId(resolved.Content.Position);
            if (!TryLabel(labels.ThreeCardPositionLabels, id, out var label) ||
                !TryTags(resolved.Content.Tags, labels, out var tags))
            {
                return null;
            }

            positions.Add(
                resolved.Content.Position,
                new TarotThreeCardPositionPresentation(
                    id,
                    resolved.Content.Position,
                    label,
                    resolved.Content.Text,
                    tags,
                    resolved.Content.OverallValence,
                    resolved.Content.OverallIntensity));
        }

        var expectsPastPresent = positions.ContainsKey(TarotThreeCardPosition.Past) &&
                                 positions.ContainsKey(TarotThreeCardPosition.Present);
        var expectsPresentFuture = positions.ContainsKey(TarotThreeCardPosition.Present) &&
                                  positions.ContainsKey(TarotThreeCardPosition.Future);
        if (expectsPastPresent != (pastPresentRelation is not null) ||
            expectsPresentFuture != (presentFutureRelation is not null))
        {
            return null;
        }

        var relations = new Dictionary<TarotThreeCardRelationId, TarotThreeCardRelationPresentation>();
        if (!TryCreateRelation(
                TarotThreeCardRelationId.PastPresent,
                pastPresentRelation,
                assignments,
                labels,
                out var pastPresentPresentation) ||
            !TryCreateRelation(
                TarotThreeCardRelationId.PresentFuture,
                presentFutureRelation,
                assignments,
                labels,
                out var presentFuturePresentation))
        {
            return null;
        }

        if (pastPresentPresentation is not null)
        {
            relations.Add(TarotThreeCardRelationId.PastPresent, pastPresentPresentation);
        }

        if (presentFuturePresentation is not null)
        {
            relations.Add(TarotThreeCardRelationId.PresentFuture, presentFuturePresentation);
        }

        TarotThreeCardOverallPresentation? overall = null;
        if (synthesis is not null)
        {
            if (positions.Count != 3 || relations.Count != 2 ||
                synthesis.TrajectoryProfile.Content.ResourceType != TarotSynthesisResourceType.TrajectoryProfile ||
                synthesis.TrajectoryProfile.Content.ResourceId != synthesis.Plan.TrajectoryProfileId ||
                synthesis.SynthesisFragment.Content.ResourceType != TarotSynthesisResourceType.SynthesisFragment ||
                synthesis.SynthesisFragment.Content.ResourceId != synthesis.Plan.SynthesisFragmentId ||
                !TryLabel(labels.RelationLabels, "overall", out var overallLabel))
            {
                throw new ArgumentException("Overall requires the exact complete Three Cards plan and resources.", nameof(synthesis));
            }

            overall = new TarotThreeCardOverallPresentation(
                "overall",
                overallLabel,
                synthesis.Plan.TrajectoryProfileId,
                synthesis.TrajectoryProfile.Content.Text,
                synthesis.Plan.SynthesisFragmentId,
                synthesis.SynthesisFragment.Content.Text);
        }
        else if (positions.Count == 3 && relations.Count == 2)
        {
            return null;
        }

        var blocks = new List<TarotThreeCardInterpretationBlock>(6);
        AddIfPresent(positions, TarotThreeCardPosition.Past, blocks);
        AddIfPresent(relations, TarotThreeCardRelationId.PastPresent, blocks);
        AddIfPresent(positions, TarotThreeCardPosition.Present, blocks);
        AddIfPresent(relations, TarotThreeCardRelationId.PresentFuture, blocks);
        AddIfPresent(positions, TarotThreeCardPosition.Future, blocks);
        if (overall is not null)
        {
            blocks.Add(overall);
        }

        var first = provenance[0];
        return new TarotThreeCardInterpretationPresentation(
            first.PackId,
            first.ContentVersion,
            first.RequestedLocale,
            first.ResolvedLocale,
            positions.Values.OrderBy(static item => item.Position).ToArray(),
            relations.Values.OrderBy(static item => item.Relation).ToArray(),
            overall,
            blocks);
    }

    private static bool TryCreateRelation(
        TarotThreeCardRelationId relation,
        ResolvedTarotInterpretation<TarotOrientedPairEntry>? resolved,
        IReadOnlyDictionary<TarotThreeCardPosition, TarotDrawnCard> assignments,
        TarotInterpretationPresentationLabels labels,
        out TarotThreeCardRelationPresentation? presentation)
    {
        presentation = null;
        if (resolved is null)
        {
            return true;
        }

        var (firstPosition, secondPosition, relationId) = relation switch
        {
            TarotThreeCardRelationId.PastPresent =>
                (TarotThreeCardPosition.Past, TarotThreeCardPosition.Present, "past-present"),
            TarotThreeCardRelationId.PresentFuture =>
                (TarotThreeCardPosition.Present, TarotThreeCardPosition.Future, "present-future"),
            _ => throw new ArgumentOutOfRangeException(nameof(relation), relation, "Unknown Three Cards relation.")
        };
        var first = assignments[firstPosition];
        var second = assignments[secondPosition];
        var canonical = TarotInterpretationKeys.CanonicalizePair(
            first.Card.Id,
            first.Orientation,
            second.Card.Id,
            second.Orientation);
        if (!canonical.IsValid || canonical.Value is null ||
            canonical.Value.CardAId != resolved.Content.CardAId ||
            canonical.Value.CardBId != resolved.Content.CardBId ||
            canonical.Value.OrientationState != resolved.Content.OrientationState)
        {
            throw new ArgumentException("Each adjacent relation must match its exact drawn endpoints.", nameof(resolved));
        }

        if (!TryLabel(labels.RelationLabels, relationId, out var label) ||
            !TryTags(resolved.Content.Tags, labels, out var tags))
        {
            return false;
        }

        presentation = new TarotThreeCardRelationPresentation(
            relationId,
            relation,
            label,
            resolved.Content.Interaction,
            resolved.Content.Direction,
            tags,
            resolved.Content.OverallValence,
            resolved.Content.OverallIntensity);
        return true;
    }

    private static bool TryTags(
        IEnumerable<TarotTagAssignment> authoredTags,
        TarotInterpretationPresentationLabels labels,
        out IReadOnlyList<TarotInterpretationTagPresentation> tags)
    {
        var result = new List<TarotInterpretationTagPresentation>();
        foreach (var tag in authoredTags)
        {
            if (!labels.TagLabels.TryGetValue(tag.ConceptId, out var label) ||
                string.IsNullOrWhiteSpace(label))
            {
                tags = [];
                return false;
            }

            result.Add(new(tag.ConceptId, label, tag.Valence, tag.Intensity));
        }

        tags = result;
        return true;
    }

    private static bool TryLabel(
        IReadOnlyDictionary<string, string> labels,
        string id,
        out string label)
    {
        if (labels.TryGetValue(id, out var value) && !string.IsNullOrWhiteSpace(value))
        {
            label = value;
            return true;
        }

        label = string.Empty;
        return false;
    }

    private static ResolutionProvenance Describe(object item) => item switch
    {
        ResolvedTarotInterpretation<TarotThreeCardPositionEntry> value =>
            new(value.PackId, value.ContentVersion, value.ModeId, value.RequestedLocale, value.ResolvedLocale),
        ResolvedTarotInterpretation<TarotOrientedPairEntry> value =>
            new(value.PackId, value.ContentVersion, value.ModeId, value.RequestedLocale, value.ResolvedLocale),
        ResolvedTarotInterpretation<TarotSynthesisResource> value =>
            new(value.PackId, value.ContentVersion, value.ModeId, value.RequestedLocale, value.ResolvedLocale),
        _ => throw new ArgumentException("Unsupported Three Cards presentation input.", nameof(item))
    };

    private static TarotThreeCardPosition ToPosition(TarotSpreadPositionId positionId) => positionId.Value switch
    {
        "past" => TarotThreeCardPosition.Past,
        "present" => TarotThreeCardPosition.Present,
        "future" => TarotThreeCardPosition.Future,
        _ => throw new ArgumentException("The reading contains an unsupported Three Cards position.", nameof(positionId))
    };

    private static string PositionId(TarotThreeCardPosition position) => position switch
    {
        TarotThreeCardPosition.Past => "past",
        TarotThreeCardPosition.Present => "present",
        TarotThreeCardPosition.Future => "future",
        _ => throw new ArgumentOutOfRangeException(nameof(position), position, "Unknown Three Cards position.")
    };

    private static void AddIfPresent<TKey, TValue>(
        IReadOnlyDictionary<TKey, TValue> values,
        TKey key,
        ICollection<TarotThreeCardInterpretationBlock> blocks)
        where TKey : notnull
        where TValue : TarotThreeCardInterpretationBlock
    {
        if (values.TryGetValue(key, out var value))
        {
            blocks.Add(value);
        }
    }

    private sealed record ResolutionProvenance(
        TarotInterpretationPackId PackId,
        int ContentVersion,
        TarotInterpretationMode Mode,
        TarotInterpretationLocale RequestedLocale,
        TarotInterpretationLocale ResolvedLocale);
}
