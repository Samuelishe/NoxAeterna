# Next Steps

The app now has a thin localized shell, JSON-backed UI localization, first dark/light theme switching, an astrology workspace foundation, a refined birth-data input flow with typed date/time controls, TZDB timezone selection, offline manual mode, an input-driven demo chart rebuild pipeline, and a lighter workspace layout pass that improves chart sizing and right-panel readability at the default window size.

Immediate next steps:

1. Add the first in-memory profile or session draft model that holds current unsaved birth-data input, resolved birth moment metadata, and chart state without writing to disk yet.

2. Keep deterministic sample chart generation behind the scenes only as a fallback or development path while the input-driven chart flow stabilizes.

3. Keep settings persistence and broader AppData storage implementation deferred until the in-memory draft/state model is stable.

4. Verify Swiss Ephemeris wrapper options before committing to a package and record the decision in `DECISIONS-LOG.md`.

5. Keep online geocoding, a bundled world-city database, real profile entry, houses, advanced collision avoidance, interpretation localization, persistence, and Swiss Ephemeris integration out of scope until the input-driven chart flow exists.
