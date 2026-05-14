namespace NoxAeterna.Presentation.Localization;

/// <summary>
/// Resolves localized text for a scope, language, and key.
/// </summary>
public interface ILocalizationProvider
{
    /// <summary>
    /// Resolves a localized text value.
    /// </summary>
    /// <param name="scope">The localization scope.</param>
    /// <param name="language">The selected language.</param>
    /// <param name="key">The localization key.</param>
    /// <returns>A deterministic localization result.</returns>
    LocalizationResult Get(LocalizationScope scope, LanguageCode language, LocalizationKey key);
}
