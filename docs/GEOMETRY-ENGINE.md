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

Current implemented convention:

- chart-space `0°` is at the top of the circle;
- angles increase clockwise;
- source zodiac longitude is projected counterclockwise into chart space rather than being treated as a chart-space angle;
- Aries-at-top uses `displayAngle = normalize(-sourceLongitude)`;
- Ascendant-at-left uses `displayAngle = normalize(270° + ascendantLongitude - sourceLongitude)`;
- radial coordinates are expressed as normalized radius ratios rather than pixels.

## Geometry Models

Prepared models should be plain data structures suitable for tests and for rendering adapters.

Expected future model categories:

- `ChartGeometryModel`
- `GlyphPlacement`
- `HitTestRegion`
- Sector, ring, arc, and line primitives

Current implemented direction:

- `AngularPosition`
- `RadialPoint`
- `RadialLaneBounds`
- `ChartRadialLanes`
- `ZodiacSectorGeometry`
- `PlanetGlyphSlot`
- `AspectLineGeometry`
- `CircularChartLayout`
- `CircularChartLayoutBuilder`

Expected future geometry output categories:

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

Geometry must not return Avalonia controls, brushes, pens, `DrawingContext`, or UI objects.

Geometry prepares layout. Rendering decides visual materialization.

The current builder consumes `NatalChart` and produces deterministic zodiac sectors, planet glyph slots, aspect lines, house cusps, house-number anchors, and principal-angle axes without Avalonia types.

## Render Contract Handoff

Likely render-facing handoff objects:

- `ChartGeometryModel`
- `ChartRenderScene`
- `RenderLayer`
- `GlyphPlacement`
- `AspectLineVisual`
- `HouseSectorVisual`
- `HitTestRegion`

`ChartRenderScene` and visual-layer objects belong to rendering-side contracts, not to astronomy or UI orchestration.

Current implemented handoff:

- `CircularChartLayout` is the geometry output consumed by rendering.
- `ChartRenderScene` is the rendering-side wrapper over that layout and materializes vector-glyph placements at geometry-owned anchors.
- Geometry still does not know Avalonia points, brushes, pens, or `DrawingContext`.
- The current astrology workspace host receives rendering-side scene data, while development-only sample scene creation remains outside presentation models.

## Collision Avoidance

Label and glyph collision avoidance can become complex. MVP should start with a simple deterministic strategy and evolve.

Possible staged approach:

1. Fixed radial slots.
2. Local angular nudging.
3. Multi-ring labels.
4. Leader lines for dense clusters.
5. More advanced layout optimization if needed.

Collision behavior should be tested with dense planet clusters.

Current implemented status:

- named non-overlapping zones for the outer boundary, zodiac ring, zodiac glyph lane, planet glyph lane, real house ring, house-number lane, and aspect interior;
- four ordered planet sub-lanes that remain entirely inside the planet lane;
- circular cluster detection by cutting the sorted longitude sequence after its largest gap, so clusters crossing `359°/0°` stay intact;
- deterministic tie-breaking by `CelestialBody`, input-order independence, and stable repeated builds;
- bounded symmetric angular spreading for close clusters, combined with ordered radial sub-lanes and minimum same-lane separation sized for the glyph-plus-degree annotation envelope;
- explicit source astronomical longitude/angle and separate display angle on `PlanetGlyphSlot`;
- aspect endpoints always use source angles and stay inside `AspectInteriorRadiusRatio`.
- `ChartOrientation` applies one counterclockwise source-longitude-to-chart-angle transform to zodiac sectors, glyph midpoints, planets, source ticks, aspects, house cusps, house numbers, and axes;
- available houses place the Ascendant at chart-space 270 degrees (9 o'clock), while unavailable or absent houses preserve Aries at chart-space 0 degrees;
- source `ZodiacLongitude` values remain unchanged by both chart rotation and collision display nudges;
- 12 cusp lines extend from the house-ring interior to the zodiac inner boundary, 12 numeric anchors sit in a dedicated lane just outside the aspect circle, two diameter axes represent ASC–DSC and MC–IC, and their four label anchors sit just outside the outer rim.

The current solver is deliberately small and deterministic. It does not use viewport pixels, font metrics, physics, randomness, or a general-purpose optimizer.

## Current Radial Zone Order

From the center outward:

1. Aspect interior.
2. House ring containing the dedicated house-number lane.
3. Planet glyph lane with ordered sub-lanes.
4. Zodiac ring containing a separate zodiac glyph lane.
5. Outer chart boundary below normalized radius `0.98`.

Rendering is still responsible for fitting known vector bounds into the actual viewport, but it must not move the geometry-owned anchors.
