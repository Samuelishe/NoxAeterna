using NodaTime;
using NoxAeterna.Domain.Tarot;
using NoxAeterna.Interpretation.Tarot.Contracts;
using NoxAeterna.Interpretation.Tarot.Keys;
using NoxAeterna.Interpretation.Tarot.Resolution;
using NoxAeterna.Presentation.Tarot;

namespace NoxAeterna.Tests.Presentation;

public sealed class TarotThreeCardInterpretationPresentationBuilderTests
{
    [Fact]
    public void Build_CompleteReadingPreservesStructuredBlocksLabelsAuthoredTagsAndSeparateSynthesisTexts()
    {
        var context = Context();

        var presentation = Assert.IsType<TarotThreeCardInterpretationPresentation>(
            new TarotThreeCardInterpretationPresentationBuilder().Build(
                context.Reading,
                context.Positions,
                context.PastPresent,
                context.PresentFuture,
                context.Synthesis,
                Labels()));

        Assert.Equal("classic", presentation.PackId.Value);
        Assert.Equal(11, presentation.ContentVersion);
        Assert.Equal("en", presentation.RequestedLocale.Value);
        Assert.Equal("ru", presentation.ResolvedLocale.Value);
        Assert.Equal(
            ["past", "past-present", "present", "present-future", "future", "overall"],
            presentation.Blocks.Select(static block => block.BlockId));
        Assert.Equal(["Прошлое", "Настоящее", "Будущее"], presentation.Positions.Select(static item => item.Label));
        Assert.Equal(["Past text", "Present text", "Future text"], presentation.Positions.Select(static item => item.Text));
        Assert.Equal([1, 2, 3], presentation.Positions.SelectMany(static item => item.Tags).Select(static tag => tag.Intensity));
        Assert.Equal(
            ["Связь прошлого и настоящего", "Связь настоящего и будущего"],
            presentation.Relations.Select(static item => item.Label));
        Assert.Equal(["PP interaction", "PF interaction"], presentation.Relations.Select(static item => item.Interaction));
        Assert.Equal(["PP direction", "PF direction"], presentation.Relations.Select(static item => item.Direction));
        Assert.Equal([2, 3], presentation.Relations.SelectMany(static item => item.Tags).Select(static tag => tag.Intensity));
        var overall = Assert.IsType<TarotThreeCardOverallPresentation>(presentation.Overall);
        Assert.Equal("Общая картина", overall.Label);
        Assert.Equal(TarotThreeCardSynthesisContract.Improving, overall.TrajectoryProfileId.Value);
        Assert.Equal("Exact trajectory text", overall.TrajectoryText);
        Assert.Equal(TarotThreeCardSynthesisContract.MixedTransitions, overall.SynthesisFragmentId.Value);
        Assert.Equal("Exact transition text", overall.TransitionText);
        Assert.NotEqual(overall.TrajectoryText, overall.TransitionText);
    }

    [Fact]
    public void Build_PastAndFutureWithoutPresentOmitsRelationsAndOverallWithoutPlaceholders()
    {
        var context = Context();

        var presentation = Assert.IsType<TarotThreeCardInterpretationPresentation>(
            new TarotThreeCardInterpretationPresentationBuilder().Build(
                context.Reading,
                [context.Positions[0], context.Positions[2]],
                null,
                null,
                null,
                Labels()));

        Assert.Equal(["past", "future"], presentation.Blocks.Select(static block => block.BlockId));
        Assert.Equal([TarotThreeCardPosition.Past, TarotThreeCardPosition.Future], presentation.Positions.Select(static item => item.Position));
        Assert.Empty(presentation.Relations);
        Assert.Null(presentation.Overall);
    }

    [Theory]
    [InlineData("wrong-card")]
    [InlineData("wrong-orientation")]
    [InlineData("wrong-position")]
    public void Build_RejectsPositionResolutionThatDoesNotMatchExactDrawnAssignment(string mutation)
    {
        var context = Context();
        var past = context.Positions[0];
        var content = mutation switch
        {
            "wrong-card" => Position(TarotThreeCardPosition.Past, new("major.fool"), TarotCardOrientation.Reversed, "Past text", "past-tag", 1),
            "wrong-orientation" => Position(TarotThreeCardPosition.Past, past.Content.CardId, TarotCardOrientation.Upright, "Past text", "past-tag", 1),
            "wrong-position" => Position(TarotThreeCardPosition.Present, past.Content.CardId, TarotCardOrientation.Reversed, "Past text", "past-tag", 1),
            _ => throw new ArgumentOutOfRangeException(nameof(mutation))
        };
        var mismatched = Resolved(content);

        Assert.Throws<ArgumentException>(() =>
            new TarotThreeCardInterpretationPresentationBuilder().Build(
                context.Reading,
                [mismatched],
                null,
                null,
                null,
                Labels()));
    }

