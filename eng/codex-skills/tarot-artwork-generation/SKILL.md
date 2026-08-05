---
name: tarot-artwork-generation
description: Interactive, owner-gated generation of Tarot illustrations, full card faces, artwork packs, deck covers, and pack key art. Use for batches that require owner approval after every card, owner-directed corrections, or promotion of an accepted candidate. Generate one independent text-only candidate at a time, pause for owner inspection, and never self-correct from the model's own visual judgment.
---

# Tarot Artwork Generation

## Purpose

Create meaning-first Tarot artwork through a strictly interactive, human-in-the-loop process. Codex prepares the prompt and technical provenance, generates exactly one candidate, saves it, reports its location and metadata, then pauses for the owner to inspect the PNG and decide what happens next.

## Supported modes

1. **Prepare batch** — propose and pause on a queue of one to five cards when cards or briefs are not yet approved.
2. **Generate next candidate** — generate one independent text-only candidate for the first unprocessed approved item, save it to the canonical Pending path, report technical metadata, and pause.
3. **Apply owner decision** — accept, redo from specific owner feedback, hold, or reject the current candidate.
4. **Batch finalization** — reconcile owner decisions once every queue item has an owner outcome.

## Required inputs

Resolve these from the task and the smallest authoritative repository sources:

- asset role and semantic identity from the authoritative catalog when applicable;
- approved meaning-first brief, or enough information to prepare one;
- artwork-pack identity and hard pack contracts;
- canonical Pending and production paths where applicable;
- target dimensions, aspect ratio, and normalization policy;
- text, frame, image-reference, and exact-object prompt requirements;
- provenance destination and repository bookkeeping rules;
- queue order and explicit owner constraints.

Ask only when a required value cannot be resolved safely. Do not assume a fixed deck catalog, size, aspect ratio, repository layout, promotion rule, or motif policy.

## Interactive owner-gated workflow

Read and follow [interactive-owner-workflow.md](references/interactive-owner-workflow.md). Use [brief-template.md](references/brief-template.md) for a compact meaning-first approved brief and [provenance-template.md](references/provenance-template.md) for any number of generations.

If the task already supplies approved cards and briefs, skip batch preparation. For generation, invoke `$imagegen` only as the raster-generation backend, use an independent text-only prompt with no attachments or image references, create exactly one candidate, complete only the permitted mechanical processing, report it, and pause through the host's standard request-user-input or turn-ending mechanism. Do not start another generation before an explicit owner command.

## Mechanical processing boundaries

After generation, Codex may only confirm that the file exists, the PNG decodes, dimensions can be read, dimensions and aspect ratio match the task contract, no stretching was applied, SHA-256 was computed, and the canonical path is correct. Normalization may use only the task-approved uniform scaling or crop policy and must never stretch the image.

## Explicit prohibition on artistic assessment

Codex must not decide whether generated pixels satisfy artistic, semantic, narrative, historical, anatomical, compositional, object-count, legibility, mood, casting, originality, or plausibility requirements. Hard requirements belong in the prompt, but only the owner decides whether the visible result fulfills them. Codex must not classify visual details as blocking or non-blocking, declare a candidate ready for review, invent rejection reasons, or initiate another generation from its own judgment.

## Owner decision commands

- `Принять` — promote the current candidate according to repository policy, then process the next queue item if one exists.
- `Переделать: <конкретный комментарий владельца>` — preserve the feedback, generate exactly one new independent candidate for the same item, report it, and pause again.
- `Отложить` — keep the current candidate Pending/Hold and proceed to the next queue item.
- `Отклонить` — record rejection, handle the PNG according to repository or explicit owner retention policy, and do not create a replacement automatically.

Only these explicit owner decisions advance the current candidate. Generation count is not limited by this skill; each additional generation requires a new `Переделать: ...` command.

## Queue behavior

Keep one ordered queue of one to five assets. Process it strictly as candidate → save → report → pause → owner decision → bookkeeping → next candidate. Never pre-generate later items, attach prior candidates, use one batch item as another's reference, or continue after the pause without owner input. The owner may stop the batch, revise a brief, replace a future queue item, or change its order before that item is generated.

## Batch finalization

After every queue item is accepted, held, or rejected, perform one short reconciliation pass: align records with owner decisions, update manifest entries for accepted assets when the repository contract requires it, update required focused inventory data, briefly update pack owner state, and show `git status --short`.

Do not automatically run builds, tests, test routes, documentation checks, coverage, Release path-set audits, CI, applications, UI smoke, or subagents. Do not update session, project-state, next-steps, or archive documents after each asset. If the repository requires documentation, update it once after the entire queue and only within explicit task scope.

## References

- [interactive-owner-workflow.md](references/interactive-owner-workflow.md) — exact sequential modes, owner transitions, and pause behavior.
- [brief-template.md](references/brief-template.md) — compact meaning-first batch proposal and approved brief.
- [provenance-template.md](references/provenance-template.md) — separate brief, prompt, feedback, decision, and technical history for unlimited owner-directed generations.
