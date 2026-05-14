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

Current status: the repository has moved beyond pure scaffold and now contains the first implementation foundations.

The repository now contains:

- Solution and project structure
- Initial project reference graph
- Domain primitives for zodiac math, birth-time modeling, positions, aspects, and natal-chart snapshots
- Astronomy contracts without Swiss Ephemeris binding
- Render-independent circular chart geometry
- A minimal Avalonia rendering layer for technical chart drawing
- A thin shell, JSON-backed UI localization, in-memory settings, and first real dark/light theme switching
- A first astrology workspace foundation that hosts the chart surface and placeholder side panels
- Development-only sample chart generation used behind that workspace for current pipeline verification
- xUnit test harness and repository-level build configuration

The application is still in foundation mode. Real profile workflows, birth-data input, ephemeris-backed calculations, persistence, Tarot UX, and final visual design are not implemented yet.

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

## Build Instructions

Restore, build, and test commands:

- `dotnet restore NoxAeterna.sln`
- `dotnet build NoxAeterna.sln`
- `dotnet test NoxAeterna.sln`

Minimal desktop shell launch:

- `dotnet run --project NoxAeterna.App`

Current app-localization note:

- UI localization catalogs are loaded from flat JSON files copied to `resources/localization/ui` under the app output.

Current theme note:

- Dark and light themes are applied in memory through `ThemeId` and Avalonia resource dictionaries.
- Theme selection is not persisted yet.

## Repository Structure Overview

- `docs/`: architecture, vision, glossary, boundaries, roadmap, risks, and agent continuity documents
- `NoxAeterna.App/`: Avalonia app host, shell window, astrology workspace host, development-only sample scene wiring, and current theme application boundary
- `NoxAeterna.Presentation/`: shell, astrology workspace models, localization, preferences, settings, and theme metadata foundations
- `NoxAeterna.Rendering/`: technical chart rendering contracts and Avalonia renderer
- `NoxAeterna.Geometry/`: render-independent circular chart layout foundation
- `NoxAeterna.Astronomy/`: time resolution and ephemeris calculation contracts
- `NoxAeterna.Symbolics/`: symbolics layer scaffold
- `NoxAeterna.Interpretation/`: interpretation layer scaffold
- `NoxAeterna.Domain/`: domain layer scaffold
- `NoxAeterna.Infrastructure/`: infrastructure layer scaffold
- `NoxAeterna.Tests/`: xUnit test scaffold
- `Directory.Build.props`: repository-level compiler defaults
- `NoxAeterna.sln`: solution root

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

Current scaffold dependencies include:

- Avalonia
- Avalonia.Desktop
- Avalonia.Themes.Fluent
- NodaTime
- Microsoft.NET.Test.Sdk
- xunit
- xunit.runner.visualstudio

The base `Avalonia` package is currently used by both the minimal app shell and the first rendering-layer contracts.

Additional planned directions, not yet added in code:

- CommunityToolkit.Mvvm
- Dapper
- Serilog
- SwissEphNet or equivalent Swiss Ephemeris wrapper

## AI-Assisted Development Notes

Future AI-assisted sessions must:

- Respect `docs/AGENTS.md` and `docs/ARCHITECTURAL-BOUNDARIES.md`.
- Update `docs/SESSION-LOG.md`, `docs/DECISIONS-LOG.md`, and `docs/NEXT-STEPS.md` when material changes occur.
- Update `README.md` and `docs/THIRD-PARTY.md` whenever external dependencies, assets, fonts, datasets, borrowed code, adapted code, or generated assets are introduced.
- Avoid another large planning pass before scaffold unless a real blocker appears.
