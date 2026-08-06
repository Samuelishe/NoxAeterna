namespace NoxAeterna.Presentation.Tarot;

/// <summary>Represents one render-independent card rectangle in the Tarot tableau.</summary>
public sealed record TarotCardLayoutBounds(double X, double Y, double Width, double Height);

/// <summary>Represents a responsive horizontal Tarot tableau layout.</summary>
public sealed record TarotTableauLayoutResult(
    IReadOnlyList<TarotCardLayoutBounds> CardBounds,
    double ContentWidth,
    double ContentHeight,
    bool RequiresHorizontalScroll);

/// <summary>Calculates responsive 7:12 Tarot-card bounds without Avalonia dependencies.</summary>
public static class TarotTableauLayout
{
    private const double BaseMinimumCardWidth = 144d;
    private const double BasePreferredCardWidth = 210d;
    private const double BaseSingleCardWidth = 252d;

    /// <summary>Gets the canonical width-to-height ratio.</summary>
    public const double CardAspectRatio = 7d / 12d;

    /// <summary>Gets the scale applied to the original playable-workspace card widths.</summary>
    public const double CardScale = 1.5d;

    /// <summary>Gets the readable minimum card width in device-independent pixels.</summary>
    public const double MinimumCardWidth = BaseMinimumCardWidth * CardScale;

    /// <summary>Gets the preferred multi-card width in device-independent pixels.</summary>
    public const double PreferredCardWidth = BasePreferredCardWidth * CardScale;

    /// <summary>Gets the maximum single-card width in device-independent pixels.</summary>
    public const double SingleCardWidth = BaseSingleCardWidth * CardScale;

    /// <summary>Gets the gap between cards in device-independent pixels.</summary>
    public const double CardGap = 20d;

    /// <summary>Gets the preferred content width of the current three-card tableau.</summary>
    public const double PreferredThreeCardContentWidth = (PreferredCardWidth * 3d) + (CardGap * 2d);

    /// <summary>Calculates a centered row, retaining the readable minimum through tableau-owned horizontal scrolling.</summary>
    public static TarotTableauLayoutResult Calculate(double availableWidth, int cardCount)
    {
        if (!double.IsFinite(availableWidth) || availableWidth < 0d)
        {
            throw new ArgumentOutOfRangeException(nameof(availableWidth));
        }

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(cardCount);

        var totalGap = CardGap * (cardCount - 1);
        var maximumWidth = cardCount == 1 ? SingleCardWidth : PreferredCardWidth;
        var fittedWidth = (availableWidth - totalGap) / cardCount;
        var cardWidth = Math.Clamp(fittedWidth, MinimumCardWidth, maximumWidth);
        var cardHeight = cardWidth / CardAspectRatio;
        var rowWidth = (cardWidth * cardCount) + totalGap;
        var requiresHorizontalScroll = rowWidth > availableWidth;
        var leadingOffset = requiresHorizontalScroll ? 0d : (availableWidth - rowWidth) / 2d;
        var bounds = Enumerable.Range(0, cardCount)
            .Select(index => new TarotCardLayoutBounds(
                leadingOffset + (index * (cardWidth + CardGap)),
                0d,
                cardWidth,
                cardHeight))
            .ToArray();

        return new TarotTableauLayoutResult(
            Array.AsReadOnly(bounds),
            Math.Max(availableWidth, rowWidth),
            cardHeight,
            requiresHorizontalScroll);
    }
}
