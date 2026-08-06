# Technical Debt

This file tracks known shortcuts, unresolved questions, and deferred cleanup. It should not duplicate `KNOWN-PROBLEMS.md`; use this file for debt that results from implementation or planning compromises.

## Current Debt

### Tarot tableau can retain stale card dimensions after spread change

- **Owner:** Tarot App layout and refresh boundary.
- **Observed behavior:** The owner reports that switching `three-cards -> single-card` or back can leave cards at the smaller prior size until Draw, card activation, or another later refresh. The initial empty-state/back preview may also be too small. This has not been reproduced or confirmed by automated evidence, and the exact cause is unknown.
- **Deferred investigation:** Check immediate layout invalidation/recalculation after spread selection change, current-reading clear, control resize, section recreation, and language/theme refresh. No code fix belongs to INT0-D1.
- **Cleanup condition:** Card dimensions are correct immediately after a spread selection change, without Draw or card click, including the initial empty-state/back preview.

## Deferred Questions

- Migration tool is not selected.
- Tarot art scope is not finalized.
- Symbolic source bibliography is not curated.

Record future shortcuts here when implementation begins.
