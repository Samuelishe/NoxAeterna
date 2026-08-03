using NoxAeterna.Domain.Tarot;
using NoxAeterna.Presentation.Localization;

namespace NoxAeterna.Presentation.Tarot;

/// <summary>Represents one selectable spread and its localized label key.</summary>
public sealed record TarotSpreadOption(TarotSpreadDefinition Definition, LocalizationKey LabelKey);

/// <summary>Represents one selectable card-back variant and its localized label key.</summary>
public sealed record TarotBackVariantOption(TarotBackVariantId Id, LocalizationKey LabelKey);

/// <summary>Represents one selectable artwork pack and its localized display-name key.</summary>
public sealed record TarotArtworkPackOption(TarotArtworkPackId Id, LocalizationKey LabelKey);

/// <summary>Owns the honest prototype selections available during the first Tarot vertical slice.</summary>
public static class TarotPrototypeSelections
{
    /// <summary>Gets the built-in Classic prototype artwork-pack identity.</summary>
    public static TarotArtworkPackId ArtworkPackId { get; } = new("prototype-symbolic");

    /// <summary>Gets the real built-in artwork packs in stable user-facing display order.</summary>
    public static IReadOnlyList<TarotArtworkPackOption> ArtworkPacks { get; } = Array.AsReadOnly(
    [
        new TarotArtworkPackOption(
            ArtworkPackId,
            new LocalizationKey("ui.tarot.artwork.prototype-symbolic")),
        new TarotArtworkPackOption(
            new TarotArtworkPackId("lupus-noctis"),
            new LocalizationKey("ui.tarot.artwork.lupus-noctis"))
    ]);

    /// <summary>Gets the single built-in prototype presentation-skin identity.</summary>
    public static TarotPresentationSkinId PresentationSkinId { get; } = new("astral-archive-prototype");

    /// <summary>Gets the foundation interpretation-set identity, which currently contains no prose.</summary>
    public static TarotInterpretationSetId InterpretationSetId { get; } = new("foundation");

    /// <summary>Gets the two programmatic card-back choices.</summary>
    public static IReadOnlyList<TarotBackVariantOption> BackVariants { get; } = Array.AsReadOnly(
    [
        new TarotBackVariantOption(
            new TarotBackVariantId("black-sun"),
            new LocalizationKey("ui.tarot.back.black-sun")),
        new TarotBackVariantOption(
            new TarotBackVariantId("lunar-seal"),
            new LocalizationKey("ui.tarot.back.lunar-seal"))
    ]);
}
