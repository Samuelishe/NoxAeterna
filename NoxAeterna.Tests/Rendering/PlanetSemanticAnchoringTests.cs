using Avalonia;
using NodaTime;
using NoxAeterna.Domain.Astrology;
using NoxAeterna.Domain.Birth;
using NoxAeterna.Geometry.Charts;
using NoxAeterna.Rendering.Charts;

namespace NoxAeterna.Tests.Rendering;

public sealed class PlanetSemanticAnchoringTests
{
    private static readonly Func<string, double, Size> TextMeasure =
        static (text, size) => new Size(text.Length * size * 0.56d, size * 1.18d);

    [Fact]
    public void SinglePlanetUsesExactPreferredGlyphCandidateAndAlwaysHasSourceMarker()
    {
        var scene = CreateScene(false, (CelestialBody.Sun, 126.5d));
        var viewport = CreateViewport(scene, 760d);

        var layout = Assert.Single(ChartPlanetAnnotationLayoutBuilder.Build(scene, viewport, TextMeasure));
        var slot = Assert.Single(scene.PlanetGlyphSlots);

        Assert.Empty(scene.AspectLines);
        Assert.Equal(slot.SourceAngle, layout.SourceRadialPoint.Angle);
        Assert.Equal(slot.Longitude, layout.GlyphLongitude);
        Assert.Equal(slot.SourceAngle, layout.GlyphRadialPoint.Angle);
        Assert.True(scene.Layout.RadialLanes.PlanetGlyphLane.Contains(layout.GlyphRadialPoint.RadiusRatio));
        Assert.True(layout.SourceMarkerBounds.Width > 0d);
        Assert.Equal(ToPoint(viewport, layout.GlyphRadialPoint), layout.GlyphAnchor);
        Assert.True(IsOnBoundary(layout.SourceLeaderEndpoint, layout.GlyphBounds));
    }

    [Fact]
    public void DenseCapricornClusterUsesRadialCandidatesBeforeBoundedAngles()
    {
        var scene = CreateScene(
            true,
            (CelestialBody.Uranus, 290d),
            (CelestialBody.Neptune, 293d),
            (CelestialBody.Saturn, 298d));
        var viewport = CreateViewport(scene, 900d);

        var layouts = ChartPlanetAnnotationLayoutBuilder.Build(scene, viewport, TextMeasure);

        Assert.Equal(3, layouts.Count);
        Assert.InRange(
            ChartPlanetAnnotationLayoutBuilder.MaximumGlyphAngularAdjustmentDegrees,
            0d,
            8d);
        Assert.All(layouts, layout => AssertSemanticPlacement(scene, layout));
        Assert.All(
            layouts,
            layout => Assert.InRange(
                CircularDelta(layout.GlyphLongitude.Degrees, GetSlot(scene, layout).Longitude.Degrees),
                0d,
                ChartPlanetAnnotationLayoutBuilder.MaximumGlyphAngularAdjustmentDegrees));
        Assert.Equal(3, layouts.Select(static layout => layout.GlyphAnchor).Distinct().Count());
        AssertNoVisualOverlaps(layouts);
    }

    [Fact]
    public void SameLongitudeBodiesUseDifferentRadiiBeforeAnyAngularMovement()
    {
        var scene = CreateScene(
            false,
            (CelestialBody.Sun, 292d),
            (CelestialBody.Moon, 292d));
        var layouts = Build(scene, 1100d);

        Assert.All(layouts, layout => Assert.Equal(292d, layout.GlyphLongitude.Degrees, precision: 10));
        Assert.Equal(2, layouts.Select(static layout => layout.GlyphRadialPoint.RadiusRatio).Distinct().Count());
    }

    [Fact]
    public void SignBoundaryGlyphsRemainInTheirSourceSigns()
    {
        var scene = CreateScene(
            false,
            (CelestialBody.Sun, 29d),
            (CelestialBody.Moon, 31d));

        var layouts = Build(scene, 620d);

        Assert.Equal(ZodiacSign.Aries, layouts.Single(x => x.Annotation.Body == CelestialBody.Sun).SourceSign);
        Assert.Equal(ZodiacSign.Taurus, layouts.Single(x => x.Annotation.Body == CelestialBody.Moon).SourceSign);
        Assert.All(layouts, layout => Assert.Equal(layout.SourceSign, layout.GlyphLongitude.Sign));
    }

