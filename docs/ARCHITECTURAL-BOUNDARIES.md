# Architectural Boundaries

This document is intentionally explicit. It exists to prevent layer leakage during autonomous development.

## Non-Negotiable Rules

- Rendering must not calculate astrology.
- Presentation must not normalize angles.
- Presentation must not calculate chart geometry.
- Core Interpretation must not access SQLite directly; `NoxAeterna.Interpretation.Sqlite` is the dedicated adapter behind core-owned store contracts.
- Interpretation must not contain UI workflow logic.
- Interpretation must not depend on App, Presentation, Avalonia, or AppData path construction.
- Tarot source DTOs, validation, canonical keys, package-store contracts, locale/mode resolution, typed results, and deterministic composition belong to Interpretation; SQLite implementation belongs only to Interpretation.Sqlite and visual controls belong to App.
- Symbolics must not contain user-facing prose generation.
- Geometry must remain Avalonia-independent.
- Geometry must not return Avalonia controls, brushes, pens, or UI objects.
- Domain must remain persistence-independent.
- Domain must remain UI-framework-independent.
- Rendering must consume prepared render models.
- Rendering must not reach into raw ephemeris or persistence adapters.
- Astronomy must hide Swiss Ephemeris behind interfaces.
- Astronomy must not depend on Avalonia.
- Infrastructure must not redefine domain rules that belong in core layers.

## Expected Handoffs

- Astronomy produces calculation results and domain-facing calculation data.
- Geometry consumes prepared calculation data and produces render-independent geometry models.
- Rendering consumes prepared geometry and rendering models and turns them into Avalonia drawing operations.
- Symbolics provides structured symbolic catalog data and typed relationships.
- Interpretation consumes symbolic factors and symbolics data and produces structured interpretation blocks.
- Presentation orchestrates use cases/view state and may transform typed Interpretation results plus already resolved pack-local labels into immutable display models; it owns neither filesystem access nor Avalonia/color/font types.
- App supplies the shipped pack source, catalog composition, selector/settings wiring, coordinator, pack-label seam, and Avalonia materialization without moving meaning or resolver fallback into UI. The current source remains contained built-in Classic only; interpretation AppData sources remain future work.
- Repository tooling may depend one-way on Interpretation and Interpretation.Sqlite to validate source and compile/inspect packages. It owns no production resolver behavior, has no App/Presentation/Infrastructure/Avalonia dependency, and is not a runtime App dependency.

## What To Reject In Review

Reject changes that:

- Put trigonometry or angle normalization into view models.
- Put Avalonia types into geometry contracts.
- Put prose generation into Symbolics.
- Put SQL access into interpretation code.
- Put Avalonia controls, AppData path construction, file-source composition, or settings I/O into interpretation-core contracts.
- Copy Interpretation schema contracts into repository tooling instead of using the approved one-way Tools.Repository → Interpretation/Interpretation.Sqlite references, or add a runtime product dependency on Tools.Repository.
- Put Swiss Ephemeris calls into UI or rendering code.
- Put raw domain entities directly into rendering code when prepared render models are warranted.
- Hide critical calculation behavior in static helpers with shared mutable state.
