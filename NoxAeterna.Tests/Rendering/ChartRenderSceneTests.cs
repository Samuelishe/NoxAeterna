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
        Assert.Equal(firstScene.ZodiacGlyphs, secondScene.ZodiacGlyphs);
        Assert.Equal(firstScene.PlanetGlyphs, secondScene.PlanetGlyphs);
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
    public void RenderSceneUsesVectorGlyphsAtGeometryOwnedAnchors()
    {
        var scene = ChartRenderScene.FromLayout(CreateLayout());

        Assert.Equal(12, scene.ZodiacGlyphs.Count);
        Assert.Equal(scene.PlanetGlyphSlots.Count, scene.PlanetGlyphs.Count);
        Assert.Equal(
            scene.PlanetGlyphSlots.Select(static slot => slot.AnchorPoint),
            scene.PlanetGlyphs.Select(static glyph => glyph.AnchorPoint));
    }

    [Fact]
    public void AspectStyleExistsForEveryAspectType()
    {
        foreach (var aspectType in Enum.GetValues<AspectType>())
        {
            var style = ChartAspectStyleCatalog.Get(aspectType);

            Assert.True(style.Thickness > 0d);
            Assert.InRange(style.Opacity, 0d, 1d);
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
            (options.ZodiacGlyphSize / 2d) +
            (options.GlyphStrokeThickness / 2d);
        Assert.True(zodiacOutermost <= viewport.SafeDrawingBounds.Right + 1e-9);
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

    private static CircularChartLayout CreateLayout()
    {
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
            });

        return new CircularChartLayoutBuilder().Build(chart);
    }
}
