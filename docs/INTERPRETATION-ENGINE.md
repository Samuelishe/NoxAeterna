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

## Avoiding Combinatorial Explosion

Do not create a static rule for every possible combination of planet, sign, house, aspect, transit, lunar phase, and profile context.

Avoid:

- Giant rule tables.
- Giant if-else systems.
- Hardcoded every-combination logic.

Instead:

- Store atomic symbolic fragments.
- Add typed modifiers.
- Define composition rules.
- Track tensions and reinforcements.
- Produce structured blocks with clear contributing factors.

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

### Independent Interpretation-Set Boundary

Semantic deck, artwork pack, presentation skin, card back, and interpretation set remain independent typed concepts. Interpretation content:

- binds to stable semantic card IDs rather than `lupus-noctis` or another artwork pack;
- contains no artwork provenance and does not change when artwork, skin, application theme, or card back changes;
- owns no Avalonia controls, geometry, colors, typography, or layout;
- must survive UI redesign without corpus rewrites;
- must not force Presentation to depend on a specific production storage mechanism.

Presentation orchestrates selection and displays a prepared structured interpretation result. It does not own the meaning corpus. Interpretation composes content and must remain independent from Presentation and persistence infrastructure.

### Structured-First Meaning

The existing structured-first rule remains cross-domain. An LLM is not a source of Tarot meanings or symbolic logic. A future LLM or template narrative layer may only be an optional downstream formatter over selected, validated meaning fragments; disabling it must leave a complete structured result.

### Orientation Is Required Input

The future contract cannot model upright meanings only. It must account explicitly for upright and reversed cards and for orientation effects in:

- single-card interpretation;
- pair interpretation;
- position-conditioned three-card interpretation.

### Russian-First Authoring

The first production corpus is authored in Russian. Mass authoring must not begin before schema and authoring-pipeline approval, including the minimum 156 single-card orientation meanings, the probable 3003 identity pairs, or position-conditioned three-card content.

### Interpretation Locale Selection

UI localization and interpretation localization have different responsibilities and must not share an implicit fallback pipeline. Runtime selects one complete interpretation corpus for one result and never mixes languages within that result. A structured result should retain diagnosable knowledge of the corpus locale actually selected, even if it is not normally displayed.

The owner direction has two future implementation and test phases:

1. **Phase 1 — Russian production corpus only:** `requested locale -> Russian`. Russian, English, and every other UI locale receive the complete Russian interpretation corpus. Runtime must not return empty text or substitute prototype English fragments.
2. **Phase 2 — complete English corpus exists:** `requested locale -> exact complete locale corpus -> English -> controlled failure only for a damaged built-in corpus`. Russian continues to receive Russian; English receives English; a future complete locale receives itself; an unsupported locale receives English. Languages are never mixed in one result.

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

- **INT0 — Interpretation architecture:** **Planning and owner discussion in progress.** Future scope includes typed set identity, schema and storage, ownership, loader/lookup, locale selection and fallback, validation, versioning/migrations, authoring/review workflow, tests, failure behavior, semantic-deck relationship, visual independence, and content granularity for every mode.
- **INT1 — Russian single-card corpus:** 78 cards, upright and reversed, at least 156 orientation entries, machine completeness, and owner-reviewed Russian production content.
- **INT2 — Two-card runtime and UX:** a new spread identity, two distinct cards, reversal behavior, tableau, combination result panel, localized UI labels, artwork independence, and tests. The 3003-entry corpus must not be authored before INT0 schema approval.
- **INT3 — Russian pair corpus:** probable 3003 unordered distinct identity pairs, canonical symmetric lookup, approved orientation composition/override strategy, progress inventory, batch authoring, completeness/duplicate validation, and owner review gates. The count is probable scope, not final approval.
- **INT4 — Russian past/present/future interpretation:** position-conditioned meanings, orientation, transitions, relationships, and compositional synthesis without all-triple enumeration.
- **INT5 — English corpus:** translate the stabilized Russian corpus while preserving semantic keys and schema; change unsupported-language fallback from Russian to English only after English completeness is proven.
- **INT6 — Future languages:** reuse schema and validation, select an exact complete locale corpus, otherwise use English, and never change semantic IDs or artwork for language support.

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
- A missing or damaged built-in entry produces controlled, diagnosable runtime failure rather than silent language mixing, empty output, or invented prose.
- Corpus versioning is independent from UI theme and artwork version.

## Open Decisions

The owner and ChatGPT discussion must decide, without INT0-P pre-answering:

1. Identity and metadata of the first production interpretation set.
2. Schema fields for upright and reversed single-card entries, including required and optional fields.
3. Short versus expanded output, and whether both are needed.
4. Whether reversed meaning is independently authored or a structured transformation of upright meaning.
5. Whether pair identity is definitively unordered.
6. Canonical pair-key ordering for two semantic IDs.
7. Whether orientation retains an internal order within an unordered identity pair.
8. Whether compositional orientation modifiers are sufficient.
9. Whether selected orientation combinations need explicit override texts.
10. A pair-entry structure that is meaningfully specific rather than template filler.
11. Validation of all probable 3003 pair identities and behavior for missing or damaged entries.
12. Reviewable authoring batch size and progress inventory.
13. Draft generation boundaries and the draft/review/accepted lifecycle.
14. Owner-review acceptance mechanics and content QA criteria.
15. Three-card position modifiers, transition rules, reinforcement/tension rules, synthesis layers, and ownership.
16. Production storage partitioning, manifest/schema versioning, and migration policy.
17. Locale representation, corpus completeness definition, and fallback diagnostics.
18. Tooling boundaries and representative test fixtures.
19. Optional future narrative formatter boundaries.
20. Whether interpretation-set selection is visible in the first implementation.
21. Whether one interpretation set may support multiple semantic decks.
22. Behavior when an interpretation set supports only some reading modes.
