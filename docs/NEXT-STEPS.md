# Next Steps

Immediate suggested steps after documentation initialization:

1. Scaffold the .NET 10 solution projects:
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

2. Define the initial project reference graph to enforce architecture boundaries.

3. Add baseline dependencies only where justified:
   - Avalonia for app/presentation/rendering.
   - CommunityToolkit.Mvvm for presentation.
   - NodaTime for domain/astronomy time modeling.
   - xUnit for tests.
   - Serilog for app/infrastructure logging.

4. Create first domain value objects and tests:
   - `ZodiacLongitude`
   - Zodiac sign derivation
   - Aspect angular delta and orb matching

5. Verify Swiss Ephemeris wrapper options before committing to a package.

Do not implement UI flows, rendering, database schema, or ephemeris integration before the solution scaffold and core boundaries exist.
