# Session Log

| Metadata | Definition |
| --- | --- |
| Role | Bounded current-wave chronology. |
| Read when | Recent provenance is explicitly needed. |
| Authoritative for | Recent session evidence and handoff chronology. |
| Not authoritative for | Current status, architecture, roadmap, or durable policy. |

Older retained evidence is indexed in [the documentation archive](archive/README.md). Use [PROJECT-STATE.md](PROJECT-STATE.md) for the current checkpoint.

## 2026-07-31: T1-B.2 Same-Day Session-Log Rollover

Summary:

- Extended the read-only documentation checker to recognize bounded partial-day session-log chunks while preserving the existing full-range archive contract.
- Added contiguous part numbering, exact normalized heading ownership, partial/full overlap, heading-date, archive-index, and typed JSON diagnostic validation.
- Moved the completed visual V1/V2 and repository T1-A/T1-B wave, including first hosted CI confirmation, intact into `SESSION-LOG_2026-07-31_part-01.md`.
- Kept the active log available for additional entries on the same calendar date without raising its hard budget.

Verification:

- `pwsh eng/doc-check.ps1`
- `pwsh eng/doc-check.ps1 -Json`
- `dotnet build NoxAeterna.sln -c Debug`
- `pwsh eng/test-route.ps1 run Repository-Verification -NoBuild`

## 2026-07-31: T2-A Project Stats and Factual Repository Analysis

Summary:

- Added one standalone BCL-only `NoxAeterna.Tools.Repository` executable with reusable Git-visible public-file inventory, deterministic text metadata, path classification, project XML/reference analysis, lexical test topology, and read-only documentation-budget snapshots.
- Added bounded console, schema-1 camelCase JSON, and Markdown output with repository-relative paths, safe output-target exclusion, controlled diagnostics, and no volatile timestamp.
- Enforced privacy-before-read and generated/runtime exclusions without filesystem fallback, network access, AppData access, semantic dependency inference, or quality verdicts.
- Added the non-overlapping `Project-Stats` leaf and placed it last in `Repository-Verification`; CI topology and product projects remain unchanged.
- Established `PROJECT-STATS.md` as report-semantics owner while reserving context routes, rankings, and character budgets for T2-B.

Verification:

- `pwsh eng/doc-check.ps1`
- `dotnet build NoxAeterna.sln -c Debug`
- `pwsh eng/test-route.ps1 run Project-Stats -NoBuild`
- `pwsh eng/test-route.ps1 run Repository-Verification -NoBuild`
- console, JSON, and Markdown CLI smoke against the current repository
- `pwsh eng/test-route.ps1 run Full -NoBuild -AllowMilestone`

## 2026-07-31: T2-A.1 Cross-Platform Project Reference Repair

Summary:

- Inspected hosted workflow run `30624650769`: Ubuntu and macOS milestone jobs plus Ubuntu coverage failed only in `CurrentRepositoryReportDiscoversExpectedProjectsWithoutUnsafeRankings`, where all 25 project references retained host-dependent parent/separator forms.
- Replaced host-filesystem combination of raw MSBuild `ProjectReference` values with a lexical resolver that accepts `/`, `\`, and mixed separators, collapses dot and parent segments, and emits canonical repository-relative `/` paths.
- Added controlled diagnostics for repository escape, absolute references, and unsupported MSBuild expressions without exposing machine paths or attempting full MSBuild evaluation.
- Added pure host-independent regression cases plus a current-repository edge assertion; coverage collection and CI topology remain unchanged.

Verification:

- `dotnet build NoxAeterna.sln -c Debug`
- `pwsh eng/test-route.ps1 run Project-Stats -NoBuild`
- `pwsh eng/test-route.ps1 run Repository-Verification -NoBuild`
- `pwsh eng/test-route.ps1 run Full -NoBuild -AllowMilestone`
- `pwsh eng/doc-check.ps1`

Hosted confirmation remains pending after commit and push.

## 2026-07-31: T2-B RAG-Lite Context Routing

Summary:

- Confirmed the T2-A.1 checkpoint passed all five hosted jobs, then extended the existing BCL repository tool with deterministic task/path context planning instead of adding a scanner, service, embeddings, or network dependency.
- Added exact additive route and retrieval-evaluation registries, project-owned cross-platform glob matching, mandatory/recommended character and file bounds, planned/directory/binary target semantics, privacy refusal, and named-test-route joins.
- Added read-only PowerShell wrappers, compact/indented JSON and console plans without contents or machine paths, and 15 retrieval cases covering engines, UI, assets, documentation, tooling, persistence planning, and multi-target work.
- Added the non-overlapping `Agent-Context` leaf to `Repository-Verification`, basic doc-check integration, focused negative/boundary coverage, and canonical context-routing documentation.

Verification:

- `pwsh eng/doc-check.ps1`
- `dotnet build NoxAeterna.sln -c Debug`
- `pwsh eng/context-plan.ps1 ...`
- `pwsh eng/context-eval.ps1`
- `pwsh eng/test-route.ps1 run Agent-Context -NoBuild`
- `pwsh eng/test-route.ps1 run Repository-Verification -NoBuild`
- `pwsh eng/test-route.ps1 run Full -NoBuild -AllowMilestone`

Hosted checkpoint `28c895c366d9a8e451b8ed5c34c998322e3986fc` passed Documentation, Windows, Ubuntu, macOS, and diagnostic coverage jobs.

## 2026-07-31: P1 Planet Semantic Anchoring

Summary:

- Confirmed the T2-B checkpoint passed all five hosted jobs, completing the repository-foundation wave before product rendering work resumed.
- Removed geometry-owned angular spreading and replaced the misleading display-angle contract with exact source angles, deterministic clusters, preferred radial lanes, and source-house membership.
- Split render-owned planet visuals into exact source markers, radial-first glyph placement with an eight-degree sign/house-safe ceiling, and independently measured labels with optional quieter association leaders.
- Removed Cartesian glyph fallback, retained source-based aspect endpoints, added controlled crowding diagnostics, and clarified house boundaries with smaller Roman numerals plus exact-angle number-lane cusp notches.
- Added synthetic Capricorn, sign-boundary, house-cusp, zero-wrap, UnknownTime, and extreme-crowding coverage together with source/leader/label/viewport contracts.

Verification:

- `pwsh eng/doc-check.ps1` and all 15 context evaluations
- Geometry, Rendering, Chart-Rendering, Desktop-UI, and Repository-Verification named routes
- Full milestone: 510 passed, 0 failed, 0 skipped
- real-control Fryazino and Prague dark/light, UnknownTime, compact/maximized, transition, and live-resize smoke with four untracked temporary screenshots

## 2026-07-31: S1 Adaptive Collapsible Shell Navigation Rail

Summary:

- Confirmed accepted P1 checkpoint `6d7b02444bd44ad5cb1846e871d7170871a9d3e1` in hosted run `30633659249`: Documentation, Windows, Ubuntu, macOS, and diagnostic coverage all succeeded.
- Replaced the fixed 240-DIP shell column with a native compact-inline `SplitView`, centralized expanded/compact dimensions and threshold, and added an in-memory wide-mode preference restored after forced compact layout.
- Added original project-owned normalized vector icons for Astrology, Tarot, Archive, Settings, and the toggle, with localized tooltips, accessible names, keyboard operation, and unchanged section ordering/selection.
- Preserved workspace instances during toggles, left chart rendering and astrology untouched, and extended semantic navigation styles without new colors, fonts, dependencies, or assets.

Verification:

- `pwsh eng/doc-check.ps1` and all 15 context evaluations
- `dotnet build NoxAeterna.sln -c Debug`
- Presentation, Localization, App-Workspace, Theme-Resources, Desktop-UI, and Repository-Verification named routes
- Full milestone: 520 passed, 0 failed, 0 skipped
- real-control pointer/keyboard toggle, all-section selection, responsive restore, theme switch, RU/EN, compact/maximized smoke
- four untracked temporary S1 screenshots

## 2026-08-03: S1 Hosted Confirmation

Summary:

- The owner visually accepted S1 adaptive shell navigation at commit `705fef517834e609b485faca511e240e6dec0a8a`.
- Hosted CI run `30636973316` completed successfully for that push to `master`.
- Exact jobs: Documentation contracts — success; Build and test (windows-latest) — success; Build and test (ubuntu-latest) — success; Build and test (macos-latest) — success; Diagnostic coverage — success.

## 2026-08-03: T0-A Tarot Semantic Foundation

Summary:

- Added validated stable deck/card/spread/position identities and immutable, language-neutral Domain definitions for Arcana, suits, ranks, assignments, readings, and controlled draw failure.
- Added the deterministic built-in `standard-78` semantic catalog, single-card and ordered past/present/future spreads, and injected without-replacement drawing with explicit orientation policy and caller timestamp.
- Kept semantic decks separate from future artwork packs, presentation skins, and back variants; added no UI, rendering, assets, persistence, interpretation prose, external package, or dependency-shape change.
- Added a non-overlapping Tarot named route plus focused structural, draw, replay, immutability, validation, and boundary coverage.

Verification:

- `pwsh eng/doc-check.ps1`: 0 warnings, 0 errors
- `dotnet build NoxAeterna.sln -c Debug`: 0 warnings, 0 errors
- Domain route: 67 passed; Tarot route: 23 passed; Repository-Verification: all four leaves succeeded
- Full milestone: 546 passed, 0 failed, 0 skipped
- UI smoke not applicable because T0-A changes neither UI nor Rendering; no screenshots created

## 2026-08-03: T1 First Playable Tarot Vertical Slice

Summary:

- Confirmed accepted T0-A checkpoint `e8cb628d5dbbfb2a5fdc16b87fc4c3247bfb861f` in hosted run `30788655353`: Documentation contracts, Windows, Ubuntu, macOS, and Diagnostic coverage all succeeded.
- Replaced the Tarot placeholder with a persistent in-memory workspace driven by `StandardTarotCatalog`, both built-in spreads, the existing `TarotDrawEngine`, explicit reversal policy, and a composition-root runtime RNG adapter.
- Added independent typed artwork-pack, presentation-skin, back-variant, and interpretation-set selections; exposed only one honest prototype art/skin foundation and the two real programmatic Black Sun/Lunar Seal backs.
- Added responsive 7:12 symbolic faces, localized RU/EN card structure and inspector, native pointer/Enter/Space selection, selected/hover/focus states, explicit 180-degree reversal, and tableau-owned compact overflow without final artwork or interpretation prose.
- Added focused presentation, layout, runtime-adapter, localization, visual-contract, and architecture tests. Mutation review found and repaired self-referential ratio/minimum assertions; no mutation remains applied.

Verification:

- `pwsh eng/doc-check.ps1`: 0 warnings, 0 errors
- `dotnet build NoxAeterna.sln -c Debug`: 0 warnings, 0 errors
- Tarot: 37 passed; Presentation, Rendering, Desktop-UI, and all Repository-Verification leaves succeeded
- Full milestone: 570 passed, 0 failed, 0 skipped
- real-control Obsidian/Porcelain, RU/EN, single/three-card, upright/reversed, both backs, every card position, pointer/Enter/Space, expanded/compact navigation, maximized/live-resize, Astrology return, and Tarot reopen smoke
- four untracked temporary T1 screenshots under `%TEMP%\NoxAeterna-T1`
