using System.Security.Cryptography;
using NoxAeterna.Domain.Tarot;
using NoxAeterna.Interpretation.Tarot.Contracts;
using NoxAeterna.Interpretation.Tarot.Resolution;
using NoxAeterna.Interpretation.Tarot.Serialization;

namespace NoxAeterna.Tests.Interpretation.Tarot;

public sealed class TarotInterpretationResolverCacheTests
{
    private static readonly TarotInterpretationPackId Classic = new("classic");
    private static readonly TarotCardId Fool = new("major.fool");
    private const string IndexPath = "indexes/ru/single-card.json";
    private const string FoolPath = "content/ru/modes/single-card/major.fool/upright.json";

    [Fact]
    public void SameSnapshotAndIndex_UsesEntryCacheUntilPackInvalidationOrClear()
    {
        var source = ReadySource();
        var resolver = Resolver(source);

        AssertResolved(resolver.ResolveSingleCard(Classic, new("ru"), Fool, TarotCardOrientation.Upright));
        AssertResolved(resolver.ResolveSingleCard(Classic, new("ru"), Fool, TarotCardOrientation.Upright));
        Assert.Equal(1, source.ReadCount("interpretation-pack.json"));
        Assert.Equal(1, source.ReadCount(IndexPath));
        Assert.Equal(1, source.ReadCount(FoolPath));

        resolver.InvalidatePack(Classic);
        AssertResolved(resolver.ResolveSingleCard(Classic, new("ru"), Fool, TarotCardOrientation.Upright));
        Assert.Equal(2, source.ReadCount(FoolPath));

        resolver.Clear();
        AssertResolved(resolver.ResolveSingleCard(Classic, new("ru"), Fool, TarotCardOrientation.Upright));
        Assert.Equal(3, source.ReadCount(FoolPath));
    }

    [Fact]
    public void NewSourceSnapshotAndIndexHash_ReloadsChangedContent()
    {
        var source = ReadySource();
        var resolver = Resolver(source);
        var first = AssertResolved(resolver.ResolveSingleCard(Classic, new("ru"), Fool, TarotCardOrientation.Upright));

        var changed = TarotInterpretationResolverTestSource.SyntheticSingleCard();
        changed.Sections!["situation"] = "Synthetic situation changed snapshot";
        ReplaceSingleEntry(source, changed, contentVersion: 1);
        source.PublishManifest(advanceSnapshot: true);

        var second = AssertResolved(resolver.ResolveSingleCard(Classic, new("ru"), Fool, TarotCardOrientation.Upright));

        Assert.NotEqual(first.Content.Sections["situation"], second.Content.Sections["situation"]);
        Assert.Equal("Synthetic situation changed snapshot", second.Content.Sections["situation"]);
        Assert.Equal(2, source.ReadCount(FoolPath));
    }

    [Fact]
    public void DifferentContentVersionLocaleAndPack_DoNotReuseEntryCache()
    {
        var classic = ReadySource();
        classic.AddSingleCardCorpus("en");
        classic.SetReady(TarotInterpretationMode.SingleCard, "en");
        classic.PublishManifest();
        var mystical = new TarotInterpretationResolverTestSource("mystical");
        mystical.AddSingleCardCorpus("ru");
        mystical.SetReady(TarotInterpretationMode.SingleCard, "ru");
        mystical.PublishManifest();
        var resolver = new TarotInterpretationPackResolver(
            new TarotInterpretationTestCatalog(classic, mystical),
            StandardTarotCatalog.Deck);

        AssertResolved(resolver.ResolveSingleCard(Classic, new("ru"), Fool, TarotCardOrientation.Upright));
        AssertResolved(resolver.ResolveSingleCard(Classic, new("en"), Fool, TarotCardOrientation.Upright));
        AssertResolved(resolver.ResolveSingleCard(new("mystical"), new("ru"), Fool, TarotCardOrientation.Upright));

        classic.Manifest.ContentVersion = 2;
        ReplaceSingleEntry(classic, TarotInterpretationResolverTestSource.SyntheticSingleCard(), contentVersion: 2);
        classic.PublishManifest(advanceSnapshot: true);
        var versionTwo = AssertResolved(resolver.ResolveSingleCard(
            Classic, new("ru"), Fool, TarotCardOrientation.Upright));

        Assert.Equal(2, versionTwo.ContentVersion);
        Assert.Equal(2, classic.ReadCount(FoolPath));
        Assert.Equal(1, classic.ReadCount("content/en/modes/single-card/major.fool/upright.json"));
        Assert.Equal(1, mystical.ReadCount(FoolPath));
    }

