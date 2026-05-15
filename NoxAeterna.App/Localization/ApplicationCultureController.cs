using System.Globalization;
using NoxAeterna.Presentation.Localization;

namespace NoxAeterna.App.Localization;

/// <summary>
/// Applies the current application language to .NET culture-sensitive UI controls.
/// </summary>
public static class ApplicationCultureController
{
    /// <summary>
    /// Applies the most appropriate culture for the selected application language.
    /// </summary>
    /// <param name="language">The selected application language.</param>
    public static void Apply(LanguageCode language)
    {
        var culture = ResolveCulture(language);

        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
    }

    /// <summary>
    /// Resolves a concrete culture for the supplied application language.
    /// </summary>
    /// <param name="language">The selected application language.</param>
    /// <returns>A concrete culture suitable for UI controls.</returns>
    public static CultureInfo ResolveCulture(LanguageCode language)
    {
        var candidateValues = new[]
        {
            GetSpecificCultureCode(language),
            language.Value,
            language.NeutralParent?.Value
        };

        foreach (var candidateValue in candidateValues)
        {
            if (string.IsNullOrWhiteSpace(candidateValue))
            {
                continue;
            }

            try
            {
                return CultureInfo.GetCultureInfo(candidateValue);
            }
            catch (CultureNotFoundException)
            {
            }
        }

        return CultureInfo.InvariantCulture;
    }

    private static string GetSpecificCultureCode(LanguageCode language) =>
        language.Value switch
        {
            "ru" => "ru-RU",
            "en" => "en-US",
            _ => language.Value
        };
}
