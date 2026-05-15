# Known Problems and Risks

This file tracks open risks, unknowns, external dependency questions, and areas likely to become complex.

## Swiss Ephemeris Packaging, Files, and License Alignment

The first spike now uses `SwissEphNet`, but several open items remain:

- the current live app runs in built-in Moshier fallback mode because external `.se1` ephemeris files are not configured yet;
- the repository still needs a formal project-license decision that is compatible with the chosen Swiss Ephemeris licensing path;
- the exact redistribution strategy for any bundled ephemeris data files is still unresolved;
- cross-platform packaging must be verified again if the integration later changes away from the current managed wrapper.

## Glyph and Font Reliability

The first readable chart foundation now uses Unicode zodiac and planetary glyphs.

Open questions:

- whether the default Avalonia font stack renders every astrology glyph consistently across all target platforms;
- whether a dedicated open-source font should be adopted later for more reliable symbol display;
- how far the current simple glyph placement can go before dense charts require stronger collision handling.

Current mitigation is intentionally light: close-planet clusters now use deterministic radial band offsets, but dense charts can still look crowded.

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

The interpretation engine can become unmaintainable if it hardcodes every possible factor combination. It must stay layered and compositional.

## Chart Label Collision Avoidance

Planet glyphs and labels can cluster tightly. Collision avoidance may become a significant geometry problem.

## Historical Symbolic Sources

Symbolic sources require careful curation. The project should distinguish traditional, modern, editorial, and experimental meanings.

## Licensing

Future dependencies, ephemeris data, fonts, icons, generated assets, and Tarot art must be checked for license compatibility.

This must be tracked continuously in `README.md` and `docs/THIRD-PARTY.md`.

## Native Dependency Packaging

Swiss Ephemeris wrappers or other astronomy dependencies may introduce native library packaging and distribution complexity across Windows, macOS, and Linux.

This risk should be evaluated before the astronomy package is locked into scaffolded projects.
