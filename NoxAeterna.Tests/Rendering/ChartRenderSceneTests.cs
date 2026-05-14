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
    }

    [Fact]
    public void RenderScene_ExposesLayoutCollectionsWithoutNatalChartDependency()
    {
        var scene = ChartRenderScene.FromLayout(CreateLayout());

        Assert.NotNull(scene.Layout);
        Assert.NotEmpty(scene.ZodiacSectors);
        Assert.NotEmpty(scene.PlanetGlyphSlots);
        Assert.NotEmpty(scene.AspectLines);
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
