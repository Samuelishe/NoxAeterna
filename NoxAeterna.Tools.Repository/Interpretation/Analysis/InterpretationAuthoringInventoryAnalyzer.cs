using NoxAeterna.Domain.Tarot;
using NoxAeterna.Interpretation.Tarot.Contracts;
using NoxAeterna.Interpretation.Tarot.Serialization;
using NoxAeterna.Tools.Repository.Interpretation.Reports;

namespace NoxAeterna.Tools.Repository.Interpretation.Analysis;

/// <summary>Identifies one canonical authoring corpus exposed by repository tooling.</summary>
public enum InterpretationAuthoringCorpus
{
    SingleCard,
    OrientedPairs,
    ThreeCardPositions,
    ThreeCardSynthesis
}

public static class InterpretationAuthoringCorpusNames
{
    public static bool TryParse(string value, out InterpretationAuthoringCorpus corpus)
    {
        corpus = value switch
        {
            "single-card" => InterpretationAuthoringCorpus.SingleCard,
            "oriented-pairs" => InterpretationAuthoringCorpus.OrientedPairs,
            "three-card-positions" => InterpretationAuthoringCorpus.ThreeCardPositions,
            "three-card-synthesis" => InterpretationAuthoringCorpus.ThreeCardSynthesis,
            _ => default
        };
        return value is "single-card" or "oriented-pairs" or "three-card-positions" or "three-card-synthesis";
    }

    public static string Get(InterpretationAuthoringCorpus corpus) => corpus switch
    {
        InterpretationAuthoringCorpus.SingleCard => "single-card",
        InterpretationAuthoringCorpus.OrientedPairs => "oriented-pairs",
        InterpretationAuthoringCorpus.ThreeCardPositions => "three-card-positions",
        InterpretationAuthoringCorpus.ThreeCardSynthesis => "three-card-synthesis",
        _ => throw new ArgumentOutOfRangeException(nameof(corpus))
    };
}

/// <summary>Builds a locale/corpus-scoped inventory from validated canonical authoring source.</summary>
public sealed class InterpretationAuthoringInventoryAnalyzer
{
    private static readonly string[] CardIdentities = StandardTarotCatalog.Deck.Cards
        .Select(card => card.Id.Value)
        .Order(StringComparer.Ordinal)
        .ToArray();

    private static readonly string[] PairIdentities = CardIdentities
        .SelectMany((cardA, index) => CardIdentities
            .Skip(index + 1)
            .Select(cardB => $"{cardA}__{cardB}"))
        .ToArray();

    public InterpretationToolReport Analyze(
        string sourceRoot,
        string localeName,
        InterpretationAuthoringCorpus corpus)
    {
        var loaded = new InterpretationSourceLoader().Load(sourceRoot);
        if (!loaded.Report.Success || loaded.Compilation is null)
        {
            return loaded.Report;
        }

        var locale = loaded.Compilation.Manifest.DeclaredLocales
            .SingleOrDefault(item => string.Equals(item.Value, localeName, StringComparison.Ordinal));
        if (locale is null)
        {
            return new InterpretationToolReport(
                [new InterpretationToolDiagnostic(
                    "authoring.locale-unknown",
                    InterpretationToolSeverity.Error,
                    localeName,
                    "The locale is not declared by the pack manifest.")]);
        }

        if (corpus == InterpretationAuthoringCorpus.ThreeCardSynthesis)
        {
            return AnalyzeSynthesis(loaded, locale);
        }

        var (expected, statesPerBundle, present, presentStates) = corpus switch
        {
            InterpretationAuthoringCorpus.SingleCard => (
                CardIdentities,
                2,
                loaded.Compilation.SingleCards[locale]
                    .Select(item => item.CardId.Value)
                    .Distinct(StringComparer.Ordinal)
                    .ToHashSet(StringComparer.Ordinal),
                loaded.Compilation.SingleCards[locale].Count),
            InterpretationAuthoringCorpus.OrientedPairs => (
                PairIdentities,
                4,
                loaded.Compilation.OrientedPairs[locale]
                    .Select(item => $"{item.CardAId.Value}__{item.CardBId.Value}")
                    .Distinct(StringComparer.Ordinal)
                    .ToHashSet(StringComparer.Ordinal),
                loaded.Compilation.OrientedPairs[locale].Count),
            InterpretationAuthoringCorpus.ThreeCardPositions => (
                CardIdentities,
                6,
                loaded.Compilation.ThreeCardPositions[locale]
                    .Select(item => item.CardId.Value)
                    .Distinct(StringComparer.Ordinal)
                    .ToHashSet(StringComparer.Ordinal),
                loaded.Compilation.ThreeCardPositions[locale].Count),
            _ => throw new ArgumentOutOfRangeException(nameof(corpus))
        };

        var missing = expected.Where(identity => !present.Contains(identity)).ToArray();
        var counts = new Dictionary<string, int>(StringComparer.Ordinal)
        {
            ["duplicateIdentities"] = loaded.Report.Counts.GetValueOrDefault("duplicateIdentities"),
            ["expectedBundles"] = expected.Length,
            ["expectedStates"] = expected.Length * statesPerBundle,
            ["invalidBundles"] = loaded.Report.Counts.GetValueOrDefault("invalidBundles"),
            ["missingBundles"] = missing.Length,
            ["missingStates"] = expected.Length * statesPerBundle - presentStates,
            ["noncanonicalIdentities"] = loaded.Report.Counts.GetValueOrDefault("noncanonicalIdentities"),
            ["presentBundles"] = present.Count,
            ["presentStates"] = presentStates
        };
        var details = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["corpus"] = InterpretationAuthoringCorpusNames.Get(corpus),
            ["locale"] = locale.Value
        };
        var inventories = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal)
        {
            ["missingIdentities"] = missing
        };
        return new InterpretationToolReport(loaded.Report.Diagnostics, counts, details, inventories: inventories);
    }

    private static InterpretationToolReport AnalyzeSynthesis(
        InterpretationSourceCompilationResult loaded,
        TarotInterpretationLocale locale)
    {
        var expected = TarotThreeCardSynthesisContract.RequiredResources
            .Select(Identity)
            .ToArray();
        var present = loaded.Compilation!.SynthesisResources[locale]
            .Select(item => Identity(new(item.ResourceType, item.ResourceId)))
            .ToHashSet(StringComparer.Ordinal);
        var missing = expected.Where(identity => !present.Contains(identity)).ToArray();
        var counts = new Dictionary<string, int>(StringComparer.Ordinal)
        {
            ["duplicateIdentities"] = loaded.Report.Counts.GetValueOrDefault("duplicateIdentities"),
            ["expectedResources"] = expected.Length,
            ["invalidResources"] = loaded.Report.Counts.GetValueOrDefault("invalidBundles"),
            ["missingResources"] = missing.Length,
            ["noncanonicalIdentities"] = loaded.Report.Counts.GetValueOrDefault("noncanonicalIdentities"),
            ["presentResources"] = present.Count
        };
        var details = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["corpus"] = InterpretationAuthoringCorpusNames.Get(InterpretationAuthoringCorpus.ThreeCardSynthesis),
            ["locale"] = locale.Value
        };
        var inventories = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal)
        {
            ["missingIdentities"] = missing
        };
        return new InterpretationToolReport(loaded.Report.Diagnostics, counts, details, inventories: inventories);
    }

    private static string Identity(TarotSynthesisResourceIdentity identity) =>
        $"{TarotSchemaText.Get(identity.ResourceType, TarotSchemaText.SynthesisResourceTypes)}/{identity.ResourceId.Value}";
}
