# Tarot Interpretation Implementation

| Metadata | Definition |
| --- | --- |
| Role | Canonical serialization, layer-allocation, migration, and staged implementation handoff for Tarot interpretation packs. |
| Read when | Implementing or validating pack contracts, JSON models, indexes, runtime resolution, caching, migration, settings wiring, selector behavior, or an approved interpretation implementation stage. |
| Authoritative for | Final serialization contracts; JSON conventions; version dimensions; hashing; manifest, entry, and generated-index schemas; typed runtime resolution results; cache identity and invalidation; layer/project ownership; migration from interpretation set/foundation to interpretation pack/classic; settings migration direction; selector and presentation implementation gates; built-in acceptance versus runtime damage; staged implementation order; stage dependencies; the first bounded implementation stage; and cross-owner decision coverage. |
| Not authoritative for | Classic prose or actual card meanings; pair prose; pack readiness/fallback policy; mode semantics; artwork; fonts or colors; exact Avalonia visual design; or production interpretation content. |

## Scope and Owner Hierarchy

This document freezes the shared implementation contract approved in INT0-D1 through D4. D4 is accepted at checkpoint `2937e989e7fcb61b89534171fe80f0dd04166d9e`; hosted run `31095939556` passed all five jobs. I1 was published at `9c1f68962e84d21d87b2af8072bb0fadf9c4a2f0`. I2 is accepted at `93a26fd8942fe0a519d60e9d5ac1a29f09930340`; hosted run `31105509521` passed all five jobs. I3 is complete locally and awaits owner commit/push plus hosted verification.

| Owner | Canonical responsibility |
| --- | --- |
| [`TAROT-INTERPRETATION-PACKS.md`](TAROT-INTERPRETATION-PACKS.md) | Pack identity, readiness, locale fallback, partial packages, broken-ready behavior, silent absence, and selection semantics. |
| [`TAROT-INTERPRETATION-CONTENT.md`](TAROT-INTERPRETATION-CONTENT.md) | Classic voice, five single-card sections, upright/reversed content, tags and metrics, Russian-source translation, and authoring quality. |
| [`TAROT-INTERPRETATION-MODES.md`](TAROT-INTERPRETATION-MODES.md) | Mode IDs, pairs and orientations, three-card composition, content paths, indexes, inventories, routing, batching, and mode validation. |
| `TAROT-INTERPRETATION-IMPLEMENTATION.md` | Exact common serialization, project/layer allocation, migration, implementation stages, handoff, and implementation gates. |
| [`INTERPRETATION-ENGINE.md`](INTERPRETATION-ENGINE.md) | Generic cross-domain structured-first interpretation principles. |
| [`TAROT-ENGINE.md`](TAROT-ENGINE.md) | Semantic deck, spreads, draws, readings, and reveal-facing Tarot behavior. |

Other documents may summarize or link these decisions, but do not become competing owners.

### Current Implementation Versus Target

Current code uses `TarotInterpretationPackId`, `InterpretationPackId`, and the prose-free `classic` identity. `NoxAeterna.Interpretation` implements schema-v1 raw documents, immutable validated contracts, exact in-memory JSON, stable enums/IDs, canonical keys/pairs, typed diagnostics/results, filesystem-free source abstractions, mode-level locale resolution, manifest/index/content trust-chain validation, lazy entry routing, and bounded LRU caches.

The App references Interpretation, packages the built-in `resources/interpretation/tarot/packs/classic/interpretation-pack.json`, and owns one contained source graph, an immutable user-facing pack catalog, settings normalization, and reveal-gated workspace resolver orchestration. Presentation exposes typed pack options/selection without manifest or filesystem access. Settings schema 2 persists `selectedInterpretationPackId` and lazily accepts schema 1 without startup rewrite. The visible RU/EN selector name comes from the manifest under UI language; interpretation language independently drives resolver locale. All Classic modules are `ready = false`, so resolution is typed `NoContent` and the host stays empty and hidden. There is no AppData pack source, production index/content/prose, five-section renderer, tag UI, or user-pack flow.

The existing `resources/localization/interpretation/ru.json` and `en.json` remain generic cross-domain localization catalogs. They are not Tarot pack documents, were not moved into Classic, and are not used as resolver fallback content.

