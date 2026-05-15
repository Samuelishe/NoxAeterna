using NoxAeterna.Domain.Astrology;
using NoxAeterna.Presentation.Localization;

namespace NoxAeterna.Presentation.Astrology;

/// <summary>
/// Maps astrology symbols to localization keys and universal glyphs for presentation concerns.
/// </summary>
public static class AstrologySymbolCatalog
{
    /// <summary>
    /// Returns the localization key for a celestial body name.
    /// </summary>
    public static LocalizationKey GetBodyLabelKey(CelestialBody body) =>
        body switch
        {
            CelestialBody.Sun => new LocalizationKey("ui.planet.sun"),
            CelestialBody.Moon => new LocalizationKey("ui.planet.moon"),
            CelestialBody.Mercury => new LocalizationKey("ui.planet.mercury"),
            CelestialBody.Venus => new LocalizationKey("ui.planet.venus"),
            CelestialBody.Mars => new LocalizationKey("ui.planet.mars"),
            CelestialBody.Jupiter => new LocalizationKey("ui.planet.jupiter"),
            CelestialBody.Saturn => new LocalizationKey("ui.planet.saturn"),
            CelestialBody.Uranus => new LocalizationKey("ui.planet.uranus"),
            CelestialBody.Neptune => new LocalizationKey("ui.planet.neptune"),
            CelestialBody.Pluto => new LocalizationKey("ui.planet.pluto"),
            _ => throw new ArgumentOutOfRangeException(nameof(body), body, "Unsupported celestial body.")
        };

    /// <summary>
    /// Returns the localization key for a zodiac sign name.
    /// </summary>
    public static LocalizationKey GetSignLabelKey(ZodiacSign sign) =>
        sign switch
        {
            ZodiacSign.Aries => new LocalizationKey("ui.zodiac.aries"),
            ZodiacSign.Taurus => new LocalizationKey("ui.zodiac.taurus"),
            ZodiacSign.Gemini => new LocalizationKey("ui.zodiac.gemini"),
            ZodiacSign.Cancer => new LocalizationKey("ui.zodiac.cancer"),
            ZodiacSign.Leo => new LocalizationKey("ui.zodiac.leo"),
            ZodiacSign.Virgo => new LocalizationKey("ui.zodiac.virgo"),
            ZodiacSign.Libra => new LocalizationKey("ui.zodiac.libra"),
            ZodiacSign.Scorpio => new LocalizationKey("ui.zodiac.scorpio"),
            ZodiacSign.Sagittarius => new LocalizationKey("ui.zodiac.sagittarius"),
            ZodiacSign.Capricorn => new LocalizationKey("ui.zodiac.capricorn"),
            ZodiacSign.Aquarius => new LocalizationKey("ui.zodiac.aquarius"),
            ZodiacSign.Pisces => new LocalizationKey("ui.zodiac.pisces"),
            _ => throw new ArgumentOutOfRangeException(nameof(sign), sign, "Unsupported zodiac sign.")
        };

    /// <summary>
    /// Returns the universal glyph for a celestial body.
    /// </summary>
    public static string GetBodyGlyph(CelestialBody body) =>
        body switch
        {
            CelestialBody.Sun => "☉",
            CelestialBody.Moon => "☽",
            CelestialBody.Mercury => "☿",
            CelestialBody.Venus => "♀",
            CelestialBody.Mars => "♂",
            CelestialBody.Jupiter => "♃",
            CelestialBody.Saturn => "♄",
            CelestialBody.Uranus => "♅",
            CelestialBody.Neptune => "♆",
            CelestialBody.Pluto => "♇",
            _ => throw new ArgumentOutOfRangeException(nameof(body), body, "Unsupported celestial body.")
        };

    /// <summary>
    /// Returns the universal glyph for a zodiac sign.
    /// </summary>
    public static string GetSignGlyph(ZodiacSign sign) =>
        sign switch
        {
            ZodiacSign.Aries => "♈",
            ZodiacSign.Taurus => "♉",
            ZodiacSign.Gemini => "♊",
            ZodiacSign.Cancer => "♋",
            ZodiacSign.Leo => "♌",
            ZodiacSign.Virgo => "♍",
            ZodiacSign.Libra => "♎",
            ZodiacSign.Scorpio => "♏",
            ZodiacSign.Sagittarius => "♐",
            ZodiacSign.Capricorn => "♑",
            ZodiacSign.Aquarius => "♒",
            ZodiacSign.Pisces => "♓",
            _ => throw new ArgumentOutOfRangeException(nameof(sign), sign, "Unsupported zodiac sign.")
        };
}
