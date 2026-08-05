# Visual Review Gates

Review the normalized final image, not the prompt's promise. First describe what is literally visible; only then compare it with the brief, semantic record, and pack contract. Review-only mode consumes no generation attempts.

## Inspection views

Inspect:

- the full image at full available resolution;
- the image at target card or delivery size;
- dedicated count crops and detail crops;
- every face and hand;
- countable objects and important mechanisms;
- all frame edges and safe margins.

Do not carry forward a previous self-review. Reinspect the new pixels after every generation or normalization.

## 1. File and normalization

- Confirm readable format, intended canonical candidate, original dimensions, target dimensions, and aspect ratio.
- Preserve aspect ratio. Permit uniform scaling and only task-authorized cropping; forbid stretching.
- Recheck safe margins and critical crops after normalization.
- Compute SHA-256 from the normalized file.
- Reject embedded text, watermark, or frame when prohibited.

## 2. Literal inventory

- Inventory people, animals, containers, weapons, mechanisms, architecture, overlays, text, and recurring motifs exactly as visible.
- State uncertainty rather than filling gaps from prompt knowledge.
- Do not treat a hidden object, an item said to be behind another object, or an ambiguous patch as present.
- Distinguish objects from shadows, reflections, ornament, empty holders, and background shapes.

## 3. Exact-object contract

- Count every required and forbidden countable object independently.
- Count applicable components: rims, stems, bodies, bases, blades, guards, grips, tips, sheaths, poles, seals, animals, and containers.
- Require the contract's promised completeness and visibility. Empty sheaths are not swords; partial blades do not automatically count as complete swords; a partly occluded object is not complete unless the contract explicitly permits it.
- Record group counts and total, for example `top 4 + bottom 6 = 10`.
- Reject wrong count, a required object missing, a forbidden extra, or an incomplete required object.

### Card-size significance and count-confusion gate

Apply exact-object contracts primarily to deliberately depicted semantic objects, objects that actually participate in the count, noticeable items that could reasonably be mistaken for an additional counted object at target card size, and explicitly forbidden story-significant objects.

Do not treat tiny buttons, rivets, eyelets, fasteners, small earrings, ordinary hardware, animal eyes, small nail heads, or fine geometric ornament as automatic blockers when, at target card size, they do not read as a separate pentacle, cup, sword, wand, seal, or other counted object; do not compete with the principal objects; do not change the story; and do not create a physical or anatomical error.

Use detail crops to verify promised details, anatomy, and real defects. Do not turn practically invisible hardware into an acceptance-blocking defect merely because strong magnification reveals a round or metallic shape. Do not authorize a new generation solely for small incidental geometry unless the owner forbids that specific item for a substantive content reason.

Briefs and correction prompts must target the actual confusion risk. Avoid blanket prohibitions such as `no other round metal object anywhere` when the real contract is to prevent another pentacle or other noticeable count-confusing symbol. Prefer precise language such as: `no additional noticeable coin-like or pentacle-like object that could be mistaken for a counted pentacle at target card size`.

## 4. Anatomy

- Inspect faces, hands, fingers, limbs, joints, posture, weight distribution, and body intersections.
- Reject broken anatomy, impossible limb ownership, fused figures, or critical hands/faces cropped away.
- Treat a minor plausible irregularity as a note, not an automatic blocker.

## 5. Occlusion and tangencies

- Check whether objects remain distinct and complete through overlaps.
- Reject misleading mergers between hands, weapons, containers, clothing, architecture, or bodies.
- Check architecture and horizon lines for accidental penetration of faces and joints.
- Inspect all edges for clipped mandatory objects or anatomy.

## 6. Meaning fidelity

- Decide whether the scene communicates intended meaning without its record.
- Separate constructive and shadow readings; ensure their visual balance follows the brief.
- Reject a beautiful location or craft scene that does not express the card's meaning.
- Reject a composition that reads primarily as a different Tarot archetype.

## 7. Role legibility

