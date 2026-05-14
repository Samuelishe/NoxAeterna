# Next Steps

This repository is now ready for scaffold and implementation startup. No further large-scale planning pass is required before coding begins.

Immediate next steps:

1. Create the .NET 10 solution scaffold:
   - `NoxAeterna.App`
   - `NoxAeterna.Presentation`
   - `NoxAeterna.Rendering`
   - `NoxAeterna.Geometry`
   - `NoxAeterna.Astronomy`
   - `NoxAeterna.Symbolics`
   - `NoxAeterna.Interpretation`
   - `NoxAeterna.Domain`
   - `NoxAeterna.Infrastructure`
   - `NoxAeterna.Tests`

2. Lock the initial project reference graph in code so architectural boundaries are enforced by `csproj` dependencies.

3. Add baseline dependencies only where justified:
   - Avalonia for app, presentation, and rendering.
   - CommunityToolkit.Mvvm for presentation.
   - NodaTime for domain and astronomy time modeling.
   - xUnit for tests.
   - Serilog for app and infrastructure logging.

4. Create the first domain primitives and tests:
   - `ZodiacLongitude`
   - Zodiac sign derivation
   - Aspect angular delta and orb matching
   - `BirthMoment` metadata shape for reproducible time handling

5. Verify Swiss Ephemeris wrapper options before committing to a package and record the decision in `DECISIONS-LOG.md`.

Do not implement UI flows, rendering behavior, database schema, or ephemeris integration before the scaffold and dependency graph are in place.
