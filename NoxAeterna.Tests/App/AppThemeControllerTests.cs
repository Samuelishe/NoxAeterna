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
    public void DarkAndLightResourcesExposeCanonicalSemanticColorAndBrushRoles()
    {
        var dark = new DarkThemeResources();
        var light = new LightThemeResources();
        var colorKeys = new[]
        {
            "DesignCanvasColor",
            "DesignSurfaceColor",
            "DesignSurfaceRaisedColor",
            "DesignSurfaceSunkenColor",
            "DesignBorderColor",
            "DesignBorderStrongColor",
            "DesignTextPrimaryColor",
            "DesignTextSecondaryColor",
            "DesignTextMutedColor",
            "DesignAccentPrimaryColor",
            "DesignAccentPrimaryStrongColor",
            "DesignAccentPrimarySoftColor",
            "DesignAccentSecondaryColor",
            "DesignAccentSecondarySoftColor",
            "DesignSolarAccentColor",
            "DesignSuccessColor",
            "DesignWarningColor",
            "DesignErrorColor",
            "DesignControlFillColor",
            "DesignControlFillHoverColor",
            "DesignControlFillPressedColor",
            "DesignDisabledFillColor",
            "DesignDisabledTextColor",
            "DesignSelectionForegroundColor",
            "DesignFocusRingColor"
        };

        Assert.All(colorKeys, key => Assert.IsType<Color>(dark[key]));
        Assert.All(colorKeys, key => Assert.IsType<Color>(light[key]));
        Assert.All(
            colorKeys,
            colorKey =>
            {
                var brushKey = colorKey[..^"Color".Length] + "Brush";
                var darkBrush = Assert.IsType<SolidColorBrush>(dark[brushKey]);
                var lightBrush = Assert.IsType<SolidColorBrush>(light[brushKey]);
                Assert.Equal(GetColor(dark, colorKey), darkBrush.Color);
                Assert.Equal(GetColor(light, colorKey), lightBrush.Color);
            });
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

    [Fact]
    public void RepeatedThemeSwitchingMaintainsOneThemeDictionaryAndResolvableRoles()
    {
        var app = new global::NoxAeterna.App.App();
        app.Initialize();
        var controller = new AppThemeController(app, ThemeRegistry.CreateDefault());
        var originalDictionaryCount = app.Resources.MergedDictionaries.Count;

        controller.ApplyTheme(new ThemeId("dark"));
        Assert.Equal(originalDictionaryCount + 1, app.Resources.MergedDictionaries.Count);

        controller.ApplyTheme(new ThemeId("light"));
        controller.ApplyTheme(new ThemeId("dark"));
        controller.ApplyTheme(new ThemeId("dark"));

        Assert.Equal(originalDictionaryCount + 1, app.Resources.MergedDictionaries.Count);
        Assert.IsType<DarkThemeResources>(controller.CurrentThemeResources);
        Assert.Equal(ThemeVariant.Dark, app.RequestedThemeVariant);
        Assert.True(app.TryGetResource(
            "DesignControlFillBrush",
            ThemeVariant.Dark,
            out var controlFill));
        Assert.IsType<SolidColorBrush>(controlFill);
        Assert.True(app.TryGetResource(
            "DesignFocusRingBrush",
            ThemeVariant.Dark,
            out var focusRing));
        Assert.IsType<SolidColorBrush>(focusRing);
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