    [Fact]
    public void EntryCache_IsPositiveBoundedLruAndEvictsLeastRecentlyUsedValue()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new TarotInterpretationResolverOptions(manifestCapacity: 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new TarotInterpretationResolverOptions(indexCapacity: 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new TarotInterpretationResolverOptions(entryCapacity: 0));
        var source = ReadySource();
        AddSecondSingleEntry(source, "major.world");
        var resolver = Resolver(source, new TarotInterpretationResolverOptions(1, 1, 1));

        AssertResolved(resolver.ResolveSingleCard(Classic, new("ru"), Fool, TarotCardOrientation.Upright));
        AssertResolved(resolver.ResolveSingleCard(Classic, new("ru"), new("major.world"), TarotCardOrientation.Upright));
        AssertResolved(resolver.ResolveSingleCard(Classic, new("ru"), Fool, TarotCardOrientation.Upright));

        Assert.Equal(2, source.ReadCount(FoolPath));
        Assert.Equal(1, source.ReadCount("content/ru/modes/single-card/major.world/upright.json"));
    }

    private static TarotInterpretationResolverTestSource ReadySource()
    {
        var source = new TarotInterpretationResolverTestSource();
        source.AddSingleCardCorpus("ru");
        source.SetReady(TarotInterpretationMode.SingleCard, "ru");
        source.PublishManifest();
        return source;
    }

    private static void AddSecondSingleEntry(TarotInterpretationResolverTestSource source, string cardId)
    {
        var document = TarotInterpretationResolverTestSource.SyntheticSingleCard(cardId);
        var bytes = TarotInterpretationJson.Serialize(document);
        var path = $"content/ru/modes/single-card/{cardId}/upright.json";
        source.Replace(path, bytes);
        var index = TarotInterpretationJson.Parse<TarotGeneratedIndexDocument>(source.Get(IndexPath)).Document!;
        index.Entries!.Single(item => item!.Key == $"{cardId}|upright")!.Sha256 = Hash(bytes);
        source.Replace(IndexPath, TarotInterpretationJson.Serialize(index));
        source.PublishManifest();
    }

    private static void ReplaceSingleEntry(
        TarotInterpretationResolverTestSource source,
        TarotSingleCardDocument document,
        int contentVersion)
    {
        var bytes = TarotInterpretationJson.Serialize(document);
        source.Replace(FoolPath, bytes);
        var index = TarotInterpretationJson.Parse<TarotGeneratedIndexDocument>(source.Get(IndexPath)).Document!;
        index.ContentVersion = contentVersion;
        index.Entries!.Single(item => item!.Key == "major.fool|upright")!.Sha256 = Hash(bytes);
        source.Replace(IndexPath, TarotInterpretationJson.Serialize(index));
    }

    private static TarotInterpretationPackResolver Resolver(
        TarotInterpretationResolverTestSource source,
        TarotInterpretationResolverOptions? options = null) =>
        new(source, StandardTarotCatalog.Deck, options);

    private static ResolvedTarotInterpretation<TarotSingleCardEntry> AssertResolved(
        TarotInterpretationResolution<TarotSingleCardEntry> result) =>
        Assert.IsType<ResolvedTarotInterpretation<TarotSingleCardEntry>>(result);

    private static string Hash(ReadOnlySpan<byte> bytes) => Convert.ToHexStringLower(SHA256.HashData(bytes));
}
