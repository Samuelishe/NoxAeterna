# Tarot Interpretation Implementation

| Metadata | Definition |
| --- | --- |
| Role | Canonical serialization, compiler, SQLite package, storage-boundary, build, and delivery owner. |
| Read when | Implementing source DTOs, validators, compilation, package inspection, runtime stores, build packaging, or migration stages. |
| Authoritative for | Manifest v2; canonical JSON; normalized compiler model; source digest; `.noxinterp`; SQLite schema v1/application ID; CLI; atomic compilation; runtime store boundary; build output; staged cutover. |
| Not authoritative for | Editorial prose, corpus wave boundaries, selector wording, or general persistence. |

## Frozen pipeline

```text
canonical Markdown / RAG
→ canonical JSON source under resources/interpretation/tarot/sources/<pack-id>/
→ strict validator and normalized immutable compilation model
→ one generated SQLite <pack-id>.noxinterp
→ read-only package store
→ semantic resolver
```

JSON is authoritative and reviewable. `.noxinterp` is generated runtime data, never manually edited or Git-tracked. SQLite here is unrelated to the future mutable settings/history database.

## Canonical JSON

All source JSON uses UTF-8 without BOM, LF, one compact JSON value, deterministic project serializer order, and exactly one final LF. Unknown members, comments, duplicate members, unsafe paths, CRLF, alternate formatting, and extra final newlines fail validation. `.gitattributes` enforces LF for the canonical source and tracked TestData JSON.

Accepted source is rooted at `resources/interpretation/tarot/sources/<pack-id>/`. `interpretation-pack.json` and every JSON file below `content/` participate in validation and `sourceDigest`. The validator accepts no runtime package files or progress-plan files in this tree.

## Source manifest v2

Manifest schema 2 retains only semantic data:

```json
{
  "schemaVersion": 2,
  "packId": "classic",
  "semanticDeckId": "standard-78",
  "sourceLocale": "ru",
  "contentVersion": 1,
  "declaredLocales": ["ru", "en"],
  "displayNames": { "ru": "Классика", "en": "Classic" },
  "modules": {
    "single-card": {
      "ru": { "ready": false, "dependencies": [] },
      "en": { "ready": false, "dependencies": [] }
    },
    "two-cards": {
      "ru": { "ready": false, "dependencies": ["oriented-pairs"] },
      "en": { "ready": false, "dependencies": ["oriented-pairs"] }
    },
    "three-cards": {
      "ru": { "ready": false, "dependencies": ["oriented-pairs", "three-card-positions", "three-card-synthesis"] },
      "en": { "ready": false, "dependencies": ["oriented-pairs", "three-card-positions", "three-card-synthesis"] }
    },
    "celtic-cross": {
      "ru": { "ready": false, "dependencies": [] },
      "en": { "ready": false, "dependencies": [] }
    }
  }
}
```

Filesystem routing declarations and per-file hash declarations are not part of schema 2. Readiness is explicit and remains `pack + locale + mode`.

## Source bundle schemas

- `labels.json`, schema 1: exact dictionaries `singleCardSections`, `threeCardPositions`, `relations` with IDs owned by the mode contract.
- `vocabulary/<concept-id>.json`, schema 1: one `conceptId`, visible `label`, and authoring `meaning`.
- `single-card/<card-id>.json`, schema 1: one card with exact `upright` and `reversed` states. Each state owns five sections, tags, `overallValence`, `overallIntensity`, and reversal mechanisms.
- `oriented-pairs/<a>__<b>.json`, schema 1: canonical `cardAId < cardBId` and exactly four orientation states. Each owns interaction, direction, tags, valence, and intensity.
- `three-card-positions/<card-id>.json`, schema 1: exact `past/present/future × upright/reversed` states. Each owns text, tags, valence, and intensity.
- `synthesis/<resource-type>/<resource-id>.json`, schema 1: a required `trajectory-profile` or `synthesis-fragment` identity from the mode-owned exact inventory, with payload exactly `data: { "text": <non-empty trimmed localized text> }`. Unknown payload members, reserved type use, wrong type/ID combinations, missing identities, and extra identities are invalid; selection rules live in code rather than JSON.

