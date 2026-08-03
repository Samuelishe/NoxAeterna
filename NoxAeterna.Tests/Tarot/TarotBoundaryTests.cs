using System.Xml.Linq;

namespace NoxAeterna.Tests.Tarot;

public sealed class TarotBoundaryTests
{
    [Fact]
    public void DomainAndSymbolics_DoNotReferenceAvaloniaOrInfrastructure()
    {
        var domain = LoadProjectDocument("NoxAeterna.Domain", "NoxAeterna.Domain.csproj");
        var symbolics = LoadProjectDocument("NoxAeterna.Symbolics", "NoxAeterna.Symbolics.csproj");
        var references = new[] { domain, symbolics }
            .SelectMany(document => document.Descendants("ProjectReference"))
            .Select(reference => (string?)reference.Attribute("Include"))
            .Where(static value => value is not null)
            .ToArray();
        var packages = new[] { domain, symbolics }
            .SelectMany(document => document.Descendants("PackageReference"))
            .Select(reference => (string?)reference.Attribute("Include"))
            .Where(static value => value is not null)
            .ToArray();

        Assert.DoesNotContain(references, value => value!.Contains("Infrastructure", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(references, value => value!.Contains("Avalonia", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(packages, value => value!.Contains("Avalonia", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void TarotFoundation_DoesNotAddExternalPackages()
    {
        var domainPackages = GetPackageReferences(LoadProjectDocument("NoxAeterna.Domain", "NoxAeterna.Domain.csproj"));
        var symbolicsPackages = GetPackageReferences(LoadProjectDocument("NoxAeterna.Symbolics", "NoxAeterna.Symbolics.csproj"));

        Assert.Equal(new[] { "NodaTime" }, domainPackages);
        Assert.Empty(symbolicsPackages);
    }

    [Fact]
    public void TarotSources_DoNotUseAmbientRandomnessOrSystemClock()
    {
        var tarotDirectory = GetRepositoryPath("NoxAeterna.Domain", "Tarot");
        var source = string.Join(
            Environment.NewLine,
            Directory.GetFiles(tarotDirectory, "*.cs", SearchOption.AllDirectories)
                .Order(StringComparer.Ordinal)
                .Select(File.ReadAllText));

        Assert.DoesNotContain("Random.Shared", source, StringComparison.Ordinal);
        Assert.DoesNotContain("DateTime.Now", source, StringComparison.Ordinal);
        Assert.DoesNotContain("DateTime.UtcNow", source, StringComparison.Ordinal);
    }

    private static string[] GetPackageReferences(XDocument document) => document
        .Descendants("PackageReference")
        .Select(reference => (string?)reference.Attribute("Include"))
        .Where(static value => value is not null)
        .Cast<string>()
        .Order(StringComparer.Ordinal)
        .ToArray();

    private static XDocument LoadProjectDocument(string projectDirectory, string projectFileName) =>
        XDocument.Load(GetRepositoryPath(projectDirectory, projectFileName));

    private static string GetRepositoryPath(params string[] segments)
    {
        var pathSegments = new[] { AppContext.BaseDirectory, "..", "..", "..", ".." }
            .Concat(segments)
            .ToArray();
        return Path.GetFullPath(Path.Combine(pathSegments));
    }
}
