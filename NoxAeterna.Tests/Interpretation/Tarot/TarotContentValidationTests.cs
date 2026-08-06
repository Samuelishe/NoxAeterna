using NoxAeterna.Domain.Tarot;
using NoxAeterna.Interpretation.Tarot.Contracts;
using NoxAeterna.Interpretation.Tarot.Validation;

namespace NoxAeterna.Tests.Interpretation.Tarot;

public sealed class TarotContentValidationTests
{
    [Fact]
    public void VocabularyAndTag_ValidateExactMeaningAndMetrics()
    {
        var vocabulary = TarotInterpretationValidator.ValidateVocabulary(TarotInterpretationTestDocuments.Vocabulary());
        var tag = TarotInterpretationValidator.ValidateTagAssignment(TarotInterpretationTestDocuments.Tag());

        Assert.True(vocabulary.IsValid);
        Assert.Equal("failure", vocabulary.Value!.ConceptId.Value);
        Assert.Equal("Неудача", vocabulary.Value.Label);
        Assert.True(tag.IsValid);
        Assert.Equal(-1, tag.Value!.Valence);
        Assert.Equal(2, tag.Value.Intensity);
    }

    [Theory]
    [InlineData("concept", "value.invalid")]
    [InlineData("label", "text.empty")]
    [InlineData("meaning", "text.empty")]
    [InlineData("valence", "metric.valence")]
    [InlineData("intensity", "metric.intensity")]
    public void VocabularyAndTag_InvalidFieldsAreErrors(string mutation, string expectedCode)
    {
        TarotValidationDiagnostic diagnostic;
        if (mutation is "valence" or "intensity")
        {
            var document = TarotInterpretationTestDocuments.Tag();
            if (mutation == "valence") document.Valence = 3;
            else document.Intensity = 0;
            var result = TarotInterpretationValidator.ValidateTagAssignment(document);
            Assert.False(result.IsValid);
            diagnostic = Assert.Single(result.Diagnostics, item => item.Code == expectedCode);
        }
        else
        {
            var document = TarotInterpretationTestDocuments.Vocabulary();
            if (mutation == "concept") document.ConceptId = "";
            else if (mutation == "label") document.Label = " ";
            else document.Meaning = null;
            var result = TarotInterpretationValidator.ValidateVocabulary(document);
            Assert.False(result.IsValid);
            diagnostic = Assert.Single(result.Diagnostics, item => item.Code == expectedCode);
        }

        Assert.Equal(TarotValidationSeverity.Error, diagnostic.Severity);
    }

    [Fact]
    public void ValidationDiagnostics_AreDeterministicallyOrderedAndReadOnly()
    {
        var document = TarotInterpretationTestDocuments.Vocabulary();
        document.SchemaVersion = 2;
        document.ConceptId = "";
        document.Label = " ";
        var first = TarotInterpretationValidator.ValidateVocabulary(document);
        var second = TarotInterpretationValidator.ValidateVocabulary(document);

        Assert.Equal(first.Diagnostics, second.Diagnostics);
        Assert.Equal(
            new[] { "schema.unsupported", "value.invalid", "text.empty" },
            first.Diagnostics.Select(item => item.Code));
        Assert.Throws<NotSupportedException>(() =>
            ((ICollection<TarotValidationDiagnostic>)first.Diagnostics).Add(first.Diagnostics[0]));
    }

    [Fact]
    public void ValidSingleCard_CreatesDefensiveImmutableValue()
    {
        var document = TarotInterpretationTestDocuments.SingleCard(TarotCardOrientation.Reversed);

        var result = TarotInterpretationValidator.ValidateSingleCard(document, StandardTarotCatalog.Deck);

        Assert.True(result.IsValid, Format(result.Diagnostics));
        var entry = Assert.IsType<TarotSingleCardEntry>(result.Value);
        Assert.Equal(5, entry.Sections.Count);
        Assert.Equal(TarotReversalMechanism.Blocked, Assert.Single(entry.ReversalMechanisms));
        document.Sections!["situation"] = "changed";
        document.Tags!.Clear();
        Assert.Equal("Ситуация", entry.Sections["situation"]);
        Assert.Equal(5, entry.Tags.Count);
        Assert.Throws<NotSupportedException>(() =>
            ((ICollection<TarotTagAssignment>)entry.Tags).Add(entry.Tags[0]));
    }

