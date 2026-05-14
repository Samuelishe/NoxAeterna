using Avalonia.Styling;
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
}
