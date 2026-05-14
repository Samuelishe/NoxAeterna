# Architecture

Nox Aeterna should follow clean architecture principles without enterprise overengineering. The goal is clear boundaries, testable components, and a stable core that can support astronomy, symbolic systems, rendering, persistence, and UI without coupling them together.

## Expected Solution Structure

Starting assumption:

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

This may evolve as implementation reveals better boundaries.

## Layer Responsibilities

### Domain

Contains core entities, value objects, domain rules, and concepts shared by the product. It should not depend on Avalonia, database libraries, Swiss Ephemeris, or UI frameworks.

### Astronomy

Owns ephemeris calculation abstractions, planetary position calculation, zodiac longitude conventions, retrograde detection, transits, lunar phase calculation, and time conversion rules. It must hide Swiss Ephemeris behind interfaces.

### Geometry

Owns chart layout math: angular placement, radial layout, collision avoidance, geometry models, hit-test geometry, and other render-independent chart preparation.

### Symbolics

Stores structured symbolic knowledge: planetary archetypes, zodiac archetypes, houses, aspects, elements, modalities, rulerships, Tarot correspondences, lunar correspondences, and historical references.

### Interpretation

Combines symbolic factors into user-facing meaning. It consumes structured symbolic knowledge and context. It should not know UI or persistence details.

### Rendering

Turns prepared geometry and rendering models into visuals. It may use Avalonia drawing primitives through a controlled rendering boundary, but should not calculate astrology or interpret meanings.

### Presentation

Contains MVVM view models, presentation models, commands, validation state, and UI orchestration. It coordinates use cases but does not perform core astronomy, geometry, interpretation, or persistence details directly.

### Infrastructure

Contains concrete adapters: Swiss Ephemeris wrapper, SQLite data access, logging, filesystem access, timezone lookup adapters, and other external integrations.

### App

Application composition root, Avalonia startup, dependency injection, configuration, and top-level shell.

### Tests

Contains unit and integration tests. Core math, domain rules, time conversion, aspect calculation, and geometry should be tested early.

## Dependency Rules

- UI must not know astronomical calculation details.
- Astronomy must not depend on Avalonia.
- Geometry must not depend on Avalonia UI controls.
- Interpretation must not depend on UI.
- Rendering receives prepared geometry/rendering models, not raw business logic.
- Domain must remain independent from infrastructure details.
- Swiss Ephemeris must be hidden behind an interface such as `IEphemerisCalculator`.
- Time handling must go through NodaTime.

## Avoid

- God services that calculate, interpret, render, and persist at once.
- Wide catch blocks that hide calculation or persistence failures.
- Static global state for time, location, or ephemeris data.
- UI controls as the primary chart model.
- Database attributes or ORM details inside domain objects unless explicitly justified.
- Premature abstractions that do not protect a real boundary.
