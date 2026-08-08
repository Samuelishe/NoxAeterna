using System.Text.Json;
using System.Xml.Linq;
using NoxAeterna.App.Preferences;
using NoxAeterna.Domain.Tarot;

namespace NoxAeterna.Tests.Interpretation.Tarot;

public sealed class TarotInterpretationBoundaryTests
{
    [Fact]
    public void InterpretationProject_ReferencesOnlyDomainAndSymbolicsWithoutPackages()
    {
        var project = XDocument.Load(RepositoryPath(
            "NoxAeterna.Interpretation", "NoxAeterna.Interpretation.csproj"));
        var references = project.Descendants("ProjectReference")
            .Select(ProjectReferenceName)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var packages = project.Descendants("PackageReference").ToArray();

        Assert.Equal(new[] { "NoxAeterna.Domain", "NoxAeterna.Symbolics" }, references);
        Assert.Empty(packages);
        Assert.DoesNotContain(references, name => name is "NoxAeterna.App" or "NoxAeterna.Presentation" or "NoxAeterna.Infrastructure");
    }

    [Theory]
    [InlineData(@"..\NoxAeterna.Domain\NoxAeterna.Domain.csproj", "NoxAeterna.Domain")]
    [InlineData("../NoxAeterna.Domain/NoxAeterna.Domain.csproj", "NoxAeterna.Domain")]
    [InlineData(@"NoxAeterna.Symbolics\NoxAeterna.Symbolics.csproj", "NoxAeterna.Symbolics")]
    [InlineData("NoxAeterna.Symbolics/NoxAeterna.Symbolics.csproj", "NoxAeterna.Symbolics")]
    public void ProjectReferenceName_NormalizesBothSeparatorsIndependentlyOfHost(
        string include,
        string expected)
    {
        var item = new XElement("ProjectReference", new XAttribute("Include", include));

        Assert.Equal(expected, ProjectReferenceName(item));
    }

    [Fact]
    public void InterpretationSource_HasNoFilesystemAppDataNetworkSqliteUiOrDependencyInjectionOwnership()
    {
        var source = InterpretationSource();

        Assert.DoesNotContain("System.IO.File", source, StringComparison.Ordinal);
        Assert.DoesNotContain("File.", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Directory.", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Environment.GetFolderPath", source, StringComparison.Ordinal);
        Assert.DoesNotContain("AppData", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("HttpClient", source, StringComparison.Ordinal);
        Assert.DoesNotContain("System.Net.Http", source, StringComparison.Ordinal);
        Assert.DoesNotContain("SQLite", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Avalonia", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Microsoft.Extensions.DependencyInjection", source, StringComparison.Ordinal);
    }

    [Fact]
    public void SourceTreeContainsCompleteCanonicalRussianPairCorpusAndRetainsExcludedProseBoundaries()
    {
        var productionRoot = RepositoryPath("resources", "interpretation", "tarot", "sources", "classic");
        var russianRoot = Path.Combine(productionRoot, "content", "ru");
        var englishRoot = Path.Combine(productionRoot, "content", "en");
        var singleCardRoot = Path.Combine(russianRoot, "single-card");
        var orientedPairRoot = Path.Combine(russianRoot, "oriented-pairs");
        var vocabularyRoot = Path.Combine(russianRoot, "vocabulary");
        var productionFiles = Directory.GetFiles(productionRoot, "*", SearchOption.AllDirectories).Order(StringComparer.Ordinal).ToArray();
        var singleCardFiles = Directory.GetFiles(singleCardRoot, "*.json", SearchOption.TopDirectoryOnly)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var expectedSingleCardFiles = StandardTarotCatalog.Deck.Cards
            .Select(card => Path.Combine(singleCardRoot, $"{card.Id.Value}.json"))
            .Order(StringComparer.Ordinal)
            .ToArray();
        var canonicalCardIds = StandardTarotCatalog.Deck.Cards.Select(card => card.Id.Value).ToArray();
        var expectedAuthoredPairIdentities = canonicalCardIds
            .SelectMany((cardAId, index) => canonicalCardIds.Skip(index + 1)
                .Select(cardBId => $"{cardAId}__{cardBId}"))
            .ToArray();
        var orientedPairFiles = Directory.GetFiles(orientedPairRoot, "*.json", SearchOption.TopDirectoryOnly)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var expectedOrientedPairFiles = expectedAuthoredPairIdentities
            .Select(identity => Path.Combine(orientedPairRoot, $"{identity}.json"))
            .Order(StringComparer.Ordinal)
            .ToArray();
        var orientedPairStateCounts = orientedPairFiles.Select(file =>
        {
            using var bundle = JsonDocument.Parse(File.ReadAllText(file));
            return bundle.RootElement.GetProperty("states").EnumerateObject().Count();
        }).ToArray();
        var vocabularyFiles = Directory.GetFiles(vocabularyRoot, "*.json", SearchOption.TopDirectoryOnly);
        var englishFiles = Directory.GetFiles(englishRoot, "*", SearchOption.AllDirectories);
        using var manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(productionRoot, "interpretation-pack.json")));

