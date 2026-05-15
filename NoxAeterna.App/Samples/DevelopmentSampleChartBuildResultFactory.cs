using NoxAeterna.Geometry.Charts;
using NoxAeterna.App.Astrology;
using NoxAeterna.Rendering.Charts;

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
        var natalChart = DevelopmentSampleNatalChartFactory.Create();
        var layout = new CircularChartLayoutBuilder().Build(natalChart);
        var scene = ChartRenderScene.FromLayout(layout);

        return new DevelopmentChartBuildResult(natalChart, scene);
    }
}
