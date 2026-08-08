using NodaTime;
using NoxAeterna.Domain.Tarot;
using NoxAeterna.Interpretation.Tarot.Contracts;
using NoxAeterna.Interpretation.Tarot.Resolution;
using NoxAeterna.Presentation.Tarot;

namespace NoxAeterna.Tests.Presentation;

public sealed class TarotOrientedPairInterpretationPresentationBuilderTests
{
    [Fact]
    public void Build_PreservesCombinedProseProvenanceMetricsAndEveryAuthoredTag()
    {
        var context = Context(swappedTableauOrder: true);

        var presentation = Assert.IsType<TarotOrientedPairInterpretationPresentation>(
            new TarotOrientedPairInterpretationPresentationBuilder().Build(
                context.Reading,
                context.Resolved,
                Labels()));

        Assert.Equal("classic", presentation.PackId.Value);
        Assert.Equal(11, presentation.ContentVersion);
        Assert.Equal("en", presentation.RequestedLocale.Value);
        Assert.Equal("ru", presentation.ResolvedLocale.Value);
        Assert.Equal("Combined interaction", presentation.Interaction);
        Assert.Equal("Combined direction", presentation.Direction);
        Assert.Equal(1, presentation.OverallValence);
        Assert.Equal(3, presentation.OverallIntensity);
        Assert.Equal(
            new[] { "change", "clarity", "release" },
            presentation.Tags.Select(static tag => tag.ConceptId.Value));
        Assert.Equal(new[] { "Перемены", "Ясность", "Освобождение" }, presentation.Tags.Select(static tag => tag.Label));
        Assert.Equal(new[] { -1, 1, 2 }, presentation.Tags.Select(static tag => tag.Valence));
        Assert.Equal(new[] { 1, 2, 3 }, presentation.Tags.Select(static tag => tag.Intensity));
        Assert.Null(typeof(TarotOrientedPairInterpretationPresentation).GetProperty("Sections"));
    }

    [Fact]
    public void Build_UsesOnlyPackVocabularyLabelsAndNeverRawConceptIds()
    {
        var context = Context(swappedTableauOrder: false);
        var labels = new TarotInterpretationPresentationLabels(
            new Dictionary<string, string>(),
            new Dictionary<TarotTagConceptId, string>
            {
                [new("change")] = "Перемены",
                [new("release")] = "Освобождение"
            });

        var presentation = Assert.IsType<TarotOrientedPairInterpretationPresentation>(
            new TarotOrientedPairInterpretationPresentationBuilder().Build(
                context.Reading,
                context.Resolved,
                labels));

        Assert.Equal(new[] { "Перемены", "Освобождение" }, presentation.Tags.Select(static tag => tag.Label));
        Assert.DoesNotContain(presentation.Tags, static tag => tag.Label == tag.ConceptId.Value);
        Assert.DoesNotContain("change", presentation.Tags.Select(static tag => tag.Label));
        Assert.DoesNotContain("release", presentation.Tags.Select(static tag => tag.Label));
    }

    [Fact]
    public void Build_AcceptsEitherTableauOrderWhenCanonicalOrientationsStayAttachedToCards()
    {
        var builder = new TarotOrientedPairInterpretationPresentationBuilder();
        var canonical = Context(swappedTableauOrder: false);
        var swapped = Context(swappedTableauOrder: true);

        var canonicalPresentation = Assert.IsType<TarotOrientedPairInterpretationPresentation>(
            builder.Build(canonical.Reading, canonical.Resolved, Labels()));
        var swappedPresentation = Assert.IsType<TarotOrientedPairInterpretationPresentation>(
            builder.Build(swapped.Reading, swapped.Resolved, Labels()));

        Assert.Equal(canonicalPresentation.Interaction, swappedPresentation.Interaction);
        Assert.Equal(canonicalPresentation.Direction, swappedPresentation.Direction);
        Assert.Equal(
            canonicalPresentation.Tags.Select(static tag => (tag.ConceptId, tag.Valence, tag.Intensity)),
            swappedPresentation.Tags.Select(static tag => (tag.ConceptId, tag.Valence, tag.Intensity)));
    }

    private static TestContext Context(bool swappedTableauOrder)
    {
        var cardA = StandardTarotCatalog.Deck.Cards.Single(static card => card.Id.Value == "major.tower");
        var cardB = StandardTarotCatalog.Deck.Cards.Single(static card => card.Id.Value == "major.world");
        var a = new TarotDrawnCard(
            StandardTarotSpreads.TwoCards.Positions[0].Id,
            cardA,
            TarotCardOrientation.Upright);
        var b = new TarotDrawnCard(
            StandardTarotSpreads.TwoCards.Positions[1].Id,
            cardB,
            TarotCardOrientation.Reversed);
        var assignments = swappedTableauOrder
            ? new[]
            {
                new TarotDrawnCard(a.PositionId, cardB, TarotCardOrientation.Reversed),
                new TarotDrawnCard(b.PositionId, cardA, TarotCardOrientation.Upright)
            }
            : [a, b];
        var reading = new TarotReading(
            StandardTarotCatalog.Deck.Id,
            StandardTarotSpreads.TwoCards.Id,
            Instant.FromUnixTimeTicks(19),
            assignments);
        var content = new TarotOrientedPairEntry(
            cardA.Id,
            cardB.Id,
            TarotOrientedPairState.UprightReversed,
            "Combined interaction",
            "Combined direction",
            [
                new(new("change"), -1, 1),
                new(new("clarity"), 1, 2),
                new(new("release"), 2, 3)
            ],
            1,
            3);
        var resolved = new ResolvedTarotInterpretation<TarotOrientedPairEntry>(
            new("classic"),
            11,
            TarotInterpretationMode.TwoCards,
            new("en"),
            new("ru"),
            content);
        return new(reading, resolved);
    }

    private static TarotInterpretationPresentationLabels Labels() => new(
        new Dictionary<string, string>(),
        new Dictionary<TarotTagConceptId, string>
        {
            [new("change")] = "Перемены",
            [new("clarity")] = "Ясность",
            [new("release")] = "Освобождение"
        });

    private sealed record TestContext(
        TarotReading Reading,
        ResolvedTarotInterpretation<TarotOrientedPairEntry> Resolved);
}
