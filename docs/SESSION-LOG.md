# Session Log

| Metadata | Definition |
| --- | --- |
| Role | Bounded current-wave chronology. |
| Read when | Recent provenance is explicitly needed. |
| Authoritative for | Recent session evidence and handoff chronology. |
| Not authoritative for | Current status, architecture, roadmap, or durable policy. |

Older retained evidence is indexed in [the documentation archive](archive/README.md). Use [PROJECT-STATE.md](PROJECT-STATE.md) for the current checkpoint.

## 2026-08-05: ART-LN-CI-FIX Canonical Four of Pentacles Production Dimensions

Summary:

- Technically normalized the owner-supplied accepted A23 Four of Pentacles from `958 × 1642` to canonical `952 × 1632` by cropping `3 px` from each vertical side and `5 px` from each horizontal side. No scaling, stretching, generation, artistic edit, owner-decision change, or generation-count change occurred.
- Updated the production hash and dimensions in the manifest, focused tuple, card record, and A23 summary; added the durable handoff rule that production normalization takes precedence over byte preservation; removed the resolved current-state blocker. Inventory remains 68 production plus 10 fallback with `partialPack: true`.
- After the loader blocker was removed, the milestone run exposed one stale fallback test still using production A23 King of Wands. Its input was corrected to omitted `minor.wands.three`; runtime code and artwork were unchanged.

Verification:

- The production PNG decode, manifest/record hashes, canonical dimensions, and inventory consistency check passed. `dotnet build NoxAeterna.sln -c Release` passed with 0 warnings and 0 errors.
- The single requested Full route completed `672` tests with `671` passed and one stale-fixture failure. That fixture was corrected after the run; the Full route was not repeated, honoring the one-run constraint.

## 2026-08-05: ART-LN A24 Technical Batch Import

Summary:

- Imported the five owner-approved A24 illustrations for `minor.pentacles.nine`, `minor.cups.seven`, `minor.wands.five`, `minor.swords.ace`, and `minor.pentacles.knight`. The owner created and artistically accepted every image with ChatGPT outside Codex; Codex performed technical batch import only and did not generate, correct, or artistically review artwork.
- Losslessly decoded each `958 × 1642` handoff PNG, applied only a centered `+3,+5` crop without scaling or stretching, and stored each `952 × 1632` result at its canonical production path. Added concise per-card handoff records, expanded the manifest to 68 accepted cards with 10 fallbacks and `partialPack: true`, and removed `studies/A24` after all references were complete.
- Updated the focused inventory/path fixtures and current owner documents once for the complete batch. A25 does not start automatically.

Verification:

- The A24 technical consistency check passed: all five production PNGs decode at `952 × 1632`; their SHA-256 values match the manifest; exact card, asset, and record mappings match; each production byte stream exists only at its canonical path; `studies/A24` is absent; inventory is 68 production plus 10 fallback.
- `pwsh eng/test-route.ps1 run App-Workspace` completed `132` tests with `56` passed and `76` failed. Every failure cascades from the pre-existing A23 `minor.pentacles.four` manifest/asset dimensions `958 × 1642`, which violate the unchanged loader requirement `952 × 1632` at exact `7:12`; A24 did not alter that owner-approved A23 asset or runtime policy.

## 2026-08-05: ART-SKILL-RM Tarot Artwork Skill Removal

Summary:

- Removed the sole repository-owned Tarot generation skill, its dedicated synchronization script, and its exact user-level installation. The system skill directory and historical repository evidence remain untouched.
- Returned prompt creation, visible-image review, and artistic acceptance to the owner working with ChatGPT outside Codex. The canonical Lupus Noctis handoff now starts with 3–5 owner-approved PNGs and limits Codex to technical batch import and repository integration.
- Preserved all artwork PNGs, card records, manifest entries, production paths, and the accepted 63/78 production plus 15/78 fallback inventory.

Verification:

- `skills/list` with `forceReload: true` returned six enabled system skills, omitted the removed skill, and reported no discovery errors. The exact repository and user-level implementation paths are absent.
- Active operational references now point to the owner handoff contract; older session chronology, archives, and card provenance remain factual history. `git diff --check` is the only content gate; builds, tests, routes, application smoke, and documentation validation are excluded by task scope.

## 2026-08-05: ART-SKILL-2 + A22-P + A23 Significance Gate, Promotion, and Review Batch

Summary:

- Added the target-card-size significance/count-confusion gate to the tracked `tarot-artwork-generation` source and synchronized its installed copy. Exact-object review now targets semantic objects and noticeable count-confusing forms; tiny buttons, rivets, eyelets, fasteners, earrings, ordinary hardware, animal eyes, nail heads, and fine ornament are non-blocking when they do not read as counted objects, compete with the scene, or create physical/anatomical defects. Detail crops no longer manufacture blockers from practically invisible incidental geometry.
- Recorded the owner's explicit acceptance of `minor.pentacles.queen`, `minor.swords.king`, and `minor.wands.ace`; moved their unchanged A22 bytes to canonical production paths, removed `studies/A22`, and expanded Lupus Noctis from 57 to 60 accepted manifest assets with controlled fallback for the other 18 semantic cards. Queen provenance remains intact while clarifying that its G1 tiny-fastener and G2 small-earring corrections were overly strict process decisions, not history to erase.
- Confirmed `minor.wands.king`, `minor.swords.eight`, and `minor.pentacles.four` against the authoritative standard-78 suit/rank catalog and found no manifest, production, active-study, or record conflicts. One shared diversity audit of the twelve A19–A22 accepted cards separated A23 into kinetic green-forest pack leadership, a solitary cold dungeon threshold, and morally ambiguous reserve custody under orange firelight.
- Generated every A23 attempt independently text-only through the built-in image workflow without attachments or image references. King of Wands passed at `G1`; Four of Pentacles passed at `G1` under the new significance gate with exactly four countable door seals; Eight of Swords used `G2` after G1's tight multi-wrap wrist rope contradicted self-release. The corrected Eight retains exactly left `4` + right `4` = `8` complete swords and a visibly escapable loop. The superseded G1 PNG was deleted while its prompt, dimensions, hash, and literal blocker remain in provenance.
- Kept exactly three A23 PNGs Pending under canonical `studies/A23/` paths, outside `artwork-pack.json`, production `cards/`, and Release output. A23-P, A24, and AP1 do not begin automatically.

Verification:

- Canonical skill install/check passed; documentation validation passed with only the expected active-log soft-threshold warning after a real part-02 rollover; the Release solution build passed with 0 warnings and 0 errors. Focused `App-Workspace` and `Architecture-Boundaries` routes passed `124/124` and `17/17`.
- Exact production source/manifest/Release path sets are `60/60/60` with matching hashes and dimensions, fallback is 18, and `partialPack` remains true. `studies/A22` is absent; A23 owns exactly three `952 × 1632` Pending PNG/record pairs with no manifest/cards/Release membership; Release contains no `studies/` content. `git diff --check` is the final whitespace gate. No broader test or artwork stage is included.
