# Next Steps

The app now uses JSON-backed UI localization catalogs. The next work should add the first real dark/light theme resource switching using the existing in-memory theme preference.

Immediate next steps:

1. Add the first real dark/light theme switching path using `ThemeId` and the current in-memory settings state.

2. Keep settings persistence deferred even after theme switching exists.

3. Preserve the current debug preview route as an easy visual verification path while theme switching is introduced.

4. Verify Swiss Ephemeris wrapper options before committing to a package and record the decision in `DECISIONS-LOG.md`.

5. Keep real profile entry, houses, advanced collision avoidance, and interpretation localization out of scope until theme switching is in place.
