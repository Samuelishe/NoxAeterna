using NodaTime;
using NoxAeterna.Domain.Astrology;
using NoxAeterna.Domain.Birth;
using NoxAeterna.Presentation.Astrology;

namespace NoxAeterna.Tests.Presentation;

public sealed class PlanetPositionSummaryBuilderTests
{
    [Theory]
    [InlineData(0, "00°00'")]
    [InlineData(14.3833, "14°22'")]
    [InlineData(29.999, "29°59'")]
    public void FormatDegree_FormatsReadableDegreeAndMinuteText(double value, string expected)
    {
        var formatted = PlanetPositionSummaryBuilder.FormatDegree(value);

        Assert.Equal(expected, formatted);
    }

    [Fact]
    public void Build_CreatesDeterministicRowsWithRetrogradeState()
    {
        var chart = NatalChart.Create(
            new BirthMoment(
                new LocalDateTime(1990, 7, 14, 13, 45),
                new TimezoneId("Europe/Prague"),
                Instant.FromUtc(1990, 7, 14, 11, 45),
                TimeResolutionStatus.Resolved,
                BirthTimeAccuracy.ExactTime,
                "Position summary fixture"),
            new[]
            {
                new PlanetPosition(CelestialBody.Mars, new ZodiacLongitude(225.25d), true),
                new PlanetPosition(CelestialBody.Sun, new ZodiacLongitude(44.5d), false)
            });

        var rows = PlanetPositionSummaryBuilder.Build(chart);

        Assert.Collection(
            rows,
            row =>
            {
                Assert.Equal("☉", row.Glyph);
                Assert.Equal("ui.planet.sun", row.BodyLabelKey.Value);
                Assert.Equal("ui.zodiac.taurus", row.SignLabelKey.Value);
                Assert.Equal("14°30'", row.DegreeText);
                Assert.False(row.IsRetrograde);
            },
            row =>
            {
                Assert.Equal("♂", row.Glyph);
                Assert.Equal("ui.planet.mars", row.BodyLabelKey.Value);
                Assert.Equal("ui.zodiac.scorpio", row.SignLabelKey.Value);
                Assert.Equal("15°15'", row.DegreeText);
                Assert.True(row.IsRetrograde);
            });
    }
}
