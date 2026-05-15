using NoxAeterna.App.Astrology;
using NoxAeterna.Astronomy.Calculation;
using NoxAeterna.Astronomy.Time;
using NoxAeterna.Infrastructure.Ephemeris;

namespace NoxAeterna.App.Samples;

/// <summary>
/// Builds deterministic sample chart results for initial workspace startup.
/// </summary>
public static class DevelopmentSampleChartBuildResultFactory
{
    /// <summary>
    /// Creates a deterministic sample chart build result.
    /// </summary>
    public static DevelopmentChartBuildResult Create()
    {
        var pipeline = new DevelopmentAstrologyChartPipeline(
            new TzdbBirthMomentResolver(),
            new SwissEphemerisCalculator());

        return pipeline.Build(
            DevelopmentSampleBirthDataFactory.Create(),
            TimeSpan.FromHours(12));
    }
}
