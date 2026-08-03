namespace NoxAeterna.Domain.Tarot;

/// <summary>Identifies the structural Arcana family of a Tarot card.</summary>
public enum TarotArcana
{
    /// <summary>Major Arcana.</summary>
    Major,

    /// <summary>Minor Arcana.</summary>
    Minor
}

/// <summary>Identifies a Minor Arcana suit without assigning display text.</summary>
public enum TarotSuit
{
    /// <summary>Wands suit.</summary>
    Wands,

    /// <summary>Cups suit.</summary>
    Cups,

    /// <summary>Swords suit.</summary>
    Swords,

    /// <summary>Pentacles suit.</summary>
    Pentacles
}

/// <summary>Identifies a Minor Arcana rank without relying on its numeric enum value.</summary>
public enum TarotRank
{
    /// <summary>Ace rank.</summary>
    Ace,
    /// <summary>Two rank.</summary>
    Two,
    /// <summary>Three rank.</summary>
    Three,
    /// <summary>Four rank.</summary>
    Four,
    /// <summary>Five rank.</summary>
    Five,
    /// <summary>Six rank.</summary>
    Six,
    /// <summary>Seven rank.</summary>
    Seven,
    /// <summary>Eight rank.</summary>
    Eight,
    /// <summary>Nine rank.</summary>
    Nine,
    /// <summary>Ten rank.</summary>
    Ten,
    /// <summary>Page court rank.</summary>
    Page,
    /// <summary>Knight court rank.</summary>
    Knight,
    /// <summary>Queen court rank.</summary>
    Queen,
    /// <summary>King court rank.</summary>
    King
}

/// <summary>Defines the language-neutral structural identity of one Tarot card.</summary>
public sealed record TarotCardDefinition
{
    private TarotCardDefinition(
        TarotCardId id,
        TarotArcana arcana,
        TarotSuit? suit,
        TarotRank? rank)
    {
        Id = id;
        Arcana = arcana;
        Suit = suit;
        Rank = rank;
    }

    /// <summary>Gets the stable semantic card identity.</summary>
    public TarotCardId Id { get; }

    /// <summary>Gets the structural Arcana family.</summary>
    public TarotArcana Arcana { get; }

    /// <summary>Gets the Minor Arcana suit, or <see langword="null"/> for a Major card.</summary>
    public TarotSuit? Suit { get; }

    /// <summary>Gets the Minor Arcana rank, or <see langword="null"/> for a Major card.</summary>
    public TarotRank? Rank { get; }

    /// <summary>Creates a Major Arcana card definition.</summary>
    public static TarotCardDefinition CreateMajor(TarotCardId id)
    {
        ArgumentNullException.ThrowIfNull(id);
        return new TarotCardDefinition(id, TarotArcana.Major, suit: null, rank: null);
    }

    /// <summary>Creates a Minor Arcana card definition.</summary>
    public static TarotCardDefinition CreateMinor(TarotCardId id, TarotSuit suit, TarotRank rank)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentOutOfRangeException.ThrowIfNotEqual(Enum.IsDefined(suit), true, nameof(suit));
        ArgumentOutOfRangeException.ThrowIfNotEqual(Enum.IsDefined(rank), true, nameof(rank));
        return new TarotCardDefinition(id, TarotArcana.Minor, suit, rank);
    }
}
