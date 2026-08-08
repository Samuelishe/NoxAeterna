using System.Text.Json;
using System.Xml.Linq;
using NodaTime;
using NoxAeterna.App.Tarot;
using NoxAeterna.Domain.Tarot;
using NoxAeterna.Interpretation.Tarot.Contracts;
using NoxAeterna.Interpretation.Tarot.Resolution;
using NoxAeterna.Presentation.Localization;
using NoxAeterna.Presentation.Tarot;
using NoxAeterna.Tests.Tooling.Interpretation;
using NoxAeterna.Tools.Repository.Interpretation.Compilation;

namespace NoxAeterna.Tests.App;

public sealed class TarotInterpretationPackageIntegrationTests
{
    [Fact]
    public void PromotedCanonicalRussianModesResolveWhileOnlyCelticCrossStaysUnready()
    {
        var sourceRoot = PathAt("resources", "interpretation", "tarot", "sources", "classic");
        using var output = BuiltInOutput.Create(sourceRoot);
        var stores = BuiltInTarotInterpretationPackStoreCatalog.Create(output.Root);
        var packId = new TarotInterpretationPackId("classic");
        var russian = new TarotInterpretationLocale("ru");
        var english = new TarotInterpretationLocale("en");

        Assert.Empty(stores.Diagnostics);
        Assert.True(stores.TryGetStore(packId, out var store));
        Assert.NotNull(store);
        Assert.True(store.Manifest.Modules[TarotInterpretationMode.SingleCard][russian].Ready);
        Assert.False(store.Manifest.Modules[TarotInterpretationMode.SingleCard][english].Ready);
        Assert.True(store.Manifest.Modules[TarotInterpretationMode.TwoCards][russian].Ready);
        Assert.False(store.Manifest.Modules[TarotInterpretationMode.TwoCards][english].Ready);
        Assert.True(store.Manifest.Modules[TarotInterpretationMode.ThreeCards][russian].Ready);
        Assert.False(store.Manifest.Modules[TarotInterpretationMode.ThreeCards][english].Ready);
        Assert.False(store.Manifest.Modules[TarotInterpretationMode.CelticCross][russian].Ready);
        Assert.False(store.Manifest.Modules[TarotInterpretationMode.CelticCross][english].Ready);

        var resolver = new TarotInterpretationPackResolver(stores, StandardTarotCatalog.Deck);
        var labelSource = new TarotPackagePresentationLabelSource(stores);
        var presentationBuilder = new TarotSingleCardInterpretationPresentationBuilder();
        var card = StandardTarotCatalog.Deck.Cards.Single(static item => item.Id.Value == "major.fool");
        var expectedSectionLabels = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["situation"] = "Основная ситуация",
            ["development"] = "Развитие",
            ["risk"] = "Риск",
            ["outcome"] = "Возможный исход",
            ["advice"] = "Совет"
        };
        var expectedTagLabels = RussianVocabularyLabels(sourceRoot);

        foreach (var orientation in Enum.GetValues<TarotCardOrientation>())
        {
            var resolved = Assert.IsType<ResolvedTarotInterpretation<TarotSingleCardEntry>>(
                resolver.ResolveSingleCard(packId, russian, card.Id, orientation));
            Assert.Equal("ru", resolved.RequestedLocale.Value);
            Assert.Equal("ru", resolved.ResolvedLocale.Value);
            Assert.Equal(orientation, resolved.Content.Orientation);
            Assert.Equal(expectedSectionLabels.Keys.Order(StringComparer.Ordinal), resolved.Content.Sections.Keys.Order(StringComparer.Ordinal));
            Assert.All(resolved.Content.Sections.Values, static text => Assert.False(string.IsNullOrWhiteSpace(text)));
            Assert.NotEmpty(resolved.Content.Tags);

            var labels = labelSource.Resolve(packId, resolved.ContentVersion, resolved.ResolvedLocale);
            Assert.NotNull(labels);
            foreach (var expected in expectedSectionLabels)
            {
                Assert.Equal(expected.Value, labels.SectionLabels[expected.Key]);
            }
            Assert.Equal(expectedTagLabels.Count, labels.TagLabels.Count);
            foreach (var expected in expectedTagLabels)
            {
                Assert.Equal(expected.Value, labels.TagLabels[new(expected.Key)]);
            }

            var assignment = new TarotDrawnCard(
                StandardTarotSpreads.SingleCard.Positions.Single().Id,
                card,
                orientation);
            var reading = new TarotReading(
                StandardTarotCatalog.Deck.Id,
                StandardTarotSpreads.SingleCard.Id,
                Instant.FromUnixTimeTicks(17),
                [assignment]);
            var presentation = Assert.IsType<TarotSingleCardInterpretationPresentation>(
                presentationBuilder.Build(reading, resolved, labels));
            Assert.Equal(5, presentation.Sections.Count);
            Assert.All(presentation.Sections, static section =>
            {
                Assert.False(string.IsNullOrWhiteSpace(section.Label));
                Assert.False(string.IsNullOrWhiteSpace(section.Text));
            });
            Assert.NotEmpty(presentation.Tags);
            Assert.All(presentation.Tags, tag =>
            {
                Assert.Equal(expectedTagLabels[tag.ConceptId.Value], tag.Label);
                Assert.NotEqual(tag.ConceptId.Value, tag.Label);
            });
        }