## Common JSON Contract

All interpretation-pack source and generated indexes follow these conventions:

- property names are `camelCase`;
- encoding is UTF-8 without BOM, with one trailing newline;
- comments, duplicate property names, `NaN`, and `Infinity` are forbidden;
- stable language-neutral IDs and serialized enum values are exact lowercase strings;
- every path uses `/` on every platform and is package-relative;
- every SHA-256 is an exact lowercase 64-character hexadecimal value;
- prose is plain text, not Markdown or HTML; embedded line breaks are not presentation contracts;
- every JSON document has a positive integer `schemaVersion`;
- unknown optional fields may be ignored only when the active schema policy permits it;
- an unknown required enum value rejects that file;
- malformed JSON is controlled damage and never crashes the whole application.

Pack source content does not use YAML, XML, SQLite, or an executable expression language.

Repository-owned interpretation JSON is checked out as LF on every host through explicit `.gitattributes` rules for production and tracked TestData trees. Validator/index tooling compares the original manifest, generated-index, and accepted-content bytes with the frozen canonical serializer output; BOM, CRLF, alternate formatting, or extra final LF is an error even when JSON parses. `authoring-inventory.json` is excluded until its separate schema owns an exact canonical writer.

## Version Dimensions

`schemaVersion` is a positive integer describing a file or contract structure. It changes only for an incompatible or explicitly versioned structural change.

`contentVersion` is a positive, monotonically increasing pack-level integer. It starts at `1` and increments whenever accepted content changes visible prose, labels, tags, metrics, deterministic tag selection, or synthesis. It participates in cache identity and deterministic tag selection and is retained by future saved-reading provenance.

Each generated index has its own positive integer `schemaVersion`. Optional source revisions, translation revisions, module revisions, and review revisions remain authoring or diagnostic metadata; they are not runtime `contentVersion` and do not create per-entry readiness.

Theme, artwork, card-back, and UI-layout changes never increment interpretation `contentVersion`.

## Pack Manifest

The logical manifest filename is `interpretation-pack.json`. Schema version 1 has this exact common shape; the example is structural and is not a production pack:

```json
{
  "schemaVersion": 1,
  "packId": "classic",
  "semanticDeckId": "standard-78",
  "sourceLocale": "ru",
  "contentVersion": 1,
  "declaredLocales": ["ru", "en"],
  "displayNames": {
    "ru": "Классика",
    "en": "Classic"
  },
  "modules": {
    "single-card": {
      "ru": {
        "ready": false,
        "indexPaths": ["indexes/ru/single-card.json"],
        "dependencies": []
      },
      "en": {
        "ready": false,
        "indexPaths": ["indexes/en/single-card.json"],
        "dependencies": []
      }
    },
    "two-cards": {
      "ru": {
        "ready": false,
        "indexPaths": ["indexes/ru/oriented-pairs.json"],
        "dependencies": ["oriented-pairs"]
      },
      "en": {
        "ready": false,
        "indexPaths": ["indexes/en/oriented-pairs.json"],
        "dependencies": ["oriented-pairs"]
      }
    },
    "three-cards": {
      "ru": {
        "ready": false,
        "indexPaths": [
          "indexes/ru/oriented-pairs.json",
          "indexes/ru/three-cards.json"
        ],
        "dependencies": [
          "oriented-pairs",
          "three-card-positions",
          "three-card-synthesis"
        ]
      },
      "en": {
        "ready": false,
        "indexPaths": [
          "indexes/en/oriented-pairs.json",
          "indexes/en/three-cards.json"
        ],
        "dependencies": [
          "oriented-pairs",
          "three-card-positions",
          "three-card-synthesis"
        ]
      }
    },
    "celtic-cross": {
      "ru": {
        "ready": false,
        "indexPaths": [],
        "dependencies": []
      },
      "en": {
        "ready": false,
        "indexPaths": [],
        "dependencies": []
      }
    }
  },
  "indexFiles": [
    {
      "path": "indexes/ru/single-card.json",
      "sha256": "<lowercase-sha256>"
    }
  ]
}
```

