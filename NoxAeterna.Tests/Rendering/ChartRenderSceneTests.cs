using Avalonia;
using NodaTime;
using NoxAeterna.Domain.Astrology;
using NoxAeterna.Domain.Birth;
using NoxAeterna.Geometry.Charts;
using NoxAeterna.Rendering.Charts;

namespace NoxAeterna.Tests.Rendering;

public sealed class ChartRenderSceneTests
{
    [Fact]
    public void RenderScene_CreationIsDeterministic()
    {
        var layout = CreateLayout();

        var firstScene = ChartRenderScene.FromLayout(layout);
        var secondScene = ChartRenderScene.FromLayout(layout);

        Assert.Equal(firstScene.ZodiacSectors, secondScene.ZodiacSectors);
        Assert.Equal(firstScene.PlanetGlyphSlots, secondScene.PlanetGlyphSlots);
        Assert.Equal(firstScene.AspectLines, secondScene.AspectLines);
        Assert.Equal(firstScene.HouseCusps, secondScene.HouseCusps);
        Assert.Equal(firstScene.HouseNumberAnchors, secondScene.HouseNumberAnchors);
        Assert.Equal(firstScene.AngleAxes, secondScene.AngleAxes);
        Assert.Equal(firstScene.AngleLabels, secondScene.AngleLabels);
        Assert.Equal(firstScene.ZodiacGlyphs, secondScene.ZodiacGlyphs);
        Assert.Equal(firstScene.PlanetAnnotations, secondScene.PlanetAnnotations);
    }

    [Fact]
    public void VectorCatalogContainsFiniteGeometryForAllChartSymbols()
    {
        var glyphs = Enum.GetValues<ZodiacSign>()
            .Select(ChartGlyphCatalog.GetSignGlyph)
            .Concat(Enum.GetValues<CelestialBody>().Select(ChartGlyphCatalog.GetBodyGlyph))
            .ToArray();

        Assert.Equal(22, glyphs.Length);
        Assert.Equal(22, glyphs.Select(static glyph => glyph.Id).Distinct().Count());
        Assert.All(
            glyphs,
            glyph =>
            {
                Assert.False(string.IsNullOrWhiteSpace(glyph.PathData));
                Assert.True(glyph.UnitBounds.Width > 0d);
                Assert.True(glyph.UnitBounds.Height > 0d);
                Assert.True(double.IsFinite(glyph.UnitBounds.X));
                Assert.DoesNotContain(glyph.Id, static character => character > 127);
            });
    }

    [Fact]
    public void RenderSceneUsesOneCanonicalPlanetAnnotationPipelineAtGeometryOwnedAnchors()
    {
        var scene = ChartRenderScene.FromLayout(CreateLayout());

        Assert.Equal(12, scene.ZodiacGlyphs.Count);
        Assert.Equal(scene.PlanetGlyphSlots.Count, scene.PlanetAnnotations.Count);
        Assert.Equal(
            scene.PlanetGlyphSlots.Select(static slot => slot.AnchorPoint),
            scene.PlanetAnnotations.Select(static annotation => annotation.AnchorPoint));
        Assert.Null(scene.GetType().GetProperty("PlanetGlyphs"));
    }

    [Fact]
    public void AspectStyleExistsForEveryAspectType()
    {
        foreach (var aspectType in Enum.GetValues<AspectType>())
        {
            var style = ChartAspectStyleCatalog.Get(aspectType);

            Assert.True(style.Thickness > 0d);
            Assert.InRange(style.Opacity, 0.70d, 1d);
        }
    }

    [Fact]
    public void ZodiacSectorPalettesRemainVisibleInDarkAndLightThemes()
    {
        foreach (var palette in new[] { ChartRenderPalette.Dark, ChartRenderPalette.Light })
        {
            Assert.InRange(palette.ZodiacSectorOpacity, 0.35d, 0.65d);
            Assert.NotEqual(palette.FireSectorColor, palette.EarthSectorColor);
            Assert.NotEqual(palette.EarthSectorColor, palette.AirSectorColor);
            Assert.NotEqual(palette.AirSectorColor, palette.WaterSectorColor);
        }
    }

    [Fact]
    public void PlanetAnnotationsCarryDegreeRetrogradeAndConditionalDisplacementState()
    {
        var scene = ChartRenderScene.FromLayout(CreateLayout());
        var sun = scene.PlanetAnnotations.Single(annotation => annotation.Body == CelestialBody.Sun);
        var mars = scene.PlanetAnnotations.Single(annotation => annotation.Body == CelestialBody.Mars);

        Assert.Equal("10°", sun.DegreeText);
        Assert.False(sun.IsRetrograde);
        Assert.False(sun.HasDisplacement);
        Assert.Equal("10°", mars.DegreeText);
        Assert.True(mars.IsRetrograde);
        Assert.False(mars.HasDisplacement);
    }

