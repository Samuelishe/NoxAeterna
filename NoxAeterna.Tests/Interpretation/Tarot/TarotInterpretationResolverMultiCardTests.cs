using NoxAeterna.Domain.Tarot;
using NoxAeterna.Interpretation.Tarot.Contracts;
using NoxAeterna.Interpretation.Tarot.Resolution;

namespace NoxAeterna.Tests.Interpretation.Tarot;

public sealed class TarotInterpretationResolverMultiCardTests
{
    private static readonly TarotInterpretationPackId Classic = new("classic");
    private static readonly TarotCardId Tower = new("major.tower");
    private static readonly TarotCardId World = new("major.world");

    [Theory]
    [InlineData(TarotOrientedPairState.UprightUpright)]
    [InlineData(TarotOrientedPairState.UprightReversed)]
    [InlineData(TarotOrientedPairState.ReversedUpright)]
    [InlineData(TarotOrientedPairState.ReversedReversed)]
    public void PairResolution_UsesAllFourCanonicalOrientationStates(TarotOrientedPairState state)
    {
        var source = new TarotInterpretationResolverTestSource();
        source.AddOrientedPairCorpus(
            "ru",
            TarotInterpretationResolverTestSource.SyntheticPair(state: state));
        source.SetReady(TarotInterpretationMode.TwoCards, "ru");
        source.PublishManifest();
        var (towerOrientation, worldOrientation) = Orientations(state);

        var result = Resolver(source).ResolveOrientedPair(
            Classic,
            TarotInterpretationMode.TwoCards,
            new("ru"),
            World,
            worldOrientation,
            Tower,
            towerOrientation);

        var resolved = Assert.IsType<ResolvedTarotInterpretation<TarotOrientedPairEntry>>(result);
        Assert.Equal(Tower, resolved.Content.CardAId);
        Assert.Equal(World, resolved.Content.CardBId);
        Assert.Equal(state, resolved.Content.OrientationState);
        Assert.Single(source.Reads, path => path.StartsWith("content/", StringComparison.Ordinal));
    }