Every present bundle is all-or-nothing. A not-ready module permits missing bundles, not partial or invalid ones. Exact inventories belong to [TAROT-INTERPRETATION-MODES.md](TAROT-INTERPRETATION-MODES.md).

## Deterministic source digest

`sourceDigest` is lower-case SHA-256 over all package source JSON files. Files are sorted by package-relative path using ordinal comparison. For each file the hash stream appends:

```text
relative-path UTF-8 bytes
0x00
exact canonical file bytes
0x00
```

Absolute paths, modification/build timestamps, machine identity, enumeration order, culture, and random data are excluded. Identical canonical source therefore has an identical digest across operating systems. SQLite binary-file equality is not a portability contract.

## Runtime package identity

- extension: `.noxinterp`;
- storage: one SQLite database per pack;
- `PRAGMA user_version = 1`;
- `PRAGMA application_id = 0x4E4F5849` (decimal `1313822793`, ASCII mnemonic `NOXI`);
- immutable semantic rows only;
- no timestamps, absolute paths, random IDs, settings, history, cache state, or authoring-wave identity.

## SQLite schema v1

The exact schema is:

```sql
PRAGMA foreign_keys = ON;
CREATE TABLE pack_metadata(
  singleton INTEGER PRIMARY KEY CHECK(singleton = 1),
  package_schema_version INTEGER NOT NULL CHECK(package_schema_version = 1),
  pack_id TEXT NOT NULL,
  semantic_deck_id TEXT NOT NULL,
  source_locale TEXT NOT NULL,
  content_version INTEGER NOT NULL CHECK(content_version > 0),
  source_digest TEXT NOT NULL CHECK(length(source_digest) = 64 AND source_digest NOT GLOB '*[^0-9a-f]*')
) STRICT;
CREATE TABLE declared_locale(
  locale TEXT PRIMARY KEY
) STRICT;
CREATE TABLE display_name(
  locale TEXT PRIMARY KEY REFERENCES declared_locale(locale),
  value TEXT NOT NULL CHECK(length(trim(value)) > 0)
) STRICT;
CREATE TABLE module(
  mode TEXT NOT NULL CHECK(mode IN ('single-card','two-cards','three-cards','celtic-cross')),
  locale TEXT NOT NULL REFERENCES declared_locale(locale),
  ready INTEGER NOT NULL CHECK(ready IN (0,1)),
  PRIMARY KEY(mode, locale)
) STRICT;
CREATE TABLE module_dependency(
  mode TEXT NOT NULL,
  locale TEXT NOT NULL,
  ordinal INTEGER NOT NULL CHECK(ordinal >= 0),
  dependency TEXT NOT NULL CHECK(dependency IN ('oriented-pairs','three-card-positions','three-card-synthesis')),
  PRIMARY KEY(mode, locale, ordinal),
  UNIQUE(mode, locale, dependency),
  FOREIGN KEY(mode, locale) REFERENCES module(mode, locale)
) STRICT;
CREATE TABLE label(
  locale TEXT NOT NULL REFERENCES declared_locale(locale),
  category TEXT NOT NULL CHECK(category IN ('single-card-section','three-card-position','relation')),
  label_id TEXT NOT NULL,
  value TEXT NOT NULL CHECK(length(trim(value)) > 0),
  PRIMARY KEY(locale, category, label_id)
) STRICT;
CREATE TABLE vocabulary(
  locale TEXT NOT NULL REFERENCES declared_locale(locale),
  concept_id TEXT NOT NULL,
  label TEXT NOT NULL CHECK(length(trim(label)) > 0),
  meaning TEXT NOT NULL CHECK(length(trim(meaning)) > 0),
  PRIMARY KEY(locale, concept_id)
) STRICT;
CREATE TABLE single_card(
  locale TEXT NOT NULL REFERENCES declared_locale(locale),
  card_id TEXT NOT NULL,
  orientation TEXT NOT NULL CHECK(orientation IN ('upright','reversed')),
  situation TEXT NOT NULL CHECK(length(trim(situation)) > 0),
  development TEXT NOT NULL CHECK(length(trim(development)) > 0),
  risk TEXT NOT NULL CHECK(length(trim(risk)) > 0),
  outcome TEXT NOT NULL CHECK(length(trim(outcome)) > 0),
  advice TEXT NOT NULL CHECK(length(trim(advice)) > 0),
  overall_valence INTEGER NOT NULL CHECK(overall_valence BETWEEN -2 AND 2),
  overall_intensity INTEGER NOT NULL CHECK(overall_intensity BETWEEN 1 AND 3),
  PRIMARY KEY(locale, card_id, orientation)
) STRICT;
CREATE TABLE single_card_reversal_mechanism(
  locale TEXT NOT NULL,
  card_id TEXT NOT NULL,
  orientation TEXT NOT NULL,
  ordinal INTEGER NOT NULL CHECK(ordinal >= 0),
  mechanism TEXT NOT NULL CHECK(mechanism IN ('blocked','delayed','internalized','excessive','distorted','resisted','depleted')),
  PRIMARY KEY(locale, card_id, orientation, ordinal),
  UNIQUE(locale, card_id, orientation, mechanism),
  FOREIGN KEY(locale, card_id, orientation) REFERENCES single_card(locale, card_id, orientation)
) STRICT;
CREATE TABLE single_card_tag(
  locale TEXT NOT NULL,
  card_id TEXT NOT NULL,
  orientation TEXT NOT NULL,
  ordinal INTEGER NOT NULL CHECK(ordinal >= 0),
  concept_id TEXT NOT NULL,
  valence INTEGER NOT NULL CHECK(valence BETWEEN -2 AND 2),
  intensity INTEGER NOT NULL CHECK(intensity BETWEEN 1 AND 3),
  PRIMARY KEY(locale, card_id, orientation, ordinal),
  UNIQUE(locale, card_id, orientation, concept_id),
  FOREIGN KEY(locale, card_id, orientation) REFERENCES single_card(locale, card_id, orientation),
  FOREIGN KEY(locale, concept_id) REFERENCES vocabulary(locale, concept_id)
) STRICT;
CREATE TABLE oriented_pair(
  locale TEXT NOT NULL REFERENCES declared_locale(locale),
  card_a_id TEXT NOT NULL,
  card_b_id TEXT NOT NULL CHECK(card_a_id < card_b_id),
  orientation_state TEXT NOT NULL CHECK(orientation_state IN ('upright-upright','upright-reversed','reversed-upright','reversed-reversed')),
  interaction TEXT NOT NULL CHECK(length(trim(interaction)) > 0),
  direction TEXT NOT NULL CHECK(length(trim(direction)) > 0),
  overall_valence INTEGER NOT NULL CHECK(overall_valence BETWEEN -2 AND 2),
  overall_intensity INTEGER NOT NULL CHECK(overall_intensity BETWEEN 1 AND 3),
  PRIMARY KEY(locale, card_a_id, card_b_id, orientation_state)
) STRICT;
CREATE TABLE oriented_pair_tag(
  locale TEXT NOT NULL,
  card_a_id TEXT NOT NULL,
  card_b_id TEXT NOT NULL,
  orientation_state TEXT NOT NULL,
  ordinal INTEGER NOT NULL CHECK(ordinal >= 0),
  concept_id TEXT NOT NULL,
  valence INTEGER NOT NULL CHECK(valence BETWEEN -2 AND 2),
  intensity INTEGER NOT NULL CHECK(intensity BETWEEN 1 AND 3),
  PRIMARY KEY(locale, card_a_id, card_b_id, orientation_state, ordinal),
  UNIQUE(locale, card_a_id, card_b_id, orientation_state, concept_id),
  FOREIGN KEY(locale, card_a_id, card_b_id, orientation_state) REFERENCES oriented_pair(locale, card_a_id, card_b_id, orientation_state),
  FOREIGN KEY(locale, concept_id) REFERENCES vocabulary(locale, concept_id)
) STRICT;
CREATE TABLE three_card_position(
  locale TEXT NOT NULL REFERENCES declared_locale(locale),
  position TEXT NOT NULL CHECK(position IN ('past','present','future')),
  card_id TEXT NOT NULL,
  orientation TEXT NOT NULL CHECK(orientation IN ('upright','reversed')),
  text TEXT NOT NULL CHECK(length(trim(text)) > 0),
  overall_valence INTEGER NOT NULL CHECK(overall_valence BETWEEN -2 AND 2),
  overall_intensity INTEGER NOT NULL CHECK(overall_intensity BETWEEN 1 AND 3),
  PRIMARY KEY(locale, position, card_id, orientation)
) STRICT;
CREATE TABLE three_card_position_tag(
  locale TEXT NOT NULL,
  position TEXT NOT NULL,
  card_id TEXT NOT NULL,
  orientation TEXT NOT NULL,
  ordinal INTEGER NOT NULL CHECK(ordinal >= 0),
  concept_id TEXT NOT NULL,
  valence INTEGER NOT NULL CHECK(valence BETWEEN -2 AND 2),
  intensity INTEGER NOT NULL CHECK(intensity BETWEEN 1 AND 3),
  PRIMARY KEY(locale, position, card_id, orientation, ordinal),
  UNIQUE(locale, position, card_id, orientation, concept_id),
  FOREIGN KEY(locale, position, card_id, orientation) REFERENCES three_card_position(locale, position, card_id, orientation),
  FOREIGN KEY(locale, concept_id) REFERENCES vocabulary(locale, concept_id)
) STRICT;
CREATE TABLE synthesis_resource(
  locale TEXT NOT NULL REFERENCES declared_locale(locale),
  resource_type TEXT NOT NULL CHECK(resource_type IN ('three-card-position','trajectory-profile','synthesis-fragment','relation-label')),
  resource_id TEXT NOT NULL,
  canonical_json TEXT NOT NULL CHECK(json_valid(canonical_json)),
  PRIMARY KEY(locale, resource_type, resource_id)
) STRICT;
```

