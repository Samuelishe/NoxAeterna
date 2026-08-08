using NoxAeterna.Domain.Tarot;
using NoxAeterna.Interpretation.Tarot.Contracts;
using NoxAeterna.Interpretation.Tarot.Resolution;
using NoxAeterna.Interpretation.Tarot.Storage;

namespace NoxAeterna.Tests.Interpretation.Tarot;

public sealed class TarotPackageStoreResolverTests
{
    [Theory]
    [InlineData("ru","ru")]
    [InlineData("en","en")]
    [InlineData("zh","en")]
    public void RequestedEnglishRussianFallbackOrderIsPreserved(string requested,string expected)
    {
        var store=new FakeStore(Manifest(ruReady:true,enReady:true));var resolver=Resolver(store);
        var result=Assert.IsType<ResolvedTarotInterpretation<TarotSingleCardEntry>>(resolver.ResolveSingleCard(new("classic"),new(requested),new("major.fool"),TarotCardOrientation.Upright));
        Assert.Equal(expected,result.ResolvedLocale.Value);Assert.Equal(expected,store.LastLocale!.Value);
    }

    [Fact]
    public void RequestedUnavailableLocaleFallsThroughEnglishThenRussian()
    {
        var store=new FakeStore(Manifest(ruReady:true,enReady:false));
        var result=Assert.IsType<ResolvedTarotInterpretation<TarotSingleCardEntry>>(Resolver(store).ResolveSingleCard(new("classic"),new("zh"),new("major.fool"),TarotCardOrientation.Upright));
        Assert.Equal("ru",result.ResolvedLocale.Value);
    }

    [Fact]
    public void BrokenReadyEnglishStopsBeforeValidRussian()
    {
        var store=new FakeStore(Manifest(ruReady:true,enReady:true)){BrokenLocale=new("en")};
        var result=Assert.IsType<NoTarotInterpretationContent<TarotSingleCardEntry>>(Resolver(store).ResolveSingleCard(new("classic"),new("zh"),new("major.fool"),TarotCardOrientation.Upright));
        Assert.Equal(TarotNoContentReason.BrokenReadyModule,result.Reason);Assert.Null(store.LastLocale);
    }

    [Fact]
    public void NoReadyLocaleAndUnknownPackageRemainDistinctTypedAbsence()
    {
        var noReady=Assert.IsType<NoTarotInterpretationContent<TarotSingleCardEntry>>(Resolver(new FakeStore(Manifest(false,false))).ResolveSingleCard(new("classic"),new("ru"),new("major.fool"),TarotCardOrientation.Upright));
        var unavailable=Assert.IsType<NoTarotInterpretationContent<TarotSingleCardEntry>>(new TarotInterpretationPackResolver(new FakeCatalog(),StandardTarotCatalog.Deck).ResolveSingleCard(new("missing"),new("ru"),new("major.fool"),TarotCardOrientation.Upright));
        Assert.Equal(TarotNoContentReason.NoReadyLocale,noReady.Reason);Assert.Equal(TarotNoContentReason.PackUnavailable,unavailable.Reason);
    }

    [Fact]
    public void MissingReadyRowIsBrokenReadyAndCanonicalPairSlotsArePreserved()
    {
        var missingStore=new FakeStore(Manifest(true,false)){MissingEntry=true};var missing=Assert.IsType<NoTarotInterpretationContent<TarotSingleCardEntry>>(Resolver(missingStore).ResolveSingleCard(new("classic"),new("ru"),new("major.fool"),TarotCardOrientation.Upright));Assert.Equal(TarotNoContentReason.BrokenReadyModule,missing.Reason);
        var pairStore=new FakeStore(Manifest(false,false,twoReady:true));var pair=Assert.IsType<ResolvedTarotInterpretation<TarotOrientedPairEntry>>(Resolver(pairStore).ResolveOrientedPair(new("classic"),TarotInterpretationMode.TwoCards,new("ru"),new("major.world"),TarotCardOrientation.Reversed,new("major.tower"),TarotCardOrientation.Upright));
        Assert.Equal("major.tower",pair.Content.CardAId.Value);Assert.Equal("major.world",pair.Content.CardBId.Value);Assert.Equal(TarotOrientedPairState.UprightReversed,pair.Content.OrientationState);
    }

    [Fact]
    public void ThreeCardPairResolutionUsesThreeCardsModeAndPreservesCanonicalCardOrientations()
    {
        var store = new FakeStore(Manifest(false, false, threeReady: true));

        var result = Assert.IsType<ResolvedTarotInterpretation<TarotOrientedPairEntry>>(
            Resolver(store).ResolveOrientedPair(
                new("classic"),
                TarotInterpretationMode.ThreeCards,
                new("ru"),
                new("major.world"),
                TarotCardOrientation.Reversed,
                new("major.tower"),
                TarotCardOrientation.Upright));

        Assert.Equal(TarotInterpretationMode.ThreeCards, result.ModeId);
        Assert.Equal(TarotInterpretationMode.ThreeCards, store.LastValidatedMode);
        Assert.Equal("major.tower", result.Content.CardAId.Value);
        Assert.Equal("major.world", result.Content.CardBId.Value);
        Assert.Equal(TarotOrientedPairState.UprightReversed, result.Content.OrientationState);
    }

