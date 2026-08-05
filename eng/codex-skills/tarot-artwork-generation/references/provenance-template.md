# Provenance Template

Use this repository-neutral structure for any number of owner-directed generations. Never invent a generation, owner comment, or decision. Preserve metadata for replaced candidates; retain or remove their PNG files according to repository policy and explicit owner instructions.

## Asset and approved brief

- **Asset role:**
- **Semantic ID and title:**
- **Artwork-pack identity:**
- **Approved brief:** exact approved text or an accurate structured rendering
- **Canonical Pending path:**
- **Canonical production path, if applicable:**
- **Target dimensions and aspect ratio:**
- **Normalization policy:**
- **Provenance destination:**
- **Current queue state:** awaiting generation | awaiting owner decision | Accepted | Pending/Hold | Rejected
- **Cumulative generation count:** actual total

## Generation `<ordinal>`

- **Generation ordinal:** positive integer
- **Trigger:** initial approved brief | owner-directed redo
- **Owner feedback that triggered this generation:** none | exact quotation or faithful rendering
- **Full independent text-only prompt:**

```text
<complete prompt exactly as submitted>
```

- **Attachments:** none
- **Image reference:** none
- **Original dimensions:** `<width> × <height>`
- **Normalization performed:** none | uniform scale | task-authorized crop and uniform scale
- **Saved dimensions:** `<width> × <height>`
- **Saved SHA-256:**
- **Canonical Pending path:**
- **Mechanical checks:** file exists; PNG decodes; dimensions readable; target dimensions/aspect ratio satisfied; no stretching; hash computed; path correct
- **Owner decision for this candidate:** awaiting | Accepted | Redo | Pending/Hold | Rejected
- **Owner decision evidence or feedback:** pending | exact quotation or faithful rendering

Repeat this block once for every actual generation, continuing with `Generation 2`, `Generation 3`, and higher ordinals without a skill-defined limit.

## Owner outcome

- **Final owner decision for this asset:** awaiting | Accepted | Pending/Hold | Rejected
- **Decision evidence:** exact quotation or faithful rendering
- **Accepted generation ordinal, if any:**
- **Production asset path, if accepted:**
- **Production dimensions and SHA-256, if accepted:**
- **Pending or rejected PNG disposition:** retained at canonical Pending path | removed under repository policy | other explicit owner instruction
- **Manifest bookkeeping:** not applicable | pending finalization | updated
- **Notes:** technical or repository facts only; do not add Codex artistic judgments
