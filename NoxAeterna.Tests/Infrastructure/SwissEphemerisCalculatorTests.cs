using NodaTime;
using NoxAeterna.Astronomy.Calculation;
using NoxAeterna.Domain.Astrology;
using NoxAeterna.Domain.Birth;
using NoxAeterna.Infrastructure.Ephemeris;
using NoxAeterna.Presentation.Astrology;

namespace NoxAeterna.Tests.Infrastructure;

public sealed class SwissEphemerisCalculatorTests
{
    [Fact]
    public void Calculate_ReturnsRequestedBodiesWithNormalizedLongitudes()
    {
        IEphemerisCalculator calculator = new SwissEphemerisCalculator();
        var request = new ChartCalculationRequest(
            CreateBirthMoment(),
            new[] { CelestialBody.Sun, CelestialBody.Moon, CelestialBody.Mars });

        var result = calculator.Calculate(request);

        Assert.Equal(request.RequestedBodies, result.Positions.Select(position => position.Body));
        Assert.All(result.Positions, position =>
        {
            Assert.InRange(position.EclipticLongitude.Degrees, 0d, 360d);
            Assert.Equal(position.EclipticLongitude.Sign, position.Sign);
        });
    }

    [Fact]
    public void Calculate_IsDeterministicForTheSameRequest()
    {
        IEphemerisCalculator calculator = new SwissEphemerisCalculator();
        var request = new ChartCalculationRequest(
            CreateBirthMoment(),
            Enum.GetValues<CelestialBody>());

        var first = calculator.Calculate(request);
        var second = calculator.Calculate(request);

        Assert.Equal(first.Positions, second.Positions);
        Assert.Equal(first.EphemerisSourceVersion, second.EphemerisSourceVersion);
    }

    [Fact]
    public void PragueNoonFallback_RemainsPhysicallyPlausibleAgainstKnownTimeSnapshot()
    {
        IEphemerisCalculator calculator = new SwissEphemerisCalculator();
        var knownTime = CreateBirthMoment();
        var noonFallback = new BirthMoment(
            new LocalDateTime(1990, 7, 14, 12, 0),
            new TimezoneId("Europe/Prague"),
            Instant.FromUtc(1990, 7, 14, 10, 0),
            TimeResolutionStatus.Resolved,
            BirthTimeAccuracy.UnknownTime,
            "UnknownTime noon sanity fixture");
        var bodies = Enum.GetValues<CelestialBody>();

        var known = calculator.Calculate(new ChartCalculationRequest(knownTime, bodies));
        var firstFallback = calculator.Calculate(new ChartCalculationRequest(noonFallback, bodies));
        var secondFallback = calculator.Calculate(new ChartCalculationRequest(noonFallback, bodies));

        Assert.Equal(firstFallback.Positions, secondFallback.Positions);
        Assert.All(
            firstFallback.Positions,
            position =>
            {
                Assert.True(double.IsFinite(position.EclipticLongitude.Degrees));
                Assert.InRange(position.EclipticLongitude.Degrees, 0d, 359.999999999999d);
            });

        foreach (var fallbackPosition in firstFallback.Positions)
        {
            var knownPosition = known.Positions.Single(position => position.Body == fallbackPosition.Body);
            var delta = CircularDelta(
                knownPosition.EclipticLongitude.Degrees,
                fallbackPosition.EclipticLongitude.Degrees);

            if (fallbackPosition.Body == CelestialBody.Moon)
            {
                Assert.InRange(delta, 0.5d, 2d);
            }
            else
            {
                Assert.InRange(delta, 0d, 0.25d);
            }
        }
    }