    [Fact]
    public void ThreeCardSynthesisResourceUsesReadyFallbackExactIdentityAndBoundedEntryCache()
    {
        var store = new FakeStore(Manifest(false, false, threeReady: true));
        var resolver = Resolver(store);
        var resourceId = new TarotSynthesisResourceId(TarotThreeCardSynthesisContract.Improving);

        var first = Assert.IsType<ResolvedTarotInterpretation<TarotSynthesisResource>>(
            resolver.ResolveThreeCardSynthesisResource(
                new("classic"),
                new("en"),
                TarotSynthesisResourceType.TrajectoryProfile,
                resourceId));
        var cached = Assert.IsType<ResolvedTarotInterpretation<TarotSynthesisResource>>(
            resolver.ResolveThreeCardSynthesisResource(
                new("classic"),
                new("en"),
                TarotSynthesisResourceType.TrajectoryProfile,
                resourceId));

        Assert.Equal(TarotInterpretationMode.ThreeCards, first.ModeId);
        Assert.Equal("en", first.RequestedLocale.Value);
        Assert.Equal("ru", first.ResolvedLocale.Value);
        Assert.Equal(TarotSynthesisResourceType.TrajectoryProfile, first.Content.ResourceType);
        Assert.Equal(resourceId, first.Content.ResourceId);
        Assert.Equal(first.Content, cached.Content);
        Assert.Equal(1, store.SynthesisLookups);
        Assert.Equal(
            (TarotSynthesisResourceType.TrajectoryProfile, resourceId),
            store.LastSynthesisIdentity);
    }

    [Fact]
    public void MissingExactSynthesisRowInReadyThreeCardsModuleIsBrokenReady()
    {
        var store = new FakeStore(Manifest(false, false, threeReady: true))
        {
            MissingSynthesis = true
        };

        var result = Assert.IsType<NoTarotInterpretationContent<TarotSynthesisResource>>(
            Resolver(store).ResolveThreeCardSynthesisResource(
                new("classic"),
                new("ru"),
                TarotSynthesisResourceType.SynthesisFragment,
                new(TarotThreeCardSynthesisContract.MixedTransitions)));

        Assert.Equal(TarotNoContentReason.BrokenReadyModule, result.Reason);
        Assert.Equal("store.row-missing", result.Diagnostic?.Code);
    }

    [Fact]
    public void EntryCacheUsesSemanticIdentityAndCanBeInvalidated()
    {
        var store=new FakeStore(Manifest(true,false));var resolver=Resolver(store);
        resolver.ResolveSingleCard(new("classic"),new("ru"),new("major.fool"),TarotCardOrientation.Upright);resolver.ResolveSingleCard(new("classic"),new("ru"),new("major.fool"),TarotCardOrientation.Upright);Assert.Equal(1,store.EntryLookups);
        resolver.InvalidatePack(new("classic"));resolver.ResolveSingleCard(new("classic"),new("ru"),new("major.fool"),TarotCardOrientation.Upright);Assert.Equal(2,store.EntryLookups);
        Assert.Throws<ArgumentOutOfRangeException>(()=>new TarotInterpretationResolverOptions(0));
    }

