using NoxAeterna.Presentation.Astrology;
using NoxAeterna.Rendering.Charts;

namespace NoxAeterna.App.Astrology;

/// <summary>
/// Holds the current development-only chart state for the astrology workspace and rebuilds it from validated input.
/// </summary>
public sealed class DevelopmentAstrologyChartCoordinator
{
    private readonly DevelopmentAstrologyChartPipeline _chartPipeline;

    /// <summary>
    /// Initializes a new instance of the <see cref="DevelopmentAstrologyChartCoordinator"/> class.
    /// </summary>
    public DevelopmentAstrologyChartCoordinator(
        DevelopmentAstrologyChartPipeline chartPipeline,
        ChartRenderScene initialScene)
    {
        _chartPipeline = chartPipeline ?? throw new ArgumentNullException(nameof(chartPipeline));
        CurrentScene = initialScene ?? throw new ArgumentNullException(nameof(initialScene));
    }

    /// <summary>
    /// Gets the current render scene.
    /// </summary>
    public ChartRenderScene CurrentScene { get; private set; }

    /// <summary>
    /// Gets the last successfully built natal chart.
    /// </summary>
    public DevelopmentChartBuildResult? CurrentBuildResult { get; private set; }

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
