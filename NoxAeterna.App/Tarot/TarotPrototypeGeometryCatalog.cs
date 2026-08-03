using NoxAeterna.Domain.Tarot;

namespace NoxAeterna.App.Tarot;

/// <summary>Owns original normalized vector paths for prototype Tarot markers and backs.</summary>
public static class TarotPrototypeGeometryCatalog
{
    private const string BlackSunPath =
        "M50,8 A42,42 0 1 1 49.9,8 M50,22 A28,28 0 1 1 49.9,22 " +
        "M50,30 A20,20 0 1 1 49.9,30 M50,2 L50,16 M50,84 L50,98 " +
        "M2,50 L16,50 M84,50 L98,50 M16,16 L26,26 M74,74 L84,84 " +
        "M84,16 L74,26 M26,74 L16,84";

    private const string LunarSealPath =
        "M50,8 A42,42 0 1 1 49.9,8 M50,18 A32,32 0 1 1 49.9,18 " +
        "M61,27 A23,23 0 1 0 61,73 A18,23 0 1 1 61,27 " +
        "M50,12 L88,50 L50,88 L12,50 Z M50,35 L65,50 L50,65 L35,50 Z";

    /// <summary>Gets the normalized path data for a card-back variant.</summary>
    public static string GetBackPathData(TarotBackVariantId backVariantId)
    {
        ArgumentNullException.ThrowIfNull(backVariantId);
        return backVariantId.Value switch
        {
            "black-sun" => BlackSunPath,
            "lunar-seal" => LunarSealPath,
            _ => throw new ArgumentOutOfRangeException(nameof(backVariantId), backVariantId, "Unknown Tarot back variant.")
        };
    }

    /// <summary>Gets a normalized project-owned Minor Arcana suit marker.</summary>
    public static string GetSuitPathData(TarotSuit suit) => suit switch
    {
        TarotSuit.Wands => "M50,12 L50,88 M42,22 L58,14 M42,42 L58,34 M42,62 L58,54",
        TarotSuit.Cups => "M26,18 L74,18 C74,46 64,58 50,58 C36,58 26,46 26,18 Z M50,58 L50,78 M34,84 L66,84",
        TarotSuit.Swords => "M50,10 L61,30 L55,68 L45,68 L39,30 Z M30,68 L70,68 M42,68 L38,88 M58,68 L62,88",
        TarotSuit.Pentacles => "M50,10 L61,38 L90,38 L66,56 L75,86 L50,68 L25,86 L34,56 L10,38 L39,38 Z M50,22 A28,28 0 1 1 49.9,22",
        _ => throw new ArgumentOutOfRangeException(nameof(suit), suit, "Unknown Tarot suit.")
    };

    /// <summary>Gets a normalized Major Arcana prototype seal.</summary>
    public static string GetMajorSealPathData() =>
        "M50,12 A38,38 0 1 1 49.9,12 M50,24 L58,42 L78,44 L63,57 L68,78 L50,67 L32,78 L37,57 L22,44 L42,42 Z";
}
