# Rendering Engine

The rendering engine is responsible for drawing prepared chart and symbolic visual models. It is a real subsystem, not a decorative UI layer.

## Rendering Approach

Do not use `Canvas.Children` as the main rendering pipeline.

Prefer:

- Avalonia `CustomControl`.
- `DrawingContext`.
- Prepared rendering models.
- Vector-style rendering.
- DPI-aware scaling.
- Layered rendering.
- Hover and selection support.
- Future SVG/PNG export.
- Future animation support.

Current implemented direction:

- `ChartRenderOptions`
- `ChartRenderScene`
- `CircularChartRenderer`

The current renderer consumes prepared chart geometry and draws:

- outer chart circle;
- zodiac inner boundary and sector separators;
- aspect lines;
- project-owned zodiac vector glyphs around the ring;
- project-owned planetary vector glyphs near geometry-owned source slots;
- authoritative source dots/notches and restrained source-to-glyph leaders.

Current temporary verification path:

```text
BirthDataInput
-> BirthData validation and mapping
-> IBirthMomentResolver
-> IEphemerisCalculator
-> IHouseCalculator
-> NatalChart
-> CircularChartLayoutBuilder
-> ChartRenderScene
-> CircularChartRenderer
-> AstrologyChartSurfaceControl inside the first astrology workspace foundation in NoxAeterna.App
```

Normal application startup is empty and materializes no synthetic chart, positions, or angle summary. Deterministic Prague sample factories remain available only to tests and explicit debug verification; the visible chart appears after validated input runs through the real SwissEphNet-backed path. The current live status is still limited by Moshier fallback mode because external Swiss ephemeris data files are not configured yet.

Current readable-chart foundation details:

- zodiac and planet symbols are original project-owned monochrome path definitions in one stable unit coordinate system;
- chart symbols do not use Unicode, emoji presentation, `Typeface.Default`, external fonts, image downloads, or platform font fallback;
- vector geometry is materialized only after Avalonia initializes its rendering backend, while deterministic path data and unit bounds remain testable without a graphics host;
- every planet renders an authoritative exact-angle source dot and radial notch, whether or not it participates in an aspect;
- a cool, restrained source-to-glyph leader always makes the coordinate/annotation relationship explicit and terminates at the glyph bounds;
- `ChartViewport` derives a centered square from the complete control bounds, reserves known stroke and vector extents, exposes safe bounds/effective radius, and clips all chart drawing to that square;
- `ChartVisualMetrics` derives bounded zodiac/planet glyph sizes, annotation text, readable minimum ring/cusp/axis/aspect strokes, anchor/connector strokes, and an aspect scale from the effective radius;
- visual geometry grows with the chart while known vector bounds continue to participate in the viewport safety calculation;
- radial lanes remain geometry contracts, but collision-lane boundaries and aspect-interior bounds are not drawn as unconditional debug guides;
- available house geometry is rendered as 12 readable cusp lines, higher-contrast plain-text house numbers, stronger ASC–DSC and MC–IC axes, and all four compact `ASC`/`DSC`/`MC`/`IC` labels;
- one intentional inner circle anchors the aspect figure and its endpoint markers; technical radial-lane boundaries remain invisible;
- planet annotation aggregates explicitly separate source marker, glyph, and degree/optional-`R` label visuals, while minute precision remains in the table;
- glyph and label bounds have independent protected geometry; Avalonia text and DIP measurement remain outside Geometry;
- deterministic glyph placement tries the exact source angle across preferred, existing, and intermediate radial candidates before longitude adjustments in one-degree steps, with an absolute eight-degree ceiling;
- every glyph candidate remains in the source zodiac sign and, when houses are reliable, the source house; UnknownTime keeps the sign constraint without inventing a house;
- if semantic candidates remain crowded, the deterministic fallback minimizes overlap without increasing the angular ceiling, crossing a sign/house, hiding a body, or applying Cartesian translation;
- degree/retrograde labels have their own nearby candidate layout. A thinner glyph-to-label leader appears only when a displaced label is no longer adjacent, and label movement never changes glyph/source semantics;
- protected annotation bounds remain invisible; planet glyphs and labels render directly on the transparent chart background without rectangles, pills, cards, shadows, or background-colored patches;
- the small render-owned `ChartLineOcclusion` utility converts each straight segment into visible parameter intervals outside measured annotation rectangles, independent of rectangle order;
- ordinary house cusps, principal axes, source notches, source leaders, and label leaders omit protected visual intervals physically rather than hiding them with a later fill;
- aspect chords remain inside the aspect circle and do not enter the planet lane, so they are not routed through annotation occlusion without evidence of an intersection;
- source leaders terminate at the glyph boundary rather than its center and are visually stronger than optional label leaders while remaining distinct from aspects;
- principal-angle labels receive a measured render-side safe inset from the outer rim and remain inside the viewport-safe drawing bounds without changing their source angles;
- house lines and aspects are drawn before source leaders, source markers, project-owned glyphs, and labels;
- Roman house numbers use a smaller, quieter metric while short exact-angle cusp notches clarify the actual boundaries; principal axes remain stronger than ordinary cusps;
- house digits and Latin angle abbreviations may use ordinary text rendering; astrology symbols remain project-owned vectors;
- the zodiac annulus uses saturated final element-coded sector fills with separate dark/light tones at full opacity, one high-contrast zodiac-glyph role, a strong outer rim, a readable inner boundary, and clear separators;
- zero, non-finite, and too-small surfaces exit without drawing.

