using NodaTime;
using NoxAeterna.Domain.Birth;

namespace NoxAeterna.Tests.Birth;

public sealed class BirthMomentTests
{
    [Fact]
    public void BirthMoment_StoresLocalTimeAndUtcInstant()
    {
        var localDateTime = new LocalDateTime(1990, 7, 14, 13, 45);
        var instant = Instant.FromUtc(1990, 7, 14, 10, 45);

        var birthMoment = new BirthMoment(
            localDateTime,
            new TimezoneId("Europe/Moscow"),
            instant,
            TimeResolutionStatus.Resolved,
            BirthTimeAccuracy.ExactTime,
            "Birth certificate");

        Assert.Equal(localDateTime, birthMoment.OriginalLocalDateTime);
        Assert.Equal(instant, birthMoment.Instant);
        Assert.Equal(TimeResolutionStatus.Resolved, birthMoment.ResolutionStatus);
        Assert.Equal(BirthTimeAccuracy.ExactTime, birthMoment.BirthTimeAccuracy);
        Assert.Equal("Birth certificate", birthMoment.SourceNote);
    }
}
