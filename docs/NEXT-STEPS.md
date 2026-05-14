# Next Steps

The first geometry contracts and presentation-side localization/theme/preferences foundation are now in place. The next work should move into render-layer contracts that can consume geometry output through Avalonia drawing abstractions without creating real application screens yet.

Immediate next steps:

1. Introduce the first render-layer contracts in `NoxAeterna.Rendering` and connect them to `CircularChartLayout` output using Avalonia `DrawingContext`.

2. Keep rendering strictly dependent on prepared geometry models rather than raw `NatalChart` state.

3. Avoid full application UI screens while establishing the first render-scene and render-layer contracts.

4. Verify Swiss Ephemeris wrapper options before committing to a package and record the decision in `DECISIONS-LOG.md`.

5. Keep persistence, actual ephemeris integration, and advanced collision avoidance out of scope until rendering contracts are stable.