    [Fact]
    public void PairResolution_RejectsUnsupportedModeAndMissingIndexedStateWithoutScanning()
    {
        var source = new TarotInterpretationResolverTestSource();
        source.AddOrientedPairCorpus("ru");
        source.SetReady(TarotInterpretationMode.TwoCards, "ru");
        source.PublishManifest();
        var resolver = Resolver(source);

        var unsupported = resolver.ResolveOrientedPair(
            Classic, TarotInterpretationMode.SingleCard, new("ru"),
            Tower, TarotCardOrientation.Reversed, World, TarotCardOrientation.Upright);
        var unsupportedAbsent = Assert.IsType<NoTarotInterpretationContent<TarotOrientedPairEntry>>(unsupported);
        Assert.Equal(TarotNoContentReason.UnsupportedMode, unsupportedAbsent.Reason);
        Assert.Empty(source.Reads);

        var missing = resolver.ResolveOrientedPair(
            Classic, TarotInterpretationMode.TwoCards, new("ru"),
            Tower, TarotCardOrientation.Upright, World, TarotCardOrientation.Upright);
        var missingAbsent = Assert.IsType<NoTarotInterpretationContent<TarotOrientedPairEntry>>(missing);
        Assert.Equal(TarotNoContentReason.BrokenReadyModule, missingAbsent.Reason);
        Assert.Equal("content.missing", missingAbsent.Diagnostic!.Code);
        Assert.Single(source.Reads, path => path.StartsWith("content/", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(TarotThreeCardPosition.Past, TarotCardOrientation.Upright)]
    [InlineData(TarotThreeCardPosition.Present, TarotCardOrientation.Reversed)]
    [InlineData(TarotThreeCardPosition.Future, TarotCardOrientation.Upright)]
    public void PositionResolution_ValidatesBothSameLocaleIndexesAndLoadsOnePosition(
        TarotThreeCardPosition position,
        TarotCardOrientation orientation)
    {
        var source = new TarotInterpretationResolverTestSource();
        source.AddOrientedPairCorpus("ru");
        source.AddThreeCardCorpus(
            "ru",
            TarotInterpretationResolverTestSource.SyntheticPosition(position, orientation: orientation));
        source.SetReady(TarotInterpretationMode.ThreeCards, "ru");
        source.PublishManifest();

        var result = Resolver(source).ResolveThreeCardPosition(
            Classic, new("ru"), position, new TarotCardId("major.fool"), orientation);

        var resolved = Assert.IsType<ResolvedTarotInterpretation<TarotThreeCardPositionEntry>>(result);
        Assert.Equal(position, resolved.Content.Position);
        Assert.Equal(orientation, resolved.Content.Orientation);
        Assert.Contains("indexes/ru/oriented-pairs.json", source.Reads);
        Assert.Contains("indexes/ru/three-cards.json", source.Reads);
        Assert.Single(source.Reads, path => path.StartsWith("content/", StringComparison.Ordinal));
        Assert.DoesNotContain(source.Reads, path => path.Contains("synthesis", StringComparison.Ordinal));
    }

    [Fact]
    public void ThreeCards_MissingOrCrossLocalePairDependencyBreaksReadyModule()
    {
        var missing = CompleteThreeCardSource();
        missing.Remove("indexes/ru/oriented-pairs.json");
        var missingResult = Resolver(missing).ResolveThreeCardPosition(
            Classic, new("ru"), TarotThreeCardPosition.Past, new("major.fool"), TarotCardOrientation.Upright);
        AssertBroken(missingResult, "index.missing");

        var mixed = new TarotInterpretationResolverTestSource();
        mixed.AddOrientedPairCorpus("ru", indexLocale: "en");
        mixed.AddThreeCardCorpus("ru");
        mixed.SetReady(TarotInterpretationMode.ThreeCards, "ru");
        mixed.PublishManifest();
        var mixedResult = Resolver(mixed).ResolveThreeCardPosition(
            Classic, new("ru"), TarotThreeCardPosition.Past, new("major.fool"), TarotCardOrientation.Upright);
        AssertBroken(mixedResult, "index.identity");
    }

    private static TarotInterpretationResolverTestSource CompleteThreeCardSource()
    {
        var source = new TarotInterpretationResolverTestSource();
        source.AddOrientedPairCorpus("ru");
        source.AddThreeCardCorpus("ru");
        source.SetReady(TarotInterpretationMode.ThreeCards, "ru");
        source.PublishManifest();
        return source;
    }

    private static void AssertBroken(
        TarotInterpretationResolution<TarotThreeCardPositionEntry> result,
        string code)
    {
        var absent = Assert.IsType<NoTarotInterpretationContent<TarotThreeCardPositionEntry>>(result);
        Assert.Equal(TarotNoContentReason.BrokenReadyModule, absent.Reason);
        Assert.Equal(code, absent.Diagnostic!.Code);
    }

    private static TarotInterpretationPackResolver Resolver(TarotInterpretationResolverTestSource source) =>
        new(source, StandardTarotCatalog.Deck);

    private static (TarotCardOrientation A, TarotCardOrientation B) Orientations(TarotOrientedPairState state) => state switch
    {
        TarotOrientedPairState.UprightUpright => (TarotCardOrientation.Upright, TarotCardOrientation.Upright),
        TarotOrientedPairState.UprightReversed => (TarotCardOrientation.Upright, TarotCardOrientation.Reversed),
        TarotOrientedPairState.ReversedUpright => (TarotCardOrientation.Reversed, TarotCardOrientation.Upright),
        TarotOrientedPairState.ReversedReversed => (TarotCardOrientation.Reversed, TarotCardOrientation.Reversed),
        _ => throw new ArgumentOutOfRangeException(nameof(state))
    };
}