    [Theory]
    [InlineData("missing-section", "sections.required")]
    [InlineData("empty-section", "sections.required")]
    [InlineData("upright-mechanism", "reversal.upright")]
    [InlineData("reversed-no-mechanism", "reversal.count")]
    [InlineData("reversed-four-mechanisms", "reversal.count")]
    [InlineData("duplicate-mechanism", "reversal.duplicate")]
    [InlineData("invalid-overall", "metric.valence")]
    [InlineData("unknown-card", "card.unknown")]
    [InlineData("duplicate-tag", "tags.duplicate")]
    public void InvalidSingleCardContracts_ReturnErrors(string mutation, string expectedCode)
    {
        var document = mutation.StartsWith("reversed", StringComparison.Ordinal)
            ? TarotInterpretationTestDocuments.SingleCard(TarotCardOrientation.Reversed)
            : TarotInterpretationTestDocuments.SingleCard();
        switch (mutation)
        {
            case "missing-section": document.Sections!.Remove("risk"); break;
            case "empty-section": document.Sections!["advice"] = " "; break;
            case "upright-mechanism": document.ReversalMechanisms = [TarotReversalMechanism.Blocked]; break;
            case "reversed-no-mechanism": document.ReversalMechanisms = []; break;
            case "reversed-four-mechanisms": document.ReversalMechanisms =
                [TarotReversalMechanism.Blocked, TarotReversalMechanism.Delayed, TarotReversalMechanism.Distorted, TarotReversalMechanism.Resisted]; break;
            case "duplicate-mechanism": document.ReversalMechanisms =
                [TarotReversalMechanism.Blocked, TarotReversalMechanism.Blocked]; break;
            case "invalid-overall": document.OverallValence = -3; break;
            case "unknown-card": document.CardId = "major.unknown"; break;
            case "duplicate-tag": document.Tags![1]!.ConceptId = document.Tags[0]!.ConceptId; break;
        }

        var result = TarotInterpretationValidator.ValidateSingleCard(document, StandardTarotCatalog.Deck);

        Assert.False(result.IsValid);
        Assert.Null(result.Value);
        Assert.Contains(result.Diagnostics, item => item.Code == expectedCode && item.Severity == TarotValidationSeverity.Error);
    }

    [Fact]
    public void SingleCardTagCountDeviation_IsWarningWithoutInvalidatingDraft()
    {
        var document = TarotInterpretationTestDocuments.SingleCard();
        document.Tags = [TarotInterpretationTestDocuments.Tag()];

        var result = TarotInterpretationValidator.ValidateSingleCard(document, StandardTarotCatalog.Deck);

        Assert.True(result.IsValid, Format(result.Diagnostics));
        var warning = Assert.Single(result.Diagnostics);
        Assert.Equal("tags.authoring-target", warning.Code);
        Assert.Equal(TarotValidationSeverity.Warning, warning.Severity);
    }

    [Fact]
    public void ValidOrientedPair_PreservesCanonicalIdentityAndOwnMetrics()
    {
        var result = TarotInterpretationValidator.ValidateOrientedPair(
            TarotInterpretationTestDocuments.Pair(),
            StandardTarotCatalog.Deck);

        Assert.True(result.IsValid, Format(result.Diagnostics));
        Assert.Equal("major.tower", result.Value!.CardAId.Value);
        Assert.Equal("major.world", result.Value.CardBId.Value);
        Assert.Equal(TarotOrientedPairState.ReversedUpright, result.Value.OrientationState);
        Assert.Equal(6, result.Value.Tags.Count);
        Assert.Equal(3, result.Value.OverallIntensity);
    }

