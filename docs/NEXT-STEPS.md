# Next Steps

The first in-memory settings foundation now exists. The next work should replace the temporary in-memory UI localization bootstrap with JSON-backed localization loading for UI resources.

Immediate next steps:

1. Add JSON-backed localization loading for UI resources and replace the temporary in-memory shell/settings localization catalog in `App`.

2. Keep settings persistence deferred even after localization loading exists.

3. Preserve the current debug preview route as an easy visual verification path while localization bootstrapping is removed from `App`.

4. Verify Swiss Ephemeris wrapper options before committing to a package and record the decision in `DECISIONS-LOG.md`.

5. Keep real profile entry, houses, advanced collision avoidance, and theme resource switching out of scope until localization loading is in place.
