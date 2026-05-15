using NoxAeterna.Geometry.Charts;

namespace NoxAeterna.Rendering.Charts;

/// <summary>
/// Represents a prepared chart text label ready for rendering.
/// </summary>
public sealed record ChartTextLabel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChartTextLabel"/> class.
    /// </summary>
    /// <param name="text">The visible label text.</param>
    /// <param name="anchorPoint">The normalized chart anchor point.</param>
    /// <param name="fontSize">The font size in device-independent pixels.</param>
    /// <param name="style">The label role.</param>
    public ChartTextLabel(
        string text,
        RadialPoint anchorPoint,
        double fontSize,
        ChartTextLabelStyle style)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Chart label text must not be blank.", nameof(text));
        }

        if (double.IsNaN(fontSize) || double.IsInfinity(fontSize) || fontSize <= 0d)
        {
            throw new ArgumentOutOfRangeException(nameof(fontSize), "Font size must be a finite positive number.");
        }

        Text = text;
        AnchorPoint = anchorPoint;
        FontSize = fontSize;
        Style = style;
    }

    /// <summary>
    /// Gets the visible label text.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// Gets the normalized chart anchor point.
    /// </summary>
    public RadialPoint AnchorPoint { get; }

    /// <summary>
    /// Gets the font size in device-independent pixels.
    /// </summary>
    public double FontSize { get; }

    /// <summary>
    /// Gets the label role.
    /// </summary>
    public ChartTextLabelStyle Style { get; }
}
