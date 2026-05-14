using NodaTime;
using NoxAeterna.Astronomy.Time;
using NoxAeterna.Domain.Birth;

namespace NoxAeterna.Tests.Birth;

public sealed class TzdbBirthMomentResolverTests
{
    private static readonly TzdbBirthMomentResolver Resolver = new();

    [Fact]
    public void Resolve_HandlesNonAmbiguousLocalTime()
    {
        var birthData = new BirthData(
            new LocalBirthDateTime(new LocalDate(1990, 7, 14), new LocalTime(13, 45)),
            BirthTimeAccuracy.ExactTime,
            new BirthLocation("Moscow", 55.7558d, 37.6176d),
            new TimezoneId("Europe/Moscow"));

        var birthMoment = Resolver.Resolve(birthData);

        Assert.Equal(new LocalDateTime(1990, 7, 14, 13, 45), birthMoment.OriginalLocalDateTime);
        Assert.Equal(Instant.FromUtc(1990, 7, 14, 9, 45), birthMoment.Instant);
        Assert.Equal(TimeResolutionStatus.Resolved, birthMoment.ResolutionStatus);
    }

    [Fact]
    public void Resolve_UsesEarlierOccurrenceForAmbiguousTime()
    {
        var birthData = new BirthData(
            new LocalBirthDateTime(new LocalDate(2024, 11, 3), new LocalTime(1, 30)),
            BirthTimeAccuracy.ExactTime,
            new BirthLocation("New York", 40.7128d, -74.0060d),
            new TimezoneId("America/New_York"));

        var birthMoment = Resolver.Resolve(birthData);

        Assert.Equal(TimeResolutionStatus.AmbiguousResolvedEarlier, birthMoment.ResolutionStatus);
        Assert.Equal(Instant.FromUtc(2024, 11, 3, 5, 30), birthMoment.Instant);
    }

    [Fact]
    public void Resolve_ShiftsForwardForInvalidTime()
    {
        var birthData = new BirthData(
            new LocalBirthDateTime(new LocalDate(2024, 3, 10), new LocalTime(2, 30)),
            BirthTimeAccuracy.ExactTime,
            new BirthLocation("New York", 40.7128d, -74.0060d),
            new TimezoneId("America/New_York"));

        var birthMoment = Resolver.Resolve(birthData);

        Assert.Equal(TimeResolutionStatus.InvalidShiftedForward, birthMoment.ResolutionStatus);
        Assert.Equal(Instant.FromUtc(2024, 3, 10, 7, 30), birthMoment.Instant);
    }

    [Fact]
    public void Resolve_RejectsUnknownBirthTime()
    {
        var birthData = new BirthData(
            new LocalBirthDateTime(new LocalDate(1990, 7, 14)),
            BirthTimeAccuracy.UnknownTime,
            new BirthLocation("Moscow", 55.7558d, 37.6176d),
            new TimezoneId("Europe/Moscow"));

        Assert.Throws<InvalidOperationException>(() => Resolver.Resolve(birthData));
    }
}
