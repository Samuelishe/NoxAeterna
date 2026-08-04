# Session Log

| Metadata | Definition |
| --- | --- |
| Role | Bounded current-wave chronology. |
| Read when | Recent provenance is explicitly needed. |
| Authoritative for | Recent session evidence and handoff chronology. |
| Not authoritative for | Current status, architecture, roadmap, or durable policy. |

Older retained evidence is indexed in [the documentation archive](archive/README.md). Use [PROJECT-STATE.md](PROJECT-STATE.md) for the current checkpoint.

## 2026-08-04: A20-P + A21 Acceptance Promotion and New Review Batch

Summary:

- Recorded the owner's explicit acceptance of corrected `major.hierophant`, `minor.cups.nine`, and `minor.swords.queen`; moved the unchanged A20 bytes to canonical production paths, removed the empty `studies/A20`, and expanded Lupus Noctis from 51 to 54 accepted manifest assets with controlled fallback for the other 24 semantic cards. Their records retain every superseded hash, prompt, literal rejection defect, correction mode, and cumulative generation count.
- Confirmed `major.world`, `minor.wands.six`, and `minor.cups.two` against the authoritative standard-78 catalog and created exactly three independent text-only A21 candidates without attachments or image references. World and Two of Cups passed at `G1`. Six of Wands used `G2` after G1 exposed only three horse hoof endpoints and one visible stirrup, then used `G3` critical recovery after G2 still hid the far boot/stirrup; the final candidate contains exactly `3 + 3 = 6` complete staffs, four distinct hooves, and both boots in visible stirrups. No card qualified for `G4`.
- Kept A21 **Pending** with one canonical PNG and one record per card under `studies/A21`; no A21 asset enters `artwork-pack.json`, production `cards/`, or Release output. Full prompts remain only in the per-card records. A21-P, A22, and AP1 remain unstarted.

Verification:

- `pwsh eng/sync-codex-skills.ps1 -Check` passed against the tracked `tarot-artwork-generation` source and installed user-level copy; `pwsh eng/doc-check.ps1` passed with no documentation errors.
- `dotnet build NoxAeterna.sln -c Release` passed with 0 warnings and 0 errors. Focused `App-Workspace` and `Architecture-Boundaries` Release routes passed `118/118` and `17/17`.
- Exact source/manifest/Release path sets all contain 54 production PNGs with manifest hashes; fallback is 24 and `partialPack` remains true. A20 production dimensions and hashes match owner-accepted bytes. A21 owns exactly three `952 × 1632` studies and three Pending records, has zero manifest/production/Release membership, and Release contains no `studies/` content.

## 2026-08-04: SKILL-TAROT-2 + A20-C Canonical Skill and Corrections

Summary:

- Promoted the previously validated `tarot-artwork-generation` skill byte-for-byte into canonical repository ownership at `eng/codex-skills/tarot-artwork-generation`, added a dependency-free `-Install`/`-Check` synchronization command, and documented the repository-source/user-runtime relationship. The installed user copy now matches all seven tracked files by relative path and SHA-256 and is discovered once with user scope.
- Applied the tracked skill's independent text-only correction state machine to all three owner-rejected A20 candidates without image references. `major.hierophant` used `G2` to make the formal guild initiation and role hierarchy legible; `minor.cups.nine` used `G3` critical recovery and now contains exactly `3 + 3 + 3 = 9` complete goblets with no other container; `minor.swords.queen` used `G2` to rebuild the checkpoint as continuous supported paving with exactly one complete sword. All three passed hard gates and separate adversarial absurdity review; no `G4` was authorized or needed.
- Kept all three candidates **Pending** under `studies/A20`, outside `artwork-pack.json`, `cards/`, and Release output. Their records retain superseded hashes and literal rejection defects plus complete new prompts, normalized hashes, reviews, and cumulative generation counts. A20-P, A21, and AP1 remain unstarted.

Verification:

- `pwsh eng/sync-codex-skills.ps1 -Install` and `-Check` passed against the actual user-level root; a separate temporary-root install/check passed and the verified temporary directory was removed. `skills/list` with `forceReload: true` reported exactly one enabled user-scope `tarot-artwork-generation` and no discovery errors.
- `pwsh eng/doc-check.ps1` passed with 0 warnings and 0 errors; `dotnet build NoxAeterna.sln -c Release` passed with 0 warnings and 0 errors.
- Focused `Tarot`, `Architecture-Boundaries`, and `App-Workspace` Release routes passed `40/40`, `17/17`, and `115/115`. Repository assertions confirmed Pending ownership, A20 absence from manifest/cards/Release, and no `studies/` content in Release.

