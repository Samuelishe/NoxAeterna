using System.Globalization;
using NodaTime;
using NoxAeterna.Domain.Tarot;
using NoxAeterna.Interpretation.Tarot.Contracts;
using NoxAeterna.Interpretation.Tarot.Resolution;
using NoxAeterna.Interpretation.Tarot.Validation;
using NoxAeterna.Presentation.Tarot;

namespace NoxAeterna.Tests.Presentation;

public sealed class TarotSingleCardInterpretationPresentationBuilderTests
{
    [Fact]
    public void Build_PreservesProvenanceMetricsAndExactFiveSectionOrder()
    {
        var context = Context(TarotCardOrientation.Upright, ticks: 17, contentVersion: 9);

        var result = new TarotSingleCardInterpretationPresentationBuilder().Build(
            context.Reading,
            context.Resolved,
            Labels("RU"));

        var presentation = Assert.IsType<TarotSingleCardInterpretationPresentation>(result);
        Assert.Equal("classic", presentation.PackId.Value);
        Assert.Equal(9, presentation.ContentVersion);
        Assert.Equal("zh", presentation.RequestedLocale.Value);
        Assert.Equal("ru", presentation.ResolvedLocale.Value);
        Assert.Equal(
            new[] { "situation", "development", "risk", "outcome", "advice" },
            presentation.Sections.Select(static section => section.SectionId));
        Assert.Equal(
            new[] { "RU situation", "RU development", "RU risk", "RU outcome", "RU advice" },
            presentation.Sections.Select(static section => section.Label));
        Assert.Equal(
            context.Resolved.Content.Sections.Values,
            presentation.Sections.Select(static section => section.Text));
        Assert.Equal(3, presentation.Tags.Count);
        Assert.Equal(context.Resolved.Content.OverallValence, presentation.OverallValence);
        Assert.Equal(context.Resolved.Content.OverallIntensity, presentation.OverallIntensity);
    }

    [Fact]
    public void Build_ShowsOnlyAvailableLabeledTagsAndNeverRawConceptIds()
    {
        var context = Context(TarotCardOrientation.Upright);
        var twoLabels = Labels("Visible", tagCount: 2);

        var two = Assert.IsType<TarotSingleCardInterpretationPresentation>(
            new TarotSingleCardInterpretationPresentationBuilder().Build(context.Reading, context.Resolved, twoLabels));
        var none = Assert.IsType<TarotSingleCardInterpretationPresentation>(
            new TarotSingleCardInterpretationPresentationBuilder().Build(
                context.Reading,
                context.Resolved,
                Labels("Visible", tagCount: 0)));

        Assert.Equal(2, two.Tags.Count);
        Assert.All(two.Tags, tag => Assert.StartsWith("Visible tag ", tag.Label, StringComparison.Ordinal));
        Assert.DoesNotContain(two.Tags, tag => tag.Label == tag.ConceptId.Value);
        Assert.Empty(none.Tags);
    }

