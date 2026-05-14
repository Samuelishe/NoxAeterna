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

## Boundaries

Rendering should receive prepared geometry and rendering models. It should not calculate planetary positions, interpret symbolic meaning, or query persistence.

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

## DPI and Scaling

Rendering must be DPI-aware and responsive. Chart visuals should scale without blurred text or distorted glyphs.

Future rendering models should include stable dimensions and scale factors so export and on-screen display can share layout logic.

## Assets

Use generated or curated bitmap assets only where appropriate, such as Tarot art, atmospheric backgrounds, or large symbolic illustrations.

Do not rely on generated images for:

- Exact astrological diagrams.
- Glyphs.
- Small icons.
- Text inside images.
- UI layout.
- Technical chart rendering.
