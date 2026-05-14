using NoxAeterna.Presentation.Localization;

namespace NoxAeterna.Presentation.Preferences;

/// <summary>
/// Represents the selected interpretation language.
/// </summary>
/// <param name="Language">The interpretation language.</param>
public sealed record InterpretationLanguagePreference(LanguageCode Language);
