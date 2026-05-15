# Next Steps

The app now has a thin localized shell, JSON-backed UI localization, first dark/light theme switching, an astrology workspace foundation, a refined birth-data input flow with typed date/time controls, TZDB timezone selection, offline manual mode, and a visible chart rebuild pipeline that now uses SwissEphNet behind `IEphemerisCalculator` with current Moshier fallback behavior.

Immediate next steps:

1. Add planet and zodiac glyph labels plus a positions table so the chart becomes readable now that real planetary longitudes are available.

2. Add a clear optional setup path for external Swiss Ephemeris `.se1` files so the app can move from the current Moshier fallback to the higher-precision Swiss-data mode without silent bundling.

3. Resolve formal license alignment for the repository before any release packaging or redistribution that depends on Swiss Ephemeris code or data files.

4. Keep all ephemeris integration hidden behind `IEphemerisCalculator` and current astronomy contracts so UI, geometry, and rendering layers remain unchanged.

5. Keep houses, online geocoding, persistence, interpretation, Tarot workflows, and profile/session draft persistence out of scope while chart readability and ephemeris setup are stabilized.
