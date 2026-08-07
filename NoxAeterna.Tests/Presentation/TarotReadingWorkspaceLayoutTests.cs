using NoxAeterna.Domain.Tarot;
using NoxAeterna.Presentation.Tarot;

namespace NoxAeterna.Tests.Presentation;

public sealed class TarotReadingWorkspaceLayoutTests
{
    [Fact]
    public void WideSingleCard_UsesSideBySideCompositionWithAcceptedCardScale()
    {
        var result = TarotReadingWorkspaceLayout.CalculateSingleCard(
            TarotReadingWorkspaceLayout.SideBySideMinimumWidth,
            700d);

        Assert.Equal(TarotSingleCardReadingComposition.SideBySide, result.Composition);
        Assert.Equal(TarotTableauLayout.SingleCardWidth, result.CardColumnWidth);
        Assert.Equal(TarotTableauLayout.SingleCardWidth, result.CardWidth);
        Assert.Equal(648d, result.CardHeight);
        Assert.Equal(
            TarotReadingWorkspaceLayout.MinimumInterpretationColumnWidth,
            result.InterpretationColumnWidth);
    }

    [Fact]
    public void NarrowSingleCard_UsesStackedCompositionWithoutShrinkingBelowReadableMinimum()
    {
        var result = TarotReadingWorkspaceLayout.CalculateSingleCard(500d, 700d);

        Assert.Equal(TarotSingleCardReadingComposition.Stacked, result.Composition);
        Assert.Equal(TarotTableauLayout.SingleCardWidth, result.CardWidth);

        var veryNarrow = TarotReadingWorkspaceLayout.CalculateSingleCard(180d, 700d);
        Assert.Equal(TarotTableauLayout.MinimumCardWidth, veryNarrow.CardWidth);
        Assert.Equal(180d, veryNarrow.InterpretationColumnWidth);
    }

    [Theory]
    [InlineData(-0.001d, TarotSingleCardReadingComposition.Stacked)]
    [InlineData(0d, TarotSingleCardReadingComposition.SideBySide)]
    [InlineData(0.001d, TarotSingleCardReadingComposition.SideBySide)]
    public void Breakpoint_IsDeterministicAtCardGapAndReadableTextSum(
        double delta,
        TarotSingleCardReadingComposition expected)
    {
        Assert.Equal(
            TarotTableauLayout.SingleCardWidth +
            TarotReadingWorkspaceLayout.ColumnGap +
            TarotReadingWorkspaceLayout.MinimumInterpretationColumnWidth,
            TarotReadingWorkspaceLayout.SideBySideMinimumWidth);

        var result = TarotReadingWorkspaceLayout.CalculateSingleCard(
            TarotReadingWorkspaceLayout.SideBySideMinimumWidth + delta,
            700d);

        Assert.Equal(expected, result.Composition);
    }

    [Fact]
    public void WideSingleCard_FitsAvailableHeightWithinAcceptedScaleBounds()
    {
        var result = TarotReadingWorkspaceLayout.CalculateSingleCard(1000d, 500d);

        Assert.Equal(TarotSingleCardReadingComposition.SideBySide, result.Composition);
        Assert.Equal(500d, result.CardHeight, precision: 10);
        Assert.Equal(500d * TarotTableauLayout.CardAspectRatio, result.CardWidth, precision: 10);
        Assert.InRange(
            result.CardWidth,
            TarotTableauLayout.MinimumCardWidth,
            TarotTableauLayout.SingleCardWidth);
    }

    [Fact]
    public void PositionLabels_AreHiddenOnlyForSingleCardSpread()
    {
        Assert.False(TarotReadingWorkspaceLayout.ShowPositionLabels(StandardTarotSpreads.SingleCard.Id));
        Assert.True(TarotReadingWorkspaceLayout.ShowPositionLabels(StandardTarotSpreads.ThreeCards.Id));
        Assert.True(TarotReadingWorkspaceLayout.ShowPositionLabels(new TarotSpreadId("future-spread")));
    }
}