## Compiler CLI

```text
interpretation-pack validate-source --source-root PATH [--json]
interpretation-pack compile --source-root PATH --output PATH [--check] [--json]
interpretation-pack inspect-package --package PATH [--json]
interpretation-pack authoring-status --source-root PATH [--locale LOCALE --corpus CORPUS] [--json]
interpretation-pack audit-content --source-root PATH --locale LOCALE --corpus CORPUS [--json]
```

Scoped `authoring-status` accepts `single-card`, `oriented-pairs`, `three-card-positions`, or `three-card-synthesis`. Bundle corpora report expected/present/missing bundle and state counts; synthesis reports exact expected/present/missing resources. Both return the complete ordinal `missingIdentities` inventory in JSON. The unscoped form retains the aggregate validation/status report. Missing targets in a not-ready corpus are normal authoring state; malformed, duplicate, noncanonical, or out-of-inventory source remains an error. Console inventories and diagnostics are bounded, while JSON is complete and deterministic.

`audit-content` requires one declared locale and one corpus. It reuses the strict source loader, stops on structural errors, and then performs deterministic offline lexical heuristics over canonical source, including the localized `text` payload of every synthesis resource: normalized exact duplicates; indexed word-trigram Jaccard near-duplicates; single-card orientation and intra-state section similarity; repeated openings, endings, and advice/outcome formulas; robust token-length outliers; RU Latin-script leakage; and factual tag, valence, intensity, and reversal-mechanism distributions where those metadata exist. Near-duplicate candidates come from a bounded inverted shingle index rather than an unconditional all-pairs scan; reports expose possible and candidate comparison counts. Thresholds are named code constants. Findings are stable warnings and do not change exit `0`; structural/execution errors return `1`, and CLI usage errors return `2`. Reports contain no timestamps, random IDs, or absolute source paths, and no tracked audit/progress artifact is created.

