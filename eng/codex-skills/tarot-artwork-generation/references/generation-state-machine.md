# Generation State Machine

Apply this machine separately to every illustration. A review-only request enters a review state and consumes no generation attempt.

## States and legal transitions

| State | Required action | Legal next state | Stop condition |
| --- | --- | --- | --- |
| Prepare | Resolve the brief, hard contracts, output contract, provenance, and budget override. | `G1`, existing-candidate review, or owner clarification if a required input cannot be resolved. | Stop only for an irreducibly missing required input or repository boundary. |
| `G1` | Make the initial independent text-only generation. | Review 1. | Never generate again before reviewing. |
| Review 1 | Normalize and run literal inventory, exact-object, technical, anatomy, meaning, role, and composition gates. | `G2` for any blocking defect; hard-contract gate if these gates pass. | Handoff if correction is forbidden by task or owner. |
| `G2` | Make a new targeted independent text-only correction prompt from Review 1. | Review 2. | Never reuse the candidate as an image reference by default. |
| Review 2 | Repeat all gates on the new normalized pixels. Treat any remaining, repeated, replacement, or newly introduced blocking defect as critical/persistent for budget purposes. | `G3` automatically for any blocker; hard-contract gate if all pass. | Do not stop merely with “correction limit exhausted.” |
| `G3` | Make a second targeted independent text-only correction, freely rebuilding staging while preserving hard contracts. | Review 3 / hard-contract gate. | None before review. |
| Review 3 / hard-contract gate | Repeat all literal, technical, meaning, and role gates. | Independent absurdity gate only if hard requirements pass. | If any blocker remains, stop generation and hand off `attempt budget exhausted`; `G4` cannot be used to repair a failed hard-contract gate. |
| Independent absurdity gate | Ignore prompt intention as an excuse and perform the separate adversarial review. | Owner handoff if it passes; `G4` if it finds an acceptance-blocking spatial, causal, narrative, critical composition, or critical story-legibility absurdity requiring new staging. | Non-blocking notes do not authorize `G4`. |
| `G4` | Make one independent text-only absurdity-rescue generation from a targeted rescue prompt. | Final review. | `G4` is allowed once per illustration and is not an ordinary extra count-correction attempt. |
| Final review | Repeat exact-object, technical, meaning, role, composition, and independent absurdity gates on `G4`. | Owner handoff. | Stop regardless of result. A fifth generation needs explicit owner override. |
| Owner handoff | Report evidence, provenance, and honest status. | Owner decision or a separately authorized task. | Never self-assign `owner Accepted`. |

If Review 1 or Review 2 passes, proceed directly to the hard-contract and absurdity gates; unused normal attempts do not need to be spent. If an early candidate passes the hard-contract gate but fails the independent absurdity gate, `G4` is legal even when `G2` or `G3` was unused.

## Defect classification

### Non-blocking note

Do not generate solely for a small taste-level roughness, a plausible but imperfect detail, a minor tail margin, or a secondary element whose visibility was never a hard requirement.

### Blocking defect

Treat the candidate as unacceptable for a wrong count, forbidden extra, missing required object, incomplete countable object, critical crop, broken anatomy, embedded text, watermark, forbidden frame, forced motif, clear anachronism, unreadable principal role or meaning, or composition contradicting the brief. After `G1`, this authorizes `G2`.

### Critical persistent defect

After `G2`, automatically authorize `G3` for any blocker, especially a repeated or replacement count failure, serious anatomy error, unacceptable crop, persistent role confusion, meaning failure, major forbidden object, a composition needing more than a local phrase, or staging that systematically provokes the generator's error.

## `G4` rescue prompt

Before `G4`:

1. State the literal absurdity.
2. Explicitly prohibit its recurrence.
3. Restate every hard contract.
4. Permit a fresh composition or staging solution.
5. State that no prior image is supplied or referenced.

Do not disguise an unresolved exact-count or other hard-contract failure as an absurdity bonus.
