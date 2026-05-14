# Architecture

Nox Aeterna should follow clean architecture principles without enterprise overengineering. The goal is clear boundaries, testable components, and a stable core that can support astronomy, symbolic systems, rendering, persistence, and UI without coupling them together.

## Repository Readiness

This architecture pass is the final pre-scaffold clarification step.

After this document set, the repository should be considered ready for:

- .NET 10 solution scaffold.
- Project creation.
- Dependency graph setup.
- Test infrastructure.
- Domain primitives.
- Astronomy abstractions.
- Rendering contracts.

No further large-scale planning pass is required before scaffold work starts.

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

Contains MVVM view models, presentation models, commands, validation state, UI orchestration, localization contracts, theme selection, and user preference models. It coordinates use cases but does not perform core astronomy, geometry, interpretation, or persistence details directly.

### Infrastructure

Contains concrete adapters: Swiss Ephemeris wrapper, SQLite data access, logging, filesystem access, timezone lookup adapters, and other external integrations.

### App

Application composition root, Avalonia startup, dependency injection, configuration, and top-level shell.

### Tests

Contains unit and integration tests. Core math, domain rules, time conversion, aspect calculation, and geometry should be tested early.

## Planned Dependency Direction

Initial dependency direction for scaffold:

- `NoxAeterna.Domain`: no dependency on Avalonia, SQLite, Dapper, or Swiss Ephemeris packages.
- `NoxAeterna.Symbolics`: depends on `NoxAeterna.Domain` only if shared primitives are required.
- `NoxAeterna.Astronomy`: may depend on `NoxAeterna.Domain` and NodaTime; must not depend on Avalonia.
- `NoxAeterna.Geometry`: may depend on `NoxAeterna.Domain` and astronomy-facing data contracts where justified; must not depend on Avalonia UI objects.
- `NoxAeterna.Interpretation`: may depend on `NoxAeterna.Domain` and `NoxAeterna.Symbolics`; must not depend on Presentation or persistence infrastructure.
- `NoxAeterna.Rendering`: may depend on `NoxAeterna.Geometry` and render models; must not contain astronomy or interpretation logic.
- `NoxAeterna.Presentation`: may depend on domain-facing application contracts and presentation models; should not own core calculation logic.
- `NoxAeterna.Infrastructure`: contains adapters to ephemeris, SQLite, logging, and external services; references core abstractions but should not redefine them.
- `NoxAeterna.App`: composition root only.
- `NoxAeterna.Tests`: references whichever project each test actually exercises.

Exact references should be kept minimal. Do not create convenience references that blur boundaries.

## Dependency Rules

- UI must not know astronomical calculation details.
- Astronomy must not depend on Avalonia.
- Geometry must not depend on Avalonia UI controls.
- Interpretation must not depend on UI.
- Rendering receives prepared geometry/rendering models, not raw business logic.
- Domain must remain independent from infrastructure details.
- Swiss Ephemeris must be hidden behind an interface such as `IEphemerisCalculator`.
- Time handling must go through NodaTime.
- Presentation must not normalize angles or perform chart math.
- Interpretation must not access SQLite directly.
- Symbolics must not contain user-facing prose generation.
- Geometry must remain render-framework-independent.
- Rendering must consume prepared render models rather than raw domain state.
- Domain models must remain language-neutral.
- User-facing text resolution must happen through localization keys and localization providers rather than domain helper methods.

See `docs/ARCHITECTURAL-BOUNDARIES.md` for stricter rules.

## Localization, Themes, and Preferences

All human-facing text should be localization-driven before substantial UI work begins.

Current architectural direction:

- Domain stays language-neutral.
- Presentation owns localization contracts and preference models.
- Interpretation and symbolic systems should eventually expose keys or structured content, not hardcoded localized prose.
- Application language and interpretation language are separate preferences even when they share the same initial value.
- Theme selection uses stable theme identifiers rather than a boolean dark-mode flag.

Current localization contract direction:

- `LanguageCode`
- `LocalizationKey`
- `LocalizationEntry`
- `LocalizationCatalog`
- `LocalizationResult`
- `ILocalizationProvider`
- `FallbackLocalizationProvider`

Current preferences and theme direction:

- `ApplicationLanguagePreference`
- `InterpretationLanguagePreference`
- `UserPreferences`
- `ThemeId`
- `ThemeDefinition`
- `ThemeRegistry`

MVP fallback chain:

```text
selected language -> ru -> key
```

Current implementation also checks the neutral parent language before `ru` when a regional code such as `en-us` is requested.

Future target fallback chain:

```text
selected language -> en -> ru -> key
```

Intended resource structure:

```text
/resources/localization/ui/ru.json
/resources/localization/ui/en.json
/resources/localization/interpretation/ru.json
/resources/localization/interpretation/en.json
/resources/localization/symbolics/ru.json
```

User preferences should eventually be persisted as JSON in a user app-data location. That persistence is not implemented yet.

## Timezone Strategy

MVP timezone strategy is deliberately conservative.

For MVP:

- Timezone selection may be explicit and manual.
- Reproducibility is more important than automation.
- Birth data flow must preserve both the user-entered local value and the resolved UTC calculation instant.
- The system must not pretend that full timezone history and place lookup are solved when they are not.

`BirthMoment` should preserve:

- Local time.
- Timezone ID.
- UTC instant.
- Ambiguity resolution.
- Source and confidence metadata.

Automatic place-to-timezone lookup is a future enhancement, not an MVP assumption.

## Rendering Contract Direction

Geometry should produce render-independent models. Rendering converts those prepared models into Avalonia drawing operations.

Likely future contracts:

- `ChartGeometryModel`
- `ChartRenderScene`
- `RenderLayer`
- `GlyphPlacement`
- `AspectLineVisual`
- `HouseSectorVisual`
- `HitTestRegion`

Geometry must not return Avalonia controls, brushes, pens, or UI objects.

## Avoid

- God services that calculate, interpret, render, and persist at once.
- Wide catch blocks that hide calculation or persistence failures.
- Static global state for time, location, or ephemeris data.
- UI controls as the primary chart model.
- Database attributes or ORM details inside domain objects unless explicitly justified.
- Premature abstractions that do not protect a real boundary.
