namespace NoxAeterna.Presentation.Theming;

using NoxAeterna.Presentation.Localization;

/// <summary>
/// Represents a deterministic registry of available application themes.
/// </summary>
public sealed class ThemeRegistry
{
    private readonly IReadOnlyDictionary<ThemeId, ThemeDefinition> _themes;

    /// <summary>
    /// Initializes a new instance of the <see cref="ThemeRegistry"/> class.
    /// </summary>
    /// <param name="themes">The available themes.</param>
    /// <exception cref="ArgumentException">Thrown when duplicate theme identifiers are provided.</exception>
    public ThemeRegistry(IEnumerable<ThemeDefinition> themes)
    {
        var copiedThemes = (themes ?? throw new ArgumentNullException(nameof(themes))).ToArray();
        var duplicateTheme = copiedThemes
            .GroupBy(static theme => theme.Id)
            .FirstOrDefault(static group => group.Count() > 1);

        if (duplicateTheme is not null)
        {
            throw new ArgumentException($"Duplicate theme identifier '{duplicateTheme.Key}' detected.", nameof(themes));
        }

        _themes = copiedThemes.ToDictionary(static theme => theme.Id);
        Themes = Array.AsReadOnly(copiedThemes.ToArray());
    }

    /// <summary>
    /// Gets the registered themes.
    /// </summary>
    public IReadOnlyList<ThemeDefinition> Themes { get; }

    /// <summary>
    /// Attempts to resolve a theme definition by identifier.
    /// </summary>
    /// <param name="themeId">The theme identifier.</param>
    /// <param name="theme">The resolved theme definition when present.</param>
    /// <returns><see langword="true"/> when the theme exists; otherwise <see langword="false"/>.</returns>
    public bool TryGet(ThemeId themeId, out ThemeDefinition? theme) => _themes.TryGetValue(themeId, out theme);

    /// <summary>
    /// Creates the current default application theme registry.
    /// </summary>
    /// <returns>A deterministic theme registry containing the initial themes.</returns>
    public static ThemeRegistry CreateDefault() =>
        new(
        [
            new ThemeDefinition(new ThemeId("dark"), new LocalizationKey("theme.dark")),
            new ThemeDefinition(new ThemeId("light"), new LocalizationKey("theme.light"))
        ]);
}
