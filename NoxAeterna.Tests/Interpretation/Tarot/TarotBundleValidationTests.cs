using NoxAeterna.Domain.Tarot;
using NoxAeterna.Interpretation.Tarot.Contracts;
using NoxAeterna.Interpretation.Tarot.Serialization;
using NoxAeterna.Interpretation.Tarot.Validation;

namespace NoxAeterna.Tests.Interpretation.Tarot;

public sealed class TarotBundleValidationTests
{
    [Fact]
    public void SingleBundleRequiresExactlyUprightAndReversedIndependentStates()
    {
        var result = TarotInterpretationValidator.ValidateSingleCardBundle(Single(), StandardTarotCatalog.Deck);
        Assert.True(result.IsValid, Format(result.Diagnostics));
        var entries=Assert.IsAssignableFrom<IReadOnlyList<TarotSingleCardEntry>>(result.Value);
        Assert.Equal(new[] { TarotCardOrientation.Upright, TarotCardOrientation.Reversed }, entries.Select(item => item.Orientation));
        Assert.Empty(entries[0].ReversalMechanisms);
        Assert.Equal(TarotReversalMechanism.Blocked, Assert.Single(entries[1].ReversalMechanisms));
    }

    [Theory]
    [InlineData("missing", "bundle.state-membership")]
    [InlineData("extra", "bundle.state-membership")]
    [InlineData("upright-mechanism", "single.upright-mechanisms")]
    [InlineData("reversed-empty", "single.reversed-mechanisms")]
    [InlineData("duplicate-tag", "tag.duplicate")]
    [InlineData("tag-range", "metric.valence")]
    public void InvalidSingleBundleIsRejected(string mutation, string expected)
    {
        var document = Single();
        switch (mutation)
        {
            case "missing": document.States!.Remove("reversed"); break;
            case "extra": document.States!["sideways"] = SingleState(false); break;
            case "upright-mechanism": document.States!["upright"]!.ReversalMechanisms = [TarotReversalMechanism.Blocked]; break;
            case "reversed-empty": document.States!["reversed"]!.ReversalMechanisms = []; break;
            case "duplicate-tag": document.States!["upright"]!.Tags = [Tag("choice"), Tag("choice")]; break;
            case "tag-range": document.States!["upright"]!.Tags = [new() { ConceptId = "choice", Valence = 3, Intensity = 2 }]; break;
        }
        var result = TarotInterpretationValidator.ValidateSingleCardBundle(document, StandardTarotCatalog.Deck);
        Assert.False(result.IsValid);
        Assert.Contains(result.Diagnostics, item => item.Code == expected);
    }

    [Fact]
    public void PairBundleRequiresAllFourOrientationStatesAndCanonicalDistinctCards()
    {
        var result = TarotInterpretationValidator.ValidateOrientedPairBundle(Pair(), StandardTarotCatalog.Deck);
        Assert.True(result.IsValid, Format(result.Diagnostics));
        Assert.Equal(4, result.Value!.Count);
        Assert.Equal(4, result.Value.Select(item => item.OrientationState).Distinct().Count());
    }

    [Theory]
    [InlineData("missing", "bundle.state-membership")]
    [InlineData("extra", "bundle.state-membership")]
    [InlineData("self", "pair.noncanonical")]
    [InlineData("noncanonical", "pair.noncanonical")]
    public void InvalidPairBundleIsRejected(string mutation, string expected)
    {
        var document = Pair();
        switch (mutation)
        {
            case "missing": document.States!.Remove("reversed-reversed"); break;
            case "extra": document.States!["sideways"] = PairState("sideways"); break;
            case "self": document.CardBId = document.CardAId; break;
            case "noncanonical": (document.CardAId, document.CardBId) = (document.CardBId, document.CardAId); break;
        }
        var result = TarotInterpretationValidator.ValidateOrientedPairBundle(document, StandardTarotCatalog.Deck);
        Assert.False(result.IsValid);
        Assert.Contains(result.Diagnostics, item => item.Code == expected);
    }

    [Fact]
    public void PositionBundleRequiresExactlyThreePositionsByTwoOrientations()
    {
        var result = TarotInterpretationValidator.ValidateThreeCardPositionsBundle(Positions(), StandardTarotCatalog.Deck);
        Assert.True(result.IsValid, Format(result.Diagnostics));
        Assert.Equal(6, result.Value!.Count);
    }

    [Theory]
    [InlineData("position-missing")]
    [InlineData("position-extra")]
    [InlineData("orientation-missing")]
    [InlineData("orientation-extra")]
    public void InvalidPositionStateMembershipIsRejected(string mutation)
    {
        var document = Positions();
        switch (mutation)
        {
            case "position-missing": document.States!.Remove("future"); break;
            case "position-extra": document.States!["elsewhere"] = PositionOrientations("elsewhere"); break;
            case "orientation-missing": document.States!["past"]!.Remove("reversed"); break;
            case "orientation-extra": document.States!["past"]!["sideways"] = PositionState("past", "sideways"); break;
        }
        var result = TarotInterpretationValidator.ValidateThreeCardPositionsBundle(document, StandardTarotCatalog.Deck);
        Assert.False(result.IsValid);
        Assert.Contains(result.Diagnostics, item => item.Code == "bundle.state-membership");
    }

    [Fact]
    public void StandardDeckDerivesExactCompleteBundleAndSemanticStateInventories()
    {
        var cards = StandardTarotCatalog.Deck.Cards.Count;
        var pairs = cards * (cards - 1) / 2;
        Assert.Equal(78, cards);
        Assert.Equal(156, cards * 2);
        Assert.Equal(3003, pairs);
        Assert.Equal(12012, pairs * 4);
        Assert.Equal(78, cards);
        Assert.Equal(468, cards * 3 * 2);
    }

