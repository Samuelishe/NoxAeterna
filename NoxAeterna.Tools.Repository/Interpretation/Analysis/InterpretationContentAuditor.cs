using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using NoxAeterna.Interpretation.Tarot.Contracts;
using NoxAeterna.Tools.Repository.Interpretation.Reports;

namespace NoxAeterna.Tools.Repository.Interpretation.Analysis;

/// <summary>Runs deterministic lexical heuristics over structurally valid canonical authoring source.</summary>
public sealed class InterpretationContentAuditor
{
    public const int ExactDuplicateMinimumTokens = 6;
    public const int NearDuplicateMinimumTokens = 8;
    public const int ShingleSize = 3;
    public const double NearDuplicateThreshold = 0.72;
    public const double OrientationSimilarityThreshold = 0.78;
    public const double SectionSimilarityThreshold = 0.78;
    public const int MaximumShinglePostingSize = 64;
    public const int RepeatedPhraseTokens = 5;
    public const int RepeatedFormulaTokens = 6;
    public const int RepeatedPhraseMinimumTargets = 3;

    private static readonly Regex TokenPattern = new(
        @"[\p{L}\p{Nd}]+",
        RegexOptions.CultureInvariant | RegexOptions.NonBacktracking);
    private static readonly string[] SectionIds = ["situation", "development", "risk", "outcome", "advice"];

