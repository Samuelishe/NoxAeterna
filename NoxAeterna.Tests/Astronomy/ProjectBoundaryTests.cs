using System.Xml.Linq;

namespace NoxAeterna.Tests.Astronomy;

public sealed class ProjectBoundaryTests
{
    [Fact]
    public void AstronomyProject_DoesNotReferenceAvalonia()
    {
        var projectDocument = LoadProjectDocument("NoxAeterna.Astronomy", "NoxAeterna.Astronomy.csproj");
        var packageReferences = projectDocument
            .Descendants("PackageReference")
            .Select(element => (string?)element.Attribute("Include"))
            .Where(static value => value is not null)
            .ToArray();

        Assert.DoesNotContain(packageReferences, package => package!.StartsWith("Avalonia", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void DomainProject_DoesNotReferenceForbiddenPackages()
    {
        var projectDocument = LoadProjectDocument("NoxAeterna.Domain", "NoxAeterna.Domain.csproj");
        var packageReferences = projectDocument
            .Descendants("PackageReference")
            .Select(element => (string?)element.Attribute("Include"))
            .Where(static value => value is not null)
            .ToArray();

        Assert.DoesNotContain(packageReferences, package => package!.Contains("Avalonia", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(packageReferences, package => package!.Contains("SQLite", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(packageReferences, package => package!.Contains("Dapper", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(packageReferences, package => package!.Contains("Swiss", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(packageReferences, package => package!.Contains("Serilog", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void SolutionProjects_DoNotReferenceSwissEphemerisPackages()
    {
        var projectFiles = new[]
        {
            LoadProjectDocument("NoxAeterna.Domain", "NoxAeterna.Domain.csproj"),
            LoadProjectDocument("NoxAeterna.Astronomy", "NoxAeterna.Astronomy.csproj")
        };

        var packageReferences = projectFiles
            .SelectMany(document => document.Descendants("PackageReference"))
            .Select(element => (string?)element.Attribute("Include"))
            .Where(static value => value is not null)
            .ToArray();

        Assert.DoesNotContain(packageReferences, package => package!.Contains("Swiss", StringComparison.OrdinalIgnoreCase));
    }

    private static XDocument LoadProjectDocument(string projectDirectory, string projectFileName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", projectDirectory, projectFileName);
        return XDocument.Load(Path.GetFullPath(path));
    }
}