    [Fact]
    public void Build_UsesStableConceptSubsetAcrossLocaleAndCultureChanges()
    {
        var context = Context(TarotCardOrientation.Upright, ticks: 211);
        var builder = new TarotSingleCardInterpretationPresentationBuilder();
        var originalCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("tr-TR");
            var russian = Assert.IsType<TarotSingleCardInterpretationPresentation>(
                builder.Build(context.Reading, context.Resolved, Labels("RU")));
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
            var english = Assert.IsType<TarotSingleCardInterpretationPresentation>(
                builder.Build(context.Reading, context.Resolved, Labels("EN")));

            Assert.Equal(
                russian.Tags.Select(static tag => tag.ConceptId),
                english.Tags.Select(static tag => tag.ConceptId));
            Assert.NotEqual(
                russian.Tags.Select(static tag => tag.Label),
                english.Tags.Select(static tag => tag.Label));
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public void Build_RankingIncludesReadingPackAndContentVersionIdentity()
    {
        var builder = new TarotSingleCardInterpretationPresentationBuilder();
        var baseline = Context(TarotCardOrientation.Upright, ticks: 1, contentVersion: 1);
        var laterReading = Context(TarotCardOrientation.Upright, ticks: 2, contentVersion: 1);
        var laterContent = Context(TarotCardOrientation.Upright, ticks: 1, contentVersion: 2);
        var otherPack = Context(TarotCardOrientation.Upright, ticks: 1, contentVersion: 1, packId: "second-pack");

        var selections = new[] { baseline, laterReading, laterContent, otherPack }
            .Select(context => Assert.IsType<TarotSingleCardInterpretationPresentation>(
                builder.Build(context.Reading, context.Resolved, Labels("Stable"))))
            .Select(presentation => string.Join('|', presentation.Tags.Select(static tag => tag.ConceptId.Value)))
            .ToArray();

        Assert.Equal(4, selections.Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void Build_ReversedEntryRetainsOrientationSpecificProseAndCandidatePool()
    {
        var upright = Context(TarotCardOrientation.Upright, conceptPrefix: "upright");
        var reversed = Context(TarotCardOrientation.Reversed, conceptPrefix: "reversed");
        var builder = new TarotSingleCardInterpretationPresentationBuilder();

        var uprightPresentation = Assert.IsType<TarotSingleCardInterpretationPresentation>(
            builder.Build(upright.Reading, upright.Resolved, Labels("Upright", conceptPrefix: "upright")));
        var reversedPresentation = Assert.IsType<TarotSingleCardInterpretationPresentation>(
            builder.Build(reversed.Reading, reversed.Resolved, Labels("Reversed", conceptPrefix: "reversed")));

        Assert.All(reversedPresentation.Sections, section => Assert.Contains("Reversed", section.Text, StringComparison.Ordinal));
        Assert.All(reversedPresentation.Tags, tag => Assert.StartsWith("reversed-", tag.ConceptId.Value, StringComparison.Ordinal));
        Assert.DoesNotContain(
            reversedPresentation.Tags.Select(static tag => tag.ConceptId),
            uprightPresentation.Tags.Select(static tag => tag.ConceptId).Contains);
    }

    [Fact]
    public void Build_MissingPackLocalSectionLabelProducesNoPresentation()
    {
        var context = Context(TarotCardOrientation.Upright);
        var sections = SectionLabels("RU").Where(static pair => pair.Key != "risk").ToDictionary();
        var labels = new TarotInterpretationPresentationLabels(sections, TagLabels("RU", "concept", 6));

        var result = new TarotSingleCardInterpretationPresentationBuilder().Build(
            context.Reading,
            context.Resolved,
            labels);

        Assert.Null(result);
    }

    [Fact]
    public void DuplicateConceptsCannotReachPresentationBecauseValidatedInputRejectsThem()
    {
        var document = Bundle("duplicate");
        document.States!["upright"]!.Tags![1]!.ConceptId = document.States["upright"]!.Tags![0]!.ConceptId;

        var validation = TarotInterpretationValidator.ValidateSingleCardBundle(document, StandardTarotCatalog.Deck);

        Assert.False(validation.IsValid);
        Assert.Contains(validation.Diagnostics, item => item.Code == "tag.duplicate");
    }

    private static TestContext Context(
        TarotCardOrientation orientation,
        long ticks = 1,
        int contentVersion = 1,
        string packId = "classic",
        string conceptPrefix = "concept")
    {
        var card = StandardTarotCatalog.Deck.Cards.Single(static card => card.Id.Value == "major.fool");
        var assignment = new TarotDrawnCard(StandardTarotSpreads.SingleCard.Positions.Single().Id, card, orientation);
        var reading = new TarotReading(
            StandardTarotCatalog.Deck.Id,
            StandardTarotSpreads.SingleCard.Id,
            Instant.FromUnixTimeTicks(ticks),
            [assignment]);
        var content = Entry(orientation, conceptPrefix);
        var resolved = new ResolvedTarotInterpretation<TarotSingleCardEntry>(
            new TarotInterpretationPackId(packId),
            contentVersion,
            TarotInterpretationMode.SingleCard,
            new TarotInterpretationLocale("zh"),
            new TarotInterpretationLocale("ru"),
            content);
        return new(reading, resolved);
    }

    private static TarotSingleCardEntry Entry(TarotCardOrientation orientation,string conceptPrefix)=>new(
        new("major.fool"),
        orientation,
        new Dictionary<string,string>(StringComparer.Ordinal)
        {
            ["situation"]=$"{orientation} situation",["development"]=$"{orientation} development",["risk"]=$"{orientation} risk",["outcome"]=$"{orientation} outcome",["advice"]=$"{orientation} advice"
        },
        Enumerable.Range(1,10).Select(index=>new TarotTagAssignment(new($"{conceptPrefix}-{index}"),(index%5)-2,((index-1)%3)+1)),
        orientation==TarotCardOrientation.Upright?1:-1,
        3,
        orientation==TarotCardOrientation.Upright?[]:[TarotReversalMechanism.Blocked]);

    private static TarotSingleCardBundleDocument Bundle(string conceptPrefix) => new()
    {
        SchemaVersion = 1,
        CardId = "major.fool",
        States = new(StringComparer.Ordinal)
        {
            ["upright"] = State(false,conceptPrefix), ["reversed"] = State(true,conceptPrefix)
        }
    };

    private static TarotSingleCardStateDocument State(bool reversed,string prefix)=>new()
    {
        Sections=new(StringComparer.Ordinal){{"situation","Situation"},{"development","Development"},{"risk","Risk"},{"outcome","Outcome"},{"advice","Advice"}},
        Tags=Enumerable.Range(1,10).Select(index=>(TarotTagAssignmentDocument?)new TarotTagAssignmentDocument{ConceptId=$"{prefix}-{index}",Valence=(index%5)-2,Intensity=((index-1)%3)+1}).ToList(),
        OverallValence=reversed?-1:1,OverallIntensity=3,ReversalMechanisms=reversed?[TarotReversalMechanism.Blocked]:[]
    };

    private static TarotInterpretationPresentationLabels Labels(
        string prefix,
        int tagCount = 10,
        string conceptPrefix = "concept") => new(
        SectionLabels(prefix),
        TagLabels(prefix, conceptPrefix, tagCount));

    private static Dictionary<string, string> SectionLabels(string prefix) => new(StringComparer.Ordinal)
    {
        ["situation"] = $"{prefix} situation",
        ["development"] = $"{prefix} development",
        ["risk"] = $"{prefix} risk",
        ["outcome"] = $"{prefix} outcome",
        ["advice"] = $"{prefix} advice"
    };

    private static Dictionary<TarotTagConceptId, string> TagLabels(
        string prefix,
        string conceptPrefix,
        int count) => Enumerable.Range(1, count).ToDictionary(
        index => new TarotTagConceptId($"{conceptPrefix}-{index}"),
        index => $"{prefix} tag {index}");

    private sealed record TestContext(
        TarotReading Reading,
        ResolvedTarotInterpretation<TarotSingleCardEntry> Resolved);
}
