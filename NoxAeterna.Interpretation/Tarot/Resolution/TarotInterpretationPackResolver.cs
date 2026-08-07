using NoxAeterna.Domain.Tarot;
using NoxAeterna.Interpretation.Tarot.Contracts;
using NoxAeterna.Interpretation.Tarot.Keys;
using NoxAeterna.Interpretation.Tarot.Storage;

namespace NoxAeterna.Interpretation.Tarot.Resolution;

/// <summary>Resolves validated semantic entries through immutable package stores.</summary>
public sealed class TarotInterpretationPackResolver
{
    private static readonly TarotInterpretationLocale English = new("en");
    private static readonly TarotInterpretationLocale Russian = new("ru");
    private readonly ITarotInterpretationPackStoreCatalog catalog;
    private readonly TarotDeckDefinition semanticDeck;
    private readonly TarotBoundedLruCache<ContentCacheKey, object> entries;

    public TarotInterpretationPackResolver(ITarotInterpretationPackStoreCatalog catalog, TarotDeckDefinition semanticDeck, TarotInterpretationResolverOptions? options = null)
    {
        this.catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        this.semanticDeck = semanticDeck ?? throw new ArgumentNullException(nameof(semanticDeck));
        entries = new((options ?? new()).EntryCapacity);
    }

    public TarotInterpretationResolution<TarotSingleCardEntry> ResolveSingleCard(TarotInterpretationPackId packId,TarotInterpretationLocale requestedLocale,TarotCardId cardId,TarotCardOrientation orientation)
    {
        ArgumentNullException.ThrowIfNull(cardId);
        var prepared=Prepare(packId,requestedLocale,TarotInterpretationMode.SingleCard);if(prepared.Context is null)return prepared.NoContent<TarotSingleCardEntry>();
        var context=prepared.Context;var key=TarotInterpretationKeys.CreateSingleCard(cardId,orientation);
        return Resolve(context,TarotInterpretationCorpus.SingleCard,key,()=>context.Store.GetSingleCard(context.ResolvedLocale,cardId,orientation));
    }

    public TarotInterpretationResolution<TarotOrientedPairEntry> ResolveOrientedPair(TarotInterpretationPackId packId,TarotInterpretationMode modeId,TarotInterpretationLocale requestedLocale,TarotCardId firstCardId,TarotCardOrientation firstOrientation,TarotCardId secondCardId,TarotCardOrientation secondOrientation)
    {
        ArgumentNullException.ThrowIfNull(firstCardId);ArgumentNullException.ThrowIfNull(secondCardId);
        if(modeId is not(TarotInterpretationMode.TwoCards or TarotInterpretationMode.ThreeCards))return NoContent<TarotOrientedPairEntry>(TarotNoContentReason.UnsupportedMode,"request.unsupported-mode","Oriented-pair resolution supports only two-cards and three-cards.");
        var canonical=TarotInterpretationKeys.CanonicalizePair(firstCardId,firstOrientation,secondCardId,secondOrientation);if(!canonical.IsValid||canonical.Value is null)return NoContent<TarotOrientedPairEntry>(TarotNoContentReason.ValidationFailed,"request.invalid-pair",canonical.Diagnostics.FirstOrDefault()?.Message??"The requested pair is invalid.");
        var pair=canonical.Value;var prepared=Prepare(packId,requestedLocale,modeId);if(prepared.Context is null)return prepared.NoContent<TarotOrientedPairEntry>();
        var context=prepared.Context;var key=TarotInterpretationKeys.CreateOrientedPair(pair.CardAId,pair.CardBId,pair.OrientationState);
        return Resolve(context,TarotInterpretationCorpus.OrientedPairs,key,()=>context.Store.GetOrientedPair(context.ResolvedLocale,pair.CardAId,pair.CardBId,pair.OrientationState));
    }

    public TarotInterpretationResolution<TarotThreeCardPositionEntry> ResolveThreeCardPosition(TarotInterpretationPackId packId,TarotInterpretationLocale requestedLocale,TarotThreeCardPosition position,TarotCardId cardId,TarotCardOrientation orientation)
    {
        ArgumentNullException.ThrowIfNull(cardId);var prepared=Prepare(packId,requestedLocale,TarotInterpretationMode.ThreeCards);if(prepared.Context is null)return prepared.NoContent<TarotThreeCardPositionEntry>();
        var context=prepared.Context;var key=TarotInterpretationKeys.CreateThreeCardPosition(position,cardId,orientation);
        return Resolve(context,TarotInterpretationCorpus.ThreeCards,key,()=>context.Store.GetThreeCardPosition(context.ResolvedLocale,position,cardId,orientation));
    }

    public TarotInterpretationResolution<TarotResolvedModuleSnapshot> ResolveMode(TarotInterpretationPackId packId,TarotInterpretationMode modeId,TarotInterpretationLocale requestedLocale)
    {
        if(!Enum.IsDefined(modeId))return NoContent<TarotResolvedModuleSnapshot>(TarotNoContentReason.UnsupportedMode,"request.unsupported-mode","The requested interpretation mode is not defined.");
        var prepared=Prepare(packId,requestedLocale,modeId);if(prepared.Context is null)return prepared.NoContent<TarotResolvedModuleSnapshot>();var context=prepared.Context;
        return new ResolvedTarotInterpretation<TarotResolvedModuleSnapshot>(context.Store.Manifest.PackId,context.Store.Manifest.ContentVersion,context.Mode,context.RequestedLocale,context.ResolvedLocale,new(context.Corpora));
    }