Every `declaredLocales` value has an entry under every canonical mode. Readiness remains one manual flag per pack/locale/mode, with no per-entry readiness. `indexFiles` lists every generated index that physically exists. A missing index is allowed when its module is `ready = false`; `ready = true` requires every declared index path and same-locale dependency. Dependency IDs are stable language-neutral identifiers.

### Display-Name Resolution

Pack display names follow application UI language, not interpretation language. Resolution is silent:

```text
application UI locale -> English display name -> Russian display name -> packId
```

Interpretation sections, tags, and prose continue to follow `InterpretationLanguagePreference` and the pack owner's module fallback. Display-name fallback and interpretation-content fallback are separate decisions.

## Accepted Entry Schemas

### Vocabulary

One locale-owned concept file has this shape:

```json
{
  "schemaVersion": 1,
  "conceptId": "failure",
  "label": "Неудача",
  "meaning": "Неблагоприятный результат, потеря преимущества или срыв ожидаемого исхода."
}
```

The path determines locale. `conceptId` is stable and language-neutral, `label` is user-facing, and `meaning` supplies internal authoring/validation context rather than default UI copy. Valence and intensity never belong to the vocabulary concept. Each file defines one concept.

### Tag Assignment

Every accepted tag assignment uses exactly this common shape:

```json
{
  "conceptId": "conflict",
  "valence": -1,
  "intensity": 2
}
```

There is no `relevance`, `weight`, color, glyph, font, or UI-style field. Every accepted candidate is already relevant. Deterministic presentation may use content version, reading identity, pack ID, semantic entry key, concept ID, and authored intensity without adding another content metric.

### Single Card

```json
{
  "schemaVersion": 1,
  "cardId": "major.fool",
  "orientation": "upright",
  "sections": {
    "situation": "...",
    "development": "...",
    "risk": "...",
    "outcome": "...",
    "advice": "..."
  },
  "tags": [
    {
      "conceptId": "opportunity",
      "valence": 1,
      "intensity": 2
    }
  ],
  "overallValence": 1,
  "overallIntensity": 2,
  "reversalMechanisms": []
}
```

All five section keys are required and non-empty. Upright entries require an empty `reversalMechanisms`; reversed entries require one to three approved mechanisms. A `5–10` tag pool is an authoring target rather than readiness by itself. Duplicate `conceptId` values in one entry are invalid. `cardId` and `orientation` must match path and index key.

### Oriented Pair

```json
{
  "schemaVersion": 1,
  "cardAId": "major.tower",
  "cardBId": "major.world",
  "orientationState": "reversed-upright",
  "interaction": "...",
  "direction": "...",
  "tags": [
    {
      "conceptId": "upheaval",
      "valence": -1,
      "intensity": 3
    }
  ],
  "overallValence": -1,
  "overallIntensity": 3
}
```

`cardAId` is ordinal-smaller than `cardBId`; self-pairs are invalid. `orientationState` attaches to canonical A/B slots. `interaction` and `direction` are required and non-empty. A `6–10` tag pool is the authoring target. Every canonical pair owns four independently authored states.

### Three-Card Position

```json
{
  "schemaVersion": 1,
  "position": "past",
  "cardId": "major.tower",
  "orientation": "upright",
  "text": "...",
  "tags": [
    {
      "conceptId": "rupture",
      "valence": -1,
      "intensity": 3
    }
  ],
  "overallValence": -1,
  "overallIntensity": 3
}
```

Canonical positions are `past`, `present`, and `future`. `text` is required and non-empty. Identity fields must match path and index key. The ready module requires exactly 468 position entries.

### Three-Card Synthesis Resources

The allowed resource categories are `three-card-position`, `trajectory-profile`, `synthesis-fragment`, and `relation-label`. Pack synthesis data is typed, versioned, limited to known fields and trajectory IDs, and incapable of arbitrary code execution. There is no general expression language.

D4 freezes this container/dependency boundary. Exact numerical thresholds and the final typed trajectory-profile record belong specifically to `INT4-I1` implementation fixtures and reviewed pack rules; this is a deferred mode-specific detail, not an unresolved cross-owner architecture question.

## Generated Index Contract

Every generated index uses this shared envelope:

```json
{
  "schemaVersion": 1,
  "packId": "classic",
  "locale": "ru",
  "corpusId": "single-card",
  "contentVersion": 1,
  "expectedEntryCount": 156,
  "entries": [
    {
      "key": "major.fool|upright",
      "path": "content/ru/modes/single-card/major.fool/upright.json",
      "sha256": "<lowercase-sha256>"
    }
  ]
}
```

Entries are arrays sorted ordinally by `key`; generation rejects duplicate keys. Runtime validates the array and materializes an immutable dictionary. Indexes contain no prose and are machine-owned, never manually edited. `packId`, `locale`, and `contentVersion` must match the manifest. Paths and hashes refer to exact accepted bytes.

Canonical keys are:

| Corpus | Key |
| --- | --- |
| Single card | `<cardId>\|<orientation>` |
| Oriented pair | `<cardAId>__<cardBId>\|<orientationState>` |
| Three-card position | `position\|<position>\|<cardId>\|<orientation>` |
| Synthesis resource | `synthesis\|<resourceType>\|<resourceId>` |

`oriented-pairs.json` additionally declares `expectedIdentityCount = 3003` and `expectedEntryCount = 12012`. `three-cards.json` separately declares `expectedPositionEntryCount = 468` and records its synthesis-resource count without treating that evolving reviewed count as universally fixed.

## Trust Chain and Runtime Resolution

### Manifest, Index, and Content Hashes

The trust chain is exact:

1. Load and validate the manifest structure.
2. Select pack, locale, and mode through the pack policy.
3. Validate each required index against the manifest path and SHA-256.
4. Validate index identity, schema, content version, and declared counts.
5. Route directly to the required content file.
6. Validate its exact bytes against the index SHA-256.
7. Parse and validate the entry.
8. Cache the immutable validated model.

The manifest owns index hashes; an index owns content hashes; content files contain no self-hashes.

### Typed Resolution Result

Internal resolution never uses a raw empty-string sentinel. The result is conceptually:

```text
Resolved
├── packId
├── contentVersion
├── modeId
├── requestedLocale
├── resolvedLocale
└── structured content

NoContent
├── internal reason code
└── optional internal diagnostic
```

`NoContent` produces no prose, placeholder, empty bordered block, heading, or technical explanation. Reason codes and diagnostics are test/logging/diagnostic data only.

### Built-In Acceptance Versus Runtime Damage

A repository-owned built-in module declared `ready = true` cannot be merged or packaged unless its exact inventory and dependencies are complete, indexes are generated, counts and hashes match, schemas validate, and every dependency is locale-pure. CI fails on any violation.

An installed or shipped pack can still become damaged after packaging. Runtime then keeps the application operational, returns `NoContent`, does not fall back from the broken ready locale, and shows no user-facing excuse. Strict built-in acceptance and silent runtime safety are complementary gates.

### Cache Identity and Invalidation

Cache keys contain:

- `packId`;
- `contentVersion`;
- `resolvedLocale`;
- corpus/mode ID;
- canonical entry key.

Values are immutable validated models. Cache entries are invalidated or replaced when the pack, content version, resolved locale, source fingerprint/index hash changes, or a pack is removed or reinstalled. Artwork, theme, UI size, card back, and localized display name never enter the key.

## Source and Working Roots

The production pack root is physically separate from authoring work:

```text
resources/interpretation/tarot/packs/classic/
├── interpretation-pack.json
├── indexes/
└── content/
```

It never contains an `authoring/` subtree. Non-shipped work exists only under:

```text
resources/interpretation/tarot/working/classic/
```

Working files never enter package indexes or application output. Built-in source, future AppData installation roots, accepted content paths, and bounded-file rules otherwise remain as specified by the mode owner. D4 creates none of these directories.

## Identity and Settings Migration

### Set/Foundation to Pack/Classic

INT0-I1 replaced the former Set identity and terminology with `TarotInterpretationPackId`/Pack, removed the former placeholder identity, and made `classic` the active compile-time identity without a compatibility alias. Semantic deck, artwork, skin, and back identities remain independent. Focused compile-time and source-boundary tests cover the migration; saved-reading compatibility was not required because readings are not persisted.

### Settings Schema 2

