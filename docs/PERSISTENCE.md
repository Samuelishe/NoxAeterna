# Persistence

Persistence is important from early stages because the core UX depends on history, archive, saved profiles, saved readings, and long-term symbolic records.

## Target Direction

- SQLite database.
- Lightweight repository/data access layer.
- Dapper preferred unless there is a strong reason for EF Core.
- Migrations should be considered after initial schema shape becomes clearer.
- Domain models should not be polluted by database implementation details.

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
