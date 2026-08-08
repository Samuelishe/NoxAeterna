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
        Assert.Equal(TarotReadingWorkspaceLayout.SideBySideMinimumWidth, result.GroupWidth);
        Assert.Equal(0d, result.LeadingOffset);
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
        Assert.Equal(180d, veryNarrow.GroupWidth);
        Assert.Equal(0d, veryNarrow.LeadingOffset);
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
        Assert.Equal(result.CardWidth / TarotTableauLayout.CardAspectRatio, result.CardHeight, precision: 10);
    }

    [Theory]
    [InlineData(1000d, 1000d, 594d, 0d)]
    [InlineData(1360d, 1126d, 720d, 117d)]
    [InlineData(1920d, 1126d, 720d, 397d)]
    [InlineData(2560d, 1126d, 720d, 717d)]
    public void WideSingleCard_BoundsAndCentersOneNaturalGroup(
        double availableWidth,
        double expectedGroupWidth,
        double expectedInterpretationWidth,
        double expectedLeadingOffset)
    {
        var result = TarotReadingWorkspaceLayout.CalculateSingleCard(availableWidth, 900d);

        Assert.Equal(TarotSingleCardReadingComposition.SideBySide, result.Composition);
        Assert.Equal(expectedGroupWidth, result.GroupWidth);
        Assert.Equal(expectedInterpretationWidth, result.InterpretationColumnWidth);
        Assert.Equal(expectedLeadingOffset, result.LeadingOffset);
        Assert.Equal(availableWidth, result.LeadingOffset * 2d + result.GroupWidth, precision: 10);
        Assert.InRange(
            result.InterpretationColumnWidth,
            TarotReadingWorkspaceLayout.MinimumInterpretationColumnWidth,
            TarotReadingWorkspaceLayout.MaximumInterpretationTextWidth);
        Assert.InRange(result.CardWidth, TarotTableauLayout.MinimumCardWidth, TarotTableauLayout.SingleCardWidth);
    }

    [Fact]
    public void MaximumWideGroup_IsDerivedFromCanonicalColumnContracts()
    {
        Assert.Equal(
            TarotTableauLayout.SingleCardWidth +
            TarotReadingWorkspaceLayout.ColumnGap +
            TarotReadingWorkspaceLayout.MaximumInterpretationTextWidth,
            TarotReadingWorkspaceLayout.MaximumSideBySideGroupWidth);

        var result = TarotReadingWorkspaceLayout.CalculateSingleCard(4000d, 2000d);

        Assert.Equal(TarotReadingWorkspaceLayout.MaximumSideBySideGroupWidth, result.GroupWidth);
        Assert.Equal(TarotReadingWorkspaceLayout.MaximumInterpretationTextWidth, result.InterpretationColumnWidth);
        Assert.True(result.LeadingOffset > 0d);
    }

    [Fact]
    public void NoReadingState_DoesNotDependOnASelectedSpread()
    {
        Assert.Equal(
            TarotReadingSurfaceState.NoReading,
            TarotReadingWorkspaceLayout.ResolveReadingSurfaceState(reading: null));
    }

    [Fact]
    public void PositionLabels_AreHiddenForNonSemanticSpreadsAndPreservedForThreeCards()
    {
        Assert.False(TarotReadingWorkspaceLayout.ShowPositionLabels(StandardTarotSpreads.SingleCard.Id));
        Assert.False(TarotReadingWorkspaceLayout.ShowPositionLabels(StandardTarotSpreads.TwoCards.Id));
        Assert.True(TarotReadingWorkspaceLayout.ShowPositionLabels(StandardTarotSpreads.ThreeCards.Id));
        Assert.False(TarotReadingWorkspaceLayout.ShowPositionLabels(new TarotSpreadId("future-spread")));
    }
}
