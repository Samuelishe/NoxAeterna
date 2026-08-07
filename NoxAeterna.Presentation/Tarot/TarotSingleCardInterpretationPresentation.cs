using System.Collections.ObjectModel;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using NoxAeterna.Domain.Tarot;
using NoxAeterna.Interpretation.Tarot.Contracts;
using NoxAeterna.Interpretation.Tarot.Resolution;

namespace NoxAeterna.Presentation.Tarot;

/// <summary>Provides already resolved pack-local labels for one single-card locale.</summary>
public sealed class TarotSingleCardInterpretationLabels
{
    public TarotSingleCardInterpretationLabels(
        IReadOnlyDictionary<string, string> sectionLabels,
        IReadOnlyDictionary<TarotTagConceptId, string> tagLabels)
    {
        ArgumentNullException.ThrowIfNull(sectionLabels);
        ArgumentNullException.ThrowIfNull(tagLabels);
        SectionLabels = new ReadOnlyDictionary<string, string>(
            sectionLabels.ToDictionary(static pair => pair.Key, static pair => pair.Value, StringComparer.Ordinal));
        TagLabels = new ReadOnlyDictionary<TarotTagConceptId, string>(
            tagLabels.ToDictionary(static pair => pair.Key, static pair => pair.Value));
    }

    public IReadOnlyDictionary<string, string> SectionLabels { get; }
    public IReadOnlyDictionary<TarotTagConceptId, string> TagLabels { get; }
}

/// <summary>Represents one visible pack-local single-card section.</summary>
public sealed record TarotSingleCardInterpretationSection(string SectionId, string Label, string Text);

/// <summary>Represents one visible labeled semantic tag without UI color data.</summary>
public sealed record TarotSingleCardInterpretationTag(
    TarotTagConceptId ConceptId,
    string Label,
    int Valence,
    int Intensity);

/// <summary>Immutable display-ready single-card interpretation with resolver provenance.</summary>
public sealed class TarotSingleCardInterpretationPresentation
{
    internal TarotSingleCardInterpretationPresentation(
        TarotInterpretationPackId packId,
        int contentVersion,
        TarotInterpretationLocale requestedLocale,
        TarotInterpretationLocale resolvedLocale,
        IEnumerable<TarotSingleCardInterpretationSection> sections,
        IEnumerable<TarotSingleCardInterpretationTag> tags,
        int overallValence,
        int overallIntensity)
    {
        PackId = packId;
        ContentVersion = contentVersion;
        RequestedLocale = requestedLocale;
        ResolvedLocale = resolvedLocale;
        Sections = Array.AsReadOnly(sections.ToArray());
        Tags = Array.AsReadOnly(tags.ToArray());
        OverallValence = overallValence;
        OverallIntensity = overallIntensity;
    }

    public TarotInterpretationPackId PackId { get; }
    public int ContentVersion { get; }
    public TarotInterpretationLocale RequestedLocale { get; }
    public TarotInterpretationLocale ResolvedLocale { get; }
    public IReadOnlyList<TarotSingleCardInterpretationSection> Sections { get; }
    public IReadOnlyList<TarotSingleCardInterpretationTag> Tags { get; }
    public int OverallValence { get; }
    public int OverallIntensity { get; }
}

/// <summary>Builds deterministic single-card display state from validated meaning and pack-local labels.</summary>
public sealed class TarotSingleCardInterpretationPresentationBuilder
{
    private static readonly string[] SectionOrder = ["situation", "development", "risk", "outcome", "advice"];

    public TarotSingleCardInterpretationPresentation? Build(
        TarotReading reading,
        ResolvedTarotInterpretation<TarotSingleCardEntry> resolved,
        TarotSingleCardInterpretationLabels labels)
    {
        ArgumentNullException.ThrowIfNull(reading);
        ArgumentNullException.ThrowIfNull(resolved);
        ArgumentNullException.ThrowIfNull(labels);
        if (reading.SpreadId != StandardTarotSpreads.SingleCard.Id ||
            resolved.ModeId != TarotInterpretationMode.SingleCard)
        {
            throw new ArgumentException("Single-card presentation requires a single-card reading and resolution.");
        }

        var assignment = reading.Cards.Single();
        if (assignment.Card.Id != resolved.Content.CardId || assignment.Orientation != resolved.Content.Orientation)
        {
            throw new ArgumentException("The resolved entry must match the exact drawn assignment.", nameof(resolved));
        }

        var sections = new List<TarotSingleCardInterpretationSection>(SectionOrder.Length);
        foreach (var sectionId in SectionOrder)
        {
            if (!labels.SectionLabels.TryGetValue(sectionId, out var label) || string.IsNullOrWhiteSpace(label))
            {
                return null;
            }

            sections.Add(new(sectionId, label, resolved.Content.Sections[sectionId]));
        }

        var seed = StableSeed(reading, assignment, resolved);
        var tags = resolved.Content.Tags
            .GroupBy(static tag => tag.ConceptId)
            .Select(static group => group.First())
            .Where(tag => labels.TagLabels.TryGetValue(tag.ConceptId, out var label) && !string.IsNullOrWhiteSpace(label))
            .Select(tag => new RankedTag(
                Rank(seed, tag.ConceptId),
                new TarotSingleCardInterpretationTag(
                    tag.ConceptId,
                    labels.TagLabels[tag.ConceptId],
                    tag.Valence,
                    tag.Intensity)))
            .OrderBy(static item => item.Hash, StringComparer.Ordinal)
            .ThenBy(static item => item.Tag.ConceptId.Value, StringComparer.Ordinal)
            .Take(3)
            .Select(static item => item.Tag)
            .ToArray();

        return new TarotSingleCardInterpretationPresentation(
            resolved.PackId,
            resolved.ContentVersion,
            resolved.RequestedLocale,
            resolved.ResolvedLocale,
            sections,
            tags,
            resolved.Content.OverallValence,
            resolved.Content.OverallIntensity);
    }

    private static string StableSeed(
        TarotReading reading,
        TarotDrawnCard assignment,
        ResolvedTarotInterpretation<TarotSingleCardEntry> resolved) => string.Join(
        '\n',
        resolved.PackId.Value,
        resolved.ContentVersion.ToString(CultureInfo.InvariantCulture),
        reading.SpreadId.Value,
        reading.DrawnAt.ToUnixTimeTicks().ToString(CultureInfo.InvariantCulture),
        assignment.PositionId.Value,
        assignment.Card.Id.Value,
        assignment.Orientation.ToString());

    private static string Rank(string seed, TarotTagConceptId conceptId) => Convert.ToHexString(
        SHA256.HashData(Encoding.UTF8.GetBytes($"{seed}\n{conceptId.Value}")));

    private sealed record RankedTag(string Hash, TarotSingleCardInterpretationTag Tag);
}
