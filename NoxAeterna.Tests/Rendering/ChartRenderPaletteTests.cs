using Avalonia.Media;
using NoxAeterna.Domain.Astrology;
using NoxAeterna.Rendering.Charts;

namespace NoxAeterna.Tests.Rendering;

public sealed class ChartRenderPaletteTests
{
    [Fact]
    public void DarkAndLightPalettesExposeEverySemanticChartRole()
    {
        var colorProperties = typeof(ChartRenderPalette)
            .GetProperties()
            .Where(static property => property.PropertyType == typeof(Color))
            .ToArray();

        Assert.Equal(19, colorProperties.Length);
        Assert.All(
            new[] { ChartRenderPalette.Dark, ChartRenderPalette.Light },
            palette => Assert.All(
                colorProperties,
                property => Assert.Equal(
                    byte.MaxValue,
                    Assert.IsType<Color>(property.GetValue(palette)).A)));
    }

    [Fact]
    public void ChartPalettesMatchTheAstralArchiveV1Specification()
    {
        AssertPalette(
            ChartRenderPalette.Dark,
            "#0B1020", "#7F8AA8", "#3E4865",
            "#7A2F43", "#285E4B", "#245A73", "#403777",
            "#FFF2C6", "#F8E8FF", "#C9B8EA", "#8FA4C7",
            "#66718F", "#D8D3E8", "#F2C14E", "#48516D",
            "#D8D3E8", "#FF6275", "#4EB7E8", "#45D1A6");
        AssertPalette(
            ChartRenderPalette.Light,
            "#FAF9FE", "#5B5870", "#C8C3D5",
            "#E77886", "#5BB187", "#58A9D8", "#8574D8",
            "#2C213C", "#2A1D3D", "#615374", "#6A7088",
            "#8A8398", "#4A425D", "#8C5A00", "#C1BBCD",
            "#514A60", "#C43F5A", "#1E8DB4", "#20866A");
    }

    [Fact]
    public void PlanetGlyphAndDegreeColorsRemainReadableAgainstChartInterior()
    {
        foreach (var palette in new[] { ChartRenderPalette.Dark, ChartRenderPalette.Light })
        {
            Assert.True(Contrast(palette.PlanetGlyphColor, palette.InteriorBackgroundColor) >= 7d);
            Assert.True(Contrast(palette.PlanetDegreeColor, palette.InteriorBackgroundColor) >= 4.5d);
        }
    }

    [Fact]
    public void ElementAndAspectSemanticsAreDistinctAndThemeAware()
    {
        foreach (var palette in new[] { ChartRenderPalette.Dark, ChartRenderPalette.Light })
        {
            var elements = new[]
            {
                palette.FireSectorColor,
                palette.EarthSectorColor,
                palette.AirSectorColor,
                palette.WaterSectorColor
            };
            Assert.Equal(elements.Length, elements.Distinct().Count());

            foreach (var aspectType in Enum.GetValues<AspectType>())
            {
                var style = ChartAspectStyleCatalog.Get(aspectType, palette);
                Assert.Equal(byte.MaxValue, style.Color.A);
                Assert.True(style.Thickness > 0d);
            }
        }

        Assert.NotEqual(
            ChartAspectStyleCatalog.Get(AspectType.Square, ChartRenderPalette.Dark).Color,
            ChartAspectStyleCatalog.Get(AspectType.Square, ChartRenderPalette.Light).Color);
        Assert.NotEqual(
            ChartAspectStyleCatalog.Get(AspectType.Trine, ChartRenderPalette.Dark).Color,
            ChartAspectStyleCatalog.Get(AspectType.Trine, ChartRenderPalette.Light).Color);
    }

    private static void AssertPalette(
        ChartRenderPalette palette,
        string interior,
        string primaryStructure,
        string secondaryStructure,
        string fire,
        string earth,
        string air,
        string water,
        string zodiacGlyph,
        string planetGlyph,
        string planetDegree,
        string planetAnchor,
        string houseCusp,
        string houseLabel,
        string angleAxis,
        string aspectCircle,
        string conjunction,
        string hardAspect,
        string harmoniousBlue,
        string harmoniousTeal)
    {
        Assert.Equal(Color.Parse(interior), palette.InteriorBackgroundColor);
        Assert.Equal(Color.Parse(primaryStructure), palette.PrimaryStructureColor);
        Assert.Equal(Color.Parse(secondaryStructure), palette.SecondaryStructureColor);
        Assert.Equal(Color.Parse(fire), palette.FireSectorColor);
        Assert.Equal(Color.Parse(earth), palette.EarthSectorColor);
        Assert.Equal(Color.Parse(air), palette.AirSectorColor);
        Assert.Equal(Color.Parse(water), palette.WaterSectorColor);
        Assert.Equal(1d, palette.ZodiacSectorOpacity);
        Assert.Equal(Color.Parse(zodiacGlyph), palette.ZodiacGlyphColor);
        Assert.Equal(Color.Parse(planetGlyph), palette.PlanetGlyphColor);
        Assert.Equal(Color.Parse(planetDegree), palette.PlanetDegreeColor);
        Assert.Equal(Color.Parse(planetAnchor), palette.PlanetAnchorColor);
        Assert.Equal(Color.Parse(houseCusp), palette.HouseCuspColor);
        Assert.Equal(Color.Parse(houseLabel), palette.HouseLabelColor);
        Assert.Equal(Color.Parse(angleAxis), palette.AngleAxisColor);
        Assert.Equal(Color.Parse(aspectCircle), palette.AspectCircleColor);
        Assert.Equal(Color.Parse(conjunction), palette.ConjunctionAspectColor);
        Assert.Equal(Color.Parse(hardAspect), palette.HardAspectColor);
        Assert.Equal(Color.Parse(harmoniousBlue), palette.HarmoniousBlueAspectColor);
        Assert.Equal(Color.Parse(harmoniousTeal), palette.HarmoniousTealAspectColor);
    }

    private static double Contrast(Color first, Color second)
    {
        var lighter = Math.Max(Luminance(first), Luminance(second));
        var darker = Math.Min(Luminance(first), Luminance(second));
        return (lighter + 0.05d) / (darker + 0.05d);
    }

    private static double Luminance(Color color) =>
        (0.2126d * Linear(color.R)) +
        (0.7152d * Linear(color.G)) +
        (0.0722d * Linear(color.B));

    private static double Linear(byte channel)
    {
        var value = channel / 255d;
        return value <= 0.04045d
            ? value / 12.92d
            : Math.Pow((value + 0.055d) / 1.055d, 2.4d);
    }
}
