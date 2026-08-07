using System.Collections.ObjectModel;
using NoxAeterna.Domain.Tarot;
using NoxAeterna.Interpretation.Sqlite;
using NoxAeterna.Interpretation.Tarot.Contracts;
using NoxAeterna.Interpretation.Tarot.Storage;

namespace NoxAeterna.App.Tarot;

/// <summary>App-owned catalog registration for immutable built-in package files.</summary>
public sealed class BuiltInTarotInterpretationPackStoreCatalog : ITarotInterpretationPackStoreCatalog
{
    public const string ClassicPackageOutputPath = "resources/interpretation/tarot/packs/classic.noxinterp";
    private readonly IReadOnlyDictionary<TarotInterpretationPackId,ITarotInterpretationPackStore> stores;

    public BuiltInTarotInterpretationPackStoreCatalog(IEnumerable<ITarotInterpretationPackStore> stores)
    {
        ArgumentNullException.ThrowIfNull(stores);var materialized=new Dictionary<TarotInterpretationPackId,ITarotInterpretationPackStore>();
        foreach(var store in stores){ArgumentNullException.ThrowIfNull(store);if(!materialized.TryAdd(store.Manifest.PackId,store))throw new ArgumentException($"Duplicate interpretation pack store '{store.Manifest.PackId.Value}'.",nameof(stores));}
        this.stores=new ReadOnlyDictionary<TarotInterpretationPackId,ITarotInterpretationPackStore>(materialized);PackIds=Array.AsReadOnly(materialized.Keys.OrderBy(static id=>id.Value,StringComparer.Ordinal).ToArray());Diagnostics=[];
    }

    private BuiltInTarotInterpretationPackStoreCatalog(IReadOnlyDictionary<TarotInterpretationPackId,ITarotInterpretationPackStore> stores,IReadOnlyList<TarotInterpretationPackCatalogDiagnostic> diagnostics)
    {this.stores=stores;PackIds=Array.AsReadOnly(stores.Keys.OrderBy(static id=>id.Value,StringComparer.Ordinal).ToArray());Diagnostics=diagnostics;}

    public IReadOnlyList<TarotInterpretationPackId> PackIds{get;}
    public IReadOnlyList<TarotInterpretationPackCatalogDiagnostic> Diagnostics{get;}

    public static BuiltInTarotInterpretationPackStoreCatalog CreateDefault()=>Create(AppContext.BaseDirectory);
    public static BuiltInTarotInterpretationPackStoreCatalog Create(string applicationBaseDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(applicationBaseDirectory);var packId=new TarotInterpretationPackId("classic");var stores=new Dictionary<TarotInterpretationPackId,ITarotInterpretationPackStore>();var diagnostics=new List<TarotInterpretationPackCatalogDiagnostic>();
        var path=Path.GetFullPath(Path.Combine(applicationBaseDirectory,ClassicPackageOutputPath.Replace('/',Path.DirectorySeparatorChar)));
        if(TarotSqlitePackageStore.TryOpen(path,packId,StandardTarotCatalog.Deck.Id,out var store,out var diagnostic)&&store is not null)stores.Add(packId,store);
        else diagnostics.Add(new(packId,"package.unavailable",diagnostic??"The built-in interpretation package is unavailable."));
        return new(new ReadOnlyDictionary<TarotInterpretationPackId,ITarotInterpretationPackStore>(stores),Array.AsReadOnly(diagnostics.ToArray()));
    }

    public bool TryGetStore(TarotInterpretationPackId packId,out ITarotInterpretationPackStore? store){ArgumentNullException.ThrowIfNull(packId);return stores.TryGetValue(packId,out store);}
}