    [Fact]
    public void Build_RejectsRelationThatDoesNotMatchItsAdjacentDrawnEndpoints()
    {
        var context = Context();
        var invalid = PairResolved(context.Reading.Cards[0], context.Reading.Cards[2], "invalid", "invalid", "relation-pp", 2);

        Assert.Throws<ArgumentException>(() =>
            new TarotThreeCardInterpretationPresentationBuilder().Build(
                context.Reading,
                context.Positions,
                invalid,
                context.PresentFuture,
                context.Synthesis,
                Labels()));
    }

    [Theory]
    [InlineData("mode")]
    [InlineData("pack")]
    [InlineData("version")]
    [InlineData("requested-locale")]
    [InlineData("resolved-locale")]
    public void Build_RejectsMixedModeOrResolverProvenance(string mutation)
    {
        var context = Context();
        var original = context.PastPresent;
        var changed = new ResolvedTarotInterpretation<TarotOrientedPairEntry>(
            mutation == "pack" ? new("other") : original.PackId,
            mutation == "version" ? 12 : original.ContentVersion,
            mutation == "mode" ? TarotInterpretationMode.TwoCards : original.ModeId,
            mutation == "requested-locale" ? new("ru") : original.RequestedLocale,
            mutation == "resolved-locale" ? new("en") : original.ResolvedLocale,
            original.Content);

        Assert.Throws<ArgumentException>(() =>
            new TarotThreeCardInterpretationPresentationBuilder().Build(
                context.Reading,
                context.Positions,
                changed,
                context.PresentFuture,
                context.Synthesis,
                Labels()));
    }

    [Theory]
    [InlineData("position")]
    [InlineData("relation")]
    [InlineData("tag")]
    public void Build_MutesPresentationWhenTrustedPackLabelsAreMissing(string missing)
    {
        var context = Context();
        var labels = Labels(
            includePositions: missing != "position",
            includeRelations: missing != "relation",
            includeTags: missing != "tag");

        Assert.Null(new TarotThreeCardInterpretationPresentationBuilder().Build(
            context.Reading,
            context.Positions,
            context.PastPresent,
            context.PresentFuture,
            context.Synthesis,
            labels));
    }

    [Fact]
    public void Build_RejectsOverallWhenPlanAndResolvedResourceIdentityDiffer()
    {
        var context = Context();
        var mismatch = context.Synthesis with
        {
            Plan = new(
                new(TarotThreeCardSynthesisContract.TurningPoint),
                context.Synthesis.Plan.SynthesisFragmentId)
        };

        Assert.Throws<ArgumentException>(() =>
            new TarotThreeCardInterpretationPresentationBuilder().Build(
                context.Reading,
                context.Positions,
                context.PastPresent,
                context.PresentFuture,
                mismatch,
                Labels()));
    }

    private static TestContext Context()
    {
        var past = Card("major.world", StandardTarotSpreads.ThreeCards.Positions[0].Id, TarotCardOrientation.Reversed);
        var present = Card("major.tower", StandardTarotSpreads.ThreeCards.Positions[1].Id, TarotCardOrientation.Upright);
        var future = Card("major.fool", StandardTarotSpreads.ThreeCards.Positions[2].Id, TarotCardOrientation.Reversed);
        var reading = new TarotReading(
            StandardTarotCatalog.Deck.Id,
            StandardTarotSpreads.ThreeCards.Id,
            Instant.FromUnixTimeTicks(37),
            [past, present, future]);
        var positions = new[]
        {
            Resolved(Position(TarotThreeCardPosition.Past, past.Card.Id, past.Orientation, "Past text", "past-tag", 1)),
            Resolved(Position(TarotThreeCardPosition.Present, present.Card.Id, present.Orientation, "Present text", "present-tag", 2)),
            Resolved(Position(TarotThreeCardPosition.Future, future.Card.Id, future.Orientation, "Future text", "future-tag", 3))
        };
        var pastPresent = PairResolved(past, present, "PP interaction", "PP direction", "relation-pp", 2);
        var presentFuture = PairResolved(present, future, "PF interaction", "PF direction", "relation-pf", 3);
        var plan = new TarotThreeCardSynthesisPlan(
            new(TarotThreeCardSynthesisContract.Improving),
            new(TarotThreeCardSynthesisContract.MixedTransitions));
        var synthesis = new TarotThreeCardResolvedSynthesis(
            plan,
            ResourceResolved(TarotSynthesisResourceType.TrajectoryProfile, plan.TrajectoryProfileId, "Exact trajectory text"),
            ResourceResolved(TarotSynthesisResourceType.SynthesisFragment, plan.SynthesisFragmentId, "Exact transition text"));
        return new(reading, positions, pastPresent, presentFuture, synthesis);
    }

