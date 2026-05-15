using NoxAeterna.Geometry.Charts;
using NoxAeterna.Rendering.Charts;

namespace NoxAeterna.App.Samples;

/// <summary>
/// Builds deterministic development-only chart render scenes for workspace verification.
/// </summary>
public static class DevelopmentSampleChartSceneFactory
{
    /// <summary>
    /// Creates a deterministic render scene for the astrology workspace foundation.
    /// </summary>
    /// <returns>A render-ready chart scene.</returns>
    public static ChartRenderScene Create()
    {
        return DevelopmentSampleChartBuildResultFactory.Create().RenderScene;
    }
}
