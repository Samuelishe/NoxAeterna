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
        var protectedAnnotationBounds = annotationLayouts
            .SelectMany(static layout => new[]
            {
                layout.SourceMarkerBounds,
                layout.GlyphProtectedBounds,
                layout.LabelProtectedBounds
            })
            .ToArray();

        drawingContext.FillRectangle(
            new SolidColorBrush(options.Palette.InteriorBackgroundColor),
            viewport.ChartBounds);
        DrawZodiacSectorFills(drawingContext, viewport, scene.ZodiacSectors, options);
        DrawZoneBoundaries(drawingContext, viewport, scene.Layout.RadialLanes, options);
        DrawSectorSeparators(drawingContext, viewport, scene.ZodiacSectors, options);
        DrawHouseCusps(
            drawingContext,
            viewport,
            scene.HouseCusps,
            protectedAnnotationBounds,
            options);
        DrawAngleAxes(
            drawingContext,
            viewport,
            scene.AngleAxes,
            protectedAnnotationBounds,
            options);
        DrawAspectCircle(drawingContext, viewport, scene.Layout.RadialLanes, options);
        DrawAspectLines(drawingContext, viewport, scene.AspectLines, options);
        DrawAspectEndpointMarkers(drawingContext, viewport, scene.AspectLines, options);
        DrawPlanetLeaders(drawingContext, viewport, annotationLayouts, options);
        DrawPlanetSourceMarkers(
            drawingContext,
            viewport,
            annotationLayouts,
            scene.Layout.RadialLanes,
            options);
        DrawPlanetAnnotations(drawingContext, viewport, annotationLayouts, options);
        DrawVectorGlyphs(drawingContext, viewport, scene.ZodiacGlyphs, options);
        DrawHouseNumbers(drawingContext, viewport, scene.HouseNumberAnchors, options);
        DrawAngleLabels(drawingContext, viewport, angleLabelLayouts, options);
    }

    private static void DrawHouseCusps(
        DrawingContext drawingContext,
        ChartViewport viewport,
        IEnumerable<HouseCuspGeometry> houseCusps,
        IReadOnlyList<Rect> protectedAnnotationBounds,
        ChartRenderOptions options)
    {
        var style = ChartHouseStyleCatalog.GetCusp(options.Palette);
        var pen = new Pen(
            new SolidColorBrush(style.Color, style.Opacity),
            viewport.VisualMetrics.HouseCuspStrokeThickness * style.ThicknessScale);
        var markerPen = new Pen(
            new SolidColorBrush(style.Color, Math.Min(1d, style.Opacity + 0.12d)),
            viewport.VisualMetrics.HouseCuspMarkerStrokeThickness * style.ThicknessScale,
            null,
            PenLineCap.Round,
            PenLineJoin.Round);

        foreach (var cusp in houseCusps)
        {
            DrawOccludedLine(
                drawingContext,
                pen,
                ToPoint(viewport, cusp.InnerPoint),
                ToPoint(viewport, cusp.OuterPoint),
                protectedAnnotationBounds);
            DrawOccludedLine(
                drawingContext,
                markerPen,
                ToPoint(viewport, cusp.NumberLaneMarkerInnerPoint),
                ToPoint(viewport, cusp.NumberLaneMarkerOuterPoint),
                protectedAnnotationBounds);
        }
    }

    private static void DrawAngleAxes(
        DrawingContext drawingContext,
        ChartViewport viewport,
        IEnumerable<ChartAngleAxisGeometry> angleAxes,
        IReadOnlyList<Rect> protectedAnnotationBounds,
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

            DrawOccludedLine(
                drawingContext,
                pen,
                ToPoint(viewport, axis.PrimaryPoint),
                ToPoint(viewport, axis.OppositePoint),
                protectedAnnotationBounds);
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
                new SolidColorBrush(
                    options.Palette.HouseLabelColor,
                    viewport.VisualMetrics.HouseNumberOpacity));
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
        var structureBrush = new SolidColorBrush(options.Palette.PrimaryStructureColor);

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
                new SolidColorBrush(options.Palette.SecondaryStructureColor, 0.9d),
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
            new SolidColorBrush(options.Palette.PrimaryStructureColor, 0.86d),
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
        IEnumerable<AspectLineGeometry> aspectLines,
        ChartRenderOptions options)
    {
        foreach (var aspectLine in aspectLines)
        {
            var style = ChartAspectStyleCatalog.Get(aspectLine.AspectType, options.Palette);
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
                new SolidColorBrush(options.Palette.AspectCircleColor, 0.92d),
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

    private static void DrawPlanetSourceMarkers(
        DrawingContext drawingContext,
        ChartViewport viewport,
        IEnumerable<ChartPlanetAnnotationLayout> layouts,
        ChartRadialLanes lanes,
        ChartRenderOptions options)
    {
        var brush = new SolidColorBrush(options.Palette.PlanetAnchorColor, 0.94d);
        var notchPen = new Pen(
            brush,
            viewport.VisualMetrics.AnchorStrokeThickness,
            null,
            PenLineCap.Round,
            PenLineJoin.Round);

        foreach (var layout in layouts)
        {
            var inner = new RadialPoint(
                layout.SourceRadialPoint.Angle,
                layout.SourceRadialPoint.RadiusRatio - 0.012d);
            var outer = new RadialPoint(
                layout.SourceRadialPoint.Angle,
                Math.Min(lanes.ZodiacRing.InnerRadiusRatio - 0.008d,
                    layout.SourceRadialPoint.RadiusRatio + 0.012d));
            drawingContext.DrawLine(notchPen, ToPoint(viewport, inner), ToPoint(viewport, outer));
            drawingContext.DrawEllipse(
                brush,
                null,
                layout.SourceAnchor,
                viewport.VisualMetrics.PlanetSourceMarkerRadius,
                viewport.VisualMetrics.PlanetSourceMarkerRadius);
        }
    }

    private static void DrawPlanetLeaders(
        DrawingContext drawingContext,
        ChartViewport viewport,
        IReadOnlyList<ChartPlanetAnnotationLayout> layouts,
        ChartRenderOptions options)
    {
        var sourcePen = new Pen(
            new SolidColorBrush(options.Palette.PlanetAnchorColor, 0.88d),
            viewport.VisualMetrics.ConnectorStrokeThickness,
            null,
            PenLineCap.Round,
            PenLineJoin.Round);
        var labelPen = new Pen(
            new SolidColorBrush(options.Palette.PlanetDegreeColor, 0.58d),
            viewport.VisualMetrics.LabelLeaderStrokeThickness,
            null,
            PenLineCap.Round,
            PenLineJoin.Round);

        foreach (var layout in layouts)
        {
            var sourceOccluders = layouts
                .Select(static other => other.LabelProtectedBounds)
                .Concat(layouts
                    .Where(other => other.Annotation.Body != layout.Annotation.Body)
                    .Select(static other => other.GlyphProtectedBounds));
            DrawOccludedLine(
                drawingContext,
                sourcePen,
                layout.SourceLeaderStart,
                layout.SourceLeaderEndpoint,
                sourceOccluders);

            if (layout.LabelLeaderStart is { } labelStart &&
                layout.LabelLeaderEndpoint is { } labelEndpoint)
            {
                var foreignProtectedBounds = layouts
                    .Where(other => other.Annotation.Body != layout.Annotation.Body)
                    .SelectMany(static other => new[]
                    {
                        other.SourceMarkerBounds,
                        other.GlyphProtectedBounds,
                        other.LabelProtectedBounds
                    });
                DrawOccludedLine(
                    drawingContext,
                    labelPen,
                    labelStart,
                    labelEndpoint,
                    foreignProtectedBounds);
            }
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
                layout.GlyphAnchor,
                annotation.Glyph,
                viewport.VisualMetrics.PlanetGlyphSize,
                options.Palette.PlanetGlyphColor,
                viewport.VisualMetrics.GlyphStrokeThickness);

            var formattedText = CreateFormattedText(
                ChartPlanetAnnotationLayoutBuilder.GetLabelText(annotation),
                viewport.VisualMetrics.PlanetAnnotationFontSize,
                new SolidColorBrush(options.Palette.PlanetDegreeColor));
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

    private static void DrawOccludedLine(
        DrawingContext drawingContext,
        Pen pen,
        Point source,
        Point target,
        IEnumerable<Rect> protectedBounds)
    {
        var margin = (pen.Thickness / 2d) + 0.5d;
        foreach (var segment in ChartLineOcclusion.GetVisibleSegments(
                     source,
                     target,
                     protectedBounds,
                     margin))
        {
            drawingContext.DrawLine(pen, segment.Source, segment.Target);
        }
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
