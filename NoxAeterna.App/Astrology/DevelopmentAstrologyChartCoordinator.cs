using NoxAeterna.Presentation.Astrology;
using NoxAeterna.Rendering.Charts;

namespace NoxAeterna.App.Astrology;

/// <summary>
/// Holds the current chart state for the astrology workspace and rebuilds it from validated input.
/// </summary>
public sealed class DevelopmentAstrologyChartCoordinator
{
    private readonly DevelopmentAstrologyChartPipeline _chartPipeline;

    /// <summary>
    /// Initializes a new instance of the <see cref="DevelopmentAstrologyChartCoordinator"/> class.
    /// </summary>
    public DevelopmentAstrologyChartCoordinator(DevelopmentAstrologyChartPipeline chartPipeline)
    {
        _chartPipeline = chartPipeline ?? throw new ArgumentNullException(nameof(chartPipeline));
    }

    /// <summary>
    /// Gets the current render scene.
    /// </summary>
    public ChartRenderScene? CurrentScene { get; private set; }

    /// <summary>
    /// Gets the last successfully built natal chart.
    /// </summary>
    public DevelopmentChartBuildResult? CurrentBuildResult { get; private set; }

    /// <summary>
    /// Gets whether a chart has been successfully built during the current application session.
    /// </summary>
    public bool HasChart => CurrentBuildResult is not null;

    /// <summary>
    /// Attempts to rebuild the current chart from the supplied birth-data input state.
    /// </summary>
    public bool TryBuild(BirthDataInputViewModel birthDataInput)
    {
        ArgumentNullException.ThrowIfNull(birthDataInput);

        if (!birthDataInput.TryCreateBirthData(out var birthData))
        {
            return false;
        }

        var buildResult = _chartPipeline.Build(birthData, birthDataInput.EffectiveTechnicalBirthTime);
        CurrentBuildResult = buildResult;
        CurrentScene = buildResult.RenderScene;
        return true;
    }
}
