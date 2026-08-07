using System.Collections.ObjectModel;
using NoxAeterna.Domain.Tarot;
using NoxAeterna.Interpretation.Tarot.Contracts;
using NoxAeterna.Interpretation.Tarot.Storage;
using NoxAeterna.Presentation.Localization;
using NoxAeterna.Presentation.Tarot;

namespace NoxAeterna.App.Tarot;

public sealed record TarotInterpretationPackCatalogDiagnostic(TarotInterpretationPackId PackId,string Code,string Message);

/// <summary>Materializes stable selector options from validated package metadata.</summary>
public sealed class TarotInterpretationPackCatalog
{
    private static readonly TarotInterpretationLocale English=new("en");private static readonly TarotInterpretationLocale Russian=new("ru");
    private readonly IReadOnlyDictionary<TarotInterpretationPackId,IReadOnlyDictionary<TarotInterpretationLocale,string>> displayNames;

    public TarotInterpretationPackCatalog(ITarotInterpretationPackStoreCatalog catalog,IEnumerable<TarotInterpretationPackId> packIds)
    {
        ArgumentNullException.ThrowIfNull(catalog);ArgumentNullException.ThrowIfNull(packIds);var ids=packIds.ToArray();if(ids.Distinct().Count()!=ids.Length)throw new ArgumentException("Interpretation pack IDs must be unique.",nameof(packIds));
        var names=new Dictionary<TarotInterpretationPackId,IReadOnlyDictionary<TarotInterpretationLocale,string>>();var diagnostics=new List<TarotInterpretationPackCatalogDiagnostic>();
        foreach(var id in ids){if(!catalog.TryGetStore(id,out var store)||store is null){diagnostics.Add(new(id,"package.missing","The interpretation package is unavailable."));continue;}if(store.Manifest.PackId!=id){diagnostics.Add(new(id,"package.identity","The package identity does not match catalog registration."));continue;}names.Add(id,store.Manifest.DisplayNames);}
        displayNames=new ReadOnlyDictionary<TarotInterpretationPackId,IReadOnlyDictionary<TarotInterpretationLocale,string>>(names);Options=Array.AsReadOnly(names.Keys.Select(static id=>new TarotInterpretationPackOption(id)).ToArray());AvailablePackIds=Array.AsReadOnly(names.Keys.ToArray());Diagnostics=Array.AsReadOnly(diagnostics.ToArray());
    }
    public IReadOnlyList<TarotInterpretationPackOption> Options{get;}public IReadOnlyList<TarotInterpretationPackId> AvailablePackIds{get;}public IReadOnlyList<TarotInterpretationPackCatalogDiagnostic> Diagnostics{get;}
    public string ResolveDisplayName(TarotInterpretationPackId packId,LanguageCode uiLanguage)
    {ArgumentNullException.ThrowIfNull(packId);if(!displayNames.TryGetValue(packId,out var names))return packId.Value;TarotInterpretationLocale? requested=null;try{requested=new(uiLanguage.Value);}catch(ArgumentException){}foreach(var locale in new[]{requested,English,Russian}.Where(static item=>item is not null).Distinct())if(names.TryGetValue(locale!,out var name)&&!string.IsNullOrWhiteSpace(name))return name;return packId.Value;}
}
