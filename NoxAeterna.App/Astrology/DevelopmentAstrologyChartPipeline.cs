using NodaTime;
using NoxAeterna.Astronomy.Calculation;
using NoxAeterna.Domain.Astrology;
using NoxAeterna.Domain.Birth;
using NoxAeterna.Geometry.Charts;
using NoxAeterna.Rendering.Charts;

namespace NoxAeterna.App.Astrology;

/// <summary>
/// Composes the current development-only birth-data-to-chart pipeline for the astrology workspace.
/// </summary>
public sealed class DevelopmentAstrologyChartPipeline
{
    private readonly IBirthMomentResolver _birthMomentResolver;
    private readonly IEphemerisCalculator _ephemerisCalculator;

    /// <summary>
    /// Initializes a new instance of the <see cref="DevelopmentAstrologyChartPipeline"/> class.
    /// </summary>
    public DevelopmentAstrologyChartPipeline(
        IBirthMomentResolver birthMomentResolver,
        IEphemerisCalculator ephemerisCalculator)
    {
        _birthMomentResolver = birthMomentResolver ?? throw new ArgumentNullException(nameof(birthMomentResolver));
        _ephemerisCalculator = ephemerisCalculator ?? throw new ArgumentNullException(nameof(ephemerisCalculator));
    }

    /// <summary>
    /// Builds a development-only natal chart and render scene from validated birth data.
    /// </summary>
    public DevelopmentChartBuildResult Build(BirthData birthData, TimeSpan technicalBirthTimeFallback)
    {
        var resolvableBirthData = CreateResolvableBirthData(birthData, technicalBirthTimeFallback);
        var resolvedBirthMoment = _birthMomentResolver.Resolve(resolvableBirthData);
        var calculationMoment = PreserveDeclaredBirthTimeAccuracy(birthData, resolvedBirthMoment, technicalBirthTimeFallback);

        var calculationRequest = new ChartCalculationRequest(
            calculationMoment,
            Enum.GetValues<CelestialBody>(),
            locationContext: birthData.BirthLocation,
            ephemerisSourceVersion: "development-demo");
        var calculationResult = _ephemerisCalculator.Calculate(calculationRequest);
        var natalChart = NatalChart.Create(
            calculationMoment,
            calculationResult.Positions,
            ephemerisSourceVersion: calculationResult.EphemerisSourceVersion);

        var layout = new CircularChartLayoutBuilder().Build(natalChart);
        var renderScene = ChartRenderScene.FromLayout(layout);

        return new DevelopmentChartBuildResult(natalChart, renderScene);
    }

    private static BirthData CreateResolvableBirthData(BirthData birthData, TimeSpan technicalBirthTimeFallback)
    {
        if (birthData.LocalBirthDateTime.HasKnownTime)
        {
            return birthData;
        }

        var fallbackTime = LocalTime.FromTicksSinceMidnight(technicalBirthTimeFallback.Ticks);

        return new BirthData(
            new LocalBirthDateTime(birthData.LocalBirthDateTime.Date, fallbackTime),
            BirthTimeAccuracy.ApproximateTime,
            birthData.BirthLocation,
            birthData.TimezoneId,
            AppendSourceNote(
                birthData.SourceNote,
                $"Technical fallback time applied for UnknownTime input: {fallbackTime:HH':'mm}."));
    }

    private static BirthMoment PreserveDeclaredBirthTimeAccuracy(
        BirthData originalBirthData,
        BirthMoment resolvedBirthMoment,
        TimeSpan technicalBirthTimeFallback)
    {
        if (originalBirthData.BirthTimeAccuracy != BirthTimeAccuracy.UnknownTime)
        {
            return resolvedBirthMoment;
        }

        return new BirthMoment(
            resolvedBirthMoment.OriginalLocalDateTime,
            resolvedBirthMoment.TimezoneId,
            resolvedBirthMoment.Instant,
            resolvedBirthMoment.ResolutionStatus,
            BirthTimeAccuracy.UnknownTime,
            AppendSourceNote(
                resolvedBirthMoment.SourceNote,
                $"Resolved using technical fallback time {technicalBirthTimeFallback:hh\\:mm} for UnknownTime input."));
    }

    private static string AppendSourceNote(string? sourceNote, string appendedNote) =>
        string.IsNullOrWhiteSpace(sourceNote)
            ? appendedNote
            : $"{sourceNote} {appendedNote}";
}
