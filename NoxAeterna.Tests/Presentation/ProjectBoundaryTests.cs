using System.Xml.Linq;

namespace NoxAeterna.Tests.Presentation;

public sealed class ProjectBoundaryTests
{
    [Fact]
    public void GeometryProject_DoesNotReferenceAvalonia()
    {
        var projectDocument = LoadProjectDocument("NoxAeterna.Geometry", "NoxAeterna.Geometry.csproj");
        var packageReferences = projectDocument
            .Descendants("PackageReference")
            .Select(element => (string?)element.Attribute("Include"))
            .Where(static value => value is not null)
            .ToArray();

        Assert.DoesNotContain(packageReferences, package => package!.StartsWith("Avalonia", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void PresentationProject_DoesNotReferenceAvalonia()
    {
        var projectDocument = LoadProjectDocument("NoxAeterna.Presentation", "NoxAeterna.Presentation.csproj");
        var packageReferences = projectDocument
            .Descendants("PackageReference")
            .Select(element => (string?)element.Attribute("Include"))
            .Where(static value => value is not null)
            .ToArray();

        Assert.DoesNotContain(packageReferences, package => package!.StartsWith("Avalonia", StringComparison.OrdinalIgnoreCase));
    }

    private static XDocument LoadProjectDocument(string projectDirectory, string projectFileName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", projectDirectory, projectFileName);
        return XDocument.Load(Path.GetFullPath(path));
    }
}
