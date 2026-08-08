using NoxAeterna.Presentation.Theming;

namespace NoxAeterna.Presentation.Preferences;

/// <summary>
/// Represents the complete immutable application preference state.
/// </summary>
public sealed record UserPreferences(
    ApplicationLanguagePreference ApplicationLanguage,
    InterpretationLanguagePreference InterpretationLanguage,
    ThemeId ThemeId,
    TarotWorkspacePreferences Tarot,
    WindowPlacementPreference? WindowPlacement = null);
