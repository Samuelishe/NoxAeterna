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

## 2026-05-14: First NodaTime-Based Birth-Time Contracts

Summary:

- Added the first explicit birth-time domain model using NodaTime: `BirthTimeAccuracy`, `LocalBirthDateTime`, `BirthLocation`, `TimezoneId`, `BirthData`, `BirthMoment`, `TimeResolutionStatus`, and `IBirthMomentResolver`.
- Added a small TZDB-based resolver implementation in `NoxAeterna.Astronomy` with deterministic MVP behavior for normal, ambiguous, and invalid local times.
- Added focused tests for location validation, timezone ID validation, known and unknown birth-time representation, resolver behavior, and public API protection against `System.DateTime` leakage.
- Introduced NodaTime as a real dependency in `NoxAeterna.Domain` and `NoxAeterna.Astronomy`, and verified the result with `dotnet build NoxAeterna.sln` and `dotnet test NoxAeterna.sln`.

Changed files:

- `NoxAeterna.Domain/NoxAeterna.Domain.csproj`
- `NoxAeterna.Domain/Birth/BirthTimeAccuracy.cs`
- `NoxAeterna.Domain/Birth/TimeResolutionStatus.cs`
- `NoxAeterna.Domain/Birth/TimezoneId.cs`
- `NoxAeterna.Domain/Birth/BirthLocation.cs`
- `NoxAeterna.Domain/Birth/LocalBirthDateTime.cs`
- `NoxAeterna.Domain/Birth/BirthData.cs`
- `NoxAeterna.Domain/Birth/BirthMoment.cs`
- `NoxAeterna.Domain/Birth/IBirthMomentResolver.cs`
- `NoxAeterna.Astronomy/NoxAeterna.Astronomy.csproj`
- `NoxAeterna.Astronomy/Time/TzdbBirthMomentResolver.cs`
- `NoxAeterna.Tests/Birth/BirthLocationTests.cs`
- `NoxAeterna.Tests/Birth/TimezoneIdTests.cs`
- `NoxAeterna.Tests/Birth/BirthDataTests.cs`
- `NoxAeterna.Tests/Birth/BirthMomentTests.cs`
- `NoxAeterna.Tests/Birth/TzdbBirthMomentResolverTests.cs`
- `NoxAeterna.Tests/Birth/PublicApiTimeTypeTests.cs`
- `README.md`
- `docs/THIRD-PARTY.md`
- `docs/DOMAIN-MODEL.md`
- `docs/ASTRONOMY-ENGINE.md`
- `docs/DECISIONS-LOG.md`
- `docs/NEXT-STEPS.md`
- `docs/SESSION-LOG.md`

Next actions:

- Define astronomy calculation contracts such as `IEphemerisCalculator`, `CelestialBody`, `PlanetPosition`, and `ChartCalculationRequest`.
- Keep Swiss Ephemeris-specific integration out of scope until those contracts are stable.

## 2026-05-14: First Astronomy Calculation Contracts

Summary:

- Added the first astronomy calculation-facing contracts without binding to Swiss Ephemeris: `ChartCalculationRequest`, `ChartCalculationResult`, and `IEphemerisCalculator`.
- Added the first shared position model in the domain: `CelestialBody` and `PlanetPosition`, with sign and degree-within-sign derived from `ZodiacLongitude`.
- Added focused tests for derived sign/degree behavior, deterministic calculator contract usage through a test fake, and project-boundary checks against Avalonia and Swiss package leakage.
- Verified the result with `dotnet build NoxAeterna.sln` and `dotnet test NoxAeterna.sln`.

Changed files:

- `NoxAeterna.Domain/Astrology/CelestialBody.cs`
- `NoxAeterna.Domain/Astrology/PlanetPosition.cs`
- `NoxAeterna.Astronomy/Calculation/ChartCalculationRequest.cs`
- `NoxAeterna.Astronomy/Calculation/ChartCalculationResult.cs`
- `NoxAeterna.Astronomy/Calculation/IEphemerisCalculator.cs`
- `NoxAeterna.Tests/Astronomy/PlanetPositionTests.cs`
- `NoxAeterna.Tests/Astronomy/ChartCalculationContractsTests.cs`
- `NoxAeterna.Tests/Astronomy/ProjectBoundaryTests.cs`
- `docs/DOMAIN-MODEL.md`
- `docs/ASTRONOMY-ENGINE.md`
- `docs/DECISIONS-LOG.md`
- `docs/NEXT-STEPS.md`
- `docs/SESSION-LOG.md`

