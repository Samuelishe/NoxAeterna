# Session Log

| Metadata | Definition |
| --- | --- |
| Role | Bounded current-wave chronology. |
| Read when | Recent provenance is explicitly needed. |
| Authoritative for | Recent session evidence and handoff chronology. |
| Not authoritative for | Current status, architecture, roadmap, or durable policy. |

Older retained evidence is indexed in [the documentation archive](archive/README.md). Use [PROJECT-STATE.md](PROJECT-STATE.md) for the current checkpoint.

## 2026-08-03: A5 / A5-P Tracked Review Contract and Production Promotion

Summary:

- Recorded the owner's acceptance of `major.hanged-man`, `minor.pentacles.eight`, and `minor.wands.four`; the Hanged Man was accepted without regeneration despite its comparatively weaker artistic assessment.
- Replaced the pack-root total-PNG assertion that made canonical Pending studies fail hosted CI with a durable boundary: production PNGs must match manifest `assetPath` entries under `cards/`, while bounded Pending candidates may exist only at canonical `studies/A<positive-number>/<filename>.png` paths with one exact linked record and no output copy or production duplicate.
- Moved the three A5 candidates byte-for-byte to canonical production paths, removed the empty `studies/A5/` directory, preserved `952 × 1632` dimensions and SHA-256 values, and expanded the partial manifest and focused resolver expectations from 6 to 9 raster cards with 69 prototype fallbacks.
- Updated the pack owner, per-card provenance records, attribution wording, asset-pipeline contract, current state, and immediate queue. The post-A5 batch remains deliberately unselected; AP1 does not start automatically.

Verification:

- `pwsh eng/doc-check.ps1` completed with 0 errors and only the existing `AGENTS.md` soft-threshold warning; `dotnet build NoxAeterna.sln -c Release` completed with 0 warnings and 0 errors.
- The focused `TarotArtworkPackTests|LupusNoctisRepositoryPack` filter passed 37/37; source and Release output each contain the same nine manifest production paths, no output `studies/` exists, `studies/A5` is absent, and every promoted A5 hash occurs once at its canonical path with unchanged dimensions.
- `git diff --check` passed; no Full milestone, coverage, UI smoke, application launch, image generation, or image editing was performed.
