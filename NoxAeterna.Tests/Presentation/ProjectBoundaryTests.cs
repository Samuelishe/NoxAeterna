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
    public void PresentationProject_ReferencesInterpretationOneWayForTypedMeaningInput()
    {
        var presentation = LoadProjectDocument("NoxAeterna.Presentation", "NoxAeterna.Presentation.csproj");
        var interpretation = LoadProjectDocument("NoxAeterna.Interpretation", "NoxAeterna.Interpretation.csproj");
        var presentationReferences = presentation.Descendants("ProjectReference")
            .Select(element => ((string?)element.Attribute("Include"))?.Replace('\\', '/'))
            .Where(static value => value is not null)
            .Cast<string>()
            .ToArray();
        var interpretationReferences = interpretation.Descendants("ProjectReference")
            .Select(element => ((string?)element.Attribute("Include"))?.Replace('\\', '/'))
            .Where(static value => value is not null)
            .Cast<string>()
            .ToArray();

        Assert.Contains(presentationReferences, reference => reference.EndsWith(
            "NoxAeterna.Interpretation/NoxAeterna.Interpretation.csproj", StringComparison.Ordinal));
        Assert.DoesNotContain(interpretationReferences, reference => reference.Contains(
            "NoxAeterna.Presentation", StringComparison.Ordinal));
    }

    [Fact]
    public void SingleCardPresentationModel_HasNoAvaloniaColorsFontsOrStorageDetails()
    {
        var source = File.ReadAllText(GetRepositoryPath(
            "NoxAeterna.Presentation", "Tarot", "TarotSingleCardInterpretationPresentation.cs"));

        Assert.DoesNotContain("Avalonia", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Brush", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Color", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Font", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Json", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Readiness", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Diagnostic", source, StringComparison.Ordinal);
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