    private static TarotDrawnCard Card(string id, TarotSpreadPositionId positionId, TarotCardOrientation orientation) =>
        new(positionId, StandardTarotCatalog.Deck.Cards.Single(card => card.Id.Value == id), orientation);

    private static TarotThreeCardPositionEntry Position(
        TarotThreeCardPosition position,
        TarotCardId cardId,
        TarotCardOrientation orientation,
        string text,
        string tagId,
        int intensity) => new(
        position,
        cardId,
        orientation,
        text,
        [new(new(tagId), intensity - 2, intensity)],
        intensity - 2,
        intensity);

    private static ResolvedTarotInterpretation<TarotThreeCardPositionEntry> Resolved(TarotThreeCardPositionEntry content) =>
        new(new("classic"), 11, TarotInterpretationMode.ThreeCards, new("en"), new("ru"), content);

    private static ResolvedTarotInterpretation<TarotOrientedPairEntry> PairResolved(
        TarotDrawnCard first,
        TarotDrawnCard second,
        string interaction,
        string direction,
        string tagId,
        int intensity)
    {
        var canonical = Assert.IsType<TarotCanonicalPair>(TarotInterpretationKeys.CanonicalizePair(
            first.Card.Id,
            first.Orientation,
            second.Card.Id,
            second.Orientation).Value);
        return new(
            new("classic"),
            11,
            TarotInterpretationMode.ThreeCards,
            new("en"),
            new("ru"),
            new TarotOrientedPairEntry(
                canonical.CardAId,
                canonical.CardBId,
                canonical.OrientationState,
                interaction,
                direction,
                [new(new(tagId), intensity - 2, intensity)],
                intensity - 2,
                intensity));
    }

    private static ResolvedTarotInterpretation<TarotSynthesisResource> ResourceResolved(
        TarotSynthesisResourceType type,
        TarotSynthesisResourceId id,
        string text) => new(
        new("classic"),
        11,
        TarotInterpretationMode.ThreeCards,
        new("en"),
        new("ru"),
        new(type, id, text, $"{{\"text\":\"{text}\"}}\n"));

    private static TarotInterpretationPresentationLabels Labels(
        bool includePositions = true,
        bool includeRelations = true,
        bool includeTags = true) => new(
        new Dictionary<string, string>(StringComparer.Ordinal),
        includePositions
            ? new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["past"] = "Прошлое",
                ["present"] = "Настоящее",
                ["future"] = "Будущее"
            }
            : new Dictionary<string, string>(StringComparer.Ordinal),
        includeRelations
            ? new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["past-present"] = "Связь прошлого и настоящего",
                ["present-future"] = "Связь настоящего и будущего",
                ["overall"] = "Общая картина"
            }
            : new Dictionary<string, string>(StringComparer.Ordinal),
        includeTags
            ? new Dictionary<TarotTagConceptId, string>
            {
                [new("past-tag")] = "Прошлый тег",
                [new("present-tag")] = "Настоящий тег",
                [new("future-tag")] = "Будущий тег",
                [new("relation-pp")] = "Тег первой связи",
                [new("relation-pf")] = "Тег второй связи"
            }
            : new Dictionary<TarotTagConceptId, string>());

    private sealed record TestContext(
        TarotReading Reading,
        IReadOnlyList<ResolvedTarotInterpretation<TarotThreeCardPositionEntry>> Positions,
        ResolvedTarotInterpretation<TarotOrientedPairEntry> PastPresent,
        ResolvedTarotInterpretation<TarotOrientedPairEntry> PresentFuture,
        TarotThreeCardResolvedSynthesis Synthesis);
}
