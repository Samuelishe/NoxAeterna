# Next Steps

The app now has a thin localized shell, JSON-backed UI localization, first dark/light theme switching, and the first astrology workspace foundation backed by development-only sample chart data.

Immediate next steps:

1. Introduce the first structured birth-data input foundation inside the astrology workspace: date, time, location, timezone, and validation flow.

2. Keep deterministic sample chart generation behind the scenes until the first input flow can drive the workspace honestly.

3. Keep settings persistence deferred until the workspace structure and preference flow are stable.

4. Verify Swiss Ephemeris wrapper options before committing to a package and record the decision in `DECISIONS-LOG.md`.

5. Keep real profile entry, houses, advanced collision avoidance, interpretation localization, and Swiss Ephemeris integration out of scope until the input foundation exists.
