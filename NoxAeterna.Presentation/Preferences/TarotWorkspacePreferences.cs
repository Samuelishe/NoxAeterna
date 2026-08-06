using NoxAeterna.Domain.Tarot;
using NoxAeterna.Presentation.Tarot;

namespace NoxAeterna.Presentation.Preferences;

/// <summary>Represents immutable user preferences for the Tarot workspace.</summary>
public sealed record TarotWorkspacePreferences(
    TarotSpreadId SpreadId,
    TarotArtworkPackId ArtworkPackId,
    TarotBackVariantId BackVariantId,
    bool AllowReversed,
    bool AutoRevealCards)
{
    /// <summary>Creates the explicit foundation defaults.</summary>
    public static TarotWorkspacePreferences CreateDefault() => new(
        StandardTarotSpreads.SingleCard.Id,
        TarotPrototypeSelections.DefaultArtworkPackId,
        new TarotBackVariantId("black-sun"),
        AllowReversed: false,
        AutoRevealCards: true);
}
