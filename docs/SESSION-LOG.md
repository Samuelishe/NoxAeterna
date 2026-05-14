# Session Log

This file tracks what was done in each Codex session. Future sessions should append entries with date, summary, changed files, decisions, and next actions.

## 2026-05-14: Documentation Initialization

Summary:

- Created the `/docs` documentation system for autonomous AI-assisted development.
- Added agent navigation, product vision, architecture, roadmap, domain, astronomy, geometry, rendering, interpretation, symbolics, Tarot, persistence, UI, assets, coding, decisions, risks, debt, and next-step documents.
- Documented that the project is still in Stage 0 and no application architecture has been implemented yet.

Changed files:

- `docs/AGENTS.md`
- `docs/INDEX.md`
- `docs/PROJECT-VISION.md`
- `docs/ARCHITECTURE.md`
- `docs/ROADMAP.md`
- `docs/DOMAIN-MODEL.md`
- `docs/ASTRONOMY-ENGINE.md`
- `docs/GEOMETRY-ENGINE.md`
- `docs/RENDERING-ENGINE.md`
- `docs/INTERPRETATION-ENGINE.md`
- `docs/SYMBOLICS.md`
- `docs/TAROT-ENGINE.md`
- `docs/PERSISTENCE.md`
- `docs/UI-VISION.md`
- `docs/ASSETS-PIPELINE.md`
- `docs/CODING-GUIDELINES.md`
- `docs/DECISIONS-LOG.md`
- `docs/SESSION-LOG.md`
- `docs/TECHNICAL-DEBT.md`
- `docs/KNOWN-PROBLEMS.md`
- `docs/NEXT-STEPS.md`

Next actions:

- Scaffold the .NET 10 solution structure.
- Add project references that enforce the documented boundaries.
- Add a basic test project and build/test workflow.

## 2026-05-14: Documentation Naming and Git Ignore Cleanup

Summary:

- Renamed all markdown files in `docs` to uppercase names for consistency with `AGENTS.md`.
- Updated internal documentation references to match the new uppercase filenames.
- Expanded `.gitignore` to exclude JetBrains settings, common .NET build outputs, test results, and OS/editor noise.

Changed files:

- `.gitignore`
- `docs/AGENTS.md`
- `docs/INDEX.md`
- `docs/ASTRONOMY-ENGINE.md`
- `docs/CODING-GUIDELINES.md`
- `docs/PERSISTENCE.md`
- `docs/SESSION-LOG.md`
- `docs/TECHNICAL-DEBT.md`

Next actions:

- Scaffold the .NET 10 solution structure.
- Keep new project artifacts aligned with the updated `.gitignore`.

## 2026-05-14: Final Architecture Clarification Pass

Summary:

- Locked structured-first interpretation, typed Symbolics catalog direction, conservative MVP timezone strategy, and render-independent geometry-to-rendering contracts.
- Added repository-level guidance for development philosophy, glossary, architectural boundaries, attribution tracking, and GitHub-facing repository documentation.
- Declared the repository ready for solution scaffold and implementation startup without another large planning pass.

Changed files:

- `README.md`
- `docs/AGENTS.md`
- `docs/ARCHITECTURE.md`
- `docs/ASSETS-PIPELINE.md`
- `docs/CODING-GUIDELINES.md`
- `docs/DECISIONS-LOG.md`
- `docs/GEOMETRY-ENGINE.md`
- `docs/INTERPRETATION-ENGINE.md`
- `docs/KNOWN-PROBLEMS.md`
- `docs/NEXT-STEPS.md`
- `docs/SESSION-LOG.md`
- `docs/SYMBOLICS.md`
- `docs/DEVELOPMENT-PHILOSOPHY.md`
- `docs/GLOSSARY.md`
- `docs/ARCHITECTURAL-BOUNDARIES.md`
- `docs/THIRD-PARTY.md`

Next actions:

- Create the .NET 10 solution scaffold and dependency graph.
- Add the first domain primitives and tests after scaffold creation.

## 2026-05-14: Initial Solution Scaffold

Summary:

- Created the initial .NET 10 solution scaffold with ten projects: one Avalonia app shell, eight production class libraries, and one xUnit test project.
- Added `Directory.Build.props` with repository-level nullable, implicit usings, and language-version defaults.
- Locked the initial `ProjectReference` graph to reflect the documented architectural boundaries.
- Verified the scaffold with `dotnet build NoxAeterna.sln` and `dotnet test NoxAeterna.sln`.

Changed files:

- `Directory.Build.props`
- `NoxAeterna.sln`
- `NoxAeterna.App/NoxAeterna.App.csproj`
- `NoxAeterna.App/Program.cs`
- `NoxAeterna.App/App.axaml`
- `NoxAeterna.App/App.axaml.cs`
- `NoxAeterna.App/MainWindow.axaml`
- `NoxAeterna.App/MainWindow.axaml.cs`
- `NoxAeterna.Presentation/NoxAeterna.Presentation.csproj`
- `NoxAeterna.Rendering/NoxAeterna.Rendering.csproj`
- `NoxAeterna.Geometry/NoxAeterna.Geometry.csproj`
- `NoxAeterna.Astronomy/NoxAeterna.Astronomy.csproj`
- `NoxAeterna.Symbolics/NoxAeterna.Symbolics.csproj`
- `NoxAeterna.Interpretation/NoxAeterna.Interpretation.csproj`
- `NoxAeterna.Domain/NoxAeterna.Domain.csproj`
- `NoxAeterna.Infrastructure/NoxAeterna.Infrastructure.csproj`
- `NoxAeterna.Tests/NoxAeterna.Tests.csproj`
- `NoxAeterna.Tests/UnitTest1.cs`
- `README.md`
- `docs/AGENTS.md`
- `docs/DECISIONS-LOG.md`
- `docs/NEXT-STEPS.md`
- `docs/SESSION-LOG.md`
- `docs/THIRD-PARTY.md`

Next actions:

- Create the first domain primitives and tests, starting with `ZodiacLongitude` and aspect angle/orb math.
- Keep the rest of the solution behavior-free until those types and tests are in place.

## 2026-05-14: First Domain Primitives and Aspect Math

Summary:

- Added the first domain primitives for zodiac longitude normalization, zodiac sign derivation, major aspect types, and compact aspect matching math.
- Kept the implementation inside `NoxAeterna.Domain` with no new external dependencies and no coupling to UI, rendering, persistence, or infrastructure.
- Added focused xUnit coverage for normalization, sign boundaries, angular delta, exact aspects, in-orb matches, and out-of-orb rejections.
- Verified the result with `dotnet build NoxAeterna.sln` and `dotnet test NoxAeterna.sln`.

Changed files:

- `NoxAeterna.Domain/Astrology/ZodiacLongitude.cs`
- `NoxAeterna.Domain/Astrology/ZodiacSign.cs`
- `NoxAeterna.Domain/Astrology/AspectType.cs`
- `NoxAeterna.Domain/Astrology/AspectMath.cs`
- `NoxAeterna.Tests/Astrology/ZodiacLongitudeTests.cs`
- `NoxAeterna.Tests/Astrology/AspectMathTests.cs`
- `docs/NEXT-STEPS.md`
- `docs/SESSION-LOG.md`

Next actions:

- Define `BirthData` and `BirthMoment` direction.
- Introduce the first NodaTime-based time model contracts without binding to Swiss Ephemeris yet.
