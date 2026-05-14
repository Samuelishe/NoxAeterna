using System.Xml.Linq;

namespace NoxAeterna.Tests.App;

public sealed class AppBoundaryTests
{
    [Fact]
    public void AppProject_DoesNotReferenceAstronomyProject()
    {
        var projectDocument = LoadProjectDocument("NoxAeterna.App", "NoxAeterna.App.csproj");

        var projectReferences = projectDocument
            .Descendants("ProjectReference")
            .Select(element => (string?)element.Attribute("Include"))
            .Where(static value => value is not null)
            .ToArray();

        Assert.DoesNotContain(projectReferences, path => path!.Contains("NoxAeterna.Astronomy", StringComparison.OrdinalIgnoreCase));
    }

    private static XDocument LoadProjectDocument(string projectDirectory, string projectFileName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", projectDirectory, projectFileName);
        return XDocument.Load(Path.GetFullPath(path));
    }
}
