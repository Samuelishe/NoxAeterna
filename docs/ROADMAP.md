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

### INT0 — Interpretation Architecture

Status: **Planning and owner discussion in progress**.

Define and approve interpretation-set identity usage, schema and storage direction, content ownership, loader/lookup, locale selection and fallback, validation, versioning/migrations, authoring and review workflow, tests, failure behavior, semantic-deck relationship, independence from artwork/presentation, and content granularity for supported modes. INT0-P records constraints, hypotheses, and open decisions; it does not finalize them.

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

Translate the stabilized Russian production corpus while preserving semantic keys and schema. Unsupported-language interpretation fallback changes from Russian to English only after the English corpus is complete.

### INT6 — Future Languages

Status: **Not started**.

Reuse the approved schema and validation, select an exact complete locale corpus when available, otherwise fall back to English, and make no language-specific change to semantic IDs or artwork.
