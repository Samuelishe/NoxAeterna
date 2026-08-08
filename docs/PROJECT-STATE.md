# Project State

| Metadata | Definition |
| --- | --- |
| Role | Current repository handoff. |
| Read when | At the beginning of every nontrivial task. |
| Authoritative for | Current checkpoint, one current focus, preserved contracts, and active blockers. |
| Not authoritative for | History, the future stage catalog, code style, or exact test routes. |

## Current Checkpoint

- V1 chart visual design is accepted.
- V2 semantic shell and theme migration is accepted.
- T1-A repository documentation foundation is complete at checkpoint commit `77538a6157e269cde20b26ae3fa72764231a3858`.
- T1-B implementation is complete, and hosted run `30619151450` passed documentation validation, Windows/Linux/macOS milestone tests, and diagnostic coverage with four expected artifacts.
- The same-day session-log rollover is complete; finished July 31 evidence is retained in indexed partial-day archive part 01.
- T2-A Project Stats and the T2-A.1 host-independent MSBuild path repair are complete. Hosted checkpoint commit `9791d672c87c06c71e365d5433b4f847d6917046` passed Documentation, Windows, Ubuntu, macOS, and diagnostic coverage jobs; the Unix parsing blocker is removed.
- T2-B deterministic RAG-lite context routing is complete. Hosted checkpoint commit `28c895c366d9a8e451b8ed5c34c998322e3986fc` passed Documentation, Windows, Ubuntu, macOS, and diagnostic coverage jobs; the repository-foundation wave is complete.
- P1 planet semantic anchoring is accepted at checkpoint commit `6d7b02444bd44ad5cb1846e871d7170871a9d3e1`. Hosted run `30633659249` passed Documentation, Windows, Ubuntu, macOS, and diagnostic coverage jobs, and the owner accepted the four visual-evidence screenshots.
- S1 adaptive shell navigation is visually accepted and complete at checkpoint commit `705fef517834e609b485faca511e240e6dec0a8a`. Hosted run `30636973316` passed Documentation contracts, Windows, Ubuntu, macOS, and Diagnostic coverage jobs.
- T0-A Tarot semantic foundation is accepted at checkpoint commit `e8cb628d5dbbfb2a5fdc16b87fc4c3247bfb861f`. Hosted run `30788655353` passed Documentation contracts, Windows, Ubuntu, macOS, and Diagnostic coverage jobs.
- T1 first playable Tarot workspace is complete at checkpoint commit `4e7e3d61bc11b875af9fb5591f5a10ed986c8962`: real single-card and three-card in-memory readings, explicit reversal preference, responsive programmatic prototype cards, two selectable backs, localized inspection, and presentation-owned session state are ready for owner visual review. Hosted run `30791606505` passed Documentation contracts, Windows, Ubuntu, macOS, and Diagnostic coverage jobs.
- A3 partial-pack integration is functionally and visually accepted at commit `4977852287051c95945b55f78d18c263dc35de20`: Lupus Noctis contributes three accepted raster cards over the unchanged semantic reading, with Classic first, controlled fallback, localized overlays, and validated built-in resources. Single-card scale remains deferred to T-UX1 and native title-bar replacement to S2.
- AP0 asset-pack runtime architecture is complete at checkpoint commit `37dc91e24a8c73ec949312c21a684f5e2998398d`; hosted run `30806291467` passed Documentation contracts, Windows, Ubuntu, macOS, and Diagnostic coverage jobs.
- A26 is owner-accepted and technically imported at checkpoint commit `0acfd17313ac649221c99273b814560e1e899bff`: `minor.swords.knight`, `minor.pentacles.page`, `minor.wands.three`, `minor.wands.seven`, `minor.wands.ten`, `minor.pentacles.ten`, and `minor.wands.nine` complete Lupus Noctis at 78/78 production illustrations with 0 fallbacks and `partialPack: false`. Hosted run `31059508937` passed Documentation contracts, Windows, Ubuntu, macOS, and Diagnostic coverage jobs. ART-LN standard artwork completion is finished.
- ART-SKILL-RM retired the repository-owned and user-level Tarot generation skill. Future artwork creation and artistic acceptance belong to the owner outside Codex; the active handoff contract is owned by [`LUPUS-NOCTIS.md`](../resources/assets/tarot/artwork-packs/lupus-noctis/LUPUS-NOCTIS.md#artwork-creation-and-codex-handoff).
- TAROT-ART-RUNTIME-1 is accepted at checkpoint commit `916ef59a081c4a465c5c1275944aa7b7da0f3afb`. Lupus Noctis is the sole default user-facing artwork pack, all 78 cards resolve to raster with zero normal fallback, required-pack damage produces a controlled unavailable workspace, and hosted run `31062978166` passed all five documentation, desktop-platform, and diagnostic coverage jobs.
- T-UX1A is complete at checkpoint commit `540bcc4e010780ea4aaf1b5c547b9541e27adc78`: cards use exact `1.5×` widths, controls remain fixed above one vertically scrolling surface, tableau overflow is locally horizontal, the selected-card inspector is removed, auto reveal defaults on with manual reveal available, and versioned AppData JSON restores language/theme and Tarot selections without restoring a reading. Hosted run `31065568715` passed all five jobs.
- INT0-D1–D4, INT0-I1–I4, and INT1-I1 are accepted at checkpoint `11514633ace93585b6ab88905790aeaebf85eb4b`; hosted run `31159098001` (57) passed Documentation, Windows, Ubuntu, macOS, and Diagnostic coverage (5/5). The scoped LF repair and structured single-card presentation are accepted.
- INT-SQL1 is recorded at baseline commit `5ce1be8677a52b3e0e5bd249868671013c1648e5`: canonical JSON bundles compile into immutable SQLite `.noxinterp` packages while runtime selection, fallback, presentation, and silent-host semantics remain preserved.
- INT1-QA0 is accepted at `1aa77fb0fcdc2a755658b0263dd828031f2dd374`; hosted run `31174870501` (59) passed CI.
- INT1-AUTH-RU-BULK is accepted at `8c6d1ad4744394965eec4b09f155608fa8d6d537`; hosted run 60 passed CI with the complete 78-bundle / 156-state Russian Classic single-card corpus.
- The actual branch and current HEAD are always reported by `eng/repo-baseline.ps1`; dynamic Git state is not owned here.
- The visual system is Astral Archive with paired Obsidian and Porcelain themes.

## Current Focus

**INT2-PAIR-RU-W5 — Russian Classic oriented-pair authoring**: indices 0..2499 contain 2,500/3,003 bundles and 10,000/12,012 states; W5 contributes indices 2000..2499 from `minor.cups.six__minor.wands.eight` through `minor.pentacles.seven__minor.wands.nine`. The next missing identity is `minor.pentacles.seven__minor.wands.page`, `two-cards` remains unready, and W6 has not started.

## Preserved Contracts

- The golden Prague astronomy fixture and SwissEphNet boundary remain unchanged.
- Zodiac projection is counterclockwise; known-time charts use Placidus and the accepted orientation.
- UnknownTime has no houses or principal angles and retains its documented technical-noon planet policy.
- Runtime and user data belong in AppData or the platform user-data location; shipped assets belong in the repository.
- Tarot semantic decks, artwork packs, skins, backs, and interpretation packs are independent. `classic` remains the sole built-in pack; only `ru + single-card` is ready and the other seven locale/mode declarations remain unready. The resolver retains read-only SQLite stores, requested→English→Russian→silent absence, and broken-ready stop behavior. Five-section presentation, deterministic tags, metrics, reveal gating, and silent `NoContent` remain unchanged. RU oriented-pair source is intentionally partial at 2,500/3,003 bundles; EN, three-card-position, and synthesis prose remain absent.
- Interpretation authoring status and audit are repository tooling under the Interpretation owner, not Project Stats. Audit warnings are deterministic review suspicions; structural source errors remain errors, and no progress/report file controls authoring or runtime state.
- Lupus Noctis A0 and A1 remain rejected. The owner accepted all A2, A4, A5, A6, A7, A8, A9, A10, A11, the two promoted A12 cards, all four A13 cards, and all A14–A26 cards; the built-in complete pack now owns 78/78 production illustrations, uses no prototype fallback, and declares `partialPack: false`. Detailed card state, provenance, hashes, and accepted exceptions belong only to `resources/assets/tarot/artwork-packs/lupus-noctis/LUPUS-NOCTIS.md` and its linked records.
- Repository packs are versioned seed sources; the target runtime reads synchronized built-in and user packs from AppData. Discovery, no-delete synchronization, fingerprints, normalization, import, tooling, and packaging are staged under `ASSET-PACK-RUNTIME.md`; A3 direct output loading is temporary built-in-only behavior.
- Visual semantics belong to `VISUAL-DESIGN-SYSTEM.md`; Avalonia theme topology belongs to `THEMES.md`.
- Meaningful UI changes require manual smoke through the real application and real controls.

## Active Blockers

- No implementation blocker remains. English translation and multi-card authoring require separate explicit stages.