    [Fact]
    public void HouseCuspBoundaryGlyphsRemainInTheirSourceHouses()
    {
        var scene = CreateScene(
            true,
            (CelestialBody.Sun, 29.5d),
            (CelestialBody.Moon, 30.5d));

        var layouts = Build(scene, 620d);

        Assert.Equal(1, layouts.Single(x => x.Annotation.Body == CelestialBody.Sun).SourceHouseNumber?.Value);
        Assert.Equal(2, layouts.Single(x => x.Annotation.Body == CelestialBody.Moon).SourceHouseNumber?.Value);
        Assert.All(layouts, layout => AssertSemanticPlacement(scene, layout));
    }

    [Fact]
    public void CircularWrapAtZeroPreservesSignsAndFinitePolarAnchors()
    {
        var scene = CreateScene(
            false,
            (CelestialBody.Sun, 359d),
            (CelestialBody.Moon, 0d));

        var layouts = Build(scene, 620d);

        Assert.All(layouts, layout =>
        {
            Assert.Equal(layout.SourceSign, layout.GlyphLongitude.Sign);
            Assert.True(double.IsFinite(layout.GlyphRadialPoint.Angle.Degrees));
            Assert.Equal(ToPoint(CreateViewport(scene, 620d), layout.GlyphRadialPoint), layout.GlyphAnchor);
        });
    }

    [Fact]
    public void UnknownTimeAppliesSignConstraintWithoutHouseConstraint()
    {
        var scene = CreateScene(
            false,
            (CelestialBody.Sun, 29.2d),
            (CelestialBody.Moon, 29.4d),
            (CelestialBody.Mercury, 29.6d));

        var layouts = Build(scene, 620d);

        Assert.False(scene.Layout.Orientation.IsAscendantOriented);
        Assert.Empty(scene.HouseCusps);
        Assert.All(layouts, layout =>
        {
            Assert.Null(layout.SourceHouseNumber);
            Assert.Equal(layout.SourceSign, layout.GlyphLongitude.Sign);
        });
    }

    [Fact]
    public void ExtremeCrowdingKeepsEveryPlanetVisibleAndReportsControlledCrowding()
    {
        var positions = Enum.GetValues<CelestialBody>()
            .Select(body => (body, 292d))
            .ToArray();
        var scene = CreateScene(true, positions);
        var viewport = CreateViewport(scene, 480d);

        var first = ChartPlanetAnnotationLayoutBuilder.Build(scene, viewport, TextMeasure);
        var second = ChartPlanetAnnotationLayoutBuilder.Build(scene, viewport, TextMeasure);

        Assert.Equal(first, second);
        Assert.Equal(Enum.GetValues<CelestialBody>().Length, first.Count);
        Assert.Contains(first, static layout => layout.IsCrowded);
        Assert.All(first, layout =>
        {
            AssertSemanticPlacement(scene, layout);
            Assert.True(layout.OverlapArea >= 0d);
            Assert.True(viewport.SafeDrawingBounds.Intersects(layout.GlyphBounds));
            Assert.Equal(ToPoint(viewport, layout.GlyphRadialPoint), layout.GlyphAnchor);
        });
    }

    [Fact]
    public void PlanetInputOrderDoesNotChangeGlyphOrLabelLayout()
    {
        var firstScene = CreateScene(
            true,
            (CelestialBody.Sun, 290d),
            (CelestialBody.Moon, 293d),
            (CelestialBody.Mercury, 298d));
        var secondScene = CreateScene(
            true,
            (CelestialBody.Mercury, 298d),
            (CelestialBody.Sun, 290d),
            (CelestialBody.Moon, 293d));

        Assert.Equal(Build(firstScene, 760d), Build(secondScene, 760d));
    }

