using NoxAeterna.Domain.Tarot;
using NoxAeterna.Interpretation.Tarot.Resolution;

namespace NoxAeterna.App.Tarot;

/// <summary>Holds the single App-owned built-in interpretation graph.</summary>
public sealed record TarotInterpretationComposition(
    BuiltInTarotInterpretationPackStoreCatalog StoreCatalog,
    TarotInterpretationPackCatalog PackCatalog,
    ITarotWorkspaceInterpretationResolver Resolver,
    ITarotInterpretationPresentationLabelSource PresentationLabels)
{
    public static TarotInterpretationComposition CreateBuiltIn()
    {
        var storeCatalog = BuiltInTarotInterpretationPackStoreCatalog.CreateDefault();
        var packCatalog = new TarotInterpretationPackCatalog(storeCatalog, storeCatalog.PackIds);
        ITarotWorkspaceInterpretationResolver resolver = new TarotWorkspaceInterpretationResolverAdapter(
            new TarotInterpretationPackResolver(storeCatalog, StandardTarotCatalog.Deck));
        ITarotInterpretationPresentationLabelSource labels = new TarotPackagePresentationLabelSource(storeCatalog);
#if DEBUG
        if (NoxAeterna.App.Debug.DebugTarotInterpretationPreview.TryCreate() is { } preview)
        {
            resolver = preview;
            labels = preview;
        }
#endif
        return new TarotInterpretationComposition(storeCatalog, packCatalog, resolver, labels);
    }
}
