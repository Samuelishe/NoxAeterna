using NoxAeterna.Presentation.Localization;
using NoxAeterna.Presentation.Preferences;
using NoxAeterna.Presentation.Theming;

namespace NoxAeterna.Presentation.Settings;

/// <summary>
/// Represents the minimal in-memory settings state for language and theme selection.
/// </summary>
public sealed class SettingsViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsViewModel"/> class.
    /// </summary>
    /// <param name="userPreferences">The current in-memory user preferences.</param>
    /// <param name="availableLanguages">The available language options.</param>
    /// <param name="availableThemes">The available theme options.</param>
    public SettingsViewModel(
        UserPreferences userPreferences,
        IEnumerable<LanguageOption> availableLanguages,
        IEnumerable<ThemeOption> availableThemes)
    {
        var languages = (availableLanguages ?? throw new ArgumentNullException(nameof(availableLanguages))).ToArray();
        var themes = (availableThemes ?? throw new ArgumentNullException(nameof(availableThemes))).ToArray();

        if (languages.Length == 0)
        {
            throw new ArgumentException("Settings must expose at least one language option.", nameof(availableLanguages));
        }

        if (themes.Length == 0)
        {
            throw new ArgumentException("Settings must expose at least one theme option.", nameof(availableThemes));
        }

        CurrentPreferences = userPreferences;
        AvailableApplicationLanguages = Array.AsReadOnly(languages);
        AvailableInterpretationLanguages = Array.AsReadOnly(languages.ToArray());
        AvailableThemes = Array.AsReadOnly(themes);
    }

    /// <summary>
    /// Gets the localization key for the settings section title.
    /// </summary>
    public LocalizationKey TitleKey { get; } = new("ui.settings.title");

    /// <summary>
    /// Gets the localization key for the application language setting label.
    /// </summary>
    public LocalizationKey ApplicationLanguageLabelKey { get; } = new("ui.settings.application_language");

    /// <summary>
    /// Gets the localization key for the interpretation language setting label.
    /// </summary>
    public LocalizationKey InterpretationLanguageLabelKey { get; } = new("ui.settings.interpretation_language");

    /// <summary>
    /// Gets the localization key for the theme setting label.
    /// </summary>
    public LocalizationKey ThemeLabelKey { get; } = new("ui.settings.theme");

    /// <summary>
    /// Gets the current in-memory user preferences.
    /// </summary>
    public UserPreferences CurrentPreferences { get; private set; }

    /// <summary>
    /// Gets the available application language options.
    /// </summary>
    public IReadOnlyList<LanguageOption> AvailableApplicationLanguages { get; }

    /// <summary>
    /// Gets the available interpretation language options.
    /// </summary>
    public IReadOnlyList<LanguageOption> AvailableInterpretationLanguages { get; }

    /// <summary>
    /// Gets the available theme options.
    /// </summary>
    public IReadOnlyList<ThemeOption> AvailableThemes { get; }

    /// <summary>
    /// Updates the selected application language in memory.
    /// </summary>
    /// <param name="languageCode">The selected application language.</param>
    public void SetApplicationLanguage(LanguageCode languageCode) =>
        CurrentPreferences = CurrentPreferences with
        {
            ApplicationLanguage = new ApplicationLanguagePreference(languageCode)
        };

    /// <summary>
    /// Updates the selected interpretation language in memory.
    /// </summary>
    /// <param name="languageCode">The selected interpretation language.</param>
    public void SetInterpretationLanguage(LanguageCode languageCode) =>
        CurrentPreferences = CurrentPreferences with
        {
            InterpretationLanguage = new InterpretationLanguagePreference(languageCode)
        };

    /// <summary>
    /// Updates the selected theme in memory.
    /// </summary>
    /// <param name="themeId">The selected theme identifier.</param>
    public void SetTheme(ThemeId themeId) =>
        CurrentPreferences = CurrentPreferences with
        {
            ThemeId = themeId
        };

    /// <summary>
    /// Creates the current default in-memory settings state.
    /// </summary>
    /// <param name="userPreferences">The current preferences.</param>
    /// <returns>A new settings view model.</returns>
    public static SettingsViewModel CreateDefault(UserPreferences userPreferences) =>
        new(
            userPreferences,
            new[]
            {
                new LanguageOption(new LanguageCode("ru"), new LocalizationKey("ui.language.ru")),
                new LanguageOption(new LanguageCode("en"), new LocalizationKey("ui.language.en"))
            },
            new[]
            {
                new ThemeOption(new ThemeId("dark"), new LocalizationKey("theme.dark")),
                new ThemeOption(new ThemeId("light"), new LocalizationKey("theme.light"))
            });
}
