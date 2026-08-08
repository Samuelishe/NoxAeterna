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
- **Interpretation pack** identifies meaning content independently from both semantic and visual selection.

Current code has validated `TarotArtworkPackId`, `TarotPresentationSkinId`, `TarotBackVariantId`, and `TarotInterpretationPackId` contracts. The workspace exposes Lupus Noctis as its sole user-facing artwork pack, one prototype skin, two programmatic backs, and the prose-free active interpretation-pack identity `classic`; none changes the semantic deck or pretends that prose exists. No parallel interpretation-set identity remains. `prototype-symbolic` remains an internal test/diagnostic rendering seam.

## Built-In Spreads

Three immutable definitions exist:

- `single-card` with internal position ID `card`;
- `two-cards` with technical internal position IDs `slot-a` and `slot-b`, which carry no semantic roles and are never visible labels;
- `three-cards` with ordered internal position IDs `past`, `present`, and `future`.

These IDs are semantic keys, not user-facing labels. Spreads contain no Avalonia geometry, pixel coordinates, localized names, or meanings. A spread becomes selectable when its Domain, Presentation, and UI behavior is implemented; interpretation-pack completeness never gates it. The canonical pack/mode relationship belongs to [`TAROT-INTERPRETATION-PACKS.md`](TAROT-INTERPRETATION-PACKS.md).

Canonical interpretation mode IDs are `single-card`, `two-cards`, `three-cards`, and `celtic-cross`. A mode normally shares its semantic spread ID; the implemented non-positional pair spread is therefore `two-cards`, while any future ordered two-card spread must use another ID and content contract. Mode content and routing belong to [`TAROT-INTERPRETATION-MODES.md`](TAROT-INTERPRETATION-MODES.md).

`classic` (`Классика` / `Classic`) is the built-in selectable interpretation pack; its user-facing names come from the manifest under application UI language and its selection is independent from artwork, back, spread, and interpretation language. Its accepted Russian single-card and two-card modules are `ready = true`: single-card contains 78 cards / 156 independently authored upright/reversed states with five sections, and two-card contains 3,003 canonical pairs / 12,012 independently authored orientation states. The other six locale/mode modules remain unready; the authorial contract belongs to [`TAROT-INTERPRETATION-CONTENT.md`](TAROT-INTERPRETATION-CONTENT.md).

Celtic Cross remains future scope. Its larger card count will require smaller card surfaces than the one-, two-, and three-card modes; exact positions, relation graph, synthesis, layout, and dimensions belong to later design stages. Its `celtic-cross` interpretation module will be compositional rather than exhaustive, and no implementation begins in INT0.

The playable `two-cards` mode draws exactly two distinct cards without replacement, assigns no semantic positions, and resolves one unordered combined interpretation after both cards are revealed. Canonical identity and card-specific orientation attachment remain owned by the Interpretation resolver; Presentation exposes localized pack vocabulary and combined `interaction`/`direction`, and the App uses the existing stacked multi-card reading surface without semantic position labels.

## Boundary With Meaning and Presentation

Domain contains no Russian or English display names, UI labels, localized strings, meanings, keywords, interpretation text, astrology correspondences, layout geometry, or persistence. It owns semantic cards, spreads, unique assignments, typed orientation, and the stable pack identity. `NoxAeterna.Interpretation` owns pure schemas/validation, canonical keys, typed results, locale/mode resolution, trust-chain loading, and bounded caches without filesystem ownership. App owns the single built-in source graph, user-facing manifest catalog, settings normalization, reveal-gated resolver coordinator, and silent UI host; Presentation owns typed selection and workspace signals without filesystem access. Artwork remains a visual mapping only.

## T1 Playable Workspace

`TarotWorkspaceViewModel` in Presentation owns the selected spread, reversal and auto-reveal preferences, current reading, revealed positions, selected assignment, controlled failure, and independent visual selections. It delegates every draw to the existing `TarotDrawEngine` over `StandardTarotCatalog.Deck`; App supplies the explicit timestamp and composes a runtime `SystemTarotRandomSource` from Infrastructure. Reveal state remains presentation policy and never mutates the immutable reading or enters Domain.

The App renders responsive 7:12 card surfaces at an explicit `1.5` scale over the original playable-workspace widths: minimum `216` DIP, preferred multi-card `315` DIP, and single-card `378` DIP. A preferred three-card surface is approximately `315 × 540` DIP per card; compact widths never shrink below the minimum and give horizontal overflow to the tableau only. Black Sun and Lunar Seal remain two selectable prototype backs.

Lupus Noctis resolves every standard card to shipped raster artwork with zero normal-runtime fallback. Programmatic frame, localized title/structure overlay, selection state, and reversal remain separate layers and rotate together under the same 180-degree visual contract. If the required pack is damaged, Draw is disabled and a localized controlled diagnostic is shown; no Classic option or silent prototype fallback appears.

