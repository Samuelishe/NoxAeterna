using System.Text.RegularExpressions;
using System.Xml.Linq;
using Avalonia.Controls;
using Avalonia.Media;
using NoxAeterna.App.Themes;

namespace NoxAeterna.Tests.App;

public sealed class SemanticThemeContractTests
{
    private static readonly string[] ComponentColorKeys =
    [
        "DesignSurfaceSunkenColor",
        "DesignBorderStrongColor",
        "DesignAccentPrimarySoftColor",
        "DesignAccentSecondarySoftColor",
        "DesignControlFillColor",
        "DesignControlFillHoverColor",
        "DesignControlFillPressedColor",
        "DesignDisabledFillColor",
        "DesignDisabledTextColor",
        "DesignSelectionForegroundColor",
        "DesignFocusRingColor"
    ];

    [Fact]
    public void ThemeDictionariesHaveExactSemanticKeyParity()
    {
        var darkKeys = GetStringKeys(new DarkThemeResources());
        var lightKeys = GetStringKeys(new LightThemeResources());

        Assert.Equal(darkKeys, lightKeys);
        Assert.All(ComponentColorKeys, key => Assert.Contains(key, darkKeys));
    }

    [Fact]
    public void ComponentStateValuesMatchAstralArchiveSpecification()
    {
        var dark = new DarkThemeResources();
        var light = new LightThemeResources();
        var expectedDark = new Dictionary<string, string>
        {
            ["DesignSurfaceSunkenColor"] = "#0D1424",
            ["DesignBorderStrongColor"] = "#3A476A",
            ["DesignAccentPrimarySoftColor"] = "#2E2759",
            ["DesignAccentSecondarySoftColor"] = "#123A46",
            ["DesignControlFillColor"] = "#0D1424",
            ["DesignControlFillHoverColor"] = "#151F35",
            ["DesignControlFillPressedColor"] = "#1D2944",
            ["DesignDisabledFillColor"] = "#141A29",
            ["DesignDisabledTextColor"] = "#68718E",
            ["DesignSelectionForegroundColor"] = "#FFFFFF",
            ["DesignFocusRingColor"] = "#45C7D9"
        };
        var expectedLight = new Dictionary<string, string>
        {
            ["DesignSurfaceSunkenColor"] = "#F2EFF8",
            ["DesignBorderStrongColor"] = "#B7AEC9",
            ["DesignAccentPrimarySoftColor"] = "#E9E3FF",
            ["DesignAccentSecondarySoftColor"] = "#DDF3F6",
            ["DesignControlFillColor"] = "#FFFFFF",
            ["DesignControlFillHoverColor"] = "#F1EDF9",
            ["DesignControlFillPressedColor"] = "#E5DFF1",
            ["DesignDisabledFillColor"] = "#EEEAF3",
            ["DesignDisabledTextColor"] = "#9A93A8",
            ["DesignSelectionForegroundColor"] = "#FFFFFF",
            ["DesignFocusRingColor"] = "#147F91"
        };

        AssertExpectedColors(dark, expectedDark);
        AssertExpectedColors(light, expectedLight);
    }

    [Fact]
    public void ControlAndSelectionStatesRemainReadableAndDistinct()
    {
        var dark = new DarkThemeResources();
        var light = new LightThemeResources();

        Assert.True(Contrast(GetColor(dark, "DesignTextPrimaryColor"), GetColor(dark, "DesignControlFillColor")) >= 7d);
        Assert.True(Contrast(GetColor(light, "DesignTextPrimaryColor"), GetColor(light, "DesignControlFillColor")) >= 7d);
        Assert.True(Contrast(GetColor(dark, "DesignSelectionForegroundColor"), GetColor(dark, "DesignAccentPrimaryStrongColor")) >= 4d);
        Assert.True(Contrast(GetColor(light, "DesignSelectionForegroundColor"), GetColor(light, "DesignAccentPrimaryStrongColor")) >= 4.5d);
        Assert.NotEqual(GetColor(dark, "DesignDisabledFillColor"), GetColor(dark, "DesignControlFillColor"));
        Assert.NotEqual(GetColor(light, "DesignDisabledFillColor"), GetColor(light, "DesignControlFillColor"));
        Assert.NotEqual(GetColor(dark, "DesignDisabledTextColor"), GetColor(dark, "DesignTextPrimaryColor"));
        Assert.NotEqual(GetColor(light, "DesignDisabledTextColor"), GetColor(light, "DesignTextPrimaryColor"));
    }

