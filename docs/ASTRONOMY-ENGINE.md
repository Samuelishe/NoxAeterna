# Astronomy Engine

The astronomy engine is a technical pillar of Nox Aeterna. It owns time conversion, ephemeris access, coordinate conventions, planetary positions, retrogrades, transits, and lunar event direction.

## Time Model

NodaTime is the main time model.

Target pipeline:

```text
LocalDateTime
-> DateTimeZone
-> ZonedDateTime
-> Instant UTC
-> ephemeris calculation
```

Special attention:

- DST.
- Historical timezone changes.
- Ambiguous local times.
- Invalid local times.
- Birth location and timezone lookup.
- Reproducibility.

The system must record how ambiguous or invalid local times are resolved. Silent conversion is not acceptable for birth data.

## Ephemeris Abstraction

Swiss Ephemeris or an equivalent high-quality ephemeris should be hidden behind an interface, likely `IEphemerisCalculator`.

The domain and presentation layers should not reference Swiss Ephemeris packages directly.

The abstraction should eventually expose:

- Planetary positions at an instant.
- Ecliptic longitude.
- Optional latitude and distance.
- Apparent or true position setting, if supported.
- Speed for retrograde detection.
- Calculation flags and ephemeris version metadata.

Package choice is not yet verified. See `KNOWN-PROBLEMS.md`.

## Coordinate Conventions

All ecliptic longitudes must be normalized to 0-360 degrees.

Zodiac sign and degree-within-sign should be derived from normalized longitude.

Calculation settings should be explicit and reproducible. Avoid hidden defaults when they affect chart results.

## Planetary Positions

The MVP should support planetary positions for the standard astrology bodies selected during implementation. The exact initial body list should be decided during Stage 3 and recorded in `DECISIONS-LOG.md`.

Each position should include:

- Body identifier.
- Normalized longitude.
- Zodiac sign.
- Degree within sign.
- Retrograde state where applicable.
- Calculation timestamp.
- Calculation settings.

## Aspects

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

Aspect calculation should be deterministic and unit-tested.

## Retrogrades

Retrograde state should be based on calculated apparent speed or a reliable ephemeris-provided equivalent, not guessed from position deltas unless explicitly documented.

## Transits

Transit snapshots should calculate positions for a selected instant and optionally compare them to natal chart positions.

Future transit support should include:

- Current transits.
- Selected-date transits.
- Natal-to-transit aspects.
- Long-term symbolic history entries.

## Lunar Events

Lunar direction should include:

- Lunar phase calculation.
- New moon and full moon support.
- Future lunar event search.
- Optional connection to Tarot and profile context.

Implementation details are deferred until core astronomy abstractions exist.
