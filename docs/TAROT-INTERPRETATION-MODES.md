# Tarot Interpretation Modes

| Metadata | Definition |
| --- | --- |
| Role | Canonical mode-level content, corpus, storage, indexing, and routing architecture for Tarot interpretation packs. |
| Read when | Designing two-card or multi-card interpretation semantics, oriented-pair identity, three-card composition, production source layout, indexes, authoring inventory, or mode validation. |
| Authoritative for | Stable interpretation mode IDs; two-card combination semantics; canonical unordered pair and orientation-state identity; exhaustive oriented-pair scope; pair content and tags; reversed mechanism metadata; three-card position content, relation graph, progressive visibility, synthesis, tags, and metrics; Celtic Cross composition direction; production source and authoring paths; manifest/index direction; direct lazy routing; mode dependencies; expected inventories; validation gates; and batching. |
| Not authoritative for | Pack identity, readiness, fallback, or silent absence; Classic prose style or single-card five-section content; exact common serialization, layer allocation, or migration; actual interpretation prose; Avalonia layout; palette or fonts; artwork; or general settings persistence. |

## Ownership and Current Boundary

Interpretation-pack identity, coarse `pack + locale + mode` readiness, locale fallback, and broken-ready behavior remain owned by [`TAROT-INTERPRETATION-PACKS.md`](TAROT-INTERPRETATION-PACKS.md). Classic voice, single-card prose, tag concepts, metrics, Russian-source translation, and content quality remain owned by [`TAROT-INTERPRETATION-CONTENT.md`](TAROT-INTERPRETATION-CONTENT.md). Exact common JSON, layers, migration, hashes, and implementation stages belong to [`TAROT-INTERPRETATION-IMPLEMENTATION.md`](TAROT-INTERPRETATION-IMPLEMENTATION.md). [`INTERPRETATION-ENGINE.md`](INTERPRETATION-ENGINE.md) owns the generic structured-first boundary; [`TAROT-ENGINE.md`](TAROT-ENGINE.md) owns Domain spread semantics.

INT0-D3 approves target architecture only. It creates no spread, runtime, loader, selector, authoring tool, production JSON, corpus directory, or interpretation prose.

## Stable Mode IDs

The stable language-neutral interpretation mode IDs are:

| Mode ID | Meaning |
| --- | --- |
| `single-card` | Existing single-card spread and its interpretation module. |
| `two-cards` | Future non-positional two-card combination mode. |
| `three-cards` | Existing ordered Past / Present / Future spread and its interpretation module. |
| `celtic-cross` | Future Celtic Cross mode. |

An interpretation mode ID normally equals its semantic spread ID. `single-card` and `three-cards` already match implemented spread IDs; future spreads use `two-cards` and `celtic-cross`. Earlier `two-card` or `three-card` wording in conceptual examples is superseded by these canonical IDs.

`two-cards` is specifically non-positional. A future ordered two-card spread with semantic roles such as cause/result or question/answer requires a distinct spread/mode ID and separate content contract.

## Non-Positional Two-Card Semantics

`two-cards` draws exactly two distinct cards without replacement. Neither card has a semantic position; the primary result is one authored interpretation of their combination. Draw order does not change combination identity:

```text
A + B == B + A
```

Both cards must be revealed before any pair interpretation or tags become visible. Revealing one card alone shows no temporary single-card interpretation, pair relation, or tags, and no hidden-card meaning may leak. This unordered rule applies only to `two-cards`, not to a future positional two-card spread.

### Canonical Pair Identity

The standard deck has 78 distinct semantic card IDs. Without self-pairs:

```text
78 × 77 / 2 = 3003 canonical unordered identities
```

Canonicalization compares the two complete stable normalized lowercase semantic ID strings using ordinal, case-sensitive ordering. The ordinal-smaller ID is card A and the ordinal-larger ID is card B:

```text
CanonicalPair(A, B) = A < B ? (A, B) : (B, A)
```

Identity never depends on localized names, artwork filenames, display numbers, catalog iteration order, draw order, or historical Tarot numbering.

### Orientation-State Identity

Every canonical identity owns four states:

- `upright-upright`;
- `upright-reversed`;
- `reversed-upright`;
- `reversed-reversed`.

Orientation slots attach to canonical card A and card B, not to draw order. For canonical IDs `major.tower` and `major.world`, `reversed-upright` means Tower reversed and World upright. If runtime receives the opposite draw order, it canonicalizes IDs and moves each orientation with its card.

The complete inventory is therefore:

```text
3003 identities × 4 orientation states = 12012 oriented pair states
```

### Exhaustive Authored Pair Corpus

Every one of the 12,012 oriented states receives its own complete, independently authored and reviewed Classic interpretation. The final quality target forbids base-pair prose plus automatic reversal modifiers, mechanical negation, runtime-generated prose, concatenated single-card meanings, sparse overrides over generic templates, or one text shared by all four states.

This is deliberate offline quality work expected to take weeks. Corpus size is not a reason to weaken content quality.

### Pair Entry Contract

Each oriented pair state has two required semantic prose fields:

- `interaction` — what the cards create, reinforce, obstruct, or expose together;
- `direction` — where the combined dynamic is likely to lead or how it should be handled.

Normally each field is one medium sentence, forming one coherent two-sentence interpretation. Sentence count is an authoring target, not a punctuation validator. Visible field headings are not required; Presentation may render one compact paragraph.

Conceptual shape:

```text
oriented pair state
├── canonical card A ID
├── canonical card B ID
├── orientation of A
├── orientation of B
├── interaction
├── direction
├── candidate tag assignments
├── overall valence
└── overall intensity
```

Source/review metadata remains authoring metadata outside runtime readiness.

### Pair Tags and Metrics

Every oriented state owns a combination-specific pool of `6–10` candidate tag assignments. It is not mechanically assembled from the two single-card pools. Tags describe the interaction as one meaning and retain their authored concept ID, valence, and intensity.

Default two-card presentation selects four distinct tags. Selection is deterministic for one reading, pack, and content version; concept IDs survive language changes while labels re-resolve in the locale. Responsive visual layout is deferred to implementation.

Pair overall valence (`-2..+2`) and intensity (`1..3`) are authored for that exact state under the D2 metric contract, not calculated from its component cards.

## Reversed Mechanism Metadata

Reversed single-card entries may declare one to three internal language-neutral `reversalMechanisms` values from this controlled vocabulary:

- `blocked`;
- `delayed`;
- `internalized`;
- `excessive`;
- `distorted`;
- `resisted`;
- `depleted`.

Upright entries declare none. These values support authoring consistency, validation, and future synthesis; they are never exposed as technical user-facing labels and never generate pair prose. All four pair states remain independently authored. This metadata extends rather than replaces the D2 upright/reversed prose contract; D4 freezes the field name and exact entry serialization in the implementation owner.

## Ordered Three-Card Mode

`three-cards` remains ordered as `past`, `present`, and `future`. It combines separately authored position-aware card content, three relations resolved from the shared oriented-pair corpus, and deterministic final synthesis. It does not enumerate all triples.

### Position Entries

Every semantic card has independent content for each position and orientation:

```text
78 cards × 3 positions × 2 orientations = 468 position entries
```

Each entry owns one concise position-aware interpretation, candidate tags for synthesis, overall valence, and overall intensity. The length target is one substantial or two short sentences minimum, and two substantial or three short sentences maximum. This is separately authored for the exact position/orientation, not copied or compressed from the five-section single-card entry.

Visible position headings remain localized as `Прошлое / Past`, `Настоящее / Present`, and `Будущее / Future`.

### Relation Graph

A complete reading resolves three canonical oriented pair states from the shared 12,012-state corpus:

| Relation ID | Inputs | Semantic role |
| --- | --- | --- |
| `past-present` | past + present | What carries from the past into the present. |
| `present-future` | present + future | The current trajectory toward the future. |
| `past-future` | past + future | The longer arc or unresolved influence across the reading. |

Pair identity remains unordered; temporal role belongs to this composition layer.

Visible navigation headings use stable relation IDs with pack/locale-owned labels:

