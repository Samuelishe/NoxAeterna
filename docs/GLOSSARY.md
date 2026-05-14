# Glossary

Canonical terminology for Nox Aeterna. Use these meanings consistently across code, documentation, and future agent sessions.

## Astrology and Symbolic Terms

- `natal chart`: A chart calculated from a person's birth moment and location, used as a foundational symbolic reference.
- `wheel`: The circular chart layout showing zodiac divisions, houses, and planetary placements.
- `glyph`: A compact symbolic mark representing a planet, sign, aspect, or related concept.
- `transit`: A planetary position or event calculated for a selected time, often compared against a natal chart.
- `spread`: A defined Tarot layout with named positions and interpretive roles.
- `archetype`: A core symbolic theme or pattern associated with a symbol such as Mars, Scorpio, or the Tower.
- `correspondence`: A structured symbolic association between systems, materials, days, elements, cards, or meanings.
- `zodiac longitude`: A normalized ecliptic longitude in the 0-360 degree range.
- `aspect orb`: The allowed angular tolerance within which an aspect is treated as active.

## Rendering and Geometry Terms

- `geometry model`: Render-independent layout data describing spatial structure such as sectors, anchor points, and aspect lines.
- `rendering model`: Prepared visual-facing data derived from geometry and ready to be consumed by the rendering layer.
- `render scene`: A composed set of render layers and visuals that the rendering engine can draw.
- `render layer`: A logical visual grouping such as wheel background, glyphs, labels, or aspect lines.
- `hit test region`: A defined interactive area used for hover, selection, or inspection.

## Interpretation Terms

- `symbolic factor`: A typed symbolic input such as Mars, Scorpio, 8th house, square Saturn, or reversed Tower.
- `meaning fragment`: A small structured unit of symbolic meaning that can be composed with other fragments.
- `context modifier`: A structured modifier that changes how a meaning fragment is weighted or interpreted in context.
- `interpretation block`: A structured output unit containing contributing factors, themes, tensions, reinforcements, and optional prose.
- `narrative layer`: An optional downstream layer that reshapes structured interpretation into polished atmospheric prose.

## Product Terms

- `profile archive`: The long-lived personal archive for a user's symbolic data, readings, history, and notes.
- `symbolic history`: A chronological record of symbolic events, interpretations, readings, transit snapshots, or reflective entries.
