using Avalonia;
using Avalonia.Media;
using System.Globalization;
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
        var annotationLayouts = ChartPlanetAnnotationLayoutBuilder.Build(scene, viewport, MeasureText);
        var angleLabelLayouts = ChartAngleLabelLayoutBuilder.Build(scene, viewport, MeasureText);

        drawingContext.FillRectangle(
            new SolidColorBrush(options.Palette.InteriorBackgroundColor),
            viewport.ChartBounds);
        DrawZodiacSectorFills(drawingContext, viewport, scene.ZodiacSectors, options);
        DrawZoneBoundaries(drawingContext, viewport, scene.Layout.RadialLanes, options);
        DrawSectorSeparators(drawingContext, viewport, scene.ZodiacSectors, options);
        DrawHouseCusps(drawingContext, viewport, scene.HouseCusps, options);
        DrawAngleAxes(drawingContext, viewport, scene.AngleAxes, options);
        DrawAspectCircle(drawingContext, viewport, scene.Layout.RadialLanes, options);
        DrawAspectLines(drawingContext, viewport, scene.AspectLines);
        DrawAspectEndpointMarkers(drawingContext, viewport, scene.AspectLines, options);
        DrawPlanetSourceTicks(drawingContext, viewport, scene.PlanetGlyphSlots, scene.Layout.RadialLanes, options);
        DrawPlanetConnectors(drawingContext, viewport, annotationLayouts, options);
        DrawAnnotationKnockouts(drawingContext, annotationLayouts, options);
        DrawPlanetAnnotations(drawingContext, viewport, annotationLayouts, options);
        DrawVectorGlyphs(drawingContext, viewport, scene.ZodiacGlyphs, options);
        DrawHouseNumbers(drawingContext, viewport, scene.HouseNumberAnchors, options);
        DrawAngleLabels(drawingContext, viewport, angleLabelLayouts, options);
    }

    private static void DrawHouseCusps(
        DrawingContext drawingContext,
        ChartViewport viewport,
        IEnumerable<HouseCuspGeometry> houseCusps,
        ChartRenderOptions options)
    {
        var style = ChartHouseStyleCatalog.GetCusp(options.Palette);
        var pen = new Pen(
            new SolidColorBrush(style.Color, style.Opacity),
            viewport.VisualMetrics.HouseCuspStrokeThickness * style.ThicknessScale);

        foreach (var cusp in houseCusps)
        {
            drawingContext.DrawLine(
                pen,
                ToPoint(viewport, cusp.InnerPoint),
                ToPoint(viewport, cusp.OuterPoint));
        }
    }

    private static void DrawAngleAxes(
        DrawingContext drawingContext,
        ChartViewport viewport,
        IEnumerable<ChartAngleAxisGeometry> angleAxes,
        ChartRenderOptions options)
    {
        foreach (var axis in angleAxes)
        {
            var style = ChartHouseStyleCatalog.GetAxis(axis.AxisType, options.Palette);
            var pen = new Pen(
                new SolidColorBrush(style.Color, style.Opacity),
                viewport.VisualMetrics.AngleAxisStrokeThickness * style.ThicknessScale,
                null,
                PenLineCap.Round,
                PenLineJoin.Round);

            drawingContext.DrawLine(
                pen,
                ToPoint(viewport, axis.PrimaryPoint),
                ToPoint(viewport, axis.OppositePoint));
        }
    }

    private static void DrawHouseNumbers(
        DrawingContext drawingContext,
        ChartViewport viewport,
        IEnumerable<HouseNumberAnchor> anchors,
        ChartRenderOptions options)
    {
        foreach (var anchor in anchors)
        {
            DrawCenteredText(
                drawingContext,
                FormatHouseNumber(anchor.HouseNumber.Value),
                ToPoint(viewport, anchor.AnchorPoint),
                viewport.VisualMetrics.HouseNumberFontSize,
                new SolidColorBrush(options.Palette.HouseLabelColor, 0.82d));
        }
    }

    private static void DrawAngleLabels(
        DrawingContext drawingContext,
        ChartViewport viewport,
        IEnumerable<ChartAngleLabelLayout> labels,
        ChartRenderOptions options)
    {
        foreach (var label in labels)
        {
            DrawCenteredText(
                drawingContext,
                label.Label.Text,
                label.Anchor,
                viewport.VisualMetrics.AngleLabelFontSize,
                new SolidColorBrush(options.Palette.AngleAxisColor, 0.94d));
        }
    }

    private static void DrawCenteredText(
        DrawingContext drawingContext,
        string text,
        Point anchor,
        double fontSize,
        IBrush brush)
    {
        var formattedText = new FormattedText(
            text,
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            Typeface.Default,
            fontSize,
            brush);
        var origin = new Point(
            anchor.X - (formattedText.Width / 2d),
            anchor.Y - (formattedText.Height / 2d));

        drawingContext.DrawText(formattedText, origin);
    }

    private static Size MeasureText(string text, double fontSize)
    {
        var formattedText = CreateFormattedText(text, fontSize, Brushes.Transparent);
        return new Size(formattedText.Width, formattedText.Height);
    }

    private static FormattedText CreateFormattedText(string text, double fontSize, IBrush brush) =>
        new(
            text,
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            Typeface.Default,
            fontSize,
            brush);

    private static void DrawZoneBoundaries(
        DrawingContext drawingContext,
        ChartViewport viewport,
        ChartRadialLanes lanes,
        ChartRenderOptions options)
    {
        var structureBrush = new SolidColorBrush(options.Palette.StructureColor);

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

    private static void DrawZodiacSectorFills(
        DrawingContext drawingContext,
        ChartViewport viewport,
        IEnumerable<ZodiacSectorGeometry> sectors,
        ChartRenderOptions options)
    {
        foreach (var sector in sectors)
        {
            var color = GetElementSectorColor(sector.Sign, options.Palette);
            drawingContext.DrawGeometry(
                new SolidColorBrush(color, options.Palette.ZodiacSectorOpacity),
                null,
                CreateAnnularSectorGeometry(viewport, sector));
        }
    }

    private static void DrawSectorSeparators(
        DrawingContext drawingContext,
        ChartViewport viewport,
        IEnumerable<ZodiacSectorGeometry> sectors,
        ChartRenderOptions options)
    {
        var pen = new Pen(
            new SolidColorBrush(options.Palette.StructureColor, 0.8d),
            viewport.VisualMetrics.SectorSeparatorStrokeThickness);

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

    private static void DrawAspectCircle(
        DrawingContext drawingContext,
        ChartViewport viewport,
        ChartRadialLanes lanes,
        ChartRenderOptions options) =>
        DrawCircle(
            drawingContext,
            viewport,
            lanes.AspectInteriorRadiusRatio,
            new Pen(
                new SolidColorBrush(options.Palette.SubtleStructureColor, 0.82d),
                viewport.VisualMetrics.AspectCircleStrokeThickness));

    private static void DrawAspectEndpointMarkers(
        DrawingContext drawingContext,
        ChartViewport viewport,
        IEnumerable<AspectLineGeometry> aspectLines,
        ChartRenderOptions options)
    {
        var brush = new SolidColorBrush(options.Palette.PlanetAnchorColor, 0.9d);
        var points = aspectLines
            .SelectMany(static line => new[] { line.SourcePoint, line.TargetPoint })
            .Distinct()
            .ToArray();

        foreach (var point in points)
        {
            drawingContext.DrawEllipse(
                brush,
                null,
                ToPoint(viewport, point),
                viewport.VisualMetrics.AspectEndpointRadius,
                viewport.VisualMetrics.AspectEndpointRadius);
        }
    }

    private static void DrawPlanetSourceTicks(
        DrawingContext drawingContext,
        ChartViewport viewport,
        IEnumerable<PlanetGlyphSlot> glyphSlots,
        ChartRadialLanes lanes,
        ChartRenderOptions options)
    {
        var brush = new SolidColorBrush(options.Palette.PlanetAnchorColor, 0.72d);
        var tickPen = new Pen(brush, viewport.VisualMetrics.AnchorStrokeThickness);
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
        }
    }

    private static void DrawPlanetConnectors(
        DrawingContext drawingContext,
        ChartViewport viewport,
        IEnumerable<ChartPlanetAnnotationLayout> layouts,
        ChartRenderOptions options)
    {
        var brush = new SolidColorBrush(options.Palette.PlanetAnchorColor, 0.72d);
        var connectorPen = new Pen(
            brush,
            viewport.VisualMetrics.ConnectorStrokeThickness,
            null,
            PenLineCap.Round,
            PenLineJoin.Round);

        foreach (var layout in layouts)
        {
            if (layout.HasDisplacement && layout.ConnectorEndpoint is { } endpoint)
            {
                drawingContext.DrawLine(connectorPen, layout.ConnectorStart, endpoint);
            }
        }
    }

    private static void DrawAnnotationKnockouts(
        DrawingContext drawingContext,
        IEnumerable<ChartPlanetAnnotationLayout> layouts,
        ChartRenderOptions options)
    {
        var brush = new SolidColorBrush(options.Palette.InteriorBackgroundColor);

        foreach (var layout in layouts)
        {
            drawingContext.DrawRectangle(
                brush,
                null,
                layout.GlyphProtectedBounds,
                6d,
                6d);
            drawingContext.DrawRectangle(
                brush,
                null,
                layout.LabelProtectedBounds,
                2d,
                2d);
        }
    }

    private static void DrawPlanetAnnotations(
        DrawingContext drawingContext,
        ChartViewport viewport,
        IEnumerable<ChartPlanetAnnotationLayout> layouts,
        ChartRenderOptions options)
    {
        foreach (var layout in layouts)
        {
            var annotation = layout.Annotation;
            DrawVectorGlyph(
                drawingContext,
                layout.FinalAnchor,
                annotation.Glyph,
                viewport.VisualMetrics.PlanetGlyphSize,
                options.Palette.PlanetGlyphColor,
                viewport.VisualMetrics.GlyphStrokeThickness);

            var formattedText = CreateFormattedText(
                ChartPlanetAnnotationLayoutBuilder.GetLabelText(annotation),
                viewport.VisualMetrics.PlanetAnnotationFontSize,
                new SolidColorBrush(options.Palette.PlanetGlyphColor, 0.96d));
            drawingContext.DrawText(
                formattedText,
                new Point(layout.LabelBounds.X, layout.LabelBounds.Y));
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
            var targetSize = placement.Style == ChartGlyphStyle.Zodiac
                ? viewport.VisualMetrics.ZodiacGlyphSize
                : viewport.VisualMetrics.PlanetGlyphSize;
            var color = placement.Style == ChartGlyphStyle.Zodiac
                ? options.Palette.ZodiacGlyphColor
                : options.Palette.PlanetGlyphColor;
            DrawVectorGlyph(
                drawingContext,
                anchor,
                placement.Glyph,
                targetSize,
                color,
                viewport.VisualMetrics.GlyphStrokeThickness);
        }
    }

    private static void DrawVectorGlyph(
        DrawingContext drawingContext,
        Point anchor,
        ChartVectorGlyph glyph,
        double targetSize,
        Color color,
        double strokeThickness)
    {
        var unitBounds = glyph.UnitBounds;
        var scale = targetSize / Math.Max(unitBounds.Width, unitBounds.Height);
        var pen = new Pen(
            new SolidColorBrush(color),
            strokeThickness / scale,
            null,
            PenLineCap.Round,
            PenLineJoin.Round);

        using var translateToAnchor = drawingContext.PushTransform(
            Matrix.CreateTranslation(anchor.X, anchor.Y));
        using var applyScale = drawingContext.PushTransform(
            Matrix.CreateScale(scale, scale));
        using var centerUnitBounds = drawingContext.PushTransform(
            Matrix.CreateTranslation(-unitBounds.Center.X, -unitBounds.Center.Y));

        drawingContext.DrawGeometry(null, pen, glyph.CreateGeometry());
    }

    private static StreamGeometry CreateAnnularSectorGeometry(
        ChartViewport viewport,
        ZodiacSectorGeometry sector)
    {
        const int ArcSegments = 8;
        var geometry = new StreamGeometry();
        using var context = geometry.Open();
        context.BeginFigure(
            ToPoint(viewport, new RadialPoint(sector.StartAngle, sector.OuterRadiusRatio)),
            isFilled: true);

        for (var index = 1; index <= ArcSegments; index++)
        {
            var angle = new AngularPosition(sector.StartAngle.Degrees - (30d * index / ArcSegments));
            context.LineTo(ToPoint(viewport, new RadialPoint(angle, sector.OuterRadiusRatio)));
        }

        context.LineTo(ToPoint(viewport, new RadialPoint(sector.EndAngle, sector.InnerRadiusRatio)));
        for (var index = ArcSegments - 1; index >= 0; index--)
        {
            var angle = new AngularPosition(sector.StartAngle.Degrees - (30d * index / ArcSegments));
            context.LineTo(ToPoint(viewport, new RadialPoint(angle, sector.InnerRadiusRatio)));
        }

        context.EndFigure(isClosed: true);
        return geometry;
    }

    private static Color GetElementSectorColor(
        ZodiacSign sign,
        ChartRenderPalette palette) =>
        ((int)sign % 4) switch
        {
            0 => palette.FireSectorColor,
            1 => palette.EarthSectorColor,
            2 => palette.AirSectorColor,
            3 => palette.WaterSectorColor,
            _ => throw new ArgumentOutOfRangeException(nameof(sign), sign, "Unsupported zodiac sign.")
        };

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

    private static string FormatHouseNumber(int houseNumber) =>
        houseNumber switch
        {
            1 => "I",
            2 => "II",
            3 => "III",
            4 => "IV",
            5 => "V",
            6 => "VI",
            7 => "VII",
            8 => "VIII",
            9 => "IX",
            10 => "X",
            11 => "XI",
            12 => "XII",
            _ => throw new ArgumentOutOfRangeException(nameof(houseNumber), houseNumber, "House number must be between 1 and 12.")
        };
}