## 2026-08-04: A19-P + A20 Production Promotion and Review Batch

Summary:

- Recorded owner acceptance of corrected `major.wheel-of-fortune`, `minor.cups.king`, and `minor.pentacles.seven`; moved their unchanged A19 bytes to canonical production paths, removed `studies/A19`, and expanded Lupus Noctis from 48 to 51 accepted manifest assets with controlled fallback for the other 27 semantic cards.
- Preserved all initial A19 hashes, owner-rejection reasons, independent correction prompts, corrected hashes, cumulative generation count two, and final acceptance. The identical five-point geometry on Seven of Pentacles' seven seals is explicitly accepted as part of the seals rather than an extra object, defect, pseudo-rune, or wolf motif.
- Generated independent text-only A20 candidates for `major.hierophant`, `minor.cups.nine`, and `minor.swords.queen` without image references. Hierophant and Queen passed their initial technical gates. Nine's initial candidate contained exactly nine cups but one cropped hand and extra open containers; its one correction fixed those defects but produced `4 + 6 = 10` cups. No third attempt was made, and the blocking tenth cup is recorded literally.
- Kept exactly three A20 PNGs Pending under canonical `studies/A20/` paths, outside the manifest and Release output. A20-P, A21, and AP1 do not begin automatically.

Verification:

- The proportional gate covers documentation validation, a Release solution build, focused artwork-pack and repository-boundary routes, source/manifest/Release/study ownership and hash checks, and `git diff --check`; broad milestone, coverage, UI, and hosted checks are intentionally excluded.

## 2026-08-04: A19 Targeted Correction Pass

Summary:

- Recorded owner rejection of all three initial A19 candidates: Wheel of Fortune contained explicit white directional-arrow overlays, King of Cups used a forced paw stamp on the cup holder, and Seven of Pentacles paired a modern-thermometer reading with a forced lintel track relief.
- Replaced each candidate in place through exactly one independent text-only correction without attachments or image references. Wheel now expresses opposing motion physically without arrows; King retains exactly one gimballed cup in a plain holder; Seven retains seven seals in `4 + 3`, uses an open preindustrial cord-and-weight draft register, and has a plain lintel. Each record preserves the initial hash, owner rejection, complete correction prompt, corrected metadata, cumulative generation count two, and Pending status.
- Added the durable rule that wolf-world elements must be narratively meaningful rather than token branding; ambient-only is required when a wolf motif does not arise naturally. Seven's identical five-point seal geometry remains explicitly disclosed for owner review.
- Kept A19 outside the production manifest and Release output. A19-P, A20, and AP1 do not begin automatically.

Verification:

- The proportional gate covers documentation validation, a Release solution build, focused artwork-pack and repository-boundary tests, corrected PNG dimensions/hash/provenance and source/manifest/output separation checks, and `git diff --check`; GitHub Actions is not inspected.

## 2026-08-04: A18-P + A19 Production Promotion and Reversal Batch

Summary:

- Recorded owner acceptance of `major.empress`, `minor.swords.four`, and corrected `minor.wands.eight`; moved their unchanged A18 bytes to canonical production paths, removed the empty A18 study directory, and expanded Lupus Noctis from 45 to 48 accepted manifest assets with controlled fallback for the other 30 semantic cards.
- Preserved the complete Eight of Wands correction history: the initial text-only generation contained `4 + 3 = 7` batons, and one targeted independent correction produced the accepted `4 + 4 = 8` result for cumulative generation count two.
- Generated independent text-only A19 candidates for `major.wheel-of-fortune`, `minor.cups.king`, and `minor.pentacles.seven` with no image references. All three passed their initial full-size, card-size, and count/detail technical reviews without correction; detailed prompts, casting and novelty contrasts, mood scripts, dimensions, hashes, motif modes, and technical observations belong to their records.
- Kept all three A19 candidates Pending under canonical `studies/A19/` paths, outside the production manifest and Release output. The post-A19 batch remains deliberately unselected, and AP1 does not begin automatically.

Verification:

- The proportional gate covers documentation validation, a Release solution build, focused artwork-pack and repository-boundary tests, source/manifest/output/study/hash consistency checks, and `git diff --check`; GitHub Actions is intentionally not inspected.
