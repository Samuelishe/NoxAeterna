# Asset-Pack Runtime

| Metadata | Definition |
| --- | --- |
| Role | Canonical architecture owner for discovering, installing, validating, normalizing, and resolving runtime asset packs. |
| Read when | Designing built-in asset delivery, AppData synchronization, user-pack discovery/import, pack diagnostics, asset tooling, or publish/installer handoff. |
| Authoritative for | Runtime asset-pack discovery, logical AppData layout, built-in seeding, user packs, manifest loading, validation state, extension fallback, normalized cache, placeholders, import workflow, asset tooling, and packaging handoff. |
| Not authoritative for | Artwork generation and curation, asset provenance, general persistence policy, Tarot semantic identities, card meanings, or UI visual composition. |

## Ownership and Scope

This document owns the target runtime architecture. [`ASSETS-PIPELINE.md`](ASSETS-PIPELINE.md) owns artistic generation, curation, provenance, and repository-ready source assets. [`PERSISTENCE.md`](PERSISTENCE.md) owns the general rule that runtime and user data belong under a platform user-data location. [`TAROT-ENGINE.md`](TAROT-ENGINE.md) owns semantic Tarot and workspace behavior.

AP0 records the target and audits A3; it does not add AppData access, a seeder, a registry, normalization, import UI, tooling, or installer logic. A3 continues to load the one built-in partial pack directly from the application output until the staged runtime work replaces that bridge.

## Source, Seed, and Runtime Model

Repository-owned built-in packs use this versioned source shape:

```text
resources/
  assets/
    tarot/
      artwork-packs/
        <pack-id>/
          artwork-pack.json
          <PACK-OWNER>.md
          cards/
          backs/
```

The repository tree is the reviewable source for built-in packs. Build and publish place the required pack content into an application seed payload; the installed product does not treat an arbitrary repository checkout as its runtime registry.

The platform-neutral logical runtime layout is:

```text
<AppData>/NoxAeterna/
  assets/
    tarot/
      packs/
        built-in/
          <pack-id>/
        user/
          <pack-id>/
      cache/
        normalized/
          <pack-id>/
  state/
    asset-packs.json
    pack-validation.json
```

A dedicated app-data path service must provide the concrete Windows, Linux, and macOS locations. Product runtime discovery ultimately reads packs from these AppData roots. Callers must not construct platform paths, use repository paths, or write beside the executable.

## Built-In Seeding and Synchronization

On first run, built-in seed packs are copied into `packs/built-in/`. On application updates, synchronization adds missing managed files and replaces changed managed files according to source manifest content and hashes.

The synchronizer must:

- prepare changes in staging and publish them through an atomic replacement appropriate to the platform;
- never modify `packs/user/`;
- never perform a destructive directory mirror;
- never automatically delete unknown or additional AppData files;
- tolerate a missing optional asset and leave per-card fallback to runtime resolution;
- keep Classic prototype available when seeding or built-in discovery fails;
- report controlled diagnostics without preventing application startup.

An explicit `sync-builtins --no-delete` CLI command may support diagnostics and repair later. It is an auxiliary operation, not a command that every Codex session or normal application launch must invoke separately; ordinary application startup owns required idempotent synchronization.

## Canonical Pack Structure and Names

Semantic identity determines the canonical relative stem. Examples:

```text
cards/major/death.png
cards/major/star.png
cards/minor/cups/six.png
backs/default.png
```

Resolution performs no fuzzy filename matching, typo correction, recursive guessing, alias inference, or nearest-name fallback. Folder segments and filename stem must match the canonical pack contract exactly. A different extension may be considered only for the same exact relative stem.

The decoder capability audit will test this candidate input order:

1. `.png`
2. `.webp`
3. `.jpg`
4. `.jpeg`
5. `.bmp`
6. `.gif` — first frame only

This list is an audit order, not a support claim. A format becomes supported only after real Avalonia/Skia decoder capability, limits, cross-platform behavior, and tests are recorded. The canonical normalized runtime format is PNG.

## Per-Card Resolution and Failure Behavior

For an exact semantic card stem, runtime resolution follows this order:

1. Use the exact PNG when it exists and passes minimum safety checks.
2. If PNG is absent, probe only confirmed supported extensions for the same exact stem.
3. If a supported non-PNG decodes, use or create its PNG representation in the normalized cache.
4. If no file exists, decoding fails, or safety limits reject it, use the programmatic placeholder.

Missing or invalid artwork never changes the semantic card identity or reading. A malformed whole pack is diagnosed and may be disabled as a selectable pack, but the application does not crash and Classic prototype remains available. There is no fuzzy fallback.

## Validation and Fingerprint State

A user-editable manifest must never contain a trusted `validated = 1` flag. Validation evidence belongs separately in `state/pack-validation.json` and is invalidated by source changes.

The planned validation record contains:

- pack ID;
- manifest hash;
- inventory fingerprint;
- validation schema version;
- normalized-cache version;
- last successful validation timestamp;
- bounded diagnostics summary.

A quick fingerprint includes manifest bytes or hash plus canonical relative paths, file sizes, and last-write timestamps. Deep SHA validation runs on first discovery, after the quick fingerprint changes, or on explicit user/tool validation. When the fingerprint is unchanged and schema/cache versions still match, runtime skips a complete scan and rehash.

Fingerprint state is a performance hint, not authority to bypass path containment, decode limits, or controlled error handling.

## Non-PNG Normalization

### Import wizard

The future wizard copies the selected folder into staging inside AppData. Confirmed supported non-PNG inputs are converted to PNG there. Before Save, the user sees the original/result preview and chooses:

- **Fit / Contain** — preserve the full image with remaining space;
- **Fill / Crop** — fill the target bounds with an explicit crop;
- **Stretch** — available only if later justified and never the recommended default.

Save validates and atomically publishes the staged pack. Cancel removes only that wizard-owned staging area.

### Manual drop-in

A user may copy a pack folder directly into `packs/user/`. Wizard normalization is not required. Runtime performs minimum safety checks, searches confirmed extensions by exact stem, and silently creates a PNG in `cache/normalized/<pack-id>/` after successful non-PNG decoding. It never overwrites the user's source file.

Decode or normalization failure produces the programmatic placeholder and a diagnostic rather than a crash. A manual manifest may relax completeness rules, but it can never bypass path-traversal protection, decode/resource limits, or controlled error handling.

## User-Pack Settings Workflow

The later Settings entry is **Settings → Tarot artwork packs**. Its planned flow is:

1. Choose **Add** and select a folder.
2. Copy into AppData staging.
3. Inspect manifest and canonical structure.
4. Show image-size and decode diagnostics.
5. Preview original and normalized result.
6. Choose Fit/Contain or Fill/Crop.
7. Normalize confirmed inputs to PNG.
8. Save through atomic publication or Cancel and remove staging.
9. Register the resulting user pack.

Settings also needs an **Open application data folder** action, pack diagnostics, and unregister/remove behavior. Unregistering must not delete source files by default; deletion requires explicit confirmation and exact scope. No workflow silently deletes unknown files.

## Asset Tooling

Asset operations belong in a separate executable named `NoxAeterna.Tools.Assets`, not a giant `NoxAeterna.Tools.CLI` and not `NoxAeterna.Tools.Repository`.

Planned commands are:

- `validate-pack`;
- `inspect-pack`;
- `fingerprint-pack`;
- `normalize-pack`;
- `sync-builtins --no-delete`;
- later, `package-pack`.

Runtime and tool must reuse manifest, containment, validation, fingerprint, and normalization logic rather than implement divergent rules. Reusable code should enter an existing appropriate layer when ownership is clear; a new shared project is justified only by demonstrated dependency and reuse evidence. AP0 creates no executable or project.

## Development, Publish, and Installer Handoff

Development and packaged applications use the same seed contract.

- **Development:** the project/build copies repository built-in packs into the app output seed directory; startup seeds AppData, so another checkout/build carries every missing built-in resource in its output.
- **Publish/installer:** the same seed payload is included in publish/install output; first run synchronizes it into AppData, and updates add or replace only managed built-in files.
- **Both:** unknown AppData files are preserved, user packs are untouched, and Classic remains available after controlled seed failure.

