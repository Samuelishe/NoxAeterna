using System.Xml.Linq;

namespace NoxAeterna.Tests.App;

public sealed class AppBoundaryTests
{
    [Fact]
    public void AppProject_ReferencesAstronomyAsCompositionRoot()
    {
        var projectDocument = LoadProjectDocument("NoxAeterna.App", "NoxAeterna.App.csproj");

        var projectReferences = projectDocument
            .Descendants("ProjectReference")
            .Select(element => (string?)element.Attribute("Include"))
            .Where(static value => value is not null)
            .ToArray();

        Assert.Contains(projectReferences, path => path!.Contains("NoxAeterna.Astronomy", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void NormalMainWindowStartup_DoesNotMaterializeDevelopmentSampleChart()
    {
        var path = Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            "NoxAeterna.App",
            "MainWindow.axaml.cs");
        var source = File.ReadAllText(Path.GetFullPath(path));

        Assert.DoesNotContain("DevelopmentSampleChartBuildResultFactory", source, StringComparison.Ordinal);
    }

    [Fact]
    public void SettingsJsonAdapter_IsOwnedByAppAndAddsNoPersistencePackage()
    {
        var preferencesDirectory = RepositoryPath("NoxAeterna.App", "Preferences");
        var adapterSource = File.ReadAllText(Path.Combine(preferencesDirectory, "JsonUserPreferencesStore.cs"));
        var appProject = LoadProjectDocument("NoxAeterna.App", "NoxAeterna.App.csproj");
        var packages = appProject.Descendants("PackageReference")
            .Select(element => (string?)element.Attribute("Include"))
            .Where(static value => value is not null)
            .Cast<string>()
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.True(Directory.Exists(preferencesDirectory));
        Assert.Contains("namespace NoxAeterna.App.Preferences", adapterSource, StringComparison.Ordinal);
        Assert.Contains("System.Text.Json", adapterSource, StringComparison.Ordinal);
        Assert.Equal(new[] { "Avalonia", "Avalonia.Desktop", "Avalonia.Themes.Fluent" }, packages);
    }

    [Fact]
    public void SettingsAdapter_DoesNotResolveExecutableOrWorkingDirectoryPaths()
    {
        var preferencesDirectory = RepositoryPath("NoxAeterna.App", "Preferences");
        var source = string.Join(
            Environment.NewLine,
            Directory.GetFiles(preferencesDirectory, "*.cs", SearchOption.AllDirectories)
                .Order(StringComparer.Ordinal)
                .Select(File.ReadAllText));

        Assert.DoesNotContain("AppContext.BaseDirectory", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Environment.CurrentDirectory", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Directory.GetCurrentDirectory", source, StringComparison.Ordinal);
        Assert.Contains("Environment.SpecialFolder.LocalApplicationData", source, StringComparison.Ordinal);
    }

    private static XDocument LoadProjectDocument(string projectDirectory, string projectFileName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", projectDirectory, projectFileName);
        return XDocument.Load(Path.GetFullPath(path));
    }

    private static string RepositoryPath(params string[] segments)
    {
        var pathSegments = new[] { AppContext.BaseDirectory, "..", "..", "..", ".." }
            .Concat(segments)
            .ToArray();
        return Path.GetFullPath(Path.Combine(pathSegments));
    }
}
