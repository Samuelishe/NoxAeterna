using NoxAeterna.Presentation.Localization;

namespace NoxAeterna.Presentation.Settings;

/// <summary>
/// Represents one selectable language option in settings.
/// </summary>
public sealed record LanguageOption(LanguageCode Code, LocalizationKey LabelKey);
