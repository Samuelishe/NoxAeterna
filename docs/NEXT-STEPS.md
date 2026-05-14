# Next Steps

The first minimal natal-chart snapshot now exists. The next work should move into render-independent chart geometry contracts that can consume chart data without pulling in Avalonia.

Immediate next steps:

1. Create the first render-independent chart geometry contracts for circular chart layout, planetary glyph slots, and stable angular placement.

2. Keep geometry models Avalonia-independent and focused on placement math rather than drawing concerns.

3. Use the existing `NatalChart`, `PlanetPosition`, and aspect snapshot models as geometry inputs without introducing rendering types.

4. Verify Swiss Ephemeris wrapper options before committing to a package and record the decision in `DECISIONS-LOG.md`.

5. Keep UI, rendering behavior, persistence, and actual ephemeris integration out of scope until geometry contracts are stable.
