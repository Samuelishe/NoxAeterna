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

## 2026-05-15: Thin Localized Shell Foundation

Summary:

- Added the first minimal shell foundation in `NoxAeterna.Presentation` with `ShellViewModel`, `ShellNavigationItem`, and `ShellSectionId`.
- Replaced the raw debug-only host in `MainWindow` with a thin localized shell layout in `NoxAeterna.App`, while keeping the chart preview as the default temporary debug section.
- Added a temporary in-memory shell localization factory in `App` and kept shell navigation labels localization-key-based.
- Added focused tests for shell section order, deterministic default section, localization-key-backed navigation items, and explicit temporary status for the debug preview section.
- Verified the result with `dotnet build NoxAeterna.sln` and `dotnet test NoxAeterna.sln`.

Changed files:

- `NoxAeterna.Presentation/Shell/ShellSectionId.cs`
- `NoxAeterna.Presentation/Shell/ShellNavigationItem.cs`
- `NoxAeterna.Presentation/Shell/ShellViewModel.cs`
- `NoxAeterna.App/Debug/DebugShellLocalizationFactory.cs`
- `NoxAeterna.App/MainWindow.axaml`
- `NoxAeterna.App/MainWindow.axaml.cs`
- `NoxAeterna.Tests/Presentation/ShellViewModelTests.cs`
- `docs/DECISIONS-LOG.md`
- `docs/NEXT-STEPS.md`
- `docs/RENDERING-ENGINE.md`
- `docs/SESSION-LOG.md`
- `docs/UI-VISION.md`

Next actions:

- Implement the first real Settings foundation for language and theme preferences without persistence.
- Keep the debug preview route available for chart-rendering verification while the shell grows.

## 2026-05-15: First In-Memory Settings Foundation

Summary:

- Added the first minimal settings presentation foundation in `NoxAeterna.Presentation`: `SettingsViewModel`, `LanguageOption`, and `ThemeOption`.
- Wired the shell `Settings` section in `NoxAeterna.App` to a temporary in-memory settings control for application language, interpretation language, and theme selection.
- Kept preference updates in memory only, with no JSON writing, app-data resolver, or infrastructure persistence.
- Extended the temporary localization bootstrap with settings-related keys and allowed shell relocalization from the selected application language.
- Added focused tests for available language/theme options, separation of application and interpretation languages, `ThemeId`-based theme selection, deterministic preference updates, and localization-key-based settings labels.
- Verified the result with `dotnet build NoxAeterna.sln` and `dotnet test NoxAeterna.sln`.

Changed files:

- `NoxAeterna.Presentation/Settings/LanguageOption.cs`
- `NoxAeterna.Presentation/Settings/ThemeOption.cs`
- `NoxAeterna.Presentation/Settings/SettingsViewModel.cs`
- `NoxAeterna.App/Debug/DebugSettingsControl.cs`
- `NoxAeterna.App/Debug/DebugShellLocalizationFactory.cs`
- `NoxAeterna.App/MainWindow.axaml.cs`
- `NoxAeterna.Tests/Presentation/SettingsViewModelTests.cs`
- `docs/CODING-GUIDELINES.md`
- `docs/DECISIONS-LOG.md`
- `docs/NEXT-STEPS.md`
- `docs/PERSISTENCE.md`
- `docs/SESSION-LOG.md`
- `docs/UI-VISION.md`

Next actions:

- Add JSON-backed localization loading for UI resources.
- Keep settings persistence deferred while replacing the in-memory localization bootstrap.

## 2026-05-15: JSON-Backed UI Localization Catalogs

Summary:

- Added `JsonLocalizationCatalogLoader` in `NoxAeterna.Presentation` to load flat key-value localization catalogs from JSON.
- Replaced the temporary in-memory shell/settings localization source in `NoxAeterna.App` with JSON-backed catalog loading through `DebugShellLocalizationProviderFactory`.
- Expanded `resources/localization/ui/ru.json` and `resources/localization/ui/en.json` to contain the current shell and settings labels.
- Configured `NoxAeterna.App` to copy UI localization JSON files into the app output so `dotnet run --project NoxAeterna.App` can load them reliably.
- Added focused tests for JSON catalog loading, malformed or invalid JSON handling, duplicate key rejection, and fallback behavior through the real Russian UI catalog.
- Verified the result with `dotnet build NoxAeterna.sln` and `dotnet test NoxAeterna.sln`.

Changed files:

