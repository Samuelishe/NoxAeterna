# Nox Aeterna

Nox Aeterna is a planned cross-platform desktop application for symbolic systems, focused on astrology, Tarot, personal birth profiles, transits, lunar events, and long-term symbolic history.

It is intended to feel like a digital esoteric cabinet, a personal occult archive, and an astrological observatory rather than a casual horoscope or novelty app.

## Product Philosophy

- Serious, calm, atmospheric, and non-ironic.
- Structured symbolic systems over shallow engagement mechanics.
- Reading-first desktop experience over mobile-style novelty loops.
- Deterministic rendering and explicit architecture over UI magic.
- Explainable interpretation over prose-first AI output.

See `docs/DEVELOPMENT-PHILOSOPHY.md` for the durable decision style behind the project.

## Current Status

Current status: documentation and architectural direction are locked for implementation startup.

The repository is ready for:

- .NET 10 solution scaffold
- Project creation
- Dependency graph setup
- Test infrastructure
- Domain primitives
- Astronomy abstractions
- Rendering contracts

Application behavior is not implemented yet.

## Planned Architecture

Planned solution structure:

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

Architecture intent:

- Astronomy, geometry, rendering, symbolics, interpretation, and persistence remain separate.
- Geometry produces render-independent models.
- Rendering converts prepared models into Avalonia drawing operations.
- Symbolics stores structured symbolic knowledge with typed relationships.
- Interpretation is structured-first and only optionally gains a narrative layer later.

See `docs/ARCHITECTURE.md` and `docs/ARCHITECTURAL-BOUNDARIES.md`.

## Technology Stack

Planned technology direction:

- C#
- .NET 10
- Avalonia UI
- MVVM
- CommunityToolkit.Mvvm
- NodaTime
- SwissEphNet or equivalent Swiss Ephemeris wrapper
- SQLite
- Dapper or a carefully justified alternative
- Serilog
- xUnit

## Planned Modules

- Astrology
- Tarot
- Personal Profiles
- Current Transits and Lunar Events
- Personal symbolic archive and history

## Build Requirements

Planned requirements for implementation startup:

- .NET 10 SDK
- A supported IDE or editor for C# development
- Platform support for Avalonia development
- Future native dependency support for astronomy packages, pending package choice

## Future Build Instructions

Build instructions will be added once the solution scaffold exists.

Expected future sections:

- Restore dependencies
- Build solution
- Run tests
- Launch desktop app

## Repository Structure Overview

- `docs/`: architecture, vision, glossary, boundaries, roadmap, risks, and agent continuity documents
- `NoxAeterna.sln`: placeholder solution file, to be replaced or expanded during scaffold work

## Documentation Navigation

Start with:

- `docs/AGENTS.md`
- `docs/INDEX.md`
- `docs/ARCHITECTURE.md`
- `docs/DEVELOPMENT-PHILOSOPHY.md`
- `docs/GLOSSARY.md`

Task-specific documents cover astronomy, geometry, rendering, interpretation, symbolics, Tarot, persistence, and UI direction.

## Development Principles

- Clarity over abstraction.
- Maintainability and explainability over cleverness.
- Atmosphere over feature count.
- Desktop-first and reading-first product design.
- No hidden architectural coupling.
- No fake implementation claims in documentation or code.

## Disclaimer

Nox Aeterna is intended for symbolic, cultural, historical, and interpretive use. It does not present astrology, Tarot, or related symbolic systems as scientific proof or empirical claims.

## Planned Open-Source Dependencies and Acknowledgements

External libraries, frameworks, assets, fonts, datasets, ephemeris sources, tools, borrowed code, adapted code, and generated assets must be tracked with authorship and license information.

See `docs/THIRD-PARTY.md`.

Current planned ecosystem includes:

- .NET
- Avalonia
- CommunityToolkit.Mvvm
- NodaTime
- Dapper
- Serilog
- xUnit
- SwissEphNet or equivalent Swiss Ephemeris wrapper

These are planned directions, not yet confirmed implementation dependencies.

## AI-Assisted Development Notes

Future AI-assisted sessions must:

- Respect `docs/AGENTS.md` and `docs/ARCHITECTURAL-BOUNDARIES.md`.
- Update `docs/SESSION-LOG.md`, `docs/DECISIONS-LOG.md`, and `docs/NEXT-STEPS.md` when material changes occur.
- Update `README.md` and `docs/THIRD-PARTY.md` whenever external dependencies, assets, fonts, datasets, borrowed code, adapted code, or generated assets are introduced.
- Avoid another large planning pass before scaffold unless a real blocker appears.
