# Decisions Log

Lightweight Architecture Decision Record log. New entries should include date, decision, reason, and consequences.

## 2026-05-14: Use .NET 10

Decision: Target .NET 10 for the application and libraries.

Reason: The project is starting fresh and should use the intended modern .NET stack.

Consequences: Dependencies must be verified for .NET 10 compatibility before adoption.

## 2026-05-14: Use Avalonia UI

Decision: Use Avalonia for the cross-platform desktop UI.

Reason: Avalonia supports cross-platform desktop development in C# and fits the target technology direction.

Consequences: UI and rendering code must still respect architectural boundaries and avoid leaking Avalonia into domain, astronomy, interpretation, or persistence.

## 2026-05-14: Use NodaTime

Decision: Use NodaTime as the main time model.

Reason: Birth data, historical timezone behavior, DST, ambiguous local times, and reproducibility require a robust time library.

Consequences: Date/time APIs should flow through NodaTime types. Direct `DateTime` use should be limited to interop boundaries.

## 2026-05-14: Hide Swiss Ephemeris Behind an Interface

Decision: Use Swiss Ephemeris or an equivalent wrapper behind an interface such as `IEphemerisCalculator`.

Reason: The rest of the system should not depend on a specific package or native interop detail.

Consequences: Package choice can change without rewriting UI, domain, geometry, or interpretation logic.

## 2026-05-14: Separate Astronomy, Geometry, Rendering, and Interpretation

Decision: Treat astronomy calculation, chart geometry, drawing, and symbolic interpretation as separate subsystems.

Reason: Each subsystem has different responsibilities, dependencies, and test strategies.

Consequences: More project boundaries are required, but the application remains testable and avoids god objects.

## 2026-05-14: Create a Dedicated Symbolics Layer

Decision: Add a Symbolics layer distinct from Interpretation.

Reason: Structured symbolic knowledge and user-facing interpretation composition are different responsibilities.

Consequences: Symbolic correspondences can be curated, tested, and reused without being buried in prose generation.

## 2026-05-14: Start With Documentation Before Code

Decision: Initialize agent-oriented documentation before writing application code.

Reason: Future autonomous sessions need continuity, boundaries, and product direction before implementation begins.

Consequences: The repository currently documents intent only. Implementation must not be assumed until later stages create code.

## 2026-05-14: Lock Structured-First Interpretation

Decision: Use a structured-first interpretation pipeline:
`SymbolicFactor[] -> MeaningFragment[] -> ContextModifier[] -> Tension/Reinforcement analysis -> InterpretationBlock -> Optional Narrative Layer`.

Reason: This avoids fragile prose-first logic and keeps interpretation explainable, testable, and compositional.

Consequences: Narrative output, including any future LLM usage, must remain optional and downstream from symbolic logic.

## 2026-05-14: Keep Symbolics as a Typed Catalog

Decision: Keep Symbolics as a structured symbolic catalog with typed relationships rather than a graph database system or flat prose repository.

Reason: The MVP needs explainability and maintainability without graph-system overhead or prose-only ambiguity.

Consequences: Symbolics should define typed entities and relationships such as symbols, meanings, relationship types, and source metadata.

## 2026-05-14: Use Conservative MVP Timezone Strategy

Decision: For MVP, allow explicit/manual timezone selection and prioritize reproducibility over automatic place-to-timezone automation.

Reason: Timezone history and location resolution are complex, and pretending they are solved early would create unreliable birth calculations.

Consequences: `BirthMoment` must preserve local time, timezone ID, UTC instant, ambiguity resolution, and source/confidence metadata.

## 2026-05-14: Lock Render-Independent Geometry Contract

Decision: Geometry must produce render-independent models, and rendering must convert prepared models into Avalonia drawing operations.

Reason: This keeps chart layout testable and prevents Avalonia leakage into geometry or domain logic.

Consequences: Geometry must not return Avalonia controls, brushes, pens, or UI objects. Rendering contracts should be explicit.

## 2026-05-14: Enforce Permanent Attribution Tracking

Decision: Every future session must document third-party libraries, frameworks, assets, fonts, rendering systems, datasets, ephemeris sources, tools, borrowed code, adapted code, and generated assets when relevant.

Reason: The project needs durable authorship, licensing, and provenance records from the start.

Consequences: `README.md`, `docs/AGENTS.md`, `docs/CODING-GUIDELINES.md`, and `docs/THIRD-PARTY.md` must be updated whenever new external material is introduced.

## 2026-05-14: Declare Repository Ready for Scaffold Startup

Decision: After this clarification pass, treat the repository as ready for solution scaffold and implementation startup.

Reason: Core philosophical, architectural, rendering, interpretation, attribution, and continuity questions have been documented to a sufficient level for coding to begin.

Consequences: The next major step should be .NET 10 solution scaffold creation and dependency graph setup rather than another large planning pass.

## 2026-05-14: Create Initial Solution Scaffold With Minimal Dependencies