Next actions:

- Create a minimal `NatalChart` contract or model that combines `BirthMoment`, calculated `PlanetPosition` collection, and aspect output using existing `AspectMath`.
- Keep houses and Swiss Ephemeris-specific integration out of scope for that step.

## 2026-05-15: Minimal Natal Chart Snapshot and Aspect Results

Summary:

- Added the first minimal `NatalChart` domain model that combines a resolved `BirthMoment`, read-only `PlanetPosition` collection, and read-only calculated major aspects.
- Added `CalculatedAspect` and `PlanetaryAspectCalculator` in `NoxAeterna.Domain` to detect canonical major-aspect pairs using the existing angular delta and orb math.
- Added focused tests for exact aspects, wrap-around across 0 degrees, duplicate-reversal avoidance, self-aspect avoidance, orb behavior, and public read-only chart collections.
- Verified the result with `dotnet build NoxAeterna.sln` and `dotnet test NoxAeterna.sln`.

Changed files:

- `NoxAeterna.Domain/Astrology/CalculatedAspect.cs`
- `NoxAeterna.Domain/Astrology/PlanetaryAspectCalculator.cs`
- `NoxAeterna.Domain/Astrology/NatalChart.cs`
- `NoxAeterna.Tests/Astrology/NatalChartTests.cs`
- `docs/DOMAIN-MODEL.md`
- `docs/ASTRONOMY-ENGINE.md`
- `docs/DECISIONS-LOG.md`
- `docs/NEXT-STEPS.md`
- `docs/SESSION-LOG.md`

Next actions:

- Create the first render-independent chart geometry contracts for circular chart layout and glyph slot placement.
- Keep geometry independent from Avalonia and separate from rendering concerns.

## 2026-05-15: Geometry Contracts and Localization Foundation

Summary:

- Added the first render-independent circular chart geometry contracts in `NoxAeterna.Geometry`, including angular positions, radial points, zodiac sectors, planetary glyph slots, aspect line geometry, and a deterministic `CircularChartLayoutBuilder`.
- Added the first presentation-side localization, theme, and user-preference contracts in `NoxAeterna.Presentation`, including deterministic fallback localization, separate application and interpretation language preferences, and stable theme identifiers.
- Added focused tests for geometry normalization, circular placement, wrap-around behavior, deterministic output, localization fallback, theme registry behavior, and project boundary checks for `Geometry` and `Presentation`.
- Added small JSON localization examples under `resources/localization/...`.
- Verified the result with `dotnet build NoxAeterna.sln` and `dotnet test NoxAeterna.sln`.

Changed files:

- `NoxAeterna.Geometry/Charts/AngularPosition.cs`
- `NoxAeterna.Geometry/Charts/RadialPoint.cs`
- `NoxAeterna.Geometry/Charts/ZodiacSectorGeometry.cs`
- `NoxAeterna.Geometry/Charts/PlanetGlyphSlot.cs`
- `NoxAeterna.Geometry/Charts/AspectLineGeometry.cs`
- `NoxAeterna.Geometry/Charts/CircularChartLayout.cs`
- `NoxAeterna.Geometry/Charts/CircularChartLayoutBuilder.cs`
- `NoxAeterna.Presentation/Localization/LanguageCode.cs`
- `NoxAeterna.Presentation/Localization/LocalizationKey.cs`
- `NoxAeterna.Presentation/Localization/LocalizationScope.cs`
- `NoxAeterna.Presentation/Localization/LocalizationEntry.cs`
- `NoxAeterna.Presentation/Localization/LocalizationCatalog.cs`
- `NoxAeterna.Presentation/Localization/LocalizationResult.cs`
- `NoxAeterna.Presentation/Localization/ILocalizationProvider.cs`
- `NoxAeterna.Presentation/Localization/FallbackLocalizationProvider.cs`
- `NoxAeterna.Presentation/Preferences/ApplicationLanguagePreference.cs`
- `NoxAeterna.Presentation/Preferences/InterpretationLanguagePreference.cs`
- `NoxAeterna.Presentation/Preferences/UserPreferences.cs`
- `NoxAeterna.Presentation/Theming/ThemeId.cs`
- `NoxAeterna.Presentation/Theming/ThemeDefinition.cs`
- `NoxAeterna.Presentation/Theming/ThemeRegistry.cs`
- `NoxAeterna.Tests/NoxAeterna.Tests.csproj`
- `NoxAeterna.Tests/Geometry/CircularChartLayoutBuilderTests.cs`
- `NoxAeterna.Tests/Presentation/LocalizationContractsTests.cs`
- `NoxAeterna.Tests/Presentation/ProjectBoundaryTests.cs`
- `resources/localization/ui/ru.json`
- `resources/localization/ui/en.json`
- `resources/localization/interpretation/ru.json`
- `resources/localization/interpretation/en.json`
- `docs/AGENTS.md`
- `docs/ARCHITECTURE.md`
- `docs/CODING-GUIDELINES.md`
- `docs/DECISIONS-LOG.md`
- `docs/DOMAIN-MODEL.md`
- `docs/GEOMETRY-ENGINE.md`
- `docs/NEXT-STEPS.md`
- `docs/SESSION-LOG.md`
- `docs/UI-VISION.md`