The first pack-selection implementation raises settings schema from 1 to 2 and adds the stable semantic field:

```json
{
  "selectedInterpretationPackId": "classic"
}
```

It may live inside the existing Tarot DTO structure. Version 1 loads successfully, a missing pack ID becomes `classic`, and v1 normalizes to v2 in memory. Startup does not rewrite the file merely because migration occurred; the next real preference save writes schema 2. An unknown ID falls back to `classic` when installed. If no pack exists, cards remain usable and the interpretation host remains empty. No migration message is shown.

The separate planned actions remain unchanged: `Сбросить настройки` resets all preferences to compiled defaults after confirmation, and `Открыть папку данных приложения` opens `<LocalApplicationData>/NoxAeterna`. Neither is part of the first contract stage.

## Layer and Project Ownership

| Project/root | Owns | Must not own |
| --- | --- | --- |
| `NoxAeterna.Domain` | Stable `TarotInterpretationPackId`, semantic Tarot identities, spreads, and readings. | Prose, JSON, file paths, readiness, Avalonia, or persistence. |
| `NoxAeterna.Interpretation` | Pack contracts; manifest/content/index models; validation; canonical keys; locale/mode resolution; structured results; deterministic composition; cache-independent resolver logic. | Avalonia, AppData path construction, App, or Presentation dependencies. |
| `NoxAeterna.Presentation` | Selected-pack and interpretation-language preferences; reveal-gated view state; typed single-card display model and deterministic tag selection from validated Interpretation input; immediate re-resolution orchestration signals. | JSON/file I/O, Avalonia/color/font types, or prose authoring. |
| `NoxAeterna.App` | Composition root; shipped/file and future AppData sources; catalog assembly; selector controls; headings/tags rendering; silent empty host; settings migration and persistence wiring. | Semantic meaning or Domain rules. |
| `NoxAeterna.Tools.Repository` | Schema/inventory validation CLI; index/hash generation; authoring progress reports; batch tooling; one-way reuse of pure Interpretation contracts. | Production runtime behavior; App, Presentation, Infrastructure, Avalonia, or reverse dependencies. |
| `resources/interpretation` | Accepted and working content under the separate D3/D4 roots. | Runtime state or user preferences. |

`NoxAeterna.Interpretation` never depends on `NoxAeterna.App` or `NoxAeterna.Presentation`.

## Selector, Language, and Presentation Gates

The implemented control is named `TarotInterpretationPackSelector`. Its label is `Толкование` in Russian and `Interpretation` in English. It appears in the Tarot control panel and remains visible with the single real manifest-named `Классика / Classic` option, matching the artwork-selector direction.

The selector display name follows UI-language resolution. Its selected stable pack ID persists. A pack switch immediately re-resolves currently visible revealed content without redrawing cards or changing artwork, back, spread, or reveal state.

Presentation gates are:

- no top-level visible `Интерпретация` or `Толкование` heading over the content host;
- single-card five-section headings remain visible;
- two-card `interaction` and `direction` may render as one compact paragraph without field headings;
- three-card position, relation, and overall headings follow the mode contract;
- no content produces no visible block;
- `ui.tarot.interpretation.unavailable` and its ViewModel key are absent from active production UI;
- no fallback-language, readiness, or damaged-file explanation is shown.

Application UI language controls control labels and pack display names. Interpretation language controls section/tag labels and prose. UI-language changes refresh controls and pack names; interpretation-language changes re-resolve content. Neither change redraws cards.

INT1-I1 materializes the compact tag row followed by `situation`, `development`, `risk`, `outcome`, and `advice` in the existing outer reading scroll, with no overall heading or nested scroller. Presentation accepts already resolved pack-local section/tag labels and never derives labels from `conceptId` or the generic UI catalog. Missing tag labels omit those chips; missing section labels suppress an unsafe incomplete presentation. Trusted same-locale vocabulary and section-label index/routing remains a mandatory `INT1-PROMOTE-RU` gate before a production module can become ready.

The default single-card subset contains exactly three distinct labeled candidates when at least three exist, otherwise all available candidates. Candidates are ordered by ordinal hexadecimal SHA-256 of UTF-8 `packId`, `contentVersion`, spread ID, stable `DrawnAt` ticks, position ID, card ID, orientation, and candidate `conceptId`; UI locale, interpretation locale, label text, theme, size, and process hash codes do not participate. Therefore locale switching changes visible labels/prose without changing selected semantic concept IDs for the same reading/pack/content version.

