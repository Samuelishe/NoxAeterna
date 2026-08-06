# Persistence

Persistence is important from early stages because the core UX depends on history, archive, saved profiles, saved readings, and long-term symbolic records.

## Target Direction

- SQLite database.
- Lightweight repository/data access layer.
- Dapper preferred unless there is a strong reason for EF Core.
- Migrations should be considered after initial schema shape becomes clearer.
- Domain models should not be polluted by database implementation details.

## Local Runtime Data Rule

All local or user-specific runtime data must eventually live under the user AppData directory or the equivalent platform-specific user data location.

Do not:

- store runtime state next to the executable;
- store runtime state in the repository;
- commit user-specific settings, recent places, caches, generated local data, or personal profile data to GitHub.

AppData-only data includes:

- user preferences;
- saved birth profiles;
- recent or saved birth places;
- geocoding cache;
- manually entered locations;
- local interpretation history;
- Tarot reading history;
- generated or cached local assets where applicable.

## Repository-Owned App Assets

The versioned source for application-owned resources that ship with the product belongs in the repository. Installed built-in seed copies, user packs, normalized caches, and pack-validation state live under AppData according to [`ASSET-PACK-RUNTIME.md`](ASSET-PACK-RUNTIME.md); this runtime copy does not transfer source ownership to AppData.

Examples:

- app icons;
- UI icons;
- theme resource dictionaries;
- curated astrology or Tarot visual assets;
- custom glyph resources if they are later introduced;
- textures, ornaments, and other shipped decorative assets.

Keep the ownership distinction clear:

- repository = product-owned, versioned, reproducible source and installation seed inputs;
- AppData = resolved runtime state, managed seed copies, caches, preferences, saved content, and user-provided packs.

This document owns the general platform-data rule. Pack discovery, synchronization, no-delete behavior, fingerprints, and import are not persistence contracts and belong only to the asset-pack runtime owner.

Future installed Tarot interpretation packs use their own `<LocalApplicationData>/NoxAeterna/interpretation/tarot/` root, independent from artwork-pack directories. Repository source and built-in/user runtime roots belong to [`TAROT-INTERPRETATION-MODES.md`](TAROT-INTERPRETATION-MODES.md); exact schemas, layers, and staged implementation belong to [`TAROT-INTERPRETATION-IMPLEMENTATION.md`](TAROT-INTERPRETATION-IMPLEMENTATION.md). Interpretation seeding/import does not inherit artwork-pack behavior automatically.

## Responsibilities

Persistence should eventually store:

- Personal profiles.
- Birth data and resolved birth moments.
- Natal chart snapshots or calculation references.
- Saved Tarot sessions.
- Saved interpretations.
- Transit snapshots.
- Lunar event notes.
- Symbolic history entries.
- User notes and archive entries.

## Repository Direction

Repositories should expose intent-focused operations and avoid leaking SQL or database tables into UI code.

Example future repository categories:

- Profile repository.
- Reading repository.
- Interpretation archive repository.
- Symbolic history repository.

## Domain Separation

Keep database concerns out of domain objects. Avoid attributes or persistence-only constructors in domain types unless a later decision justifies them.

Mapping belongs in infrastructure or data access code.

## Migrations

Migration strategy is deferred. It should be added before schema churn becomes risky.

Options to evaluate later:

- Lightweight custom SQL migrations.
- DbUp.
- FluentMigrator.
- EF Core migrations only if EF Core is adopted for a strong reason.

Record the migration decision in `DECISIONS-LOG.md`.

## Implemented Settings Persistence

The App composition boundary owns a narrow `System.Text.Json` adapter at the logical platform path `<LocalApplicationData>/NoxAeterna/settings.json` (on Windows, `%LOCALAPPDATA%\NoxAeterna\settings.json`). Presentation owns only typed immutable preferences; it does not use file, environment, or JSON APIs. Domain and Infrastructure do not participate.

Schema version `1` persists:

- application and interpretation language IDs;
- theme ID;
- Tarot spread, artwork-pack, and back-variant IDs;
- Tarot reversal and auto-reveal booleans.

The document does not persist a reading, drawn or revealed cards, selection, interpretation, random state, timestamps, bitmap/cache state, failures, navigation, resize, scroll offset, profile, or history data.

Missing files are normal first-run state and return `ru` / `ru` / `dark` plus `single-card` / `lupus-noctis` / `black-sun` / reversed `false` / auto reveal `true`, without a diagnostic or write. Malformed JSON, unsupported schema, and read failures return controlled defaults with structured diagnostics. For a supported document, invalid string IDs are normalized independently so other valid fields and booleans survive.

An actual preference change updates one App-owned immutable root snapshot and triggers one save attempt. Draw, redraw, reveal, selection, navigation, resizing, control recreation, and bitmap loading do not save. Writes create the directory, serialize to a same-directory temporary file, flush and close it, then replace/move the final file; controlled failure does not crash the application and performs best-effort temporary cleanup.

Theme is loaded and applied before MainWindow is created. A DEBUG-only injected AppData root supports isolated real-control smoke without reading or modifying the real user file.

## Deferred Persistence Areas

Birth-data input, profiles, saved readings, reading history, interpretations, archive data, SQLite, repositories, and a migrations framework remain deferred. The settings JSON foundation does not select the later SQLite design.

A future saved Tarot reading should be able to retain interpretation pack ID, content version, mode ID, requested/resolved interpretation locales, and semantic card IDs/orientations for provenance. Whether it also archives rendered prose remains an unresolved persistence/history decision.

## Approved Settings Schema 2 Direction

The first pack-selection implementation migrates settings schema 1 to schema 2 and adds stable semantic field `selectedInterpretationPackId`, nested according to the existing DTO. It defaults to `classic`, restores at startup, and changes when the user selects another pack. Version 1 loads and normalizes in memory; startup does not rewrite merely for migration, while the next actual preference save writes version 2. An unknown ID resolves to `classic` when available; with no available pack, cards remain usable and interpretation remains empty. No migration message is shown.

Settings persist only the selected stable ID: current interpretation text is not settings state, and resolved fallback locale is runtime provenance rather than a user preference. Pack selection/fallback belongs to [`TAROT-INTERPRETATION-PACKS.md`](TAROT-INTERPRETATION-PACKS.md); exact migration and selector gates belong to [`TAROT-INTERPRETATION-IMPLEMENTATION.md`](TAROT-INTERPRETATION-IMPLEMENTATION.md).

Settings also gains two shared actions:

- **`Сбросить настройки` / Reset settings:** one application-wide button restores all application preferences from program defaults. It has a normal confirmation step and no separate reset buttons per section. It does not delete artwork packs, user content, or saved readings without another explicit decision.
- **`Открыть папку данных приложения` / Open application data folder:** opens the `<LocalApplicationData>/NoxAeterna` root, not only `settings.json`. This action already belongs to the AP1 plan in [`ASSET-PACK-RUNTIME.md`](ASSET-PACK-RUNTIME.md#completion-gates-and-staged-roadmap); this settings direction links to that plan rather than creating a competing implementation stage.

Neither action is implemented by INT0-D4 or required for INT0-I1.