- Identify the principal figure without relying on centrality, size, costume quality, or record labels.
- Require role evidence through action, others' reactions, authority over space, objects, social procedure, and consequence.
- Confirm supporting figures do not become equal alternative protagonists unintentionally.
- For an institutional-teacher archetype, require recognizable office, transmission of tradition, initiation or formal procedure, exactly required initiates, and distinction between initiates and witnesses.

## 8. Casting and novelty

- Compare recent accepted works for repeated face archetypes, demographics, costumes, poses, action, camera, location, palette, silhouettes, and social roles.
- Require purposeful casting contrast without violating the brief.
- Do not use accepted artwork as an image reference merely to achieve continuity.

## 9. Historical/material coherence

- Check declared technological level and period coherence.
- Reject modern thermometers, laboratory instruments, UI markers, or other undeclared technology.
- Verify that materials, fasteners, ropes, weapons, vessels, tools, and mechanisms could perform their apparent function.
- Allow supernatural elements only within the declared boundary and judge them by the world's internal logic.

## 10. Pop-fantasy resemblance

- Look for accidental resemblance to a recognizable franchise character caused by several stacked traits, not by one generic feature.
- Check costume, hairstyle, pose, props, silhouette, color blocking, and setting together.
- Treat a strong composite resemblance as a blocker when it compromises originality; do not over-flag ordinary genre vocabulary.

## 11. Independent absurdity

Run this adversarial pass only after hard requirements, technical review, meaning, and role gates pass. Ignore prompt intent as an excuse and ask:

> What in this scene is physically, spatially, causally, historically, or narratively nonsensical without extra textual explanation?

### Support and gravity

- Does every person stand, sit, lie, or hang on an actual support?
- Is anyone outside a railing without a platform?
- Do feet occupy air, wall, water, or void?
- Are body and object weight physically supported?
- Do ropes and fixtures attach to something real?

### Space and perspective

- Do doors, gates, stairs, bridges, rails, and platforms form usable space?
- Can the declared route physically be traversed?
- Does a platform vanish unexpectedly?
- Do bodies and architecture intersect impossibly?
- Are character and object scales mutually compatible?

### Contact and action

- Does a hand actually contact the lever, sword, cup, or other acted-on object?
- Does one hand perform incompatible actions?
- Do characters look at and react to the event?
- Is cause and effect legible?
- Is a falling, lifted, or transferred object connected to the action?

### Narrative hierarchy

- Is the principal figure evident without the record?
- Is it clear who teaches, judges, commands, deceives, saves, or decides?
- Are supporting figures subordinate in the intended way?
- Does the scene show meaning rather than merely craft or location?
- Does it avoid reading as another Arcana archetype?

### Historical and material coherence

- Is there undeclared modern instrumentation or interface language?
- Do props match the declared technological level?
- Can materials and mechanisms perform their apparent purpose?

### Composition and accidental semantics

- Are there infographic arrows, chevrons, or overlay marks?
- Is a recurring motif reduced to a random logo, badge, stamp, relief, or token?
- Do architectural lines pierce faces or joints?
- Does background staging create an impossible scene?
- Does a pose plus multiple traits accidentally evoke a famous franchise character?

### Internal fantasy logic

Do not reject the supernatural merely for violating real-world physics when the art direction permits it. Require coherent rules, supports, cause, and consequences within the declared world.

Classify the result:

- **Pass** — no acceptance-blocking absurdity.
- **Non-blocking note** — noticeable but plausible or taste-level imperfection; no generation authorization.
- **Acceptance-blocking failure** — owner-visible spatial, causal, narrative, critical composition, or critical story-legibility absurdity requiring new staging; authorize the single `G4` if unused.

## 12. Owner handoff

- Report literal evidence before interpretations.
- Separate hard blockers from non-blocking notes.
- State exact counts and component evidence where applicable.
- Report normalized dimensions, SHA-256, attempt history, and reference-image use.
- Use `Pending owner review` for a technically eligible candidate.
- Never assign `owner Accepted` without an explicit owner decision.
- After `G4`, report the result honestly and stop even if a blocker remains.
