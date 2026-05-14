# Nox Aeterna Agent Guide

This is the primary operational guide for Codex and future autonomous coding agents working on Nox Aeterna.

## Project Identity

Nox Aeterna is a serious cross-platform desktop application for symbolic systems, built with C# / .NET 10 and Avalonia UI.

The product should feel like a digital esoteric cabinet, a personal occult archive, an astrological atlas, and a symbolic observatory. It presents structured symbolic systems, historical correspondences, astronomical data, interpretive layers, and personal archives. It does not try to prove, convince, parody, or imitate shallow horoscope products.

Tone: serious, calm, atmospheric, intelligent, restrained, premium, non-ironic.

Avoid: joke generation, fake AI magic, meme occult aesthetics, TikTok witchcore, cheap glow effects, cartoon magic, shallow daily horoscope features, and invented systems unless explicitly documented as fictional or experimental.

## Current Phase

Current phase: Stage 1 scaffold initialized.

The architecture is documented but not implemented. Do not claim that projects, engines, renderers, persistence, or UI flows exist until they are created in code.

## Start Here

Read these first:

1. `docs/INDEX.md` for efficient navigation.
2. `docs/PROJECT-VISION.md` for product identity and non-goals.
3. `docs/ARCHITECTURE.md` for layer boundaries and expected solution structure.
4. `docs/NEXT-STEPS.md` for the immediate work queue.
5. `docs/SESSION-LOG.md` for continuity from previous agent sessions.

Then read task-specific files:

- Decision style and product philosophy: `docs/DEVELOPMENT-PHILOSOPHY.md`
- Canonical terminology: `docs/GLOSSARY.md`
- Hard layer rules: `docs/ARCHITECTURAL-BOUNDARIES.md`
- Astrology or time work: `docs/ASTRONOMY-ENGINE.md`
- Chart layout: `docs/GEOMETRY-ENGINE.md`
- Rendering: `docs/RENDERING-ENGINE.md`
- Interpretation: `docs/INTERPRETATION-ENGINE.md`
- Symbolic knowledge: `docs/SYMBOLICS.md`
- Tarot: `docs/TAROT-ENGINE.md`
- Storage: `docs/PERSISTENCE.md`
- UI: `docs/UI-VISION.md`
- Code style: `docs/CODING-GUIDELINES.md`

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

Do not add application code during documentation-only tasks. When scaffolding starts, keep project references aligned with the documented boundaries.

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

## Documentation Rules

Update docs when a decision, boundary, module responsibility, or known risk changes.

Use:

- `docs/DECISIONS-LOG.md` for durable architecture and product decisions.
- `docs/SESSION-LOG.md` for what happened in each agent session.
- `docs/TECHNICAL-DEBT.md` for known shortcuts and deferred cleanup.
- `docs/KNOWN-PROBLEMS.md` for risks, unknowns, and external dependencies.
- `docs/NEXT-STEPS.md` for the next actionable queue.
- `docs/THIRD-PARTY.md` for provenance and license tracking.

Keep docs practical and agent-readable. Prefer concrete constraints, ownership, and next actions over aspirational prose.

## Final Response Format

Every final response to the project owner must be in Russian and include:

1. A short summary in Russian of what was created or changed.
2. An English commit message description, 1-2 sentences, ready to paste.
3. A proposed next step in Russian.

## What Not To Do

- Do not create fake implementation details.
- Do not invent astronomical, astrological, or symbolic systems without documenting them as fictional or experimental.
- Do not couple Avalonia UI to astronomy, geometry, interpretation, or persistence internals.
- Do not use LLM-generated prose as the source of symbolic logic.
- Do not make the app feel like a joke, meme, shallow horoscope app, or AI mysticism assistant.
- Do not rely on generated images for exact charts, glyphs, small icons, text inside images, UI layout, or technical rendering.
- Do not start another large planning pass before scaffold unless a hard blocker appears.

## Preserving Continuity

At the end of each meaningful session:

1. Update `docs/SESSION-LOG.md`.
2. Update `docs/NEXT-STEPS.md` if priorities changed.
3. Add decisions to `docs/DECISIONS-LOG.md`.
4. Add unresolved risks to `docs/KNOWN-PROBLEMS.md`.
5. Add shortcuts to `docs/TECHNICAL-DEBT.md`.
6. Update `docs/THIRD-PARTY.md` and `README.md` if external dependencies, assets, or borrowed material were introduced.

If uncertain, choose a safe minimal direction, document the uncertainty, and continue.