        Assert.Equal(3145, productionFiles.Length);
        Assert.Equal(78, singleCardFiles.Length);
        Assert.Equal(expectedSingleCardFiles, singleCardFiles);
        Assert.Equal(3003, orientedPairFiles.Length);
        Assert.Equal(expectedOrientedPairFiles, orientedPairFiles);
        Assert.All(orientedPairStateCounts, count => Assert.Equal(4, count));
        Assert.Equal(12012, orientedPairStateCounts.Sum());
        Assert.Equal("major.chariot__major.death", expectedAuthoredPairIdentities[0]);
        Assert.Equal("major.hanged-man__minor.swords.seven", expectedAuthoredPairIdentities[499]);
        Assert.Equal("major.hanged-man__minor.swords.six", expectedAuthoredPairIdentities[500]);
        Assert.Equal("major.moon__minor.cups.knight", expectedAuthoredPairIdentities[999]);
        Assert.Equal("major.moon__minor.cups.nine", expectedAuthoredPairIdentities[1000]);
        Assert.Equal("minor.cups.ace__minor.swords.seven", expectedAuthoredPairIdentities[1499]);
        Assert.Equal("minor.cups.ace__minor.swords.six", expectedAuthoredPairIdentities[1500]);
        Assert.Equal("minor.cups.six__minor.wands.ace", expectedAuthoredPairIdentities[1999]);
        Assert.Equal("minor.cups.six__minor.wands.eight", expectedAuthoredPairIdentities[2000]);
        Assert.Equal("minor.pentacles.seven__minor.wands.nine", expectedAuthoredPairIdentities[2499]);
        Assert.Equal("minor.pentacles.seven__minor.wands.page", expectedAuthoredPairIdentities[2500]);
        Assert.Equal("minor.wands.three__minor.wands.two", expectedAuthoredPairIdentities[^1]);
        Assert.Equal(61, vocabularyFiles.Length);
        Assert.Contains(Path.Combine(productionRoot, "interpretation-pack.json"), productionFiles);
        Assert.Contains(Path.Combine(russianRoot, "labels.json"), productionFiles);
        Assert.Equal([Path.Combine(englishRoot, "labels.json")], englishFiles);
        var readiness = manifest.RootElement.GetProperty("modules")
            .EnumerateObject()
            .SelectMany(mode => mode.Value.EnumerateObject()
                .Select(locale => new
                {
                    Identity = $"{mode.Name}/{locale.Name}",
                    Ready = locale.Value.GetProperty("ready").GetBoolean()
                }))
            .ToArray();
        Assert.Equal(8, readiness.Length);
        Assert.True(Assert.Single(readiness, item => item.Identity == "single-card/ru").Ready);
        Assert.True(Assert.Single(readiness, item => item.Identity == "two-cards/ru").Ready);
        Assert.Equal(
            new[] { "single-card/ru", "two-cards/ru" },
            readiness.Where(static item => item.Ready).Select(static item => item.Identity).Order(StringComparer.Ordinal));
        Assert.Equal(6, readiness.Count(static item => !item.Ready));
        Assert.False(Directory.Exists(RepositoryPath("resources", "interpretation", "tarot", "working")));
        Assert.False(Directory.Exists(Path.Combine(russianRoot, "three-card-positions")));
        Assert.False(Directory.Exists(Path.Combine(russianRoot, "synthesis")));
        Assert.False(Directory.Exists(Path.Combine(AppContext.BaseDirectory, "TestData", "Interpretation")));
    }

    [Fact]
    public void PresentationOwnsTypedPackSelectionWhileSettingsUseSchemaTwo()
    {
        var presentationSource = string.Join(
            Environment.NewLine,
            Directory.GetFiles(RepositoryPath("NoxAeterna.Presentation", "Tarot"), "*.cs", SearchOption.AllDirectories)
                .Order(StringComparer.Ordinal)
                .Select(File.ReadAllText));

        Assert.DoesNotContain("File.", presentationSource, StringComparison.Ordinal);
        Assert.DoesNotContain("Directory.", presentationSource, StringComparison.Ordinal);
        Assert.Contains("TarotInterpretationPackOption", presentationSource, StringComparison.Ordinal);
        Assert.Equal(2, JsonUserPreferencesStore.CurrentSchemaVersion);
        Assert.NotNull(typeof(TarotWorkspacePreferencesDocument).GetProperty("SelectedInterpretationPackId"));
    }

    [Fact]
    public void I4RemovesUnavailablePlaceholderFromActiveProductionCode()
    {
        var presentation = File.ReadAllText(RepositoryPath(
            "NoxAeterna.Presentation", "Tarot", "TarotWorkspaceViewModel.cs"));
        var app = File.ReadAllText(RepositoryPath(
            "NoxAeterna.App", "Tarot", "TarotWorkspaceControl.cs"));

        Assert.DoesNotContain("ui.tarot.interpretation.unavailable", presentation, StringComparison.Ordinal);
        Assert.DoesNotContain("InterpretationUnavailableKey", app, StringComparison.Ordinal);
        Assert.Contains("interpretationHost.Content = null", app, StringComparison.Ordinal);
        Assert.Contains("interpretationHost.IsVisible = false", app, StringComparison.Ordinal);
    }

    private static string InterpretationSource() => string.Join(
        Environment.NewLine,
        Directory.GetFiles(RepositoryPath("NoxAeterna.Interpretation"), "*.cs", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .Order(StringComparer.Ordinal)
            .Select(File.ReadAllText));

    private static string ProjectReferenceName(XElement item)
    {
        var include = (string?)item.Attribute("Include")
            ?? throw new InvalidOperationException("ProjectReference Include is missing.");
        var normalized = include.Replace('\\', '/');
        return Path.GetFileNameWithoutExtension(normalized);
    }

    private static string RepositoryPath(params string[] segments)
    {
        var all = new[] { AppContext.BaseDirectory, "..", "..", "..", ".." }.Concat(segments).ToArray();
        return Path.GetFullPath(Path.Combine(all));
    }
}
