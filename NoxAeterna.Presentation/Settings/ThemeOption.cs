using NoxAeterna.Presentation.Localization;
using NoxAeterna.Presentation.Theming;

namespace NoxAeterna.Presentation.Settings;

/// <summary>
/// Represents one selectable theme option in settings.
/// </summary>
public sealed record ThemeOption(ThemeId ThemeId, LocalizationKey LabelKey);
