using NoxAeterna.Domain.Astrology;
using NoxAeterna.Rendering.Charts;

namespace NoxAeterna.App.Astrology;

/// <summary>
/// Represents one development-only chart rebuild result produced from validated workspace input.
/// </summary>
public sealed record DevelopmentChartBuildResult(
    NatalChart NatalChart,
    ChartRenderScene RenderScene);
