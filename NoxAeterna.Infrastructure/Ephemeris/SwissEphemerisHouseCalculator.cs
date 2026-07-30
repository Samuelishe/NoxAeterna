using NodaTime;
using NoxAeterna.Astronomy.Calculation;
using NoxAeterna.Domain.Astrology;
using SwissEphNet;

namespace NoxAeterna.Infrastructure.Ephemeris;

/// <summary>
/// Calculates natal houses through SwissEphNet while containing all provider-specific details.
/// </summary>
public sealed class SwissEphemerisHouseCalculator : IHouseCalculator
{
    private const char PlacidusSystemCode = 'P';
    private const int CuspArrayLength = 13;
    private const int AngleArrayLength = 10;

    /// <inheritdoc />
    public HouseCalculationResult Calculate(HouseCalculationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        using var swissEph = new SwissEph();
        var julianDayUt = ToJulianDayUt(swissEph, request.CalculationMoment.Instant);
        var cusps = new double[CuspArrayLength];
        var anglePoints = new double[AngleArrayLength];
        var systemCode = MapHouseSystem(request.HouseSystem);

        var returnCode = swissEph.swe_houses(
            julianDayUt,
            request.Location.Latitude,
            request.Location.Longitude,
            systemCode,
            cusps,
            anglePoints);

        var sourceMetadata = $"SwissEphNet {swissEph.swe_version()} ({request.HouseSystem} houses)";

        // Swiss Ephemeris may fill the arrays with a fallback structure even when
        // Placidus itself failed. A non-zero return must therefore remain unavailable.
        if (returnCode != 0)
        {
            return HouseCalculationResult.Unavailable(
                request.HouseSystem,
                HouseCalculationFailureReason.UnsupportedGeographicConditions,
                sourceMetadata);
        }

        if (!cusps.Skip(1).Take(12).All(double.IsFinite) ||
            !double.IsFinite(anglePoints[SwissEph.SE_ASC]) ||
            !double.IsFinite(anglePoints[SwissEph.SE_MC]))
        {
            return HouseCalculationResult.Unavailable(
                request.HouseSystem,
                HouseCalculationFailureReason.InvalidProviderOutput,
                sourceMetadata);
        }

        try
        {
            var houseCusps = Enumerable.Range(1, 12)
                .Select(index => new HouseCusp(
                    new HouseNumber(index),
                    new ZodiacLongitude(cusps[index])))
                .ToArray();

            var angles = new ChartAngles(
                new ZodiacLongitude(anglePoints[SwissEph.SE_ASC]),
                new ZodiacLongitude(anglePoints[SwissEph.SE_MC]));

            return HouseCalculationResult.Available(
                NatalHouses.CreateAvailable(
                    request.HouseSystem,
                    houseCusps,
                    angles,
                    sourceMetadata));
        }
        catch (ArgumentException)
        {
            return HouseCalculationResult.Unavailable(
                request.HouseSystem,
                HouseCalculationFailureReason.InvalidProviderOutput,
                sourceMetadata);
        }
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

    private static char MapHouseSystem(HouseSystem houseSystem) =>
        houseSystem switch
        {
            HouseSystem.Placidus => PlacidusSystemCode,
            _ => throw new ArgumentOutOfRangeException(
                nameof(houseSystem),
                houseSystem,
                "Unsupported house system for Swiss Ephemeris calculation.")
        };
}
