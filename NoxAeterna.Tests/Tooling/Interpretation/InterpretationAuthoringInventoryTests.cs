using NoxAeterna.Domain.Tarot;
using NoxAeterna.Interpretation.Tarot.Contracts;
using NoxAeterna.Tools.Repository.Interpretation.Analysis;
using NoxAeterna.Tools.Repository.Interpretation.Reports;

namespace NoxAeterna.Tests.Tooling.Interpretation;

public sealed class InterpretationAuthoringInventoryTests
{
    [Fact]
    public void ScopesRuAndEnInventoriesIndependentlyWhenOnlyRuHasContent()
    {
        using var fixture = InterpretationToolingFixture.CreateSkeleton();
        fixture.AddSingle("ru", "major.fool");
        var analyzer = new InterpretationAuthoringInventoryAnalyzer();

        var ru = analyzer.Analyze(fixture.Root, "ru", InterpretationAuthoringCorpus.SingleCard);
        var en = analyzer.Analyze(fixture.Root, "en", InterpretationAuthoringCorpus.SingleCard);

        Assert.True(ru.Success);
        Assert.True(en.Success);
        Assert.Equal(1, ru.Counts["presentBundles"]);
        Assert.Equal(2, ru.Counts["presentStates"]);
        Assert.Equal(77, ru.Counts["missingBundles"]);
        Assert.Equal(154, ru.Counts["missingStates"]);
        Assert.Equal(0, en.Counts["presentBundles"]);
        Assert.Equal(0, en.Counts["presentStates"]);
        Assert.Equal(78, en.Counts["missingBundles"]);
        Assert.Equal(156, en.Counts["missingStates"]);
        Assert.DoesNotContain("major.fool", ru.Inventories["missingIdentities"]);
        Assert.Contains("major.fool", en.Inventories["missingIdentities"]);
    }

    [Fact]
    public void EmptyRuSingleCardInventoryHasCanonicalCountsAndAllCardIdentities()
    {
        using var fixture = InterpretationToolingFixture.CreateSkeleton();

        var report = new InterpretationAuthoringInventoryAnalyzer()
            .Analyze(fixture.Root, "ru", InterpretationAuthoringCorpus.SingleCard);

        Assert.True(report.Success);
        Assert.Equal("ru", report.Details["locale"]);
        Assert.Equal("single-card", report.Details["corpus"]);
        Assert.Equal(78, report.Counts["expectedBundles"]);
        Assert.Equal(0, report.Counts["presentBundles"]);
        Assert.Equal(78, report.Counts["missingBundles"]);
        Assert.Equal(156, report.Counts["expectedStates"]);
        Assert.Equal(0, report.Counts["presentStates"]);
        Assert.Equal(156, report.Counts["missingStates"]);
        Assert.Equal(CanonicalCardIdentities(), report.Inventories["missingIdentities"]);
    }

    [Fact]
    public void OrientedPairInventoryUsesAllCanonicalPairIdentitiesInFrozenOrder()
    {
        using var fixture = InterpretationToolingFixture.CreateSkeleton();
        var cards = CanonicalCardIdentities();
        var expected = cards
            .SelectMany((cardA, index) => cards.Skip(index + 1).Select(cardB => $"{cardA}__{cardB}"))
            .ToArray();

        var report = new InterpretationAuthoringInventoryAnalyzer()
            .Analyze(fixture.Root, "ru", InterpretationAuthoringCorpus.OrientedPairs);

        Assert.True(report.Success);
        Assert.Equal(3003, report.Counts["expectedBundles"]);
        Assert.Equal(12012, report.Counts["expectedStates"]);
        Assert.Equal(3003, report.Counts["missingBundles"]);
        Assert.Equal(12012, report.Counts["missingStates"]);
        Assert.Equal(expected, report.Inventories["missingIdentities"]);
        Assert.Equal(3003, report.Inventories["missingIdentities"].Distinct(StringComparer.Ordinal).Count());
        Assert.Equal("major.chariot__major.death", report.Inventories["missingIdentities"][0]);
        Assert.Equal("minor.wands.three__minor.wands.two", report.Inventories["missingIdentities"][^1]);
    }

    [Fact]
    public void ThreeCardPositionInventoryUsesAllCanonicalCardIdentities()
    {
        using var fixture = InterpretationToolingFixture.CreateSkeleton();

        var report = new InterpretationAuthoringInventoryAnalyzer()
            .Analyze(fixture.Root, "ru", InterpretationAuthoringCorpus.ThreeCardPositions);

        Assert.True(report.Success);
        Assert.Equal(78, report.Counts["expectedBundles"]);
        Assert.Equal(468, report.Counts["expectedStates"]);
        Assert.Equal(78, report.Counts["missingBundles"]);
        Assert.Equal(468, report.Counts["missingStates"]);
        Assert.Equal(CanonicalCardIdentities(), report.Inventories["missingIdentities"]);
    }

