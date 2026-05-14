namespace NoxAeterna.Presentation.Localization;

/// <summary>
/// Represents the outcome of a localization lookup.
/// </summary>
public sealed record LocalizationResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LocalizationResult"/> class.
    /// </summary>
    /// <param name="key">The localization key.</param>
    /// <param name="text">The resolved text or key fallback.</param>
    /// <param name="requestedLanguage">The originally requested language.</param>
    /// <param name="resolvedLanguage">The language that actually provided the text, when available.</param>
    public LocalizationResult(
        LocalizationKey key,
        string text,
        LanguageCode requestedLanguage,
        LanguageCode? resolvedLanguage)
    {
        Key = key;
        Text = string.IsNullOrWhiteSpace(text) ? key.Value : text;
        RequestedLanguage = requestedLanguage;
        ResolvedLanguage = resolvedLanguage;
    }

    /// <summary>
    /// Gets the localization key.
    /// </summary>
    public LocalizationKey Key { get; }

    /// <summary>
    /// Gets the resolved text or deterministic key fallback.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// Gets the originally requested language.
    /// </summary>
    public LanguageCode RequestedLanguage { get; }

    /// <summary>
    /// Gets the language that actually provided the text, when available.
    /// </summary>
    public LanguageCode? ResolvedLanguage { get; }

    /// <summary>
    /// Gets a value indicating whether the resolved text came from a fallback rather than the originally requested language.
    /// </summary>
    public bool UsedFallback => ResolvedLanguage is null || ResolvedLanguage != RequestedLanguage;
}
