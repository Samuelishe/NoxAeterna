using System.Security.Cryptography;
using NoxAeterna.Domain.Tarot;
using NoxAeterna.Interpretation.Tarot.Contracts;
using NoxAeterna.Interpretation.Tarot.Resolution;
using NoxAeterna.Interpretation.Tarot.Serialization;

namespace NoxAeterna.Tests.Interpretation.Tarot;

public sealed class TarotInterpretationResolverTrustChainTests
{
    private static readonly TarotInterpretationPackId Classic = new("classic");
    private static readonly TarotCardId Fool = new("major.fool");
    private const string EnglishIndex = "indexes/en/single-card.json";
    private const string EnglishEntry = "content/en/modes/single-card/major.fool/upright.json";

    [Theory]
    [InlineData("missing-index", "index.missing")]
    [InlineData("manifest-index-hash", "index.hash")]
    [InlineData("missing-entry", "content.missing")]
    [InlineData("content-hash", "content.hash")]
    [InlineData("malformed-content", "content.json")]
    [InlineData("identity-mismatch", "content.identity")]
    public void DamageAfterEnglishBecomesAuthoritative_ReturnsBrokenReadyWithoutRussianFallback(
        string damage,
        string expectedCode)
    {
        var source = HealthyEnglishAndRussian();
        ApplyDamage(source, damage);
        source.ClearReads();

        var result = Resolver(source).ResolveSingleCard(Classic, new("zh"), Fool, TarotCardOrientation.Upright);

        var absent = Assert.IsType<NoTarotInterpretationContent<TarotSingleCardEntry>>(result);
        Assert.Equal(TarotNoContentReason.BrokenReadyModule, absent.Reason);
        Assert.Equal(expectedCode, absent.Diagnostic!.Code);
        Assert.DoesNotContain("indexes/ru/single-card.json", source.Reads);
        Assert.DoesNotContain("content/ru/modes/single-card/major.fool/upright.json", source.Reads);
    }

    [Fact]
    public void IndexIdentityMismatchAndCrossLocaleContentPath_BreakTheSelectedLocale()
    {
        var wrongIndexLocale = new TarotInterpretationResolverTestSource();
        wrongIndexLocale.AddSingleCardCorpus("en", indexLocale: "ru");
        wrongIndexLocale.SetReady(TarotInterpretationMode.SingleCard, "en");
        wrongIndexLocale.PublishManifest();

        var localeResult = Resolver(wrongIndexLocale).ResolveSingleCard(
            Classic, new("en"), Fool, TarotCardOrientation.Upright);
        AssertBroken(localeResult, "index.identity");

        var wrongContentLocale = new TarotInterpretationResolverTestSource();
        wrongContentLocale.AddSingleCardCorpus(
            "en",
            targetPath: "content/ru/modes/single-card/major.fool/upright.json");
        wrongContentLocale.SetReady(TarotInterpretationMode.SingleCard, "en");
        wrongContentLocale.PublishManifest();

        var contentResult = Resolver(wrongContentLocale).ResolveSingleCard(
            Classic, new("en"), Fool, TarotCardOrientation.Upright);
        AssertBroken(contentResult, "index.locale-integrity");
    }

    [Theory]
    [InlineData("pack")]
    [InlineData("content-version")]
    public void GeneratedIndexPackAndContentVersionMustMatchManifest(string mutation)
    {
        var source = new TarotInterpretationResolverTestSource();
        source.AddSingleCardCorpus("en");
        source.SetReady(TarotInterpretationMode.SingleCard, "en");
        var index = TarotInterpretationJson.Parse<TarotGeneratedIndexDocument>(source.Get(EnglishIndex)).Document!;
        if (mutation == "pack")
        {
            index.PackId = "other";
        }
        else
        {
            index.ContentVersion = 2;
        }

        source.Replace(EnglishIndex, TarotInterpretationJson.Serialize(index));
        source.PublishManifest();

        var result = Resolver(source).ResolveSingleCard(Classic, new("en"), Fool, TarotCardOrientation.Upright);

        AssertBroken(result, "index.identity");
    }

