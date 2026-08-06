using NoxAeterna.Domain.Tarot;
using NoxAeterna.Interpretation.Tarot.Contracts;
using NoxAeterna.Interpretation.Tarot.Resolution;
using NoxAeterna.Interpretation.Tarot.Sources;

namespace NoxAeterna.Tests.Interpretation.Tarot;

public sealed class TarotInterpretationResolverLocaleTests
{
    private static readonly TarotInterpretationPackId Classic = new("classic");
    private static readonly TarotCardId Fool = new("major.fool");

    [Theory]
    [InlineData("ru", "ru")]
    [InlineData("zh", "en")]
    public void SingleCardResolution_UsesRequestedThenEnglish(string requested, string expected)
    {
        var source = new TarotInterpretationResolverTestSource();
        source.AddSingleCardCorpus(expected);
        source.SetReady(TarotInterpretationMode.SingleCard, expected);
        source.PublishManifest();
        var resolver = Resolver(source);

        var result = resolver.ResolveSingleCard(Classic, new(requested), Fool, TarotCardOrientation.Upright);

        var resolved = Assert.IsType<ResolvedTarotInterpretation<TarotSingleCardEntry>>(result);
        Assert.Equal(requested, resolved.RequestedLocale.Value);
        Assert.Equal(expected, resolved.ResolvedLocale.Value);
        Assert.Equal("Synthetic situation major.fool Upright", resolved.Content.Sections["situation"]);
    }

    [Fact]
    public void LocaleResolution_UsesRussianAfterEnglishAndDeduplicatesRequestedEnglish()
    {
        var russian = new TarotInterpretationResolverTestSource();
        russian.AddSingleCardCorpus("ru");
        russian.SetReady(TarotInterpretationMode.SingleCard, "ru");
        russian.PublishManifest();

        var fromChinese = Resolver(russian).ResolveSingleCard(Classic, new("zh"), Fool, TarotCardOrientation.Upright);
        var resolvedRussian = Assert.IsType<ResolvedTarotInterpretation<TarotSingleCardEntry>>(fromChinese);
        Assert.Equal("ru", resolvedRussian.ResolvedLocale.Value);

        var english = new TarotInterpretationResolverTestSource();
        english.AddSingleCardCorpus("en");
        english.SetReady(TarotInterpretationMode.SingleCard, "en");
        english.PublishManifest();
        var fromEnglish = Resolver(english).ResolveSingleCard(Classic, new("en"), Fool, TarotCardOrientation.Upright);

        Assert.IsType<ResolvedTarotInterpretation<TarotSingleCardEntry>>(fromEnglish);
        Assert.Equal(1, english.ReadCount("indexes/en/single-card.json"));
    }

    [Theory]
    [InlineData("ru", TarotInterpretationMode.SingleCard)]
    [InlineData("en", TarotInterpretationMode.SingleCard)]
    [InlineData("zh", TarotInterpretationMode.SingleCard)]
    [InlineData("ru", TarotInterpretationMode.TwoCards)]
    [InlineData("ru", TarotInterpretationMode.ThreeCards)]
    [InlineData("ru", TarotInterpretationMode.CelticCross)]
    public void NotReadyClassicShape_ReturnsNoReadyLocaleWithoutIndexReads(
        string requested,
        TarotInterpretationMode mode)
    {
        var source = new TarotInterpretationResolverTestSource();
        var result = Resolver(source).ResolveMode(Classic, mode, new(requested));

        var absent = Assert.IsType<NoTarotInterpretationContent<TarotResolvedModuleSnapshot>>(result);
        Assert.Equal(TarotNoContentReason.NoReadyLocale, absent.Reason);
        Assert.Equal(new[] { "interpretation-pack.json" }, source.Reads);
    }

    [Fact]
    public void UnknownPack_ReturnsPackUnavailableWithoutOpeningClassicSource()
    {
        var source = new TarotInterpretationResolverTestSource();

        var result = Resolver(source).ResolveSingleCard(
            new TarotInterpretationPackId("unknown"),
            new TarotInterpretationLocale("ru"),
            Fool,
            TarotCardOrientation.Upright);

        var absent = Assert.IsType<NoTarotInterpretationContent<TarotSingleCardEntry>>(result);
        Assert.Equal(TarotNoContentReason.PackUnavailable, absent.Reason);
        Assert.Empty(source.Reads);
    }

    [Fact]
    public void InvalidManifestIdentity_ReturnsValidationFailedBeforeIndexReads()
    {
        var source = new TarotInterpretationResolverTestSource();
        source.Manifest.SemanticDeckId = "other-78";
        source.PublishManifest();

        var result = Resolver(source).ResolveSingleCard(Classic, new("ru"), Fool, TarotCardOrientation.Upright);

        var absent = Assert.IsType<NoTarotInterpretationContent<TarotSingleCardEntry>>(result);
        Assert.Equal(TarotNoContentReason.ValidationFailed, absent.Reason);
        Assert.Equal("manifest.identity", absent.Diagnostic!.Code);
        Assert.Equal(new[] { "interpretation-pack.json" }, source.Reads);
    }

    [Fact]
    public void SourceReadResult_DefensivelyCopiesExactBytesAndKeepsFailuresTyped()
    {
        byte[] bytes = [1, 2, 3];
        var found = TarotInterpretationSourceReadResult.Found(bytes);
        bytes[0] = 9;
        var missing = TarotInterpretationSourceReadResult.Missing();
        var failed = TarotInterpretationSourceReadResult.Failed("source.synthetic", "Synthetic controlled failure.");

        Assert.Equal(new byte[] { 1, 2, 3 }, found.Bytes.ToArray());
        Assert.Equal(TarotInterpretationSourceReadStatus.Found, found.Status);
        Assert.Equal(TarotInterpretationSourceReadStatus.Missing, missing.Status);
        Assert.Empty(missing.Bytes.ToArray());
        Assert.Equal(TarotInterpretationSourceReadStatus.Failed, failed.Status);
        Assert.Equal("source.synthetic", failed.Diagnostic!.Code);
    }

    private static TarotInterpretationPackResolver Resolver(ITarotInterpretationPackSourceCatalog source) =>
        new(source, StandardTarotCatalog.Deck);
}
