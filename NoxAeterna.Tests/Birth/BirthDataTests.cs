using NodaTime;
using NoxAeterna.Domain.Birth;

namespace NoxAeterna.Tests.Birth;

public sealed class BirthDataTests
{
    [Fact]
    public void BirthData_CanRepresentExactKnownBirthTime()
    {
        var birthData = new BirthData(
            new LocalBirthDateTime(new LocalDate(1990, 7, 14), new LocalTime(13, 45)),
            BirthTimeAccuracy.ExactTime,
            new BirthLocation("Moscow", 55.7558d, 37.6176d, "Russia"),
            new TimezoneId("Europe/Moscow"),
            "Birth certificate");

        Assert.True(birthData.LocalBirthDateTime.HasKnownTime);
        Assert.Equal(BirthTimeAccuracy.ExactTime, birthData.BirthTimeAccuracy);
        Assert.Equal("Birth certificate", birthData.SourceNote);
    }

    [Fact]
    public void BirthData_CanRepresentUnknownBirthTime()
    {
        var birthData = new BirthData(
            new LocalBirthDateTime(new LocalDate(1990, 7, 14)),
            BirthTimeAccuracy.UnknownTime,
            new BirthLocation("Moscow", 55.7558d, 37.6176d, "Russia"),
            new TimezoneId("Europe/Moscow"));

        Assert.False(birthData.LocalBirthDateTime.HasKnownTime);
        Assert.Equal(BirthTimeAccuracy.UnknownTime, birthData.BirthTimeAccuracy);
    }

    [Fact]
    public void BirthData_RejectsKnownTimeWithUnknownAccuracy()
    {
        Assert.Throws<ArgumentException>(() =>
            new BirthData(
                new LocalBirthDateTime(new LocalDate(1990, 7, 14), new LocalTime(13, 45)),
                BirthTimeAccuracy.UnknownTime,
                new BirthLocation("Moscow", 55.7558d, 37.6176d),
                new TimezoneId("Europe/Moscow")));
    }

    [Fact]
    public void BirthData_RejectsMissingTimeWithKnownAccuracy()
    {
        Assert.Throws<ArgumentException>(() =>
            new BirthData(
                new LocalBirthDateTime(new LocalDate(1990, 7, 14)),
                BirthTimeAccuracy.ExactTime,
                new BirthLocation("Moscow", 55.7558d, 37.6176d),
                new TimezoneId("Europe/Moscow")));
    }
}
