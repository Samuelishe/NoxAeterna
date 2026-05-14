using NoxAeterna.Presentation.Localization;

namespace NoxAeterna.App.Debug;

/// <summary>
/// Builds the temporary UI localization provider from JSON resources for the thin shell host.
/// This remains bootstrap infrastructure until localization loading moves out of App.
/// </summary>
public static class DebugShellLocalizationProviderFactory
{
    /// <summary>
    /// Creates the UI localization provider for the selected application language.
    /// </summary>
    /// <param name="applicationLanguage">The selected application language.</param>
    /// <returns>A fallback localization provider backed by JSON catalogs.</returns>
    public static ILocalizationProvider Create(LanguageCode applicationLanguage)
    {
        var resourceRoot = Path.Combine(AppContext.BaseDirectory, "resources", "localization", "ui");
        var catalogs = new List<LocalizationCatalog>();

        foreach (var candidateLanguage in GetCandidateLanguages(applicationLanguage))
        {
            var filePath = Path.Combine(resourceRoot, $"{candidateLanguage.Value}.json");

            if (!File.Exists(filePath))
            {
                continue;
            }

            catalogs.Add(JsonLocalizationCatalogLoader.LoadFromFile(
                LocalizationScope.Ui,
                candidateLanguage,
                filePath));
        }

        return new FallbackLocalizationProvider(catalogs);
    }

    private static IEnumerable<LanguageCode> GetCandidateLanguages(LanguageCode selectedLanguage)
    {
        yield return selectedLanguage;

        if (selectedLanguage.NeutralParent is { } neutralParent && neutralParent != selectedLanguage)
        {
            yield return neutralParent;
        }

        if (selectedLanguage != new LanguageCode("ru"))
        {
            yield return new LanguageCode("ru");
        }
    }
}
