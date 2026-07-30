using Avalonia;
using Avalonia.Media;

namespace NoxAeterna.Rendering.Charts;

/// <summary>
/// Represents a project-owned vector chart symbol in a stable unit coordinate system.
/// </summary>
public sealed record ChartVectorGlyph
{
    private readonly Lazy<Avalonia.Media.Geometry> _geometry;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChartVectorGlyph"/> class.
    /// </summary>
    public ChartVectorGlyph(string id, string pathData, Rect unitBounds)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("A vector glyph identifier must not be blank.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(pathData))
        {
            throw new ArgumentException("Vector glyph path data must not be blank.", nameof(pathData));
        }

        if (!IsFinite(unitBounds) || unitBounds.Width <= 0d || unitBounds.Height <= 0d)
        {
            throw new ArgumentException("Vector glyph unit bounds must be finite and non-empty.", nameof(unitBounds));
        }

        Id = id;
        PathData = pathData;
        UnitBounds = unitBounds;
        _geometry = new Lazy<Avalonia.Media.Geometry>(
            () => Avalonia.Media.Geometry.Parse(PathData),
            LazyThreadSafetyMode.ExecutionAndPublication);
    }

    /// <summary>
    /// Gets the stable glyph identifier.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the non-empty Avalonia path data for the vector geometry.
    /// </summary>
    public string PathData { get; }

    /// <summary>
    /// Gets the deterministic bounds in the catalog unit coordinate system.
    /// </summary>
    public Rect UnitBounds { get; }

    /// <summary>
    /// Materializes the Avalonia geometry after the application render platform has been initialized.
    /// </summary>
    public Avalonia.Media.Geometry CreateGeometry() => _geometry.Value;

    private static bool IsFinite(Rect bounds) =>
        double.IsFinite(bounds.X) &&
        double.IsFinite(bounds.Y) &&
        double.IsFinite(bounds.Width) &&
        double.IsFinite(bounds.Height);
}
