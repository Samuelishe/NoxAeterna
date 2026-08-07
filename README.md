# Nox Aeterna

Nox Aeterna is a planned cross-platform desktop application for symbolic systems, focused on astrology, Tarot, personal birth profiles, transits, lunar events, and long-term symbolic history.

It is intended to feel like a digital esoteric cabinet, a personal occult archive, and an astrological observatory rather than a casual horoscope or novelty app.

Project intent: Nox Aeterna is being developed as a fully open-source project. Commercial distribution is not part of the current plan.

## Product Philosophy

- Serious, calm, atmospheric, and non-ironic.
- Structured symbolic systems over shallow engagement mechanics.
- Reading-first desktop experience over mobile-style novelty loops.
- Deterministic rendering and explicit architecture over UI magic.
- Explainable interpretation over prose-first AI output.

See `docs/DEVELOPMENT-PHILOSOPHY.md` for the durable decision style behind the project.

## Current Status

The current checkpoint and one active focus are maintained in [`docs/PROJECT-STATE.md`](docs/PROJECT-STATE.md). Operational repository rules begin in [`AGENTS.md`](AGENTS.md).

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
- SQLite via Microsoft.Data.Sqlite for immutable compiled interpretation packages; mutable user persistence remains planned
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

## Repository Verification

Repository tooling requires PowerShell 7. Start with the read-only checks:

- `pwsh eng/repo-baseline.ps1`
- `pwsh eng/doc-check.ps1`

Run named verification or diagnostic coverage:

- `pwsh eng/test-route.ps1 list`
- `pwsh eng/test-route.ps1 run Repository-Verification`
- `pwsh eng/test-route.ps1 run Full -AllowMilestone`
- `pwsh eng/coverage.ps1`
- `dotnet run --project NoxAeterna.Tools.Repository -- stats .`

JSON output is available for tooling commands. See [`docs/TEST-EXECUTION.md`](docs/TEST-EXECUTION.md), [`docs/UI-SMOKE.md`](docs/UI-SMOKE.md), and [`eng/README.md`](eng/README.md). Manual real-control smoke remains separate from automated tests and CI.

Current app-localization note:

- UI localization catalogs are loaded from flat JSON files copied to `resources/localization/ui` under the app output.
- The product name `Nox Aeterna` remains unchanged in every language catalog and visible app title.
- The app now applies the selected application language to culture-sensitive date and time controls.

Current theme note:

- Dark and light themes are applied in memory through `ThemeId` and Avalonia resource dictionaries.
- Theme selection is not persisted yet.

Current birth-input note:

- The astrology workspace now supports an offline-first birth-data input mode.
- Date selection uses a picker, timezone selection comes from local TZDB IDs, and coordinates remain manual.
- Valid input now rebuilds the visible chart through `IBirthMomentResolver`, `SwissEphNet`, natal-chart composition, geometry layout, and the isolated renderer.
- The visible chart is now readable enough to inspect: zodiac ring glyphs, planet glyphs, and a compact positions list are present.
- Startup now uses a deterministic real-chart sample routed through the live SwissEphNet-backed pipeline rather than old integer-only placeholder positions.
- The right birth-data panel now scrolls instead of clipping at the default window size.
- The current live calculation uses SwissEphNet in built-in Moshier mode because external `.se1` ephemeris files are not configured yet.

Current asset and runtime-data note:

- shipped application assets belong in the repository and must stay attributed and reviewable;
- owner-accepted Lupus Noctis production illustrations were created across earlier built-in OpenAI/Codex-assisted waves and later owner-with-ChatGPT handoff waves outside Codex, then manually accepted by the owner; the exact complete 78-card inventory belongs to `artwork-pack.json`, while provenance details and unresolved release-rights verification belong to `docs/THIRD-PARTY.md`, `LUPUS-NOCTIS.md`, and its linked records;
- user-specific runtime data belongs in AppData or the equivalent platform-specific user data directory;
- random unlicensed internet images and raw AI-generation dumps must not be committed.

## Repository Structure Overview

- `docs/`: architecture, vision, glossary, boundaries, roadmap, risks, and agent continuity documents
- `NoxAeterna.App/`: Avalonia app host, shell window, astrology workspace host, demo chart-rebuild composition, fallback sample scene wiring, and current theme application boundary
- `NoxAeterna.Presentation/`: shell, astrology workspace models, localization, preferences, settings, and theme metadata foundations
- `NoxAeterna.Rendering/`: technical chart rendering contracts and Avalonia renderer
- `NoxAeterna.Geometry/`: render-independent circular chart layout foundation
- `NoxAeterna.Astronomy/`: time resolution and ephemeris calculation contracts
- `NoxAeterna.Symbolics/`: symbolics layer scaffold
- `NoxAeterna.Interpretation/`: interpretation layer scaffold
- `NoxAeterna.Domain/`: domain layer scaffold
- `NoxAeterna.Infrastructure/`: infrastructure layer scaffold
- `NoxAeterna.Tools.Repository/`: BCL-only factual repository inventory and Project Stats CLI
- `NoxAeterna.Tests/`: xUnit test scaffold
- `Directory.Build.props`: repository-level compiler defaults
- `NoxAeterna.sln`: solution root

## Documentation Navigation

Start with:

- `AGENTS.md`
- `docs/PROJECT-STATE.md`
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
- No repository-local runtime storage for user data, caches, or preferences.
- The product name `Nox Aeterna` and other intended Latin proper names stay untranslated across all localizations.

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
- SwissEphNet
- Microsoft.NET.Test.Sdk
- coverlet.collector
- xunit
- xunit.runner.visualstudio

The base `Avalonia` package is currently used by both the minimal app shell and the first rendering-layer contracts.

Additional planned directions, not yet added in code:

- CommunityToolkit.Mvvm
- Dapper
- Serilog

Swiss Ephemeris note:

- The current spike uses `SwissEphNet` as a managed .NET port of Swiss Ephemeris.
- The live app currently runs through the wrapper's built-in Moshier fallback because external Swiss ephemeris `.se1` data files are not configured yet.
- External ephemeris data files must not be vendored silently. Their source, authorship, license terms, and installation strategy must be documented before bundling.
- The upstream Swiss Ephemeris licensing position must be tracked explicitly. The wrapper package embeds an older dual-license notice, while current Astrodienst documentation describes AGPL or a professional license.
- All ephemeris integration remains hidden behind `IEphemerisCalculator`.

## AI-Assisted Development Notes

Future AI-assisted sessions must:

- Respect root `AGENTS.md`, `docs/PROJECT-STATE.md`, and `docs/ARCHITECTURAL-BOUNDARIES.md`.
- Follow `docs/DOCUMENTATION-GOVERNANCE.md` and update only the owners affected by material changes.
- Update `README.md` and `docs/THIRD-PARTY.md` whenever external dependencies, assets, fonts, datasets, borrowed code, adapted code, or generated assets are introduced.
- Avoid another large planning pass before scaffold unless a real blocker appears.
