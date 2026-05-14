using Avalonia;
using Avalonia.Media;
using NoxAeterna.Domain.Astrology;
using NoxAeterna.Geometry.Charts;

namespace NoxAeterna.Rendering.Charts;

/// <summary>
/// Draws a minimal circular chart preview from prepared geometry.
/// </summary>
public sealed class CircularChartRenderer
{
    private static readonly IBrush OuterCircleBrush = Brushes.Gray;
    private static readonly IBrush SectorBrush = Brushes.DimGray;
    private static readonly IBrush PlanetMarkerBrush = Brushes.Gainsboro;
    private static readonly IBrush ConjunctionAspectBrush = Brushes.SlateGray;
    private static readonly IBrush SextileAspectBrush = Brushes.SeaGreen;
    private static readonly IBrush SquareAspectBrush = Brushes.IndianRed;
    private static readonly IBrush TrineAspectBrush = Brushes.SteelBlue;
    private static readonly IBrush OppositionAspectBrush = Brushes.Goldenrod;

    /// <summary>
    /// Renders the supplied chart scene into an Avalonia drawing context.
    /// </summary>
    /// <param name="drawingContext">The target drawing context.</param>
    /// <param name="bounds">The chart drawing bounds.</param>
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

        if (bounds.Width <= 0d || bounds.Height <= 0d)
        {
            return;
        }

        options ??= new ChartRenderOptions();

        var center = new Point(bounds.X + bounds.Width / 2d, bounds.Y + bounds.Height / 2d);
        var radius = (Math.Min(bounds.Width, bounds.Height) / 2d) * (1d - options.PaddingRatio * 2d);

        if (radius <= 0d)
        {
            return;
        }

        DrawOuterCircle(drawingContext, center, radius, options);
        DrawSectorSeparators(drawingContext, center, radius, scene.ZodiacSectors, options);
        DrawAspectLines(drawingContext, center, radius, scene.AspectLines, options);
        DrawPlanetMarkers(drawingContext, center, radius, scene.PlanetGlyphSlots, options);
    }

    private static void DrawOuterCircle(
        DrawingContext drawingContext,
        Point center,
        double radius,
        ChartRenderOptions options)
    {
        drawingContext.DrawEllipse(
            brush: null,
            pen: new Pen(OuterCircleBrush, options.OuterCircleStrokeThickness),
            center,
            radius,
            radius);
    }

    private static void DrawSectorSeparators(
        DrawingContext drawingContext,
        Point center,
        double radius,
        IEnumerable<ZodiacSectorGeometry> sectors,
        ChartRenderOptions options)
    {
        var pen = new Pen(SectorBrush, options.SectorLineThickness);

        foreach (var sector in sectors)
        {
            var startPoint = ToPoint(center, radius, new RadialPoint(sector.StartAngle, sector.InnerRadiusRatio));
            var endPoint = ToPoint(center, radius, new RadialPoint(sector.StartAngle, sector.OuterRadiusRatio));
            drawingContext.DrawLine(pen, startPoint, endPoint);
        }
    }

    private static void DrawAspectLines(
        DrawingContext drawingContext,
        Point center,
        double radius,
        IEnumerable<AspectLineGeometry> aspectLines,
        ChartRenderOptions options)
    {
        foreach (var aspectLine in aspectLines)
        {
            var pen = new Pen(GetAspectBrush(aspectLine.AspectType), options.AspectLineThickness);
            var sourcePoint = ToPoint(center, radius, aspectLine.SourcePoint);
            var targetPoint = ToPoint(center, radius, aspectLine.TargetPoint);
            drawingContext.DrawLine(pen, sourcePoint, targetPoint);
        }
    }

    private static void DrawPlanetMarkers(
        DrawingContext drawingContext,
        Point center,
        double radius,
        IEnumerable<PlanetGlyphSlot> glyphSlots,
        ChartRenderOptions options)
    {
        foreach (var glyphSlot in glyphSlots)
        {
            var point = ToPoint(center, radius, glyphSlot.AnchorPoint);
            drawingContext.DrawEllipse(
                brush: PlanetMarkerBrush,
                pen: null,
                point,
                options.PlanetMarkerRadius,
                options.PlanetMarkerRadius);
        }
    }

    private static Point ToPoint(Point center, double radius, RadialPoint radialPoint) =>
        new(
            center.X + radialPoint.X * radius,
            center.Y + radialPoint.Y * radius);

    private static IBrush GetAspectBrush(AspectType aspectType) =>
        aspectType switch
        {
            AspectType.Conjunction => ConjunctionAspectBrush,
            AspectType.Sextile => SextileAspectBrush,
            AspectType.Square => SquareAspectBrush,
            AspectType.Trine => TrineAspectBrush,
            AspectType.Opposition => OppositionAspectBrush,
            _ => OuterCircleBrush
        };
}
