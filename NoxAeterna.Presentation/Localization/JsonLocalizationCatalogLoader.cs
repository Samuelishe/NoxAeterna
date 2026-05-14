using System.Text.Json;

namespace NoxAeterna.Presentation.Localization;

/// <summary>
/// Loads flat key-value localization catalogs from JSON files.
/// </summary>
public static class JsonLocalizationCatalogLoader
{
    /// <summary>
    /// Loads a localization catalog from the supplied JSON file path.
    /// </summary>
    /// <param name="scope">The localization scope.</param>
    /// <param name="language">The catalog language.</param>
    /// <param name="filePath">The JSON file path.</param>
    /// <returns>A loaded localization catalog.</returns>
    public static LocalizationCatalog LoadFromFile(
        LocalizationScope scope,
        LanguageCode language,
        string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path must not be empty.", nameof(filePath));
        }

        return LoadFromJson(scope, language, File.ReadAllText(filePath));
    }

    /// <summary>
    /// Loads a localization catalog from flat key-value JSON content.
    /// </summary>
    /// <param name="scope">The localization scope.</param>
    /// <param name="language">The catalog language.</param>
    /// <param name="json">The JSON content.</param>
    /// <returns>A loaded localization catalog.</returns>
    public static LocalizationCatalog LoadFromJson(
        LocalizationScope scope,
        LanguageCode language,
        string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new ArgumentException("JSON content must not be empty.", nameof(json));
        }

        using var document = JsonDocument.Parse(json);

        if (document.RootElement.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidOperationException("Localization JSON root must be an object.");
        }

        var seenKeys = new HashSet<string>(StringComparer.Ordinal);
        var entries = new List<LocalizationEntry>();

        foreach (var property in document.RootElement.EnumerateObject())
        {
            if (!seenKeys.Add(property.Name))
            {
                throw new InvalidOperationException($"Duplicate localization key '{property.Name}' detected in JSON content.");
            }

            if (property.Value.ValueKind != JsonValueKind.String)
            {
                throw new InvalidOperationException($"Localization value for key '{property.Name}' must be a string.");
            }

            entries.Add(new LocalizationEntry(
                new LocalizationKey(property.Name),
                property.Value.GetString() ?? string.Empty));
        }

        return new LocalizationCatalog(scope, language, entries);
    }
}
