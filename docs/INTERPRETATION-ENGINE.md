# Interpretation Engine

The interpretation engine produces structured user-facing meaning from symbolic factors. It must be rule-based, compositional, and grounded in curated symbolic knowledge.

## Critical Rule

Do not generate meaningless LLM-style text.

The runtime interpretation system does not use a language model as the source of symbolic logic or per-reading prose. Authoring-time Codex may create curated, reviewed repository content; the shipped application remains deterministic and corpus-backed.

Interpretation is structured-first.

## Layered Interpretation

Interpretation should combine symbolic layers such as:

- Base archetype.
- Zodiac modifier.
- House modifier.
- Aspect modifier.
- Retrograde modifier.
- Transit context.
- Lunar context.
- Personal profile context.

Example:

```text
Mars
+ Scorpio
+ 8th house
+ square Saturn
```

This should be interpreted by combining structured symbolic fragments, not by hardcoding every possible combination.

## Pipeline Direction

Target pipeline:

```text
SymbolicFactor[]
-> MeaningFragment[]
-> ContextModifier[]
-> Tension/Reinforcement analysis
-> InterpretationBlock
-> Optional Narrative Layer
```

Operational direction:

1. Collect symbolic factors.
2. Normalize meaning fragments.
3. Apply contextual modifiers.
4. Detect tensions and reinforcements.
5. Produce structured interpretation blocks.
6. Optionally produce atmospheric prose from curated lexicon and narrative templates.

## Bounded Corpora and Combinatorial Explosion

Do not mechanically enumerate effectively unbounded combinations such as every possible planet, sign, house, aspect, transit, lunar phase, and profile context without a meaningful authored reason. Those domains should prefer atomic symbolic fragments, typed modifiers, composition rules, and explicit tension/reinforcement analysis.

This rule does not prohibit finite, owner-approved Tarot corpora, exhaustive tables, thousands of manually prepared interpretations, or one authored result for every state in a bounded semantic space. A large curated offline corpus may be an intentional quality strategy. The approved 12,012-state oriented-pair corpus and compositional three-card boundary belong to [`TAROT-INTERPRETATION-MODES.md`](TAROT-INTERPRETATION-MODES.md).

## Output Shape

Interpretation output should be structured before it becomes prose.

Expected future output:

- Title.
- Contributing factors.
- Primary theme.
- Supporting fragments.
- Tension markers.
- Reinforcement markers.
- Practical or reflective prompts, if appropriate.
- Optional narrative prose.

## Optional Narrative Layer

Narrative tone belongs to the selected interpretation pack. Structured meaning remains authoritative: deterministic presentation may reshape validated fragments but must not invent their symbolic basis. Classic uses a living, literary, direct, predictive voice; future psychological, mystical, or meme packs may use another tone. Detailed Tarot authoring policy belongs to [`TAROT-INTERPRETATION-CONTENT.md`](TAROT-INTERPRETATION-CONTENT.md).

Runtime LLM generation is absent. Optional narrative presentation remains downstream from structured interpretation and cannot be required for a complete result.

## Tarot INT0 Architecture Status

INT0-D1–D4, I1–I4, INT1-I1, INT-SQL1, INT1-QA0, the Russian Classic single-card corpus, and the complete Russian oriented-pair corpus are accepted. The runtime resolver, selector/settings orchestration, silent host, structured single-card renderer, and combined two-card renderer exist. Classic `ru + single-card` and `ru + two-cards` are ready; the other six locale/mode modules remain unready, and no AppData pack source or user-pack flow is implemented.

## Current Implementation Baseline

Literal source inspection confirms:

