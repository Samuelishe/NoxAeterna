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

    private static XDocument LoadProjectDocument(string projectDirectory, string projectFileName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", projectDirectory, projectFileName);
        return XDocument.Load(Path.GetFullPath(path));
    }
}