    [Fact]
    public void RenderSceneExposesProviderIndependentHouseGeometry()
    {
        var scene = ChartRenderScene.FromLayout(CreateLayout(withHouses: true));

        Assert.Equal(12, scene.HouseCusps.Count);
        Assert.Equal(12, scene.HouseNumberAnchors.Count);
        Assert.Equal(2, scene.AngleAxes.Count);
        Assert.Equal(
            new[] { "ASC", "DSC", "MC", "IC" },
            scene.AngleLabels.Select(static label => label.Text));
        Assert.DoesNotContain(
            scene.GetType().Assembly.GetReferencedAssemblies(),
            assembly => assembly.Name?.Contains("SwissEph", StringComparison.OrdinalIgnoreCase) == true);
    }

    [Fact]
    public void HouseStyleHierarchyIsDeterministicForCuspsAndBothAxes()
    {
        var firstCusp = ChartHouseStyleCatalog.GetCusp(ChartRenderPalette.Dark);
        var secondCusp = ChartHouseStyleCatalog.GetCusp(ChartRenderPalette.Dark);

        Assert.Equal(firstCusp, secondCusp);
        Assert.True(firstCusp.Opacity >= 0.78d);

        foreach (var axisType in Enum.GetValues<ChartAngleAxisType>())
        {
            var first = ChartHouseStyleCatalog.GetAxis(axisType, ChartRenderPalette.Dark);
            var second = ChartHouseStyleCatalog.GetAxis(axisType, ChartRenderPalette.Dark);
            Assert.Equal(first, second);
            Assert.True(first.ThicknessScale > firstCusp.ThicknessScale * 0.85d);
        }
    }

    [Fact]
    public void ViewportCentersSquareAndKeepsKnownGlyphExtentsInsideSafeBounds()
    {
        var lanes = ChartRadialLanes.Default;
        var options = new ChartRenderOptions();

        var created = ChartViewport.TryCreate(
            new Rect(0d, 0d, 800d, 500d),
            lanes,
            options,
            out var viewport);

        Assert.True(created);
        Assert.Equal(viewport.ChartBounds.Width, viewport.ChartBounds.Height, precision: 10);
        Assert.True(viewport.ChartBounds.X > 0d);
        var zodiacOutermost =
            viewport.Center.X +
            (viewport.EffectiveRadius * lanes.ZodiacGlyphLane.MidpointRadiusRatio) +
            (viewport.VisualMetrics.ZodiacGlyphSize / 2d) +
            (viewport.VisualMetrics.GlyphStrokeThickness / 2d);
        Assert.True(zodiacOutermost <= viewport.SafeDrawingBounds.Right + 1e-9);
        var angleLabelOutermost =
            viewport.Center.X +
            (viewport.EffectiveRadius * lanes.AngleLabelRadiusRatio) +
            (viewport.VisualMetrics.AngleLabelFontSize * 1.1d);
        Assert.True(angleLabelOutermost <= viewport.SafeDrawingBounds.Right + 1e-9);
    }

    [Theory]
    [InlineData(0d, 500d)]
    [InlineData(10d, 10d)]
    public void ViewportRejectsZeroOrTooSmallBounds(double width, double height)
    {
        Assert.False(
            ChartViewport.TryCreate(
                new Rect(0d, 0d, width, height),
                ChartRadialLanes.Default,
                new ChartRenderOptions(),
                out _));
    }

    private static CircularChartLayout CreateLayout(bool withHouses = false)
    {
        var houses = withHouses
            ? NatalHouses.CreateAvailable(
                HouseSystem.Placidus,
                Enumerable.Range(1, 12).Select(index =>
                    new HouseCusp(
                        new HouseNumber(index),
                        new ZodiacLongitude(15d + ((index - 1) * 30d)))),
                new ChartAngles(new ZodiacLongitude(15d), new ZodiacLongitude(285d)))
            : null;
        var chart = NatalChart.Create(
            new BirthMoment(
                new LocalDateTime(1990, 7, 14, 13, 45),
                new TimezoneId("Europe/Moscow"),
                Instant.FromUtc(1990, 7, 14, 9, 45),
                TimeResolutionStatus.Resolved,
                BirthTimeAccuracy.ExactTime,
                "Render fixture"),
            new[]
            {
                new PlanetPosition(CelestialBody.Sun, new ZodiacLongitude(10d), false),
                new PlanetPosition(CelestialBody.Moon, new ZodiacLongitude(100d), false),
                new PlanetPosition(CelestialBody.Mars, new ZodiacLongitude(220d), true)
            },
            houses: houses);

        return new CircularChartLayoutBuilder().Build(chart);
    }
}
