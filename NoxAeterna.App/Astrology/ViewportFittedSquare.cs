using Avalonia;
using Avalonia.Controls;

namespace NoxAeterna.App.Astrology;

/// <summary>
/// Measures its child as a square constrained by both content width and the real chart-column viewport height.
/// </summary>
public sealed class ViewportFittedSquare : Decorator
{
    private double _viewportHeightConstraint;

    /// <summary>
    /// Prevents an excessively large chart on unusually large displays.
    /// </summary>
    public const double MaximumSide = 1100d;

    /// <summary>
    /// Gets or sets vertical space reserved for the chart panel title, padding, and scroll viewport margins.
    /// </summary>
    public double ReservedVerticalSpace { get; set; } = 96d;

    /// <summary>
    /// Resolves a deterministic square side from finite width and viewport-height constraints.
    /// </summary>
    public static double CalculateSide(
        double availableWidth,
        double availableViewportHeight,
        double reservedVerticalSpace = 0d,
        double fallbackWidth = 0d)
    {
        var width = double.IsFinite(availableWidth)
            ? availableWidth
            : fallbackWidth;
        var height = double.IsFinite(availableViewportHeight)
            ? availableViewportHeight - Math.Max(0d, reservedVerticalSpace)
            : 0d;

        if (!double.IsFinite(width) || width <= 0d || height <= 0d)
        {
            return 0d;
        }

        return Math.Min(Math.Min(width, height), MaximumSide);
    }

    /// <summary>
    /// Supplies the finite height of the owning column viewport before scroll content is measured.
    /// </summary>
    internal void SetViewportHeightConstraint(double viewportHeight)
    {
        _viewportHeightConstraint = double.IsFinite(viewportHeight) && viewportHeight > 0d
            ? viewportHeight
            : 0d;
    }

    /// <inheritdoc />
    protected override Size MeasureOverride(Size availableSize)
    {
        var side = CalculateSide(
            availableSize.Width,
            _viewportHeightConstraint,
            ReservedVerticalSpace,
            Child?.DesiredSize.Width ?? 0d);
        Child?.Measure(new Size(side, side));
        return new Size(side, side);
    }

    /// <inheritdoc />
    protected override Size ArrangeOverride(Size finalSize)
    {
        var side = CalculateSide(
            finalSize.Width,
            _viewportHeightConstraint,
            ReservedVerticalSpace,
            Child?.DesiredSize.Width ?? 0d);
        var horizontalOffset = Math.Max(0d, (finalSize.Width - side) / 2d);
        Child?.Arrange(new Rect(horizontalOffset, 0d, side, side));
        return finalSize;
    }
}
