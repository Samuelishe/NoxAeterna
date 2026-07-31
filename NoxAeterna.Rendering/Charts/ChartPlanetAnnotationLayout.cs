using Avalonia;
using NoxAeterna.Domain.Astrology;
using NoxAeterna.Geometry.Charts;

namespace NoxAeterna.Rendering.Charts;

/// <summary>
/// Represents the complete render-owned visual layout of one planet annotation.
/// </summary>
public sealed record ChartPlanetAnnotationLayout(
    ChartPlanetAnnotationPlacement Annotation,
    RadialPoint SourceRadialPoint,
    Point SourceAnchor,
    Rect SourceMarkerBounds,
    ZodiacLongitude GlyphLongitude,
    RadialPoint GlyphRadialPoint,
    Point GlyphAnchor,
    Rect GlyphBounds,
    Rect GlyphProtectedBounds,
    Point LabelAnchor,
    Rect LabelBounds,
    Rect LabelProtectedBounds,
    Rect VisualBounds,
    Rect ProtectedBounds,
    Point SourceLeaderStart,
    Point SourceLeaderEndpoint,
    Point? LabelLeaderStart,
    Point? LabelLeaderEndpoint,
    bool HasGlyphDisplacement,
    bool HasLabelDisplacement,
    bool IsCrowded,
    double OverlapArea,
    ZodiacSign SourceSign,
    HouseNumber? SourceHouseNumber);
