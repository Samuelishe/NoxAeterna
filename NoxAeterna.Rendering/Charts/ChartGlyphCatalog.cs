using NoxAeterna.Domain.Astrology;

namespace NoxAeterna.Rendering.Charts;

/// <summary>
/// Supplies universal astrology glyphs for rendering concerns without introducing localization dependencies.
/// </summary>
public static class ChartGlyphCatalog
{
    private const string TextVariationSelector = "\uFE0E";

    /// <summary>
    /// Gets the universal glyph for a zodiac sign.
    /// </summary>
    public static string GetSignGlyph(ZodiacSign sign) =>
        sign switch
        {
            ZodiacSign.Aries => "♈" + TextVariationSelector,
            ZodiacSign.Taurus => "♉" + TextVariationSelector,
            ZodiacSign.Gemini => "♊" + TextVariationSelector,
            ZodiacSign.Cancer => "♋" + TextVariationSelector,
            ZodiacSign.Leo => "♌" + TextVariationSelector,
            ZodiacSign.Virgo => "♍" + TextVariationSelector,
            ZodiacSign.Libra => "♎" + TextVariationSelector,
            ZodiacSign.Scorpio => "♏" + TextVariationSelector,
            ZodiacSign.Sagittarius => "♐" + TextVariationSelector,
            ZodiacSign.Capricorn => "♑" + TextVariationSelector,
            ZodiacSign.Aquarius => "♒" + TextVariationSelector,
            ZodiacSign.Pisces => "♓" + TextVariationSelector,
            _ => throw new ArgumentOutOfRangeException(nameof(sign), sign, "Unsupported zodiac sign.")
        };

    /// <summary>
    /// Gets the universal glyph for a planetary body.
    /// </summary>
    public static string GetBodyGlyph(CelestialBody body) =>
        body switch
        {
            CelestialBody.Sun => "☉" + TextVariationSelector,
            CelestialBody.Moon => "☽" + TextVariationSelector,
            CelestialBody.Mercury => "☿" + TextVariationSelector,
            CelestialBody.Venus => "♀" + TextVariationSelector,
            CelestialBody.Mars => "♂" + TextVariationSelector,
            CelestialBody.Jupiter => "♃" + TextVariationSelector,
            CelestialBody.Saturn => "♄" + TextVariationSelector,
            CelestialBody.Uranus => "♅" + TextVariationSelector,
            CelestialBody.Neptune => "♆" + TextVariationSelector,
            CelestialBody.Pluto => "♇" + TextVariationSelector,
            _ => throw new ArgumentOutOfRangeException(nameof(body), body, "Unsupported celestial body.")
        };
}
