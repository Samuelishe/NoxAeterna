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

## MVP Direction

Start with a manageable scope:

- Single card and/or three-card spread.
- Major Arcana first, or symbolic abstract cards before a full 78-card deck.
- Upright/reversed support if it does not inflate the first implementation too much.
- Saved reading model, even before full persistence UI.

## Card Selection

Even if card selection is technically random, the user experience should feel contextual and intentional.

Randomness should be explicit, testable, and injectable where possible. Avoid hidden global random state.

## Card Model

Expected card data:

- Card identity.
- Arcana.
- Suit and rank where applicable.
- Upright symbolic meanings.
- Reversed symbolic meanings.
- Keywords.
- Visual asset reference.
- Optional astrological or elemental correspondences.

## Spread Model

A spread should define:

- Name.
- Number of positions.
- Position meanings.
- Placement order.
- Optional visual layout metadata.

## Reading Model

A reading should preserve:

- Timestamp.
- Deck.
- Spread.
- Drawn cards.
- Orientation.
- Profile context, if any.
- Lunar or transit context, if any.
- Interpretation blocks.

## Interpretation

Tarot interpretation should use the same structured interpretation principles as astrology:

- Symbolic factors.
- Meaning fragments.
- Contextual modifiers.
- Tensions and reinforcements.
- Optional narrative prose from curated templates or lexicon.

## Asset Direction

Tarot assets that ship with the application belong in the repository and must be tracked with provenance, authorship, and license information.

This includes future:

- card backs;
- card illustrations;
- deck-specific ornaments;
- spread-layout decorative assets.

User-specific Tarot runtime data does not belong in the repository or next to the executable. Saved readings, reading history, local notes, and other per-user Tarot state belong in AppData or the equivalent platform-specific user data directory.

AI-assisted asset generation is acceptable for Tarot visuals, but raw generation dumps must not be committed. Only selected, curated, app-ready assets should enter the repository.
