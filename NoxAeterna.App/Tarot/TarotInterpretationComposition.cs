using NoxAeterna.Domain.Tarot;
using NoxAeterna.Interpretation.Tarot.Resolution;

namespace NoxAeterna.App.Tarot;

/// <summary>Holds the single App-owned built-in interpretation graph.</summary>
public sealed record TarotInterpretationComposition(
    BuiltInTarotInterpretationPackSourceCatalog SourceCatalog,
    TarotInterpretationPackCatalog PackCatalog,
    ITarotWorkspaceInterpretationResolver Resolver,
    ITarotSingleCardPresentationLabelSource PresentationLabels)
{
    public static TarotInterpretationComposition CreateBuiltIn()
    {
        var sourceCatalog = BuiltInTarotInterpretationPackSourceCatalog.CreateDefault();
        var packCatalog = new TarotInterpretationPackCatalog(sourceCatalog, sourceCatalog.PackIds);
        ITarotWorkspaceInterpretationResolver resolver = new TarotWorkspaceInterpretationResolverAdapter(
            new TarotInterpretationPackResolver(sourceCatalog, StandardTarotCatalog.Deck));
        ITarotSingleCardPresentationLabelSource labels = EmptyTarotSingleCardPresentationLabelSource.Instance;
#if DEBUG
        if (NoxAeterna.App.Debug.DebugTarotInterpretationPreview.TryCreate() is { } preview)
        {
            resolver = preview;
            labels = preview;
        }
#endif
        return new TarotInterpretationComposition(sourceCatalog, packCatalog, resolver, labels);
    }
}
