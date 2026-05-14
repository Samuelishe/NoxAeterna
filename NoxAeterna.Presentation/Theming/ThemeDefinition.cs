using NoxAeterna.Presentation.Localization;

namespace NoxAeterna.Presentation.Theming;

/// <summary>
/// Represents one named theme available to the application.
/// </summary>
public sealed record ThemeDefinition(ThemeId Id, LocalizationKey DisplayNameKey);