    [Fact]
    public void LabelMeasurementCanMoveLabelsWithoutMovingGlyphs()
    {
        var scene = CreateScene(
            false,
            (CelestialBody.Sun, 110d),
            (CelestialBody.Moon, 111d),
            (CelestialBody.Mercury, 112d));
        var viewport = CreateViewport(scene, 760d);
        var compact = ChartPlanetAnnotationLayoutBuilder.Build(scene, viewport, TextMeasure);
        var wide = ChartPlanetAnnotationLayoutBuilder.Build(
            scene,
            viewport,
            static (text, size) => new Size(text.Length * size * 0.9d, size * 1.3d));

        Assert.Equal(
            compact.Select(static layout => layout.GlyphAnchor),
            wide.Select(static layout => layout.GlyphAnchor));
        Assert.NotEqual(
            compact.Select(static layout => layout.LabelBounds).ToArray(),
            wide.Select(static layout => layout.LabelBounds).ToArray());
    }

    [Fact]
    public void MovedLabelsUseAssociationLeadersWhileAdjacentSingleLabelDoesNot()
    {
        var dense = Build(
            CreateScene(
                false,
                (CelestialBody.Sun, 110d),
                (CelestialBody.Moon, 111d),
                (CelestialBody.Mercury, 112d),
                (CelestialBody.Venus, 113d)),
            620d);
        var single = Assert.Single(Build(CreateScene(false, (CelestialBody.Sun, 110d)), 620d));

        Assert.Contains(
            dense,
            static layout => layout.HasLabelDisplacement &&
                             layout.LabelLeaderStart is not null &&
                             layout.LabelLeaderEndpoint is not null);
        Assert.All(
            dense.Where(static layout => layout.LabelLeaderStart is not null),
            layout =>
            {
                Assert.True(IsOnBoundary(layout.LabelLeaderStart!.Value, layout.GlyphBounds));
                Assert.True(IsOnBoundary(layout.LabelLeaderEndpoint!.Value, layout.LabelBounds));
            });
        Assert.Null(single.LabelLeaderStart);
        Assert.Null(single.LabelLeaderEndpoint);
    }

    [Fact]
    public void LabelAndGlyphBoundsRemainIndependentWithoutBackgroundVisualContract()
    {
        var layout = Assert.Single(Build(CreateScene(false, (CelestialBody.Saturn, 292d)), 620d));
        var propertyNames = typeof(ChartPlanetAnnotationLayout).GetProperties()
            .Select(static property => property.Name)
            .ToArray();

        Assert.NotEqual(layout.GlyphBounds, layout.LabelBounds);
        Assert.DoesNotContain(propertyNames, static name =>
            name.Contains("Background", StringComparison.Ordinal) ||
            name.Contains("Fill", StringComparison.Ordinal) ||
            name.Contains("Knockout", StringComparison.Ordinal));
    }

    [Fact]
    public void LayoutGeometryIsIndependentOfDarkAndLightPalette()
    {
        var scene = CreateScene(
            true,
            (CelestialBody.Sun, 290d),
            (CelestialBody.Moon, 293d),
            (CelestialBody.Mercury, 298d));
        var dark = CreateViewport(scene, 760d, ChartRenderPalette.Dark);
        var light = CreateViewport(scene, 760d, ChartRenderPalette.Light);

        Assert.Equal(
            ChartPlanetAnnotationLayoutBuilder.Build(scene, dark, TextMeasure),
            ChartPlanetAnnotationLayoutBuilder.Build(scene, light, TextMeasure));
    }

    [Theory]
    [InlineData(480d)]
    [InlineData(620d)]
    [InlineData(900d)]
    public void NormalFixturesKeepGlyphsAndLabelsInsideSafeViewportWithoutVisualOverlap(double side)
    {
        var scene = CreateScene(
            true,
            (CelestialBody.Uranus, 290d),
            (CelestialBody.Neptune, 293d),
            (CelestialBody.Saturn, 298d));
        var viewport = CreateViewport(scene, side);
        var layouts = ChartPlanetAnnotationLayoutBuilder.Build(scene, viewport, TextMeasure);

        AssertNoVisualOverlaps(layouts);
        Assert.All(layouts, layout =>
        {
            AssertContains(viewport.SafeDrawingBounds, layout.GlyphProtectedBounds);
            AssertContains(viewport.SafeDrawingBounds, layout.LabelProtectedBounds);
        });
    }

