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

## T0-A Implemented Foundation

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

Four concepts must remain independent:

- **Semantic deck** owns stable card identities and the valid Arcana/suit/rank structure. It is independent from images.
- **Artwork pack** will map semantic card identities to illustrations. Different artwork does not create a new semantic deck.
- **Presentation skin** will own programmatic frames, numbers, labels, ornaments, safe areas, and composition rules instead of baking repeated frame/text generation into every illustration.
- **Back variant** will be a selectable visual option, not a separate deck.

Artwork packs, presentation skins, and back variants are terminology and architecture boundaries only in T0-A; no code contracts for them exist yet.

## Built-In Spreads

Two immutable definitions exist:

- `single-card` with internal position ID `card`;
- `three-cards` with ordered internal position IDs `past`, `present`, and `future`.

These IDs are semantic keys, not user-facing labels. Spreads contain no Avalonia geometry, pixel coordinates, localized names, or meanings. Celtic Cross remains future scope.

## Boundary With Meaning and Presentation

Domain contains no Russian or English display names, UI labels, localized strings, meanings, keywords, interpretation text, astrology correspondences, layout geometry, or persistence. Symbolics and Interpretation remain unchanged; future meaning composition follows [`INTERPRETATION-ENGINE.md`](INTERPRETATION-ENGINE.md).

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