Decision: Create the initial .NET 10 solution scaffold with a minimal Avalonia app shell, class-library layer projects, xUnit test project, repository-level build props, and a narrow project reference graph.

Reason: The repository needed a real implementation starting point without introducing domain behavior, infrastructure behavior, or premature package sprawl.

Consequences: The next implementation step should focus on first domain primitives and tests rather than more scaffold work.

## 2026-05-14: Keep Birth-Time Types in Domain and Resolver in Astronomy

Decision: Place birth-time value objects and the `IBirthMomentResolver` contract in `NoxAeterna.Domain`, with the first TZDB-backed resolver implementation in `NoxAeterna.Astronomy`.

Reason: Birth-time representations are core domain concepts, while timezone-to-instant resolution is a time-conversion rule aligned with the astronomy layer.

Consequences: The domain stays explicit and NodaTime-based, while astronomy owns the first concrete time resolution behavior without introducing ephemeris coupling.

## 2026-05-14: Use Deterministic MVP Birth-Time Resolution

Decision: Use a deterministic MVP resolver strategy where ambiguous local times resolve to the earlier occurrence and invalid local times shift forward by the gap duration.

Reason: The first time model needs reproducible, testable behavior before more elaborate user-configurable resolution strategies exist.

Consequences: `BirthMoment` stores `TimeResolutionStatus`, and tests must cover normal, ambiguous, and invalid local-time cases explicitly.

## 2026-05-14: Keep Position Models in Domain and Calculation Boundary in Astronomy

Decision: Keep `CelestialBody` and `PlanetPosition` in `NoxAeterna.Domain`, while placing `ChartCalculationRequest`, `ChartCalculationResult`, and `IEphemerisCalculator` in `NoxAeterna.Astronomy`.

Reason: The position model is shared domain data, while calculation orchestration and provider abstraction belong to the astronomy layer.

Consequences: Future ephemeris implementations can stay astronomy-local, while domain models remain reusable by chart, transit, and archive features without provider leakage.

## 2026-05-15: Keep Natal Chart Snapshot and Aspect Detection in Domain

Decision: Keep the minimal `NatalChart`, `CalculatedAspect`, and `PlanetaryAspectCalculator` in `NoxAeterna.Domain`.

Reason: These types are pure domain snapshots and deterministic angle rules over already calculated positions. They do not need ephemeris provider details, UI concerns, or persistence behavior.

Consequences: Astronomy remains responsible for producing positions, while the domain can compose chart snapshots and major aspects without Swiss Ephemeris coupling.

## 2026-05-15: Keep Circular Chart Geometry Render-Independent

Decision: Keep the first circular chart layout contracts in `NoxAeterna.Geometry`, using chart-space angles, normalized radius ratios, and plain numeric geometry models with no Avalonia types.

Reason: Chart placement math must stay testable and reusable before any rendering code or UI surface is introduced.

Consequences: Rendering will later consume `CircularChartLayout` output rather than computing placement directly, and geometry conventions such as `0°` at the top and clockwise rotation are now explicit.

## 2026-05-15: Keep Localization and Preferences in Presentation

Decision: Keep localization contracts, fallback resolution, theme identifiers, and language/theme preferences in `NoxAeterna.Presentation` rather than `NoxAeterna.Domain`.

Reason: Domain models must remain language-neutral, while language selection, localization catalogs, and theme selection are application-facing concerns.

Consequences: Future UI, interpretation, and symbolic text should resolve through localization keys and providers, with a deterministic MVP fallback chain of `selected language -> ru -> key`.

## 2026-05-15: Keep the First Rendering Boundary Scene-Based

Decision: Make the first renderer consume `ChartRenderScene` derived from `CircularChartLayout`, instead of accepting `NatalChart` or astronomy-facing models directly.

Reason: Rendering should prove the Geometry -> Rendering handoff without backsliding into raw domain-model rendering or calculation leakage.

Consequences: Geometry remains the only source of chart placement, while rendering stays focused on Avalonia drawing commands, numeric options, and visual placeholder output.

## 2026-05-15: Start the App Shell in Presentation While Keeping Debug Preview Temporary

Decision: Start the application shell in `NoxAeterna.Presentation` with explicit section identifiers and localization-key-based navigation items, while continuing to host the chart preview as a temporary debug section.

Reason: The app now needs a stable shell shape for future real sections, but the current chart host still exists only to verify the rendering pipeline.

Consequences: The shell can grow into real sections without rewriting the window host from scratch, and the debug preview remains visibly temporary instead of being mistaken for the final Astrology UI.

## 2026-05-15: Keep the First Settings Foundation In Presentation and In Memory

Decision: Keep the first language and theme settings foundation in `NoxAeterna.Presentation`, with in-memory updates only and no persistence adapter yet.

Reason: The app needs a real settings shape before storage exists, but introducing persistence now would blur boundaries and overextend the current step.