    public void InvalidatePack(TarotInterpretationPackId packId){ArgumentNullException.ThrowIfNull(packId);entries.RemoveWhere(key=>key.PackId==packId);}
    public void Clear()=>entries.Clear();

    private PreparationResult Prepare(TarotInterpretationPackId packId,TarotInterpretationLocale requestedLocale,TarotInterpretationMode mode)
    {
        ArgumentNullException.ThrowIfNull(packId);ArgumentNullException.ThrowIfNull(requestedLocale);
        if(!catalog.TryGetStore(packId,out var store)||store is null)return PreparationResult.Failed(TarotNoContentReason.PackUnavailable,"store.pack-unavailable","The requested interpretation package is unavailable.");
        if(store.Manifest.PackId!=packId||store.Manifest.SemanticDeckId!=semanticDeck.Id)return PreparationResult.Failed(TarotNoContentReason.PackUnavailable,"store.identity","The package identity does not match the selected pack or semantic deck.");
        foreach(var locale in LocaleChain(requestedLocale))
        {
            if(!store.Manifest.DeclaredLocales.Contains(locale))continue;
            if(!store.Manifest.Modules.TryGetValue(mode,out var byLocale)||!byLocale.TryGetValue(locale,out var module))return PreparationResult.Failed(TarotNoContentReason.PackUnavailable,"store.module-missing","Package metadata is missing a mode/locale declaration.");
            if(!module.Ready)continue;
            var validation=store.ValidateReadyModule(locale,mode);if(validation.Status!=TarotInterpretationStoreStatus.Found||validation.Value is null)return PreparationResult.Failed(TarotNoContentReason.BrokenReadyModule,validation.Diagnostic?.Code??"store.ready-incomplete",validation.Diagnostic?.Message??"The ready module is incomplete.");
            return PreparationResult.Resolved(new(store,mode,requestedLocale,locale,validation.Value));
        }
        return PreparationResult.Failed(TarotNoContentReason.NoReadyLocale,"locale.no-ready-module","No locale in the resolution chain declares this mode ready.");
    }

    private TarotInterpretationResolution<T> Resolve<T>(ResolutionContext context,TarotInterpretationCorpus corpus,string key,Func<TarotInterpretationStoreResult<T>> lookup) where T:class
    {
        var cacheKey=new ContentCacheKey(context.Store.Manifest.PackId,context.Store.Manifest.ContentVersion,context.Store.SourceDigest.Value,context.ResolvedLocale.Value,context.Mode,corpus,key);
        if(entries.TryGetValue(cacheKey,out var cached)&&cached is T value)return Resolved(context,value);
        var result=lookup();if(result.Status!=TarotInterpretationStoreStatus.Found||result.Value is null)return NoContent<T>(TarotNoContentReason.BrokenReadyModule,result.Diagnostic?.Code??"store.row-missing",result.Diagnostic?.Message??"A required ready-module row is missing.");
        entries.Set(cacheKey,result.Value);return Resolved(context,result.Value);
    }

    private static ResolvedTarotInterpretation<T> Resolved<T>(ResolutionContext context,T content) where T:class=>new(context.Store.Manifest.PackId,context.Store.Manifest.ContentVersion,context.Mode,context.RequestedLocale,context.ResolvedLocale,content);
    private static NoTarotInterpretationContent<T> NoContent<T>(TarotNoContentReason reason,string code,string message) where T:class=>new(reason,new(code,message));
    private static IReadOnlyList<TarotInterpretationLocale> LocaleChain(TarotInterpretationLocale requested)=>new[]{requested,English,Russian}.Distinct().ToArray();
    private sealed record ContentCacheKey(TarotInterpretationPackId PackId,int ContentVersion,string SourceDigest,string Locale,TarotInterpretationMode Mode,TarotInterpretationCorpus Corpus,string Key);
    private sealed record ResolutionContext(ITarotInterpretationPackStore Store,TarotInterpretationMode Mode,TarotInterpretationLocale RequestedLocale,TarotInterpretationLocale ResolvedLocale,IReadOnlyList<TarotInterpretationCorpus> Corpora);
    private sealed class PreparationResult
    {
        private PreparationResult(ResolutionContext? context,TarotNoContentReason? reason,string? code,string? message)=>(Context,Reason,Code,Message)=(context,reason,code,message);
        public ResolutionContext? Context{get;}private TarotNoContentReason? Reason{get;}private string? Code{get;}private string? Message{get;}
        public static PreparationResult Resolved(ResolutionContext context)=>new(context,null,null,null);
        public static PreparationResult Failed(TarotNoContentReason reason,string code,string message)=>new(null,reason,code,message);
        public NoTarotInterpretationContent<T> NoContent<T>() where T:class=>TarotInterpretationPackResolver.NoContent<T>(Reason!.Value,Code!,Message!);
    }
}
