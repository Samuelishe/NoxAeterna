using NoxAeterna.Domain.Astrology;
using Avalonia.Media;

namespace NoxAeterna.Rendering.Charts;

/// <summary>
/// Supplies original project-owned vector astrology glyphs without platform font fallback.
/// </summary>
public static class ChartGlyphCatalog
{
    private static readonly Avalonia.Rect UnitBounds = new(-0.55d, -0.65d, 1.1d, 1.3d);

    private static readonly IReadOnlyDictionary<ZodiacSign, ChartVectorGlyph> SignGlyphs =
        new Dictionary<ZodiacSign, ChartVectorGlyph>
        {
            [ZodiacSign.Aries] = Create("zodiac.aries", "M 0 0.45 L 0 -0.05 C 0 -0.34 -0.34 -0.5 -0.45 -0.22 C -0.5 -0.05 -0.35 0.08 -0.2 0.08 M 0 -0.05 C 0 -0.34 0.34 -0.5 0.45 -0.22 C 0.5 -0.05 0.35 0.08 0.2 0.08"),
            [ZodiacSign.Taurus] = Create("zodiac.taurus", "M -0.38 -0.42 C -0.38 -0.14 -0.2 -0.05 0 -0.05 C 0.2 -0.05 0.38 -0.14 0.38 -0.42 M 0 -0.05 C 0.28 -0.05 0.43 0.13 0.43 0.28 C 0.43 0.47 0.23 0.53 0 0.53 C -0.23 0.53 -0.43 0.47 -0.43 0.28 C -0.43 0.13 -0.28 -0.05 0 -0.05"),
            [ZodiacSign.Gemini] = Create("zodiac.gemini", "M -0.38 -0.42 C -0.15 -0.32 0.15 -0.32 0.38 -0.42 M -0.38 0.42 C -0.15 0.32 0.15 0.32 0.38 0.42 M -0.22 -0.34 L -0.22 0.34 M 0.22 -0.34 L 0.22 0.34"),
            [ZodiacSign.Cancer] = Create("zodiac.cancer", "M -0.44 -0.08 C -0.27 -0.34 0.18 -0.42 0.42 -0.19 M 0.44 0.08 C 0.27 0.34 -0.18 0.42 -0.42 0.19 M -0.24 -0.15 C -0.24 -0.27 -0.08 -0.27 -0.08 -0.15 C -0.08 -0.03 -0.24 -0.03 -0.24 -0.15 M 0.24 0.15 C 0.24 0.27 0.08 0.27 0.08 0.15 C 0.08 0.03 0.24 0.03 0.24 0.15"),
            [ZodiacSign.Leo] = Create("zodiac.leo", "M -0.28 0.24 C -0.42 0.24 -0.45 0.05 -0.34 -0.04 C -0.23 -0.13 -0.06 -0.07 -0.06 0.08 C -0.06 0.3 0.22 0.43 0.37 0.23 C 0.49 0.07 0.39 -0.12 0.25 -0.18 C 0.08 -0.25 0.08 -0.43 0.18 -0.5"),
            [ZodiacSign.Virgo] = Create("zodiac.virgo", "M -0.43 -0.35 L -0.43 0.34 M -0.43 -0.15 C -0.35 -0.38 -0.18 -0.38 -0.14 -0.15 L -0.14 0.3 M -0.14 -0.15 C -0.05 -0.38 0.12 -0.38 0.16 -0.15 L 0.16 0.27 C 0.16 0.44 0.4 0.44 0.4 0.22 C 0.4 0.03 0.2 0.02 0.07 0.13 M 0.17 0.27 L 0.43 0.48"),
            [ZodiacSign.Libra] = Create("zodiac.libra", "M -0.46 0.34 L 0.46 0.34 M -0.46 0.12 L -0.2 0.12 C -0.2 -0.12 -0.08 -0.28 0 -0.28 C 0.08 -0.28 0.2 -0.12 0.2 0.12 L 0.46 0.12"),
            [ZodiacSign.Scorpio] = Create("zodiac.scorpio", "M -0.43 -0.34 L -0.43 0.3 M -0.43 -0.13 C -0.35 -0.36 -0.18 -0.36 -0.14 -0.13 L -0.14 0.3 M -0.14 -0.13 C -0.05 -0.36 0.12 -0.36 0.16 -0.13 L 0.16 0.24 C 0.16 0.38 0.32 0.38 0.4 0.25 M 0.29 0.25 L 0.42 0.25 L 0.42 0.12"),
            [ZodiacSign.Sagittarius] = Create("zodiac.sagittarius", "M -0.38 0.38 L 0.4 -0.4 M 0.09 -0.4 L 0.4 -0.4 L 0.4 -0.09 M -0.32 -0.06 L 0.08 0.34"),
            [ZodiacSign.Capricorn] = Create("zodiac.capricorn", "M -0.44 -0.28 L -0.44 0.3 M -0.44 -0.1 C -0.32 -0.36 -0.12 -0.3 -0.1 -0.06 L -0.02 0.29 M -0.02 0.29 C 0.08 0.5 0.4 0.42 0.42 0.18 C 0.42 -0.02 0.18 -0.08 0.08 0.07 C -0.04 0.25 0.14 0.38 0.31 0.36"),
            [ZodiacSign.Aquarius] = Create("zodiac.aquarius", "M -0.48 -0.18 L -0.31 -0.31 L -0.14 -0.18 L 0.03 -0.31 L 0.2 -0.18 L 0.37 -0.31 L 0.48 -0.22 M -0.48 0.18 L -0.31 0.05 L -0.14 0.18 L 0.03 0.05 L 0.2 0.18 L 0.37 0.05 L 0.48 0.14"),
            [ZodiacSign.Pisces] = Create("zodiac.pisces", "M -0.32 -0.43 C -0.1 -0.2 -0.1 0.2 -0.32 0.43 M 0.32 -0.43 C 0.1 -0.2 0.1 0.2 0.32 0.43 M -0.42 0 L 0.42 0")
        };

