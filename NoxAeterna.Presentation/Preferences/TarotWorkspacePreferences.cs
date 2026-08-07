using NoxAeterna.Domain.Tarot;
using NoxAeterna.Presentation.Tarot;

namespace NoxAeterna.Presentation.Preferences;

/// <summary>Represents immutable user preferences for the Tarot workspace.</summary>
public sealed record TarotWorkspacePreferences(
    TarotSpreadId SpreadId,
    TarotArtworkPackId ArtworkPackId,
    TarotInterpretationPackId InterpretationPackId,
    TarotBackVariantId BackVariantId,
    bool AllowReversed,
    bool AutoRevealCards)
{
    /// <summary>Creates the explicit compiled defaults.</summary>
    public static TarotWorkspacePreferences CreateDefault() => new(
        StandardTarotSpreads.SingleCard.Id,
        TarotPrototypeSelections.DefaultArtworkPackId,
        TarotPrototypeSelections.InterpretationPackId,
        new TarotBackVariantId("black-sun"),
        AllowReversed: false,
        AutoRevealCards: true);
}
