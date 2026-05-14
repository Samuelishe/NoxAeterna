using NoxAeterna.Domain.Astrology;

namespace NoxAeterna.Tests.Astrology;

public sealed class ZodiacLongitudeTests
{
    [Theory]
    [InlineData(0d, 0d)]
    [InlineData(359.999d, 359.999d)]
    [InlineData(360d, 0d)]
    [InlineData(361d, 1d)]
    [InlineData(-1d, 359d)]
    [InlineData(-720d, 0d)]
    [InlineData(721.5d, 1.5d)]
    public void Constructor_NormalizesDegrees(double input, double expected)
    {
        var longitude = new ZodiacLongitude(input);

        Assert.Equal(expected, longitude.Degrees, precision: 10);
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void Constructor_RejectsNonFiniteValues(double input)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ZodiacLongitude(input));
    }

    [Theory]
    [InlineData(0d, ZodiacSign.Aries)]
    [InlineData(29.999d, ZodiacSign.Aries)]
    [InlineData(30d, ZodiacSign.Taurus)]
    [InlineData(59.999d, ZodiacSign.Taurus)]
    [InlineData(60d, ZodiacSign.Gemini)]
    [InlineData(90d, ZodiacSign.Cancer)]
    [InlineData(120d, ZodiacSign.Leo)]
    [InlineData(150d, ZodiacSign.Virgo)]
    [InlineData(180d, ZodiacSign.Libra)]
    [InlineData(210d, ZodiacSign.Scorpio)]
    [InlineData(240d, ZodiacSign.Sagittarius)]
    [InlineData(270d, ZodiacSign.Capricorn)]
    [InlineData(300d, ZodiacSign.Aquarius)]
    [InlineData(330d, ZodiacSign.Pisces)]
    [InlineData(359.999d, ZodiacSign.Pisces)]
    public void Sign_ReturnsExpectedZodiacSign(double input, ZodiacSign expected)
    {
        var longitude = new ZodiacLongitude(input);

        Assert.Equal(expected, longitude.Sign);
    }
}
