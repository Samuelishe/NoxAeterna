using Avalonia.Styling;
using Avalonia.Media;
using NoxAeterna.App.Themes;
using NoxAeterna.Presentation.Theming;

namespace NoxAeterna.Tests.App;

public sealed class AppThemeControllerTests
{
    [Fact]
    public void ApplyTheme_UpdatesActiveThemeDeterministically()
    {
        var app = new global::NoxAeterna.App.App();
        app.Initialize();

        var controller = new AppThemeController(app, ThemeRegistry.CreateDefault());

        controller.ApplyTheme(new ThemeId("light"));

        Assert.Equal(new ThemeId("light"), controller.CurrentThemeId);
        Assert.IsType<LightThemeResources>(controller.CurrentThemeResources);
        Assert.Equal(ThemeVariant.Light, app.RequestedThemeVariant);

        controller.ApplyTheme(new ThemeId("dark"));

        Assert.Equal(new ThemeId("dark"), controller.CurrentThemeId);
        Assert.IsType<DarkThemeResources>(controller.CurrentThemeResources);
        Assert.Equal(ThemeVariant.Dark, app.RequestedThemeVariant);
    }

    [Fact]
    public void ApplyTheme_RejectsUnregisteredTheme()
    {
        var app = new global::NoxAeterna.App.App();
        app.Initialize();

        var controller = new AppThemeController(app, ThemeRegistry.CreateDefault());

        Assert.Throws<ArgumentException>(() => controller.ApplyTheme(new ThemeId("archive")));
    }

    [Fact]
    public void DarkAndLightResourcesExposeCanonicalSemanticColorRoles()
    {
        var dark = new DarkThemeResources();
        var light = new LightThemeResources();
        var keys = new[]
        {
            "DesignCanvasColor",
            "DesignSurfaceColor",
            "DesignSurfaceRaisedColor",
            "DesignBorderColor",
            "DesignTextPrimaryColor",
            "DesignTextSecondaryColor",
            "DesignTextMutedColor",
            "DesignAccentPrimaryColor",
            "DesignAccentPrimaryStrongColor",
            "DesignAccentSecondaryColor",
            "DesignSolarAccentColor",
            "DesignSuccessColor",
            "DesignWarningColor",
            "DesignErrorColor"
        };

        Assert.All(keys, key => Assert.IsType<Color>(dark[key]));
        Assert.All(keys, key => Assert.IsType<Color>(light[key]));
        Assert.Equal(Color.Parse("#090D18"), dark["DesignCanvasColor"]);
        Assert.Equal(Color.Parse("#F7F5FC"), light["DesignCanvasColor"]);
    }

    [Fact]
    public void CanonicalPrimaryAndSecondaryTextMeetContrastTargets()
    {
        var dark = new DarkThemeResources();
        var light = new LightThemeResources();

        Assert.True(Contrast(GetColor(dark, "DesignTextPrimaryColor"), GetColor(dark, "DesignCanvasColor")) >= 7d);
        Assert.True(Contrast(GetColor(dark, "DesignTextSecondaryColor"), GetColor(dark, "DesignCanvasColor")) >= 4.5d);
        Assert.True(Contrast(GetColor(light, "DesignTextPrimaryColor"), GetColor(light, "DesignCanvasColor")) >= 7d);
        Assert.True(Contrast(GetColor(light, "DesignTextSecondaryColor"), GetColor(light, "DesignCanvasColor")) >= 4.5d);
    }

    private static Color GetColor(Avalonia.Controls.ResourceDictionary resources, string key) =>
        Assert.IsType<Color>(resources[key]);

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
