# Next Steps

The first birth-time domain contracts are now in place. The next work should move into astronomy calculation contracts that consume the existing longitude and birth-moment models.

Immediate next steps:

1. Define astronomy calculation contracts such as `IEphemerisCalculator`, `CelestialBody`, `PlanetPosition`, and `ChartCalculationRequest`.

2. Keep those contracts free of Swiss Ephemeris-specific types and native interop details.

3. Reuse the existing `ZodiacLongitude` and `BirthMoment` models as the calculation inputs and outputs where appropriate.

4. Verify Swiss Ephemeris wrapper options before committing to a package and record the decision in `DECISIONS-LOG.md`.

5. Keep UI, rendering behavior, persistence, and actual ephemeris integration out of scope until the calculation contracts and their core tests are stable.