Compilation validates source, creates a normalized immutable model, calculates the digest, writes a sibling temporary database, creates/inserts rows in ordinal order, enforces constraints, verifies package identity/integrity, closes it, and atomically replaces the target. Failure leaves no partial final package. Temporary-path randomness is not semantic database data.

`--check` writes nothing. It opens the existing package and verifies identity, schema/integrity, metadata, source digest, and semantic row inventories against current source. `inspect-package` returns controlled metadata and counts. No command destructively overwrites accepted source content.

## Project and dependency boundary

```text
NoxAeterna.App ───────────────┐
                              v
NoxAeterna.Interpretation.Sqlite → NoxAeterna.Interpretation → NoxAeterna.Domain
                              ^
NoxAeterna.Tools.Repository ──┘
```

`NoxAeterna.Interpretation.Sqlite` owns schema, validated writer, read-only store, metadata inspection, and SQL-specific checks. It references `Microsoft.Data.Sqlite` and never references App, Presentation, Avalonia, repository tooling, or mutable user persistence. Interpretation owns semantic contracts and resolver behavior without SQL. Presentation/App never issue SQL.

## Runtime store and resolver

`ITarotInterpretationPackStore` exposes validated metadata, display names, locales/modules/dependencies, labels, vocabulary, exact single/pair/position lookups, and synthesis resources. `ITarotInterpretationPackStoreCatalog` resolves registered immutable packages. SQL does not cross this boundary.

