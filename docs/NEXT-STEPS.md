# Next Steps

The app now has a thin localized shell, JSON-backed UI localization, first dark/light theme switching, an astrology workspace foundation, and the first structured birth-data input state with basic validation.

Immediate next steps:

1. Connect validated birth-data input to the existing `IBirthMomentResolver` and a deterministic or fake ephemeris flow so the workspace can build charts from user-entered data without Swiss Ephemeris yet.

2. Keep deterministic sample chart generation behind the scenes only as a fallback or development path while input-driven chart building is introduced.

3. Keep settings persistence deferred until the workspace structure and preference flow are stable.

4. Verify Swiss Ephemeris wrapper options before committing to a package and record the decision in `DECISIONS-LOG.md`.

5. Keep real profile entry, houses, advanced collision avoidance, interpretation localization, persistence, and Swiss Ephemeris integration out of scope until the input-driven chart flow exists.
