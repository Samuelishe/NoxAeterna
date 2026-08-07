using NoxAeterna.Interpretation.Tarot.Contracts;
using NoxAeterna.Tools.Repository.Interpretation.Analysis;
using NoxAeterna.Tools.Repository.Interpretation.Reports;

namespace NoxAeterna.Tests.Tooling.Interpretation;

public sealed class InterpretationContentAuditorTests
{
    [Fact]
    public void ReportsOneCanonicalExactDuplicateForWhitespaceCaseEquivalentPassages()
    {
        using var fixture = InterpretationToolingFixture.CreateSkeleton();
        var shared = "Сейчас важно спокойно увидеть главную линию происходящего события";
        fixture.AddSingleStates(
            "ru",
            "major.fool",
            State("fool-upright", advice: shared),
            State("fool-reversed", reversed: true));
        fixture.AddSingleStates(
            "ru",
            "major.magician",
            State("magician-upright"),
            State("magician-reversed", advice: "СЕЙЧАС   важно спокойно увидеть главную линию происходящего события", reversed: true));

        var report = Audit(fixture, InterpretationAuthoringCorpus.SingleCard);

        var finding = Assert.Single(report.Diagnostics, item => item.Code == "audit.text.exact-duplicate");
        Assert.Equal("ru/single-card/major.fool/upright:advice", finding.Target);
        Assert.Equal(["ru/single-card/major.magician/reversed:advice"], finding.RelatedTargets);
        Assert.Equal(InterpretationToolSeverity.Warning, finding.Severity);
    }

    [Fact]
    public void NearDuplicateCandidateThresholdIncludesMinorEditAndExcludesDistinctPassage()
    {
        using var fixture = InterpretationToolingFixture.CreateSkeleton();
        var closeA = "первый второй третий четвертый пятый шестой седьмой восьмой девятый десятый одиннадцатый двенадцатый";
        var closeB = "первый второй третий четвертый пятый шестой седьмой восьмой девятый десятый одиннадцатый иной";
        fixture.AddSingleStates("ru", "major.fool", State("fool-up", situation: closeA), State("fool-rev", reversed: true));
        fixture.AddSingleStates("ru", "major.magician", State("mag-up"), State("mag-rev", risk: closeB, reversed: true));
        fixture.AddSingleStates(
            "ru",
            "major.moon",
            State("moon-up", outcome: "река зеркало сумерки берег тропа облако птица свеча камень окно"),
            State("moon-rev", reversed: true));

        var report = Audit(fixture, InterpretationAuthoringCorpus.SingleCard);

        var findings = report.Diagnostics.Where(item => item.Code == "audit.text.near-duplicate").ToArray();
        Assert.Contains(findings, item =>
            item.Target == "ru/single-card/major.fool/upright:situation" &&
            item.RelatedTargets.SequenceEqual(["ru/single-card/major.magician/reversed:risk"]));
        Assert.DoesNotContain(findings, item =>
            item.Target.Contains("major.moon", StringComparison.Ordinal) ||
            item.RelatedTargets.Any(target => target.Contains("major.moon", StringComparison.Ordinal)));
    }

    [Fact]
    public void SingleCardOrientationSimilarityFlagsOnlyCloseAuthoredStates()
    {
        using var fixture = InterpretationToolingFixture.CreateSkeleton();
        fixture.AddSingleStates(
            "ru",
            "major.fool",
            State("shared-orientation"),
            State("shared-orientation", advice: Passage("shared-orientation-changed"), reversed: true));
        fixture.AddSingleStates(
            "ru",
            "major.magician",
            State("independent-light"),
            State("independent-shadow", reversed: true));

        var findings = Audit(fixture, InterpretationAuthoringCorpus.SingleCard).Diagnostics
            .Where(item => item.Code == "audit.single.orientation-similarity")
            .ToArray();

        var finding = Assert.Single(findings);
        Assert.Equal("ru/single-card/major.fool/upright", finding.Target);
        Assert.Equal(["ru/single-card/major.fool/reversed"], finding.RelatedTargets);
    }

