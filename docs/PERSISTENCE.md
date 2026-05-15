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

Future AppData-only data includes:

- user preferences;
- saved birth profiles;
- recent or saved birth places;
- geocoding cache;
- manually entered locations;
- local interpretation history;
- Tarot reading history;
- generated or cached local assets where applicable.

## Repository-Owned App Assets

Application-owned resources that ship with the product belong in the repository rather than AppData.

Examples:

- app icons;
- UI icons;
- theme resource dictionaries;
- curated astrology or Tarot visual assets;
- custom glyph resources if they are later introduced;
- textures, ornaments, and other shipped decorative assets.

Keep the distinction clear:

- repository = product-owned, versioned, reproducible application resources;
- AppData = user-specific runtime state, caches, preferences, and saved content.

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

## Current Deferred Areas

Settings persistence is still deferred.

Current implemented state:

- language and theme preferences can exist in presentation and app memory;
- dark/light theme switching can already be applied in memory through `ThemeId`;
- birth-data input currently works in memory only and supports offline manual entry;
- no `settings.json` writing exists yet;
- no app-data path resolver exists yet;
- no infrastructure settings adapter exists yet.

Do not treat the current in-memory settings foundation as a persistence solution.