- Domain provides `single-card` with position `card`, non-positional `two-cards` with technical slots `slot-a`/`slot-b`, and ordered `three-cards` with positions `past`, `present`, and `future`.
- `TarotDrawEngine` already draws without replacement and supports explicit upright-only or upright/reversed orientation policies.
- Current code has `TarotInterpretationPackId` as a separate typed identity from the semantic deck, artwork pack, presentation skin, and back variant; the active compile-time value is `classic`.
- Presentation owns separate interpretation-language and selected-pack preferences plus the pure structured single-card display builder; it reads no files and resolves no fallback/readiness.
- `NoxAeterna.Interpretation` owns schema-v2 source-manifest and bundle contracts, canonical keys/pairs, typed results, package-store abstractions, locale/mode resolution, and bounded semantic caching. `NoxAeterna.Interpretation.Sqlite` is the SQL-specific adapter; neither layer owns AppData settings, UI, or Tarot prose.
- `resources/localization/interpretation/ru.json` and `en.json` contain only the early `interpretation.aspect.square` placeholder. They are not an approved production storage format for a future Tarot corpus.
- The production Classic package resolves its accepted Russian single-card corpus into the five-section host with package-local Russian labels and vocabulary. Unready English and multi-card modules retain silent `NoContent`; Debug-only injected fixtures remain diagnostic seams rather than production fallback.

## Confirmed Constraints

### Confirmed UX Decision: Reveal-Gated Visibility

Reveal state is a Presentation-owned visibility policy. With auto reveal enabled, every card is revealed after Draw and a future MVP interpretation appears immediately. With auto reveal disabled:

- a single-card interpretation appears only after that card is manually revealed;
- the non-positional `two-cards` mode may reveal cards separately, but shows no interpretation or tags until both are revealed and never substitutes temporary single-card content;
- Past / Present / Future may progressively add a position-aware fragment for each revealed card;
- transition content requires both cards involved in that transition to be revealed;
- whole-spread synthesis requires all three cards to be revealed.

A hidden card must never influence visible text, titles, keywords, transitions, advice, or diagnostics. Progressive content must not rewrite already visible meaning with information from a card that is still hidden.

This cross-layer decision defines the visibility boundary. Exact pair identity, progressive three-card relations, bundle inventories, and authoring waves are owned by [`TAROT-INTERPRETATION-MODES.md`](TAROT-INTERPRETATION-MODES.md); compilation/storage belongs to the implementation owner.

### Tarot Interpretation-Pack Boundary

Tarot interpretation packages remain independent from semantic and visual selections. Their canonical identity, capability, readiness, locale resolution, selection, and content-absence rules now belong to [`TAROT-INTERPRETATION-PACKS.md`](TAROT-INTERPRETATION-PACKS.md). This document continues to own the cross-domain structured-first composition boundary; it does not redefine pack runtime policy.

### Structured-First Meaning

The existing structured-first rule remains cross-domain. Runtime LLM generation is not a source of Tarot meanings, symbolic logic, or per-reading prose. Authoring-time Codex may create curated content under the review and quality contract in [`TAROT-INTERPRETATION-CONTENT.md`](TAROT-INTERPRETATION-CONTENT.md); disabling any optional deterministic narrative formatter still leaves a complete structured result.

### Orientation Is Required Input

The future contract cannot model upright meanings only. It must account explicitly for upright and reversed cards and for orientation effects in:

- single-card interpretation;
- pair interpretation;
- position-conditioned three-card interpretation.

### Russian-First Authoring and Locale Resolution

Russian is the primary Tarot authoring locale. The approved entry-level authoring and literary translation contract belongs to [`TAROT-INTERPRETATION-CONTENT.md`](TAROT-INTERPRETATION-CONTENT.md); per-pack/per-mode readiness and `requested -> English -> Russian -> no content` resolution belong only to [`TAROT-INTERPRETATION-PACKS.md`](TAROT-INTERPRETATION-PACKS.md).

## Tarot Mode Planning

### INT1 — Single-Card Interpretations

The implemented semantic mode draws one card into `single-card/card` with upright or reversed orientation. Minimum future corpus scope is 78 semantic cards × 2 orientations = 156 orientation meanings, with machine completeness validation and owner-reviewed Russian production content.

INT0-D2 approves expanded standalone content with five visible sections, independent upright/reversed meanings, tags, and authored metrics. The content contract belongs to [`TAROT-INTERPRETATION-CONTENT.md`](TAROT-INTERPRETATION-CONTENT.md); exact serialization belongs to [`TAROT-INTERPRETATION-IMPLEMENTATION.md`](TAROT-INTERPRETATION-IMPLEMENTATION.md).

