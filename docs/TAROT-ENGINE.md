# Tarot Engine

The Tarot module should feel ritualized, calm, atmospheric, deliberate, and visually refined. It must not feel like a cheap random card generator.

## Goals

Eventually support:

- Single-card readings.
- Three-card spreads.
- Celtic Cross.
- Relationship spreads.
- Upright and reversed cards.
- Saved sessions.
- Interpretation history.
- Optional connection to personal profile.
- Optional connection to current lunar phase, transits, and planetary context.

## Implemented Foundation

The Domain layer now owns language-neutral, immutable contracts for:

- validated deck, card, spread, and spread-position identities;
- Major/Minor Arcana, four suits, and fourteen Minor ranks;
- semantic card and deck definitions;
- ordered spread definitions;
- upright/reversed assignments and immutable in-memory readings;
- typed draw success/failure and an injected bounded-index randomness contract.

`StandardTarotCatalog` provides exactly 78 structural identities: 22 Major cards and all 56 unique four-suit/fourteen-rank Minor combinations. IDs and catalog construction do not depend on images, filenames, file order, localization, or enum numeric values. The deterministic catalog iteration order is not a universal historical numbering; future display numbers and deck-specific presentation order must be explicit metadata.

## Card Selection

Even if card selection is technically random, the user experience should feel contextual and intentional.

`TarotDrawEngine` draws without replacement through project-owned `ITarotRandomSource`. `UprightOnly` consumes no orientation randomness; `UprightAndReversed` uses the same injected source explicitly. The caller supplies the NodaTime timestamp. There is no ambient clock, shared RNG, automatic persistence, or hidden mutable state. An undersized deck returns a typed `InsufficientDeckSize` failure; invalid random indices and invalid contracts are not silently normalized.

## Independent Concepts

Five concepts must remain independent:

- **Semantic deck** owns stable card identities and the valid Arcana/suit/rank structure. It is independent from images.
- **Artwork pack** maps semantic card identities to illustrations. Different artwork does not create a new semantic deck.
- **Presentation skin** owns programmatic frames, numbers, labels, ornaments, safe areas, and composition rules instead of baking repeated frame/text generation into every illustration.
- **Back variant** owns a selectable card back, not a separate deck.
- **Interpretation set** identifies future meaning content independently from both semantic and visual selection.

Validated `TarotArtworkPackId`, `TarotPresentationSkinId`, `TarotBackVariantId`, and `TarotInterpretationSetId` contracts keep those selections distinct from `TarotDeckId`. The workspace exposes Lupus Noctis as its sole user-facing artwork pack through an explicit default ID, plus one prototype skin, two programmatic back variants, and one foundation interpretation-set identity; none changes the semantic deck or pretends that interpretation prose exists. `prototype-symbolic` remains an internal test/diagnostic rendering seam, not a user-facing Classic option.

## Built-In Spreads

Two immutable definitions exist:

- `single-card` with internal position ID `card`;
- `three-cards` with ordered internal position IDs `past`, `present`, and `future`.

These IDs are semantic keys, not user-facing labels. Spreads contain no Avalonia geometry, pixel coordinates, localized names, or meanings. A spread becomes selectable when its Domain, Presentation, and UI behavior is implemented; interpretation-pack completeness never gates it. The canonical pack/mode relationship belongs to [`TAROT-INTERPRETATION-PACKS.md`](TAROT-INTERPRETATION-PACKS.md).

Celtic Cross remains future scope. Its larger card count will require smaller card surfaces than the single-, two-, and three-card modes; exact layout and dimensions belong to a separate UX design stage. Its meanings will be a separate interpretation mode module, and neither semantics nor implementation begins in INT0-D1.

The planned two-card combination mode does not exist in Domain or UI. Owner direction is to draw exactly two distinct cards without replacement and make the primary result one pair meaning, not necessarily a positional “card 1/card 2” model. Pair ordering, canonical identity, orientation composition, schema, and content granularity remain INT0 open decisions owned by [`INTERPRETATION-ENGINE.md`](INTERPRETATION-ENGINE.md).

## Boundary With Meaning and Presentation

Domain contains no Russian or English display names, UI labels, localized strings, meanings, keywords, interpretation text, astrology correspondences, layout geometry, or persistence. It owns semantic cards, spreads, unique assignments, and typed orientation. `NoxAeterna.Interpretation` owns future structured meaning composition but currently contains only its project boundary and no Tarot runtime or corpus. Presentation orchestrates selections and will display prepared results without owning prose or depending on a concrete corpus storage format. Artwork remains a visual mapping only. General structured interpretation belongs to [`INTERPRETATION-ENGINE.md`](INTERPRETATION-ENGINE.md); Tarot pack identity, readiness, fallback, and silent absence belong to [`TAROT-INTERPRETATION-PACKS.md`](TAROT-INTERPRETATION-PACKS.md).

## T1 Playable Workspace

`TarotWorkspaceViewModel` in Presentation owns the selected spread, reversal and auto-reveal preferences, current reading, revealed positions, selected assignment, controlled failure, and independent visual selections. It delegates every draw to the existing `TarotDrawEngine` over `StandardTarotCatalog.Deck`; App supplies the explicit timestamp and composes a runtime `SystemTarotRandomSource` from Infrastructure. Reveal state remains presentation policy and never mutates the immutable reading or enters Domain.