| Relation ID | Classic RU | Classic EN |
| --- | --- | --- |
| `past-present` | Что привело к настоящему | What shaped the present |
| `present-future` | Куда движется ситуация | Where the situation is heading |
| `overall` | Общая картина | The overall picture |

The `past-future` relation is an internal synthesis input rather than a separately required visible block. Presentation owns typography.

### Progressive Visibility

With auto reveal disabled, only content whose complete inputs are revealed may appear:

- **One revealed card:** show only that position entry; show no other position, pair relation, final synthesis, or spread-level tags.
- **Past and present revealed:** show both position entries and the `past-present` relation using the pair state's `interaction` prose.
- **Present and future revealed:** show both position entries and the `present-future` relation using the pair state's `direction` prose.
- **Past and future revealed while present is hidden:** show both position entries, but reserve the `past-future` relation for complete synthesis; show no content implying knowledge of present.
- **All three revealed:** show all position entries, adjacent relations, final synthesis, and three spread-level tags.

Hidden cards never influence visible text, tags, metrics, relation choice, or synthesis.

### Deterministic Synthesis

Distinct ordered card triples number:

```text
78 × 77 × 76 = 456456 identity triples
456456 × 8 orientation states = 3651648 oriented triples
```

The project deliberately rejects authoring 3,651,648 triple texts. A complete result instead follows this deterministic corpus-backed pipeline:

```text
3 position entries
+ past-present oriented pair state
+ present-future oriented pair state
+ past-future oriented pair state
+ typed trajectory classification
+ localized curated synthesis fragments
-> deterministic three-card synthesis
```

The output is normally a two-to-four-sentence final synthesis block, exactly three spread-level tags when enough distinct candidates exist, and synthesized overall valence/intensity.

Synthesis preserves all positions; identifies reinforcement, contradiction, or reversal of trajectory; distinguishes improving, worsening, stable, volatile, and unresolved motion; treats the future card as direction rather than guaranteed fate; incorporates the past-to-future long arc; avoids concatenation and arithmetic averaging; and is reproducible for the same reading, pack, and content version. No runtime LLM is used. Exact implementation classes and algorithms remain deferred.

The small internal pack-owned trajectory vocabulary is:

- `improving`;
- `worsening`;
- `stable`;
- `volatile`;
- `blocked`;
- `reversal`;
- `culmination`;
- `unresolved`.

Classification deterministically uses position/relation concepts and metrics. Codes are not displayed; pack-localized templates express them naturally. Exact numerical thresholds belong to implementation fixtures and reviewed pack rules.

### Three-Card Tags and Metrics

A complete reading shows exactly three spread-level tags when enough candidates exist:

1. one from `past-present`;
2. one from `present-future`;
3. one from `past-future`.

Selection uses oriented pair pools and deterministic pack rules, may prefer authored intensity, avoids duplicate concept IDs by taking the next suitable candidate, and never invents an absent concept. Every accepted candidate is already relevant; there is no separate relevance or weight field. The three concepts remain stable for one reading, pack, and content version; a language change changes labels only. Position-entry tags remain synthesis inputs and do not create extra default tag rows.

Complete-spread `overallValence` (`-2..+2`) and `overallIntensity` (`1..3`) come from deterministic pack rules. They are not a simple average, a clamped sum, a random choice, or an automatic copy of the future card. Dominant relations, future direction, and high-intensity conflicts may outweigh weaker themes; exact rule tables are pack data validated during implementation.

## Shared Corpus, Dependencies, and Locale Integrity

The 12,012-state oriented-pair corpus is shared once per pack/locale. It supports visible `two-cards`, relations inside `three-cards`, and later explicitly approved multi-card composition; it is not duplicated under each mode.

Coarse readiness remains `pack + locale + mode`. A ready mode may declare same-locale dependencies:

```text
two-cards / ru
└── oriented-pairs / ru

three-cards / ru
├── oriented-pairs / ru
├── three-card positions / ru
└── synthesis rules and fragments / ru
```

