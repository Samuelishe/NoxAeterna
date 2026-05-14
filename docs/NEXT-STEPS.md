# Next Steps

The temporary preview path now proves the Domain -> Geometry -> Rendering -> Avalonia pipeline. The next work should start replacing the debug-only path with a thin presentation-led shell structure while keeping chart rendering isolated.

Immediate next steps:

1. Introduce a thin Presentation-level view model or presentation contract for chart preview hosting, without moving rendering logic out of `NoxAeterna.Rendering`.

2. Start shaping a proper localized shell structure around the preview, still keeping it clearly minimal and non-productized.

3. Avoid settings UI, persistence, real data-entry flows, and Swiss Ephemeris integration while the first presentation shell is being established.

4. Verify Swiss Ephemeris wrapper options before committing to a package and record the decision in `DECISIONS-LOG.md`.

5. Keep houses, advanced collision avoidance, and theme integration out of scope until the shell/presentation boundary is cleaner.
