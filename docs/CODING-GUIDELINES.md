# Coding Guidelines

These guidelines apply once application code begins.

## General Style

- Use readable, intention-revealing names.
- Keep methods and classes focused.
- Prefer explicit types and value objects for domain concepts.
- Use XML documentation for public APIs.
- Avoid hidden global state.
- Avoid clever code where simple code is clearer.
- Keep commits and pull requests small and reviewable.

## Boundaries

- UI must not calculate astronomy.
- UI must not directly query SQLite.
- Astronomy must not depend on Avalonia.
- Geometry must not depend on Avalonia UI controls.
- Interpretation must not depend on UI.
- Rendering must not consume raw business logic when a prepared geometry/rendering model is appropriate.
- Domain should not depend on infrastructure packages.

## Errors

- Avoid broad catch blocks.
- Do not swallow calculation, timezone, ephemeris, or persistence errors.
- Prefer typed results or specific exceptions where failure is expected and meaningful.
- Preserve diagnostic context for logs.

## Tests

Add tests early for:

- Longitude normalization.
- Zodiac sign derivation.
- Aspect delta and orb calculation.
- Time conversion and ambiguous/invalid local time handling.
- Geometry math.
- Interpretation composition rules.
- Repository behavior once persistence exists.

## Abstractions

Add abstractions when they protect real boundaries:

- Ephemeris calculation.
- Timezone lookup.
- Persistence.
- Rendering adapters.
- Randomness for Tarot selection.

Avoid abstractions that only rename a single implementation without clarifying a boundary.

## Documentation

When behavior changes, update relevant docs in the same session. Record major decisions in `DECISIONS-LOG.md`.
