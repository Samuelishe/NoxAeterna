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
