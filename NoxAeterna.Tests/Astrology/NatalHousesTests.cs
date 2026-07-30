using NoxAeterna.Domain.Astrology;

namespace NoxAeterna.Tests.Astrology;

public sealed class NatalHousesTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    [InlineData(-1)]
    public void HouseNumber_RejectsValuesOutsideOneThroughTwelve(int value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new HouseNumber(value));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(12)]
    public void HouseNumber_AcceptsBoundaryValues(int value)
    {
        Assert.Equal(value, new HouseNumber(value).Value);
    }

    [Fact]
    public void CreateAvailable_OrdersTwelveCuspsAndKeepsCollectionReadOnly()
    {
        var source = CreateCusps().Reverse().ToList();

        var houses = NatalHouses.CreateAvailable(
            HouseSystem.Placidus,
            source,
            new ChartAngles(new ZodiacLongitude(15d), new ZodiacLongitude(285d)));
        source.Clear();

        Assert.Equal(Enumerable.Range(1, 12), houses.Cusps.Select(cusp => cusp.HouseNumber.Value));
        Assert.Equal(12, houses.Cusps.Count);
        Assert.Throws<NotSupportedException>(() =>
            ((ICollection<HouseCusp>)houses.Cusps).Add(
                new HouseCusp(new HouseNumber(1), new ZodiacLongitude(15d))));
    }

    [Fact]
    public void CreateAvailable_RejectsDuplicateOrIncompleteHouseNumbers()
    {
        var duplicate = CreateCusps().ToArray();
        duplicate[^1] = new HouseCusp(new HouseNumber(1), new ZodiacLongitude(345d));

        Assert.Throws<ArgumentException>(() =>
            NatalHouses.CreateAvailable(
                HouseSystem.Placidus,
                duplicate,
                new ChartAngles(new ZodiacLongitude(15d), new ZodiacLongitude(285d))));
        Assert.Throws<ArgumentException>(() =>
            NatalHouses.CreateAvailable(
                HouseSystem.Placidus,
                CreateCusps().Take(11),
                new ChartAngles(new ZodiacLongitude(15d), new ZodiacLongitude(285d))));
    }

    [Fact]
    public void CreateAvailable_RejectsAscendantThatDoesNotMatchFirstCusp()
    {
        Assert.Throws<ArgumentException>(() =>
            NatalHouses.CreateAvailable(
                HouseSystem.Placidus,
                CreateCusps(),
                new ChartAngles(new ZodiacLongitude(16d), new ZodiacLongitude(285d))));
    }

    [Fact]
    public void ChartAngles_DeriveExactOppositePointsAndNormalizeLongitudes()
    {
        var angles = new ChartAngles(
            new ZodiacLongitude(370d),
            new ZodiacLongitude(-5d));

        Assert.Equal(10d, angles.Ascendant.Degrees, precision: 10);
        Assert.Equal(190d, angles.Descendant.Degrees, precision: 10);
        Assert.Equal(355d, angles.Midheaven.Degrees, precision: 10);
        Assert.Equal(175d, angles.ImumCoeli.Degrees, precision: 10);
    }

    [Fact]
    public void UnavailableHouses_NeverExposeCuspsOrAngles()
    {
        var houses = NatalHouses.CreateUnavailable(
            HouseSystem.Placidus,
            NatalHousesAvailability.UnavailableUnknownTime);

        Assert.False(houses.IsAvailable);
        Assert.Empty(houses.Cusps);
        Assert.Null(houses.Angles);
    }

    private static IEnumerable<HouseCusp> CreateCusps() =>
        Enumerable.Range(1, 12)
            .Select(index => new HouseCusp(
                new HouseNumber(index),
                new ZodiacLongitude(15d + ((index - 1) * 30d))));
}
