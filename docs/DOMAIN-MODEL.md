# Domain Model

The domain model should express the product's symbolic and archival concepts without depending on UI, persistence, or external calculation libraries.

Domain models must remain language-neutral. Localization keys, localized strings, application language, interpretation language, and theme preferences belong outside the domain layer.

## Core Entities and Value Objects

### PersonalProfile

Central product entity. Expected data:

- Name.
- Birth data.
- Natal chart reference or calculated snapshot.
- Saved Tarot sessions.
- Saved interpretations.
- Symbolic history.
- Current transit snapshots.
- Notes or reflective archive entries.

### BirthData

User-provided birth information. Expected fields:

- Local birth date/time input.
- Birth time accuracy.
- Birth city or settlement display name.
- Explicit timezone ID.
- Optional source note or confidence metadata.

Current implemented direction:

- `LocalBirthDateTime`
- `BirthTimeAccuracy`
- `BirthLocation`
- `TimezoneId`
- `BirthData`

Current workspace integration:

- the astrology workspace now has a presentation-side birth-data input foundation;
- parsing, TZDB timezone selection, and validation stay outside the domain layer;
- valid presentation input is mapped explicitly into `BirthData`.
- manual coordinates are currently the honest offline-first location mode.

### BirthMoment

Resolved time object produced from local birth data:

- Original local date/time.
- Explicit timezone ID.
- UTC instant.
- Ambiguity or invalid-time resolution metadata.
- Birth time accuracy.

Current implemented direction:

- `BirthMoment`
- `TimeResolutionStatus`
- `IBirthMomentResolver`

### BirthLocation

Geographic and administrative location:

- Display name.
- Latitude.
- Longitude.
- Optional country/region identifiers.
- No automatic geocoding in MVP.

### LocalBirthDateTime

User-provided local birth date plus optional local birth time.

Current direction:

- NodaTime `LocalDate`
- Optional NodaTime `LocalTime`
- No timezone resolution by itself

### TimezoneId

Explicit IANA timezone identifier such as `Europe/Moscow`.

Current direction:

- Reproducible string value object
- No automatic lookup in the value object

### BirthTimeAccuracy

Reliability of the provided birth time.

Current direction:

- `UnknownTime`
- `ApproximateTime`
- `ExactTime`

### TimeResolutionStatus

How ambiguous or invalid local time was handled during resolution.

Current direction:

- `Resolved`
- `AmbiguousResolvedEarlier`
- `AmbiguousResolvedLater`
- `InvalidShiftedForward`
- `InvalidRejected`

### NatalChart

Calculated chart snapshot for a profile:

- Birth moment.
- Planetary positions.
- Aspects.
- Optional typed house structure and chart angles.
- Ephemeris/version metadata for reproducibility.

Current implemented direction:

- `NatalChart`
- `CalculatedAspect`
- `PlanetaryAspectCalculator`

The current `NatalChart` is intentionally minimal:

- `BirthMoment`
- read-only `PlanetPosition` collection
- read-only calculated major aspects
- optional ephemeris source metadata
- optional `NatalHouses`, including an explicit unavailable state when a chart has no trustworthy house calculation

### PlanetPosition

Planetary body position:

- Body identifier.
- Ecliptic longitude.
- Zodiac sign.
- Degree within sign.
- Retrograde state.
- Optional speed and latitude.

Current implemented direction:

- `CelestialBody`
- `PlanetPosition`

`PlanetPosition` currently derives zodiac sign and degree-within-sign from `ZodiacLongitude`.

### ZodiacLongitude

Value object for normalized ecliptic longitude.

Rule: all ecliptic longitudes must be normalized to 0-360 degrees.

### ZodiacSign

Twelve-sign zodiac enum or value object. It should be derived from normalized longitude when appropriate.

### House

House number and cusp position. House system support should be explicit and reproducible.

Current implemented direction:

- `HouseSystem` explicitly identifies Placidus as the first implemented system;
- `HouseNumber` accepts only values 1 through 12;
- `HouseCusp` combines a validated house number with normalized `ZodiacLongitude`;
- `ChartAngles` stores ASC and MC and derives the exact opposite DSC and IC values;
- `NatalHouses` owns exactly 12 unique cusps in deterministic numeric order, validates cusp 1 against ASC and cusp 10 against MC, and exposes read-only collections;
- `NatalHousesAvailability` distinguishes complete data, unknown-time unavailability, and calculation unavailability.

House cusps and angles are not celestial bodies and are not stored as `PlanetPosition`.

### Aspect

Relationship between two positions:

- Source body.
- Target body.
- Aspect type.
- Exact angle.
- Orb.
- Applying/separating direction when available.

Current implemented direction:

- `CalculatedAspect` stores a canonical body pair, detected aspect type, angular delta, orb distance, and both longitudes.
- `PlanetaryAspectCalculator` detects major aspects across a position set using the existing orb math.

Initial major aspects:

- Conjunction: 0 degrees.
- Sextile: 60 degrees.
- Square: 90 degrees.
- Trine: 120 degrees.
- Opposition: 180 degrees.

Default orb: 6 degrees.

Angular delta:

```text
delta = min(abs(a - b), 360 - abs(a - b))
```

### TransitSnapshot

Calculated planetary positions for a current or selected time, optionally compared to a natal chart.

### TarotDeck

Implemented semantic deck definition and immutable card collection:

- `TarotDeckId` is a validated stable identity;
- `TarotDeckDefinition` owns a defensively copied, deterministic, read-only card sequence;
- duplicate card IDs and duplicate Minor suit/rank identities are rejected;
- semantic deck identity does not depend on illustrations, frames, backs, filenames, or enum numeric values.

The built-in `standard-78` catalog contains exactly 22 Major and 56 Minor cards. Its deterministic identity order is catalog traversal, not a claim that one historical Major Arcana numbering is universal. A future deck definition must own any display number and deck-specific display order explicitly.

### TarotCard

Implemented language-neutral structural card definition:

- validated `TarotCardId` stable text identity;
- typed `TarotArcana`, `TarotSuit`, and `TarotRank`;
- Major cards have no suit or rank;
- Minor cards require one of four suits and one of fourteen Ace-through-court ranks.

Meanings, localization, and visual references are deliberately outside the card definition.

Current code has validated `TarotArtworkPackId`, `TarotPresentationSkinId`, `TarotBackVariantId`, and `TarotInterpretationSetId` value objects for independent selections. None is interchangeable with `TarotDeckId`; they add no manifests, asset references, or interpretation content to Domain. The frozen implementation target replaces only Set with `TarotInterpretationPackId`/Pack and default `classic`, as owned by [`TAROT-INTERPRETATION-IMPLEMENTATION.md`](TAROT-INTERPRETATION-IMPLEMENTATION.md).

### TarotSpread

Implemented `TarotSpreadId`, `TarotSpreadPositionId`, and immutable `TarotSpreadDefinition`. Position count and deterministic order belong to the definition; empty spreads and duplicate internal position IDs are rejected. Built-ins are one `card` position and ordered `past`, `present`, `future` positions. Names, meanings, and layout geometry remain outside Domain.

### TarotReading

Implemented immutable in-memory `TarotReading` with semantic deck/spread identities, caller-supplied NodaTime `Instant`, and unique `TarotDrawnCard` assignments using typed upright/reversed orientation. Readings are not saved and add no context, interpretation, profile, or persistence contracts.

### InterpretationBlock

Structured user-facing interpretation:

- Title.
- Factors.
- Meaning fragments.
- Tensions and reinforcements.
- Optional prose.
- Source or symbolic basis metadata.

### SymbolicFactor

Atomic symbolic input such as Mars, Scorpio, 8th house, square Saturn, lunar phase, or reversed Tarot card.

Symbolic factors should be composable and typed, not plain unstructured strings.
