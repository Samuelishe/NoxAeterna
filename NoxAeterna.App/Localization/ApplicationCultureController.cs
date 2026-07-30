using System.Globalization;
using Avalonia;
using Avalonia.Controls;
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

        if (Application.Current is { } application)
        {
            ApplyPickerSegmentResources(application.Resources, ResolvePickerSegmentLabels(language));
        }
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

    /// <summary>
    /// Resolves the Fluent picker segment labels not supplied by .NET culture.
    /// </summary>
    public static DateTimePickerSegmentLabels ResolvePickerSegmentLabels(LanguageCode language) =>
        language.Value == "ru"
            ? new("день", "месяц", "год", "час", "минута")
            : new("day", "month", "year", "hour", "minute");

    private static void ApplyPickerSegmentResources(
        IResourceDictionary resources,
        DateTimePickerSegmentLabels labels)
    {
        resources["StringDatePickerDayText"] = labels.Day;
        resources["StringDatePickerMonthText"] = labels.Month;
        resources["StringDatePickerYearText"] = labels.Year;
        resources["StringTimePickerHourText"] = labels.Hour;
        resources["StringTimePickerMinuteText"] = labels.Minute;
    }

    private static string GetSpecificCultureCode(LanguageCode language) =>
        language.Value switch
        {
            "ru" => "ru-RU",
            "en" => "en-US",
            _ => language.Value
        };
}

/// <summary>
/// Provides localized empty-state labels for Avalonia Fluent date/time picker segments.
/// </summary>
public sealed record DateTimePickerSegmentLabels(
    string Day,
    string Month,
    string Year,
    string Hour,
    string Minute);
