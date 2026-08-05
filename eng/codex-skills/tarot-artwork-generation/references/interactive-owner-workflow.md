# Interactive Owner-Gated Workflow

This workflow gives the owner exclusive authority over the visible result. Codex handles briefing, one-at-a-time generation, mechanical file processing, provenance, and repository bookkeeping.

## Mode 1: Prepare batch

Use this mode only when cards, assets, or briefs have not already been approved.

1. Verify each Tarot semantic ID against the repository's authoritative catalog.
2. Prepare an ordered proposal of one to five assets using the prepare-batch block in [brief-template.md](brief-template.md).
3. For each item provide its semantic ID, title, core meaning, one-sentence scene, principal casting, location, and any supplied exact-object requirements.
4. Do not generate an image.
5. Invoke the host's standard request-user-input or pause mechanism and wait for the owner to approve or revise the queue and briefs.

Do not perform a complete audit of the latest records or images and do not read full historical generation prompts for this mode. Use only the smallest authoritative sources needed to verify identity and pack compatibility. Skip this mode when the task already contains approved cards and briefs.

## Mode 2: Generate next candidate

For the first unprocessed item in the approved queue:

1. Load its approved brief and governing pack or repository contracts.
2. Write one complete, meaning-first, independent text-only generation prompt. Include hard requirements as instructions, not as claims about the future pixels.
3. Invoke `$imagegen` exactly once.
4. Supply no attachment, image reference, earlier candidate, accepted artwork, or other queue item as a reference.
5. Normalize only as the task permits, preserve aspect ratio, never stretch, and save the result directly at the canonical Pending study path.
6. Perform only the permitted mechanical checks.
7. Compute saved dimensions and SHA-256 and append the exact prompt and technical provenance.
8. Report the asset title, semantic ID, actual generation ordinal, repository-relative path, absolute path, dimensions, and SHA-256.
9. Invoke the host's standard request-user-input or pause mechanism and wait for one owner decision command.

The pause is a hard workflow boundary. Do not generate, promote, remove, review, or start the next item while waiting. When a structured request-user-input control is available, offer the four owner decisions and allow the redo comment to be entered. If the host provides no such control, end the turn with a concise request for one exact command. Resume only from the owner's next message.

## Mode 3: Owner decision — accept

After `Принять`:

1. Record that the owner accepted the current generation.
2. Move the PNG byte-for-byte to the canonical production path and remove the Pending duplicate.
3. Update owner acceptance, production status, production path, and any manifest entry required by repository policy.
4. Preserve the prompt, dimensions, SHA-256, actual generation ordinal, and explicit owner decision.
5. Do not run builds, tests, routes, documentation checks, Release verification, or artistic checks.
6. If another queue item remains, generate exactly one candidate for it using Mode 2, report it, and pause again. Otherwise proceed to batch finalization.

## Mode 4: Owner decision — redo

After `Переделать: <конкретный комментарий владельца>`:

1. Preserve the owner's comment verbatim when practical, otherwise render it faithfully without adding a Codex rejection rationale.
2. Create a new independent text-only prompt from the approved brief, hard repository and pack contracts, and that specific comment.
3. Change only the necessary scope; do not broaden the correction from Codex's own preferences.
4. Do not use the prior PNG or any other image as a reference.
5. Invoke `$imagegen` exactly once and replace the canonical Pending PNG with the newly normalized candidate.
6. Record the next actual generation ordinal, full prompt, dimensions, SHA-256, and owner feedback.
7. Report the same path and new metadata, then pause again.

Only another explicit owner command can cause a further generation. The skill imposes no generation-count ceiling.

## Mode 5: Owner decision — hold

After `Отложить`:

1. Leave the PNG at its Pending path.
2. Record the owner outcome as `Pending/Hold`.
3. If another queue item remains, generate exactly one candidate for it using Mode 2 and pause again. Otherwise proceed to batch finalization.

## Mode 6: Owner decision — reject

After `Отклонить`:

1. Record the owner rejection.
2. Apply the repository or pack disposition policy: do not retain a rejected or replaced PNG when policy forbids it, but do not delete it when the owner explicitly asks to retain it as Pending.
3. Do not generate a replacement.
4. Move to the next queue item with Mode 2, or finalize when the queue is complete.

## Mechanical checks only

Codex may verify only:

- the expected file exists;
- the PNG decodes;
- dimensions are readable and meet the target contract;
- aspect ratio meets the task contract;
- no stretching was applied;
- SHA-256 is computed from the saved PNG;
- the saved path is canonical.

Codex must not inspect or judge counts of Tarot objects or other semantic objects; anatomy or body details; physical mechanisms; composition, perspective, causality, plausibility, historical coherence, or scene logic; meaning, role legibility, mood, casting, foliage, visual noise, tangencies, occlusion, recurring motifs, or resemblance to a known franchise. The owner evaluates all visible and artistic properties after opening the reported PNG.

## Queue control

The queue contains one to five assets and is strictly sequential. Never pre-generate later items. Before its generation, the owner may stop the batch, revise its brief, reorder it, remove it, or replace it. Preserve each asset's approved brief, generated prompts, owner feedback, owner decisions, and technical metadata as distinct provenance fields using [provenance-template.md](provenance-template.md).

## Batch finalization

Once every item has an owner outcome:

1. Reconcile records with those outcomes.
2. Update manifest entries for accepted assets when required.
3. Update focused inventory data only when the repository contract directly requires it.
4. Briefly update pack owner state.
5. Show `git status --short`.

Do not automatically run builds, tests, routes, documentation checks, coverage, Release path-set audits, CI, applications, UI smoke, or subagents. Do not update session, project-state, next-steps, or archive documents after individual items. If documentation is required, update it once after the complete queue and only within the owner's task scope.
