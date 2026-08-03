using NoxAeterna.Presentation.Tarot;

namespace NoxAeterna.Tests.Tarot;

public sealed class TarotTableauLayoutTests
{
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
    }

    [Fact]
    public void ThreeCardLayout_ProducesOrderedNonOverlappingBounds()
    {
        var layout = TarotTableauLayout.Calculate(900d, 3);

        Assert.Equal(3, layout.CardBounds.Count);
        Assert.False(layout.RequiresHorizontalScroll);
        Assert.All(layout.CardBounds, bounds => Assert.Equal(TarotTableauLayout.PreferredCardWidth, bounds.Width));
        Assert.True(layout.CardBounds[0].X + layout.CardBounds[0].Width < layout.CardBounds[1].X);
        Assert.True(layout.CardBounds[1].X + layout.CardBounds[1].Width < layout.CardBounds[2].X);
        Assert.True(layout.CardBounds[^1].X + layout.CardBounds[^1].Width <= layout.ContentWidth);
    }

    [Fact]
    public void CompactWidth_RetainsReadableMinimumAndOwnsHorizontalScroll()
    {
        var layout = TarotTableauLayout.Calculate(350d, 3);

        Assert.Equal(144d, TarotTableauLayout.MinimumCardWidth);
        Assert.True(layout.RequiresHorizontalScroll);
        Assert.All(layout.CardBounds, bounds => Assert.Equal(144d, bounds.Width));
        Assert.True(layout.ContentWidth > 350d);
        Assert.Equal(0d, layout.CardBounds[0].X);
    }

    [Fact]
    public void SingleCard_IsLargeAndCenteredWithoutCompetingEmptySpace()
    {
        var layout = TarotTableauLayout.Calculate(900d, 1);
        var card = Assert.Single(layout.CardBounds);

        Assert.Equal(TarotTableauLayout.SingleCardWidth, card.Width);
        Assert.Equal((900d - card.Width) / 2d, card.X);
        Assert.False(layout.RequiresHorizontalScroll);
    }
}