    [Fact]
    public void SourceLeaderCanBeOccludedAroundForeignAnnotationsWithoutEnteringThem()
    {
        var scene = CreateScene(
            false,
            (CelestialBody.Sun, 110d),
            (CelestialBody.Moon, 111d),
            (CelestialBody.Mercury, 112d));
        var layouts = Build(scene, 620d);
        var layout = layouts[^1];
        var foreign = layouts
            .Take(layouts.Count - 1)
            .SelectMany(static other => new[] { other.GlyphProtectedBounds, other.LabelProtectedBounds })
            .ToArray();

        var visible = ChartLineOcclusion.GetVisibleSegments(
            layout.SourceLeaderStart,
            layout.SourceLeaderEndpoint,
            foreign,
            1d);

        Assert.DoesNotContain(
            visible,
            segment => foreign.Any(bounds => SegmentIntersectsInterior(segment.Source, segment.Target, bounds)));
    }

    [Fact]
    public void HouseNumbersRemainSubordinateAndCuspsExposeExactAngleNotches()
    {
        var scene = CreateScene(true, (CelestialBody.Sun, 126d));
        var metrics = ChartVisualMetrics.Calculate(400d, new ChartRenderOptions());

        Assert.InRange(metrics.HouseNumberFontSize, 12d, 18d);
        Assert.InRange(metrics.HouseNumberOpacity, 0.64d, 0.74d);
        Assert.True(metrics.HouseCuspMarkerStrokeThickness > metrics.HouseCuspStrokeThickness);
        Assert.True(metrics.AngleAxisStrokeThickness > metrics.HouseCuspMarkerStrokeThickness);
        Assert.All(scene.HouseCusps, cusp =>
        {
            Assert.Equal(cusp.DisplayAngle, cusp.NumberLaneMarkerInnerPoint.Angle);
            Assert.Equal(cusp.DisplayAngle, cusp.NumberLaneMarkerOuterPoint.Angle);
        });
    }

    [Fact]
    public void SourceAndLabelLeaderTreatmentsRemainDistinctFromAspectTreatment()
    {
        var metrics = ChartVisualMetrics.Calculate(400d, new ChartRenderOptions());
        var hardAspect = ChartAspectStyleCatalog.Get(AspectType.Square, ChartRenderPalette.Dark);

        Assert.True(metrics.ConnectorStrokeThickness > metrics.LabelLeaderStrokeThickness);
        Assert.NotEqual(ChartRenderPalette.Dark.PlanetAnchorColor, hardAspect.Color);
        Assert.NotEqual(ChartRenderPalette.Dark.PlanetDegreeColor, ChartRenderPalette.Dark.PlanetAnchorColor);
    }

    private static void AssertSemanticPlacement(
        ChartRenderScene scene,
        ChartPlanetAnnotationLayout layout)
    {
        Assert.Equal(layout.SourceSign, layout.GlyphLongitude.Sign);
        if (layout.SourceHouseNumber is not null)
        {
            Assert.Equal(
                layout.SourceHouseNumber.Value,
                ChartHouseMembership.Find(layout.GlyphLongitude, scene.HouseCusps)?.Value);
        }
    }

    private static PlanetGlyphSlot GetSlot(ChartRenderScene scene, ChartPlanetAnnotationLayout layout) =>
        scene.PlanetGlyphSlots.Single(slot => slot.Body == layout.Annotation.Body);

    private static IReadOnlyList<ChartPlanetAnnotationLayout> Build(ChartRenderScene scene, double side)
    {
        var viewport = CreateViewport(scene, side);
        return ChartPlanetAnnotationLayoutBuilder.Build(scene, viewport, TextMeasure);
    }

