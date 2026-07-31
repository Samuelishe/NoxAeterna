using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using NoxAeterna.App.Shell;
using NoxAeterna.Presentation.Shell;

namespace NoxAeterna.Tests.App;

public sealed class ShellNavigationContractTests
{
    [Fact]
    public void MainWindowUsesAdaptiveCompactInlineSplitViewWithAccessibleLocalizedItems()
    {
        var source = File.ReadAllText(AppPath("MainWindow.axaml"));

        Assert.Contains("<SplitView", source, StringComparison.Ordinal);
        Assert.Contains("DisplayMode=\"CompactInline\"", source, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"NavigationToggleButton\"", source, StringComparison.Ordinal);
        Assert.Contains("Click=\"OnNavigationToggleClick\"", source, StringComparison.Ordinal);
        Assert.Contains("Data=\"{Binding IconGeometry}\"", source, StringComparison.Ordinal);
        Assert.DoesNotContain("ColumnDefinitions=\"240,*\"", source, StringComparison.Ordinal);

        var styles = File.ReadAllText(AppPath("Themes", "SemanticControlStyles.axaml"));
        Assert.Contains("ToolTip.Tip\" Value=\"{ReflectionBinding Label}\"", styles, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.Name\" Value=\"{ReflectionBinding Label}\"", styles, StringComparison.Ordinal);
    }

    [Fact]
    public void ShellDimensionsAreCentralizedAndAppliedByTheWindow()
    {
        var source = File.ReadAllText(AppPath("MainWindow.axaml.cs"));

        Assert.InRange(ShellNavigationLayout.ExpandedPaneLength, 210d, 240d);
        Assert.InRange(ShellNavigationLayout.CompactPaneLength, 64d, 72d);
        Assert.True(ShellNavigationLayout.CompactViewportThreshold > 1180d);
        Assert.Contains("ShellNavigationLayout.ExpandedPaneLength", source, StringComparison.Ordinal);
        Assert.Contains("ShellNavigationLayout.CompactPaneLength", source, StringComparison.Ordinal);
        Assert.Contains("ShellNavigationLayout.CompactViewportThreshold", source, StringComparison.Ordinal);
    }

    [Fact]
    public void ProjectOwnedIconCatalogCoversEveryIdentifierInsideNormalizedCoordinateBounds()
    {
        foreach (var iconId in Enum.GetValues<ShellNavigationIconId>())
        {
            var pathData = ShellNavigationIconCatalog.GetPathData(iconId);
            var coordinates = Regex
                .Matches(pathData, @"\d+(?:\.\d+)?", RegexOptions.CultureInvariant)
                .Select(match => double.Parse(match.Value, CultureInfo.InvariantCulture))
                .ToArray();

            Assert.False(string.IsNullOrWhiteSpace(pathData));
            Assert.DoesNotContain("http", pathData, StringComparison.OrdinalIgnoreCase);
            Assert.NotEmpty(coordinates);
            Assert.All(coordinates, coordinate => Assert.InRange(coordinate, 0d, 24d));
        }
    }

    [Fact]
    public void AppAddsNoExternalIconOrFontDependency()
    {
        var project = XDocument.Load(AppPath("NoxAeterna.App.csproj"));
        var packageNames = project
            .Descendants("PackageReference")
            .Select(element => (string?)element.Attribute("Include"))
            .Where(static value => value is not null)
            .Select(static value => value!);
        var source = File.ReadAllText(AppPath("Shell", "ShellNavigationIconCatalog.cs"));

        Assert.DoesNotContain(packageNames, package =>
            package.Contains("Icon", StringComparison.OrdinalIgnoreCase) ||
            package.Contains("Font", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain("http://", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("https://", source, StringComparison.OrdinalIgnoreCase);
    }

    private static string AppPath(params string[] segments)
    {
        var pathSegments = new[]
        {
            AppContext.BaseDirectory,
            "..", "..", "..", "..",
            "NoxAeterna.App"
        }.Concat(segments).ToArray();
        return Path.GetFullPath(Path.Combine(pathSegments));
    }
}
