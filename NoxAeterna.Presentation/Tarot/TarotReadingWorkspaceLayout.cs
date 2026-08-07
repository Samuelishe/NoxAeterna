using NoxAeterna.Domain.Tarot;

namespace NoxAeterna.Presentation.Tarot;

/// <summary>Identifies the responsive composition used by a single-card reading.</summary>
public enum TarotSingleCardReadingComposition
{
    Stacked = 0,
    SideBySide = 1
}

/// <summary>Describes a render-independent single-card reading composition.</summary>
public sealed record TarotSingleCardReadingLayoutResult(
    TarotSingleCardReadingComposition Composition,
    double CardColumnWidth,
    double CardWidth,
    double CardHeight,
    double InterpretationColumnWidth);

/// <summary>Owns responsive reading-workspace measurements without Avalonia dependencies.</summary>
public static class TarotReadingWorkspaceLayout
{
    /// <summary>Gets the gap between the card and interpretation columns.</summary>
    public const double ColumnGap = 28d;

    /// <summary>Gets the minimum width required for a readable interpretation column.</summary>
    public const double MinimumInterpretationColumnWidth = 400d;

    /// <summary>Gets the maximum readable measure of interpretation content.</summary>
    public const double MaximumInterpretationTextWidth = 720d;

    /// <summary>
    /// Gets the content width at which the accepted single-card width and a readable interpretation column fit together.
    /// </summary>
    public const double SideBySideMinimumWidth =
        TarotTableauLayout.SingleCardWidth + ColumnGap + MinimumInterpretationColumnWidth;

    /// <summary>Calculates the responsive composition for one single-card reading surface.</summary>
    public static TarotSingleCardReadingLayoutResult CalculateSingleCard(
        double availableWidth,
        double availableHeight)
    {
        if (!double.IsFinite(availableWidth) || availableWidth < 0d)
        {
            throw new ArgumentOutOfRangeException(nameof(availableWidth));
        }

        if (!double.IsFinite(availableHeight) || availableHeight < 0d)
        {
            throw new ArgumentOutOfRangeException(nameof(availableHeight));
        }

        if (availableWidth < SideBySideMinimumWidth)
        {
            var stackedCardWidth = Math.Clamp(
                availableWidth,
                TarotTableauLayout.MinimumCardWidth,
                TarotTableauLayout.SingleCardWidth);
            return new TarotSingleCardReadingLayoutResult(
                TarotSingleCardReadingComposition.Stacked,
                availableWidth,
                stackedCardWidth,
                stackedCardWidth / TarotTableauLayout.CardAspectRatio,
                availableWidth);
        }

        var heightFittedCardWidth = availableHeight <= 0d
            ? TarotTableauLayout.SingleCardWidth
            : availableHeight * TarotTableauLayout.CardAspectRatio;
        var cardWidth = Math.Clamp(
            heightFittedCardWidth,
            TarotTableauLayout.MinimumCardWidth,
            TarotTableauLayout.SingleCardWidth);
        return new TarotSingleCardReadingLayoutResult(
            TarotSingleCardReadingComposition.SideBySide,
            TarotTableauLayout.SingleCardWidth,
            cardWidth,
            cardWidth / TarotTableauLayout.CardAspectRatio,
            availableWidth - TarotTableauLayout.SingleCardWidth - ColumnGap);
    }

    /// <summary>Returns whether semantic position labels should be visible for a spread.</summary>
    public static bool ShowPositionLabels(TarotSpreadId spreadId)
    {
        ArgumentNullException.ThrowIfNull(spreadId);
        return spreadId != StandardTarotSpreads.SingleCard.Id;
    }
}
