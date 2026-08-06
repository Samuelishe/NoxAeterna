using System.Text;
using NoxAeterna.Domain.Tarot;
using NoxAeterna.Interpretation.Tarot.Contracts;
using NoxAeterna.Interpretation.Tarot.Serialization;
using NoxAeterna.Interpretation.Tarot.Validation;

namespace NoxAeterna.Tests.Interpretation.Tarot;

public sealed class TarotInterpretationJsonTests
{
    [Fact]
    public void Manifest_RoundTripsWithExactCamelCaseAndDeterministicBytes()
    {
        var document = TarotInterpretationTestDocuments.Manifest();

        var first = TarotInterpretationJson.Serialize(document);
        var second = TarotInterpretationJson.Serialize(document);
        var json = Encoding.UTF8.GetString(first);
        var parsed = TarotInterpretationJson.Parse<TarotInterpretationPackDocument>(first);

        Assert.Equal(first, second);
        Assert.False(first.AsSpan().StartsWith(Encoding.UTF8.Preamble));
        Assert.Equal((byte)'\n', first[^1]);
        Assert.NotEqual((byte)'\n', first[^2]);
        Assert.NotEqual((byte)'\r', first[^2]);
        Assert.StartsWith("{\"schemaVersion\":1,", json, StringComparison.Ordinal);
        Assert.Contains("\"packId\":\"classic\"", json, StringComparison.Ordinal);
        Assert.DoesNotContain("SchemaVersion", json, StringComparison.Ordinal);
        Assert.True(parsed.IsSuccess);
        Assert.Null(parsed.Failure);
        Assert.Equal("classic", parsed.Document!.PackId);
        Assert.Equal(new[] { "ru", "en" }, parsed.Document.DeclaredLocales);
    }

    [Fact]
    public void ContentDocuments_RoundTripThroughRawDocumentBoundary()
    {
        AssertRoundTrip(TarotInterpretationTestDocuments.Vocabulary(), value => Assert.Equal("failure", value.ConceptId));
        AssertRoundTrip(TarotInterpretationTestDocuments.Tag(), value => Assert.Equal(-1, value.Valence));
        AssertRoundTrip(TarotInterpretationTestDocuments.SingleCard(), value =>
            Assert.Equal(TarotCardOrientation.Upright, value.Orientation));
        AssertRoundTrip(TarotInterpretationTestDocuments.Pair(), value =>
            Assert.Equal(TarotOrientedPairState.ReversedUpright, value.OrientationState));
        AssertRoundTrip(TarotInterpretationTestDocuments.Position(), value =>
            Assert.Equal(TarotThreeCardPosition.Past, value.Position));
        AssertRoundTrip(TarotInterpretationTestDocuments.SingleCardIndex(), value =>
            Assert.Equal(156, value.Entries!.Count));
    }

    [Fact]
    public void ClosedEnums_SerializeAsExactLowercaseSchemaStrings()
    {
        Assert.Equal("\"upright\"\n", TarotInterpretationJson.SerializeToString(TarotCardOrientation.Upright));
        Assert.Equal("\"single-card\"\n", TarotInterpretationJson.SerializeToString(TarotInterpretationMode.SingleCard));
        Assert.Equal("\"reversed-upright\"\n", TarotInterpretationJson.SerializeToString(TarotOrientedPairState.ReversedUpright));
        Assert.Equal("\"present\"\n", TarotInterpretationJson.SerializeToString(TarotThreeCardPosition.Present));
        Assert.Equal("\"internalized\"\n", TarotInterpretationJson.SerializeToString(TarotReversalMechanism.Internalized));
        Assert.Equal("\"trajectory-profile\"\n", TarotInterpretationJson.SerializeToString(TarotSynthesisResourceType.TrajectoryProfile));
        Assert.Equal("\"three-card-synthesis\"\n", TarotInterpretationJson.SerializeToString(TarotModuleDependency.ThreeCardSynthesis));
        Assert.Equal("\"oriented-pairs\"\n", TarotInterpretationJson.SerializeToString(TarotInterpretationCorpus.OrientedPairs));
        Assert.Equal("\"broken-ready-module\"\n", TarotInterpretationJson.SerializeToString(TarotNoContentReason.BrokenReadyModule));
    }

