# Tarot Interpretation Packs

| Metadata | Definition |
| --- | --- |
| Role | Canonical product owner for Tarot interpretation-pack identity, locale selection, readiness, fallback, and distribution. |
| Read when | Changing pack metadata, selector behavior, locale fallback, readiness, or package discovery. |
| Authoritative for | `TarotInterpretationPackId`; Classic identity; semantic deck; source and declared locales; display names; mode readiness; fallback and broken-ready behavior; built-in/user package direction; `.noxinterp` distribution. |
| Not authoritative for | Prose style, bundle JSON schemas, SQLite DDL, resolver implementation, or authoring wave boundaries. |

## Identity

An interpretation pack is a versioned semantic corpus for one semantic deck. It is independent from artwork, card backs, theme, UI language, and spread layout.

The first pack is fixed as:

- pack ID: `classic`;
- semantic deck ID: `standard-78`;
- source locale: `ru`;
- declared direction: Russian and English;
- manifest-owned display names: `Классика` and `Classic`.

`TarotInterpretationPackId` remains the stable selection identity. Settings schema 2 stores `selectedInterpretationPackId`; interpretation language remains independent from UI language. The selector remains `Толкование / Interpretation`, and its Classic item uses the display name stored in the package rather than a generic UI localization key.

## Source and runtime are different artifacts

The authoritative pipeline is:

```text
Markdown / RAG rules
        ↓
canonical reviewable JSON authoring source
        ↓
strict validation + compilation
        ↓
one immutable SQLite .noxinterp package
        ↓
runtime resolver
```

The Git source of truth is `resources/interpretation/tarot/sources/<pack-id>/`. A distributable or built-in runtime pack is one `<pack-id>.noxinterp` file. The database is generated, is never manually edited, and cannot replace source review. Repository source JSON, working files, TestData, and authoring metadata are not copied beside the application.

The separate draft root is `resources/interpretation/tarot/working/<pack-id>/`. Runtime packages never live under either authoring root. A future user-installed pack is likewise one `.noxinterp` file after trust and compatibility checks; this stage does not implement user installation.

## Locale readiness

Readiness is one explicit Boolean per `pack + locale + mode`. There is no per-entry readiness and no inferred promotion. Every declared locale has every frozen mode declaration:

- `single-card`;
- `two-cards`;
- `three-cards`;
- `celtic-cross`.

Dependencies are same-locale:

- `single-card`: none;
- `two-cards`: `oriented-pairs`;
- `three-cards`: `oriented-pairs`, `three-card-positions`, `three-card-synthesis`;
- `celtic-cross`: none until its future contract is frozen.

Classic declares `ru + single-card` and `ru + two-cards` as `ready = true`. The other six Russian/English mode combinations remain `ready = false`; labels in an unready locale do not imply prose readiness.

## Locale resolution

For an explicitly requested interpretation locale, the resolver tries the unique chain:

```text
requested → en → ru → silent absence
```

Duplicates are removed while preserving order. UI language does not silently replace the interpretation-language choice.

For each locale in the chain:

1. inspect the selected pack and requested mode;
2. if the module is not ready, continue;
3. if it is ready, resolve the entire request in that locale, including dependencies, labels, vocabulary, and content;
4. return typed `Resolved` on success;
5. if a ready module is incomplete, corrupt, or invalid, return typed `NoContent/BrokenReadyModule` and stop.

Thus a broken ready locale never falls through to another locale. If no locale is ready, return `NoContent/NoReadyLocale`. An unknown, missing, or rejected package returns `NoContent/PackUnavailable`. Expected package damage never escapes as a user-facing technical error; the host remains silent.

## Package trust

A runtime catalog admits only packages that pass package-level checks: `.noxinterp` extension, SQLite format, project `application_id`, `user_version`, required schema, one metadata row, expected pack/deck identity, a lower-case 64-hex source digest, and an appropriate SQLite integrity check. Runtime connections are read-only and query-only.

Pack-local visible section/position/relation labels and vocabulary labels resolve from the same locale as the content. Raw semantic IDs are never a user-facing fallback.

## Ownership links

- Prose, research, translation, tags, and stateless Codex workflow: [TAROT-INTERPRETATION-CONTENT.md](TAROT-INTERPRETATION-CONTENT.md).
- Bundle granularity, exact inventories, pair identity, and authoring waves: [TAROT-INTERPRETATION-MODES.md](TAROT-INTERPRETATION-MODES.md).
- Manifest v2, source digest, SQLite DDL, compiler, store, and build integration: [TAROT-INTERPRETATION-IMPLEMENTATION.md](TAROT-INTERPRETATION-IMPLEMENTATION.md).
- Resolver and typed-result layering: [INTERPRETATION-ENGINE.md](INTERPRETATION-ENGINE.md).