The App renders responsive 7:12 card surfaces at an explicit `1.5` scale over the original playable-workspace widths: minimum `216` DIP, preferred multi-card `315` DIP, and single-card `378` DIP. A preferred three-card surface is approximately `315 × 540` DIP per card; compact widths never shrink below the minimum and give horizontal overflow to the tableau only. Black Sun and Lunar Seal remain two selectable prototype backs.

Lupus Noctis resolves every standard card to shipped raster artwork with zero normal-runtime fallback. Programmatic frame, localized title/structure overlay, selection state, and reversal remain separate layers and rotate together under the same 180-degree visual contract. If the required pack is damaged, Draw is disabled and a localized controlled diagnostic is shown; no Classic option or silent prototype fallback appears.

## T-UX1A Reading Surface and Reveal Preferences

The fixed control panel sits above one stretching reading surface. That surface owns the only vertical Tarot scrollbar and contains, in order, the tableau viewport and a full-width interpretation content host. The tableau alone owns horizontal overflow. The former visible tableau heading and selected-card metadata inspector are not materialized; the tableau retains a localized automation name, and the spread selector retains its visible control label.

Auto reveal defaults to `true`. A successful draw then reveals every position immediately. When disabled, a new draw starts entirely face-down and each card activation reveals only that position. Changing the toggle affects later draws only: it neither reveals nor hides the current reading, resets selection, nor changes the immutable Domain result. Draw failure clears reading, selection, and revealed state.

Until a real corpus exists, the current implementation keeps the interpretation host empty with no reading or revealed cards and shows the localized unavailable message after a reveal. This is a temporary honest placeholder, not the future missing-content policy. The first real interpretation implementation stage removes `ui.tarot.interpretation.unavailable` from production UI; thereafter no displayable content means an empty host with no heading, bordered surface, placeholder, or diagnostic copy, as defined by [`TAROT-INTERPRETATION-PACKS.md`](TAROT-INTERPRETATION-PACKS.md).

Application language, interpretation language, theme, selected spread, Lupus Noctis ID, back ID, reversal policy, and auto-reveal policy are persisted together in the versioned AppData `settings.json`. Current readings, revealed positions, selection, and scroll state remain session-only.

## Historical A3 Built-In Partial Artwork Pack

The versioned `lupus-noctis` manifest targets `standard-78`, declares canonical `7:12` and `952 × 1632` source dimensions, and maps exactly three accepted semantic identities to package-relative PNG paths, checksums, status, and owner-document provenance. The pack is explicitly partial: its 75 omitted cards resolve to a localized prototype fallback without changing the reading or semantic identity.

The App-owned read-only loader accepts only shipped package-relative resources and rejects traversal, duplicate or unknown card IDs, invalid dimensions/aspect ratio, invalid status, and checksum mismatch. A malformed built-in pack is disabled with an explicit diagnostic while Classic remains usable; an omitted optional partial-pack card is normal fallback, not a startup failure. User-pack import and arbitrary filesystem paths remain outside this stage.

## Current Post-A26 Artwork State

ART-LN is complete. The current `lupus-noctis` manifest contains 78/78 accepted standard-card raster entries, has 0 partial-pack fallbacks, and declares `partialPack: false`. This does not rewrite the historical A3 integration: A3 proved the partial-pack loader with three raster cards and 75 controlled fallbacks; A26 later completed the same pack while preserving semantic identities and runtime validation.

TAROT-ART-RUNTIME-1 subsequently made Lupus Noctis the sole default user-facing pack. The selector remains visible with exactly that option. Required-pack failure now produces a controlled unavailable workspace instead of selecting Classic or resolving production cards through the internal prototype seam.

## Interpretation Planning Baseline

- Only `single-card/card` and ordered `three-cards/past,present,future` are implemented.
- Draws are without replacement and already support upright and reversed orientation.
- The foundation interpretation set contains no prose, and the UI reports interpretation unavailability.
- No two-card spread, Tarot interpretation runtime, production schema, or Tarot meaning corpus exists.
- INT0-D1 package/localization documentation is complete, while INT0 owner discussion remains in progress and INT0-D2 is next. No selector, manifest, runtime, schema, or corpus implementation was added.

## Asset Direction

Tarot assets that ship with the application belong in the repository and must be tracked with provenance, authorship, and license information.

This includes future:

- card backs;
- card illustrations;
- deck-specific ornaments;
- spread-layout decorative assets.

User-specific Tarot runtime data does not belong in the repository or next to the executable. Saved readings, reading history, local notes, and other per-user Tarot state belong in AppData or the equivalent platform-specific user data directory.

AI-assisted asset generation is acceptable for Tarot visuals, but raw generation dumps must not be committed. Only selected, curated, app-ready assets should enter the repository.

Future generated artwork that ships is curated project-owned repository content. Future user-provided visual packs and other user-specific runtime data belong in AppData. A later generic asset contract will define packs, skins, backs, manifests, safe areas, and tooling without changing semantic card identity.
