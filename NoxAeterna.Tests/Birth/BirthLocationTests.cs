using NoxAeterna.Domain.Birth;

namespace NoxAeterna.Tests.Birth;

public sealed class BirthLocationTests
{
    [Theory]
    [InlineData(-90d)]
    [InlineData(0d)]
    [InlineData(90d)]
    public void Constructor_AllowsLatitudeWithinRange(double latitude)
    {
        var location = new BirthLocation("Test", latitude, 10d);

        Assert.Equal(latitude, location.Latitude);
    }

    [Theory]
    [InlineData(-90.0001d)]
    [InlineData(90.0001d)]
    [InlineData(double.NaN)]
    [InlineData(double.NegativeInfinity)]
    [InlineData(double.PositiveInfinity)]
    public void Constructor_RejectsInvalidLatitude(double latitude)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new BirthLocation("Test", latitude, 10d));
    }

    [Theory]
    [InlineData(-180d)]
    [InlineData(0d)]
    [InlineData(180d)]
    public void Constructor_AllowsLongitudeWithinRange(double longitude)
    {
        var location = new BirthLocation("Test", 10d, longitude);

        Assert.Equal(longitude, location.Longitude);
    }

    [Theory]
    [InlineData(-180.0001d)]
    [InlineData(180.0001d)]
    [InlineData(double.NaN)]
    [InlineData(double.NegativeInfinity)]
    [InlineData(double.PositiveInfinity)]
    public void Constructor_RejectsInvalidLongitude(double longitude)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new BirthLocation("Test", 10d, longitude));
    }
}