## Boundaries

Rendering should receive prepared geometry and rendering models. It should not calculate planetary positions, interpret symbolic meaning, or query persistence.

Current boundary handoff:

```text
CircularChartLayout -> ChartRenderScene -> CircularChartRenderer
```

`CircularChartRenderer` accepts `ChartRenderScene`, `Rect`, and Avalonia `DrawingContext`. It does not accept `NatalChart` directly. `ChartRenderScene.PlanetAnnotations` is the canonical planet visual input; the obsolete duplicate standalone `PlanetGlyphs` layer has been removed.

Allowed responsibilities:

- Draw chart rings, lines, glyphs, labels, and visual layers.
- Convert render models to Avalonia drawing commands.
- Manage visual states such as hover, selected, muted, and highlighted.
- Support export adapters when added.

Not allowed:

- Raw astrology calculation.
- Birth time conversion.
- Interpretation rules.
- Database access.
- UI workflow orchestration.

## Astrology Chart Renderer

The chart renderer should eventually handle:

- Circular chart layout.
- Zodiac wheel.
- Houses.
- Planet glyph placement.
- Aspect lines.
- Label collision display.
- Radial positioning.
- Scaling.
- Export.

Exact astrological diagrams should be rendered programmatically. Generated images must not be used for technical chart output.

Current functional strategy:

- 12 zodiac and 10 planet glyphs are original functional vector graphics, not a final artistic font;
- dark and light chart palettes expose role-based colors selected by the App host, without renderer ownership of shell-theme orchestration; exact roles and values belong to `VISUAL-DESIGN-SYSTEM.md`;
- every supported `AspectType` has an explicit theme-aware style: hard aspects use rose/red, trines use blue, sextiles use teal, and conjunctions use a neutral tone alongside line/marker form;
- conjunctions render as compact local markers instead of arbitrary full chords;
- aspect chords remain entirely inside the geometry-owned aspect interior and use readable, non-sub-pixel minimum weights;
- `ChartHouseStyleCatalog` supplies deterministic cusp and axis hierarchy from chart-local dark/light palettes;
- `ChartRenderScene` exposes provider-independent house geometry and never references SwissEphNet.

## DPI and Scaling

Rendering must be DPI-aware and responsive. Chart visuals should scale without blurred text or distorted glyphs.

Future rendering models should include stable dimensions and scale factors so export and on-screen display can share layout logic.

The current options model carries the chart-local semantic palette plus deterministic baseline stroke/glyph values, border inset, safety margin, and minimum radius. `ChartVisualMetrics` scales those visual values from the effective chart radius within explicit caps, and `ChartViewport` iteratively reserves the resulting known vector extents.

## Assets

Use generated or curated bitmap assets only where appropriate, such as Tarot art, atmospheric backgrounds, or large symbolic illustrations.

Do not rely on generated images for:

- Exact astrological diagrams.
- Glyphs.
- Small icons.
- Text inside images.
- UI layout.
- Technical chart rendering.