    public InterpretationToolReport Audit(
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
                    "audit.locale-unknown",
                    InterpretationToolSeverity.Error,
                    localeName,
                    "The locale is not declared by the pack manifest.")]);
        }

        var diagnostics = new List<InterpretationToolDiagnostic>();
        var units = ExtractTextUnits(loaded.Compilation, locale, corpus);
        var metadata = ExtractMetadata(loaded.Compilation, locale, corpus);
        if (units.Count == 0)
        {
            diagnostics.Add(new(
                "audit.empty-corpus",
                InterpretationToolSeverity.Warning,
                $"{locale.Value}/{InterpretationAuthoringCorpusNames.Get(corpus)}",
                "The selected structurally valid corpus contains no authored text units."));
        }

        FindExactDuplicates(units, diagnostics);
        var candidateComparisons = FindNearDuplicates(units, diagnostics, out var possibleComparisons);
        if (corpus == InterpretationAuthoringCorpus.SingleCard)
        {
            FindSingleCardSimilarity(loaded.Compilation.SingleCards[locale], locale.Value, diagnostics);
        }
        FindRepeatedPhrases(units, diagnostics);
        var statistics = FindLengthOutliers(units, diagnostics);
        FindLocaleLeakage(units, locale.Value, diagnostics);
        FindReversalDominance(metadata.ReversalMechanisms, locale.Value, corpus, diagnostics);

        var counts = new Dictionary<string, int>(StringComparer.Ordinal)
        {
            ["candidateComparisons"] = candidateComparisons,
            ["possibleComparisons"] = possibleComparisons,
            ["textUnits"] = units.Count
        };
        var details = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["corpus"] = InterpretationAuthoringCorpusNames.Get(corpus),
            ["locale"] = locale.Value,
            ["nearDuplicateMetric"] = $"word-{ShingleSize}-shingle Jaccard >= {NearDuplicateThreshold.ToString("0.00", CultureInfo.InvariantCulture)}",
            ["nearDuplicateCandidates"] = $"shared shingle with posting size <= {MaximumShinglePostingSize}"
        };
        var distributions = new Dictionary<string, IReadOnlyDictionary<string, int>>(StringComparer.Ordinal)
        {
            ["overallIntensity"] = metadata.OverallIntensity,
            ["overallValence"] = metadata.OverallValence,
            ["reversalMechanism"] = metadata.ReversalMechanisms,
            ["tagConceptUsage"] = metadata.Tags
        };
        return new InterpretationToolReport(
            diagnostics,
            counts,
            details,
            distributions: distributions,
            statistics: statistics);
    }

    private static IReadOnlyList<AuditTextUnit> ExtractTextUnits(
        TarotInterpretationCompilation compilation,
        TarotInterpretationLocale locale,
        InterpretationAuthoringCorpus corpus)
    {
        var result = new List<AuditTextUnit>();
        switch (corpus)
        {
            case InterpretationAuthoringCorpus.SingleCard:
                foreach (var state in compilation.SingleCards[locale]
                             .OrderBy(item => item.CardId.Value, StringComparer.Ordinal)
                             .ThenBy(item => OrientationName(item.Orientation), StringComparer.Ordinal))
                {
                    var stateTarget = $"{locale.Value}/single-card/{state.CardId.Value}/{OrientationName(state.Orientation)}";
                    foreach (var sectionId in SectionIds)
                    {
                        result.Add(CreateUnit(
                            $"{stateTarget}:{sectionId}",
                            $"single-card.section.{sectionId}",
                            stateTarget,
                            sectionId,
                            state.Sections[sectionId]));
                    }
                }
                break;
            case InterpretationAuthoringCorpus.OrientedPairs:
                foreach (var state in compilation.OrientedPairs[locale]
                             .OrderBy(item => item.CardAId.Value, StringComparer.Ordinal)
                             .ThenBy(item => item.CardBId.Value, StringComparer.Ordinal)
                             .ThenBy(item => PairStateName(item.OrientationState), StringComparer.Ordinal))
                {
                    var identity = $"{state.CardAId.Value}__{state.CardBId.Value}";
                    var stateTarget = $"{locale.Value}/oriented-pairs/{identity}/{PairStateName(state.OrientationState)}";
                    result.Add(CreateUnit($"{stateTarget}:interaction", "oriented-pairs.interaction", stateTarget, "interaction", state.Interaction));
                    result.Add(CreateUnit($"{stateTarget}:direction", "oriented-pairs.direction", stateTarget, "direction", state.Direction));
                }
                break;
            case InterpretationAuthoringCorpus.ThreeCardPositions:
                foreach (var state in compilation.ThreeCardPositions[locale]
                             .OrderBy(item => item.CardId.Value, StringComparer.Ordinal)
                             .ThenBy(item => PositionName(item.Position), StringComparer.Ordinal)
                             .ThenBy(item => OrientationName(item.Orientation), StringComparer.Ordinal))
                {
                    var stateTarget = $"{locale.Value}/three-card-positions/{state.CardId.Value}/{PositionName(state.Position)}/{OrientationName(state.Orientation)}";
                    result.Add(CreateUnit($"{stateTarget}:text", $"three-card-positions.{PositionName(state.Position)}", stateTarget, "text", state.Text));
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(corpus));
        }
        return result;
    }

    private static AuditTextUnit CreateUnit(string target, string family, string stateTarget, string field, string text)
    {
        var tokens = Tokenize(text);
        return new(target, family, stateTarget, field, text, tokens, Shingles(tokens));
    }

    private static void FindExactDuplicates(
        IReadOnlyList<AuditTextUnit> units,
        ICollection<InterpretationToolDiagnostic> diagnostics)
    {
        foreach (var group in units
                     .Where(unit => unit.Tokens.Count >= ExactDuplicateMinimumTokens)
                     .GroupBy(unit => string.Join('\u001f', unit.Tokens), StringComparer.Ordinal)
                     .Where(group => group.Select(unit => unit.Target).Distinct(StringComparer.Ordinal).Count() > 1)
                     .OrderBy(group => group.Key, StringComparer.Ordinal))
        {
            var targets = group.Select(unit => unit.Target).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
            diagnostics.Add(new(
                "audit.text.exact-duplicate",
                InterpretationToolSeverity.Warning,
                targets[0],
                $"The same normalized passage occurs in {targets.Length} semantic targets.",
                targets.Skip(1)));
        }
    }

    private static int FindNearDuplicates(
        IReadOnlyList<AuditTextUnit> units,
        ICollection<InterpretationToolDiagnostic> diagnostics,
        out int possibleComparisons)
    {
        var eligible = units.Where(unit => unit.Tokens.Count >= NearDuplicateMinimumTokens && unit.Shingles.Count > 0).ToArray();
        possibleComparisons = checked(eligible.Length * (eligible.Length - 1) / 2);
        var inverted = new Dictionary<string, List<int>>(StringComparer.Ordinal);
        for (var index = 0; index < eligible.Length; index++)
        {
            foreach (var shingle in eligible[index].Shingles)
            {
                if (!inverted.TryGetValue(shingle, out var posting))
                {
                    posting = [];
                    inverted.Add(shingle, posting);
                }
                posting.Add(index);
            }
        }

        var candidates = new SortedSet<long>();
        foreach (var posting in inverted.OrderBy(pair => pair.Key, StringComparer.Ordinal).Select(pair => pair.Value))
        {
            if (posting.Count is < 2 or > MaximumShinglePostingSize)
            {
                continue;
            }
            for (var left = 0; left < posting.Count - 1; left++)
            for (var right = left + 1; right < posting.Count; right++)
            {
                candidates.Add(((long)posting[left] << 32) | (uint)posting[right]);
            }
        }

        foreach (var candidate in candidates)
        {
            var left = eligible[(int)(candidate >> 32)];
            var right = eligible[(int)candidate];
            if (left.NormalizedText == right.NormalizedText)
            {
                continue;
            }
            var similarity = Jaccard(left.Shingles, right.Shingles);
            if (similarity < NearDuplicateThreshold)
            {
                continue;
            }
            var targets = new[] { left.Target, right.Target }.Order(StringComparer.Ordinal).ToArray();
            diagnostics.Add(new(
                "audit.text.near-duplicate",
                InterpretationToolSeverity.Warning,
                targets[0],
                $"Word-{ShingleSize}-shingle Jaccard similarity is {similarity.ToString("0.000", CultureInfo.InvariantCulture)}.",
                [targets[1]]));
        }
        return candidates.Count;
    }

    private static void FindSingleCardSimilarity(
        IReadOnlyList<TarotSingleCardEntry> entries,
        string locale,
        ICollection<InterpretationToolDiagnostic> diagnostics)
    {
        foreach (var card in entries.GroupBy(item => item.CardId.Value, StringComparer.Ordinal).OrderBy(group => group.Key, StringComparer.Ordinal))
        {
            var upright = card.SingleOrDefault(item => OrientationName(item.Orientation) == "upright");
            var reversed = card.SingleOrDefault(item => OrientationName(item.Orientation) == "reversed");
            if (upright is not null && reversed is not null)
            {
                var uprightTarget = $"{locale}/single-card/{card.Key}/upright";
                var reversedTarget = $"{locale}/single-card/{card.Key}/reversed";
                var similarity = SectionIds
                    .Select(sectionId => TextSimilarity(
                        Tokenize(upright.Sections[sectionId]),
                        Tokenize(reversed.Sections[sectionId])))
                    .Average();
                if (similarity >= OrientationSimilarityThreshold)
                {
                    diagnostics.Add(new(
                        "audit.single.orientation-similarity",
                        InterpretationToolSeverity.Warning,
                        uprightTarget,
                        $"Mean aligned-section word-{ShingleSize}-shingle Jaccard similarity is {similarity.ToString("0.000", CultureInfo.InvariantCulture)}.",
                        [reversedTarget]));
                }
            }

            foreach (var state in card.OrderBy(item => OrientationName(item.Orientation), StringComparer.Ordinal))
            {
                for (var left = 0; left < SectionIds.Length - 1; left++)
                for (var right = left + 1; right < SectionIds.Length; right++)
                {
                    var leftTokens = Tokenize(state.Sections[SectionIds[left]]);
                    var rightTokens = Tokenize(state.Sections[SectionIds[right]]);
                    if (leftTokens.Count < NearDuplicateMinimumTokens || rightTokens.Count < NearDuplicateMinimumTokens)
                    {
                        continue;
                    }
                    var similarity = TextSimilarity(leftTokens, rightTokens);
                    if (similarity < SectionSimilarityThreshold)
                    {
                        continue;
                    }
                    var prefix = $"{locale}/single-card/{card.Key}/{OrientationName(state.Orientation)}";
                    diagnostics.Add(new(
                        "audit.single.section-similarity",
                        InterpretationToolSeverity.Warning,
                        $"{prefix}:{SectionIds[left]}",
                        $"Sections have word-{ShingleSize}-shingle Jaccard similarity {similarity.ToString("0.000", CultureInfo.InvariantCulture)}.",
                        [$"{prefix}:{SectionIds[right]}"]));
                }
            }
        }
    }

    private static void FindRepeatedPhrases(
        IReadOnlyList<AuditTextUnit> units,
        ICollection<InterpretationToolDiagnostic> diagnostics)
    {
        AddRepeatedPhraseFindings(
            units.Where(unit => unit.Tokens.Count >= RepeatedPhraseTokens),
            unit => string.Join(' ', unit.Tokens.Take(RepeatedPhraseTokens)),
            "audit.text.repeated-opening",
            diagnostics);
        AddRepeatedPhraseFindings(
            units.Where(unit => unit.Tokens.Count >= RepeatedPhraseTokens),
            unit => string.Join(' ', unit.Tokens.TakeLast(RepeatedPhraseTokens)),
            "audit.text.repeated-ending",
            diagnostics);
        AddRepeatedPhraseFindings(
            units.Where(unit => unit.Field is "advice" or "outcome" && unit.Tokens.Count >= RepeatedFormulaTokens),
            unit => string.Join(' ', unit.Tokens.Take(RepeatedFormulaTokens)),
            "audit.text.repeated-formula",
            diagnostics);
    }

    private static void AddRepeatedPhraseFindings(
        IEnumerable<AuditTextUnit> units,
        Func<AuditTextUnit, string> phraseSelector,
        string code,
        ICollection<InterpretationToolDiagnostic> diagnostics)
    {
        foreach (var group in units
                     .GroupBy(phraseSelector, StringComparer.Ordinal)
                     .Select(group => new
                     {
                         Phrase = group.Key,
                         Units = group.GroupBy(unit => unit.StateTarget, StringComparer.Ordinal)
                             .Select(states => states.OrderBy(unit => unit.Target, StringComparer.Ordinal).First())
                             .OrderBy(unit => unit.Target, StringComparer.Ordinal)
                             .ToArray()
                     })
                     .Where(group => group.Units.Length >= RepeatedPhraseMinimumTargets)
                     .OrderBy(group => group.Phrase, StringComparer.Ordinal))
        {
            diagnostics.Add(new(
                code,
                InterpretationToolSeverity.Warning,
                group.Units[0].Target,
                $"Normalized phrase '{group.Phrase}' occurs in {group.Units.Length} distinct semantic states.",
                group.Units.Skip(1).Select(unit => unit.Target)));
        }
    }

    private static IReadOnlyDictionary<string, InterpretationTextStatistics> FindLengthOutliers(
        IReadOnlyList<AuditTextUnit> units,
        ICollection<InterpretationToolDiagnostic> diagnostics)
    {
        var result = new Dictionary<string, InterpretationTextStatistics>(StringComparer.Ordinal);
        foreach (var family in units.GroupBy(unit => unit.Family, StringComparer.Ordinal).OrderBy(group => group.Key, StringComparer.Ordinal))
        {
            AddLengthFamily($"tokens.{family.Key}", family.Select(unit => (unit.Target, unit.Tokens.Count)).ToArray(), result, diagnostics);
        }
        AddLengthFamily(
            "tokens.state-total",
            units.GroupBy(unit => unit.StateTarget, StringComparer.Ordinal)
                .Select(group => (group.Key, group.Sum(unit => unit.Tokens.Count)))
                .ToArray(),
            result,
            diagnostics);
        return result;
    }

    private static void AddLengthFamily(
        string name,
        IReadOnlyList<(string Target, int Length)> values,
        IDictionary<string, InterpretationTextStatistics> statistics,
        ICollection<InterpretationToolDiagnostic> diagnostics)
    {
        if (values.Count == 0)
        {
            return;
        }
        var ordered = values.Select(item => item.Length).Order().ToArray();
        var q1 = Quantile(ordered, 1);
        var median = Quantile(ordered, 2);
        var q3 = Quantile(ordered, 3);
        statistics[name] = new(ordered.Length, ordered[0], q1, median, q3, ordered[^1]);
        if (values.Count < 5)
        {
            return;
        }
        var iqr = q3 - q1;
        var lower = iqr == 0 ? q1 / 3 : Math.Max(0, q1 - 3 * iqr);
        var upper = iqr == 0 ? Math.Max(q3 + 1, q3 * 3) : q3 + 3 * iqr;
        foreach (var value in values.Where(item => item.Length < lower || item.Length > upper).OrderBy(item => item.Target, StringComparer.Ordinal))
        {
            diagnostics.Add(new(
                "audit.text.length-outlier",
                InterpretationToolSeverity.Warning,
                value.Target,
                $"Token length {value.Length} is outside the robust {name} range {lower}..{upper} (median {median})."));
        }
    }

    private static void FindLocaleLeakage(
        IEnumerable<AuditTextUnit> units,
        string locale,
        ICollection<InterpretationToolDiagnostic> diagnostics)
    {
        if (!locale.Equals("ru", StringComparison.Ordinal) && !locale.StartsWith("ru-", StringComparison.Ordinal))
        {
            return;
        }
        foreach (var unit in units)
        {
            var letters = unit.Text.Count(char.IsLetter);
            var latin = unit.Text.Count(character => character is >= 'A' and <= 'Z' or >= 'a' and <= 'z');
            if (letters < 20 || latin < 15 || latin * 100 < letters * 45)
            {
                continue;
            }
            diagnostics.Add(new(
                "audit.locale.leakage",
                InterpretationToolSeverity.Warning,
                unit.Target,
                $"Latin-script letters account for {latin * 100 / letters}% of visible prose letters."));
        }
    }

    private static AuditMetadata ExtractMetadata(
        TarotInterpretationCompilation compilation,
        TarotInterpretationLocale locale,
        InterpretationAuthoringCorpus corpus)
    {
        var tags = new Dictionary<string, int>(StringComparer.Ordinal);
        var valence = new Dictionary<string, int>(StringComparer.Ordinal);
        var intensity = new Dictionary<string, int>(StringComparer.Ordinal);
        var reversal = new Dictionary<string, int>(StringComparer.Ordinal);
        IEnumerable<(IReadOnlyList<TarotTagAssignment> Tags, int Valence, int Intensity)> entries;
        switch (corpus)
        {
            case InterpretationAuthoringCorpus.SingleCard:
                var singles = compilation.SingleCards[locale];
                entries = singles.Select(item => (item.Tags, item.OverallValence, item.OverallIntensity));
                foreach (var mechanism in singles.SelectMany(item => item.ReversalMechanisms)) Increment(reversal, ReversalName(mechanism));
                break;
            case InterpretationAuthoringCorpus.OrientedPairs:
                entries = compilation.OrientedPairs[locale].Select(item => (item.Tags, item.OverallValence, item.OverallIntensity));
                break;
            case InterpretationAuthoringCorpus.ThreeCardPositions:
                entries = compilation.ThreeCardPositions[locale].Select(item => (item.Tags, item.OverallValence, item.OverallIntensity));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(corpus));
        }
        foreach (var entry in entries)
        {
            Increment(valence, entry.Valence.ToString(CultureInfo.InvariantCulture));
            Increment(intensity, entry.Intensity.ToString(CultureInfo.InvariantCulture));
            foreach (var tag in entry.Tags) Increment(tags, tag.ConceptId.Value);
        }
        return new(tags, valence, intensity, reversal);
    }

    private static void FindReversalDominance(
        IReadOnlyDictionary<string, int> distribution,
        string locale,
        InterpretationAuthoringCorpus corpus,
        ICollection<InterpretationToolDiagnostic> diagnostics)
    {
        var total = distribution.Values.Sum();
        if (corpus != InterpretationAuthoringCorpus.SingleCard || total < 10 || distribution.Count == 0)
        {
            return;
        }
        var dominant = distribution.OrderByDescending(pair => pair.Value).ThenBy(pair => pair.Key, StringComparer.Ordinal).First();
        if (dominant.Value * 100 < total * 80)
        {
            return;
        }
        diagnostics.Add(new(
            "audit.reversal.dominance",
            InterpretationToolSeverity.Warning,
            $"{locale}/single-card",
            $"Reversal mechanism '{dominant.Key}' accounts for {dominant.Value} of {total} mechanism assignments."));
    }

    private static double TextSimilarity(IReadOnlyList<string> left, IReadOnlyList<string> right) =>
        Jaccard(Shingles(left), Shingles(right));

    private static double Jaccard(IReadOnlySet<string> left, IReadOnlySet<string> right)
    {
        if (left.Count == 0 || right.Count == 0)
        {
            return 0;
        }
        var intersection = left.Count <= right.Count ? left.Count(right.Contains) : right.Count(left.Contains);
        return intersection / (double)(left.Count + right.Count - intersection);
    }

    private static IReadOnlyList<string> Tokenize(string text) => TokenPattern.Matches(text
            .Normalize(NormalizationForm.FormKC)
            .ToLowerInvariant())
        .Select(match => match.Value)
        .ToArray();

    private static IReadOnlySet<string> Shingles(IReadOnlyList<string> tokens)
    {
        var result = new SortedSet<string>(StringComparer.Ordinal);
        for (var index = 0; index <= tokens.Count - ShingleSize; index++)
        {
            result.Add(string.Join('\u001f', tokens.Skip(index).Take(ShingleSize)));
        }
        return result;
    }

    private static int Quantile(IReadOnlyList<int> ordered, int quarter) => ordered[(ordered.Count - 1) * quarter / 4];
    private static void Increment(IDictionary<string, int> values, string key) =>
        values[key] = values.TryGetValue(key, out var count) ? count + 1 : 1;
    private static string OrientationName(NoxAeterna.Domain.Tarot.TarotCardOrientation value) => value == NoxAeterna.Domain.Tarot.TarotCardOrientation.Upright ? "upright" : "reversed";
    private static string PositionName(TarotThreeCardPosition value) => value switch { TarotThreeCardPosition.Past => "past", TarotThreeCardPosition.Present => "present", TarotThreeCardPosition.Future => "future", _ => throw new ArgumentOutOfRangeException(nameof(value)) };
    private static string PairStateName(TarotOrientedPairState value) => value switch { TarotOrientedPairState.UprightUpright => "upright-upright", TarotOrientedPairState.UprightReversed => "upright-reversed", TarotOrientedPairState.ReversedUpright => "reversed-upright", TarotOrientedPairState.ReversedReversed => "reversed-reversed", _ => throw new ArgumentOutOfRangeException(nameof(value)) };
    private static string ReversalName(TarotReversalMechanism value) => value switch { TarotReversalMechanism.Blocked => "blocked", TarotReversalMechanism.Delayed => "delayed", TarotReversalMechanism.Internalized => "internalized", TarotReversalMechanism.Excessive => "excessive", TarotReversalMechanism.Distorted => "distorted", TarotReversalMechanism.Resisted => "resisted", TarotReversalMechanism.Depleted => "depleted", _ => throw new ArgumentOutOfRangeException(nameof(value)) };

    private sealed record AuditTextUnit(
        string Target,
        string Family,
        string StateTarget,
        string Field,
        string Text,
        IReadOnlyList<string> Tokens,
        IReadOnlySet<string> Shingles)
    {
        public string NormalizedText => string.Join('\u001f', Tokens);
    }

    private sealed record AuditMetadata(
        IReadOnlyDictionary<string, int> Tags,
        IReadOnlyDictionary<string, int> OverallValence,
        IReadOnlyDictionary<string, int> OverallIntensity,
        IReadOnlyDictionary<string, int> ReversalMechanisms);
}