    [Fact]
    public void SynthesisResourceRequiresFrozenIdentityAndExactTrimmedTextPayload()
    {
        var document = Synthesis(
            TarotSynthesisResourceType.TrajectoryProfile,
            TarotThreeCardSynthesisContract.Improving,
            "Траектория постепенно становится более конструктивной.");

        var result = TarotInterpretationValidator.ValidateSynthesisResource(document);

        Assert.True(result.IsValid, Format(result.Diagnostics));
        Assert.Equal("Траектория постепенно становится более конструктивной.", result.Value!.Text);
        Assert.Equal(
            TarotInterpretationJson.SerializeToString(new TarotSynthesisTextDocument { Text = "Траектория постепенно становится более конструктивной." }),
            result.Value.CanonicalJson);
    }

    [Theory]
    [InlineData(TarotSynthesisResourceType.TrajectoryProfile, "unknown", "synthesis.inventory")]
    [InlineData(TarotSynthesisResourceType.SynthesisFragment, TarotThreeCardSynthesisContract.Improving, "synthesis.inventory")]
    [InlineData(TarotSynthesisResourceType.RelationLabel, "overall", "synthesis.inventory")]
    public void SynthesisResourceRejectsUnknownReservedAndWrongTypeIdentities(
        TarotSynthesisResourceType resourceType,
        string resourceId,
        string expectedCode)
    {
        var result = TarotInterpretationValidator.ValidateSynthesisResource(Synthesis(resourceType, resourceId, "Valid text"));

        Assert.False(result.IsValid);
        Assert.Contains(result.Diagnostics, item => item.Code == expectedCode);
    }

    [Fact]
    public void SynthesisResourceRejectsUnknownPayloadMembers()
    {
        using var data = System.Text.Json.JsonDocument.Parse("{\"text\":\"Valid text\",\"kind\":\"extra\"}");
        var document = new TarotSynthesisResourceDocument
        {
            SchemaVersion = 1,
            ResourceType = TarotSynthesisResourceType.TrajectoryProfile,
            ResourceId = TarotThreeCardSynthesisContract.Improving,
            Data = data.RootElement.Clone()
        };

        var result = TarotInterpretationValidator.ValidateSynthesisResource(document);

        Assert.False(result.IsValid);
        Assert.Contains(result.Diagnostics, item => item.Code == "synthesis.payload");
    }

    private static TarotSingleCardBundleDocument Single() => new()
    {
        SchemaVersion = 1, CardId = "major.fool", States = new(StringComparer.Ordinal)
        {
            ["upright"] = SingleState(false), ["reversed"] = SingleState(true)
        }
    };
    private static TarotSingleCardStateDocument SingleState(bool reversed) => new()
    {
        Sections = new(StringComparer.Ordinal) { ["situation"] = "Situation", ["development"] = "Development", ["risk"] = "Risk", ["outcome"] = "Outcome", ["advice"] = "Advice" },
        Tags = [], OverallValence = reversed ? -1 : 1, OverallIntensity = 2,
        ReversalMechanisms = reversed ? [TarotReversalMechanism.Blocked] : []
    };
    private static TarotOrientedPairBundleDocument Pair() => new()
    {
        SchemaVersion = 1, CardAId = "major.fool", CardBId = "major.magician",
        States = new(StringComparer.Ordinal)
        {
            ["upright-upright"] = PairState("upright-upright"), ["upright-reversed"] = PairState("upright-reversed"),
            ["reversed-upright"] = PairState("reversed-upright"), ["reversed-reversed"] = PairState("reversed-reversed")
        }
    };
    private static TarotOrientedPairStateDocument PairState(string id) => new() { Interaction = $"Interaction {id}", Direction = $"Direction {id}", Tags = [], OverallValence = 0, OverallIntensity = 2 };
    private static TarotThreeCardPositionsBundleDocument Positions() => new()
    {
        SchemaVersion = 1, CardId = "major.fool", States = new(StringComparer.Ordinal)
        {
            ["past"] = PositionOrientations("past"), ["present"] = PositionOrientations("present"), ["future"] = PositionOrientations("future")
        }
    };
    private static Dictionary<string, TarotThreeCardPositionStateDocument?> PositionOrientations(string position) => new(StringComparer.Ordinal)
    {
        ["upright"] = PositionState(position, "upright"), ["reversed"] = PositionState(position, "reversed")
    };
    private static TarotThreeCardPositionStateDocument PositionState(string position, string orientation) => new() { Text = $"{position} {orientation}", Tags = [], OverallValence = 0, OverallIntensity = 2 };
    private static TarotTagAssignmentDocument Tag(string id) => new() { ConceptId = id, Valence = 0, Intensity = 2 };
    private static TarotSynthesisResourceDocument Synthesis(TarotSynthesisResourceType type, string id, string text)
    {
        using var data = System.Text.Json.JsonDocument.Parse($"{{\"text\":{System.Text.Json.JsonSerializer.Serialize(text)}}}");
        return new() { SchemaVersion = 1, ResourceType = type, ResourceId = id, Data = data.RootElement.Clone() };
    }
    private static string Format(IEnumerable<TarotValidationDiagnostic> diagnostics) => string.Join(Environment.NewLine, diagnostics.Select(item => $"{item.Code}: {item.Message}"));
}
