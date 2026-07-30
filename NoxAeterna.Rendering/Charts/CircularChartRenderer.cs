using Avalonia;
using Avalonia.Media;
using NoxAeterna.Domain.Astrology;
using NoxAeterna.Geometry.Charts;

namespace NoxAeterna.Rendering.Charts;

/// <summary>
/// Draws a circular chart scene inside an explicit render-safe viewport.
/// </summary>
public sealed class CircularChartRenderer
{
    /// <summary>
    /// Renders the supplied chart scene into an Avalonia drawing context.
    /// </summary>
    /// <param name="drawingContext">The target drawing context.</param>
    /// <param name="bounds">The complete control bounds.</param>
    /// <param name="scene">The render-ready chart scene.</param>
    /// <param name="options">The rendering options.</param>
    public void Render(
        DrawingContext drawingContext,
        Rect bounds,
        ChartRenderScene scene,
        ChartRenderOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(drawingContext);
        ArgumentNullException.ThrowIfNull(scene);

        options ??= new ChartRenderOptions();

        if (!ChartViewport.TryCreate(bounds, scene.Layout.RadialLanes, options, out var viewport))
        {
            return;
        }

        using var clip = drawingContext.PushClip(viewport.ChartBounds);

        DrawZoneBoundaries(drawingContext, viewport, scene.Layout.RadialLanes, options);
        DrawSectorSeparators(drawingContext, viewport, scene.ZodiacSectors, options);
        DrawAspectLines(drawingContext, viewport, scene.AspectLines);
        DrawPlanetAnchors(drawingContext, viewport, scene.PlanetGlyphSlots, scene.Layout.RadialLanes, options);
        DrawVectorGlyphs(drawingContext, viewport, scene.ZodiacGlyphs, options);
        DrawVectorGlyphs(drawingContext, viewport, scene.PlanetGlyphs, options);
    }

    private static void DrawZoneBoundaries(
        DrawingContext drawingContext,
        ChartViewport viewport,
        ChartRadialLanes lanes,
        ChartRenderOptions options)
    {
        var structureBrush = new SolidColorBrush(options.Palette.StructureColor);
        var zodiacBandBrush = new SolidColorBrush(options.Palette.ZodiacGlyphColor, 0.045d);
        var zodiacBandMidpoint =
            (lanes.OuterBoundaryRadiusRatio + lanes.ZodiacRing.InnerRadiusRatio) / 2d;
        var zodiacBandThickness =
            (lanes.OuterBoundaryRadiusRatio - lanes.ZodiacRing.InnerRadiusRatio) *
            viewport.EffectiveRadius;

        DrawCircle(
            drawingContext,
            viewport,
            zodiacBandMidpoint,
            new Pen(zodiacBandBrush, zodiacBandThickness));

        DrawCircle(
            drawingContext,
            viewport,
            lanes.OuterBoundaryRadiusRatio,
            new Pen(structureBrush, viewport.VisualMetrics.OuterRingStrokeThickness));
        DrawCircle(
            drawingContext,
            viewport,
            lanes.ZodiacRing.InnerRadiusRatio,
            new Pen(
                new SolidColorBrush(options.Palette.SubtleStructureColor, 0.82d),
                viewport.VisualMetrics.StructuralStrokeThickness));
    }

    private static void DrawSectorSeparators(
        DrawingContext drawingContext,
        ChartViewport viewport,
        IEnumerable<ZodiacSectorGeometry> sectors,
        ChartRenderOptions options)
    {
        var pen = new Pen(
            new SolidColorBrush(options.Palette.StructureColor, 0.8d),
            viewport.VisualMetrics.StructuralStrokeThickness);

        foreach (var sector in sectors)
        {
            var startPoint = ToPoint(
                viewport,
                new RadialPoint(sector.StartAngle, sector.InnerRadiusRatio));
            var endPoint = ToPoint(
                viewport,
                new RadialPoint(sector.StartAngle, sector.OuterRadiusRatio));
            drawingContext.DrawLine(pen, startPoint, endPoint);
        }
    }

