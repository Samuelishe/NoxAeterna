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

Current input-mode direction:

- the workspace birth-data panel now supports offline manual mode;
- the user selects a TZDB timezone from a local list rather than typing arbitrary timezone text;
- city or settlement name, coordinates, and timezone remain explicit until optional online lookup exists;
- no online geocoding or bundled world-city database is used at this stage.

The system must record how ambiguous or invalid local times are resolved. Silent conversion is not acceptable for birth data.

Current implemented direction:

- Birth-time domain types live in `NoxAeterna.Domain`.
- A resolver contract exists as `IBirthMomentResolver`.
- A first TZDB-based resolver implementation exists in `NoxAeterna.Astronomy`.
- the astrology workspace input is now connected to `IBirthMomentResolver` through an app-level chart pipeline.
- The current deterministic resolver policy is:
  - ambiguous local times resolve to the earlier occurrence;
  - invalid local times shift forward by the gap duration.
  - unknown birth time may still be resolved for the demo chart flow by applying a technical noon fallback while preserving `BirthTimeAccuracy.UnknownTime` in the final `BirthMoment`.

This is an MVP resolution strategy, not the final breadth of time-resolution options.

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

Current contract direction:

- `ChartCalculationRequest`
- `ChartCalculationResult`
- `IEphemerisCalculator`
- `DevelopmentEphemerisCalculator` as a temporary development-only implementation
- `SwissEphemerisCalculator` in `NoxAeterna.Infrastructure` as the first real ephemeris-backed adapter using `SwissEphNet`

The current contract is synchronous and deterministic. It does not expose Swiss Ephemeris package types.
The development-only implementation remains intentionally fake and must not be presented as real astronomy.

Current real-integration spike:

- `SwissEphNet 2.8.0.2` is now wired behind `IEphemerisCalculator`.
- The adapter currently requests Swiss Ephemeris calculation flags with speed data.
- Because no external `.se1` data files are configured yet, the live app currently falls back to the built-in Moshier mode.
- This still produces real astronomical positions and retrograde state, but it is not the final high-precision Swiss-data setup.
- Formal project-license alignment and external ephemeris data packaging remain open follow-up items. See `KNOWN-PROBLEMS.md`.

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

Current implemented direction:

- `CelestialBody` lives in `NoxAeterna.Domain`.
- `PlanetPosition` lives in `NoxAeterna.Domain`.
- `ChartCalculationRequest` and `ChartCalculationResult` live in `NoxAeterna.Astronomy`.
- `ChartCalculationRequest` can now carry optional location context.
- The live app now calculates Sun, Moon, Mercury, Venus, Mars, Jupiter, Saturn, Uranus, Neptune, and Pluto through the SwissEphNet-backed adapter.

## Ephemeris Data Files

Current spike strategy:

- Do not silently vendor large Swiss Ephemeris data files into the repository.
- Keep the first live integration working without external files by allowing SwissEphNet to fall back to built-in Moshier mode.
- Treat external `.se1` file support as a separate setup step with explicit documentation and attribution.

Remaining work:

- decide the installation or app-data strategy for optional `.se1` files;
- document exact redistribution terms before bundling any ephemeris data;
- make the active calculation mode more visible once multiple modes are configurable.

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

Current implemented direction:

- `CalculatedAspect` lives in `NoxAeterna.Domain`.
- `PlanetaryAspectCalculator` lives in `NoxAeterna.Domain`.
- `NatalChart` can be created from a resolved `BirthMoment` and calculated `PlanetPosition` values, then derive major aspects without any Swiss Ephemeris-specific types.

This remains a minimal chart snapshot. Houses, transits, and ephemeris-backed astronomy calculations are still separate concerns.

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
