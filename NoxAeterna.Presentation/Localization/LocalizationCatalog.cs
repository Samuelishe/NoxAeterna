namespace NoxAeterna.Presentation.Localization;

/// <summary>
/// Represents a localization catalog for one language and scope.
/// </summary>
public sealed record LocalizationCatalog
{
    private readonly IReadOnlyDictionary<LocalizationKey, string> _entries;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalizationCatalog"/> class.
    /// </summary>
    /// <param name="scope">The localization scope.</param>
    /// <param name="language">The catalog language.</param>
    /// <param name="entries">The localized entries.</param>
    /// <exception cref="ArgumentException">Thrown when duplicate keys are provided.</exception>
    public LocalizationCatalog(
        LocalizationScope scope,
        LanguageCode language,
        IEnumerable<LocalizationEntry> entries)
    {
        var copiedEntries = (entries ?? throw new ArgumentNullException(nameof(entries))).ToArray();
        var duplicateEntry = copiedEntries
            .GroupBy(static entry => entry.Key)
            .FirstOrDefault(static group => group.Count() > 1);

        if (duplicateEntry is not null)
        {
            throw new ArgumentException(
                $"Duplicate localization key '{duplicateEntry.Key}' detected in one catalog.",
                nameof(entries));
        }

        Scope = scope;
        Language = language;
        _entries = copiedEntries.ToDictionary(static entry => entry.Key, static entry => entry.Text);
        Entries = Array.AsReadOnly(copiedEntries.ToArray());
    }

    /// <summary>
    /// Gets the localization scope.
    /// </summary>
    public LocalizationScope Scope { get; }

    /// <summary>
    /// Gets the catalog language.
    /// </summary>
    public LanguageCode Language { get; }

    /// <summary>
    /// Gets the catalog entries.
    /// </summary>
    public IReadOnlyList<LocalizationEntry> Entries { get; }

    /// <summary>
    /// Attempts to resolve localized text for the supplied key.
    /// </summary>
    /// <param name="key">The localization key.</param>
    /// <param name="text">The resolved text when present.</param>
    /// <returns><see langword="true"/> when the key exists; otherwise <see langword="false"/>.</returns>
    public bool TryGetText(LocalizationKey key, out string? text) => _entries.TryGetValue(key, out text);
}