    [Fact]
    public void LegacyShellPaletteLiteralsAreAbsentFromThemeDictionaries()
    {
        var source = File.ReadAllText(AppPath("Themes", "DarkThemeResources.axaml")) +
                     File.ReadAllText(AppPath("Themes", "LightThemeResources.axaml"));
        var forbidden = new[]
        {
            "#0E0E11", "#17171A", "#121214", "#34343A", "#D08B8B",
            "#F2EFEA", "#E9E2D8", "#FAF7F2", "#B8AA95", "#A24F4A",
            "#F3EEE6", "#C0B19C", "#F5F1EA"
        };

        Assert.All(forbidden, value => Assert.DoesNotContain(value, source, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void SemanticControlStylesOwnRequiredControlFamilies()
    {
        var source = File.ReadAllText(AppPath("Themes", "SemanticControlStyles.axaml"));
        var requiredSelectors = new[]
        {
            "ListBox.navigation-list ListBoxItem",
            "Button",
            "TextBox",
            "ComboBox",
            "ComboBoxItem",
            "DatePicker",
            "TimePicker",
            "ScrollBar"
        };

        Assert.All(requiredSelectors, selector =>
            Assert.Contains($"Selector=\"{selector}", source, StringComparison.Ordinal));
        Assert.Contains("DesignFocusRingBrush", source, StringComparison.Ordinal);
        Assert.Contains("ComboBox:dropdownopen /template/ Border#PopupBorder", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Red", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Salmon", source, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ThemeXamlKeysAreUniqueAndFluentMappingsReferenceSemanticOwners()
    {
        foreach (var themeFile in new[] { "DarkThemeResources.axaml", "LightThemeResources.axaml" })
        {
            var document = XDocument.Load(AppPath("Themes", themeFile));
            var keyAttribute = XName.Get("Key", "http://schemas.microsoft.com/winfx/2006/xaml");
            var keys = document.Root!
                .Elements()
                .Select(element => (string?)element.Attribute(keyAttribute))
                .Where(static key => key is not null)
                .Select(static key => key!)
                .ToArray();

            Assert.Equal(keys.Length, keys.Distinct(StringComparer.Ordinal).Count());

            var fluentMappings = document.Root!
                .Elements()
                .Where(element => element.Name.LocalName == "StaticResource")
                .ToArray();
            Assert.NotEmpty(fluentMappings);
            Assert.All(
                fluentMappings,
                mapping => Assert.StartsWith(
                    "Design",
                    (string?)mapping.Attribute("ResourceKey"),
                    StringComparison.Ordinal));
        }
    }

    [Fact]
    public void ProductionAppViewsDoNotContainRawUiColorLiterals()
    {
        var appDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "NoxAeterna.App"));
        var approvedThemeFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            Path.GetFullPath(Path.Combine(appDirectory, "Themes", "DarkThemeResources.axaml")),
            Path.GetFullPath(Path.Combine(appDirectory, "Themes", "LightThemeResources.axaml"))
        };
        var sourceFiles = Directory
            .EnumerateFiles(appDirectory, "*.*", SearchOption.AllDirectories)
            .Where(path => path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) ||
                           path.EndsWith(".axaml", StringComparison.OrdinalIgnoreCase))
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .Where(path => !approvedThemeFiles.Contains(Path.GetFullPath(path)))
            .ToArray();
        var rawColorPattern = new Regex(
            @"#[0-9A-Fa-f]{6,8}\b|Color\.From(?:Rgb|Argb)\s*\(",
            RegexOptions.CultureInvariant);

        Assert.All(
            sourceFiles,
            path => Assert.DoesNotMatch(rawColorPattern, File.ReadAllText(path)));
    }

    private static string[] GetStringKeys(ResourceDictionary dictionary) =>
        dictionary.Keys
            .OfType<string>()
            .OrderBy(static key => key, StringComparer.Ordinal)
            .ToArray();

    private static void AssertExpectedColors(
        ResourceDictionary resources,
        IReadOnlyDictionary<string, string> expected)
    {
        foreach (var (key, value) in expected)
        {
            Assert.Equal(Color.Parse(value), GetColor(resources, key));
        }
    }

    private static string AppPath(params string[] segments)
    {
        var pathSegments = new[]
        {
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            "NoxAeterna.App"
        }.Concat(segments).ToArray();
        return Path.GetFullPath(Path.Combine(pathSegments));
    }

    private static Color GetColor(ResourceDictionary resources, string key) =>
        Assert.IsType<Color>(resources[key]);

    private static double Contrast(Color first, Color second)
    {
        var lighter = Math.Max(Luminance(first), Luminance(second));
        var darker = Math.Min(Luminance(first), Luminance(second));
        return (lighter + 0.05d) / (darker + 0.05d);
    }

    private static double Luminance(Color color) =>
        (0.2126d * Linear(color.R)) +
        (0.7152d * Linear(color.G)) +
        (0.0722d * Linear(color.B));

    private static double Linear(byte channel)
    {
        var value = channel / 255d;
        return value <= 0.04045d
            ? value / 12.92d
            : Math.Pow((value + 0.055d) / 1.055d, 2.4d);
    }
}