Next actions:

- Introduce the first render-layer contracts in `NoxAeterna.Rendering`.
- Connect rendering to `CircularChartLayout` through Avalonia drawing abstractions without building full UI screens yet.

## 2026-05-15: First Rendering-Layer Contracts

Summary:

- Added the first minimal rendering-layer contracts in `NoxAeterna.Rendering`: `ChartRenderOptions`, `ChartRenderScene`, and `CircularChartRenderer`.
- Added a scene-based rendering boundary where Avalonia drawing code consumes `CircularChartLayout` through `ChartRenderScene` instead of using `NatalChart` directly.
- Implemented deterministic technical drawing of the outer chart circle, zodiac sector separators, aspect lines, and planet marker placeholders using Avalonia `DrawingContext`.
- Added focused tests for render-scene determinism, render-option validation, and rendering project boundary rules.
- Verified the result with `dotnet build NoxAeterna.sln` and `dotnet test NoxAeterna.sln`.

Changed files:

- `NoxAeterna.Rendering/NoxAeterna.Rendering.csproj`
- `NoxAeterna.Rendering/Charts/ChartRenderOptions.cs`
- `NoxAeterna.Rendering/Charts/ChartRenderScene.cs`
- `NoxAeterna.Rendering/Charts/CircularChartRenderer.cs`
- `NoxAeterna.Tests/NoxAeterna.Tests.csproj`
- `NoxAeterna.Tests/Rendering/ChartRenderSceneTests.cs`
- `NoxAeterna.Tests/Rendering/ChartRenderOptionsTests.cs`
- `NoxAeterna.Tests/Rendering/RenderingBoundaryTests.cs`
- `README.md`
- `docs/THIRD-PARTY.md`
- `docs/DECISIONS-LOG.md`
- `docs/GEOMETRY-ENGINE.md`
- `docs/NEXT-STEPS.md`
- `docs/RENDERING-ENGINE.md`
- `docs/SESSION-LOG.md`

Next actions:

- Add a minimal temporary/debug preview path in `NoxAeterna.App`.
- Use a deterministic sample chart only to visually verify the Geometry -> Rendering pipeline.

## 2026-05-15: Temporary Debug Chart Preview Window

Summary:

- Added a temporary debug-only preview path in `NoxAeterna.App` to visually verify the current chart pipeline without introducing product UI.
- Added `DebugSampleNatalChartFactory` with deterministic hardcoded domain data, `DebugChartPreviewControl`, and a temporary `MainWindow` host that renders the sample chart through `CircularChartLayoutBuilder`, `ChartRenderScene`, and `CircularChartRenderer`.
- Added focused tests for sample-chart determinism and an app boundary check confirming that the app project still does not reference `NoxAeterna.Astronomy`.
- Verified the result with `dotnet build NoxAeterna.sln` and `dotnet test NoxAeterna.sln`.

Changed files:

- `NoxAeterna.App/NoxAeterna.App.csproj`
- `NoxAeterna.App/Debug/DebugSampleNatalChartFactory.cs`
- `NoxAeterna.App/Debug/DebugChartPreviewControl.cs`
- `NoxAeterna.App/MainWindow.axaml`
- `NoxAeterna.Tests/NoxAeterna.Tests.csproj`
- `NoxAeterna.Tests/App/DebugSampleNatalChartFactoryTests.cs`
- `NoxAeterna.Tests/App/AppBoundaryTests.cs`
- `docs/NEXT-STEPS.md`
- `docs/RENDERING-ENGINE.md`
- `docs/SESSION-LOG.md`

Next actions:

- Replace the temporary debug-only host with a thin Presentation-level shell structure.
- Keep chart rendering isolated in `NoxAeterna.Rendering` while the shell becomes more real.
