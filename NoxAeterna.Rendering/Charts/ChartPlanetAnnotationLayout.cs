using Avalonia;

namespace NoxAeterna.Rendering.Charts;

/// <summary>
/// Represents the complete render-owned visual layout of one planet annotation.
/// </summary>
public sealed record ChartPlanetAnnotationLayout(
    ChartPlanetAnnotationPlacement Annotation,
    Point FinalAnchor,
    Rect GlyphBounds,
    Rect LabelBounds,
    Rect GlyphProtectedBounds,
    Rect LabelProtectedBounds,
    Rect VisualBounds,
    Rect ProtectedBounds,
    Point ConnectorStart,
    Point? ConnectorEndpoint,
    bool HasDisplacement);