    [Theory]
    [InlineData("self", "pair.self")]
    [InlineData("noncanonical", "pair.noncanonical")]
    [InlineData("missing-prose", "text.empty")]
    [InlineData("orientation", "enum.invalid")]
    [InlineData("duplicate-tag", "tags.duplicate")]
    [InlineData("unknown-card", "card.unknown")]
    public void InvalidPairContracts_ReturnErrors(string mutation, string expectedCode)
    {
        var document = TarotInterpretationTestDocuments.Pair();
        switch (mutation)
        {
            case "self": document.CardBId = document.CardAId; break;
            case "noncanonical": (document.CardAId, document.CardBId) = (document.CardBId, document.CardAId); break;
            case "missing-prose": document.Interaction = " "; break;
            case "orientation": document.OrientationState = (TarotOrientedPairState)99; break;
            case "duplicate-tag": document.Tags![1]!.ConceptId = document.Tags[0]!.ConceptId; break;
            case "unknown-card": document.CardBId = "major.unknown"; break;
        }

        var result = TarotInterpretationValidator.ValidateOrientedPair(document, StandardTarotCatalog.Deck);

        Assert.False(result.IsValid);
        Assert.Contains(result.Diagnostics, item => item.Code == expectedCode);
    }

    [Fact]
    public void PairTagCountDeviation_IsWarningNotSchemaFailure()
    {
        var document = TarotInterpretationTestDocuments.Pair();
        document.Tags = [TarotInterpretationTestDocuments.Tag()];

        var result = TarotInterpretationValidator.ValidateOrientedPair(document, StandardTarotCatalog.Deck);

        Assert.True(result.IsValid, Format(result.Diagnostics));
        Assert.Contains(result.Diagnostics, item =>
            item.Code == "tags.authoring-target" && item.Severity == TarotValidationSeverity.Warning);
    }

    [Fact]
    public void PositionEntry_ValidatesExactPositionTextTagsAndMetrics()
    {
        var result = TarotInterpretationValidator.ValidateThreeCardPosition(
            TarotInterpretationTestDocuments.Position(),
            StandardTarotCatalog.Deck);

        Assert.True(result.IsValid, Format(result.Diagnostics));
        Assert.Equal(TarotThreeCardPosition.Past, result.Value!.Position);
        Assert.Equal("major.tower", result.Value.CardId.Value);
        Assert.Single(result.Value.Tags);
    }

    [Theory]
    [InlineData("position", "enum.invalid")]
    [InlineData("text", "text.empty")]
    [InlineData("card", "card.unknown")]
    [InlineData("metric", "metric.intensity")]
    [InlineData("duplicate-tag", "tags.duplicate")]
    public void InvalidPositionContracts_ReturnErrors(string mutation, string expectedCode)
    {
        var document = TarotInterpretationTestDocuments.Position();
        switch (mutation)
        {
            case "position": document.Position = (TarotThreeCardPosition)99; break;
            case "text": document.Text = null; break;
            case "card": document.CardId = "major.unknown"; break;
            case "metric": document.OverallIntensity = 4; break;
            case "duplicate-tag": document.Tags!.Add(TarotInterpretationTestDocuments.Tag("release")); break;
        }

        var result = TarotInterpretationValidator.ValidateThreeCardPosition(document, StandardTarotCatalog.Deck);

        Assert.False(result.IsValid);
        Assert.Contains(result.Diagnostics, item => item.Code == expectedCode);
    }

    private static string Format(IEnumerable<TarotValidationDiagnostic> diagnostics) =>
        string.Join(Environment.NewLine, diagnostics.Select(item => $"{item.Code}: {item.Message}"));
}
