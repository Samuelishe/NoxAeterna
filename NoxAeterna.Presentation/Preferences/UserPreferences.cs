using NoxAeterna.Presentation.Theming;

namespace NoxAeterna.Presentation.Preferences;

/// <summary>
/// Represents minimal user preferences for language and theme selection.
/// </summary>
public sealed record UserPreferences(
    ApplicationLanguagePreference ApplicationLanguage,
    InterpretationLanguagePreference InterpretationLanguage,
    ThemeId ThemeId);