## Decision-Coverage Matrix

| Owner decision | Canonical owner | Implementation stage | Validation/evidence | Status |
| --- | --- | --- | --- | --- |
| Independent selectable non-executable packs; default `classic`; partial packs remain selectable | Packs | I1–I3 | Identity, manifest, catalog, and partial-readiness fixtures | Implemented |
| Reading modes remain available independently from interpretation completeness | Packs / Tarot Engine | INT2-I1 and later spread stages | Selector availability with every module not ready | Frozen; existing modes already ignore corpus completeness |
| UI locales may precede complete interpretation locales | Packs | I3 | Readiness-matrix locale fixtures | Implemented |
| Readiness is manual `pack + locale + mode`, never per entry | Packs | I1–I3 | Manifest validation and no-inference tests | Implemented |
| Fallback is requested → EN → RU → no content | Packs | I3 | Deduplicated resolution fixtures | Implemented |
| Broken `ready = true` forbids fallback | Packs | I2–I3 | Damaged same-locale fixture returns `NoContent` | Implemented |
| One locale supplies an entire mode result | Packs / Modes | I2–I3 | Cross-locale dependency rejection | Implemented |
| Missing/fallback content gives no user explanation | Packs | I3–I4 | Typed result tests and real-control UI smoke | Implemented |
| Pack and language switches re-resolve immediately | Packs | I4 | Presentation orchestration tests and UI smoke | Implemented |
| Selector remains visible with one real pack; its name follows UI language | Packs / Implementation | I4 | Catalog/display-name tests and RU/EN UI smoke | Implemented |
| No placeholder or overall interpretation heading | Packs / UI Vision | I4 | Host materialization and UI smoke | Implemented; no-content host is empty and hidden |
| Auto/manual reveal gates content; hidden cards never leak | Tarot Engine / Modes | I4, INT1-I1, INT2-I1, INT4-I1 | View-state and progressive-reveal fixtures | Implemented for single-card and existing position resolution |
| Selector preference persists | Packs / Implementation | I4 | Settings v1→v2 and restart tests | Implemented |
| Classic uses traditional meanings and original prose | Content | INT1-AUTH-RU onward | Owner review, provenance, similarity audits | Frozen; corpus not authored |
| Classic voice is living and predictive without profanity, slang, memes, insults, or sarcasm | Content | INT1-AUTH-RU onward | Style tooling plus owner review | Frozen; corpus not authored |
| Interpretive openness remains specific and emotionally strong | Content | INT1-AUTH-RU onward | Batch literary review | Frozen; corpus not authored |
| Russian is accepted source; translations are literary and meaning-preserving | Content | INT1-AUTH-RU, INT5 onward | Source/translation structural and semantic review | Frozen; corpus not authored |
| Five visible single-card sections | Content | INT1-I1 | Schema and presentation fixtures | Implemented for resolved single-card content |
| 156 independent upright/reversed entries | Content / Modes | INT1-AUTH-RU, INT1-PROMOTE-RU | Exact inventory and orientation checks | Frozen; corpus not authored |
| Reversed prose is independent; mechanisms remain internal | Content / Modes | I1, INT1-AUTH-RU | Schema and similarity checks | Frozen; not implemented |
| Single-card pools target 5–10; UI presents 2–4 | Content | INT1-I1 | Quality diagnostics and display tests | Implemented default: exactly 3 when available |
| Tag assignments and overall metrics use valence −2..+2 and intensity 1..3 | Content / Implementation | I1, INT1-I1 | Range and round-trip tests | Implemented in model; tag metrics render semantically |
| `conceptId` is stable; visible labels are pack/locale-owned; package-specific concepts are allowed | Content | I1–I2 | Vocabulary and locale fixtures | Frozen; not implemented |
| Tag selection is deterministic and non-flickering | Content | INT1-I1 onward | Stable reading/content-version tests | Implemented with SHA-256 ordinal ranking |
| Content carries no literal UI colors or fonts | Content / Visual Design | I1–I2 | Schema rejection and content audit | Frozen; not implemented |
| `two-cards` is non-positional and unordered | Modes | INT2-I1 | Spread and reveal-gate tests | Frozen; not implemented |
| Pair canonicalization is ordinal by semantic ID | Modes / Implementation | I1 | Canonical key tests including reversed input order | Frozen; not implemented |
| 3003 identities own four states and 12,012 independent texts | Modes | INT3-TOOLING, INT3-AUTH-RU | Exact inventory, duplicate, and state tests | Frozen; corpus not authored |
| Pair entries own `interaction`, `direction`, and their own tag pools | Modes / Implementation | I1, INT2-I1 | Schema and presentation fixtures | Frozen; not implemented |
| Two-card content remains hidden until both cards reveal | Modes | INT2-I1 | Reveal-order tests and UI smoke | Frozen; not implemented |
| Three cards own 468 position entries | Modes | INT4-I1 onward | Exact position inventory | Frozen; corpus not authored |
| Three-card graph uses past-present, present-future, and past-future | Modes | INT4-I1 | Relation-resolution fixtures | Frozen; not implemented |
| Progressive visibility exposes only revealed inputs | Modes | INT4-I1 | Every reveal subset tested | Frozen; not implemented |
| Synthesis is deterministic and rejects exhaustive triple prose | Modes | INT4-I1 | Reproducibility and no-LLM/runtime-generation tests | Frozen; not implemented |
| Complete three-card results show three relation-derived tags | Modes | INT4-I1 | Deduplication and stable-label tests | Frozen; not implemented |
| Authored source uses many bounded JSON files | Modes | I2 and authoring stages | Path/schema/inventory checks | Frozen; no tree exists |
| Generated indexes are prose-free, hashed, and directly routed | Modes / Implementation | I2–I3 | Generation snapshots and trust-chain damage tests | Frozen; not implemented |
| Runtime lookup is lazy and does not recursively scan/eager-load corpus | Modes | I3 | Source-spy and bounded-load tests | Frozen; not implemented |
| Working and production roots are separate | Modes / Implementation | I2 and authoring stages | Packaging exclusion and path-containment tests | Frozen; no tree exists |
| Authoring lifecycle is draft → reviewed → accepted | Content / Modes | I2 and authoring stages | Inventory transition rules and Git review | Frozen; tooling not implemented |
| Codex may author curated content; runtime stays offline and corpus-backed | Content / Engine | Authoring stages and I3+ | Packaging/runtime dependency audit | Frozen; no corpus/runtime |
| Celtic Cross uses smaller cards and later compositional design | Modes / UI Vision | Later dedicated stages | Future UX and mode design evidence | Deferred; does not block I1 |
| Interpretation fonts require script coverage, redistribution, fallback, and provenance | Content / Assets | Later typography stage | License/provenance and RU/EN glyph checks | Deferred; no font selected |
| Reset settings and Open AppData remain separate shared actions | Persistence | Separate settings/AP1 work | Confirmation and path-service tests | Deferred; does not block I1 |
| Stale tableau dimensions are corrected immediately after spread change | Technical Debt | T-UX1B | Reproduction plus no-Draw/no-click layout evidence | Deferred; does not block I1 |