    [Fact]
    public void SingleCardSectionSimilarityFlagsNearRepeatedSections()
    {
        using var fixture = InterpretationToolingFixture.CreateSkeleton();
        var situation = "мягкий свет ведет героя через старый сад к новому ясному решению";
        var development = "мягкий свет ведет героя через старый сад к новому ясному выбору";
        fixture.AddSingleStates(
            "ru",
            "major.fool",
            State("fool-up", situation: situation, development: development),
            State("fool-rev", reversed: true));

        var findings = Audit(fixture, InterpretationAuthoringCorpus.SingleCard).Diagnostics
            .Where(item => item.Code == "audit.single.section-similarity")
            .ToArray();

        Assert.Contains(findings, item =>
            item.Target == "ru/single-card/major.fool/upright:situation" &&
            item.RelatedTargets.SequenceEqual(["ru/single-card/major.fool/upright:development"]));
    }

    [Fact]
    public void RepeatedOpeningAdviceEndingAndOutcomeFormulasRequireLongPhraseAcrossDistinctTargets()
    {
        using var fixture = InterpretationToolingFixture.CreateSkeleton();
        var cards = new[] { "major.fool", "major.magician", "major.moon" };
        for (var index = 0; index < cards.Length; index++)
        {
            var opening = $"Сейчас важно не торопиться с главным выбором вариант{index}";
            var outcome = $"Общий исход формируется через ясный выбор сегодня начало{index} итог остается в ваших умелых руках";
            var twoTargetOpening = index < 2
                ? $"Длинная общая формула для двух целей вариант{index}"
                : Passage("third-development");
            fixture.AddSingleStates(
                "ru",
                cards[index],
                State($"formula-{index}-up", development: twoTargetOpening, risk: "Сейчас важно", outcome: outcome, advice: opening),
                State($"formula-{index}-rev", reversed: true));
        }

        var diagnostics = Audit(fixture, InterpretationAuthoringCorpus.SingleCard).Diagnostics;

        Assert.Contains(diagnostics, item => item.Code == "audit.text.repeated-opening" && item.Target.EndsWith(":advice", StringComparison.Ordinal) && item.RelatedTargets.Count == 2);
        Assert.Contains(diagnostics, item => item.Code == "audit.text.repeated-ending" && item.Target.EndsWith(":outcome", StringComparison.Ordinal) && item.RelatedTargets.Count == 2);
        Assert.Contains(diagnostics, item => item.Code == "audit.text.repeated-formula" && item.Target.EndsWith(":advice", StringComparison.Ordinal) && item.RelatedTargets.Count == 2);
        Assert.Contains(diagnostics, item => item.Code == "audit.text.repeated-formula" && item.Target.EndsWith(":outcome", StringComparison.Ordinal) && item.RelatedTargets.Count == 2);
        Assert.DoesNotContain(diagnostics, item =>
            item.Code is "audit.text.repeated-opening" or "audit.text.repeated-ending" or "audit.text.repeated-formula" &&
            (item.Target.EndsWith(":risk", StringComparison.Ordinal) || item.Target.EndsWith(":development", StringComparison.Ordinal) ||
             item.RelatedTargets.Any(target => target.EndsWith(":risk", StringComparison.Ordinal) || target.EndsWith(":development", StringComparison.Ordinal))));
    }

    [Fact]
    public void LengthOutlierUsesSectionFamilyAndStateStatisticsWithDeterministicTarget()
    {
        using var fixture = InterpretationToolingFixture.CreateSkeleton();
        var cards = new[] { "major.chariot", "major.death", "major.devil", "major.emperor", "major.empress", "major.fool" };
        for (var index = 0; index < cards.Length; index++)
        {
            var adviceText = index == cards.Length - 1
                ? string.Join(' ', Enumerable.Range(0, 60).Select(word => $"длинноеслово{word}"))
                : Passage($"normal-advice-{index}");
            fixture.AddSingleStates(
                "ru",
                cards[index],
                State($"length-{index}-up", advice: adviceText),
                State($"length-{index}-rev", reversed: true));
        }

        var report = Audit(fixture, InterpretationAuthoringCorpus.SingleCard);

        Assert.Contains(report.Diagnostics, item =>
            item.Code == "audit.text.length-outlier" &&
            item.Target == "ru/single-card/major.fool/upright:advice");
        var adviceStatistics = report.Statistics["tokens.single-card.section.advice"];
        Assert.Equal(12, adviceStatistics.Count);
        Assert.True(adviceStatistics.Maximum > adviceStatistics.Median * 3);
        Assert.True(report.Statistics.ContainsKey("tokens.state-total"));
    }