- `NoxAeterna.Presentation/Localization/JsonLocalizationCatalogLoader.cs`
- `NoxAeterna.App/Debug/DebugShellLocalizationProviderFactory.cs`
- `NoxAeterna.App/NoxAeterna.App.csproj`
- `NoxAeterna.App/MainWindow.axaml.cs`
- `NoxAeterna.Tests/Presentation/JsonLocalizationCatalogLoaderTests.cs`
- `resources/localization/ui/ru.json`
- `resources/localization/ui/en.json`
- `docs/CODING-GUIDELINES.md`
- `docs/DECISIONS-LOG.md`
- `docs/NEXT-STEPS.md`
- `docs/SESSION-LOG.md`
- `docs/UI-VISION.md`
- `README.md`

Next actions:

- Add the first real dark/light theme switching path using `ThemeId`.
- Keep settings persistence deferred while theme switching is introduced.

## 2026-05-15: First Real Dark/Light Theme Switching

Summary:

- Added the first real `ThemeId`-driven dark/light theme switching path in `NoxAeterna.App` through `AppThemeController` and Avalonia resource dictionaries.
- Kept theme metadata and available themes in `NoxAeterna.Presentation`, and updated the settings flow to source available themes from `ThemeRegistry`.
- Updated the shell host and debug preview surface to consume theme resources while keeping chart rendering isolated in `NoxAeterna.Rendering`.
- Added focused tests for default theme registry contents and deterministic app-level theme application, including rejection of unregistered themes.
- Re-synchronized core documentation and `README.md` to match the current architecture and implementation state.
- Verified the result with `dotnet build NoxAeterna.sln`, `dotnet test NoxAeterna.sln`, and a short `dotnet run --project NoxAeterna.App` startup check with no immediate startup exception.

Changed files:

- `NoxAeterna.App/App.axaml.cs`
- `NoxAeterna.App/Debug/DebugChartPreviewControl.cs`
- `NoxAeterna.App/MainWindow.axaml`
- `NoxAeterna.App/MainWindow.axaml.cs`
- `NoxAeterna.App/Themes/AppThemeController.cs`
- `NoxAeterna.App/Themes/DarkThemeResources.axaml`
- `NoxAeterna.App/Themes/DarkThemeResources.axaml.cs`
- `NoxAeterna.App/Themes/LightThemeResources.axaml`
- `NoxAeterna.App/Themes/LightThemeResources.axaml.cs`
- `NoxAeterna.Presentation/Settings/SettingsViewModel.cs`
- `NoxAeterna.Presentation/Theming/ThemeRegistry.cs`
- `NoxAeterna.Tests/App/AppThemeControllerTests.cs`
- `NoxAeterna.Tests/Presentation/ThemeRegistryTests.cs`
- `README.md`
- `docs/ARCHITECTURE.md`
- `docs/CODING-GUIDELINES.md`
- `docs/DECISIONS-LOG.md`
- `docs/NEXT-STEPS.md`
- `docs/PERSISTENCE.md`
- `docs/RENDERING-ENGINE.md`
- `docs/SESSION-LOG.md`
- `docs/UI-VISION.md`

Next actions:

- Start replacing the temporary debug preview section with the first real astrology workspace foundation.
- Keep settings persistence deferred while the shell, workspace, and preference flow stabilize.

## 2026-05-15: First Astrology Workspace Foundation

Summary:

- Replaced the visible debug-preview shell route with the first astrology workspace foundation.
- Added presentation-side astrology workspace models and moved the shell default section to `Astrology`.
- Added app-level `AstrologyWorkspaceControl` and `AstrologyChartSurfaceControl`, while keeping development-only sample chart and scene generation in a dedicated `Samples` area.
- Preserved the Geometry -> Rendering isolation by feeding the workspace a prepared `ChartRenderScene` instead of calculation services or raw rendering code in presentation models.
- Updated localization catalogs, theme resource usage for workspace panels, and synchronized architecture/UI/rendering documentation to the new shell/workspace state.

Changed files:

- `NoxAeterna.Presentation/Astrology/AstrologyWorkspacePanelId.cs`
- `NoxAeterna.Presentation/Astrology/AstrologyWorkspacePanel.cs`
- `NoxAeterna.Presentation/Astrology/AstrologyWorkspaceViewModel.cs`
- `NoxAeterna.Presentation/Shell/ShellSectionId.cs`
- `NoxAeterna.Presentation/Shell/ShellViewModel.cs`
- `NoxAeterna.App/Astrology/AstrologyChartSurfaceControl.cs`
- `NoxAeterna.App/Astrology/AstrologyWorkspaceControl.cs`
- `NoxAeterna.App/Samples/DevelopmentSampleNatalChartFactory.cs`
- `NoxAeterna.App/Samples/DevelopmentSampleChartSceneFactory.cs`
- `NoxAeterna.App/MainWindow.axaml.cs`
- `NoxAeterna.App/Themes/DarkThemeResources.axaml`
- `NoxAeterna.App/Themes/LightThemeResources.axaml`
- `NoxAeterna.Tests/App/DevelopmentSampleFactoriesTests.cs`
- `NoxAeterna.Tests/Presentation/AstrologyWorkspaceViewModelTests.cs`
- `NoxAeterna.Tests/Presentation/ShellViewModelTests.cs`
- `resources/localization/ui/ru.json`
- `resources/localization/ui/en.json`
- `README.md`
- `docs/ARCHITECTURE.md`
- `docs/DECISIONS-LOG.md`
- `docs/GEOMETRY-ENGINE.md`
- `docs/NEXT-STEPS.md`
- `docs/RENDERING-ENGINE.md`
- `docs/SESSION-LOG.md`
- `docs/UI-VISION.md`

Next actions:

- Introduce the first structured birth-data input foundation inside the astrology workspace.
- Keep settings persistence and real ephemeris-backed calculation flow deferred while the input foundation is introduced.

## 2026-05-15: Birth Data Input Foundation

Summary:

- Added the first structured birth-data input foundation in `NoxAeterna.Presentation`, including editable input state, birth-time accuracy options, validation rules, validation results, and explicit mapping to domain `BirthData`.
- Added an app-side `BirthDataInputControl` inside the astrology workspace, with date, time, place, latitude, longitude, timezone, and birth-time-accuracy fields plus localized validation feedback.
- Kept validation and text parsing in presentation code, with no geocoding, no persistence, no resolver wiring, and no real chart recalculation yet.
- Preserved the current sample-based chart surface so the workspace still renders while input remains an independent foundation.
- Updated localization catalogs and synchronized the architecture, domain, astronomy, UI, README, and next-step documents to the new workspace state.

Changed files:

- `NoxAeterna.Presentation/Astrology/BirthDataInputField.cs`
- `NoxAeterna.Presentation/Astrology/BirthTimeAccuracyOption.cs`
- `NoxAeterna.Presentation/Astrology/BirthDataInputError.cs`
- `NoxAeterna.Presentation/Astrology/BirthDataValidationResult.cs`
- `NoxAeterna.Presentation/Astrology/BirthDataInputState.cs`
- `NoxAeterna.Presentation/Astrology/BirthDataInputValidator.cs`
- `NoxAeterna.Presentation/Astrology/BirthDataInputMapper.cs`
- `NoxAeterna.Presentation/Astrology/BirthDataInputViewModel.cs`
- `NoxAeterna.Presentation/Astrology/AstrologyWorkspaceViewModel.cs`
- `NoxAeterna.App/Astrology/BirthDataInputControl.cs`
- `NoxAeterna.App/Astrology/AstrologyWorkspaceControl.cs`
- `NoxAeterna.App/Themes/DarkThemeResources.axaml`
- `NoxAeterna.App/Themes/LightThemeResources.axaml`
- `NoxAeterna.Tests/Presentation/BirthDataInputViewModelTests.cs`
- `NoxAeterna.Tests/Presentation/AstrologyWorkspaceViewModelTests.cs`
- `resources/localization/ui/ru.json`
- `resources/localization/ui/en.json`
- `README.md`
- `docs/ASTRONOMY-ENGINE.md`
- `docs/DECISIONS-LOG.md`
- `docs/DOMAIN-MODEL.md`
- `docs/NEXT-STEPS.md`
- `docs/SESSION-LOG.md`
- `docs/UI-VISION.md`

Next actions:

- Connect validated birth-data input to `IBirthMomentResolver` and a deterministic or fake chart-calculation path.
- Keep persistence, geocoding, Swiss Ephemeris, and real profile workflows deferred while input-driven chart flow is introduced.

## 2026-05-15: Birth Data Input UX Refinement

Summary:

- Replaced the raw typed date and timezone path with a more honest typed input model: `DatePicker`, constrained time input, and timezone selection from the local TZDB list.
- Added an explicit `LocationSource` model so the current offline/manual flow can distinguish name-only input from manual-coordinate input and evolve later toward online lookup or saved places.
- Kept unknown birth time honest by allowing empty time input, disabling the time control in that mode, and preserving a separate technical noon fallback for later calculation work without changing `BirthTimeAccuracy.UnknownTime`.
- Locked the rule that user-specific runtime data must eventually live under AppData or the platform-equivalent user data location rather than in the repository or next to the executable.
- Updated localization resources, validation tests, and architecture/persistence/UI documentation to reflect the offline-first birth-input model and deferred online lookup.

Changed files:

- `NoxAeterna.Presentation/Astrology/LocationSource.cs`
- `NoxAeterna.Presentation/Astrology/TimezoneOption.cs`
- `NoxAeterna.Presentation/Astrology/BirthDataInputState.cs`
- `NoxAeterna.Presentation/Astrology/BirthDataInputValidator.cs`
- `NoxAeterna.Presentation/Astrology/BirthDataInputMapper.cs`
- `NoxAeterna.Presentation/Astrology/BirthDataInputViewModel.cs`
- `NoxAeterna.App/Astrology/BirthDataInputControl.cs`
- `NoxAeterna.Tests/Presentation/BirthDataInputViewModelTests.cs`
- `resources/localization/ui/ru.json`
- `resources/localization/ui/en.json`
- `README.md`
- `docs/ARCHITECTURE.md`
- `docs/ASTRONOMY-ENGINE.md`
- `docs/CODING-GUIDELINES.md`
- `docs/DECISIONS-LOG.md`
- `docs/DOMAIN-MODEL.md`
- `docs/NEXT-STEPS.md`
- `docs/PERSISTENCE.md`
- `docs/SESSION-LOG.md`
- `docs/UI-VISION.md`

Next actions:

- Connect validated birth-data input to `IBirthMomentResolver` and a deterministic or fake chart-building flow.
- Keep online lookup, persistence, AppData implementation, and Swiss Ephemeris integration deferred while input-driven chart rebuilding is introduced.

## 2026-05-15: UI Localization and Default-Window Layout Correction Pass

Summary:

- Corrected the visible product-name localization so `Nox Aeterna` now stays unchanged across UI languages instead of being translated in Russian catalogs.
- Cleaned the current Russian UI catalogs to remove avoidable mixed English strings from workspace and birth-input copy, and replaced the main time-zone label with a user-facing `Часовой пояс` / `Time zone` plus helper text for TZDB/IANA identifiers.
- Added an app-level culture controller so date and time pickers follow the selected application language more closely, while also configuring the date picker with neutral numeric formats.
- Fixed the current default-window clipping path by making the right birth-data side panel scroll instead of relying on the whole workspace to fit without overflow.
- Added focused tests for real localization catalogs, proper-name preservation, required UI keys, and deterministic culture resolution.

Changed files:

- `NoxAeterna.App/Localization/ApplicationCultureController.cs`
- `NoxAeterna.App/MainWindow.axaml`
- `NoxAeterna.App/MainWindow.axaml.cs`
- `NoxAeterna.App/Astrology/AstrologyWorkspaceControl.cs`
- `NoxAeterna.App/Astrology/BirthDataInputControl.cs`
- `NoxAeterna.Presentation/Astrology/BirthDataInputViewModel.cs`
- `NoxAeterna.Tests/App/ApplicationCultureControllerTests.cs`
- `NoxAeterna.Tests/Presentation/BirthDataInputViewModelTests.cs`
- `NoxAeterna.Tests/Presentation/JsonLocalizationCatalogLoaderTests.cs`
- `NoxAeterna.Tests/Presentation/LocalizationContractsTests.cs`
- `resources/localization/ui/ru.json`
- `resources/localization/ui/en.json`
- `README.md`
- `docs/ARCHITECTURE.md`
- `docs/CODING-GUIDELINES.md`
- `docs/DECISIONS-LOG.md`
- `docs/NEXT-STEPS.md`
- `docs/SESSION-LOG.md`
- `docs/UI-VISION.md`

Next actions:

- Connect validated birth-data input to `IBirthMomentResolver` and a deterministic or fake chart-building flow.
- Keep sample chart rendering internal while the workspace starts rebuilding charts from user-entered data.

## 2026-05-15: Input-Driven Demo Chart Rebuild Flow

Summary:

