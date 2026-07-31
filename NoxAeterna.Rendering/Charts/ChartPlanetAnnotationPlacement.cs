using NoxAeterna.Domain.Astrology;

namespace NoxAeterna.Rendering.Charts;

/// <summary>
/// Represents one render-ready planet annotation group.
/// </summary>
public sealed record ChartPlanetAnnotationPlacement(
    CelestialBody Body,
    ChartVectorGlyph Glyph,
    string DegreeText,
    bool IsRetrograde);
