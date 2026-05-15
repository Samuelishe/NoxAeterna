# Next Steps

The app now has a thin localized shell, JSON-backed UI localization, first dark/light theme switching, an astrology workspace foundation, a refined birth-data input flow with typed date/time controls, TZDB timezone selection, offline manual mode, an input-driven demo chart rebuild pipeline, and a corrected workspace layout that keeps the birth-data panel readable at the default window size.

Immediate next steps:

1. Start a Swiss Ephemeris integration spike behind `IEphemerisCalculator` and evaluate the most suitable .NET wrapper, packaging path, and licensing constraints before committing to a concrete dependency.

2. Replace the fake demo ephemeris flow with real planetary position calculation for Sun, Moon, Mercury, Venus, Mars, Jupiter, Saturn, Uranus, Neptune, and Pluto, while keeping houses out of scope.

3. Keep all real ephemeris integration hidden behind `IEphemerisCalculator` and current astronomy contracts so UI, geometry, and rendering layers remain unchanged.

4. Keep persistence, AppData storage implementation, interpretation, and profile/session draft modeling deferred until real chart calculation exists and is worth saving.

5. Keep houses, online geocoding, a bundled world-city database, persistence, interpretation, Tarot workflows, and advanced chart-polish work out of scope during the initial Swiss Ephemeris spike.
