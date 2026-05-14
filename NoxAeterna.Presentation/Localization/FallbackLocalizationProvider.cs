namespace NoxAeterna.Presentation.Localization;

/// <summary>
/// Resolves localized text through a deterministic fallback chain.
/// </summary>
public sealed class FallbackLocalizationProvider : ILocalizationProvider
{
    private static readonly LanguageCode RussianLanguage = new("ru");

    private readonly IReadOnlyDictionary<(LocalizationScope Scope, LanguageCode Language), LocalizationCatalog> _catalogs;

    /// <summary>
    /// Initializes a new instance of the <see cref="FallbackLocalizationProvider"/> class.
    /// </summary>
    /// <param name="catalogs">The available localization catalogs.</param>
    /// <exception cref="ArgumentException">Thrown when duplicate scope/language catalogs are provided.</exception>
    public FallbackLocalizationProvider(IEnumerable<LocalizationCatalog> catalogs)
    {
        var copiedCatalogs = (catalogs ?? throw new ArgumentNullException(nameof(catalogs))).ToArray();
        var duplicateCatalog = copiedCatalogs
            .GroupBy(static catalog => (catalog.Scope, catalog.Language))
            .FirstOrDefault(static group => group.Count() > 1);

        if (duplicateCatalog is not null)
        {
            throw new ArgumentException(
                $"Duplicate localization catalog detected for scope '{duplicateCatalog.Key.Scope}' and language '{duplicateCatalog.Key.Language}'.",
                nameof(catalogs));
        }

        _catalogs = copiedCatalogs.ToDictionary(static catalog => (catalog.Scope, catalog.Language));
    }

    /// <inheritdoc />
    public LocalizationResult Get(LocalizationScope scope, LanguageCode language, LocalizationKey key)
    {
        foreach (var candidateLanguage in GetCandidateLanguages(language))
        {
            if (_catalogs.TryGetValue((scope, candidateLanguage), out var catalog) &&
                catalog.TryGetText(key, out var text) &&
                !string.IsNullOrWhiteSpace(text))
            {
                return new LocalizationResult(key, text, language, candidateLanguage);
            }
        }

        return new LocalizationResult(key, key.Value, language, resolvedLanguage: null);
    }

    private static IEnumerable<LanguageCode> GetCandidateLanguages(LanguageCode selectedLanguage)
    {
        yield return selectedLanguage;

        if (selectedLanguage.NeutralParent is { } neutralParent && neutralParent != selectedLanguage)
        {
            yield return neutralParent;
        }

        if (selectedLanguage != RussianLanguage)
        {
            yield return RussianLanguage;
        }
    }
}
