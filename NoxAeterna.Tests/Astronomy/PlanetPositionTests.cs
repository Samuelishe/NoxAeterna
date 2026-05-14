using NoxAeterna.Domain.Astrology;

namespace NoxAeterna.Tests.Astronomy;

public sealed class PlanetPositionTests
{
    [Theory]
    [InlineData(0d, ZodiacSign.Aries)]
    [InlineData(30d, ZodiacSign.Taurus)]
    [InlineData(210d, ZodiacSign.Scorpio)]
    [InlineData(359.999d, ZodiacSign.Pisces)]
    public void Sign_IsDerivedFromLongitude(double longitude, ZodiacSign expectedSign)
    {
        var position = new PlanetPosition(CelestialBody.Mars, new ZodiacLongitude(longitude), isRetrograde: false);

        Assert.Equal(expectedSign, position.Sign);
    }

    [Theory]
    [InlineData(0d, 0d)]
    [InlineData(29.5d, 29.5d)]
    [InlineData(30d, 0d)]
    [InlineData(45.25d, 15.25d)]
    [InlineData(359.999d, 29.999d)]
    public void DegreeWithinSign_IsDerivedFromLongitude(double longitude, double expected)
    {
        var position = new PlanetPosition(CelestialBody.Venus, new ZodiacLongitude(longitude), isRetrograde: true);

        Assert.Equal(expected, position.DegreeWithinSign, precision: 10);
    }
}
