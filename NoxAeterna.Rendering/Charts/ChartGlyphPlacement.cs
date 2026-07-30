using NoxAeterna.Geometry.Charts;

namespace NoxAeterna.Rendering.Charts;

/// <summary>
/// Represents a vector glyph prepared for chart rendering.
/// </summary>
public sealed record ChartGlyphPlacement
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChartGlyphPlacement"/> class.
    /// </summary>
    public ChartGlyphPlacement(
        ChartVectorGlyph glyph,
        RadialPoint anchorPoint,
        double size,
        ChartGlyphStyle style)
    {
        if (!double.IsFinite(size) || size <= 0d)
        {
            throw new ArgumentOutOfRangeException(nameof(size), "Glyph size must be a finite positive number.");
        }

        Glyph = glyph ?? throw new ArgumentNullException(nameof(glyph));
        AnchorPoint = anchorPoint;
        Size = size;
        Style = style;
    }

    /// <summary>
    /// Gets the project-owned vector glyph.
    /// </summary>
    public ChartVectorGlyph Glyph { get; }

    /// <summary>
    /// Gets the normalized chart anchor.
    /// </summary>
    public RadialPoint AnchorPoint { get; }

    /// <summary>
    /// Gets the desired glyph size in device-independent pixels.
    /// </summary>
    public double Size { get; }

    /// <summary>
    /// Gets the glyph visual role.
    /// </summary>
    public ChartGlyphStyle Style { get; }
}
