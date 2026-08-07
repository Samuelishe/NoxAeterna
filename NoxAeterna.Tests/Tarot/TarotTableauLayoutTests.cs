using NoxAeterna.Presentation.Tarot;

namespace NoxAeterna.Tests.Tarot;

public sealed class TarotTableauLayoutTests
{
    [Fact]
    public void ScaleConstants_AreExactOnePointFiveMultiplesOfBaselineWidths()
    {
        Assert.Equal(1.5d, TarotTableauLayout.CardScale);
        Assert.Equal(144d * TarotTableauLayout.CardScale, TarotTableauLayout.MinimumCardWidth);
        Assert.Equal(210d * TarotTableauLayout.CardScale, TarotTableauLayout.PreferredCardWidth);
        Assert.Equal(252d * TarotTableauLayout.CardScale, TarotTableauLayout.SingleCardWidth);
        Assert.Equal(216d, TarotTableauLayout.MinimumCardWidth);
        Assert.Equal(315d, TarotTableauLayout.PreferredCardWidth);
        Assert.Equal(378d, TarotTableauLayout.SingleCardWidth);
        Assert.Equal(985d, TarotTableauLayout.PreferredThreeCardContentWidth);
    }

    [Fact]
    public void CardBounds_AlwaysUseCanonicalSevenByTwelveRatio()
    {
        Assert.Equal(7d / 12d, TarotTableauLayout.CardAspectRatio, precision: 10);

        foreach (var availableWidth in new[] { 360d, 720d, 1200d })
        {
            var layout = TarotTableauLayout.Calculate(availableWidth, 3);

            Assert.All(layout.CardBounds, bounds =>
                Assert.Equal(7d / 12d, bounds.Width / bounds.Height, precision: 10));
        }

        Assert.Equal(216d / (7d / 12d), TarotTableauLayout.Calculate(216d, 1).ContentHeight, precision: 10);
        Assert.Equal(540d, TarotTableauLayout.Calculate(985d, 3).ContentHeight, precision: 10);
        Assert.Equal(648d, TarotTableauLayout.Calculate(900d, 1).ContentHeight, precision: 10);
    }

    [Fact]
    public void WideThreeCardLayout_UsesPreferredWidthWithOrderedNonOverlappingBounds()
    {
        var layout = TarotTableauLayout.Calculate(1200d, 3);

        Assert.Equal(3, layout.CardBounds.Count);
        Assert.False(layout.RequiresHorizontalScroll);
        Assert.All(layout.CardBounds, bounds => Assert.Equal(TarotTableauLayout.PreferredCardWidth, bounds.Width));
        Assert.All(layout.CardBounds, bounds => Assert.Equal(540d, bounds.Height));
        Assert.Equal(1200d, layout.ContentWidth);
        Assert.Equal(540d, layout.ContentHeight);
        Assert.True(layout.CardBounds[0].X + layout.CardBounds[0].Width < layout.CardBounds[1].X);
        Assert.True(layout.CardBounds[1].X + layout.CardBounds[1].Width < layout.CardBounds[2].X);
        Assert.True(layout.CardBounds[^1].X + layout.CardBounds[^1].Width <= layout.ContentWidth);
    }

    [Fact]
    public void CompactLayout_RetainsScaledMinimumAndOwnsHorizontalScroll()
    {
        var layout = TarotTableauLayout.Calculate(350d, 3);

        Assert.Equal(216d, TarotTableauLayout.MinimumCardWidth);
        Assert.True(layout.RequiresHorizontalScroll);
        Assert.All(layout.CardBounds, bounds =>
        {
            Assert.Equal(216d, bounds.Width);
            Assert.Equal(216d / TarotTableauLayout.CardAspectRatio, bounds.Height, precision: 10);
        });
        Assert.Equal((3d * 216d) + (2d * TarotTableauLayout.CardGap), layout.ContentWidth);
        Assert.Equal(0d, layout.CardBounds[0].X);
    }

    [Fact]
    public void SingleCardLayout_UsesSingleCardWidthAndExactRatio()
    {
        var layout = TarotTableauLayout.Calculate(900d, 1);
        var card = Assert.Single(layout.CardBounds);

        Assert.Equal(378d, card.Width);
        Assert.Equal(648d, card.Height);
        Assert.Equal((900d - card.Width) / 2d, card.X);
        Assert.Equal(648d, layout.ContentHeight);
        Assert.False(layout.RequiresHorizontalScroll);
    }

    [Fact]
    public void HeightFittedSingleCard_DoesNotChangeMultiCardTableauContract()
    {
        var single = TarotTableauLayout.CalculateSingleCard(378d, 300d);
        var three = TarotTableauLayout.Calculate(985d, 3);

        Assert.Equal(300d, Assert.Single(single.CardBounds).Width);
        Assert.Equal(300d / TarotTableauLayout.CardAspectRatio, single.ContentHeight, precision: 10);
        Assert.Equal(3, three.CardBounds.Count);
        Assert.All(three.CardBounds, bounds => Assert.Equal(315d, bounds.Width));
        Assert.Equal(985d, three.ContentWidth);
        Assert.False(three.RequiresHorizontalScroll);
    }

    [Theory]
    [InlineData(687.999d, true)]
    [InlineData(688d, false)]
    [InlineData(688.001d, false)]
    public void HorizontalScroll_IsRequiredOnlyWhenMinimumRowExceedsAvailableWidth(
        double availableWidth,
        bool expected)
    {
        var layout = TarotTableauLayout.Calculate(availableWidth, 3);

        Assert.Equal(expected, layout.RequiresHorizontalScroll);
        Assert.All(layout.CardBounds, bounds => Assert.True(bounds.Width >= TarotTableauLayout.MinimumCardWidth));
    }

    [Fact]
    public void IntermediateWidth_FitsCenteredCardsBetweenMinimumAndPreferredWithoutScroll()
    {
        const double availableWidth = 800d;

        var layout = TarotTableauLayout.Calculate(availableWidth, 3);

        Assert.False(layout.RequiresHorizontalScroll);
        Assert.Equal(availableWidth, layout.ContentWidth);
        Assert.All(layout.CardBounds, bounds => Assert.Equal((availableWidth - 40d) / 3d, bounds.Width));
        Assert.Equal(0d, layout.CardBounds[0].X, precision: 10);
        Assert.Equal(TarotTableauLayout.CardGap, layout.CardBounds[1].X - layout.CardBounds[0].X - layout.CardBounds[0].Width, precision: 10);
        Assert.Equal(TarotTableauLayout.CardGap, layout.CardBounds[2].X - layout.CardBounds[1].X - layout.CardBounds[1].Width, precision: 10);
    }

    [Theory]
    [InlineData(-1d)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void Calculate_RejectsInvalidAvailableWidth(double availableWidth)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            TarotTableauLayout.Calculate(availableWidth, 1));

        Assert.Equal("availableWidth", exception.ParamName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Calculate_RejectsNonPositiveCardCount(int cardCount)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            TarotTableauLayout.Calculate(800d, cardCount));

        Assert.Equal("cardCount", exception.ParamName);
    }
}
