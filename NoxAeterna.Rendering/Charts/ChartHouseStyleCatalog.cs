using NoxAeterna.Geometry.Charts;

namespace NoxAeterna.Rendering.Charts;

/// <summary>
/// Supplies a restrained hierarchy for house cusps and principal axes.
/// </summary>
public static class ChartHouseStyleCatalog
{
    /// <summary>
    /// Gets the style for ordinary house cusp lines.
    /// </summary>
    public static ChartHouseVisualStyle GetCusp(ChartRenderPalette palette)
    {
        ArgumentNullException.ThrowIfNull(palette);
        return new ChartHouseVisualStyle(palette.HouseCuspColor, 0.58d, 1d);
    }

    /// <summary>
    /// Gets the stronger style for one principal chart axis.
    /// </summary>
    public static ChartHouseVisualStyle GetAxis(
        ChartAngleAxisType axisType,
        ChartRenderPalette palette)
    {
        ArgumentNullException.ThrowIfNull(palette);

        return axisType switch
        {
            ChartAngleAxisType.AscendantDescendant =>
                new ChartHouseVisualStyle(palette.AngleAxisColor, 0.88d, 1d),
            ChartAngleAxisType.MidheavenImumCoeli =>
                new ChartHouseVisualStyle(palette.AngleAxisColor, 0.76d, 0.9d),
            _ => throw new ArgumentOutOfRangeException(
                nameof(axisType),
                axisType,
                "Unsupported chart angle axis.")
        };
    }
}