        var englishFallback = Assert.IsType<ResolvedTarotInterpretation<TarotSingleCardEntry>>(
            resolver.ResolveSingleCard(packId, english, card.Id, TarotCardOrientation.Upright));
        Assert.Equal("en", englishFallback.RequestedLocale.Value);
        Assert.Equal("ru", englishFallback.ResolvedLocale.Value);

        var noContent = Assert.IsType<NoTarotInterpretationContent<TarotResolvedModuleSnapshot>>(
            resolver.ResolveMode(packId, TarotInterpretationMode.CelticCross, russian));
        Assert.Equal(TarotNoContentReason.NoReadyLocale, noContent.Reason);
    }

    [Fact]
    public void PromotedRussianTwoCardCorpusResolvesAllFourStatesSwappedInputsAndEnglishFallback()
    {
        var sourceRoot = PathAt("resources", "interpretation", "tarot", "sources", "classic");
        using var output = BuiltInOutput.Create(sourceRoot);
        var stores = BuiltInTarotInterpretationPackStoreCatalog.Create(output.Root);
        var resolver = new TarotInterpretationPackResolver(stores, StandardTarotCatalog.Deck);
        var adapter = new TarotWorkspaceInterpretationResolverAdapter(resolver);
        var labelSource = new TarotPackagePresentationLabelSource(stores);
        var packId = new TarotInterpretationPackId("classic");
        var russian = new TarotInterpretationLocale("ru");
        var english = new TarotInterpretationLocale("en");
        var cardA = StandardTarotCatalog.Deck.Cards.Single(static card => card.Id.Value == "major.chariot");
        var cardB = StandardTarotCatalog.Deck.Cards.Single(static card => card.Id.Value == "major.death");
        var bundlePath = Path.Combine(
            sourceRoot,
            "content",
            "ru",
            "oriented-pairs",
            "major.chariot__major.death.json");
        using var bundle = JsonDocument.Parse(File.ReadAllText(bundlePath));
        var vocabulary = RussianVocabularyLabels(sourceRoot);

        var cases = new[]
        {
            (A: TarotCardOrientation.Upright, B: TarotCardOrientation.Upright,
                State: TarotOrientedPairState.UprightUpright, SourceKey: "upright-upright"),
            (A: TarotCardOrientation.Upright, B: TarotCardOrientation.Reversed,
                State: TarotOrientedPairState.UprightReversed, SourceKey: "upright-reversed"),
            (A: TarotCardOrientation.Reversed, B: TarotCardOrientation.Upright,
                State: TarotOrientedPairState.ReversedUpright, SourceKey: "reversed-upright"),
            (A: TarotCardOrientation.Reversed, B: TarotCardOrientation.Reversed,
                State: TarotOrientedPairState.ReversedReversed, SourceKey: "reversed-reversed")
        };

        foreach (var testCase in cases)
        {
            var resolved = Assert.IsType<ResolvedTarotInterpretation<TarotOrientedPairEntry>>(
                adapter.ResolveOrientedPair(packId, russian, cardA.Id, testCase.A, cardB.Id, testCase.B));
            var swapped = Assert.IsType<ResolvedTarotInterpretation<TarotOrientedPairEntry>>(
                adapter.ResolveOrientedPair(packId, russian, cardB.Id, testCase.B, cardA.Id, testCase.A));
            var authored = bundle.RootElement.GetProperty("states").GetProperty(testCase.SourceKey);

            Assert.Equal("ru", resolved.RequestedLocale.Value);
            Assert.Equal("ru", resolved.ResolvedLocale.Value);
            Assert.Equal(TarotInterpretationMode.TwoCards, resolved.ModeId);
            Assert.Equal(cardA.Id, resolved.Content.CardAId);
            Assert.Equal(cardB.Id, resolved.Content.CardBId);
            Assert.Equal(testCase.State, resolved.Content.OrientationState);
            Assert.Equal(authored.GetProperty("interaction").GetString(), resolved.Content.Interaction);
            Assert.Equal(authored.GetProperty("direction").GetString(), resolved.Content.Direction);
            Assert.Equal(authored.GetProperty("overallValence").GetInt32(), resolved.Content.OverallValence);
            Assert.Equal(authored.GetProperty("overallIntensity").GetInt32(), resolved.Content.OverallIntensity);
            Assert.Equal(
                authored.GetProperty("tags").EnumerateArray().Select(static tag =>
                    (
                        tag.GetProperty("conceptId").GetString(),
                        tag.GetProperty("valence").GetInt32(),
                        tag.GetProperty("intensity").GetInt32())),
                resolved.Content.Tags.Select(static tag =>
                    ((string?)tag.ConceptId.Value, tag.Valence, tag.Intensity)));
            Assert.Equal(resolved.Content.CardAId, swapped.Content.CardAId);
            Assert.Equal(resolved.Content.CardBId, swapped.Content.CardBId);
            Assert.Equal(resolved.Content.OrientationState, swapped.Content.OrientationState);
            Assert.Equal(resolved.Content.Interaction, swapped.Content.Interaction);
            Assert.Equal(resolved.Content.Direction, swapped.Content.Direction);

            var labels = Assert.IsType<TarotInterpretationPresentationLabels>(
                labelSource.Resolve(packId, resolved.ContentVersion, resolved.ResolvedLocale));
            var reading = new TarotReading(
                StandardTarotCatalog.Deck.Id,
                StandardTarotSpreads.TwoCards.Id,
                Instant.FromUnixTimeTicks(23),
                [
                    new(StandardTarotSpreads.TwoCards.Positions[0].Id, cardB, testCase.B),
                    new(StandardTarotSpreads.TwoCards.Positions[1].Id, cardA, testCase.A)
                ]);
            var presentation = Assert.IsType<TarotOrientedPairInterpretationPresentation>(
                new TarotOrientedPairInterpretationPresentationBuilder().Build(reading, resolved, labels));
            Assert.Equal(resolved.Content.Tags.Count, presentation.Tags.Count);
            Assert.All(presentation.Tags, tag =>
            {
                Assert.Equal(vocabulary[tag.ConceptId.Value], tag.Label);
                Assert.NotEqual(tag.ConceptId.Value, tag.Label);
                var authoredTag = resolved.Content.Tags.Single(item => item.ConceptId == tag.ConceptId);
                Assert.Equal(authoredTag.Valence, tag.Valence);
                Assert.Equal(authoredTag.Intensity, tag.Intensity);
            });
        }

        var englishFallback = Assert.IsType<ResolvedTarotInterpretation<TarotOrientedPairEntry>>(
            adapter.ResolveOrientedPair(
                packId,
                english,
                cardA.Id,
                TarotCardOrientation.Upright,
                cardB.Id,
                TarotCardOrientation.Reversed));
        Assert.Equal("en", englishFallback.RequestedLocale.Value);
        Assert.Equal("ru", englishFallback.ResolvedLocale.Value);
        Assert.Equal(
            bundle.RootElement.GetProperty("states").GetProperty("upright-reversed")
                .GetProperty("interaction").GetString(),
            englishFallback.Content.Interaction);
        var fallbackLabels = Assert.IsType<TarotInterpretationPresentationLabels>(
            labelSource.Resolve(packId, englishFallback.ContentVersion, englishFallback.ResolvedLocale));
        Assert.All(englishFallback.Content.Tags, tag =>
            Assert.Equal(vocabulary[tag.ConceptId.Value], fallbackLabels.TagLabels[tag.ConceptId]));

        var headingProvider = new FallbackLocalizationProvider(
        [
            JsonLocalizationCatalogLoader.LoadFromFile(
                LocalizationScope.Ui,
                new LanguageCode("ru"),
                PathAt("resources", "localization", "ui", "ru.json")),
            JsonLocalizationCatalogLoader.LoadFromFile(
                LocalizationScope.Ui,
                new LanguageCode("en"),
                PathAt("resources", "localization", "ui", "en.json"))
        ]);
        var headingLanguage = new LanguageCode(englishFallback.ResolvedLocale.Value);
        Assert.Equal(
            "Взаимодействие",
            headingProvider.Get(
                LocalizationScope.Ui,
                headingLanguage,
                new LocalizationKey("ui.tarot.interpretation.pair.interaction")).Text);
        Assert.Equal(
            "Направление",
            headingProvider.Get(
                LocalizationScope.Ui,
                headingLanguage,
                new LocalizationKey("ui.tarot.interpretation.pair.direction")).Text);
    }

    [Fact]
    public void PromotedRussianThreeCardsMatchesExactSourcePlannerResourcesFallbackAndAllPairOrientations()
    {
        var sourceRoot = PathAt("resources", "interpretation", "tarot", "sources", "classic");
        using var output = BuiltInOutput.Create(sourceRoot);
        var stores = BuiltInTarotInterpretationPackStoreCatalog.Create(output.Root);
        var resolver = new TarotInterpretationPackResolver(stores, StandardTarotCatalog.Deck);
        var adapter = new TarotWorkspaceInterpretationResolverAdapter(resolver);
        var labelSource = new TarotPackagePresentationLabelSource(stores);
        var packId = new TarotInterpretationPackId("classic");
        var russian = new TarotInterpretationLocale("ru");
        var english = new TarotInterpretationLocale("en");
        var pastCard = StandardTarotCatalog.Deck.Cards.Single(static card => card.Id.Value == "major.world");
        var presentCard = StandardTarotCatalog.Deck.Cards.Single(static card => card.Id.Value == "major.tower");
        var futureCard = StandardTarotCatalog.Deck.Cards.Single(static card => card.Id.Value == "major.fool");
        var reading = new TarotReading(
            StandardTarotCatalog.Deck.Id,
            StandardTarotSpreads.ThreeCards.Id,
            Instant.FromUnixTimeTicks(29),
            [
                new(StandardTarotSpreads.ThreeCards.Positions[0].Id, pastCard, TarotCardOrientation.Reversed),
                new(StandardTarotSpreads.ThreeCards.Positions[1].Id, presentCard, TarotCardOrientation.Upright),
                new(StandardTarotSpreads.ThreeCards.Positions[2].Id, futureCard, TarotCardOrientation.Reversed)
            ]);

        var positions = new[]
        {
            Assert.IsType<ResolvedTarotInterpretation<TarotThreeCardPositionEntry>>(
                adapter.ResolveThreeCardPosition(packId, russian, TarotThreeCardPosition.Past, pastCard.Id, TarotCardOrientation.Reversed)),
            Assert.IsType<ResolvedTarotInterpretation<TarotThreeCardPositionEntry>>(
                adapter.ResolveThreeCardPosition(packId, russian, TarotThreeCardPosition.Present, presentCard.Id, TarotCardOrientation.Upright)),
            Assert.IsType<ResolvedTarotInterpretation<TarotThreeCardPositionEntry>>(
                adapter.ResolveThreeCardPosition(packId, russian, TarotThreeCardPosition.Future, futureCard.Id, TarotCardOrientation.Reversed))
        };
        AssertPositionMatchesSource(sourceRoot, positions[0], "major.world", "past", "reversed");
        AssertPositionMatchesSource(sourceRoot, positions[1], "major.tower", "present", "upright");
        AssertPositionMatchesSource(sourceRoot, positions[2], "major.fool", "future", "reversed");

        var pastPresent = Assert.IsType<ResolvedTarotInterpretation<TarotOrientedPairEntry>>(
            adapter.ResolveThreeCardRelation(
                packId,
                russian,
                pastCard.Id,
                TarotCardOrientation.Reversed,
                presentCard.Id,
                TarotCardOrientation.Upright));
        var presentFuture = Assert.IsType<ResolvedTarotInterpretation<TarotOrientedPairEntry>>(
            adapter.ResolveThreeCardRelation(
                packId,
                russian,
                presentCard.Id,
                TarotCardOrientation.Upright,
                futureCard.Id,
                TarotCardOrientation.Reversed));
        Assert.Equal(TarotInterpretationMode.ThreeCards, pastPresent.ModeId);
        Assert.Equal(TarotInterpretationMode.ThreeCards, presentFuture.ModeId);
        Assert.Equal("major.tower", pastPresent.Content.CardAId.Value);
        Assert.Equal("major.world", pastPresent.Content.CardBId.Value);
        Assert.Equal(TarotOrientedPairState.UprightReversed, pastPresent.Content.OrientationState);
        Assert.Equal("major.fool", presentFuture.Content.CardAId.Value);
        Assert.Equal("major.tower", presentFuture.Content.CardBId.Value);
        Assert.Equal(TarotOrientedPairState.ReversedUpright, presentFuture.Content.OrientationState);
        AssertPairMatchesSource(sourceRoot, pastPresent, "major.tower", "major.world", "upright-reversed");
        AssertPairMatchesSource(sourceRoot, presentFuture, "major.fool", "major.tower", "reversed-upright");

        var plan = TarotThreeCardSynthesisPlanner.Plan(new(
            positions[0].Content.OverallValence,
            positions[1].Content.OverallValence,
            positions[2].Content.OverallValence,
            pastPresent.Content.OverallValence,
            pastPresent.Content.OverallIntensity,
            presentFuture.Content.OverallValence,
            presentFuture.Content.OverallIntensity));
        Assert.Equal(TarotThreeCardSynthesisContract.DifficultContinuity, plan.TrajectoryProfileId.Value);
        Assert.Equal(TarotThreeCardSynthesisContract.MutuallyConflicted, plan.SynthesisFragmentId.Value);
        var trajectory = Assert.IsType<ResolvedTarotInterpretation<TarotSynthesisResource>>(
            adapter.ResolveThreeCardSynthesisResource(
                packId,
                russian,
                TarotSynthesisResourceType.TrajectoryProfile,
                plan.TrajectoryProfileId));
        var transition = Assert.IsType<ResolvedTarotInterpretation<TarotSynthesisResource>>(
            adapter.ResolveThreeCardSynthesisResource(
                packId,
                russian,
                TarotSynthesisResourceType.SynthesisFragment,
                plan.SynthesisFragmentId));
        AssertSynthesisMatchesSource(sourceRoot, trajectory);
        AssertSynthesisMatchesSource(sourceRoot, transition);

        var labels = Assert.IsType<TarotInterpretationPresentationLabels>(
            labelSource.Resolve(packId, positions[0].ContentVersion, positions[0].ResolvedLocale));
        Assert.Equal("Прошлое", labels.ThreeCardPositionLabels["past"]);
        Assert.Equal("Настоящее", labels.ThreeCardPositionLabels["present"]);
        Assert.Equal("Будущее", labels.ThreeCardPositionLabels["future"]);
        Assert.Equal("Что привело к настоящему", labels.RelationLabels["past-present"]);
        Assert.Equal("Куда движется ситуация", labels.RelationLabels["present-future"]);
        Assert.Equal("Общая картина", labels.RelationLabels["overall"]);
        var presentation = Assert.IsType<TarotThreeCardInterpretationPresentation>(
            new TarotThreeCardInterpretationPresentationBuilder().Build(
                reading,
                positions,
                pastPresent,
                presentFuture,
                new(plan, trajectory, transition),
                labels));
        Assert.Equal(
            ["past", "past-present", "present", "present-future", "future", "overall"],
            presentation.Blocks.Select(static block => block.BlockId));
        Assert.Equal(trajectory.Content.Text, presentation.Overall?.TrajectoryText);
        Assert.Equal(transition.Content.Text, presentation.Overall?.TransitionText);
        Assert.All(presentation.Positions, block =>
        {
            var authored = positions.Single(item => item.Content.Position == block.Position).Content.Tags;
            Assert.Equal(
                authored.Select(static tag => (tag.ConceptId, tag.Valence, tag.Intensity)),
                block.Tags.Select(static tag => (tag.ConceptId, tag.Valence, tag.Intensity)));
            Assert.All(block.Tags, static tag => Assert.NotEqual(tag.ConceptId.Value, tag.Label));
        });
        Assert.All(presentation.Relations, block =>
        {
            var authored = (block.Relation == TarotThreeCardRelationId.PastPresent
                ? pastPresent
                : presentFuture).Content.Tags;
            Assert.Equal(
                authored.Select(static tag => (tag.ConceptId, tag.Valence, tag.Intensity)),
                block.Tags.Select(static tag => (tag.ConceptId, tag.Valence, tag.Intensity)));
            Assert.All(block.Tags, static tag => Assert.NotEqual(tag.ConceptId.Value, tag.Label));
        });

        var englishPositions = new[]
        {
            Assert.IsType<ResolvedTarotInterpretation<TarotThreeCardPositionEntry>>(
                adapter.ResolveThreeCardPosition(packId, english, TarotThreeCardPosition.Past, pastCard.Id, TarotCardOrientation.Reversed)),
            Assert.IsType<ResolvedTarotInterpretation<TarotThreeCardPositionEntry>>(
                adapter.ResolveThreeCardPosition(packId, english, TarotThreeCardPosition.Present, presentCard.Id, TarotCardOrientation.Upright)),
            Assert.IsType<ResolvedTarotInterpretation<TarotThreeCardPositionEntry>>(
                adapter.ResolveThreeCardPosition(packId, english, TarotThreeCardPosition.Future, futureCard.Id, TarotCardOrientation.Reversed))
        };
        var englishPastPresent = Assert.IsType<ResolvedTarotInterpretation<TarotOrientedPairEntry>>(
            adapter.ResolveThreeCardRelation(packId, english, pastCard.Id, TarotCardOrientation.Reversed, presentCard.Id, TarotCardOrientation.Upright));
        var englishPresentFuture = Assert.IsType<ResolvedTarotInterpretation<TarotOrientedPairEntry>>(
            adapter.ResolveThreeCardRelation(packId, english, presentCard.Id, TarotCardOrientation.Upright, futureCard.Id, TarotCardOrientation.Reversed));
        var englishTrajectory = Assert.IsType<ResolvedTarotInterpretation<TarotSynthesisResource>>(
            adapter.ResolveThreeCardSynthesisResource(packId, english, TarotSynthesisResourceType.TrajectoryProfile, plan.TrajectoryProfileId));
        var englishTransition = Assert.IsType<ResolvedTarotInterpretation<TarotSynthesisResource>>(
            adapter.ResolveThreeCardSynthesisResource(packId, english, TarotSynthesisResourceType.SynthesisFragment, plan.SynthesisFragmentId));
        Assert.All(englishPositions, static item => Assert.Equal("ru", item.ResolvedLocale.Value));
        Assert.All(
            new[] { englishPastPresent, englishPresentFuture },
            static item => Assert.Equal("ru", item.ResolvedLocale.Value));
        Assert.All(
            new[] { englishTrajectory, englishTransition },
            static item => Assert.Equal("ru", item.ResolvedLocale.Value));
        var fallbackPresentation = Assert.IsType<TarotThreeCardInterpretationPresentation>(
            new TarotThreeCardInterpretationPresentationBuilder().Build(
                reading,
                englishPositions,
                englishPastPresent,
                englishPresentFuture,
                new(plan, englishTrajectory, englishTransition),
                Assert.IsType<TarotInterpretationPresentationLabels>(
                    labelSource.Resolve(packId, englishPositions[0].ContentVersion, englishPositions[0].ResolvedLocale))));
        Assert.Equal("en", fallbackPresentation.RequestedLocale.Value);
        Assert.Equal("ru", fallbackPresentation.ResolvedLocale.Value);
        Assert.Equal("Прошлое", fallbackPresentation.Positions[0].Label);
        Assert.Equal("Общая картина", fallbackPresentation.Overall?.Label);

        var orientationCases = new[]
        {
            (Past: TarotCardOrientation.Upright, Present: TarotCardOrientation.Upright, Future: TarotCardOrientation.Upright,
                PastPresent: TarotOrientedPairState.UprightUpright, PresentFuture: TarotOrientedPairState.UprightUpright),
            (Past: TarotCardOrientation.Reversed, Present: TarotCardOrientation.Upright, Future: TarotCardOrientation.Reversed,
                PastPresent: TarotOrientedPairState.UprightReversed, PresentFuture: TarotOrientedPairState.ReversedUpright),
            (Past: TarotCardOrientation.Reversed, Present: TarotCardOrientation.Reversed, Future: TarotCardOrientation.Reversed,
                PastPresent: TarotOrientedPairState.ReversedReversed, PresentFuture: TarotOrientedPairState.ReversedReversed)
        };
        foreach (var item in orientationCases)
        {
            var first = Assert.IsType<ResolvedTarotInterpretation<TarotOrientedPairEntry>>(
                adapter.ResolveThreeCardRelation(packId, russian, pastCard.Id, item.Past, presentCard.Id, item.Present));
            var second = Assert.IsType<ResolvedTarotInterpretation<TarotOrientedPairEntry>>(
                adapter.ResolveThreeCardRelation(packId, russian, presentCard.Id, item.Present, futureCard.Id, item.Future));
            Assert.Equal(item.PastPresent, first.Content.OrientationState);
            Assert.Equal(item.PresentFuture, second.Content.OrientationState);
            Assert.Equal(TarotInterpretationMode.ThreeCards, first.ModeId);
            Assert.Equal(TarotInterpretationMode.ThreeCards, second.ModeId);
        }
    }

    [Fact]
    public void BuiltInSkeletonRegistersSelectorNamesAndRemainsSilentlyNotReady()
    {
        using var fixture=InterpretationToolingFixture.CreateSkeleton();using var output=BuiltInOutput.Create(fixture);
        var stores=BuiltInTarotInterpretationPackStoreCatalog.Create(output.Root);var catalog=new TarotInterpretationPackCatalog(stores,stores.PackIds);

        Assert.Empty(stores.Diagnostics);Assert.Equal(new[]{"classic"},catalog.AvailablePackIds.Select(static id=>id.Value));
        Assert.Equal("Классика",catalog.ResolveDisplayName(new("classic"),new("ru")));Assert.Equal("Classic",catalog.ResolveDisplayName(new("classic"),new("en")));Assert.Equal("Classic",catalog.ResolveDisplayName(new("classic"),new("zh")));
        var result=Assert.IsType<NoTarotInterpretationContent<TarotSingleCardEntry>>(new TarotInterpretationPackResolver(stores,StandardTarotCatalog.Deck).ResolveSingleCard(new("classic"),new("ru"),new("major.fool"),TarotCardOrientation.Upright));
        Assert.Equal(TarotNoContentReason.NoReadyLocale,result.Reason);
    }

    [Fact]
    public void BuiltInDamageIsControlledUnavailableAndLabelsComeFromSamePackageLocale()
    {
        using var fixture=InterpretationToolingFixture.CreateSkeleton();using var output=BuiltInOutput.Create(fixture);
        var valid=BuiltInTarotInterpretationPackStoreCatalog.Create(output.Root);var labels=new TarotPackagePresentationLabelSource(valid).Resolve(new("classic"),1,new("ru"));
        Assert.NotNull(labels);Assert.Equal("Label situation",labels.SectionLabels["situation"]);Assert.Empty(labels.TagLabels);

        File.WriteAllBytes(output.PackagePath,"corrupt"u8.ToArray());var damaged=BuiltInTarotInterpretationPackStoreCatalog.Create(output.Root);
        Assert.Empty(damaged.PackIds);Assert.Equal("package.unavailable",Assert.Single(damaged.Diagnostics).Code);
    }

    [Fact]
    public void AppBuildContractShipsOnePackageAndNoAuthoringJsonOrToolingRuntimeReference()
    {
        var project=XDocument.Load(PathAt("NoxAeterna.App","NoxAeterna.App.csproj"));var xml=project.ToString(SaveOptions.DisableFormatting);
        Assert.Contains("CompileBuiltInInterpretationPackage",xml,StringComparison.Ordinal);Assert.Contains("resources\\interpretation\\tarot\\sources\\classic",xml,StringComparison.Ordinal);Assert.Contains("classic.noxinterp",xml,StringComparison.Ordinal);
        Assert.DoesNotContain("resources\\interpretation\\tarot\\packs\\**",xml,StringComparison.Ordinal);Assert.DoesNotContain("content\\**\\*.json",xml,StringComparison.Ordinal);
        var tooling=project.Descendants("ProjectReference").Single(item=>((string?)item.Attribute("Include"))?.Contains("NoxAeterna.Tools.Repository",StringComparison.Ordinal)==true);
        Assert.Equal("false",(string?)tooling.Attribute("ReferenceOutputAssembly"));Assert.Equal("false",(string?)tooling.Attribute("Private"));Assert.Equal("all",(string?)tooling.Attribute("PrivateAssets"));

        var configuration=new DirectoryInfo(AppContext.BaseDirectory).Parent?.Name??"Debug";var output=PathAt("NoxAeterna.App","bin",configuration,"net10.0");
        var packages=Directory.GetFiles(output,"*.noxinterp",SearchOption.AllDirectories);Assert.Single(packages);Assert.EndsWith(Path.Combine("resources","interpretation","tarot","packs","classic.noxinterp"),packages[0],StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(Directory.GetFiles(output,"*.json",SearchOption.AllDirectories),path=>path.Contains($"{Path.DirectorySeparatorChar}interpretation{Path.DirectorySeparatorChar}",StringComparison.OrdinalIgnoreCase));
        Assert.False(File.Exists(Path.Combine(output,"NoxAeterna.Tools.Repository.dll")));
    }

    private static void AssertPositionMatchesSource(
        string sourceRoot,
        ResolvedTarotInterpretation<TarotThreeCardPositionEntry> resolved,
        string cardId,
        string position,
        string orientation)
    {
        using var source = JsonDocument.Parse(File.ReadAllText(Path.Combine(
            sourceRoot,
            "content",
            "ru",
            "three-card-positions",
            $"{cardId}.json")));
        var authored = source.RootElement.GetProperty("states").GetProperty(position).GetProperty(orientation);

        Assert.Equal("ru", resolved.RequestedLocale.Value);
        Assert.Equal("ru", resolved.ResolvedLocale.Value);
        Assert.Equal(TarotInterpretationMode.ThreeCards, resolved.ModeId);
        Assert.Equal(cardId, resolved.Content.CardId.Value);
        Assert.Equal(authored.GetProperty("text").GetString(), resolved.Content.Text);
        Assert.Equal(authored.GetProperty("overallValence").GetInt32(), resolved.Content.OverallValence);
        Assert.Equal(authored.GetProperty("overallIntensity").GetInt32(), resolved.Content.OverallIntensity);
        Assert.Equal(
            authored.GetProperty("tags").EnumerateArray().Select(static tag =>
                (
                    tag.GetProperty("conceptId").GetString(),
                    tag.GetProperty("valence").GetInt32(),
                    tag.GetProperty("intensity").GetInt32())),
            resolved.Content.Tags.Select(static tag =>
                ((string?)tag.ConceptId.Value, tag.Valence, tag.Intensity)));
    }

    private static void AssertPairMatchesSource(
        string sourceRoot,
        ResolvedTarotInterpretation<TarotOrientedPairEntry> resolved,
        string cardAId,
        string cardBId,
        string state)
    {
        using var source = JsonDocument.Parse(File.ReadAllText(Path.Combine(
            sourceRoot,
            "content",
            "ru",
            "oriented-pairs",
            $"{cardAId}__{cardBId}.json")));
        var authored = source.RootElement.GetProperty("states").GetProperty(state);

        Assert.Equal(authored.GetProperty("interaction").GetString(), resolved.Content.Interaction);
        Assert.Equal(authored.GetProperty("direction").GetString(), resolved.Content.Direction);
        Assert.Equal(authored.GetProperty("overallValence").GetInt32(), resolved.Content.OverallValence);
        Assert.Equal(authored.GetProperty("overallIntensity").GetInt32(), resolved.Content.OverallIntensity);
        Assert.Equal(
            authored.GetProperty("tags").EnumerateArray().Select(static tag =>
                (
                    tag.GetProperty("conceptId").GetString(),
                    tag.GetProperty("valence").GetInt32(),
                    tag.GetProperty("intensity").GetInt32())),
            resolved.Content.Tags.Select(static tag =>
                ((string?)tag.ConceptId.Value, tag.Valence, tag.Intensity)));
    }

    private static void AssertSynthesisMatchesSource(
        string sourceRoot,
        ResolvedTarotInterpretation<TarotSynthesisResource> resolved)
    {
        var type = resolved.Content.ResourceType switch
        {
            TarotSynthesisResourceType.TrajectoryProfile => "trajectory-profile",
            TarotSynthesisResourceType.SynthesisFragment => "synthesis-fragment",
            _ => throw new ArgumentOutOfRangeException()
        };
        using var source = JsonDocument.Parse(File.ReadAllText(Path.Combine(
            sourceRoot,
            "content",
            "ru",
            "synthesis",
            type,
            $"{resolved.Content.ResourceId.Value}.json")));

        Assert.Equal("ru", resolved.ResolvedLocale.Value);
        Assert.Equal(TarotInterpretationMode.ThreeCards, resolved.ModeId);
        Assert.Equal(type, source.RootElement.GetProperty("resourceType").GetString());
        Assert.Equal(resolved.Content.ResourceId.Value, source.RootElement.GetProperty("resourceId").GetString());
        Assert.Equal(source.RootElement.GetProperty("data").GetProperty("text").GetString(), resolved.Content.Text);
    }

    private static string PathAt(params string[] segments)=>Path.Combine(new[]{Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,"..","..","..",".."))}.Concat(segments).ToArray());

    private static Dictionary<string, string> RussianVocabularyLabels(string sourceRoot)
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var path in Directory.GetFiles(Path.Combine(sourceRoot, "content", "ru", "vocabulary"), "*.json"))
        {
            using var document = JsonDocument.Parse(File.ReadAllText(path));
            result.Add(
                document.RootElement.GetProperty("conceptId").GetString()!,
                document.RootElement.GetProperty("label").GetString()!);
        }
        return result;
    }

    private sealed class BuiltInOutput:IDisposable
    {
        private BuiltInOutput(string root,string path)=>(Root,PackagePath)=(root,path);public string Root{get;}public string PackagePath{get;}
        public static BuiltInOutput Create(InterpretationToolingFixture fixture)=>Create(fixture.Root);
        public static BuiltInOutput Create(string sourceRoot){var root=Path.Combine(Path.GetTempPath(),$"NoxAeterna-app-package-{Guid.NewGuid():N}");var path=Path.Combine(root,BuiltInTarotInterpretationPackStoreCatalog.ClassicPackageOutputPath.Replace('/',Path.DirectorySeparatorChar));var report=new InterpretationPackageCompiler().Compile(sourceRoot,path,false);Assert.True(report.Success,string.Join(Environment.NewLine,report.Diagnostics.Select(static item=>item.Message)));return new(root,path);}
        public void Dispose(){if(Directory.Exists(Root))Directory.Delete(Root,true);}
    }
}
