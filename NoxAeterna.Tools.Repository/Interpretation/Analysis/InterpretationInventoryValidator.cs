using NoxAeterna.Domain.Tarot;
using NoxAeterna.Interpretation.Tarot.Contracts;
using NoxAeterna.Interpretation.Tarot.Keys;
using NoxAeterna.Tools.Repository.Interpretation.Reports;

namespace NoxAeterna.Tools.Repository.Interpretation.Analysis;

public static class InterpretationInventoryValidator
{
    public static InterpretationToolReport ValidateSingleCard(IEnumerable<string> keys) =>
        ValidateSingleCard(keys, StandardTarotCatalog.Deck);

    public static InterpretationToolReport ValidateSingleCard(
        IEnumerable<string> keys,
        TarotDeckDefinition deck)
    {
        var diagnostics = new InterpretationDiagnosticBag();
        var parsed = ParseUnique(keys, TarotInterpretationKeys.ParseSingleCard, diagnostics);
        var expected = deck.Cards
            .SelectMany(card => Enum.GetValues<TarotCardOrientation>()
                .Select(orientation => TarotInterpretationKeys.CreateSingleCard(card.Id, orientation)))
            .Order(StringComparer.Ordinal)
            .ToArray();
        CompareExpected(parsed, expected, "inventory.single", diagnostics);
        return Report(diagnostics, new Dictionary<string, int>
        {
            ["singleCardEntries"] = parsed.Count
        });
    }

    public static InterpretationToolReport ValidateOrientedPairs(IEnumerable<string> keys) =>
        ValidateOrientedPairs(keys, StandardTarotCatalog.Deck);

    public static InterpretationToolReport ValidateOrientedPairs(
        IEnumerable<string> keys,
        TarotDeckDefinition deck)
    {
        var diagnostics = new InterpretationDiagnosticBag();
        var parsed = ParseUnique(keys, TarotInterpretationKeys.ParseOrientedPair, diagnostics);
        var cards = deck.Cards.Select(card => card.Id).OrderBy(id => id.Value, StringComparer.Ordinal).ToArray();
        var expected = new List<string>(12012);
        for (var first = 0; first < cards.Length; first++)
        {
            for (var second = first + 1; second < cards.Length; second++)
            {
                foreach (var state in Enum.GetValues<TarotOrientedPairState>())
                {
                    expected.Add(TarotInterpretationKeys.CreateOrientedPair(cards[first], cards[second], state));
                }
            }
        }

        expected.Sort(StringComparer.Ordinal);
        CompareExpected(parsed, expected, "inventory.pairs", diagnostics);
        var identities = parsed.Select(key => key.Split('|')[0]).Distinct(StringComparer.Ordinal).Count();
        if (identities != 3003)
        {
            diagnostics.Error("inventory.pair-identities", "oriented-pairs", $"Expected 3003 identities, found {identities}.");
        }

        return Report(diagnostics, new Dictionary<string, int>
        {
            ["orientedPairIdentities"] = identities,
            ["orientedPairStates"] = parsed.Count
        });
    }

    public static InterpretationToolReport ValidateThreeCardPositions(IEnumerable<string> keys) =>
        ValidateThreeCardPositions(keys, StandardTarotCatalog.Deck);

    public static InterpretationToolReport ValidateThreeCardPositions(
        IEnumerable<string> keys,
        TarotDeckDefinition deck)
    {
        var diagnostics = new InterpretationDiagnosticBag();
        var parsed = ParseUnique(keys, TarotInterpretationKeys.ParseThreeCardPosition, diagnostics);
        var expected = deck.Cards
            .SelectMany(card => Enum.GetValues<TarotThreeCardPosition>()
                .SelectMany(position => Enum.GetValues<TarotCardOrientation>()
                    .Select(orientation => TarotInterpretationKeys.CreateThreeCardPosition(position, card.Id, orientation))))
            .Order(StringComparer.Ordinal)
            .ToArray();
        CompareExpected(parsed, expected, "inventory.positions", diagnostics);
        return Report(diagnostics, new Dictionary<string, int>
        {
            ["threeCardPositions"] = parsed.Count
        });
    }

    private static List<string> ParseUnique<T>(
        IEnumerable<string> keys,
        Func<string, NoxAeterna.Interpretation.Tarot.Validation.TarotValidationResult<T>> parser,
        InterpretationDiagnosticBag diagnostics)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(keys);
        var result = new List<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var key in keys)
        {
            if (!seen.Add(key))
            {
                diagnostics.Error("inventory.duplicate", key, "Inventory contains a duplicate key.");
                continue;
            }

            var parsed = parser(key);
            if (!parsed.IsValid)
            {
                diagnostics.Error("inventory.key", key, "Inventory key is malformed or noncanonical.");
                continue;
            }

            result.Add(key);
        }

        result.Sort(StringComparer.Ordinal);
        return result;
    }

    private static void CompareExpected(
        IReadOnlyCollection<string> actual,
        IReadOnlyCollection<string> expected,
        string code,
        InterpretationDiagnosticBag diagnostics)
    {
        var actualSet = actual.ToHashSet(StringComparer.Ordinal);
        var expectedSet = expected.ToHashSet(StringComparer.Ordinal);
        foreach (var missing in expectedSet.Except(actualSet, StringComparer.Ordinal).Order(StringComparer.Ordinal).Take(20))
        {
            diagnostics.Error($"{code}.missing", missing, "Required inventory entry is missing.");
        }

        foreach (var extra in actualSet.Except(expectedSet, StringComparer.Ordinal).Order(StringComparer.Ordinal).Take(20))
        {
            diagnostics.Error($"{code}.extra", extra, "Inventory contains an unexpected entry.");
        }

        if (actual.Count != expected.Count)
        {
            diagnostics.Error($"{code}.count", code, $"Expected {expected.Count} entries, found {actual.Count}.");
        }
    }

    private static InterpretationToolReport Report(
        InterpretationDiagnosticBag diagnostics,
        IReadOnlyDictionary<string, int> counts) => new(diagnostics.Items, counts);
}
