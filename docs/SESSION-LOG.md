# Session Log

| Metadata | Definition |
| --- | --- |
| Role | Bounded current-wave chronology. |
| Read when | Recent provenance is explicitly needed. |
| Authoritative for | Recent session evidence and handoff chronology. |
| Not authoritative for | Current status, architecture, roadmap, or durable policy. |

Older retained evidence is indexed in [the documentation archive](archive/README.md). Use [PROJECT-STATE.md](PROJECT-STATE.md) for the current checkpoint.

## 2026-08-03: A9-P + A10 Production Promotion and Review Batch

Summary:

- Recorded the owner's acceptance of `major.tower`, `minor.cups.eight`, and `minor.pentacles.three`; moved the unchanged A9 bytes to canonical production paths, removed the empty A9 study directory, and expanded Lupus Noctis from 18 to 21 accepted manifest assets with controlled fallback for the other 57 semantic cards.
- Generated independent text-only A10 candidates for `major.temperance`, `minor.swords.ten`, and `minor.wands.queen` with no image references. Temperance used one targeted correction to remove an accidental tattoo; Ten of Swords and Queen of Wands passed their initial technical gates. Exact generation counts, dimensions, hashes, prompts, and technical reviews belong to their linked records.
- Kept all three A10 candidates Pending under canonical `studies/A10/` paths, outside the production manifest and application output. The post-A10 batch remains deliberately unselected, and AP1 does not begin automatically.

Verification:

- The proportional gate covers documentation validation, a Release solution build, focused artwork-pack and repository-boundary tests, source/manifest/output/study/hash consistency checks, and `git diff --check`; broad hosted CI remains deferred until a future owner-authorized commit and push.

## 2026-08-03: A8-P + A9 Production Promotion and Review Batch

Summary:

- Recorded the owner's acceptance of `major.justice`, `minor.pentacles.five`, and `minor.cups.queen`; moved the unchanged A8 bytes to canonical production paths, removed the empty A8 study directory, and expanded Lupus Noctis from 15 to 18 accepted manifest assets with controlled fallback for the other 60 semantic cards.
- Generated independent text-only A9 candidates for `major.tower`, `minor.cups.eight`, and `minor.pentacles.three` with no image references. Tower passed its initial gate; Eight of Cups used one targeted correction to remove an extra planter, and Three of Pentacles used one targeted correction to remove an accidental tattoo. Exact generation counts, dimensions, hashes, prompts, and technical reviews belong to their linked records.
- Kept all three A9 candidates Pending under canonical `studies/A9/` paths, outside the production manifest and application output. The post-A9 batch remains deliberately unselected, and AP1 does not begin automatically.

Verification:

- The proportional gate covers documentation validation, a Release solution build, the focused artwork-pack and repository-boundary tests, source/manifest/output/study/hash consistency checks, and `git diff --check`; broad hosted CI remains deferred until a future owner-authorized commit and push.

## 2026-08-03: A7-P + A8 Production Promotion and Review Batch

Summary:

- Recorded the owner's acceptance of `major.emperor`, `minor.cups.five`, and `minor.wands.page`; moved the unchanged A7 bytes to canonical production paths, removed the empty A7 study directory, and expanded Lupus Noctis from 12 to 15 accepted manifest assets with controlled fallback for the other 63 semantic cards.
- Generated independent text-only A8 candidates for `major.justice`, `minor.pentacles.five`, and `minor.cups.queen` with no image references. Justice and Five of Pentacles passed their initial technical gates; Queen of Cups used one targeted text-only correction to remove an extra background vessel. Exact generation counts, dimensions, hashes, prompts, and technical reviews belong to their linked records.
- Kept all three A8 candidates Pending under canonical `studies/A8/` paths, outside the production manifest and application output. The post-A8 batch remains deliberately unselected, and AP1 does not begin automatically.

Verification:

- The proportional gate covers documentation validation, a Release solution build, the focused artwork-pack and repository-boundary tests, source/manifest/output/study/hash consistency checks, and `git diff --check`; broad hosted CI remains deferred until a future owner-authorized commit and push.

## 2026-08-03: A6-P + A7 Production Promotion and Review Batch

Summary:

- Recorded the owner's acceptance of `major.lovers`, `minor.swords.nine`, and `minor.pentacles.two`; moved the unchanged A6 bytes to canonical production paths, removed the empty A6 study directory, and expanded Lupus Noctis from 9 to 12 accepted manifest assets with controlled fallback for the other 66 semantic cards.
- Generated independent text-only A7 candidates for `major.emperor`, `minor.cups.five`, and `minor.wands.page` with no image references. All three initial generations met their narrative and countable-object contracts, so no corrective generation was used; exact generation counts, dimensions, hashes, prompts, and technical reviews belong to their linked records.
- Kept all three A7 candidates Pending under canonical `studies/A7/` paths, outside the production manifest and application output. The post-A7 batch remains deliberately unselected, and AP1 does not begin automatically.

Verification:

- The proportional gate covers documentation validation, a Release solution build, the focused artwork-pack and repository-boundary tests, source/manifest/output/study/hash consistency checks, and `git diff --check`; broad hosted CI remains deferred until a future owner-authorized commit and push.

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
