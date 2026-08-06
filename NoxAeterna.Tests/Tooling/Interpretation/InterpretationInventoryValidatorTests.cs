using NoxAeterna.Domain.Tarot;
using NoxAeterna.Interpretation.Tarot.Contracts;
using NoxAeterna.Interpretation.Tarot.Keys;
using NoxAeterna.Tools.Repository.Interpretation.Analysis;

namespace NoxAeterna.Tests.Tooling.Interpretation;

public sealed class InterpretationInventoryValidatorTests
{
    [Fact]
    public void FrozenStandardDeckInventoriesHaveExactAcceptedCounts()
    {
        var cards = StandardTarotCatalog.Deck.Cards.Select(card => card.Id).ToArray();
        var singles = cards.SelectMany(card => Enum.GetValues<TarotCardOrientation>()
            .Select(orientation => TarotInterpretationKeys.CreateSingleCard(card, orientation)));
        var positions = cards.SelectMany(card => Enum.GetValues<TarotThreeCardPosition>()
            .SelectMany(position => Enum.GetValues<TarotCardOrientation>()
                .Select(orientation => TarotInterpretationKeys.CreateThreeCardPosition(position, card, orientation))));
        var pairs = CompletePairKeys(cards);

        var singleReport = InterpretationInventoryValidator.ValidateSingleCard(singles);
        var pairReport = InterpretationInventoryValidator.ValidateOrientedPairs(pairs);
        var positionReport = InterpretationInventoryValidator.ValidateThreeCardPositions(positions);

        Assert.True(singleReport.Success, string.Join(Environment.NewLine, singleReport.Diagnostics));
        Assert.Equal(156, singleReport.Counts["singleCardEntries"]);
        Assert.True(pairReport.Success, string.Join(Environment.NewLine, pairReport.Diagnostics));
        Assert.Equal(3003, pairReport.Counts["orientedPairIdentities"]);
        Assert.Equal(12012, pairReport.Counts["orientedPairStates"]);
        Assert.True(positionReport.Success, string.Join(Environment.NewLine, positionReport.Diagnostics));
        Assert.Equal(468, positionReport.Counts["threeCardPositions"]);
    }

    [Fact]
    public void PairInventoryRejectsMissingOrientationDuplicateSelfPairAndNoncanonicalPair()
    {
        var cards = StandardTarotCatalog.Deck.Cards.Select(card => card.Id).ToArray();
        var keys = CompletePairKeys(cards).ToList();
        keys.RemoveAt(0);
        keys.Add(keys[0]);
        keys.Add($"{cards[0]}__{cards[0]}|upright-upright");
        var ordered = cards.OrderBy(card => card.Value, StringComparer.Ordinal).Take(2).ToArray();
        keys.Add($"{ordered[1]}__{ordered[0]}|upright-upright");

        var report = InterpretationInventoryValidator.ValidateOrientedPairs(keys);

        Assert.False(report.Success);
        Assert.Contains(report.Diagnostics, item => item.Code == "inventory.duplicate");
        Assert.Contains(report.Diagnostics, item => item.Code == "inventory.key");
        Assert.Contains(report.Diagnostics, item => item.Code == "inventory.pairs.missing");
    }

    private static IEnumerable<string> CompletePairKeys(IReadOnlyList<TarotCardId> cards)
    {
        var ordered = cards.OrderBy(card => card.Value, StringComparer.Ordinal).ToArray();
        for (var first = 0; first < ordered.Length; first++)
        {
            for (var second = first + 1; second < ordered.Length; second++)
            {
                foreach (var state in Enum.GetValues<TarotOrientedPairState>())
                {
                    yield return TarotInterpretationKeys.CreateOrientedPair(ordered[first], ordered[second], state);
                }
            }
        }
    }
}