Consequences: Settings can already model separate application and interpretation languages plus theme selection, but `settings.json` storage, app-data location handling, and real theme resource switching remain deferred.

## 2026-05-15: Use Flat JSON Catalogs for the First UI Localization Loader

Decision: Load UI localization catalogs from simple flat key-value JSON files and compose fallback behavior through `FallbackLocalizationProvider`.

Reason: The app needs real resource-backed localization without introducing a heavy localization framework or premature nested catalog complexity.

Consequences: UI localization can now be loaded from `resources/localization/ui/<language>.json`, while persistence, interpretation localization, and broader resource-loading architecture remain deferred.

## 2026-05-15: Keep the First Real Theme Switching in App and Theme Metadata in Presentation

Decision: Keep theme metadata and selection contracts in `NoxAeterna.Presentation`, and apply the active `ThemeId` through a small `AppThemeController` in `NoxAeterna.App` using Avalonia resource dictionaries.

Reason: Theme identity and preference state belong to presentation concerns, while actual Avalonia resource application belongs at the application host boundary.

Consequences: Dark/light switching now works in memory without polluting domain, rendering, or infrastructure. Persistence, richer theme catalogs, and broader design-system work remain deferred.

## 2026-05-15: Replace the Visible Debug Section With an Astrology Workspace Foundation

Decision: Replace the visible debug-preview shell route with a first reusable astrology workspace foundation, while keeping development-only sample chart generation in an internal sample area under `NoxAeterna.App`.

Reason: The app now needs a real workspace structure that can later host input, chart controls, and interpretation panels without presenting the chart area as pure debug infrastructure.

Consequences: The shell opens into the astrology workspace by default, rendering stays isolated behind `ChartRenderScene`, and development-only sample data remains an internal temporary source rather than a visible product section.

## 2026-05-15: Keep Birth-Data Parsing and Validation in Presentation

Decision: Keep the first structured birth-data input state, parsing, validation, and `BirthData` mapping in `NoxAeterna.Presentation`, then pass only validated values into `NoxAeterna.Domain`.

Reason: Input parsing rules, field errors, and localization-key-based validation feedback are presentation concerns, while domain types should continue receiving clean value objects instead of UI-specific text state.

Consequences: The astrology workspace can validate and map user input without leaking text parsing into domain models. Resolver and calculation wiring can be added later on top of already validated `BirthData`.

## 2026-05-15: Keep Birth Input Offline-First and TZDB-Based

Decision: Refine the initial birth-data input flow around an offline-first mode: date picker, constrained time input, manual coordinates, and timezone selection from the local TZDB list.

Reason: The MVP must remain honest and usable without network access, while still preparing for future online lookup and AppData-backed recent-place storage.

Consequences: Online geocoding, reverse lookup, and a bundled world-city database remain deferred. Manual coordinates stay as the current truthful fallback, and arbitrary timezone text is no longer accepted as the main UI path.

## 2026-05-15: Preserve Product Names Across Localizations and Apply App Language to Date-Time Controls

Decision: Keep `Nox Aeterna` and other intended Latin proper names untranslated in user-facing localization catalogs, and apply the selected application language to culture-sensitive desktop controls through an app-level culture boundary.

Reason: Product names should remain stable brand identifiers, while mixed-language date and time picker chrome becomes confusing if desktop controls do not follow the chosen UI language.

Consequences: Localization catalogs must preserve `Nox Aeterna` verbatim across languages, and `App` is responsible for applying culture to date and time controls without leaking localization behavior into domain models.

## 2026-05-15: Use an Input-Driven Demo Chart Pipeline Before Real Ephemerides

Decision: Connect validated birth-data input to the visible chart through `IBirthMomentResolver`, a clearly named fake deterministic `IEphemerisCalculator` implementation, `NatalChart`, geometry layout, and the existing renderer.

Reason: The app needs an honest end-to-end chart rebuild flow before Swiss Ephemeris integration, but that flow must stay visibly marked as demo-only and keep rendering isolated from calculation logic.

Consequences: `NoxAeterna.App` now acts as the composition root for the current development chart pipeline and references `NoxAeterna.Astronomy`. The visible chart can rebuild from user input, while real astronomy, persistence, and geocoding remain deferred.

## 2026-05-15: Place the First Real Swiss Ephemeris Adapter in Infrastructure

Decision: Put the first real `IEphemerisCalculator` implementation in `NoxAeterna.Infrastructure` as `SwissEphemerisCalculator`, using `SwissEphNet` while keeping all package-specific types inside the adapter boundary.

Reason: Swiss Ephemeris integration is an external dependency concern, not a domain, presentation, rendering, or geometry concern. The app already acts as the composition root and can swap implementations without changing core contracts.

Consequences: The live chart path now uses real planetary positions through `IEphemerisCalculator`, while the old `DevelopmentEphemerisCalculator` remains available as a fallback or test tool. External ephemeris data-file setup and formal repository license alignment remain follow-up items.