Locale resolves once for the requested pack and mode through the D1 policy. After resolution, position entries, pair states, synthesis resources, labels, tags, and every dependency come from that one locale. A shared corpus never performs its own fallback. RU positions plus EN pairs, ZH tags plus EN prose, or EN templates plus RU positions are invalid.

If a ready mode lacks or damages a required corpus, index, position entry, or synthesis resource, the module is broken: no other locale is tried, no interpretation is displayed, and internal diagnostics remain possible under the D1 broken-ready rule.

## Production Source Architecture

The future built-in repository source root is:

```text
resources/interpretation/tarot/packs/<pack-id>/
```

The first pack target is `resources/interpretation/tarot/packs/classic/`. This tree is approved but is not created by INT0-D3:

```text
classic/
├── interpretation-pack.json
├── indexes/
└── content/
```

Only accepted production content and generated runtime indexes are packaged. The production pack never contains an `authoring/` subtree; non-shipped work lives exclusively under the separate working root below.

### Accepted Content Paths

```text
content/<locale>/vocabulary/<concept-id>.json

content/<locale>/modes/single-card/<card-id>/upright.json
content/<locale>/modes/single-card/<card-id>/reversed.json

content/<locale>/shared/oriented-pairs/<card-a-id>__<card-b-id>/<orientation-state>.json

content/<locale>/modes/three-cards/positions/<position>/<card-id>/<orientation>.json
content/<locale>/modes/three-cards/synthesis/<rule-or-fragment-id>.json
```

Examples:

```text
content/ru/modes/single-card/major.fool/upright.json
content/ru/modes/single-card/minor.cups.ace/reversed.json
content/ru/shared/oriented-pairs/major.tower__major.world/reversed-upright.json
content/ru/modes/three-cards/positions/past/major.tower/upright.json
```

In pair paths, card A is always ordinal-smaller than card B. One single-card state, oriented pair state, three-card position state, vocabulary concept, and synthesis rule/fragment each use one bounded authored file. Hundreds or thousands of prose entries never share one hand-authored JSON file. Generated indexes may contain thousands of path/hash records because they are prose-free and machine-owned.

### Manifest Direction

The versioned manifest logically requires:

- `schemaVersion`;
- `packId`;
- `semanticDeckId`;
- `sourceLocale`;
- `contentVersion`;
- localized display names;
- a readiness matrix using `single-card`, `two-cards`, `three-cards`, and `celtic-cross`;
- module dependencies;
- generated index paths.

For Classic, `packId = classic`, `semanticDeckId = standard-78`, and `sourceLocale = ru`. These logical fields are mandatory; their exact schema is frozen in [`TAROT-INTERPRETATION-IMPLEMENTATION.md`](TAROT-INTERPRETATION-IMPLEMENTATION.md).

Version dimensions stay separate:

- `schemaVersion` — storage/contract format;
- `contentVersion` — reviewed meaning content;
- generated-index schema/version;
- optional module revisions for authoring and diagnostics.

Theme, artwork, and back changes never alter interpretation content version. Any change that can affect visible prose, tags, or deterministic tag selection increments `contentVersion`, which participates in deterministic selection.

### Generated Indexes and Direct Routing

Machine-generated indexes are:

```text
indexes/<locale>/single-card.json
indexes/<locale>/oriented-pairs.json
indexes/<locale>/three-cards.json
```

Each contains index schema version, pack ID, locale, corpus/mode ID, expected count, canonical entry key, package-relative path, SHA-256, and only approved compact routing metadata. Indexes contain no prose, are generated from accepted content, and are never manually authored. The shared envelope, canonical key strings, and manifest-index-content trust chain are frozen in [`TAROT-INTERPRETATION-IMPLEMENTATION.md`](TAROT-INTERPRETATION-IMPLEMENTATION.md).

Target resolution loads the pack manifest, resolves pack/locale/mode once, loads only required same-locale indexes, computes canonical keys directly, opens only files needed for the revealed result, verifies expected paths/hashes, caches bounded immutable results, and re-resolves on pack, language, or content-version change.

Runtime must not recursively search per reading, use fuzzy or nearest filename lookup, mix locales, infer semantic order from file order, load all 12,012 prose files at startup, or scan the whole corpus for a key. Many small authored files are intentional; direct indexed lookup keeps resolution bounded.