Every approved D1–D3 area has an implementation stage or an explicit independent deferral. “Frozen” means the architecture is settled, not that code or content exists.

## Staged Implementation Roadmap

Implementation status after local CI-R56-FIX + INT1-I1: INT0 I1–I3 are accepted; I4 product work is accepted at `b684bb08b2b1369bfd9c014e45bb6748154534da`, while run `31153781317` was 4/5 green because Windows checkout changed canonical LF bytes. The local repair pins interpretation JSON to LF and rejects noncanonical source bytes. INT1-I1 connects the typed snapshot to a pure Presentation builder and App renderer with five sections, deterministic tags, semantic valence, intensity dots, reveal gating, and Debug-only RU/EN fixtures. No AppData pack source, production index/content, corpus prose, or trusted production vocabulary renderer exists; replacement hosted verification and small-batch Russian authoring are next.

| Stage | Scope | Primary gates |
| --- | --- | --- |
| **INT0-I1 — Pack Identity and Schema Contracts** | Rename Set → Pack; add `classic`; manifest/content/index DTOs, enums, canonical keys, typed results, and pure validation models. No filesystem, UI, or production pack. | Domain/Interpretation boundaries; JSON round trips; canonical-pair keys; schema validation; no Avalonia or file I/O in Interpretation core. |
| **INT0-I2 — Validator and Index Tooling** | Repository CLI/tooling, synthetic fixture pack, content/path/hash validation, index generation, exact inventory checks, and authoring-inventory validation. No production Classic prose. | Deterministic generation; count/hash/path/schema gates; fixture-only resources. |
| **INT0-I3 — Built-In Pack Source and Resolver** | Shipped source abstraction, manifest/index loading, locale/mode resolution, broken-ready behavior, bounded cache, and a `classic` skeleton whose modules are all `ready = false`. No visible prose. | Same-locale resolution; typed `NoContent`; trust chain; bounded loading/cache. |
| **INT0-I4 — Selector, Settings v2 and Silent Host** | Pack selector, `classic` default, v1→v2 migration, removal of `foundation` and unavailable placeholder, immediate re-resolution, and empty host. | Settings migration, presentation orchestration, host materialization, real-control UI smoke. |
| **INT1-I1 — Single-Card Runtime Presentation** | Five sections, visible headings, deterministic tags, metrics presentation model, and fixture content. No full production corpus. | Structured display and reveal tests; semantic typography roles. |
| **INT1-AUTH-RU — Russian Classic Single-Card Authoring** | 156 Russian entries, vocabulary, small reviewable batches, Codex drafts, and owner acceptance. Module stays not ready until exact completion. | Style, repetition, structure, metrics, provenance, and owner review. |
| **INT1-PROMOTE-RU** | Promote accepted content, generate indexes, prove exact completeness, mark `classic/ru/single-card` ready, and smoke end-to-end behavior. | 156 exact; hashes/indexes; UI smoke; silent fallback for other locales. |
| **INT5-EN-SINGLE** | Meaning-preserving English translation and structural audit; mark English ready only after all 156 entries are complete. | Source/translation parity and literary review. |
| **INT2-I1 — Two-Card Domain and UX** | Implement `two-cards`, two distinct cards, reveal gating, and pair rendering without the full Russian pair corpus. | Domain draw/identity tests and real-control reveal smoke. |
| **INT3-TOOLING / INT3-AUTH-RU** | Batch generation/review tooling and all 12,012 Russian pair states; keep the module not ready until complete. | Bounded owner-reviewed batches; exact identities/states/indexes. |
| **INT4-I1 and later** | 468 position entries, final typed trajectory profile and thresholds, synthesis fragments, deterministic runtime, and promotion after complete validation. | Position/relation/synthesis fixtures, exact inventories, progressive reveal, and UI smoke. |

