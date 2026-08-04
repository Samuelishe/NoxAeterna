using System.Text.Json;
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
    public void LupusNoctisRepositoryPack_SeparatesProductionAssetsFromBoundedReviewStudies()
    {
        var packRoot = GetRepositoryPath(
            "resources", "assets", "tarot", "artwork-packs", "lupus-noctis");
        var files = Directory.GetFiles(packRoot, "*", SearchOption.AllDirectories);
        var relativePaths = files
            .Select(path => Path.GetRelativePath(packRoot, path).Replace('\\', '/'))
            .ToArray();
        var relativeDirectories = Directory.GetDirectories(packRoot, "*", SearchOption.AllDirectories)
            .Select(path => Path.GetRelativePath(packRoot, path).Replace('\\', '/'))
            .ToArray();

        Assert.DoesNotContain(relativeDirectories, path =>
            path.Equals("studies/A0", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("studies/A0/", StringComparison.OrdinalIgnoreCase) ||
            path.Equals("studies/A1", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("studies/A1/", StringComparison.OrdinalIgnoreCase) ||
            path.Equals("studies/A16", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("studies/A16/", StringComparison.OrdinalIgnoreCase) ||
            path.Equals("studies/A17", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("studies/A17/", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(relativeDirectories, path =>
            path.Equals("studies/A18", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("studies/A18/", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(relativeDirectories, path =>
            path.Equals("studies/A19", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("studies/A19/", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(relativePaths, path =>
            path.Contains("contact-sheet", StringComparison.OrdinalIgnoreCase) ||
            path.Contains("collage", StringComparison.OrdinalIgnoreCase) ||
            path.Contains("combined-preview", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(relativePaths, path =>
            path.EndsWith(".ttf", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(".otf", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(".woff", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(".woff2", StringComparison.OrdinalIgnoreCase));

        var pngPaths = relativePaths
            .Where(path => path.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            .ToArray();
        var productionPngPaths = pngPaths
            .Where(path => path.StartsWith("cards/", StringComparison.Ordinal))
            .Order(StringComparer.Ordinal)
            .ToArray();
        const string canonicalStudyPattern = @"^studies/A[1-9][0-9]*/[^/]+\.png$";
        var studyPngPaths = pngPaths
            .Where(path => System.Text.RegularExpressions.Regex.IsMatch(
                path,
                canonicalStudyPattern,
                System.Text.RegularExpressions.RegexOptions.CultureInvariant))
            .ToArray();
        string[] expectedStudyPngPaths =
        [
            "studies/A20/hierophant.png",
            "studies/A20/nine-of-cups.png",
            "studies/A20/queen-of-swords.png"
        ];

        Assert.Equal(expectedStudyPngPaths, studyPngPaths.Order(StringComparer.Ordinal));

        Assert.DoesNotContain(pngPaths, path =>
            !path.StartsWith("cards/", StringComparison.Ordinal) &&
            !studyPngPaths.Contains(path, StringComparer.Ordinal));

        using var manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(packRoot, "artwork-pack.json")));
        var manifestAssetPaths = manifest.RootElement
            .GetProperty("cards")
            .EnumerateArray()
            .Select(card => card.GetProperty("assetPath").GetString()!)
            .Order(StringComparer.Ordinal)
            .ToArray();
        Assert.All(manifestAssetPaths, path => Assert.Matches(
            @"^cards/(?:major/[^/]+|minor/[^/]+/[^/]+)\.png$",
            path));
        Assert.Equal(manifestAssetPaths, productionPngPaths);

        var recordPaths = Directory.GetFiles(
            Path.Combine(packRoot, "records"),
            "*.md",
            SearchOption.AllDirectories);
        foreach (var studyPath in studyPngPaths)
        {
            var matchingRecords = recordPaths
                .Select(path => (Path: path, Content: File.ReadAllText(path)))
                .Where(record => record.Content.Contains(studyPath, StringComparison.Ordinal))
                .ToArray();
            var record = Assert.Single(matchingRecords);
            Assert.Contains("Owner acceptance: **Pending**", record.Content, StringComparison.Ordinal);
            Assert.DoesNotContain(studyPath, manifestAssetPaths);

            var studyContent = File.ReadAllBytes(Path.Combine(
                packRoot,
                studyPath.Replace('/', Path.DirectorySeparatorChar)));
            Assert.DoesNotContain(productionPngPaths, productionPath =>
                File.ReadAllBytes(Path.Combine(
                        packRoot,
                        productionPath.Replace('/', Path.DirectorySeparatorChar)))
                    .SequenceEqual(studyContent));
        }

        var appProjectSource = File.ReadAllText(GetRepositoryPath(
            "NoxAeterna.App",
            "NoxAeterna.App.csproj"));
        Assert.Contains(
            @"lupus-noctis\cards\**\*.png",
            appProjectSource,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            @"lupus-noctis\studies\",
            appProjectSource,
            StringComparison.OrdinalIgnoreCase);
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