### Expected Classic/Russian Inventories

```text
single-card entries:       156
oriented pair identities: 3003
oriented pair states:    12012
three-card positions:      468
manifest:                     1
generated indexes:      bounded set
```

Vocabulary concepts and synthesis fragments/rules are evolving finite reviewed inventories. Readiness stays false while required inventories are incomplete; no per-entry runtime readiness exists.

## Runtime AppData Direction

The repository owns built-in source. Future installed interpretation packages use a dedicated root independent from artwork:

```text
<LocalApplicationData>/NoxAeterna/
  interpretation/
    tarot/
      packs/
        built-in/
          <pack-id>/
        user/
          <pack-id>/
      state/
```

Interpretation packs never live inside artwork directories. General platform-data ownership remains in [`PERSISTENCE.md`](PERSISTENCE.md); future interpretation seeding, discovery, no-delete updates, and user installation need their own implementation planning and are not silently transferred to [`ASSET-PACK-RUNTIME.md`](ASSET-PACK-RUNTIME.md). INT0-D3 implements none of them.

## Authoring Workspace and Inventory

Non-shipped work lives under the future root:

```text
resources/interpretation/tarot/working/<pack-id>/
```

It may contain drafts, translation work, review batches, progress inventories, permitted generation prompts/structured inputs, and intentionally retained superseded drafts. It is never packaged. Accepted work is promoted into `resources/interpretation/tarot/packs/<pack-id>/content/`. Neither directory is created during D3.

Per-entry authoring status is allowed outside runtime readiness with lifecycle `draft -> reviewed -> accepted`. A machine-readable authoring inventory may track entry key, mode/corpus, locale, review batch, source/translation revision, status, reviewer, and acceptance timestamp or commit reference. It supports production, is omitted from packages unless needed for provenance, never controls fallback, and never creates per-entry readiness.

### Batching

Russian single-card review batches naturally cover Major Arcana (`22 × 2 = 44` entries) and each suit (`14 × 2 = 28`).

Pair authoring partitions by canonical first card and bounded ranges of second cards. A recommended review batch contains 24–40 state files: 6–10 canonical identities with all four states kept together. Review need not take every state for one first card as a giant batch, and the four states of one identity are not split across unrelated acceptance batches without a concrete reason.

Three-card position batches may group by position, Arcana/suit, and orientation.

## Validation Gates

Future tooling validates all D2 content-quality rules plus these mode-level contracts.

Pair identity:

- no self-pair, noncanonical order, duplicate pair, missing identity, or unknown semantic ID;
- exactly 3003 distinct canonical identities.

Orientation:

- all four states exist exactly once;
- filename/content orientation agree;
- each orientation remains attached to its canonical card slot;
- exactly 12,012 states exist.

Mode inventories:

- exactly 468 three-card position entries;
- exactly 156 single-card entries when `single-card` is ready;
- position, card, orientation, relation, and mode IDs are known and canonical.

Files and indexes:

- every indexed file exists and every accepted file is indexed;
- paths are unique, package-contained, and match canonical keys;
- hashes and declared/index counts match;
- generated indexes contain no interpretation prose.

Dependencies and locale:

- every ready mode has every required shared corpus/index and synthesis resource;
- no resolved dependency points to another locale;
- any required dependency failure in a ready module yields silent no-content with no locale fallback.

## Celtic Cross Direction

The stable future mode ID is `celtic-cross`. The spread becomes available when its Domain, Presentation, and UI behavior exists, independently from interpretation completeness, and its larger card count requires smaller surfaces than one-, two-, and three-card modes.

Its interpretation receives a dedicated position-content module and structured position/relation composition. It never enumerates every ten-card combination. Exact positions, relation graph, synthesis, and layout remain a later design stage; D3 names no Celtic Cross positions and starts no implementation.

## Saved-Reading Provenance Direction

A future saved reading should be able to retain interpretation pack ID, content version, mode ID, requested and resolved interpretation locales, and semantic card IDs with orientations. Whether rendered prose itself is archived remains a separate persistence/history decision.
