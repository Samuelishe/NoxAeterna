using NoxAeterna.Domain.Tarot;
using NoxAeterna.Presentation.Localization;

namespace NoxAeterna.Presentation.Tarot;

/// <summary>Represents one selectable spread and its localized label key.</summary>
public sealed record TarotSpreadOption(TarotSpreadDefinition Definition, LocalizationKey LabelKey);

/// <summary>Represents one selectable card-back variant and its localized label key.</summary>
public sealed record TarotBackVariantOption(TarotBackVariantId Id, LocalizationKey LabelKey);

/// <summary>Represents one selectable artwork pack and its localized display-name key.</summary>
public sealed record TarotArtworkPackOption(TarotArtworkPackId Id, LocalizationKey LabelKey);

/// <summary>Represents one selectable interpretation pack by stable semantic identity.</summary>
public sealed record TarotInterpretationPackOption(TarotInterpretationPackId Id);

/// <summary>Owns the built-in Tarot selection identities used by the current workspace.</summary>
public static class TarotPrototypeSelections
{
    /// <summary>Gets the internal programmatic prototype artwork identity.</summary>
    public static TarotArtworkPackId PrototypeArtworkPackId { get; } = new("prototype-symbolic");

    /// <summary>Gets the sole user-facing built-in artwork-pack identity.</summary>
    public static TarotArtworkPackId LupusNoctisArtworkPackId { get; } = new("lupus-noctis");

    /// <summary>Gets the explicit default user-facing artwork-pack identity.</summary>
    public static TarotArtworkPackId DefaultArtworkPackId { get; } = LupusNoctisArtworkPackId;

    /// <summary>Gets the real built-in artwork packs in stable user-facing display order.</summary>
    public static IReadOnlyList<TarotArtworkPackOption> ArtworkPacks { get; } = Array.AsReadOnly(
    [
        new TarotArtworkPackOption(
            LupusNoctisArtworkPackId,
            new LocalizationKey("ui.tarot.artwork.lupus-noctis"))
    ]);

    /// <summary>Gets the single built-in prototype presentation-skin identity.</summary>
    public static TarotPresentationSkinId PresentationSkinId { get; } = new("astral-archive-prototype");

    /// <summary>Gets the Classic interpretation-pack identity, which currently contains no prose.</summary>
    public static TarotInterpretationPackId InterpretationPackId { get; } = new("classic");

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
