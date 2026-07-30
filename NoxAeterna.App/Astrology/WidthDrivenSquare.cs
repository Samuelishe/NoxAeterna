using Avalonia;
using Avalonia.Controls;

namespace NoxAeterna.App.Astrology;

/// <summary>
/// Measures its child as a square whose side follows the available width.
/// </summary>
public sealed class WidthDrivenSquare : Decorator
{
    /// <summary>
    /// Prevents an excessively large chart on unusually wide displays.
    /// </summary>
    public const double MaximumSide = 1100d;

    /// <summary>
    /// Resolves a deterministic square side from the layout constraint.
    /// </summary>
    public static double CalculateSide(double availableWidth, double fallbackWidth = 0d)
    {
        var candidate = double.IsFinite(availableWidth)
            ? availableWidth
            : fallbackWidth;

        if (!double.IsFinite(candidate) || candidate <= 0d)
        {
            return 0d;
        }

        return Math.Min(candidate, MaximumSide);
    }

    /// <inheritdoc />
    protected override Size MeasureOverride(Size availableSize)
    {
        var side = CalculateSide(availableSize.Width, Child?.DesiredSize.Width ?? 0d);
        Child?.Measure(new Size(side, side));
        return new Size(side, side);
    }

    /// <inheritdoc />
    protected override Size ArrangeOverride(Size finalSize)
    {
        var side = CalculateSide(finalSize.Width, Child?.DesiredSize.Width ?? 0d);
        var horizontalOffset = Math.Max(0d, (finalSize.Width - side) / 2d);
        Child?.Arrange(new Rect(horizontalOffset, 0d, side, side));
        return finalSize;
    }
}
