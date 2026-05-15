using NodaTime;
using NoxAeterna.Astronomy.Calculation;
using NoxAeterna.Domain.Astrology;
using SwissEphNet;

namespace NoxAeterna.Infrastructure.Ephemeris;

/// <summary>
/// Calculates real planetary positions through SwissEphNet while keeping Swiss Ephemeris types inside the adapter boundary.
/// </summary>
public sealed class SwissEphemerisCalculator : IEphemerisCalculator
{
    private const int CalculationFlags = SwissEph.SEFLG_SWIEPH | SwissEph.SEFLG_SPEED;
    private readonly string? _ephemerisDataPath;

    /// <summary>
    /// Initializes a new instance of the <see cref="SwissEphemerisCalculator"/> class.
    /// </summary>
    /// <param name="ephemerisDataPath">
    /// Optional directory containing Swiss Ephemeris data files.
    /// When omitted, SwissEphNet can still calculate positions by falling back to the built-in Moshier mode.
    /// </param>
    public SwissEphemerisCalculator(string? ephemerisDataPath = null)
    {
        _ephemerisDataPath = string.IsNullOrWhiteSpace(ephemerisDataPath) ? null : ephemerisDataPath.Trim();
    }

    /// <inheritdoc />
    public ChartCalculationResult Calculate(ChartCalculationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        using var swissEph = new SwissEph();

        if (_ephemerisDataPath is not null)
        {
            swissEph.swe_set_ephe_path(_ephemerisDataPath);
        }

        var julianDayUt = ToJulianDayUt(swissEph, request.CalculationMoment.Instant);
        var positions = new List<PlanetPosition>(request.RequestedBodies.Count);
        var calculationMessages = new List<string>();

        foreach (var body in request.RequestedBodies)
        {
            positions.Add(CalculatePosition(swissEph, julianDayUt, body, calculationMessages));
        }

        return new ChartCalculationResult(
            request.CalculationMoment,
            positions,
            ephemerisSourceVersion: BuildSourceVersion(swissEph, calculationMessages));
    }

    private static PlanetPosition CalculatePosition(
        SwissEph swissEph,
        double julianDayUt,
        CelestialBody body,
        ICollection<string> calculationMessages)
    {
        var xx = new double[6];
        var errorText = string.Empty;
        var resultFlags = swissEph.swe_calc_ut(
            julianDayUt,
            MapBody(body),
            CalculationFlags,
            xx,
            ref errorText);

        if (resultFlags < 0)
        {
            throw new InvalidOperationException(
                $"Swiss Ephemeris calculation failed for body '{body}'. {errorText}".Trim());
        }

        if (!string.IsNullOrWhiteSpace(errorText))
        {
            calculationMessages.Add(errorText.Trim());
        }

        return new PlanetPosition(
            body,
            new ZodiacLongitude(xx[0]),
            isRetrograde: xx[3] < 0d,
            eclipticLatitude: xx[1],
            speed: xx[3]);
    }

    private static double ToJulianDayUt(SwissEph swissEph, Instant instant)
    {
        var utc = instant.InUtc();
        var hour = utc.TimeOfDay.TickOfDay / (double)TimeSpan.TicksPerHour;

        return swissEph.swe_julday(
            utc.Year,
            utc.Month,
            utc.Day,
            hour,
            SwissEph.SE_GREG_CAL);
    }

    private static string BuildSourceVersion(SwissEph swissEph, IEnumerable<string> calculationMessages)
    {
        var version = swissEph.swe_version();
        var usesMoshierFallback = calculationMessages.Any(message =>
            message.Contains("using Moshier eph.", StringComparison.OrdinalIgnoreCase));

        return usesMoshierFallback
            ? $"SwissEphNet {version} (Moshier fallback)"
            : $"SwissEphNet {version} (Swiss ephemeris files)";
    }

    private static int MapBody(CelestialBody body) =>
        body switch
        {
            CelestialBody.Sun => SwissEph.SE_SUN,
            CelestialBody.Moon => SwissEph.SE_MOON,
            CelestialBody.Mercury => SwissEph.SE_MERCURY,
            CelestialBody.Venus => SwissEph.SE_VENUS,
            CelestialBody.Mars => SwissEph.SE_MARS,
            CelestialBody.Jupiter => SwissEph.SE_JUPITER,
            CelestialBody.Saturn => SwissEph.SE_SATURN,
            CelestialBody.Uranus => SwissEph.SE_URANUS,
            CelestialBody.Neptune => SwissEph.SE_NEPTUNE,
            CelestialBody.Pluto => SwissEph.SE_PLUTO,
            _ => throw new ArgumentOutOfRangeException(nameof(body), body, "Unsupported celestial body for Swiss Ephemeris calculation.")
        };
}
