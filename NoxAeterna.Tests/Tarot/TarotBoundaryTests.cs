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

    [Fact]
    public void RuntimeRandomAdapter_LivesOutsideDomainBehindProjectContract()
    {
        var domainTarotFiles = Directory.GetFiles(GetRepositoryPath("NoxAeterna.Domain", "Tarot"), "*.cs");
        var adapterPath = GetRepositoryPath("NoxAeterna.Infrastructure", "Tarot", "SystemTarotRandomSource.cs");
        var adapterSource = File.ReadAllText(adapterPath);

        Assert.True(File.Exists(adapterPath));
        Assert.DoesNotContain(domainTarotFiles, path => Path.GetFileName(path) == "SystemTarotRandomSource.cs");
        Assert.Contains("ITarotRandomSource", adapterSource, StringComparison.Ordinal);
        Assert.Contains("RandomNumberGenerator.GetInt32", adapterSource, StringComparison.Ordinal);
        Assert.DoesNotContain("static Random", adapterSource, StringComparison.Ordinal);
    }

    [Fact]
    public void LupusNoctisRepositoryPack_ContainsNoRejectedStudiesContactSheetsOrFonts()
    {
        var packRoot = GetRepositoryPath(
            "resources", "assets", "tarot", "artwork-packs", "lupus-noctis");
        var files = Directory.GetFiles(packRoot, "*", SearchOption.AllDirectories);
        var relativePaths = files
            .Select(path => Path.GetRelativePath(packRoot, path).Replace('\\', '/'))
            .ToArray();

        Assert.DoesNotContain(relativePaths, path =>
            path.Contains("studies/A0", StringComparison.OrdinalIgnoreCase) ||
            path.Contains("studies/A1", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(relativePaths, path =>
            path.Contains("contact-sheet", StringComparison.OrdinalIgnoreCase) ||
            path.Contains("collage", StringComparison.OrdinalIgnoreCase) ||
            path.Contains("combined-preview", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(relativePaths, path =>
            path.EndsWith(".ttf", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(".otf", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(".woff", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(".woff2", StringComparison.OrdinalIgnoreCase));
        Assert.Equal(6, relativePaths.Count(path => path.EndsWith(".png", StringComparison.OrdinalIgnoreCase)));
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