- Connected validated birth-data input to the visible chart through an app-level development pipeline: `BirthData` mapping, `TzdbBirthMomentResolver`, fake deterministic ephemeris calculation, `NatalChart`, geometry layout, and `ChartRenderScene`.
- Added `DevelopmentEphemerisCalculator` in `NoxAeterna.Astronomy` as a clearly marked fake implementation of `IEphemerisCalculator`. It is deterministic, varies by input, and is explicitly not real astronomy.
- Added app-level chart coordinator and pipeline classes so the astrology workspace can rebuild the chart on successful validation while leaving the previous chart unchanged on invalid input.
- Kept the renderer isolated: the chart surface still consumes only prepared `ChartRenderScene`.
- Added a localized honesty notice that the current chart uses a demo calculation and renamed the action button to build the chart explicitly.

Changed files:

- `NoxAeterna.Domain/Birth/BirthMoment.cs`
- `NoxAeterna.Astronomy/Calculation/ChartCalculationRequest.cs`
- `NoxAeterna.Astronomy/Calculation/DevelopmentEphemerisCalculator.cs`
- `NoxAeterna.App/NoxAeterna.App.csproj`
- `NoxAeterna.App/Astrology/AstrologyChartSurfaceControl.cs`
- `NoxAeterna.App/Astrology/AstrologyWorkspaceControl.cs`
- `NoxAeterna.App/Astrology/BirthDataInputControl.cs`
- `NoxAeterna.App/Astrology/DevelopmentChartBuildResult.cs`
- `NoxAeterna.App/Astrology/DevelopmentAstrologyChartPipeline.cs`
- `NoxAeterna.App/Astrology/DevelopmentAstrologyChartCoordinator.cs`
- `NoxAeterna.App/MainWindow.axaml.cs`
- `NoxAeterna.Tests/App/AppBoundaryTests.cs`
- `NoxAeterna.Tests/App/DevelopmentAstrologyChartCoordinatorTests.cs`
- `NoxAeterna.Tests/Astronomy/ChartCalculationContractsTests.cs`
- `NoxAeterna.Tests/Astronomy/DevelopmentEphemerisCalculatorTests.cs`
- `NoxAeterna.Tests/Birth/BirthMomentTests.cs`
- `README.md`
- `docs/ARCHITECTURE.md`
- `docs/ASTRONOMY-ENGINE.md`
- `docs/DECISIONS-LOG.md`
- `docs/NEXT-STEPS.md`
- `docs/RENDERING-ENGINE.md`
- `docs/SESSION-LOG.md`
- `docs/UI-VISION.md`
- `resources/localization/ui/ru.json`
- `resources/localization/ui/en.json`

Next actions:

- Add the first in-memory profile or session draft model for current unsaved birth data and chart state.
- Keep persistence, real ephemerides, geocoding, and interpretation work deferred until that in-memory state model exists.

## 2026-05-15: Astrology Workspace UI Sanity Pass

Summary:

- Reworked the current astrology workspace layout so the right birth-data column feels less cramped at the default window size and remains comfortably scrollable.
- Increased effective chart usage of the available left-side area by widening the chart column and letting the chart surface fill a stronger vertical slot.
- Replaced the workspace subtitle with a more product-oriented description while keeping the demo-calculation warning as a separate explicit localized notice near the chart.
- Improved the birth-data form hierarchy with clearer spacing and compact two-column groups for date/time and latitude/longitude fields, without changing the underlying architecture or the Avalonia-native control choice.
- Added tests that keep the workspace subtitle and demo warning as separate localization concepts.

Changed files:

- `NoxAeterna.App/Astrology/AstrologyWorkspaceControl.cs`
- `NoxAeterna.App/Astrology/BirthDataInputControl.cs`
- `NoxAeterna.Presentation/Astrology/AstrologyWorkspaceViewModel.cs`
- `NoxAeterna.Tests/Presentation/AstrologyWorkspaceViewModelTests.cs`
- `NoxAeterna.Tests/Presentation/JsonLocalizationCatalogLoaderTests.cs`
- `resources/localization/ui/ru.json`
- `resources/localization/ui/en.json`
- `docs/CODING-GUIDELINES.md`
- `docs/NEXT-STEPS.md`
- `docs/SESSION-LOG.md`
- `docs/UI-VISION.md`

Next actions:

- Add the first in-memory profile or session draft model for current unsaved birth data and chart state.
- Keep persistence, real ephemerides, geocoding, and interpretation work deferred until that in-memory state model exists.
