using System.Globalization;
using NoxAeterna.Domain.Tarot;
using NoxAeterna.Interpretation.Tarot.Contracts;
using NoxAeterna.Interpretation.Tarot.Keys;

namespace NoxAeterna.Tests.Interpretation.Tarot;

public sealed class TarotInterpretationKeyTests
{
    [Fact]
    public void CanonicalKeys_HaveExactFrozenStringsAndRoundTrip()
    {
        var single = TarotInterpretationKeys.CreateSingleCard(new("major.fool"), TarotCardOrientation.Upright);
        var pair = TarotInterpretationKeys.CreateOrientedPair(
            new("major.tower"), new("major.world"), TarotOrientedPairState.ReversedUpright);
        var position = TarotInterpretationKeys.CreateThreeCardPosition(
            TarotThreeCardPosition.Past, new("major.tower"), TarotCardOrientation.Upright);
        var synthesis = TarotInterpretationKeys.CreateSynthesisResource(
            TarotSynthesisResourceType.SynthesisFragment, new("improving-strong"));

        Assert.Equal("major.fool|upright", single);
        Assert.Equal("major.tower__major.world|reversed-upright", pair);
        Assert.Equal("position|past|major.tower|upright", position);
        Assert.Equal("synthesis|synthesis-fragment|improving-strong", synthesis);
        Assert.Equal(single, TarotInterpretationKeys.CreateSingleCard(
            TarotInterpretationKeys.ParseSingleCard(single).Value!.CardId,
            TarotInterpretationKeys.ParseSingleCard(single).Value!.Orientation));
        Assert.Equal(pair, TarotInterpretationKeys.CreateOrientedPair(
            TarotInterpretationKeys.ParseOrientedPair(pair).Value!.CardAId,
            TarotInterpretationKeys.ParseOrientedPair(pair).Value!.CardBId,
            TarotInterpretationKeys.ParseOrientedPair(pair).Value!.OrientationState));
        Assert.True(TarotInterpretationKeys.ParseThreeCardPosition(position).IsValid);
        Assert.True(TarotInterpretationKeys.ParseSynthesisResource(synthesis).IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("major.fool")]
    [InlineData("major.fool|upright|extra")]
    [InlineData("major.fool|")]
    [InlineData("major.fool|UPRIGHT")]
    [InlineData(" major.fool|upright")]
    public void MalformedSingleCardKeys_AreRejected(string key)
    {
        var result = TarotInterpretationKeys.ParseSingleCard(key);

        Assert.False(result.IsValid);
        Assert.Null(result.Value);
        Assert.NotEmpty(result.Diagnostics);
    }

    [Theory]
    [InlineData("major.tower_major.world|upright-upright")]
    [InlineData("major.tower__major.world|upright-upright|extra")]
    [InlineData("major.world__major.tower|upright-upright")]
    [InlineData("major.tower__major.tower|upright-upright")]
    [InlineData("major.tower__major.world|unknown")]
    public void MalformedOrNoncanonicalPairKeys_AreRejected(string key)
    {
        Assert.False(TarotInterpretationKeys.ParseOrientedPair(key).IsValid);
    }

    [Theory]
    [InlineData(TarotCardOrientation.Upright, TarotCardOrientation.Upright, TarotOrientedPairState.UprightUpright)]
    [InlineData(TarotCardOrientation.Upright, TarotCardOrientation.Reversed, TarotOrientedPairState.UprightReversed)]
    [InlineData(TarotCardOrientation.Reversed, TarotCardOrientation.Upright, TarotOrientedPairState.ReversedUpright)]
    [InlineData(TarotCardOrientation.Reversed, TarotCardOrientation.Reversed, TarotOrientedPairState.ReversedReversed)]
    public void CanonicalPair_AlreadyOrderedPreservesAllOrientationStates(
        TarotCardOrientation a,
        TarotCardOrientation b,
        TarotOrientedPairState expected)
    {
        var result = TarotInterpretationKeys.CanonicalizePair(
            new("major.tower"), a, new("major.world"), b);

        Assert.True(result.IsValid);
        Assert.Equal("major.tower", result.Value!.CardAId.Value);
        Assert.Equal("major.world", result.Value.CardBId.Value);
        Assert.Equal(expected, result.Value.OrientationState);
    }

    [Theory]
    [InlineData(TarotCardOrientation.Upright, TarotCardOrientation.Upright, TarotOrientedPairState.UprightUpright)]
    [InlineData(TarotCardOrientation.Upright, TarotCardOrientation.Reversed, TarotOrientedPairState.ReversedUpright)]
    [InlineData(TarotCardOrientation.Reversed, TarotCardOrientation.Upright, TarotOrientedPairState.UprightReversed)]
    [InlineData(TarotCardOrientation.Reversed, TarotCardOrientation.Reversed, TarotOrientedPairState.ReversedReversed)]
    public void CanonicalPair_ReversedInputOrderMovesOrientationsWithCards(
        TarotCardOrientation world,
        TarotCardOrientation tower,
        TarotOrientedPairState expected)
    {
        var result = TarotInterpretationKeys.CanonicalizePair(
            new("major.world"), world, new("major.tower"), tower);

        Assert.True(result.IsValid);
        Assert.Equal("major.tower", result.Value!.CardAId.Value);
        Assert.Equal("major.world", result.Value.CardBId.Value);
        Assert.Equal(expected, result.Value.OrientationState);
    }

    [Fact]
    public void CanonicalPair_RejectsSelfPair()
    {
        var result = TarotInterpretationKeys.CanonicalizePair(
            new("major.tower"), TarotCardOrientation.Upright,
            new("major.tower"), TarotCardOrientation.Reversed);

        Assert.False(result.IsValid);
        Assert.Contains(result.Diagnostics, item => item.Code == "pair.self");
    }

    [Fact]
    public void CanonicalPair_UsesFullOrdinalIdWithCommonPrefixesAndIgnoresCultureAndCatalogOrder()
    {
        var previousCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("tr-TR");
            var result = TarotInterpretationKeys.CanonicalizePair(
                new("minor.cups.ten"), TarotCardOrientation.Reversed,
                new("minor.cups.ace"), TarotCardOrientation.Upright);

            Assert.True(result.IsValid);
            Assert.Equal("minor.cups.ace", result.Value!.CardAId.Value);
            Assert.Equal("minor.cups.ten", result.Value.CardBId.Value);
            Assert.Equal(TarotOrientedPairState.UprightReversed, result.Value.OrientationState);
            Assert.Equal(
                "minor.cups.ace__minor.cups.ten|upright-reversed",
                TarotInterpretationKeys.CreateOrientedPair(
                    result.Value.CardAId,
                    result.Value.CardBId,
                    result.Value.OrientationState));
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
        }
    }
}
