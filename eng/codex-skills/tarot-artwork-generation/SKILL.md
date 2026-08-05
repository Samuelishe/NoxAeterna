---
name: tarot-artwork-generation
description: Orchestrate meaning-first generation, correction, and strict visual review of Tarot card artwork, full card faces, alternate artwork packs, deck covers, and pack key art. Use for new Tarot illustrations, approved-brief generation, text-only correction passes, exact object-count checks, composition, meaning, or absurdity review of existing Tarot imagery, provenance handoff, or batches of independent Tarot assets. Do not use for generic icons, UI assets or layouts, non-Tarot imagery, software code review, or C# testing.
---

# Tarot Artwork Generation

## Purpose

Create and assess Tarot imagery through a meaning-first workflow. Make the scene communicate the semantic identity before adding familiar symbols. Orchestrate `$imagegen` for generation, inspection, normalization, dimensions, cropping or resizing, and SHA-256 work; do not recreate its backend.

## Modes

Choose exactly one mode per asset:

1. **Brief and generate** — derive a meaning-first brief, then generate a new candidate.
2. **Generate from approved brief** — preserve the approved core requirements; do not artistically reinvent them.
3. **Review existing candidate** — load the local candidate with the available image-inspection tool, then perform literal and adversarial review only. Consume no generation attempt.
4. **Correct rejected candidate** — read provenance and owner rejection, diagnose the blocker, then issue the next legal independent text-only generation.
5. **Batch** — process each asset independently with its own brief, budget, provenance, and final status. Never use one card as another card's image reference. Continue technically independent items after one fails unless the task explicitly requires atomicity.

## Required inputs

Resolve these from the task, authoritative semantic catalog, pack owner document, card record, manifest, or nearby accepted records before asking the user:

- asset role: card illustration, full card face, deck cover, or pack key art;
- semantic card ID or title; intended, constructive, and shadow meanings;
- artwork-pack identity and art direction;
- canonical output path, target dimensions and aspect ratio;
- frame and text policy;
- exact-object contracts, required objects, and forbidden objects;
- supernatural boundary and recurring-motif policy;
- provenance destination and recent accepted works for diversity comparison;
- Git/repository boundaries and any owner attempt override.

Ask only when a required value remains genuinely unresolved. Do not assume a 78-card deck, a fixed size or aspect ratio, a specific motif, or a particular repository layout.

## Workflow

1. Read [brief-template.md](references/brief-template.md) when creating or validating a brief. Determine the core meaning, separate constructive from shadow meaning, and choose one concrete narrative moment with one main action verb.
2. Define principal and supporting figures, social roles, setting, time, mood, intensity, composition, and movement. Make the principal role legible through action, reactions, spatial authority, objects, procedure, and consequence—not merely size, centering, costume, or record text.
3. Compare recent accepted work for scene novelty and casting contrast.
4. Define hard requirements, artistic preferences, optional details, exact-object contracts, and negative constraints. Integrate any recurring motif meaningfully through story, culture, material, character, or location; treat it neither as a quota nor as a random logo, badge, stamp, relief, or token. Permit physical absence when the pack allows ambient-only use.
5. For generation or correction, read [generation-state-machine.md](references/generation-state-machine.md) and follow its legal transitions exactly. For all reviews, read and apply [visual-review-gates.md](references/visual-review-gates.md).
6. Normalize according to the task: preserve aspect ratio, allow uniform scaling, crop only when permitted, never stretch, recheck safe margins, and compute the normalized file's SHA-256.
7. Inspect the normalized final image at full size, target card size, and with count/detail crops for faces, hands, countable objects, mechanisms, and frame edges. Apply the card-size significance and count-confusion gate: incidental hardware or geometry is blocking only when it reads at target size as a counted object, competes with the principal objects, changes the story, or creates a physical/anatomical defect. Detail crops verify promised details and real defects; they do not promote practically invisible fittings into blockers. State literal observations before comparing them with the brief or record. Never infer hidden, occluded, incomplete, or merely prompted objects.
8. Once hard requirements, technical review, and meaning review pass, run a separate adversarial absurdity review. Ask: “What in this scene is physically, spatially, causally, historically, or narratively nonsensical without extra textual explanation?”
9. Record every generation with [provenance-template.md](references/provenance-template.md). Retain only one canonical final candidate; retain prompt, hash, dimensions, and defects for superseded candidates, not their PNG files.
10. Use [regression-cases.md](references/regression-cases.md) when exact counts, spatial support, role hierarchy, overlays, anachronisms, motifs, or occlusion are risk areas.

Do not reduce a Tarot scene to a checklist of recognizable symbols. The image must convey the card's meaning without requiring its record.

## Attempt budget

Apply the budget per illustration, not per task:

- `G1` (`initial`): initial independent text-only generation.
- `G2` (`targeted correction`): first targeted independent text-only correction after a blocking defect.
- `G3` (`critical recovery`): second targeted independent text-only correction. A blocking defect after `G2`, including a repeated or replacement blocker, authorizes `G3` automatically; do not stop at `G2` as “correction limit exhausted.”
- `G4` (`absurdity rescue`): one optional final absurdity-rescue generation, available only when a candidate has passed hard requirements and an independent absurdity review finds an acceptance-blocking spatial, causal, narrative, composition, or story-legibility failure requiring new staging.

Stop as soon as a candidate passes hard requirements, technical and meaning review, and the absurdity gate. The budget is a ceiling, not a quota. Default maximum is `G1 + G2 + G3 + G4`; a fifth or later generation requires explicit owner override. After `G4`, rerun every gate and stop regardless of outcome.

## Calling imagegen

- Invoke `$imagegen` and follow its built-in-first workflow for every actual generation. Do not invoke image generation in review-only mode.
- Default every `G1`–`G4` to a new text-only generation with no attachment or image reference.
- Do not use a rejected candidate, an accepted card, or another new card as an image reference. Depart only under explicit owner override or a pack contract.
- Build each correction prompt anew from the literal diagnosis. Do not phrase it as editing the previous image.
- Preserve semantic identity, core meaning, pack identity, hard required/forbidden objects, canonical output contract, and explicit owner constraints.
- Freely change camera angle, staging, placement, movement, poses, some casting, background, allowed location, meaning visualization, countable-object placement, architecture, props, lighting, or scene density. Simplify when it improves exact counts, story clarity, geometry, or hierarchy.

## Final handoff

Report the canonical candidate path, normalized dimensions and SHA-256, generation history and cumulative count, literal blockers or non-blocking notes, gate results, image-reference use, and one honest status:

- `superseded` for an earlier generation;
- `Pending owner review` for a technically eligible candidate;
- `owner Accepted` or `owner Rejected` only when explicitly decided by the owner;
- `blocking defect remains` or `attempt budget exhausted` when applicable.

Never self-promote a candidate to production or mark it Accepted without explicit owner authority or a direct task contract.

## Reference map

- [generation-state-machine.md](references/generation-state-machine.md) — legal attempt transitions and stop conditions.
- [visual-review-gates.md](references/visual-review-gates.md) — literal, semantic, technical, and absurdity gates.
- [brief-template.md](references/brief-template.md) — reusable meaning-first brief.
- [provenance-template.md](references/provenance-template.md) — repository-neutral history for one to four generations.
- [regression-cases.md](references/regression-cases.md) — known failure patterns used as review training examples.
