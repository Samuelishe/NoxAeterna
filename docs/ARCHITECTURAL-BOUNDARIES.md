# Architectural Boundaries

This document is intentionally explicit. It exists to prevent layer leakage during autonomous development.

## Non-Negotiable Rules

- Rendering must not calculate astrology.
- Presentation must not normalize angles.
- Presentation must not calculate chart geometry.
- Interpretation must not access SQLite directly.
- Interpretation must not contain UI workflow logic.
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
- Presentation orchestrates use cases and view state; it does not own core symbolic logic.

## What To Reject In Review

Reject changes that:

- Put trigonometry or angle normalization into view models.
- Put Avalonia types into geometry contracts.
- Put prose generation into Symbolics.
- Put SQL access into interpretation code.
- Put Swiss Ephemeris calls into UI or rendering code.
- Put raw domain entities directly into rendering code when prepared render models are warranted.
- Hide critical calculation behavior in static helpers with shared mutable state.
