using NoxAeterna.App.Astrology;
using NoxAeterna.Astronomy.Calculation;
using NoxAeterna.Astronomy.Time;
using NoxAeterna.Infrastructure.Ephemeris;

namespace NoxAeterna.App.Samples;

/// <summary>
/// Builds deterministic sample chart results for tests and explicit debug verification.
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
            new SwissEphemerisCalculator(),
            new SwissEphemerisHouseCalculator());

        return pipeline.Build(
            DevelopmentSampleBirthDataFactory.Create(),
            TimeSpan.FromHours(12));
    }
}
