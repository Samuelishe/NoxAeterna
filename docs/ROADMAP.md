# Roadmap

This roadmap is staged to keep the project coherent and reviewable. Stages may overlap when useful, but each stage should leave the repository in a buildable and understandable state.

## Stage 0: Documentation Foundation

Create agent-oriented documentation, project vision, architecture notes, logs, known risks, and immediate next steps.

Status: in progress, documentation initialized.

## Stage 1: Solution Scaffold and Project Structure

Create .NET 10 solution projects with empty or minimal class libraries, project references, test project, package baseline, formatting settings, and build/test commands.

No product behavior is expected yet.

## Stage 2: Domain Model Skeleton

Create core entities and value objects for personal profiles, birth data, zodiac longitude, planetary positions, aspects, Tarot readings, symbolic factors, and interpretation blocks.

Add focused unit tests for normalization and basic invariants.

## Stage 3: Astronomy Abstraction and Time Model

Define `IEphemerisCalculator`, time conversion pipeline, zodiac longitude conventions, aspect calculation, retrograde direction, transit snapshot contracts, and lunar phase contracts.

Implementation may start with a deterministic test adapter before binding Swiss Ephemeris.

## Stage 4: Geometry Model for Charts

Define render-independent chart geometry models, angular math, radial positioning, glyph slots, aspect line models, and collision strategy interfaces.

Test core geometry math.

## Stage 5: Interpretation Engine Prototype

Implement structured symbolic factors, meaning fragments, modifiers, tension/reinforcement detection, and interpretation blocks without LLM dependency.

## Stage 6: Rendering Prototype

Create Avalonia-compatible chart rendering via `CustomControl`, `DrawingContext`, prepared geometry models, vector-style drawing, DPI-aware scaling, hover/selection state, and testable render models.

## Stage 7: Avalonia Shell and First UI Flow

Create the application shell, navigation, profile entry flow, and a first read-only astrology chart or profile view.

## Stage 8: Persistence and Profile Archive

Introduce SQLite storage, repositories, profile archive, saved readings, saved interpretations, and basic migration direction.

## Stage 9: Tarot MVP

Implement a deliberate Tarot reading flow, likely single card and three-card spread first, with upright/reversed support, saved sessions, and structured interpretation.

## Stage 10: Polish, Assets, Export, Packaging

Refine visual identity, asset pipeline, chart export, packaging, logging, diagnostics, and release workflow.

## Tarot Interpretation Roadmap

ART-LN standard artwork is complete at 78/78 and is independent from interpretation work. This preliminary sequence does not replace the global stages above.

### T-UX1A — Unified Tarot Reading Surface

Status: **Implemented**.

Cards use exact `1.5×` widths inside one unified vertically scrolling reading surface with tableau-local horizontal overflow. The inspector is removed, reveal behavior is explicit and persisted, and AppData JSON now stores application and Tarot workspace preferences. No interpretation corpus or two-card runtime was added.

### INT0 — Interpretation Architecture

Status: **Accepted and complete**.

Architecture passes:

1. **INT0-D1 — package and localization architecture:** **Accepted.** Own independent plugin-like interpretation packs, `classic`, locale/mode readiness, silent fallback/absence, spread independence, selection refresh, preference direction, and implementation gates in [`TAROT-INTERPRETATION-PACKS.md`](TAROT-INTERPRETATION-PACKS.md). No runtime was implemented.
2. **INT0-D2 — Classic content and tags:** **Accepted.** Checkpoint `e625b68bb424c589fbc840c600ab377237530434` and hosted run `31091471397` passed all five jobs, including the explicit 16-case repair for run 49. [`TAROT-INTERPRETATION-CONTENT.md`](TAROT-INTERPRETATION-CONTENT.md) owns Classic voice, single-card content, tags/metrics, translation, authoring quality, and typography direction.
3. **INT0-D3 — modes, exhaustive corpora, and routing:** **Accepted.** Checkpoint `67218ccc071719f6425da84b6579c550e4e6b0b6` and hosted run `31093430806` passed all five jobs. INT-SQL1 later supersedes its physical storage direction while preserving stable modes, 3003 unordered identities, 12,012 independently authored states, 468 position entries, and composition semantics.
4. **INT0-D4 — final reconciliation and implementation handoff:** **Accepted.** Checkpoint `2937e989e7fcb61b89534171fe80f0dd04166d9e` and hosted run `31095939556` passed all five jobs. [`TAROT-INTERPRETATION-IMPLEMENTATION.md`](TAROT-INTERPRETATION-IMPLEMENTATION.md) freezes exact JSON/version/hash contracts, layers, migration, settings/selector gates, decision coverage, and staged delivery.

