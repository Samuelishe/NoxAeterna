using System.Xml.Linq;

namespace NoxAeterna.Tests.Infrastructure;

public sealed class InfrastructureBoundaryTests
{
    [Fact]
    public void InfrastructureProject_CanReferenceSwissEphemerisWithoutAvalonia()
    {
        var projectDocument = LoadProjectDocument("NoxAeterna.Infrastructure", "NoxAeterna.Infrastructure.csproj");
        var packageReferences = projectDocument
            .Descendants("PackageReference")
            .Select(element => (string?)element.Attribute("Include"))
            .Where(static value => value is not null)
            .ToArray();

        Assert.Contains(packageReferences, package => package!.Contains("SwissEphNet", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(packageReferences, package => package!.StartsWith("Avalonia", StringComparison.OrdinalIgnoreCase));
    }

    private static XDocument LoadProjectDocument(string projectDirectory, string projectFileName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", projectDirectory, projectFileName);
        return XDocument.Load(Path.GetFullPath(path));
    }
}
