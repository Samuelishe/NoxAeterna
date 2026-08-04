# Provenance Template

Use this repository-neutral structure for one through four generations. Omit unused generation blocks; never invent attempts. Preserve metadata for superseded candidates but do not retain their PNG files unless the governing task explicitly requires it.

## Asset

- **Asset role:**
- **Semantic identity:**
- **Artwork-pack identity:**
- **Canonical output path:**
- **Target dimensions / aspect ratio:**
- **Provenance destination:**
- **Owner attempt override:** none | details
- **Cumulative generation count:** 1–4 by default
- **Current status:** `Pending owner review` | `blocking defect remains` | `attempt budget exhausted` | `owner Accepted` | `owner Rejected`

## Generation `<G1|G2|G3|G4>`

- **Generation ordinal:** `<1|2|3|4>`
- **Generation mode:** `initial` | `targeted correction` | `critical recovery` | `absurdity rescue`
- **Full text-only prompt:**

```text
<complete prompt exactly as submitted>
```

- **Image reference:** `none` by default | explicit override and source
- **Original dimensions:** `<width> × <height>`
- **Normalized dimensions:** `<width> × <height>`
- **Normalized SHA-256:**
- **Literal blocking defects:** none | exact observations
- **Non-blocking notes:** none | exact observations
- **Hard-contract review:** pass | fail, with evidence
- **Technical review:** pass | fail, with evidence
- **Meaning/role review:** pass | fail, with evidence
- **Independent absurdity review:** not reached | pass | fail, with evidence
- **Decision:** `superseded` | `Pending owner review` | `owner Accepted` | `owner Rejected`
- **Cumulative generation count after this attempt:**
- **Next legal transition:** owner handoff | `G2` | `G3` | `G4` | stop; owner override required

Repeat the generation block for each actual attempt, up to `G4` by default.

## Owner handoff

- **Canonical candidate:**
- **Normalized dimensions:**
- **Normalized SHA-256:**
- **Generations used / default maximum:** `<n> / 4`
- **Unused attempts:** no obligation to spend them
- **Remaining blockers:**
- **Technical eligibility:** eligible for owner review | ineligible
- **Owner decision:** pending | Accepted with explicit citation | Rejected with explicit citation
- **Production promotion:** not performed unless separately authorized
