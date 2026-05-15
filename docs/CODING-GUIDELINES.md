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
- Presentation must not normalize angles.
- Astronomy must not depend on Avalonia.
- Geometry must not depend on Avalonia UI controls.
- Geometry must remain Avalonia-independent.
- Interpretation must not depend on UI.
- Interpretation must not access SQLite directly.
- Symbolics must not contain user-facing prose generation.
- Rendering must not consume raw business logic when a prepared geometry/rendering model is appropriate.
- Rendering must consume prepared render models.
- Domain should not depend on infrastructure packages.
- Domain must remain language-neutral.

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
- Localization fallback and theme/preferences contracts before substantial UI work begins.
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

Prefer clarity over abstraction when a boundary is not yet proven.

## Documentation

When behavior changes, update relevant docs in the same session. Record major decisions in `DECISIONS-LOG.md`.

## User-Facing Text Rule

All future user-facing text must use localization keys.

Hardcoded strings are allowed only for:

- tests;
- diagnostics;
- logs;
- exceptions and internal debugging where appropriate.

Do not add helper methods such as `ZodiacSign.GetRussianName()` to domain types.

Do not translate the product name `Nox Aeterna` or other intended Latin proper names inside localization catalogs unless a specific product decision explicitly changes that rule.

Do not overload product-facing workspace titles or subtitles with temporary implementation disclaimers. Keep demo, fake, or development-only status in separate localized notices near the affected control or panel.

Temporary in-memory localization or settings bootstrapping is acceptable in `App` only while real loading and persistence are still absent. Do not let that bootstrap code spread into domain or infrastructure boundaries.

When JSON-backed localization exists, prefer catalog loading over inline bootstrap dictionaries. Keep the JSON shape simple and flat unless a stronger reason appears.

Theme switching should continue to flow through `ThemeId` and a small app-level theme controller or equivalent boundary. Do not reintroduce boolean dark-mode flags or scatter hardcoded product colors across shell controls when theme resources can carry them.

User-specific runtime data must not be stored in the repository or next to the executable. When persistence arrives, route preferences, caches, saved places, and profile data into AppData or the equivalent platform-specific user data directory.

Application-owned shipped assets belong in the repository. Do not route product icons, curated textures, Tarot art, theme resources, or future glyph assets into AppData just because they are visual files.

When using native desktop picker controls, favor readable layouts over tight packing. Do not force `DatePicker` or `TimePicker` into narrow multi-column arrangements that clip or overlap their segmented UI.

Prefer programmatic or vector-first chart visuals where possible. Do not add random internet images, unlicensed icon packs, or raw AI-generation dumps to solve basic chart readability problems.

## Attribution and Provenance

Every session that introduces external material must document:

- Package or asset name.
- Author or organization.
- License.
- Project purpose.
- Official repository or site.

This applies to libraries, frameworks, assets, fonts, datasets, ephemeris sources, borrowed code, adapted code, rendering systems, and generated assets when relevant.

Update `README.md` and `docs/THIRD-PARTY.md` in the same session.
