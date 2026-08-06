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

Status: **Planning and owner discussion in progress**.

INT0 is four consecutive documentation passes; completing one pass does not complete INT0:

1. **INT0-D1 — package and localization architecture:** **Documentation complete.** Own independent plugin-like interpretation packs, `classic`, locale/mode readiness, silent fallback/absence, spread independence, selection refresh, preference direction, and implementation gates in [`TAROT-INTERPRETATION-PACKS.md`](TAROT-INTERPRETATION-PACKS.md). No runtime was implemented.
2. **INT0-D2 — Classic content and tags:** **Next.** Approve the Classic single-card content contract, tag model, valence/intensity, Russian-source authoring, translation workflow, and related validation boundaries.
3. **INT0-D3 — modes, exhaustive corpora, and routing:** **Not started.** Approve pair and three-card mode contracts, bounded exhaustive corpora, orientation strategy, synthesis, and content routing.
4. **INT0-D4 — final reconciliation and implementation handoff:** **Not started.** Reconcile the complete INT0 owner set, remove remaining ambiguity, and prepare bounded implementation stages.

### INT1 — Russian Single-Card Corpus

Status: **Not started**.

Author and owner-review Russian upright and reversed meanings for 78 cards, at least 156 orientation entries, with machine completeness validation.

### INT2 — Two-Card Runtime and UX

Status: **Not started**.

Add an approved two-card spread identity, draw two distinct cards, support reversal behavior, provide a two-card tableau and combination panel, localize UI labels, preserve artwork independence, and add tests. Do not author the probable 3003-entry corpus before INT0 schema approval.

### INT3 — Russian Pair Corpus

Status: **Not started**.

Probable scope is 3003 unordered distinct identity pairs with canonical symmetric lookup, an approved orientation composition/override strategy, progress inventory, batch authoring, machine completeness/duplicate validation, and owner review gates. The pair count is a working scope hypothesis, not final implementation approval.

### INT4 — Russian Past / Present / Future Interpretation

Status: **Not started**.

Compose position-conditioned card meaning, orientation, relationship transitions, reinforcement/tension, and spread synthesis without enumerating every ordered triple.

### INT5 — English Corpus

Status: **Not started**.

Translate stabilized Russian modules while preserving semantic keys and schema. English readiness is declared independently for each pack/locale/mode module; runtime fallback already follows the canonical pack-level chain rather than waiting for every English mode to complete together.

### INT6 — Future Languages

Status: **Not started**.

Reuse the approved schema and manual module-readiness model, follow the canonical per-mode locale chain, and make no language-specific change to semantic IDs or artwork.

### Future Mode — Celtic Cross

Status: **Not started**.

Celtic Cross becomes available when its Domain, Presentation, and UI mode is implemented, independently from interpretation-pack completeness. Its larger card count needs smaller surfaces than single-, two-, and three-card modes; exact layout and dimensions belong to a separate UX design stage. Interpretation is a separate `celtic-cross` mode module. INT0-D1 starts no Celtic Cross semantics or implementation.