    [Fact]
    public void RuLocaleLeakageFlagsLatinProseButIgnoresOneConventionalToken()
    {
        using var fixture = InterpretationToolingFixture.CreateSkeleton();
        fixture.AddSingleStates(
            "ru",
            "major.fool",
            State("russian", situation: "Tarot помогает увидеть спокойный путь через сложную ситуацию без поспешности"),
            State("english", risk: "This passage is clearly written as English prose and should be reviewed by the Russian editor", reversed: true));

        var findings = Audit(fixture, InterpretationAuthoringCorpus.SingleCard).Diagnostics
            .Where(item => item.Code == "audit.locale.leakage")
            .ToArray();

        var finding = Assert.Single(findings);
        Assert.Equal("ru/single-card/major.fool/reversed:risk", finding.Target);
        Assert.DoesNotContain(findings, item => item.Target.EndsWith(":situation", StringComparison.Ordinal));
    }

    [Fact]
    public void MetadataDistributionsAreOrdinalAndCountTagsValenceIntensityAndReversalMechanisms()
    {
        using var fixture = InterpretationToolingFixture.CreateSkeleton();
        fixture.AddVocabulary("ru", "clarity");
        fixture.AddVocabulary("ru", "motion");
        fixture.AddSingleStates(
            "ru",
            "major.fool",
            State("metadata-up", tags: [Tag("clarity", 1), Tag("motion", 2)], valence: 1, intensity: 3),
            State("metadata-rev", tags: [Tag("clarity", -1)], valence: -2, intensity: 1, reversed: true,
                mechanisms: [TarotReversalMechanism.Blocked, TarotReversalMechanism.Delayed]));

        var report = Audit(fixture, InterpretationAuthoringCorpus.SingleCard);

        Assert.Equal(new Dictionary<string, int>(StringComparer.Ordinal) { ["clarity"] = 2, ["motion"] = 1 }, report.Distributions["tagConceptUsage"]);
        Assert.Equal(new Dictionary<string, int>(StringComparer.Ordinal) { ["-2"] = 1, ["1"] = 1 }, report.Distributions["overallValence"]);
        Assert.Equal(new Dictionary<string, int>(StringComparer.Ordinal) { ["1"] = 1, ["3"] = 1 }, report.Distributions["overallIntensity"]);
        Assert.Equal(new Dictionary<string, int>(StringComparer.Ordinal) { ["blocked"] = 1, ["delayed"] = 1 }, report.Distributions["reversalMechanism"]);
        Assert.Equal(report.Distributions.Keys.Order(StringComparer.Ordinal), report.Distributions.Keys);
    }

    [Theory]
    [InlineData(InterpretationAuthoringCorpus.OrientedPairs, "ru/oriented-pairs/major.fool__major.magician/upright-upright:interaction")]
    [InlineData(InterpretationAuthoringCorpus.ThreeCardPositions, "ru/three-card-positions/major.fool/present/reversed:text")]
    public void ExactDuplicateExtractionCoversPairAndThreeCardPositionText(
        InterpretationAuthoringCorpus corpus,
        string expectedRelatedTarget)
    {
        using var fixture = InterpretationToolingFixture.CreateSkeleton();
        var duplicate = "общий образ соединяет разные цели через ясное движение вперед";
        if (corpus == InterpretationAuthoringCorpus.OrientedPairs)
        {
            fixture.AddPairStates("ru", "major.fool", "major.magician", PairStates(duplicate));
        }
        else
        {
            fixture.AddPositionStates("ru", "major.fool", PositionStates(duplicate));
        }

        var finding = Assert.Single(
            Audit(fixture, corpus).Diagnostics,
            item => item.Code == "audit.text.exact-duplicate");

        Assert.Equal(expectedRelatedTarget, Assert.Single(finding.RelatedTargets));
        Assert.True(string.CompareOrdinal(finding.Target, finding.RelatedTargets[0]) < 0);
    }

    [Fact]
    public void NearDuplicateCandidateIndexAvoidsUnconditionalAllPairsComparison()
    {
        using var fixture = InterpretationToolingFixture.CreateSkeleton();
        var cards = new[]
        {
            "major.chariot", "major.death", "major.devil", "major.emperor", "major.empress",
            "major.fool", "major.hanged-man", "major.hermit", "major.hierophant", "major.high-priestess"
        };
        for (var index = 0; index < cards.Length; index++)
        {
            fixture.AddSingleStates(
                "ru",
                cards[index],
                State($"unique-{index}-up"),
                State($"unique-{index}-rev", reversed: true));
        }

        var first = Audit(fixture, InterpretationAuthoringCorpus.SingleCard);
        var second = Audit(fixture, InterpretationAuthoringCorpus.SingleCard);

        Assert.Equal(100, first.Counts["textUnits"]);
        Assert.Equal(4950, first.Counts["possibleComparisons"]);
        Assert.InRange(first.Counts["candidateComparisons"], 0, 4949);
        Assert.Equal(first.Counts["candidateComparisons"], second.Counts["candidateComparisons"]);
        Assert.Contains(first.Diagnostics, item =>
            item.Code == "audit.reversal.dominance" &&
            item.Target == "ru/single-card" &&
            item.Severity == InterpretationToolSeverity.Warning);
        Assert.Equal(10, first.Distributions["reversalMechanism"]["blocked"]);
        Assert.Equal(InterpretationToolReportWriter.WriteJson(first), InterpretationToolReportWriter.WriteJson(second));
    }

