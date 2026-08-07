using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using NoxAeterna.App.Preferences;
using NoxAeterna.App.Themes;
using NoxAeterna.App.Tarot;
using NoxAeterna.Presentation.Theming;

namespace NoxAeterna.App;

public partial class App : Application
{
    private AppThemeController? _themeController;

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        _themeController = new AppThemeController(this, ThemeRegistry.CreateDefault());
        var interpretation = TarotInterpretationComposition.CreateBuiltIn();
        var preferencesStore = new JsonUserPreferencesStore(
            ResolveSettingsPath(),
            interpretation.PackCatalog.AvailablePackIds);
        var preferencesCoordinator = new UserPreferencesCoordinator(preferencesStore, preferencesStore.Load());
        _themeController.ApplyTheme(preferencesCoordinator.Current.ThemeId);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow(preferencesCoordinator, interpretation);
        }

        base.OnFrameworkInitializationCompleted();
    }

    public void ApplyTheme(ThemeId themeId)
    {
        _themeController ??= new AppThemeController(this, ThemeRegistry.CreateDefault());
        _themeController.ApplyTheme(themeId);
    }

    private static string ResolveSettingsPath()
    {
#if DEBUG
        var debugRoot = Environment.GetEnvironmentVariable("NOXAETERNA_DEBUG_APPDATA_ROOT");
        if (!string.IsNullOrWhiteSpace(debugRoot))
        {
            return UserPreferencesPathResolver.GetSettingsPath(debugRoot);
        }
#endif
        return UserPreferencesPathResolver.GetSettingsPath();
    }
}