    private static void DrawAspectLines(
        DrawingContext drawingContext,
        ChartViewport viewport,
        IEnumerable<AspectLineGeometry> aspectLines)
    {
        foreach (var aspectLine in aspectLines)
        {
            var style = ChartAspectStyleCatalog.Get(aspectLine.AspectType);
            var brush = new SolidColorBrush(style.Color, style.Opacity);
            var dashStyle = style.DashPattern is { Count: > 0 }
                ? new DashStyle(style.DashPattern, 0d)
                : null;
            var pen = new Pen(
                brush,
                style.Thickness * viewport.VisualMetrics.AspectScale,
                dashStyle,
                PenLineCap.Round,
                PenLineJoin.Round);
            var sourcePoint = ToPoint(viewport, aspectLine.SourcePoint);
            var targetPoint = ToPoint(viewport, aspectLine.TargetPoint);

            if (aspectLine.AspectType == AspectType.Conjunction)
            {
                var midpoint = new Point(
                    (sourcePoint.X + targetPoint.X) / 2d,
                    (sourcePoint.Y + targetPoint.Y) / 2d);
                var markerRadius = 2.5d * viewport.VisualMetrics.AspectScale;
                drawingContext.DrawEllipse(null, pen, midpoint, markerRadius, markerRadius);
                continue;
            }

            drawingContext.DrawLine(pen, sourcePoint, targetPoint);
        }
    }

    private static void DrawPlanetAnchors(
        DrawingContext drawingContext,
        ChartViewport viewport,
        IEnumerable<PlanetGlyphSlot> glyphSlots,
        ChartRadialLanes lanes,
        ChartRenderOptions options)
    {
        var brush = new SolidColorBrush(options.Palette.PlanetAnchorColor, 0.72d);
        var tickPen = new Pen(brush, viewport.VisualMetrics.AnchorStrokeThickness);
        var connectorPen = new Pen(brush, viewport.VisualMetrics.ConnectorStrokeThickness);
        var tickInnerRadius = lanes.PlanetGlyphLane.OuterRadiusRatio + 0.012d;
        var tickOuterRadius = Math.Min(
            lanes.ZodiacRing.InnerRadiusRatio - 0.012d,
            tickInnerRadius + 0.018d);

        foreach (var glyphSlot in glyphSlots)
        {
            var tickInner = new RadialPoint(glyphSlot.SourceAngle, tickInnerRadius);
            var tickOuter = new RadialPoint(glyphSlot.SourceAngle, tickOuterRadius);
            drawingContext.DrawLine(
                tickPen,
                ToPoint(viewport, tickInner),
                ToPoint(viewport, tickOuter));

            var sourceAnchor = ToPoint(viewport, tickInner);
            var displayAnchor = ToPoint(viewport, glyphSlot.AnchorPoint);
            drawingContext.DrawLine(connectorPen, sourceAnchor, displayAnchor);
        }
    }

    private static void DrawVectorGlyphs(
        DrawingContext drawingContext,
        ChartViewport viewport,
        IEnumerable<ChartGlyphPlacement> glyphs,
        ChartRenderOptions options)
    {
        foreach (var placement in glyphs)
        {
            var anchor = ToPoint(viewport, placement.AnchorPoint);
            var unitBounds = placement.Glyph.UnitBounds;
            var targetSize = placement.Style == ChartGlyphStyle.Zodiac
                ? viewport.VisualMetrics.ZodiacGlyphSize
                : viewport.VisualMetrics.PlanetGlyphSize;
            var scale = targetSize / Math.Max(unitBounds.Width, unitBounds.Height);
            var color = placement.Style == ChartGlyphStyle.Zodiac
                ? options.Palette.ZodiacGlyphColor
                : options.Palette.PlanetGlyphColor;
            var pen = new Pen(
                new SolidColorBrush(color),
                viewport.VisualMetrics.GlyphStrokeThickness / scale,
                null,
                PenLineCap.Round,
                PenLineJoin.Round);

            using var translateToAnchor = drawingContext.PushTransform(
                Matrix.CreateTranslation(anchor.X, anchor.Y));
            using var applyScale = drawingContext.PushTransform(
                Matrix.CreateScale(scale, scale));
            using var centerUnitBounds = drawingContext.PushTransform(
                Matrix.CreateTranslation(-unitBounds.Center.X, -unitBounds.Center.Y));

            drawingContext.DrawGeometry(null, pen, placement.Glyph.CreateGeometry());
        }
    }

    private static void DrawCircle(
        DrawingContext drawingContext,
        ChartViewport viewport,
        double radiusRatio,
        Pen pen)
    {
        var radius = viewport.EffectiveRadius * radiusRatio;
        drawingContext.DrawEllipse(null, pen, viewport.Center, radius, radius);
    }

    private static Point ToPoint(ChartViewport viewport, RadialPoint radialPoint) =>
        new(
            viewport.Center.X + (radialPoint.X * viewport.EffectiveRadius),
            viewport.Center.Y + (radialPoint.Y * viewport.EffectiveRadius));
}