    [Fact]
    public void SqlDoesNotLeakAboveTheSqliteAdapterBoundary()
    {
        var root=Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,"..","..","..",".."));var owners=new[]{"NoxAeterna.Interpretation","NoxAeterna.Presentation","NoxAeterna.App"};
        var source=string.Join(Environment.NewLine,owners.SelectMany(owner=>Directory.GetFiles(Path.Combine(root,owner),"*.cs",SearchOption.AllDirectories)).Where(path=>!path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}",StringComparison.Ordinal)).Select(File.ReadAllText));
        Assert.DoesNotContain("Microsoft.Data.Sqlite",source,StringComparison.Ordinal);Assert.DoesNotContain("SELECT * FROM",source,StringComparison.OrdinalIgnoreCase);
    }

    private static TarotInterpretationPackResolver Resolver(FakeStore store)=>new(new FakeCatalog(store),StandardTarotCatalog.Deck,new(4));
    private static TarotInterpretationPackManifest Manifest(bool ruReady,bool enReady,bool twoReady=false,bool threeReady=false)
    {
        var locales=new[]{new TarotInterpretationLocale("ru"),new TarotInterpretationLocale("en")};var modules=new Dictionary<TarotInterpretationMode,IReadOnlyDictionary<TarotInterpretationLocale,TarotInterpretationModule>>();
        foreach(var mode in Enum.GetValues<TarotInterpretationMode>())modules[mode]=locales.ToDictionary(locale=>locale,locale=>new TarotInterpretationModule(mode switch{TarotInterpretationMode.SingleCard=>locale.Value=="ru"?ruReady:enReady,TarotInterpretationMode.TwoCards=>twoReady,TarotInterpretationMode.ThreeCards=>threeReady&&locale.Value=="ru",_=>false},mode switch{TarotInterpretationMode.TwoCards=>[TarotModuleDependency.OrientedPairs],TarotInterpretationMode.ThreeCards=>[TarotModuleDependency.OrientedPairs,TarotModuleDependency.ThreeCardPositions,TarotModuleDependency.ThreeCardSynthesis],_=>[]}));
        return new(new("classic"),StandardTarotCatalog.Deck.Id,new("ru"),1,locales,new Dictionary<TarotInterpretationLocale,string>{{new("ru"),"Классика"},{new("en"),"Classic"}},modules);
    }

    private sealed class FakeCatalog(params ITarotInterpretationPackStore[] stores):ITarotInterpretationPackStoreCatalog
    {private readonly Dictionary<TarotInterpretationPackId,ITarotInterpretationPackStore> items=stores.ToDictionary(static store=>store.Manifest.PackId);public bool TryGetStore(TarotInterpretationPackId packId,out ITarotInterpretationPackStore? store)=>items.TryGetValue(packId,out store);}
    private sealed class FakeStore(TarotInterpretationPackManifest manifest):ITarotInterpretationPackStore
    {
        public TarotInterpretationPackManifest Manifest{get;}=manifest;public TarotSha256 SourceDigest{get;}=new(new string('a',64));public TarotInterpretationLocale? BrokenLocale{get;init;}public bool MissingEntry{get;init;}public bool MissingSynthesis{get;init;}public TarotInterpretationLocale? LastLocale{get;private set;}public TarotInterpretationMode? LastValidatedMode{get;private set;}public int EntryLookups{get;private set;}public int SynthesisLookups{get;private set;}public (TarotSynthesisResourceType Type,TarotSynthesisResourceId Id)? LastSynthesisIdentity{get;private set;}
        public TarotInterpretationStoreResult<IReadOnlyList<TarotInterpretationCorpus>> ValidateReadyModule(TarotInterpretationLocale locale,TarotInterpretationMode mode){LastValidatedMode=mode;return locale==BrokenLocale?TarotInterpretationStoreResult<IReadOnlyList<TarotInterpretationCorpus>>.Missing():TarotInterpretationStoreResult<IReadOnlyList<TarotInterpretationCorpus>>.Found(Array.AsReadOnly(mode switch{TarotInterpretationMode.SingleCard=>[TarotInterpretationCorpus.SingleCard],TarotInterpretationMode.TwoCards=>[TarotInterpretationCorpus.OrientedPairs],TarotInterpretationMode.ThreeCards=>[TarotInterpretationCorpus.OrientedPairs,TarotInterpretationCorpus.ThreeCards],_=>Array.Empty<TarotInterpretationCorpus>()}));}
        public TarotInterpretationStoreResult<TarotSingleCardEntry> GetSingleCard(TarotInterpretationLocale locale,TarotCardId cardId,TarotCardOrientation orientation){EntryLookups++;if(MissingEntry)return TarotInterpretationStoreResult<TarotSingleCardEntry>.Missing();LastLocale=locale;return TarotInterpretationStoreResult<TarotSingleCardEntry>.Found(new(cardId,orientation,Sections(),[],0,2,orientation==TarotCardOrientation.Reversed?[TarotReversalMechanism.Blocked]:[]));}
        public TarotInterpretationStoreResult<TarotOrientedPairEntry> GetOrientedPair(TarotInterpretationLocale locale,TarotCardId cardAId,TarotCardId cardBId,TarotOrientedPairState state){LastLocale=locale;return TarotInterpretationStoreResult<TarotOrientedPairEntry>.Found(new(cardAId,cardBId,state,"Interaction","Direction",[],0,2));}
        public TarotInterpretationStoreResult<TarotThreeCardPositionEntry> GetThreeCardPosition(TarotInterpretationLocale locale,TarotThreeCardPosition position,TarotCardId cardId,TarotCardOrientation orientation)=>TarotInterpretationStoreResult<TarotThreeCardPositionEntry>.Found(new(position,cardId,orientation,"Position",[],0,2));
        public TarotInterpretationStoreResult<TarotLocalizedInterpretationLabels> GetLabels(TarotInterpretationLocale locale)=>TarotInterpretationStoreResult<TarotLocalizedInterpretationLabels>.Missing();
        public TarotInterpretationStoreResult<TarotSynthesisResource> GetSynthesisResource(TarotInterpretationLocale locale,TarotSynthesisResourceType resourceType,TarotSynthesisResourceId resourceId){SynthesisLookups++;LastLocale=locale;LastSynthesisIdentity=(resourceType,resourceId);return MissingSynthesis?TarotInterpretationStoreResult<TarotSynthesisResource>.Missing():TarotInterpretationStoreResult<TarotSynthesisResource>.Found(new(resourceType,resourceId,"Synthesis text","{\"text\":\"Synthesis text\"}\n"));}
        private static IReadOnlyDictionary<string,string> Sections()=>new Dictionary<string,string>(StringComparer.Ordinal){{"situation","Situation"},{"development","Development"},{"risk","Risk"},{"outcome","Outcome"},{"advice","Advice"}};
    }
}
