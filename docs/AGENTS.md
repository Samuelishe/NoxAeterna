# Nox Aeterna Agent Guide

| Metadata | Definition |
| --- | --- |
| Role | Extended product and agent guide for Nox Aeterna. |
| Read when | Product identity, tone, detailed domain navigation, or attribution policy is relevant. |
| Authoritative for | Product identity, product tone, detailed domain navigation, attribution requirements, and project-specific non-goals. |
| Not authoritative for | Git operations, current project status, documentation ownership, executable test routes, or exact documentation budgets. |

Operational repository rules belong to the root [`AGENTS.md`](../AGENTS.md). The current checkpoint belongs to [`PROJECT-STATE.md`](PROJECT-STATE.md), and documentation ownership belongs to [`DOCUMENTATION-GOVERNANCE.md`](DOCUMENTATION-GOVERNANCE.md).

## Project Identity

Nox Aeterna is a serious cross-platform desktop application for symbolic systems, built with C# / .NET 10 and Avalonia UI.

The product should feel like a digital esoteric cabinet, a personal occult archive, an astrological atlas, and a symbolic observatory. It presents structured symbolic systems, historical correspondences, astronomical data, interpretive layers, and personal archives. It does not try to prove, convince, parody, or imitate shallow horoscope products.

Tone: serious, calm, atmospheric, intelligent, restrained, premium, non-ironic.

Avoid: joke generation, fake AI magic, meme occult aesthetics, TikTok witchcore, cheap glow effects, cartoon magic, shallow daily horoscope features, and invented systems unless explicitly documented as fictional or experimental.

## Product and Domain Navigation

Use [`INDEX.md`](INDEX.md) for discovery and open only the documents relevant to the task.

Then read task-specific files:

- Decision style and product philosophy: [`DEVELOPMENT-PHILOSOPHY.md`](DEVELOPMENT-PHILOSOPHY.md)
- Canonical terminology: [`GLOSSARY.md`](GLOSSARY.md)
- Hard layer rules: [`ARCHITECTURAL-BOUNDARIES.md`](ARCHITECTURAL-BOUNDARIES.md)
- Astrology or time work: [`ASTRONOMY-ENGINE.md`](ASTRONOMY-ENGINE.md)
- Chart layout: [`GEOMETRY-ENGINE.md`](GEOMETRY-ENGINE.md)
- Rendering: [`RENDERING-ENGINE.md`](RENDERING-ENGINE.md)
- Interpretation: [`INTERPRETATION-ENGINE.md`](INTERPRETATION-ENGINE.md)
- Symbolic knowledge: [`SYMBOLICS.md`](SYMBOLICS.md)
- Tarot: [`TAROT-ENGINE.md`](TAROT-ENGINE.md)
- Storage: [`PERSISTENCE.md`](PERSISTENCE.md)
- UI product direction: [`UI-VISION.md`](UI-VISION.md)
- Exact visual semantics: [`VISUAL-DESIGN-SYSTEM.md`](VISUAL-DESIGN-SYSTEM.md)
- Avalonia theme implementation: [`THEMES.md`](THEMES.md)
- Code style: [`CODING-GUIDELINES.md`](CODING-GUIDELINES.md)

## Intended Technology Direction

- C#
- .NET 10
- Avalonia UI
- MVVM
- CommunityToolkit.Mvvm
- NodaTime
- SwissEphNet or equivalent Swiss Ephemeris wrapper
- SQLite
- Dapper unless a stronger alternative is justified
- Serilog
- xUnit
- Skia/Avalonia rendering through proper rendering abstractions

Verify package availability before adding dependencies. Some package choices, especially Swiss Ephemeris wrappers for .NET 10, are open risks.

## Expected Solution Structure

Starting assumption, subject to revision:

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

Keep project references aligned with the documented boundaries.

## Architectural Boundaries

Strictly separate:

- Domain models
- Astronomy engine
- Geometry engine
- Symbolics layer
- Interpretation engine
- Rendering engine
- Presentation/UI
- Persistence layer
- Infrastructure

Rules:

- UI must not know astronomical calculation details.
- Astronomy must not depend on Avalonia.
- Geometry must not depend on Avalonia UI controls.
- Interpretation must not depend on UI.
- Rendering receives prepared geometry/rendering models, not raw business logic.
- Swiss Ephemeris must be hidden behind an interface, likely `IEphemerisCalculator`.
- Time handling must go through NodaTime.
- Ecliptic longitudes are normalized to 0-360 degrees.
- Avoid god objects, wide catch blocks, and premature abstractions.

See `docs/ARCHITECTURAL-BOUNDARIES.md` for non-negotiable boundary rules.

## Coding Rules

Use readable naming, XML docs for public APIs, explicit value objects where useful, and tests for core math, time handling, and domain logic.

Prefer small, reviewable changes. Keep implementation details honest: do not document features as implemented until code and tests exist.

All user-facing text must use localization keys. Do not introduce raw UI strings into application code unless they are strictly test-only, diagnostic, or exception text.

See `docs/CODING-GUIDELINES.md` for detailed rules.

## Attribution Rule

Every future session must explicitly document any introduced:

- Third-party libraries.
- Frameworks.
- Assets.
- Fonts.
- Rendering systems.
- Datasets.
- Ephemeris sources.
- Tools.
- Borrowed or adapted code.
- Generated assets, when relevant.

Track authorship, license, purpose, and official source in `README.md` and `docs/THIRD-PARTY.md`. If a session adds external material and does not update attribution tracking, the task is incomplete.

## What Not To Do

- Do not create fake implementation details.
- Do not invent astronomical, astrological, or symbolic systems without documenting them as fictional or experimental.
- Do not couple Avalonia UI to astronomy, geometry, interpretation, or persistence internals.
- Do not use LLM-generated prose as the source of symbolic logic.
- Do not make the app feel like a joke, meme, shallow horoscope app, or AI mysticism assistant.
- Do not rely on generated images for exact charts, glyphs, small icons, text inside images, UI layout, or technical rendering.
- Do not expand scope beyond the current verified task without explicit authorization.