    [Fact]
    public void Calculate_UsesSwissEphNetMetadataAndCanFallbackWithoutExternalFiles()
    {
        IEphemerisCalculator calculator = new SwissEphemerisCalculator();
        var request = new ChartCalculationRequest(
            CreateBirthMoment(),
            new[] { CelestialBody.Sun });

        var result = calculator.Calculate(request);

        Assert.Contains("SwissEphNet", result.EphemerisSourceVersion, StringComparison.Ordinal);
        Assert.Contains("Moshier", result.EphemerisSourceVersion, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void HouseCalculator_ReturnsTwelveFiniteNormalizedPlacidusCuspsAndAngles()
    {
        IHouseCalculator calculator = new SwissEphemerisHouseCalculator();
        var request = new HouseCalculationRequest(
            CreateBirthMoment(),
            new BirthLocation("Prague, Czechia", 50.0755d, 14.4378d),
            HouseSystem.Placidus);

        var result = calculator.Calculate(request);

        Assert.True(result.IsAvailable);
        var houses = Assert.IsType<NatalHouses>(result.Houses);
        Assert.Equal(HouseSystem.Placidus, houses.HouseSystem);
        Assert.Equal(12, houses.Cusps.Count);
        Assert.All(houses.Cusps, cusp =>
        {
            Assert.True(double.IsFinite(cusp.Longitude.Degrees));
            Assert.InRange(cusp.Longitude.Degrees, 0d, 359.999999999999d);
        });
        Assert.NotNull(houses.Angles);
        Assert.True(double.IsFinite(houses.Angles.Ascendant.Degrees));
        Assert.True(double.IsFinite(houses.Angles.Midheaven.Degrees));
        Assert.Equal(
            houses.Cusps[0].Longitude.Degrees,
            houses.Angles.Ascendant.Degrees,
            precision: 8);
        Assert.Contains("SwissEphNet", houses.SourceMetadata, StringComparison.Ordinal);
    }

    [Fact]
    public void PragueGoldenFixture_MatchesExpectedPlanetsCuspsAndPrincipalAngles()
    {
        var birthMoment = CreateBirthMoment();
        Assert.Equal(Instant.FromUtc(1990, 7, 14, 11, 45), birthMoment.Instant);

        IEphemerisCalculator ephemerisCalculator = new SwissEphemerisCalculator();
        var planetResult = ephemerisCalculator.Calculate(
            new ChartCalculationRequest(birthMoment, Enum.GetValues<CelestialBody>()));
        var chart = NatalChart.Create(birthMoment, planetResult.Positions);
        var positionRows = PlanetPositionSummaryBuilder.Build(chart);
        var expectedPositions = new[]
        {
            (CelestialBody.Sun, ZodiacSign.Cancer, "21°47'", false),
            (CelestialBody.Moon, ZodiacSign.Aries, "09°12'", false),
            (CelestialBody.Mercury, ZodiacSign.Leo, "04°52'", false),
            (CelestialBody.Venus, ZodiacSign.Gemini, "23°12'", false),
            (CelestialBody.Mars, ZodiacSign.Taurus, "01°16'", false),
            (CelestialBody.Jupiter, ZodiacSign.Cancer, "22°19'", false),
            (CelestialBody.Saturn, ZodiacSign.Capricorn, "22°02'", true),
            (CelestialBody.Uranus, ZodiacSign.Capricorn, "07°00'", true),
            (CelestialBody.Neptune, ZodiacSign.Capricorn, "12°57'", true),
            (CelestialBody.Pluto, ZodiacSign.Scorpio, "15°00'", true)
        };

        Assert.Equal(expectedPositions.Length, planetResult.Positions.Count);
        for (var index = 0; index < expectedPositions.Length; index++)
        {
            var expected = expectedPositions[index];
            var actualPosition = planetResult.Positions[index];
            var actualRow = positionRows[index];
            Assert.Equal(expected.Item1, actualPosition.Body);
            Assert.Equal(expected.Item2, actualPosition.Sign);
            Assert.Equal(expected.Item3, actualRow.PositionText);
            Assert.Equal(expected.Item4, actualPosition.IsRetrograde);
        }

        IHouseCalculator houseCalculator = new SwissEphemerisHouseCalculator();
        var houseResult = houseCalculator.Calculate(
            new HouseCalculationRequest(
                birthMoment,
                new BirthLocation("Prague, Czechia", 50.0755d, 14.4378d),
                HouseSystem.Placidus));
        var houses = Assert.IsType<NatalHouses>(houseResult.Houses);
        var expectedCusps = new[]
        {
            203.4687440076d,
            230.1837604702d,
            263.0854176255d,
            300.5684847742d,
            334.8680247028d,
            2.2464963277d,
            23.4687440076d,
            50.1837604702d,
            83.0854176255d,
            120.5684847742d,
            154.8680247028d,
            182.2464963277d
        };

        Assert.Equal(12, houses.Cusps.Count);
        for (var index = 0; index < expectedCusps.Length; index++)
        {
            Assert.Equal(index + 1, houses.Cusps[index].HouseNumber.Value);
            Assert.InRange(
                Math.Abs(houses.Cusps[index].Longitude.Degrees - expectedCusps[index]),
                0d,
                0.0000001d);
        }

        Assert.NotNull(houses.Angles);
        Assert.InRange(
            Math.Abs(houses.Angles.Ascendant.Degrees - 203.4687440076d),
            0d,
            0.0000001d);
        Assert.InRange(
            Math.Abs(houses.Angles.Midheaven.Degrees - 120.5684847742d),
            0d,
            0.0000001d);
        Assert.Equal(houses.Cusps[0].Longitude, houses.Angles.Ascendant);
        Assert.Equal(houses.Cusps[9].Longitude, houses.Angles.Midheaven);
        Assert.Equal(
            ZodiacLongitude.Normalize(houses.Angles.Ascendant.Degrees + 180d),
            houses.Angles.Descendant.Degrees,
            precision: 10);
        Assert.Equal(
            ZodiacLongitude.Normalize(houses.Angles.Midheaven.Degrees + 180d),
            houses.Angles.ImumCoeli.Degrees,
            precision: 10);
    }

    [Fact]
    public void HouseCalculator_HighLatitudePlacidusFailureDoesNotExposeSwissFallbackCusps()
    {
        IHouseCalculator calculator = new SwissEphemerisHouseCalculator();
        var request = new HouseCalculationRequest(
            CreateBirthMoment(),
            new BirthLocation("High latitude fixture", 75d, 14.4378d),
            HouseSystem.Placidus);

        var result = calculator.Calculate(request);

        Assert.False(result.IsAvailable);
        Assert.Null(result.Houses);
        Assert.Equal(
            HouseCalculationFailureReason.UnsupportedGeographicConditions,
            result.FailureReason);
    }

    private static BirthMoment CreateBirthMoment() =>
        new(
            new LocalDateTime(1990, 7, 14, 13, 45),
            new TimezoneId("Europe/Prague"),
            Instant.FromUtc(1990, 7, 14, 11, 45),
            TimeResolutionStatus.Resolved,
            BirthTimeAccuracy.ExactTime,
            "Swiss ephemeris test fixture");

    private static double CircularDelta(double first, double second)
    {
        var delta = Math.Abs(first - second);
        return Math.Min(delta, 360d - delta);
    }
}
