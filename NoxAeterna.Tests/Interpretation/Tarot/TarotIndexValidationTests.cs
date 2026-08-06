using NoxAeterna.Interpretation.Tarot.Contracts;
using NoxAeterna.Interpretation.Tarot.Validation;

namespace NoxAeterna.Tests.Interpretation.Tarot;

public sealed class TarotIndexValidationTests
{
    [Fact]
    public void CompleteSingleCardIndex_ValidatesAndDefensivelyCopiesRoutes()
    {
        var document = TarotInterpretationTestDocuments.SingleCardIndex();

        var result = TarotInterpretationValidator.ValidateGeneratedIndex(document);

        Assert.True(result.IsValid, Format(result.Diagnostics));
        var index = Assert.IsType<TarotGeneratedIndex>(result.Value);
        Assert.Equal(156, index.ExpectedEntryCount);
        Assert.Equal(156, index.Entries.Count);
        Assert.Null(index.ExpectedIdentityCount);
        Assert.Null(index.ExpectedPositionEntryCount);
        var firstKey = index.Entries[0].Key;
        document.Entries!.Clear();
        Assert.Equal(156, index.Entries.Count);
        Assert.Equal(firstKey, index.Entries[0].Key);
        Assert.Throws<NotSupportedException>(() =>
            ((ICollection<TarotGeneratedIndexEntry>)index.Entries).Add(index.Entries[0]));
    }

    [Theory]
    [InlineData("unsorted", "index.order")]
    [InlineData("duplicate-key", "index.key-duplicate")]
    [InlineData("duplicate-path", "index.path-duplicate")]
    [InlineData("bad-hash", "value.invalid")]
    [InlineData("unsafe-path", "value.invalid")]
    [InlineData("count-mismatch", "index.count")]
    [InlineData("wrong-specific-count", "index.single-extra-count")]
    [InlineData("bad-pack", "value.invalid")]
    [InlineData("bad-locale", "value.invalid")]
    [InlineData("bad-corpus", "enum.invalid")]
    [InlineData("negative-count", "number.non-negative")]
    public void InvalidIndexContracts_ReturnTypedErrors(string mutation, string expectedCode)
    {
        var document = TarotInterpretationTestDocuments.SingleCardIndex();
        switch (mutation)
        {
            case "unsorted":
                (document.Entries![0], document.Entries[1]) = (document.Entries[1], document.Entries[0]);
                break;
            case "duplicate-key": document.Entries![1]!.Key = document.Entries[0]!.Key; break;
            case "duplicate-path": document.Entries![1]!.Path = document.Entries[0]!.Path; break;
            case "bad-hash": document.Entries![0]!.Sha256 = TarotInterpretationTestDocuments.Hash.ToUpperInvariant(); break;
            case "unsafe-path": document.Entries![0]!.Path = "../entry.json"; break;
            case "count-mismatch": document.ExpectedEntryCount = 155; break;
            case "wrong-specific-count": document.ExpectedIdentityCount = 3003; break;
            case "bad-pack": document.PackId = "Classic"; break;
            case "bad-locale": document.Locale = "ru_RU"; break;
            case "bad-corpus": document.CorpusId = (TarotInterpretationCorpus)99; break;
            case "negative-count": document.ExpectedEntryCount = -1; break;
        }

        var result = TarotInterpretationValidator.ValidateGeneratedIndex(document);

        Assert.False(result.IsValid);
        Assert.Null(result.Value);
        Assert.Contains(result.Diagnostics, item => item.Code == expectedCode);
    }

    [Fact]
    public void OrientedPairIndex_RequiresExactIdentityAndStateCounts()
    {
        var document = EmptyIndex(TarotInterpretationCorpus.OrientedPairs);
        document.ExpectedEntryCount = 12011;
        document.ExpectedIdentityCount = 3002;

        var result = TarotInterpretationValidator.ValidateGeneratedIndex(document);

        Assert.False(result.IsValid);
        Assert.Contains(result.Diagnostics, item => item.Code == "index.pair-count");
        Assert.Contains(result.Diagnostics, item => item.Code == "index.pair-identities");
    }

    [Fact]
    public void ThreeCardsIndex_RequiresExactPositionCountWithoutFreezingSynthesisCount()
    {
        var document = EmptyIndex(TarotInterpretationCorpus.ThreeCards);
        document.ExpectedPositionEntryCount = 467;

        var result = TarotInterpretationValidator.ValidateGeneratedIndex(document);

        Assert.False(result.IsValid);
        Assert.Contains(result.Diagnostics, item => item.Code == "index.position-count");
        Assert.DoesNotContain(result.Diagnostics, item => item.Code.Contains("synthesis", StringComparison.Ordinal));
    }

    [Fact]
    public void GeneratedIndexJson_RejectsProseField()
    {
        const string json = "{\"schemaVersion\":1,\"packId\":\"classic\",\"locale\":\"ru\",\"corpusId\":\"single-card\",\"contentVersion\":1,\"expectedEntryCount\":0,\"entries\":[],\"prose\":\"forbidden\"}";

        var parsed = NoxAeterna.Interpretation.Tarot.Serialization.TarotInterpretationJson
            .Parse<TarotGeneratedIndexDocument>(json);

        Assert.False(parsed.IsSuccess);
        Assert.Contains("prose", parsed.Failure!.Message, StringComparison.Ordinal);
    }

    private static TarotGeneratedIndexDocument EmptyIndex(TarotInterpretationCorpus corpus) => new()
    {
        SchemaVersion = 1,
        PackId = "classic",
        Locale = "ru",
        CorpusId = corpus,
        ContentVersion = 1,
        ExpectedEntryCount = 0,
        Entries = []
    };

    private static string Format(IEnumerable<TarotValidationDiagnostic> diagnostics) =>
        string.Join(Environment.NewLine, diagnostics.Select(item => $"{item.Code}: {item.Message}"));
}
