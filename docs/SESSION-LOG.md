# Session Log

| Metadata | Definition |
| --- | --- |
| Role | Bounded current-wave chronology. |
| Read when | Recent provenance is explicitly needed. |
| Authoritative for | Recent session evidence and handoff chronology. |
| Not authoritative for | Current status, architecture, roadmap, or durable policy. |

Older retained evidence is indexed in [the documentation archive](archive/README.md). Use [PROJECT-STATE.md](PROJECT-STATE.md) for the current checkpoint.

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
- Checkpoint commit `4e7e3d61bc11b875af9fb5591f5a10ed986c8962` passed hosted run `30791606505`: Documentation contracts, Windows, Ubuntu, macOS, and Diagnostic coverage all succeeded; owner visual acceptance remains pending.

## 2026-08-03: A2 Lupus Noctis Meaning-First Tonal-Range Study

Summary:

- Recorded the owner's complete rejection of A0 and A1; none of the nine earlier studies remains accepted, an anchor, a composition reference, or a production asset, and accepted Lupus Noctis production cards remain 0.
- Established `resources/assets/tarot/artwork-packs/lupus-noctis/LUPUS-NOCTIS.md` as the canonical human-readable owner for the working identity, varied fantasy world, four wolf-motif modes, contemporary narrative-fantasy direction, meaning-first workflow, emotional range, typography boundary, rejection record, research sources, briefs, prompts, and candidate records.
- Researched historical Death, Six of Cups, and Star meanings from public-domain Waite and Mathers texts, then generated exactly three independent review candidates spanning dark/transitional, warm/nostalgic, and luminous/hopeful tones without image references.
- Stored normalized `952 × 1632 px` (`7:12`) candidates at `studies/A2/death.png`, `six-of-cups.png`, and `star.png`; all remain owner-acceptance Pending and no contact sheet, manifest, production path, font, or runtime integration was added.
- Death and Six of Cups required one generation each. Star required one targeted second generation to remove a random tattoo-like pseudo-symbol from the exposed shoulder.

## 2026-08-03: A3 Lupus Noctis Partial-Pack Integration

Summary:

- Recorded the owner's acceptance of all three A2 candidates while keeping A0 and A1 rejected, then promoted Death, Six of Cups, and the Star into stable semantic production paths without retaining study copies or contact sheets.
- Added a versioned `lupus-noctis` partial-pack manifest with exact `standard-78` identities, package-relative paths, `952 × 1632` dimensions, SHA-256 checksums, accepted status, and owner-document provenance references.
- Added a read-only built-in loader/catalog that rejects traversal, duplicates, unknown identities, malformed dimensions, and hash mismatches; optional omissions resolve to the honest localized prototype face rather than changing the reading.
- Added a second artwork-pack selector after Classic while keeping raster artwork, programmatic frame/title overlay, selection, and reversal under one visual transform contract.
- Preserved the A2 meaning research and prompts, recorded independent casting policy and generated-asset provenance, and left the next artistic batch unselected pending owner visual review.

Verification:

- documentation check completed with one expected session-log soft-budget warning and no errors; Debug solution build completed with 0 warnings and 0 errors;
- Tarot, Desktop-UI, and Repository-Verification named routes succeeded; Full milestone passed 605 tests with 0 failed and 0 skipped;
- real-control smoke covered both artwork packs without reading changes, all three raster cards, prototype fallback, RU/EN, Obsidian/Porcelain, upright/reversed, single/three-card, both backs, inspector, keyboard activation, navigation collapse/restore, maximized/compact live resize, and Astrology return;
- four review screenshots remain untracked under the local temporary A3 smoke directory.

## 2026-08-03: AP0 Asset-Pack Runtime Architecture

Summary:

- Audited the accepted A3 manifest, built-in loader/catalog, partial fallback, selection state, raster composition, resource packaging, and focused tests without changing runtime code or images.
- Established `ASSET-PACK-RUNTIME.md` as the owner for repository seed to AppData synchronization, exact-stem discovery, separate fingerprint state, normalization cache, user import, Assets tooling, and publish/installer handoff.
- Kept ART-LN independent from AP1–AP5 and PKG1, and recorded T-UX1, S2, and BRAND1 as separate later stages.

## 2026-08-03: A4 / A4-P Lupus Noctis Contrast Batch

- Generated text-only Sun (2 calls), Five of Swords (1), and Moon (2) without image references; prompts, corrections, hashes, and reviews remain in `LUPUS-NOCTIS.md`.
- The owner accepted all three; A4-P moved them from `studies/A4/` to canonical production paths, removed study copies, and expanded the partial manifest from 3 to 6 cards.
- Updated focused pack tests without runtime, UI, or rendering changes; docs, Debug build, Tarot, Desktop-UI, Repository-Verification, and six-asset checks passed; no UI smoke was required.