    [Fact]
    public void ThreeCardSynthesisInventoryReportsExactFrozenMissingResources()
    {
        using var fixture = InterpretationToolingFixture.CreateSkeleton();
        var expected = TarotThreeCardSynthesisContract.RequiredResources
            .Select(identity => $"{NoxAeterna.Interpretation.Tarot.Serialization.TarotSchemaText.Get(identity.ResourceType, NoxAeterna.Interpretation.Tarot.Serialization.TarotSchemaText.SynthesisResourceTypes)}/{identity.ResourceId.Value}")
            .Order(StringComparer.Ordinal)
            .ToArray();

        var empty = new InterpretationAuthoringInventoryAnalyzer()
            .Analyze(fixture.Root, "ru", InterpretationAuthoringCorpus.ThreeCardSynthesis);
        fixture.AddSynthesis("ru", TarotSynthesisResourceType.TrajectoryProfile, "trajectory-profile", TarotThreeCardSynthesisContract.Improving);
        var partial = new InterpretationAuthoringInventoryAnalyzer()
            .Analyze(fixture.Root, "ru", InterpretationAuthoringCorpus.ThreeCardSynthesis);

        Assert.True(empty.Success);
        Assert.Equal(13, empty.Counts["expectedResources"]);
        Assert.Equal(0, empty.Counts["presentResources"]);
        Assert.Equal(13, empty.Counts["missingResources"]);
        Assert.Equal(expected, empty.Inventories["missingIdentities"]);
        Assert.Equal(1, partial.Counts["presentResources"]);
        Assert.Equal(12, partial.Counts["missingResources"]);
        Assert.DoesNotContain("trajectory-profile/improving", partial.Inventories["missingIdentities"]);
    }

    [Fact]
    public void IncompleteNotReadyCorpusSucceedsButMalformedSourceFails()
    {
        using var fixture = InterpretationToolingFixture.CreateSkeleton();
        var analyzer = new InterpretationAuthoringInventoryAnalyzer();

        var incomplete = analyzer.Analyze(fixture.Root, "ru", InterpretationAuthoringCorpus.SingleCard);
        File.WriteAllText(fixture.ManifestPath, "{");
        var malformed = analyzer.Analyze(fixture.Root, "ru", InterpretationAuthoringCorpus.SingleCard);

        Assert.True(incomplete.Success);
        Assert.False(malformed.Success);
        Assert.Contains(malformed.Diagnostics, item => item.Code == "source.json" && item.Severity == InterpretationToolSeverity.Error);
    }

    [Fact]
    public void StatusJsonIsCompleteAndStableWhileConsoleIsBounded()
    {
        using var fixture = InterpretationToolingFixture.CreateSkeleton();
        var report = new InterpretationAuthoringInventoryAnalyzer()
            .Analyze(fixture.Root, "ru", InterpretationAuthoringCorpus.OrientedPairs);

        var firstJson = InterpretationToolReportWriter.WriteJson(report);
        var secondJson = InterpretationToolReportWriter.WriteJson(report);
        var console = InterpretationToolReportWriter.WriteConsole(report);

        Assert.Equal(firstJson, secondJson);
        Assert.Equal(3003, report.Inventories["missingIdentities"].Count);
        Assert.Contains(report.Inventories["missingIdentities"][^1], firstJson, StringComparison.Ordinal);
        Assert.DoesNotContain(fixture.Root, firstJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("timestamp", firstJson, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("missingIdentities: 3003; sample", console, StringComparison.Ordinal);
        Assert.Contains("missingIdentities omitted: 2991", console, StringComparison.Ordinal);
        Assert.DoesNotContain(report.Inventories["missingIdentities"][^1], console, StringComparison.Ordinal);
        Assert.True(console.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).Length < 30);
    }

    [Fact]
    public void UnknownDeclaredLocaleIsAControlledError()
    {
        using var fixture = InterpretationToolingFixture.CreateSkeleton();

        var report = new InterpretationAuthoringInventoryAnalyzer()
            .Analyze(fixture.Root, "de", InterpretationAuthoringCorpus.SingleCard);

        var diagnostic = Assert.Single(report.Diagnostics);
        Assert.False(report.Success);
        Assert.Equal("authoring.locale-unknown", diagnostic.Code);
        Assert.Equal(InterpretationToolSeverity.Error, diagnostic.Severity);
        Assert.Equal("de", diagnostic.Target);
    }

    private static string[] CanonicalCardIdentities() => StandardTarotCatalog.Deck.Cards
        .Select(card => card.Id.Value)
        .Order(StringComparer.Ordinal)
        .ToArray();
}
