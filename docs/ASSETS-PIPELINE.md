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

Production assets and bounded owner-review candidates are the only generated artwork classes permitted in the repository. Do not commit raw generation dumps or bulk output folders from image-generation workflows.

### Production Assets

Generated production artwork:

- lives only under the pack's canonical `cards/` tree;
- is listed in `artwork-pack.json`;
- has owner acceptance **Accepted**;
- is copied into the build and publish seed;
- is covered by loader and runtime tests;
- counts as a shipped asset.

The exact current production inventory belongs to `artwork-pack.json`.

### Tracked Review Candidates

A bounded review batch may be committed and pushed to GitHub under `studies/A<positive-batch-number>/` only when all of these conditions hold:

- exactly one retained final candidate exists for each semantic card in the batch;
- every candidate has a separate provenance record under `records/`;
- the record contains the exact candidate path, dimensions, SHA-256, generation count, and `Owner acceptance: **Pending**`;
- the candidate is absent from `artwork-pack.json` and does not live under `cards/`;
- the application project does not copy the candidate into build or publish output;
- rejected or superseded generations are not retained;
- no speculative variants, raw generation dumps, contact sheets, collages, or combined previews are retained.

After acceptance, move the candidate without duplication to its canonical production path and update its record and manifest status. After rejection, delete the candidate. An accepted production file and an identical study copy must never coexist.

This is a narrow exception for bounded owner review, not permission to retain bulk generation output.

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

The full 78-card Lupus Noctis deck is complete in the controlled repository structure:

- `Lupus Noctis` ships all 78 owner-accepted `952 × 1632` (`7:12`) production illustrations mapped by a versioned complete pack-local manifest; the exact current inventory belongs to `artwork-pack.json`;
- the complete pack has no omitted semantic cards and uses no partial-pack prototype fallback; the separate Classic prototype remains available;
- programmatic frame, selection state, title, and reversal transform remain separate from the raster illustration;
- provenance details belong to `LUPUS-NOCTIS.md` and its linked records, which retain meaning briefs, full prompts, review decisions, hashes, and generation history.

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