    private static InterpretationToolReport Audit(
        InterpretationToolingFixture fixture,
        InterpretationAuthoringCorpus corpus) =>
        new InterpretationContentAuditor().Audit(fixture.Root, "ru", corpus);

    private static TarotSingleCardStateDocument State(
        string seed,
        string? situation = null,
        string? development = null,
        string? risk = null,
        string? outcome = null,
        string? advice = null,
        IReadOnlyList<TarotTagAssignmentDocument>? tags = null,
        int valence = 0,
        int intensity = 2,
        bool reversed = false,
        IReadOnlyList<TarotReversalMechanism>? mechanisms = null) => InterpretationToolingFixture.CreateSingleState(
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["situation"] = situation ?? Passage($"{seed}-situation"),
            ["development"] = development ?? Passage($"{seed}-development"),
            ["risk"] = risk ?? Passage($"{seed}-risk"),
            ["outcome"] = outcome ?? Passage($"{seed}-outcome"),
            ["advice"] = advice ?? Passage($"{seed}-advice")
        },
        tags,
        valence,
        intensity,
        mechanisms ?? (reversed ? [TarotReversalMechanism.Blocked] : []));

    private static string Passage(string seed)
    {
        var numericSeed = string.Concat(seed.Select(character => ((int)character).ToString("D3", System.Globalization.CultureInfo.InvariantCulture)));
        return string.Join(' ', Enumerable.Range(0, 10).Select(index => $"слово{numericSeed}{index}"));
    }

    private static TarotTagAssignmentDocument Tag(string conceptId, int valence) => new()
    {
        ConceptId = conceptId,
        Valence = valence,
        Intensity = 2
    };

    private static IReadOnlyDictionary<string, TarotOrientedPairStateDocument> PairStates(string duplicate) =>
        new Dictionary<string, TarotOrientedPairStateDocument>(StringComparer.Ordinal)
        {
            ["upright-upright"] = PairState("pair-uu", interaction: duplicate),
            ["upright-reversed"] = PairState("pair-ur"),
            ["reversed-upright"] = PairState("pair-ru"),
            ["reversed-reversed"] = PairState("pair-rr", direction: duplicate)
        };

    private static TarotOrientedPairStateDocument PairState(string seed, string? interaction = null, string? direction = null) => new()
    {
        Interaction = interaction ?? Passage($"{seed}-interaction"),
        Direction = direction ?? Passage($"{seed}-direction"),
        Tags = [],
        OverallValence = 0,
        OverallIntensity = 2
    };

    private static IReadOnlyDictionary<string, IReadOnlyDictionary<string, TarotThreeCardPositionStateDocument>> PositionStates(string duplicate) =>
        new Dictionary<string, IReadOnlyDictionary<string, TarotThreeCardPositionStateDocument>>(StringComparer.Ordinal)
        {
            ["past"] = new Dictionary<string, TarotThreeCardPositionStateDocument>(StringComparer.Ordinal)
            {
                ["upright"] = PositionState("past-up", duplicate),
                ["reversed"] = PositionState("past-rev")
            },
            ["present"] = new Dictionary<string, TarotThreeCardPositionStateDocument>(StringComparer.Ordinal)
            {
                ["upright"] = PositionState("present-up"),
                ["reversed"] = PositionState("present-rev", duplicate)
            },
            ["future"] = new Dictionary<string, TarotThreeCardPositionStateDocument>(StringComparer.Ordinal)
            {
                ["upright"] = PositionState("future-up"),
                ["reversed"] = PositionState("future-rev")
            }
        };

    private static TarotThreeCardPositionStateDocument PositionState(string seed, string? text = null) => new()
    {
        Text = text ?? Passage(seed),
        Tags = [],
        OverallValence = 0,
        OverallIntensity = 2
    };
}
