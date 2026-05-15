# Next Steps

The app now has a thin localized shell, JSON-backed UI localization, first dark/light theme switching, an astrology workspace foundation, a refined birth-data input flow with typed date/time controls, TZDB timezone selection, offline manual mode, a visible chart rebuild pipeline that uses SwissEphNet behind `IEphemerisCalculator` with current Moshier fallback behavior, and a first readable chart with zodiac/planet glyphs plus a positions summary.

Immediate next steps:

1. Investigate and implement the explicit external `.se1` ephemeris file setup path so the app can move from current Moshier fallback to the higher-precision Swiss-data mode without silent bundling.

2. Keep the current readable chart foundation stable while verifying cross-platform glyph rendering and deciding whether an open-source astrology-capable font is needed later.

3. Resolve formal license alignment for the repository before any release packaging or redistribution that depends on Swiss Ephemeris code or data files.

4. Keep all ephemeris integration hidden behind `IEphemerisCalculator` and current astronomy contracts so UI, geometry, and rendering layers remain unchanged.

5. Keep houses, online geocoding, persistence, interpretation, Tarot workflows, and profile/session draft persistence out of scope while ephemeris setup and chart readability are stabilized.
