# Next Steps

The first astronomy calculation contracts are now in place. The next work should move into a minimal natal-chart contract that combines birth moment, calculated positions, and existing aspect math.

Immediate next steps:

1. Create a minimal `NatalChart` contract or model that combines `BirthMoment`, calculated `PlanetPosition` collection, and aspect output using the existing `AspectMath`.

2. Keep houses out of scope for that first chart model.

3. Keep Swiss Ephemeris-specific types and native interop details out of the chart model and calculation contracts.

4. Verify Swiss Ephemeris wrapper options before committing to a package and record the decision in `DECISIONS-LOG.md`.

5. Keep UI, rendering behavior, persistence, and actual ephemeris integration out of scope until the chart model and calculation contracts are stable.
