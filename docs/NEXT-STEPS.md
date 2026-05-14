# Next Steps

The initial solution scaffold is now in place. The next work should move into domain primitives and focused tests rather than more repository setup.

Immediate next steps:

1. Create the first domain primitives and tests:
   - `ZodiacLongitude`
   - Zodiac sign derivation
   - Aspect angular delta and orb matching
   - `BirthMoment` metadata shape for reproducible time handling

2. Add the first domain-facing enums and identifiers only where they directly support the above primitives.

3. Verify Swiss Ephemeris wrapper options before committing to a package and record the decision in `DECISIONS-LOG.md`.

4. Introduce NodaTime only when `BirthMoment` and time conversion contracts are ready to be implemented.

5. Keep UI, rendering behavior, persistence, and ephemeris calculations out of scope until the first domain math/types are tested and stable.