Stages are deliberately bounded and are not combined into one implementation task. English translation follows a stabilized Russian single-card module; the 12,012-pair authoring wave does not precede the single-card foundation.

## Current Implementation Handoff

After replacement hosted-green evidence, the next content stage is:

```text
INT1-AUTH-RU — first small owner-reviewable Russian batch
```

Before any Russian single-card promotion, trusted same-locale pack-local vocabulary/section-label routing, exactly 156 accepted entries, and generated index/hash evidence are mandatory.

INT0-I1 completed the Pack/classic migration and pure contracts. INT0-I2 added explicit-root repository validation/index/authoring tooling. INT0-I3 added the production skeleton manifest, App-owned built-in source catalog, pure resolver, trust-chain loading, and bounded caches. INT0-I4 added the user-facing catalog/selector, settings migration, resolver orchestration, reveal gating, and silent host. INT1-I1 adds structured presentation and test-only preview content; AppData sources, production prose/vocabulary/indexes, `two-cards`, and corpus authoring remain unimplemented.

## Independent Deferred Work

T-UX1B stale card-size refresh debt, interpretation typography/font selection, Reset settings, Open AppData, Celtic Cross layout/composition, AP1–AP5, PKG1, S2, saved readings/history/SQLite, and other interpretation packs remain separate. None blocks the authoring preparation stage.