    private static readonly IReadOnlyDictionary<CelestialBody, ChartVectorGlyph> BodyGlyphs =
        new Dictionary<CelestialBody, ChartVectorGlyph>
        {
            [CelestialBody.Sun] = Create("planet.sun", "M 0 -0.43 C 0.24 -0.43 0.43 -0.24 0.43 0 C 0.43 0.24 0.24 0.43 0 0.43 C -0.24 0.43 -0.43 0.24 -0.43 0 C -0.43 -0.24 -0.24 -0.43 0 -0.43 M 0 -0.08 C 0.05 -0.08 0.08 -0.05 0.08 0 C 0.08 0.05 0.05 0.08 0 0.08 C -0.05 0.08 -0.08 0.05 -0.08 0 C -0.08 -0.05 -0.05 -0.08 0 -0.08"),
            [CelestialBody.Moon] = Create("planet.moon", "M 0.28 -0.43 C -0.12 -0.38 -0.3 -0.12 -0.28 0.14 C -0.25 0.39 0.02 0.5 0.32 0.34 C 0.04 0.36 -0.12 0.18 -0.12 -0.03 C -0.12 -0.23 0.02 -0.38 0.28 -0.43"),
            [CelestialBody.Mercury] = Create("planet.mercury", "M -0.28 -0.43 C -0.25 -0.24 -0.12 -0.18 0 -0.18 C 0.12 -0.18 0.25 -0.24 0.28 -0.43 M 0 -0.18 C 0.22 -0.18 0.31 -0.02 0.31 0.12 C 0.31 0.29 0.17 0.37 0 0.37 C -0.17 0.37 -0.31 0.29 -0.31 0.12 C -0.31 -0.02 -0.22 -0.18 0 -0.18 M 0 0.37 L 0 0.55 M -0.16 0.47 L 0.16 0.47"),
            [CelestialBody.Venus] = Create("planet.venus", "M 0 -0.43 C 0.23 -0.43 0.38 -0.27 0.38 -0.08 C 0.38 0.12 0.23 0.27 0 0.27 C -0.23 0.27 -0.38 0.12 -0.38 -0.08 C -0.38 -0.27 -0.23 -0.43 0 -0.43 M 0 0.27 L 0 0.54 M -0.16 0.43 L 0.16 0.43"),
            [CelestialBody.Mars] = Create("planet.mars", "M -0.15 0.16 C -0.37 0.16 -0.45 0 -0.45 -0.15 C -0.45 -0.34 -0.28 -0.46 -0.08 -0.46 C 0.12 -0.46 0.28 -0.32 0.28 -0.15 C 0.28 0.03 0.12 0.16 -0.15 0.16 M 0.19 -0.34 L 0.48 -0.54 M 0.26 -0.54 L 0.48 -0.54 L 0.48 -0.33"),
            [CelestialBody.Jupiter] = Create("planet.jupiter", "M -0.34 -0.34 C -0.11 -0.5 0.17 -0.34 0.12 -0.08 C 0.07 0.17 -0.22 0.24 -0.4 0.18 M 0.12 -0.08 L 0.12 0.48 M -0.16 0.23 L 0.35 0.23"),
            [CelestialBody.Saturn] = Create("planet.saturn", "M -0.14 -0.52 L -0.14 0.18 M -0.37 -0.32 L 0.1 -0.32 M -0.14 -0.05 C 0.03 -0.2 0.28 -0.12 0.27 0.08 C 0.25 0.26 0.04 0.29 0.02 0.43 C 0 0.55 0.18 0.58 0.31 0.45"),
            [CelestialBody.Uranus] = Create("planet.uranus", "M 0 -0.5 L 0 0.5 M -0.38 -0.34 L -0.38 0.08 C -0.38 0.26 -0.2 0.32 0 0.32 C 0.2 0.32 0.38 0.26 0.38 0.08 L 0.38 -0.34 M -0.48 -0.34 L -0.28 -0.34 M 0.28 -0.34 L 0.48 -0.34 M 0 0.36 C 0.08 0.36 0.13 0.41 0.13 0.49 C 0.13 0.57 0.08 0.62 0 0.62 C -0.08 0.62 -0.13 0.57 -0.13 0.49 C -0.13 0.41 -0.08 0.36 0 0.36"),
            [CelestialBody.Neptune] = Create("planet.neptune", "M 0 -0.5 L 0 0.48 M -0.34 -0.38 C -0.34 -0.05 -0.18 0.12 0 0.12 C 0.18 0.12 0.34 -0.05 0.34 -0.38 M -0.45 -0.27 L -0.34 -0.38 L -0.23 -0.27 M -0.11 -0.39 L 0 -0.5 L 0.11 -0.39 M 0.23 -0.27 L 0.34 -0.38 L 0.45 -0.27 M -0.17 0.34 L 0.17 0.34"),
            [CelestialBody.Pluto] = Create("planet.pluto", "M 0 -0.5 C 0.16 -0.5 0.27 -0.39 0.27 -0.24 C 0.27 -0.1 0.16 0 0 0 C -0.16 0 -0.27 -0.1 -0.27 -0.24 C -0.27 -0.39 -0.16 -0.5 0 -0.5 M -0.4 -0.02 C -0.33 0.2 -0.18 0.29 0 0.29 C 0.18 0.29 0.33 0.2 0.4 -0.02 M 0 0.29 L 0 0.55 M -0.16 0.44 L 0.16 0.44")
        };

    /// <summary>
    /// Gets the universal glyph for a zodiac sign.
    /// </summary>
    public static ChartVectorGlyph GetSignGlyph(ZodiacSign sign) =>
        SignGlyphs.TryGetValue(sign, out var glyph)
            ? glyph
            : throw new ArgumentOutOfRangeException(nameof(sign), sign, "Unsupported zodiac sign.");

    /// <summary>
    /// Gets the universal glyph for a planetary body.
    /// </summary>
    public static ChartVectorGlyph GetBodyGlyph(CelestialBody body) =>
        BodyGlyphs.TryGetValue(body, out var glyph)
            ? glyph
            : throw new ArgumentOutOfRangeException(nameof(body), body, "Unsupported celestial body.");

    private static ChartVectorGlyph Create(string id, string pathData) =>
        new(id, pathData, UnitBounds);
}