PKG1 must verify seed inclusion, first-run synchronization, update behavior, path service results, permissions, atomic replacement, and no-delete guarantees on Windows, Linux, and macOS.

## Completion Gates and Staged Roadmap

The full Lupus Noctis artwork pack does not depend on the future AppData subsystem. Further meaning-first production already has the required foundation: stable semantic IDs, canonical folders, manifest schema, canonical owner document, and explicit owner-acceptance workflow.

| Stage | Scope | Gate relationship |
| --- | --- | --- |
| **AP0** | Audit A3 and record this architecture. | Current documentation checkpoint. |
| **ART-LN** | Complete the first full Lupus Noctis pack in the accepted repository structure. | May continue immediately after AP0; does not wait for AP1–AP5. |
| **AP1** | AppData path service, built-in seed synchronizer, and Settings button to open application data. | Required before AppData becomes the runtime source. |
| **AP2** | Runtime pack registry, fingerprint validation state, exact-stem extension fallback, normalized PNG cache, and placeholders. | Required before resilient AppData/user-pack resolution. |
| **AP3** | `NoxAeterna.Tools.Assets`. | Follows proven shared runtime contracts; not required for art generation. |
| **AP4** | Manual user-pack discovery and drop-in registration. | Depends on AP1/AP2 safety and registry behavior. |
| **AP5** | Settings import wizard with staging, preview, crop/fit, PNG normalization, Save, and Cancel. | Depends on AP1/AP2 and shared normalization. |
| **PKG1** | Publish/installer seed integration and cross-platform verification. | Required before packaged delivery claims the runtime contract. |

## Separate UX and Brand Stages

These are intentionally independent from asset-pack runtime work:

- **T-UX1 — Tarot artwork scale and detail:** a single card uses materially more tableau space; add card zoom/detail; allow 5+ card layouts to reduce size; give the tableau its own scroll when necessary; keep interpretation/inspector below through vertical scroll; never make detailed artwork unreadably small.
- **S2 — seamless custom window chrome:** the title may remain visible while native Windows chrome is removed; preserve drag, double-click maximize, minimize/maximize/close, system menu, Windows Snap Layouts, DPI behavior, Linux/macOS behavior, and keyboard accessibility.
- **BRAND1 — project-owned application icon:** select one accepted source design with repository provenance; produce multi-size Windows `.ico`, desktop/package PNG sizes, and future macOS `.icns`; update README/THIRD-PARTY for generated or external material; verify executable, window, taskbar, and packaged application surfaces.

AP0 implements none of these stages and generates no icon.

## A3 Audit

| Classification | Current A3 evidence | Planned treatment |
| --- | --- | --- |
| Already reusable | Typed `TarotArtworkPackId`; schema-versioned manifest foundation; exact standard card IDs; package-relative paths; hash/dimension/status checks; partial-pack fallback; unchanged semantic reading; raster/frame/title/reversal separation; per-card raster resolution concept. | Preserve these behaviors as runtime/tool contracts. |
| Temporary built-in-only behavior | `NoxAeterna.App` directly opens one hard-coded `lupus-noctis` directory below `AppContext.BaseDirectory`; pack options are statically composed; assets are loaded into memory from app output. | Keep as the verified A3 bridge until AP1/AP2 replace discovery and storage. |
| Must migrate | Final runtime source, pack enumeration, built-in synchronization, app-data path ownership, validation/fingerprint state, decoder limits, exact-stem extension probing, normalization cache, and diagnostics persistence. | Implement incrementally in AP1/AP2 with shared contracts before user-pack workflows. |
| Deferred | Asset CLI, manual drop-in, import wizard, packaging verification, larger Tarot artwork UX, custom chrome, and application icon. | AP3–AP5, PKG1, T-UX1, S2, and BRAND1 respectively. |

The audit found no reason to rewrite A3 during AP0. Its built-in-only loader is safe for the present shipped partial pack and has focused path, identity, dimension, checksum, fallback, display-order, localization, and composition coverage; it is not represented as the final AppData runtime architecture.
