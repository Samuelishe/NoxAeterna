using NoxAeterna.Presentation.Localization;
using NoxAeterna.Presentation.Preferences;
using NoxAeterna.Presentation.Theming;

namespace NoxAeterna.App.Preferences;

/// <summary>Owns the explicit first-run preference defaults.</summary>
public static class UserPreferencesDefaults
{
    /// <summary>Creates the normalized current-schema defaults.</summary>
    public static UserPreferences Create() => new(
        new ApplicationLanguagePreference(new LanguageCode("ru")),
        new InterpretationLanguagePreference(new LanguageCode("ru")),
        new ThemeId("dark"),
        TarotWorkspacePreferences.CreateDefault());
}
