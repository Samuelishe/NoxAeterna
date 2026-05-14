using NoxAeterna.Domain.Astrology;

namespace NoxAeterna.Tests.Astrology;

public sealed class AspectMathTests
{
    [Theory]
    [InlineData(359d, 1d, 2d)]
    [InlineData(1d, 359d, 2d)]
    [InlineData(10d, 190d, 180d)]
    [InlineData(15d, 75d, 60d)]
    public void CalculateAngularDelta_ReturnsShortestDistance(double first, double second, double expected)
    {
        var firstLongitude = new ZodiacLongitude(first);
        var secondLongitude = new ZodiacLongitude(second);

        var delta = AspectMath.CalculateAngularDelta(firstLongitude, secondLongitude);

        Assert.Equal(expected, delta, precision: 10);
    }

    [Theory]
    [InlineData(10d, 10d, AspectType.Conjunction)]
    [InlineData(10d, 70d, AspectType.Sextile)]
    [InlineData(10d, 100d, AspectType.Square)]
    [InlineData(10d, 130d, AspectType.Trine)]
    [InlineData(10d, 190d, AspectType.Opposition)]
    public void IsMatch_ReturnsTrueForExactAspect(double first, double second, AspectType aspectType)
    {
        var isMatch = AspectMath.IsMatch(new ZodiacLongitude(first), new ZodiacLongitude(second), aspectType);

        Assert.True(isMatch);
    }

    [Theory]
    [InlineData(10d, 15.5d, AspectType.Conjunction)]
    [InlineData(10d, 65.5d, AspectType.Sextile)]
    [InlineData(10d, 104.5d, AspectType.Square)]
    [InlineData(10d, 124.5d, AspectType.Trine)]
    [InlineData(10d, 184.5d, AspectType.Opposition)]
    public void IsMatch_ReturnsTrueWithinDefaultOrb(double first, double second, AspectType aspectType)
    {
        var isMatch = AspectMath.IsMatch(new ZodiacLongitude(first), new ZodiacLongitude(second), aspectType);

        Assert.True(isMatch);
    }

    [Theory]
    [InlineData(10d, 16.5d, AspectType.Conjunction)]
    [InlineData(10d, 76.5d, AspectType.Sextile)]
    [InlineData(10d, 106.5d, AspectType.Square)]
    [InlineData(10d, 136.5d, AspectType.Trine)]
    [InlineData(10d, 196.5d, AspectType.Opposition)]
    public void IsMatch_ReturnsFalseOutsideDefaultOrb(double first, double second, AspectType aspectType)
    {
        var isMatch = AspectMath.IsMatch(new ZodiacLongitude(first), new ZodiacLongitude(second), aspectType);

        Assert.False(isMatch);
    }

    [Fact]
    public void IsMatch_UsesProvidedOrb()
    {
        var isMatch = AspectMath.IsMatch(
            new ZodiacLongitude(10d),
            new ZodiacLongitude(17d),
            AspectType.Conjunction,
            orbDegrees: 7d);

        Assert.True(isMatch);
    }

    [Fact]
    public void IsMatch_RejectsNegativeOrb()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            AspectMath.IsMatch(
                new ZodiacLongitude(10d),
                new ZodiacLongitude(10d),
                AspectType.Conjunction,
                orbDegrees: -0.1d));
    }
}
