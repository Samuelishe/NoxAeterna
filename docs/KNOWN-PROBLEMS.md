# Known Problems and Risks

This file tracks open risks, unknowns, external dependency questions, and areas likely to become complex.

## Swiss Ephemeris Package Choice

SwissEphNet or an equivalent package must be verified for availability, license fit, .NET 10 compatibility, native dependency behavior, and cross-platform packaging.

## Birth Place to Timezone Resolution

Birth location to timezone mapping needs design.

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
