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

The current renderer is intentionally minimal and technical. It consumes prepared chart geometry and draws:

- outer chart circle;
- explicit visual-zone boundaries;
- zodiac sector separators;
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
-> NatalChart
-> CircularChartLayoutBuilder
-> ChartRenderScene
-> CircularChartRenderer
-> AstrologyChartSurfaceControl inside the first astrology workspace foundation in NoxAeterna.App
```

The app still keeps a fallback sample-scene path for development, but the visible chart now rebuilds from validated input through the real SwissEphNet-backed path. The current live status is still limited by Moshier fallback mode because external Swiss ephemeris data files are not configured yet.

Current readable-chart foundation details:

- zodiac and planet symbols are original project-owned monochrome path definitions in one stable unit coordinate system;
- chart symbols do not use Unicode, emoji presentation, `Typeface.Default`, external fonts, image downloads, or platform font fallback;
- vector geometry is materialized only after Avalonia initializes its rendering backend, while deterministic path data and unit bounds remain testable without a graphics host;
- planet glyphs render directly at geometry-owned display anchors; the previous extra rendering-side radial offset and large white marker circles are gone;
- true longitude remains visible through a small neutral tick and connector without replacing the astronomical source angle;
- `ChartViewport` derives a centered square from the complete control bounds, reserves known stroke and vector extents, exposes safe bounds/effective radius, and clips all chart drawing to that square;
- zero, non-finite, and too-small surfaces exit without drawing.

## Boundaries

Rendering should receive prepared geometry and rendering models. It should not calculate planetary positions, interpret symbolic meaning, or query persistence.

Current boundary handoff:

```text
CircularChartLayout -> ChartRenderScene -> CircularChartRenderer
```

`CircularChartRenderer` accepts `ChartRenderScene`, `Rect`, and Avalonia `DrawingContext`. It does not accept `NatalChart` directly.

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
- dark and light chart palettes are render-facing inputs selected by the App host, without renderer ownership of shell-theme orchestration;
- every supported `AspectType` has an explicit restrained style using thickness, opacity, muted color, and optional dash pattern;
- conjunctions render as compact local markers instead of arbitrary full chords;
- aspect chords remain entirely inside the geometry-owned aspect interior.

## DPI and Scaling

Rendering must be DPI-aware and responsive. Chart visuals should scale without blurred text or distorted glyphs.

Future rendering models should include stable dimensions and scale factors so export and on-screen display can share layout logic.

The current options model carries the chart-local palette plus deterministic stroke, glyph-size, border-inset, safety-margin, and minimum-radius values. Known vector unit bounds participate in effective-radius calculation.

## Assets

Use generated or curated bitmap assets only where appropriate, such as Tarot art, atmospheric backgrounds, or large symbolic illustrations.

Do not rely on generated images for:

- Exact astrological diagrams.
- Glyphs.
- Small icons.
- Text inside images.
- UI layout.
- Technical chart rendering.