## T-UX1A/T-UX2A/T-UX2B Reading Surface and Reveal Preferences

The fixed control panel sits above one stretching reading surface. Visual composition follows the actual session reading rather than the persisted spread selector: a null reading is the explicit spread-neutral `NoReading` state with one localized guidance message and no card back, tableau, position slot, or interpretation host. Changing to an incompatible spread clears the prior reading and therefore returns to this state until Draw; restart restores only the selector and also begins in `NoReading`.

For `single-card`, a render-independent Presentation contract selects a wide two-column composition only when the content width can hold the accepted `378` DIP card column, a `28` DIP gap, and a `400` DIP minimum interpretation column. The card remains in the bounded left column and fits available height between the existing `216` and `378` DIP width limits; the interpretation column owns vertical scrolling, stays at most `720` DIP, and joins the card in one centered group of at most `1,126` DIP. Both columns share an explicit top alignment. Below the derived threshold, single-card returns to the ordered card → tags → interpretation stack and its outer vertical scroller.

Existing multi-card layout is not redesigned: it retains the ordered outer vertical reading surface, tableau-owned horizontal overflow, equal position-aware cards, and visible semantic position labels. The single-card `card` position label is hidden because it adds no distinguishing information. The former visible tableau heading and selected-card metadata inspector remain absent; the tableau retains a localized automation name, and the spread selector retains its visible control label. The Tarot navigation item also opts out of the shell-owned section header through the normal per-section header contract, so no title row or margin is reserved for the redundant `Таро / Tarot` heading; other sections retain their headers.

Auto reveal defaults to `true`. A successful draw then reveals every position immediately. When disabled, a new draw starts entirely face-down and each card activation reveals only that position. Changing the toggle affects later draws only: it neither reveals nor hides the current reading, resets selection, nor changes the immutable Domain result. Draw failure clears reading, selection, and revealed state.

Until a real corpus and section renderer exist, every `NoContent`, hidden-card, unavailable-pack, and broken-ready result leaves the interpretation host empty and hidden. There is no overall heading, bordered empty surface, placeholder, fallback explanation, readiness state, or technical diagnostic; card drawing and reveal remain usable.

Application language, interpretation language, theme, selected spread, Lupus Noctis ID, selected interpretation-pack ID, back ID, reversal policy, and auto-reveal policy are persisted together in schema-2 AppData `settings.json`. Current readings, revealed positions, selected card, resolved locale/content, diagnostics, and scroll state remain session-only.

## Historical A3 Built-In Partial Artwork Pack

The versioned `lupus-noctis` manifest targets `standard-78`, declares canonical `7:12` and `952 × 1632` source dimensions, and maps exactly three accepted semantic identities to package-relative PNG paths, checksums, status, and owner-document provenance. The pack is explicitly partial: its 75 omitted cards resolve to a localized prototype fallback without changing the reading or semantic identity.

The App-owned read-only loader accepts only shipped package-relative resources and rejects traversal, duplicate or unknown card IDs, invalid dimensions/aspect ratio, invalid status, and checksum mismatch. A malformed built-in pack is disabled with an explicit diagnostic while Classic remains usable; an omitted optional partial-pack card is normal fallback, not a startup failure. User-pack import and arbitrary filesystem paths remain outside this stage.

## Current Post-A26 Artwork State

ART-LN is complete. The current `lupus-noctis` manifest contains 78/78 accepted standard-card raster entries, has 0 partial-pack fallbacks, and declares `partialPack: false`. This does not rewrite the historical A3 integration: A3 proved the partial-pack loader with three raster cards and 75 controlled fallbacks; A26 later completed the same pack while preserving semantic identities and runtime validation.

TAROT-ART-RUNTIME-1 subsequently made Lupus Noctis the sole default user-facing pack. The selector remains visible with exactly that option. Required-pack failure now produces a controlled unavailable workspace instead of selecting Classic or resolving production cards through the internal prototype seam.

## Interpretation Planning Baseline

- `single-card/card`, non-positional `two-cards/slot-a,slot-b`, and ordered `three-cards/past,present,future` are implemented; only the first two currently have ready Russian interpretation modules.
- Draws are without replacement and already support upright and reversed orientation.
- The active `classic` interpretation-pack resolves the complete accepted Russian single-card prose; only `ru + single-card` is ready, while English and multi-card `NoContent` remains silent.
- Schema-v2 source bundles, strict validation/compiler direction, typed package-store resolution, settings-v2, selector, five-section presentation, and reveal-gated orchestration are the current contracts. The accepted Russian single-card corpus is the only production Tarot meaning corpus.
- INT1-AUTH-RU-BULK is accepted at checkpoint `8c6d1ad4744394965eec4b09f155608fa8d6d537`; hosted run 60 passed CI. INT1-PROMOTE-RU performs only the bounded `classic + ru + single-card` readiness promotion.

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
