namespace NoxAeterna.Presentation.Localization;

/// <summary>
/// Represents one localized text entry.
/// </summary>
/// <param name="Key">The stable localization key.</param>
/// <param name="Text">The localized text.</param>
public sealed record LocalizationEntry(LocalizationKey Key, string Text);
