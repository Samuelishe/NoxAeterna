# Tarot Interpretation Modes

| Metadata | Definition |
| --- | --- |
| Role | Canonical owner for Tarot mode identities, bundle granularity, corpus inventories, source layout, composition, and authoring-wave scope. |
| Read when | Changing single/two/three-card semantics, source paths, pair identity, bundle membership, inventories, or authoring scope. |
| Authoritative for | Frozen mode IDs; source taxonomy; bundle units; canonical pairs; exact state inventories; large-wave policy; three-card relation model; no-wave-path rule. |
| Not authoritative for | Editorial voice, manifest/SQLite serialization, runtime fallback, UI styling, or spread drawing rules. |

## Frozen modes

Stable mode IDs are `single-card`, `two-cards`, `three-cards`, and `celtic-cross`. The current app exposes single-card behavior; later modes use the contracts here without silently redefining identity.

## Canonical authoring tree

```text
resources/interpretation/tarot/sources/<pack-id>/
├── interpretation-pack.json
└── content/
    ├── ru/
    │   ├── labels.json
    │   ├── vocabulary/<concept-id>.json
    │   ├── single-card/<card-id>.json
    │   ├── oriented-pairs/<card-a-id>__<card-b-id>.json
    │   ├── three-card-positions/<card-id>.json
    │   └── synthesis/<resource-type>/<resource-id>.json
    └── en/
        └── same structure
```

The separate non-shipped draft root is `resources/interpretation/tarot/working/<pack-id>/`. Wave, batch, suit, Arcana, or progress names never appear as extra canonical source directories. In particular, paths such as `wave-3/`, `batch-04/`, `major-pairs/`, or `wands-cups/` are not canonical.

## Vocabulary and labels

Vocabulary remains one concept per JSON file. `content/<locale>/labels.json` is one trusted locale bundle containing exactly:

- single-card section IDs: `situation`, `development`, `risk`, `outcome`, `advice`;
- position IDs: `past`, `present`, `future`;
- relation IDs: `past-present`, `present-future`, `overall`.

Labels are compiled with content and resolve in the same locale. Tag labels come from same-locale vocabulary.

## Single-card bundle

One semantic card is one JSON file. It owns exactly `upright` and `reversed`; each state independently owns five sections, tags, overall valence/intensity, and reversal mechanisms.

```json
{
  "schemaVersion": 1,
  "cardId": "major.fool",
  "states": {
    "upright": { "sections": {}, "tags": [], "overallValence": 1, "overallIntensity": 2, "reversalMechanisms": [] },
    "reversed": { "sections": {}, "tags": [], "overallValence": -1, "overallIntensity": 2, "reversalMechanisms": ["blocked"] }
  }
}
```

A valid accepted bundle is never partial. Complete inventory per locale: 78 files and 156 semantic states.

## Oriented-pair bundle

Two-card mode draws distinct cards without replacement and has no positional roles. The unordered pair identity is canonicalized by ordinal semantic card ID: `cardAId < cardBId`. Orientation remains attached to canonical A/B slots.

One canonical pair is one JSON file containing exactly:

- `upright-upright`;
- `upright-reversed`;
- `reversed-upright`;
- `reversed-reversed`.

Each state independently owns `interaction`, `direction`, tags, overall valence, and overall intensity. It is not mechanically assembled from single-card text. Self-pairs, reversed canonical order, partial bundles, and extra states are invalid.

Complete inventory per locale: `C(78,2) = 3003` files and `3003 × 4 = 12012` semantic states.

## Three-card position bundle and synthesis

One semantic card is one position JSON file. It contains exactly the cross-product:

```text
past/upright       past/reversed
present/upright    present/reversed
future/upright     future/reversed
```

Each state independently owns its text, tags, overall valence, and overall intensity. Complete inventory per locale: 78 files and `78 × 3 × 2 = 468` semantic states.

Three-card interpretation composes three revealed position entries, two adjacent oriented-pair relations (`past-present`, `present-future`), and an `overall` synthesis resource. Progressive reveal never leaks hidden-card meaning. No exhaustive triple-text corpus is authored.

## Authoring-wave policy

An entire Russian Classic single-card corpus may be authored in one autonomous run: 78 bundles / 156 states. An entire Russian three-card-position corpus may likewise be authored in one run: 78 bundles / 468 states.

The 12,012 oriented-pair states are split into several large work waves of approximately 1,500–2,500 states. All four states of a canonical pair stay in one wave. The authoring prompt supplies exact canonical first-card/range boundaries, and the completed scope is recorded in `SESSION-LOG`.

Waves are session work scope only. They do not alter source/runtime identities or paths, and they require no separate plan JSON. Every wave begins by inspecting and validating the actual source tree, preserves valid existing bundles, writes only requested missing identities, and ends with zero missing/duplicate/invalid/noncanonical identities within the wave.

## Validation and readiness

Every existing source file must be byte-canonical and internally complete even while its module is not ready. Validation covers taxonomy, safe paths, filename/identity agreement, card IDs, exact state membership, canonical pair ordering, unique semantic identities, labels, vocabulary, locale purity, tag/metric ranges, and synthesis contracts.

When `ready = true`, exact same-locale inventory and dependencies are mandatory:

- single-card: 78 bundles / 156 states plus labels and used vocabulary;
- two-cards: 3003 pair bundles / 12012 states plus labels and used vocabulary;
- three-cards: the pair inventory, 78 position bundles / 468 states, synthesis resources, labels, and used vocabulary.

When `ready = false`, a corpus may be incomplete, but every file that exists must be a complete valid bundle. Readiness remains a deliberate manifest decision; neither file presence nor compilation promotes it.

## Runtime relationship

The source tree is compiler input, not runtime routing. Compilation normalizes bundle states into indexed SQLite rows in one `.noxinterp` file. Runtime performs exact semantic key lookups and does not know source filenames, authoring waves, or repository layout. Serialization and database details belong to [TAROT-INTERPRETATION-IMPLEMENTATION.md](TAROT-INTERPRETATION-IMPLEMENTATION.md).
