# Known Problems and Risks

This file tracks open risks, unknowns, external dependency questions, and areas likely to become complex.

## Swiss Ephemeris Packaging, Files, and License Alignment

The first spike now uses `SwissEphNet`, but several open items remain:

- the current live app runs in built-in Moshier fallback mode because external `.se1` ephemeris files are not configured yet;
- the repository still needs a formal project-license decision that is compatible with the chosen Swiss Ephemeris licensing path;
- the exact redistribution strategy for any bundled ephemeris data files is still unresolved;
- cross-platform packaging must be verified again if the integration later changes away from the current managed wrapper.

## Functional Vector Glyph Art Direction

Chart symbols no longer depend on Unicode, emoji presentation, or platform font fallback. The current 22 project-owned paths are clean functional graphics, but they are not a final artistic glyph system and may need later optical refinement after user visual review.

## Placidus Geographic Availability and Verification

Placidus is the explicit first house system and may be unavailable at high geographic latitudes. The current adapter treats every Swiss Ephemeris error return as unavailable and deliberately ignores any fallback cusp arrays, preserving the planet chart without fake houses.

The Prague fixture now has golden-value coverage for its UTC instant, ten displayed planet positions, twelve cusps, ASC, and MC. Its counterclockwise screen projection and summary are also deterministic. Independent comparison against the user's trusted chart source and cross-platform visual review remain required before treating the house presentation as broadly validated.

## Birth Place to Timezone Resolution

Birth location to timezone mapping needs design.

Current MVP direction:

- Timezone may be chosen explicitly and manually.
- Reproducibility is prioritized over automation.
- `BirthMoment` must preserve local time, timezone ID, UTC instant, ambiguity resolution, and source/confidence metadata.
- Full historical timezone automation is not considered solved yet.

Open questions:

- Which geocoding source is used?
- Is offline lookup required?
- How are historical timezone changes handled?
- How are ambiguous or invalid local times presented to users?

## Full Tarot Deck Art Scope

A full 78-card deck is large. MVP should avoid committing to full illustrated deck production too early.

## Interpretation Combinatorial Explosion

The interpretation engine can become unmaintainable if it mechanically hardcodes an effectively unbounded space of astrology factors. It should stay layered and compositional for that problem. This risk does not prohibit bounded, owner-approved exhaustive Tarot corpora or thousands of curated authored results; the distinction is owned by [`INTERPRETATION-ENGINE.md`](INTERPRETATION-ENGINE.md).

## Extreme Planet Cluster Presentation

Circular zero-crossing detection and deterministic preferred radial lanes now preserve exact source semantics without geometry-owned angular spreading. Rendering separates exact source markers, bounded glyphs, and independently placed labels; glyph candidates stay inside the source sign and available source house with an eight-degree absolute ceiling and no Cartesian fallback. Protected bounds remain invisible and straight secondary lines are physically interrupted.

Synthetic extreme crowding can still require controlled visual overlap after all semantic candidates are exhausted. The renderer keeps every planet visible and reports this state internally rather than crossing a sign/house or hiding a body. Any future improvement should be driven by concrete screenshot evidence rather than an unbounded optimizer.

Automated coverage confirms deterministic line occlusion and cluster bounds. V1 and V2 Windows dark/light and UnknownTime screenshots were visually accepted. Cross-platform visual smoke remains outstanding but does not justify an unbounded visual-polish stage without a concrete defect.

## Historical Symbolic Sources

Symbolic sources require careful curation. The project should distinguish traditional, modern, editorial, and experimental meanings.

Classic source bibliography and provenance still require deliberate curation. The approved 156 single-card states, 12,012 independently authored oriented-pair states, 468 three-card-position states, and synthesis resources require strict bundle validation, SQLite compilation/inspection, dependency checks, repetition analysis, and style-quality tooling. Their scale is deliberate offline quality work and an authoring risk, not a runtime-architecture blocker.

## Licensing

Future dependencies, ephemeris data, fonts, icons, generated assets, and Tarot art must be checked for license compatibility.

This must be tracked continuously in `README.md` and `docs/THIRD-PARTY.md`.

## Native Dependency Packaging

Swiss Ephemeris wrappers or other astronomy dependencies may introduce native library packaging and distribution complexity across Windows, macOS, and Linux.

This risk should be evaluated before the astronomy package is locked into scaffolded projects.