    [Fact]
    public void ValidSingleEntry_UsesManifestIndexContentHashesAndReadsOnlyExactEntry()
    {
        var source = new TarotInterpretationResolverTestSource();
        source.AddSingleCardCorpus("ru");
        source.SetReady(TarotInterpretationMode.SingleCard, "ru");
        source.PublishManifest();

        var result = Resolver(source).ResolveSingleCard(Classic, new("ru"), Fool, TarotCardOrientation.Upright);

        var resolved = Assert.IsType<ResolvedTarotInterpretation<TarotSingleCardEntry>>(result);
        Assert.Equal(1, resolved.ContentVersion);
        Assert.Equal(new[] { "interpretation-pack.json", "indexes/ru/single-card.json", "content/ru/modes/single-card/major.fool/upright.json" }, source.Reads);
        Assert.DoesNotContain(source.Reads, path => path.Contains("major.world", StringComparison.Ordinal));
    }

    [Fact]
    public void MalformedManifest_ReturnsValidationFailedRatherThanBrokenReady()
    {
        var source = new TarotInterpretationResolverTestSource();
        source.Replace("interpretation-pack.json", "{"u8.ToArray());

        var result = Resolver(source).ResolveSingleCard(Classic, new("ru"), Fool, TarotCardOrientation.Upright);

        var absent = Assert.IsType<NoTarotInterpretationContent<TarotSingleCardEntry>>(result);
        Assert.Equal(TarotNoContentReason.ValidationFailed, absent.Reason);
        Assert.Equal("manifest.json", absent.Diagnostic!.Code);
    }

    private static TarotInterpretationResolverTestSource HealthyEnglishAndRussian()
    {
        var source = new TarotInterpretationResolverTestSource();
        source.AddSingleCardCorpus("en");
        source.AddSingleCardCorpus("ru");
        source.SetReady(TarotInterpretationMode.SingleCard, "en");
        source.SetReady(TarotInterpretationMode.SingleCard, "ru");
        source.PublishManifest();
        return source;
    }

    private static void ApplyDamage(TarotInterpretationResolverTestSource source, string damage)
    {
        switch (damage)
        {
            case "missing-index":
                source.Remove(EnglishIndex);
                break;
            case "manifest-index-hash":
                source.Replace(EnglishIndex, [.. source.Get(EnglishIndex), (byte)' ']);
                break;
            case "missing-entry":
                source.Remove(EnglishEntry);
                break;
            case "content-hash":
                source.Replace(EnglishEntry, [.. source.Get(EnglishEntry), (byte)' ']);
                break;
            case "malformed-content":
                ReplaceEntryAndTrustChain(source, "{"u8.ToArray());
                break;
            case "identity-mismatch":
                ReplaceEntryAndTrustChain(
                    source,
                    TarotInterpretationJson.Serialize(
                        TarotInterpretationResolverTestSource.SyntheticSingleCard("major.world")));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(damage));
        }
    }

    private static void ReplaceEntryAndTrustChain(
        TarotInterpretationResolverTestSource source,
        byte[] entryBytes)
    {
        source.Replace(EnglishEntry, entryBytes);
        var parsed = TarotInterpretationJson.Parse<TarotGeneratedIndexDocument>(source.Get(EnglishIndex));
        var index = parsed.Document!;
        index.Entries!.Single(item => item!.Key == "major.fool|upright")!.Sha256 = Hash(entryBytes);
        source.Replace(EnglishIndex, TarotInterpretationJson.Serialize(index));
        source.PublishManifest();
    }

    private static void AssertBroken(
        TarotInterpretationResolution<TarotSingleCardEntry> result,
        string expectedCode)
    {
        var absent = Assert.IsType<NoTarotInterpretationContent<TarotSingleCardEntry>>(result);
        Assert.Equal(TarotNoContentReason.BrokenReadyModule, absent.Reason);
        Assert.Equal(expectedCode, absent.Diagnostic!.Code);
    }

    private static TarotInterpretationPackResolver Resolver(TarotInterpretationResolverTestSource source) =>
        new(source, StandardTarotCatalog.Deck);

    private static string Hash(ReadOnlySpan<byte> bytes) => Convert.ToHexStringLower(SHA256.HashData(bytes));
}
