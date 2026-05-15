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
- zodiac sector separators;
- aspect lines;
- zodiac glyph labels around the ring;
- planetary glyph labels at planet slots.

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

- zodiac ring labels are derived from sector geometry and rendered from scene text labels;
- planet glyph labels are derived from prepared glyph slots rather than astronomy services inside rendering;
- the renderer currently prefers text-presentation Unicode glyphs to reduce emoji-style rendering where the platform font stack allows it;
- simple label-overlap mitigation currently comes from geometry-level radial band offsets, not from a heavy rendering-side collision solver.

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

Current placeholder strategy:

- universal zodiac and planetary Unicode glyphs for the first readable chart foundation;
- deterministic aspect line colors by aspect type;
- no final art direction, dense-label collision handling, or final font strategy yet;
- app-level theme switching may change preview surface resources around the renderer, but rendering still does not own shell-theme orchestration.
- text-presentation selectors are now preferred for astrology glyphs to avoid emoji-style rendering where the platform font stack allows it.

If platform font behavior still renders some astrology glyphs poorly, fall back to compact text labels before introducing custom image assets.

## DPI and Scaling

Rendering must be DPI-aware and responsive. Chart visuals should scale without blurred text or distorted glyphs.

Future rendering models should include stable dimensions and scale factors so export and on-screen display can share layout logic.

The current options model carries only numeric drawing parameters such as padding ratio and stroke thickness.

## Assets

Use generated or curated bitmap assets only where appropriate, such as Tarot art, atmospheric backgrounds, or large symbolic illustrations.

Do not rely on generated images for:

- Exact astrological diagrams.
- Glyphs.
- Small icons.
- Text inside images.
- UI layout.
- Technical chart rendering.
