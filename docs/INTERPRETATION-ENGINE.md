# Interpretation Engine

The interpretation engine produces structured user-facing meaning from symbolic factors. It must be rule-based, compositional, and grounded in curated symbolic knowledge.

## Critical Rule

Do not generate meaningless LLM-style text.

The interpretation system should not use a language model as the source of symbolic logic. Future LLM use may exist only as an optional narrative polishing layer over already structured meanings.

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

This rule does not prohibit finite, owner-approved Tarot corpora, exhaustive tables, thousands of manually prepared interpretations, or one authored result for every state in a bounded semantic space. A large curated offline corpus may be an intentional quality strategy. Exact Tarot counts and pair architecture remain INT0-D3 decisions.

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

If a future LLM or template system is added, it may only polish or reshape curated structured output. It must not invent the symbolic basis.

Generated narrative should remain calm, restrained, serious, and non-ironic.

The narrative layer must always remain optional and downstream from structured interpretation.

## INT0-P Planning Status

`INT0 planning and owner discussion in progress`.

This is a preliminary architecture and discussion baseline, not an approved production schema or completed INT0 contract. No Tarot interpretation runtime, two-card spread, production corpus, corpus storage format, loader, or authoring tool is implemented by this stage. Durable decisions belong in `DECISIONS-LOG.md` only after the owner completes the discussion with ChatGPT and approves them.

## Current Implementation Baseline

Literal source inspection confirms:

- Domain provides only `single-card` with position `card` and ordered `three-cards` with positions `past`, `present`, and `future`; no two-card spread exists.
- `TarotDrawEngine` already draws without replacement and supports explicit upright-only or upright/reversed orientation policies.
- `TarotInterpretationSetId` is a separate typed identity from the semantic deck, artwork pack, presentation skin, and back variant.
- Presentation has a separate `InterpretationLanguagePreference` and a foundation interpretation-set identity that honestly contains no prose.
- `NoxAeterna.Interpretation` currently provides only the project boundary over Domain and Symbolics; it contains no Tarot interpretation runtime.
- `resources/localization/interpretation/ru.json` and `en.json` contain only the early `interpretation.aspect.square` placeholder. They are not an approved production storage format for a future Tarot corpus.
- The current Tarot UI honestly reports that interpretation content is unavailable.

## Confirmed Constraints

### Confirmed UX Decision: Reveal-Gated Visibility

Reveal state is a Presentation-owned visibility policy. With auto reveal enabled, every card is revealed after Draw and a future MVP interpretation appears immediately. With auto reveal disabled:

- a single-card interpretation appears only after that card is manually revealed;
- a future two-card combination may reveal cards separately, but its one pair interpretation appears only after both are revealed;
- Past / Present / Future may progressively add a position-aware fragment for each revealed card;
- transition content requires both cards involved in that transition to be revealed;
- whole-spread synthesis requires all three cards to be revealed.

A hidden card must never influence visible text, titles, keywords, transitions, advice, or diagnostics. Progressive content must not rewrite already visible meaning with information from a card that is still hidden.

This decision defines presentation visibility only. It does not decide corpus schema, pair ordering, reversed composition, interpretation-set identity, semantic IDs, storage partitioning, or authoring workflow; those remain INT0 decisions.

### Tarot Interpretation-Pack Boundary

Tarot interpretation packages remain independent from semantic and visual selections. Their canonical identity, capability, readiness, locale resolution, selection, and content-absence rules now belong to [`TAROT-INTERPRETATION-PACKS.md`](TAROT-INTERPRETATION-PACKS.md). This document continues to own the cross-domain structured-first composition boundary; it does not redefine pack runtime policy.

### Structured-First Meaning

The existing structured-first rule remains cross-domain. An LLM is not a source of Tarot meanings or symbolic logic. A future LLM or template narrative layer may only be an optional downstream formatter over selected, validated meaning fragments; disabling it must leave a complete structured result.

### Orientation Is Required Input

The future contract cannot model upright meanings only. It must account explicitly for upright and reversed cards and for orientation effects in:

- single-card interpretation;
- pair interpretation;
- position-conditioned three-card interpretation.

### Russian-First Authoring and Locale Resolution

Russian is the primary Tarot authoring locale, but exact entry and translation workflows remain INT0-D2 scope. The approved per-pack/per-mode readiness and `requested -> English -> Russian -> no content` resolution contract belongs only to [`TAROT-INTERPRETATION-PACKS.md`](TAROT-INTERPRETATION-PACKS.md).

## Tarot Mode Planning

### INT1 — Single-Card Interpretations

The implemented semantic mode draws one card into `single-card/card` with upright or reversed orientation. Minimum future corpus scope is 78 semantic cards × 2 orientations = 156 orientation meanings, with machine completeness validation and owner-reviewed Russian production content.

The exact fields remain open: short and/or expanded form; keywords; central theme; constructive expression; shadow; advice; warning; reflection prompt; required versus optional fields; and whether reversed meaning is independent authored content or a structured transformation of upright meaning.

### INT2/INT3 — Two-Card Combination Mode

