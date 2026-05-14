# Next Steps

The first rendering-layer contracts are now in place. The next work should add a minimal app-level preview path so the geometry-to-rendering pipeline can be visually verified without pretending a real product screen exists yet.

Immediate next steps:

1. Create a minimal temporary/debug preview path in `NoxAeterna.App` that builds a deterministic sample `NatalChart`, passes it through `CircularChartLayoutBuilder`, and renders it through `CircularChartRenderer`.

2. Keep that preview clearly marked as temporary/debug infrastructure rather than product UI.

3. Avoid settings UI, navigation, persistence, and real data-entry flows while validating the first chart rendering pipeline.

4. Verify Swiss Ephemeris wrapper options before committing to a package and record the decision in `DECISIONS-LOG.md`.

5. Keep houses, advanced collision avoidance, and theme integration out of scope until the preview path proves the current rendering contracts.
