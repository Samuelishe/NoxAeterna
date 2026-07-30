using NodaTime;
using NoxAeterna.Domain.Astrology;
using NoxAeterna.Domain.Birth;
using NoxAeterna.Presentation.Astrology;

namespace NoxAeterna.Tests.Presentation;

public sealed class ChartAngleSummaryBuilderTests
{
    [Fact]
    public void Build_FormatsAscendantAndMidheavenWithMinutePrecision()
    {
        var houses = NatalHouses.CreateAvailable(
            HouseSystem.Placidus,
            CreateCusps(15.3833d, 285.75d),
            new ChartAngles(new ZodiacLongitude(15.3833d), new ZodiacLongitude(285.75d)));
        var chart = CreateChart(houses);

        var summary = ChartAngleSummaryBuilder.Build(chart);

        Assert.True(summary.IsAvailable);
        Assert.Collection(
            summary.Rows,
            ascendant =>
            {
                Assert.Equal("ui.chart.angles.ascendant", ascendant.AngleLabelKey.Value);
                Assert.Equal("ui.zodiac.aries", ascendant.SignLabelKey.Value);
                Assert.Equal("15°23'", ascendant.PositionText);
            },
            midheaven =>
            {
                Assert.Equal("ui.chart.angles.midheaven", midheaven.AngleLabelKey.Value);
                Assert.Equal("ui.zodiac.capricorn", midheaven.SignLabelKey.Value);
                Assert.Equal("15°45'", midheaven.PositionText);
            });
    }

    [Fact]
    public void Build_UsesDedicatedUnknownTimeStatus()
    {
        var chart = CreateChart(
            NatalHouses.CreateUnavailable(
                HouseSystem.Placidus,
                NatalHousesAvailability.UnavailableUnknownTime));

        var summary = ChartAngleSummaryBuilder.Build(chart);

        Assert.False(summary.IsAvailable);
        Assert.Empty(summary.Rows);
        Assert.Equal(
            "ui.chart.angles.unavailable.unknown_time",
            summary.UnavailableStatusKey?.Value);
    }

    private static IEnumerable<HouseCusp> CreateCusps(double ascendant, double midheaven)
    {
        var values = new[]
        {
            ascendant, 44d, 74d, 105.75d, 134d, 165d,
            ascendant + 180d, 224d, 254d, midheaven, 314d, 345d
        };

        return values.Select((longitude, index) =>
            new HouseCusp(new HouseNumber(index + 1), new ZodiacLongitude(longitude)));
    }

    private static NatalChart CreateChart(NatalHouses houses) =>
        NatalChart.Create(
            new BirthMoment(
                new LocalDateTime(1990, 7, 14, 13, 45),
                new TimezoneId("Europe/Prague"),
                Instant.FromUtc(1990, 7, 14, 11, 45),
                TimeResolutionStatus.Resolved,
                BirthTimeAccuracy.ExactTime),
            [new PlanetPosition(CelestialBody.Sun, new ZodiacLongitude(10d), false)],
            houses: houses);
}
