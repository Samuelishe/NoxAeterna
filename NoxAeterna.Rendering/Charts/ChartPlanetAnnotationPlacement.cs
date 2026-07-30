using NoxAeterna.Domain.Astrology;
using NoxAeterna.Geometry.Charts;

namespace NoxAeterna.Rendering.Charts;

/// <summary>
/// Represents one render-ready planet annotation group.
/// </summary>
public sealed record ChartPlanetAnnotationPlacement(
    CelestialBody Body,
    ChartVectorGlyph Glyph,
    RadialPoint AnchorPoint,
    string DegreeText,
    bool IsRetrograde,
    bool HasDisplacement);
