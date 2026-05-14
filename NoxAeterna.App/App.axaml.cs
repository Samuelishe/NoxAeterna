using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using NoxAeterna.App.Themes;
using NoxAeterna.Presentation.Theming;

namespace NoxAeterna.App;

public partial class App : Application
{
    private AppThemeController? _themeController;

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        _themeController = new AppThemeController(this, ThemeRegistry.CreateDefault());
        _themeController.ApplyTheme(new ThemeId("dark"));

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }

    public void ApplyTheme(ThemeId themeId)
    {
        _themeController ??= new AppThemeController(this, ThemeRegistry.CreateDefault());
        _themeController.ApplyTheme(themeId);
    }
}
