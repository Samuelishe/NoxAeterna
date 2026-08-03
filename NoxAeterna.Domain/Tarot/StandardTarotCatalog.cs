namespace NoxAeterna.Domain.Tarot;

/// <summary>Provides the built-in semantic catalog for the standard 78-card Tarot structure.</summary>
public static class StandardTarotCatalog
{
    private static readonly string[] MajorCardIds =
    [
        "major.chariot",
        "major.death",
        "major.devil",
        "major.emperor",
        "major.empress",
        "major.fool",
        "major.hanged-man",
        "major.hermit",
        "major.hierophant",
        "major.high-priestess",
        "major.judgement",
        "major.justice",
        "major.lovers",
        "major.magician",
        "major.moon",
        "major.star",
        "major.strength",
        "major.sun",
        "major.temperance",
        "major.tower",
        "major.wheel-of-fortune",
        "major.world"
    ];

    private static readonly TarotSuit[] Suits =
    [
        TarotSuit.Wands,
        TarotSuit.Cups,
        TarotSuit.Swords,
        TarotSuit.Pentacles
    ];

    private static readonly TarotRank[] Ranks =
    [
        TarotRank.Ace,
        TarotRank.Two,
        TarotRank.Three,
        TarotRank.Four,
        TarotRank.Five,
        TarotRank.Six,
        TarotRank.Seven,
        TarotRank.Eight,
        TarotRank.Nine,
        TarotRank.Ten,
        TarotRank.Page,
        TarotRank.Knight,
        TarotRank.Queen,
        TarotRank.King
    ];

    /// <summary>Gets the immutable standard semantic deck.</summary>
    public static TarotDeckDefinition Deck { get; } = CreateDeck();

    private static TarotDeckDefinition CreateDeck()
    {
        var cards = MajorCardIds
            .Select(static id => TarotCardDefinition.CreateMajor(new TarotCardId(id)))
            .Concat(
                Suits.SelectMany(suit => Ranks.Select(rank =>
                    TarotCardDefinition.CreateMinor(
                        new TarotCardId($"minor.{GetSuitSegment(suit)}.{GetRankSegment(rank)}"),
                        suit,
                        rank))))
            .OrderBy(static card => card.Id.Value, StringComparer.Ordinal)
            .ToArray();

        return new TarotDeckDefinition(new TarotDeckId("standard-78"), cards);
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
