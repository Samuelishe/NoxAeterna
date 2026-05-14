using NoxAeterna.Presentation.Localization;

namespace NoxAeterna.Presentation.Preferences;

/// <summary>
/// Represents the selected application interface language.
/// </summary>
/// <param name="Language">The application language.</param>
public sealed record ApplicationLanguagePreference(LanguageCode Language);
