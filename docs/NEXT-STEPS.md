# Next Steps

The first domain primitives are now in place. The next work should move into birth-data and time-model contracts rather than broader engine work.

Immediate next steps:

1. Define `BirthData` and `BirthMoment` direction in the domain with a minimal, reproducibility-focused shape.

2. Introduce the first NodaTime-based time model contracts for local time, timezone identity, and UTC instant handling.

3. Add focused tests for ambiguous and invalid local-time scenarios at the contract level, without binding to Swiss Ephemeris yet.

4. Verify Swiss Ephemeris wrapper options before committing to a package and record the decision in `DECISIONS-LOG.md`.

5. Keep UI, rendering behavior, persistence, and ephemeris calculations out of scope until the first time-model primitives are stable and tested.