### INT0 Implementation Foundation

Status: **Accepted through INT1-I1 at `11514633ace93585b6ab88905790aeaebf85eb4b`; hosted run 57 (`31159098001`) passed 5/5.**

1. **INT0-I1 — Pack Identity and Schema Contracts:** **Accepted at `9c1f68962e84d21d87b2af8072bb0fadf9c4a2f0`.** Pack/classic identity, pure raw/validated schema contracts, exact JSON/enums, canonical keys/pairs, typed results/diagnostics, and focused tests.
2. **INT0-I2 — Validator and Index Tooling:** **Accepted at `93a26fd8942fe0a519d60e9d5ac1a29f09930340`; run `31105509521` passed 5/5.** Repository CLI/tooling and synthetic fixtures cover path/schema/hash/index/inventory checks, deterministic check mode, and authoring status; no production prose or runtime UI.
3. **INT0-I3 — Built-In Pack Source and Resolver:** **Accepted at `062e1e193d1a62b8c5f61c828e24314a112e7984`.** App-packaged all-not-ready Classic skeleton, pure source/resolver contracts, same-locale trust-chain loading, broken-ready behavior, lazy entries, and bounded caches; no prose.
4. **INT0-I4 — Selector, Settings v2 and Silent Host:** **Product complete and owner/review accepted at `b684bb08b2b1369bfd9c014e45bb6748154534da`; the accepted INT1-I1 checkpoint later passed hosted run 57 (`31159098001`) 5/5.** Manifest-named Classic selector, lazy v1→v2 settings migration, immediate reveal-gated re-resolution, removal of the unavailable placeholder, silent host, and real-control UI evidence.

### INT-SQL1 — Source/Runtime Storage Pivot

Status: **Complete locally, pending owner commit/push and hosted verification.**

Canonical JSON now uses manifest v2 and complete card/pair/position bundles under `resources/interpretation/tarot/sources/`. Strict tooling validates and compiles one immutable SQLite `.noxinterp`; App runtime resolves through read-only package stores. The former production filesystem/index/hash runtime is removed. No Tarot prose was authored.

### INT1 — Single-Card Runtime, Russian Authoring, and Promotion

Status: **INT1-I1 accepted; bulk Russian authoring follows hosted-green INT-SQL1.**

**INT1-I1** adds fixture-backed five-section presentation, deterministic three-tag selection, semantic valence roles, intensity markers, reveal gating, and a Debug-only visual seam without production prose. Next, author all 78 Russian bundles / 156 upright/reversed states in one autonomous research/generation/QA/correction run under **INT1-AUTH-RU** while readiness stays false; complete inventory, vocabulary, labels, compilation, inspection, runtime smoke, and explicit owner promotion are mandatory before **INT1-PROMOTE-RU**.

### INT2 — Two-Card Runtime and UX

Status: **Not started**.

Implement the approved non-positional `two-cards` spread, draw two distinct cards, gate meaning until both reveal, provide a combination panel with four tags, localize UI labels, preserve artwork independence, and add tests against canonical ordinal pair/orientation routing.

### INT3 — Pair Tooling and Russian Pair Corpus

Status: **Not started**.

After the single-card foundation, author all 12,012 independent orientation-state interpretations over exactly 3003 canonical unordered identities in several large waves of approximately 1,500–2,500 states. All four states of a pair remain together; prompts and `SESSION-LOG` own wave ranges, while canonical paths contain no wave taxonomy. Readiness remains false until validation and compilation prove completeness.

### INT4 — Russian Past / Present / Future Interpretation

Status: **Not started**.

In **INT4-I1 and later**, finalize the typed trajectory profile/threshold fixtures, author 468 position/orientation entries and curated synthesis resources, and compose all three relations deterministically without enumerating 3,651,648 oriented triples.

### INT5 — English Content

Status: **Not started**.

Begin with **INT5-EN-SINGLE** after the stabilized Russian single-card module. Translate meaning-preservingly while retaining semantic keys/schema; English readiness is declared independently per pack/locale/mode and never waits for all modes together.

### INT6 — Future Languages

Status: **Not started**.

Reuse the approved schema and manual module-readiness model, follow the canonical per-mode locale chain, and make no language-specific change to semantic IDs or artwork.

### Future Mode — Celtic Cross

Status: **Not started**.

Celtic Cross becomes available when its Domain, Presentation, and UI mode is implemented, independently from interpretation-pack completeness. Its larger card count needs smaller surfaces than single-, two-, and three-card modes; exact layout and dimensions belong to a separate UX design stage. Interpretation is a separate compositional `celtic-cross` module. INT0-D3 defines no position names, relation graph, synthesis, layout, or implementation.