Owner product direction is a separate mode that draws exactly two distinct cards without replacement and interprets the pair primarily as one combination, not necessarily as positional “card 1/card 2” prose. No spread or UI is added during INT0-P.

The mathematical design space is:

- unordered identity pairs: `78 × 77 / 2 = 3003`;
- ordered identity pairs: `78 × 77 = 6006`;
- four orientation combinations for a canonical unordered pair: upright/upright, upright/reversed, reversed/upright, reversed/reversed;
- potential oriented states under that model: `3003 × 4 = 12012`.

`4004` is not a valid scope count.

### INT4 — Past / Present / Future

The implemented `three-cards` spread is ordered as `past`, `present`, `future`. Future interpretation should be compositional rather than manually enumerating every possible ordered triple. Candidate composition layers are base card meaning, orientation meaning, position-conditioned meaning, past-to-present transition, present-to-future transition, reinforcement or tension between cards, and final spread synthesis. Their exact shape and ownership remain open.

## Working Hypotheses

These are discussion candidates, not an approved schema or implementation contract:

- Pair identity is likely unordered, with canonical symmetric lookup so `A+B == B+A`.
- The production corpus is likely to contain 3003 meaningful identity-pair entries.
- Orientation should not automatically require 12012 independently authored long texts.
- The likely direction is an identity-level pair meaning composed with orientation modifiers, with the possibility of explicitly approved overrides where composition is insufficient.

## Preliminary Roadmap

- **INT0 — Interpretation architecture:** **Planning and owner discussion in progress.** INT0-D1 has approved package and localization architecture; INT0-D2 through INT0-D4 still own content/schema, mode/corpus routing, and final implementation handoff.
- **INT1 — Russian single-card corpus:** 78 cards, upright and reversed, at least 156 orientation entries, machine completeness, and owner-reviewed Russian production content.
- **INT2 — Two-card runtime and UX:** a new spread identity, two distinct cards, reversal behavior, tableau, combination result panel, localized UI labels, artwork independence, and tests. The 3003-entry corpus must not be authored before INT0 schema approval.
- **INT3 — Russian pair corpus:** probable 3003 unordered distinct identity pairs, canonical symmetric lookup, approved orientation composition/override strategy, progress inventory, batch authoring, completeness/duplicate validation, and owner review gates. The count is probable scope, not final approval.
- **INT4 — Russian past/present/future interpretation:** position-conditioned meanings, orientation, transitions, relationships, and compositional synthesis without all-triple enumeration.
- **INT5 — English corpus:** translate stabilized Russian modules while preserving semantic keys and schema, then mark each pack/locale/mode module ready through explicit owner-controlled readiness.
- **INT6 — Future languages:** reuse the approved schema and readiness model without changing semantic IDs or artwork.

INT1–INT6 are not started.

## Authoring and Validation Direction

Preliminary requirements, pending detailed INT0 decisions:

- Production content is machine-readable and keyed by stable semantic identities.
- Large corpora do not live in one giant Markdown file. Markdown owns architecture, decisions, workflows, and progress summaries, not thousands of production texts.
- Storage partitioning and the final format remain unapproved; the current placeholder interpretation JSON does not select them automatically.
- Missing and duplicate entries are detected mechanically. Completeness is evaluated against the semantic deck and supported reading mode.
- An unordered pair has one canonical representation.
- Generated drafts are separate from accepted production content and never become production automatically.
- Review status and progress are machine-verifiable; owner review is the acceptance boundary.
- Draft tooling produces reviewable drafts without writing unreviewed prose directly into production.
- Missing-content presentation and damaged-ready-module behavior follow the silent contract in [`TAROT-INTERPRETATION-PACKS.md`](TAROT-INTERPRETATION-PACKS.md); internal validation remains diagnosable and never invents prose.
- Corpus versioning is independent from UI theme and artwork version.

## Open Decisions

The owner and ChatGPT discussion must decide, without INT0-P pre-answering:

1. Schema fields for upright and reversed single-card entries, including required and optional fields.
2. Short versus expanded output, and whether both are needed.
3. Whether reversed meaning is independently authored or a structured transformation of upright meaning.
4. Whether pair identity is definitively unordered.
5. Canonical pair-key ordering for two semantic IDs.
6. Whether orientation retains an internal order within an unordered identity pair.
7. Whether compositional orientation modifiers are sufficient.
8. Whether selected orientation combinations need explicit override texts.
9. A pair-entry structure that is meaningfully specific rather than template filler.
10. Validation of all probable 3003 pair identities and behavior for missing or damaged entries.
11. Reviewable authoring batch size and progress inventory.
12. Draft generation boundaries and the draft/review/accepted lifecycle.
13. Owner-review acceptance mechanics and content QA criteria.
14. Three-card position modifiers, transition rules, reinforcement/tension rules, synthesis layers, and ownership.
15. Production storage partitioning, manifest/schema versioning, and migration policy.
16. Tooling boundaries and representative test fixtures.
17. Optional future narrative formatter boundaries.
18. Exact semantic-deck capability declaration for an interpretation pack.