### INT2/INT3 — Two-Card Combination Mode

The implemented `two-cards` mode draws two distinct cards without replacement and has no positional roles. Interpretation canonicalizes the unordered identity through ordinal semantic-ID order, attaches four orientation states to the canonical card slots, and resolves one of the accepted `3003 × 4 = 12012` independently authored states. It shows nothing until both cards are revealed; Presentation then exposes pack-local tags plus the combined `interaction` and `direction`. Pair fields, tags, bundle paths, and validation belong to [`TAROT-INTERPRETATION-MODES.md`](TAROT-INTERPRETATION-MODES.md).

### INT4 — Past / Present / Future

The implemented `three-cards` spread is ordered as `past`, `present`, `future`. Its target interpretation uses 468 independently authored position/orientation entries, all three oriented pair relations including past-future, typed trajectory rules, curated localized fragments, and deterministic synthesis. It deliberately rejects an exhaustive 3,651,648-state oriented triple corpus. Exact contracts belong to [`TAROT-INTERPRETATION-MODES.md`](TAROT-INTERPRETATION-MODES.md).

## Superseded Pair Hypotheses

The earlier hypotheses that pair identity might remain ordered, that 3003 base texts could receive orientation modifiers, or that 12,012 independent texts might be unnecessary are superseded by INT0-D3. The durable target is an unordered canonical identity and one independently authored interpretation for each of four orientation states.

## Tarot Implementation Sequence

- **INT0-I1 through I4:** accepted pack foundations, validator, resolver, selector/settings v2, and silent host.
- **INT-SQL1:** canonical bundle source, strict compiler, immutable SQLite package store, and removal of filesystem/index runtime routing.
- **INT1:** single-card runtime foundation followed by all 156 Russian states in one autonomous authoring/QA wave and explicit promotion.
- **INT2:** canonical `two-cards` Domain and UX.
- **INT3:** tooling and 12,012 reviewed Russian oriented-pair states.
- **INT4:** 468 position entries and deterministic three-card composition.
- **INT5:** meaning-preserving English modules after stabilized Russian sources.
- **INT6:** future languages through the same readiness and schema family.

The exact gates belong to [`TAROT-INTERPRETATION-IMPLEMENTATION.md`](TAROT-INTERPRETATION-IMPLEMENTATION.md). INT-SQL1 performs the storage pivot without starting production authoring.

## Authoring and Validation Direction

Approved Tarot-specific detail is delegated to the mode owner; the generic engine requirements are:

- Production content is machine-readable and keyed by stable semantic identities.
- Large corpora do not live in one giant Markdown file. Markdown owns architecture, decisions, workflows, and progress summaries, not thousands of production texts.
- Canonical JSON bundles are source-of-truth authoring data; one generated `.noxinterp` SQLite file is the runtime representation.
- Missing and duplicate entries are detected mechanically. Completeness is evaluated against the semantic deck and supported reading mode.
- Canonical pair identity, orientations, paths, counts, and dependency gates are fixed by the mode owner.
- Draft/review/accepted quality and stateless continuation belong to [`TAROT-INTERPRETATION-CONTENT.md`](TAROT-INTERPRETATION-CONTENT.md); exact inventories and waves belong to [`TAROT-INTERPRETATION-MODES.md`](TAROT-INTERPRETATION-MODES.md).
- Missing-content presentation and damaged-ready-module behavior follow the silent contract in [`TAROT-INTERPRETATION-PACKS.md`](TAROT-INTERPRETATION-PACKS.md); internal validation remains diagnosable and never invents prose.
- Corpus versioning is independent from UI theme and artwork version.

## Frozen Implementation Handoff

INT-SQL1 supersedes the earlier filesystem-runtime handoff with source manifest v2, bundle source schemas, deterministic source digest, SQLite schema v1, typed store contracts, and build-time `.noxinterp` compilation in [`TAROT-INTERPRETATION-IMPLEMENTATION.md`](TAROT-INTERPRETATION-IMPLEMENTATION.md). It preserves typed absence, settings schema 2, locale integrity, mode identities, exhaustive pair scope, position inventory, and the broken-ready stop rule.
