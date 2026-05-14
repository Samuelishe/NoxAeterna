# Geometry Engine

The geometry engine prepares chart layout data independently from rendering and UI. It should know math, angles, radial placement, and collision rules, but not Avalonia controls.

## Responsibilities

The geometry engine should eventually handle:

- Circular chart layout.
- Zodiac wheel segments.
- House cusp placement.
- Planet glyph radial positions.
- Aspect line endpoints.
- Label and glyph collision avoidance.
- Radial positioning.
- Scaling models.
- Hit-test geometry.
- Export-ready layout models.

## Angular Math

All input longitudes should already be normalized to 0-360 degrees.

Aspect and angle calculations should use circular math, including:

```text
delta = min(abs(a - b), 360 - abs(a - b))
```

Geometry code should avoid UI coordinate assumptions where possible. Use clear coordinate conventions and document orientation choices, such as whether 0 degrees starts at the top, right, ascendant, or Aries origin.

## Geometry Models

Prepared models should be plain data structures suitable for tests and for rendering adapters.

Expected future model categories:

- Chart bounds.
- Rings and radial bands.
- Zodiac segment geometry.
- House segment geometry.
- Planet glyph anchor points.
- Label anchor points.
- Aspect line geometry.
- Selection and hover hit areas.

## Separation From Rendering

Geometry must not depend on Avalonia UI controls. It may define primitives such as points, rectangles, arcs, angles, and rings using domain-owned or geometry-owned types.

Rendering should consume geometry models and decide how to draw them.

## Collision Avoidance

Label and glyph collision avoidance can become complex. MVP should start with a simple deterministic strategy and evolve.

Possible staged approach:

1. Fixed radial slots.
2. Local angular nudging.
3. Multi-ring labels.
4. Leader lines for dense clusters.
5. More advanced layout optimization if needed.

Collision behavior should be tested with dense planet clusters.
