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

    [Fact]
    public void PresentationPreferenceAndTarotState_DoNotUseFileSystemEnvironmentOrJsonSerialization()
    {
        var projectRoot = GetRepositoryPath("NoxAeterna.Presentation");
        var sourcePaths = Directory.GetFiles(Path.Combine(projectRoot, "Preferences"), "*.cs", SearchOption.AllDirectories)
            .Concat(Directory.GetFiles(Path.Combine(projectRoot, "Tarot"), "*.cs", SearchOption.AllDirectories))
            .Order(StringComparer.Ordinal)
            .ToArray();
        var source = string.Join(Environment.NewLine, sourcePaths.Select(File.ReadAllText));

        Assert.DoesNotContain("using System.IO", source, StringComparison.Ordinal);
        Assert.DoesNotContain("File.", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Directory.", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Environment.GetFolderPath", source, StringComparison.Ordinal);
        Assert.DoesNotContain("System.Text.Json", source, StringComparison.Ordinal);
        Assert.DoesNotContain("JsonSerializer", source, StringComparison.Ordinal);
    }

    private static XDocument LoadProjectDocument(string projectDirectory, string projectFileName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", projectDirectory, projectFileName);
        return XDocument.Load(Path.GetFullPath(path));
    }

    private static string GetRepositoryPath(params string[] segments)
    {
        var pathSegments = new[] { AppContext.BaseDirectory, "..", "..", "..", ".." }
            .Concat(segments)
            .ToArray();
        return Path.GetFullPath(Path.Combine(pathSegments));
    }
}
