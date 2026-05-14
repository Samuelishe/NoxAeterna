using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;
using NoxAeterna.App.Themes;
using NoxAeterna.Presentation.Theming;

namespace NoxAeterna.App.Themes;

/// <summary>
/// Applies the current application theme to Avalonia resources and theme variant state.
/// </summary>
public sealed class AppThemeController
{
    private readonly Application _application;
    private readonly ThemeRegistry _themeRegistry;

    /// <summary>
    /// Initializes a new instance of the <see cref="AppThemeController"/> class.
    /// </summary>
    /// <param name="application">The target application instance.</param>
    /// <param name="themeRegistry">The available theme registry.</param>
    public AppThemeController(Application application, ThemeRegistry themeRegistry)
    {
        _application = application ?? throw new ArgumentNullException(nameof(application));
        _themeRegistry = themeRegistry ?? throw new ArgumentNullException(nameof(themeRegistry));
    }

    /// <summary>
    /// Gets the currently applied theme identifier.
    /// </summary>
    public ThemeId? CurrentThemeId { get; private set; }

    /// <summary>
    /// Gets the currently applied theme resources.
    /// </summary>
    public ResourceDictionary? CurrentThemeResources { get; private set; }

    /// <summary>
    /// Applies the requested theme to the application.
    /// </summary>
    /// <param name="themeId">The theme identifier.</param>
    public void ApplyTheme(ThemeId themeId)
    {
        if (!_themeRegistry.TryGet(themeId, out _))
        {
            throw new ArgumentException($"Theme '{themeId}' is not registered.", nameof(themeId));
        }

        if (CurrentThemeId == themeId && CurrentThemeResources is not null)
        {
            return;
        }

        if (CurrentThemeResources is not null)
        {
            _application.Resources.MergedDictionaries.Remove(CurrentThemeResources);
        }

        CurrentThemeResources = CreateResources(themeId);
        _application.Resources.MergedDictionaries.Add(CurrentThemeResources);
        _application.RequestedThemeVariant = GetThemeVariant(themeId);
        CurrentThemeId = themeId;
    }

    private static ResourceDictionary CreateResources(ThemeId themeId) =>
        themeId.Value switch
        {
            "dark" => new DarkThemeResources(),
            "light" => new LightThemeResources(),
            _ => throw new NotSupportedException($"Theme '{themeId}' does not have an App resource dictionary.")
        };

    private static ThemeVariant GetThemeVariant(ThemeId themeId) =>
        themeId.Value switch
        {
            "dark" => ThemeVariant.Dark,
            "light" => ThemeVariant.Light,
            _ => ThemeVariant.Default
        };
}
