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
- project-owned planetary vector glyphs at geometry-owned slots;
- neutral source-longitude ticks and restrained connectors.

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
- planet glyphs render directly at geometry-owned display anchors; the previous extra rendering-side radial offset and large white marker circles are gone;
- true longitude remains visible through a small neutral tick; a restrained connector is drawn only when collision handling actually displaces the annotation;
- `ChartViewport` derives a centered square from the complete control bounds, reserves known stroke and vector extents, exposes safe bounds/effective radius, and clips all chart drawing to that square;
- `ChartVisualMetrics` derives bounded zodiac/planet glyph sizes, annotation text, readable minimum ring/cusp/axis/aspect strokes, anchor/connector strokes, and an aspect scale from the effective radius;
- visual geometry grows with the chart while known vector bounds continue to participate in the viewport safety calculation;
- radial lanes remain geometry contracts, but collision-lane boundaries and aspect-interior bounds are not drawn as unconditional debug guides;
- available house geometry is rendered as 12 readable cusp lines, higher-contrast plain-text house numbers, stronger ASC–DSC and MC–IC axes, and all four compact `ASC`/`DSC`/`MC`/`IC` labels;
- one intentional inner circle anchors the aspect figure and its endpoint markers; technical radial-lane boundaries remain invisible;
- planet annotation groups combine a project-owned vector glyph, two-digit degree-within-sign text, and an optional `R`, while minute precision remains in the table;
- each planet annotation is now one render-owned viewport visual with measured glyph bounds, measured degree/retrograde text bounds, a combined visual envelope, and a minimally padded protected envelope; Avalonia text and DIP measurement remain outside Geometry;
- deterministic render-side placement first respects the geometry-owned display anchor and existing radial sub-lanes, then permits only a small bounded angular correction when measured annotation envelopes would overlap;
- protected annotation bounds remain invisible; planet glyphs and labels render directly on the transparent chart background without rectangles, pills, cards, shadows, or background-colored patches;
- the small render-owned `ChartLineOcclusion` utility converts each straight segment into visible parameter intervals outside measured annotation rectangles, independent of rectangle order;
- ordinary house cusps, principal axes, source ticks, and displacement connectors omit protected annotation intervals physically rather than hiding them with a later fill;
- aspect chords remain inside the aspect circle and do not enter the planet lane, so they are not routed through annotation occlusion without evidence of an intersection;
- displaced-planet connectors terminate at the nearest protected-envelope boundary instead of the glyph center; a connector is absent when neither geometry nor final render placement displaced the annotation;
- principal-angle labels receive a measured render-side safe inset from the outer rim and remain inside the viewport-safe drawing bounds without changing their source angles;
- house lines and aspects are drawn before project-owned zodiac and planet glyphs and the primary labels remain above secondary structure;
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
