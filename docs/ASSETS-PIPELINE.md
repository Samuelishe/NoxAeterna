# Assets Pipeline

The project may use AI-assisted visual asset generation, but the final application must feel curated, not like a random folder of generated images.

This document owns creation, curation, provenance, and repository-ready source assets. Runtime discovery, AppData seeding, user packs, normalization, validation state, tooling, and packaging belong to [`ASSET-PACK-RUNTIME.md`](ASSET-PACK-RUNTIME.md).

## Repository vs Runtime Data

Project-owned application assets belong in the repository and GitHub.

This includes:

- app icons;
- UI icons;
- custom glyph resources if they are later adopted;
- chart ornaments;
- theme resources;
- textures and backgrounds;
- Tarot card backs;
- Tarot card illustrations;
- curated generated imagery that is actually shipped with the app.

User-specific or runtime data does not belong in the repository.

This includes:

- settings;
- saved profiles;
- saved charts;
- recent places;
- geocoding cache;
- local history;
- generated user-specific files.

Those belong in AppData or the equivalent platform-specific user data directory.

## Asset Strategy Decision

Programmatic rendering is the source of truth for technical visuals.

Programmatic rendering should be used for:

- Charts.
- Glyphs.
- Houses.
- Aspects.
- Technical visuals.
- Small icons.

Curated or generated assets may be used for:

- Tarot illustrations.
- Textures.
- Decorative engravings.
- Atmospheric imagery.

Generated AI imagery must never become the source of technical chart rendering.

Simple functional chart symbols should stay vector-first or programmatic whenever possible. Random image assets are not an acceptable substitute for chart glyphs, markers, rings, or aspect lines.

Current implementation uses 22 original project-owned functional path definitions in `NoxAeterna.Rendering`: 12 zodiac symbols and 10 planet symbols. They are monochrome programmatic graphics with stable unit bounds, introduce no external asset or font provenance, and remain intentionally simpler than a future artistic typeface.

## Intended Pipeline

1. Generate visual concepts.
2. Select and curate.
3. Clean manually where needed.
4. Vectorize or simplify when appropriate.
5. Normalize palette.
6. Store reusable assets.
7. Maintain a visual style guide.

Commit only curated, app-ready assets. Do not commit raw generation dumps or bulk output folders from image-generation workflows.

## Appropriate Generated Assets

AI-assisted assets may be useful for:

- Tarot card concept art.
- Atmospheric backgrounds.
- Large symbolic illustrations.
- Texture studies.
- Mood explorations.

Generated assets should be reviewed for style consistency, artifacts, unreadable text, anatomical issues, and inappropriate symbolism.

## Do Not Use Generated Images For

- Exact astrological diagrams.
- Glyphs.
- Small icons.
- Text inside images.
- UI layout.
- Technical chart rendering.

Astrological charts should be rendered programmatically.

## Tarot Art Scope

The full 78-card deck is a large art commitment. The first built-in partial pack proves the controlled scope without presenting an unfinished pack as complete:

- `Lupus Noctis` currently ships six owner-accepted `952 × 1632` (`7:12`) inner illustrations mapped by a versioned pack-local manifest;
- omitted semantic cards resolve to the honest programmatic prototype face while retaining their identity and localized overlay;
- programmatic frame, selection state, title, and reversal transform remain separate from the raster illustration;
- the pack owner document retains meaning briefs, full prompts, review decisions, hashes, and provenance.

Production filenames follow semantic identities. A pack may deliberately be partial, but every accepted entry must declare its card ID, package-relative path, dimensions, checksum, status, and provenance reference. Raw or rejected generations and contact sheets are not shipped.

## Style Guide Direction

Future style guide should define:

- Palette.
- Typography.
- Icon style.
- Glyph usage.
- Chart line weights.
- Texture usage.
- Tarot card framing.
- Background and surface rules.
- Export visual rules.

## Runtime Packaging Boundary

Repository-approved assets remain reviewable source material. The canonical pack structure, seed payload, runtime AppData layout, manifest loading, extension behavior, normalization, user import, and publish/installer contract are defined only in [`ASSET-PACK-RUNTIME.md`](ASSET-PACK-RUNTIME.md).

## Attribution Rule

Every future session that introduces assets, fonts, textures, icon sets, or generated imagery must document:

- Asset or source name.
- Author or origin.
- License or usage terms.
- Purpose in the project.
- Storage location or source link.

Track this in `README.md` and `docs/THIRD-PARTY.md`, and update project docs when the visual pipeline materially changes.