    private static ChartRenderScene CreateScene(
        bool withHouses,
        params (CelestialBody Body, double Longitude)[] positions)
    {
        var houses = withHouses
            ? NatalHouses.CreateAvailable(
                HouseSystem.Placidus,
                Enumerable.Range(1, 12).Select(index => new HouseCusp(
                    new HouseNumber(index),
                    new ZodiacLongitude((index - 1) * 30d))),
                new ChartAngles(new ZodiacLongitude(0d), new ZodiacLongitude(270d)),
                "synthetic-semantic-layout")
            : null;
        var chart = NatalChart.Create(
            new BirthMoment(
                new LocalDateTime(2000, 1, 1, 12, 0),
                new TimezoneId("Etc/UTC"),
                Instant.FromUtc(2000, 1, 1, 12, 0),
                TimeResolutionStatus.Resolved,
                withHouses ? BirthTimeAccuracy.ExactTime : BirthTimeAccuracy.UnknownTime,
                "Synthetic layout fixture"),
            positions.Select(static position => new PlanetPosition(
                position.Body,
                new ZodiacLongitude(position.Longitude),
                false)),
            houses: houses);
        return ChartRenderScene.FromLayout(new CircularChartLayoutBuilder().Build(chart));
    }

    private static ChartViewport CreateViewport(
        ChartRenderScene scene,
        double side,
        ChartRenderPalette? palette = null)
    {
        Assert.True(ChartViewport.TryCreate(
            new Rect(0d, 0d, side, side),
            scene.Layout.RadialLanes,
            new ChartRenderOptions(palette ?? ChartRenderPalette.Light),
            out var viewport));
        return viewport;
    }

    private static void AssertNoVisualOverlaps(IReadOnlyList<ChartPlanetAnnotationLayout> layouts)
    {
        for (var first = 0; first < layouts.Count; first++)
        {
            Assert.False(Overlaps(layouts[first].GlyphBounds, layouts[first].LabelBounds));
            for (var second = first + 1; second < layouts.Count; second++)
            {
                var firstBounds = new[] { layouts[first].GlyphBounds, layouts[first].LabelBounds };
                var secondBounds = new[] { layouts[second].GlyphBounds, layouts[second].LabelBounds };
                Assert.DoesNotContain(
                    firstBounds,
                    firstBound => secondBounds.Any(secondBound => Overlaps(firstBound, secondBound)));
            }
        }
    }

    private static bool Overlaps(Rect first, Rect second) =>
        Math.Min(first.Right, second.Right) - Math.Max(first.Left, second.Left) > 0.5d &&
        Math.Min(first.Bottom, second.Bottom) - Math.Max(first.Top, second.Top) > 0.5d;

    private static bool IsOnBoundary(Point point, Rect bounds)
    {
        const double tolerance = 1e-7;
        return ((Math.Abs(point.X - bounds.Left) <= tolerance || Math.Abs(point.X - bounds.Right) <= tolerance) &&
                point.Y >= bounds.Top - tolerance && point.Y <= bounds.Bottom + tolerance) ||
               ((Math.Abs(point.Y - bounds.Top) <= tolerance || Math.Abs(point.Y - bounds.Bottom) <= tolerance) &&
                point.X >= bounds.Left - tolerance && point.X <= bounds.Right + tolerance);
    }

    private static void AssertContains(Rect outer, Rect inner)
    {
        Assert.True(inner.Left >= outer.Left - 1e-8);
        Assert.True(inner.Top >= outer.Top - 1e-8);
        Assert.True(inner.Right <= outer.Right + 1e-8);
        Assert.True(inner.Bottom <= outer.Bottom + 1e-8);
    }

    private static bool SegmentIntersectsInterior(Point start, Point end, Rect bounds)
    {
        for (var index = 1; index < 200; index++)
        {
            var ratio = index / 200d;
            var point = new Point(
                start.X + ((end.X - start.X) * ratio),
                start.Y + ((end.Y - start.Y) * ratio));
            if (point.X > bounds.Left && point.X < bounds.Right &&
                point.Y > bounds.Top && point.Y < bounds.Bottom)
            {
                return true;
            }
        }

        return false;
    }

    private static Point ToPoint(ChartViewport viewport, RadialPoint point) =>
        new(
            viewport.Center.X + (point.X * viewport.EffectiveRadius),
            viewport.Center.Y + (point.Y * viewport.EffectiveRadius));

    private static double CircularDelta(double first, double second)
    {
        var delta = Math.Abs(first - second);
        return Math.Min(delta, 360d - delta);
    }
}
