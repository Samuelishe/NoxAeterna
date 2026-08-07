using NoxAeterna.Domain.Tarot;
using NoxAeterna.Interpretation.Tarot.Resolution;

namespace NoxAeterna.App.Tarot;

/// <summary>Holds the single App-owned built-in interpretation graph.</summary>
public sealed record TarotInterpretationComposition(
    BuiltInTarotInterpretationPackSourceCatalog SourceCatalog,
    TarotInterpretationPackCatalog PackCatalog,
    ITarotWorkspaceInterpretationResolver Resolver)
{
    public static TarotInterpretationComposition CreateBuiltIn()
    {
        var sourceCatalog = BuiltInTarotInterpretationPackSourceCatalog.CreateDefault();
        var packCatalog = new TarotInterpretationPackCatalog(sourceCatalog, sourceCatalog.PackIds);
        var resolver = new TarotWorkspaceInterpretationResolverAdapter(
            new TarotInterpretationPackResolver(sourceCatalog, StandardTarotCatalog.Deck));
        return new TarotInterpretationComposition(sourceCatalog, packCatalog, resolver);
    }
}
