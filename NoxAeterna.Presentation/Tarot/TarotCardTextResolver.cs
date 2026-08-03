using System.Globalization;
using NoxAeterna.Domain.Tarot;
using NoxAeterna.Presentation.Localization;

namespace NoxAeterna.Presentation.Tarot;

/// <summary>Resolves localized Tarot presentation text without placing display names in Domain.</summary>
public static class TarotCardTextResolver
{
    /// <summary>Resolves the localized card display name.</summary>
    public static string GetCardName(
        TarotCardDefinition card,
        ILocalizationProvider localizationProvider,
        LanguageCode language)
    {
        ArgumentNullException.ThrowIfNull(card);
        ArgumentNullException.ThrowIfNull(localizationProvider);

        if (card.Arcana == TarotArcana.Major)
        {
            return Localize(localizationProvider, language, $"ui.tarot.card.{card.Id.Value}");
        }

        var format = Localize(localizationProvider, language, "ui.tarot.card.minor.format");
        var rank = GetRankName(card.Rank!.Value, localizationProvider, language);
        var suit = GetSuitName(card.Suit!.Value, localizationProvider, language);
        return string.Format(GetCulture(language), format, rank, suit);
    }

    /// <summary>Resolves a localized spread-position name.</summary>
    public static string GetPositionName(
        TarotSpreadPositionId positionId,
        ILocalizationProvider localizationProvider,
        LanguageCode language) =>
        Localize(localizationProvider, language, $"ui.tarot.position.{positionId.Value}");

    /// <summary>Resolves a localized card orientation.</summary>
    public static string GetOrientationName(
        TarotCardOrientation orientation,
        ILocalizationProvider localizationProvider,
        LanguageCode language) =>
        Localize(localizationProvider, language, orientation switch
        {
            TarotCardOrientation.Upright => "ui.tarot.orientation.upright",
            TarotCardOrientation.Reversed => "ui.tarot.orientation.reversed",
            _ => throw new ArgumentOutOfRangeException(nameof(orientation), orientation, "Unknown Tarot orientation.")
        });

    /// <summary>Resolves a localized Arcana family name.</summary>
    public static string GetArcanaName(
        TarotArcana arcana,
        ILocalizationProvider localizationProvider,
        LanguageCode language) =>
        Localize(localizationProvider, language, arcana switch
        {
            TarotArcana.Major => "ui.tarot.arcana.major",
            TarotArcana.Minor => "ui.tarot.arcana.minor",
            _ => throw new ArgumentOutOfRangeException(nameof(arcana), arcana, "Unknown Tarot Arcana.")
        });

    /// <summary>Resolves a localized Minor Arcana suit name.</summary>
    public static string GetSuitName(
        TarotSuit suit,
        ILocalizationProvider localizationProvider,
        LanguageCode language) =>
        Localize(localizationProvider, language, $"ui.tarot.suit.{GetSuitSegment(suit)}");

    /// <summary>Resolves a localized Minor Arcana rank name.</summary>
    public static string GetRankName(
        TarotRank rank,
        ILocalizationProvider localizationProvider,
        LanguageCode language) =>
        Localize(localizationProvider, language, $"ui.tarot.rank.{GetRankSegment(rank)}");

    private static string Localize(
        ILocalizationProvider localizationProvider,
        LanguageCode language,
        string key) =>
        localizationProvider.Get(LocalizationScope.Ui, language, new LocalizationKey(key)).Text;

    private static CultureInfo GetCulture(LanguageCode language)
    {
        try
        {
            return CultureInfo.GetCultureInfo(language.Value);
        }
        catch (CultureNotFoundException)
        {
            return CultureInfo.InvariantCulture;
        }
    }

    private static string GetSuitSegment(TarotSuit suit) => suit switch
    {
        TarotSuit.Wands => "wands",
        TarotSuit.Cups => "cups",
        TarotSuit.Swords => "swords",
        TarotSuit.Pentacles => "pentacles",
        _ => throw new ArgumentOutOfRangeException(nameof(suit), suit, "Unknown Tarot suit.")
    };

    private static string GetRankSegment(TarotRank rank) => rank switch
    {
        TarotRank.Ace => "ace",
        TarotRank.Two => "two",
        TarotRank.Three => "three",
        TarotRank.Four => "four",
        TarotRank.Five => "five",
        TarotRank.Six => "six",
        TarotRank.Seven => "seven",
        TarotRank.Eight => "eight",
        TarotRank.Nine => "nine",
        TarotRank.Ten => "ten",
        TarotRank.Page => "page",
        TarotRank.Knight => "knight",
        TarotRank.Queen => "queen",
        TarotRank.King => "king",
        _ => throw new ArgumentOutOfRangeException(nameof(rank), rank, "Unknown Tarot rank.")
    };
}