    [Fact]
    public void SchemaTextMaps_CoverEveryClosedEnumValue()
    {
        Assert.Equal(Enum.GetValues<TarotCardOrientation>(), TarotSchemaText.CardOrientations.Keys.Order());
        Assert.Equal(Enum.GetValues<TarotInterpretationMode>(), TarotSchemaText.Modes.Keys.Order());
        Assert.Equal(Enum.GetValues<TarotOrientedPairState>(), TarotSchemaText.PairStates.Keys.Order());
        Assert.Equal(Enum.GetValues<TarotThreeCardPosition>(), TarotSchemaText.Positions.Keys.Order());
        Assert.Equal(Enum.GetValues<TarotReversalMechanism>(), TarotSchemaText.ReversalMechanisms.Keys.Order());
        Assert.Equal(Enum.GetValues<TarotSynthesisResourceType>(), TarotSchemaText.SynthesisResourceTypes.Keys.Order());
        Assert.Equal(Enum.GetValues<TarotModuleDependency>(), TarotSchemaText.Dependencies.Keys.Order());
        Assert.Equal(Enum.GetValues<TarotInterpretationCorpus>(), TarotSchemaText.Corpora.Keys.Order());
        Assert.Equal(Enum.GetValues<TarotNoContentReason>(), TarotSchemaText.NoContentReasons.Keys.Order());
        Assert.All(
            TarotSchemaText.Modes.Values
                .Concat(TarotSchemaText.PairStates.Values)
                .Concat(TarotSchemaText.Positions.Values)
                .Concat(TarotSchemaText.ReversalMechanisms.Values)
                .Concat(TarotSchemaText.SynthesisResourceTypes.Values)
                .Concat(TarotSchemaText.Dependencies.Values)
                .Concat(TarotSchemaText.Corpora.Values)
                .Concat(TarotSchemaText.NoContentReasons.Values),
            value => Assert.Matches("^[a-z]+(?:-[a-z]+)*$", value));
    }

    [Theory]
    [InlineData("{", TarotJsonParseFailureKind.MalformedJson)]
    [InlineData("{/*comment*/\"schemaVersion\":1}", TarotJsonParseFailureKind.MalformedJson)]
    [InlineData("{\"schemaVersion\":1,}", TarotJsonParseFailureKind.MalformedJson)]
    [InlineData("{\"conceptId\":\"x\",\"valence\":NaN,\"intensity\":1}", TarotJsonParseFailureKind.MalformedJson)]
    public void MalformedJson_IsControlledAndDistinctFromValidation(string json, TarotJsonParseFailureKind kind)
    {
        var result = TarotInterpretationJson.Parse<TarotTagAssignmentDocument>(json);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Document);
        Assert.Equal(kind, result.Failure?.Kind);
    }

    [Fact]
    public void DuplicateProperties_AreRejectedBeforeDeserialization()
    {
        const string json = "{\"schemaVersion\":1,\"conceptId\":\"failure\",\"conceptId\":\"choice\",\"label\":\"x\",\"meaning\":\"y\"}";

        var result = TarotInterpretationJson.Parse<TarotVocabularyDocument>(json);

        Assert.False(result.IsSuccess);
        Assert.Equal(TarotJsonParseFailureKind.DuplicateProperty, result.Failure?.Kind);
        Assert.Equal("$.conceptId", result.Failure?.Path);
    }

    [Fact]
    public void Utf8Bom_IsRejectedByTheParseContract()
    {
        var json = Encoding.UTF8.GetBytes("{}");
        var bytes = Encoding.UTF8.GetPreamble().Concat(json).ToArray();

        var result = TarotInterpretationJson.Parse<TarotVocabularyDocument>(bytes);

        Assert.False(result.IsSuccess);
        Assert.Equal(TarotJsonParseFailureKind.UnsupportedValue, result.Failure?.Kind);
        Assert.Contains("BOM", result.Failure!.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void UnknownRequiredEnum_IsUnsupportedRatherThanAnEmptyDocument()
    {
        const string json = "{\"schemaVersion\":1,\"cardId\":\"major.fool\",\"orientation\":\"UPRIGHT\"}";

        var result = TarotInterpretationJson.Parse<TarotSingleCardDocument>(json);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Document);
        Assert.Equal(TarotJsonParseFailureKind.UnsupportedValue, result.Failure?.Kind);
    }

    [Fact]
    public void ForbiddenOrUnknownTagFields_AreRejectedBySchemaPolicy()
    {
        const string json = "{\"conceptId\":\"conflict\",\"valence\":-1,\"intensity\":2,\"relevance\":5}";

        var result = TarotInterpretationJson.Parse<TarotTagAssignmentDocument>(json);

        Assert.False(result.IsSuccess);
        Assert.Contains("relevance", result.Failure!.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ParseAndValidationFailures_AreSeparateOutcomes()
    {
        var parsed = TarotInterpretationJson.Parse<TarotVocabularyDocument>("{}");

        Assert.True(parsed.IsSuccess);
        var validated = TarotInterpretationValidator.ValidateVocabulary(parsed.Document!);
        Assert.False(validated.IsValid);
        Assert.Null(validated.Value);
        Assert.Contains(validated.Diagnostics, item => item.Code == "schema.unsupported");
        Assert.Contains(validated.Diagnostics, item => item.Field == "conceptId");
    }

    [Fact]
    public void StreamOverloads_RemainInMemoryAndPreserveByteContract()
    {
        using var stream = new MemoryStream();
        TarotInterpretationJson.Serialize(stream, TarotInterpretationTestDocuments.Vocabulary());
        stream.Position = 0;

        var result = TarotInterpretationJson.Parse<TarotVocabularyDocument>(stream);

        Assert.True(result.IsSuccess);
        Assert.Equal("Неудача", result.Document!.Label);
    }

    private static void AssertRoundTrip<TDocument>(TDocument document, Action<TDocument> assert)
        where TDocument : class
    {
        var parsed = TarotInterpretationJson.Parse<TDocument>(TarotInterpretationJson.Serialize(document));
        Assert.True(parsed.IsSuccess, parsed.Failure?.Message);
        assert(parsed.Document!);
    }
}