The resolver retains pack selection, locale chain, same-locale dependency resolution, canonical pair handling, and typed `Resolved`/`NoContent`. Missing rows or query/package damage in a ready module produce `BrokenReadyModule` and stop locale fallback. No ready locale produces `NoReadyLocale`; unknown/missing package produces `PackUnavailable`.

Source and SQLite ready-module validation require the exact mode-owned synthesis inventory and reparse every stored canonical payload through the typed text contract. SQLite schema v1 remains sufficient: `canonical_json` stores the validated payload without moving classifier rules into data or changing DDL.

The playable workspace keeps a narrow App adapter over that resolver. `single-card` resolves after its assignment is revealed; `two-cards` resolves exactly one unordered oriented-pair entry only after both technical draw slots are revealed; `three-cards` continues to use semantic position entries when their corpus becomes ready. Presentation converts resolved entries and pack-local vocabulary into typed UI models, so Avalonia receives localized tag labels and authored valence/intensity rather than storage DTOs or raw concept IDs. Pair section chrome (`interaction` and `direction`) is selected with the resolved interpretation locale, including locale fallback.

Only bounded semantic entry caching is permitted. Cache identity uses pack ID, content version/source digest, resolved locale, corpus/mode, and canonical key—not artwork, theme, dimensions, or display name.

## Build integration

App build/publish compiles the built-in Classic source to:

```text
<output>/resources/interpretation/tarot/packs/classic.noxinterp
```

The target runs for direct App and solution Debug/Release builds, needs no pre-existing package, and regenerates when canonical source changes. Repository source JSON, working content, and TestData are not copied. Tooling may execute at build time but is not a runtime App dependency or output payload. Generated `.noxinterp` files are ignored and untracked.

## INT-SQL1 checkpoints

- A: manifest v2, bundle contracts, labels, stateless authoring rules, validator/inventory, compiler, package inspection, deterministic digest, and build generation must be green.
- B: replace filesystem/index resolver and catalog with SQLite stores, preserve product semantics/UI, then delete the obsolete runtime JSON routing, hashes, caches, commands, contracts, and tests.

No production Tarot prose is created in INT-SQL1. After hosted acceptance, the next stage is one autonomous Russian Classic single-card authoring/QA wave covering all 78 bundles / 156 states.
