using System.Xml.Linq;
using NoxAeterna.App.Preferences;

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
        Assert.Contains("Stream", source, StringComparison.Ordinal);
        Assert.Contains("MemoryStream", source, StringComparison.Ordinal);
    }

    [Fact]
    public void I3_KeepsSyntheticFixturesTestOnlyAndCreatesOnlyClassicSkeletonResource()
    {
        var productionRoot = RepositoryPath("resources", "interpretation", "tarot", "packs", "classic");
        var productionFiles = Directory.GetFiles(productionRoot, "*", SearchOption.AllDirectories);
        Assert.Equal(new[] { Path.Combine(productionRoot, "interpretation-pack.json") }, productionFiles);
        Assert.False(Directory.Exists(RepositoryPath("resources", "interpretation", "tarot", "working")));
        Assert.False(Directory.Exists(Path.Combine(productionRoot, "content")));
        Assert.False(Directory.Exists(Path.Combine(productionRoot, "indexes")));
        var fixtureRoot = RepositoryPath("NoxAeterna.Tests", "TestData", "Interpretation");
        Assert.True(Directory.Exists(fixtureRoot));
        Assert.All(
            Directory.GetFiles(fixtureRoot, "*.json", SearchOption.AllDirectories),
            path => Assert.StartsWith(RepositoryPath("NoxAeterna.Tests"), path, StringComparison.OrdinalIgnoreCase));
        Assert.False(Directory.Exists(Path.Combine(AppContext.BaseDirectory, "TestData", "Interpretation")));
        Assert.DoesNotContain(
            Directory.GetFiles(fixtureRoot, "*.json", SearchOption.AllDirectories).Select(File.ReadAllText),
            text => text.Contains("Classic prose", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(
            productionFiles.Select(File.ReadAllText),
            text => text.Contains("situation", StringComparison.OrdinalIgnoreCase) ||
                    text.Contains("interaction", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void PresentationStillHasNoPackSelectorAndSettingsRemainSchemaOne()
    {
        var presentationSource = string.Join(
            Environment.NewLine,
            Directory.GetFiles(RepositoryPath("NoxAeterna.Presentation", "Tarot"), "*.cs", SearchOption.AllDirectories)
                .Order(StringComparer.Ordinal)
                .Select(File.ReadAllText));

        Assert.DoesNotContain("TarotInterpretationPackSelector", presentationSource, StringComparison.Ordinal);
        Assert.DoesNotContain("TarotInterpretationPackOption", presentationSource, StringComparison.Ordinal);
        Assert.Equal(1, JsonUserPreferencesStore.CurrentSchemaVersion);
        Assert.Null(typeof(TarotWorkspacePreferencesDocument).GetProperty("SelectedInterpretationPackId"));
    }

    [Fact]
    public void CurrentUnavailablePlaceholderRemainsUntilI4()
    {
        var presentation = File.ReadAllText(RepositoryPath(
            "NoxAeterna.Presentation", "Tarot", "TarotWorkspaceViewModel.cs"));
        var app = File.ReadAllText(RepositoryPath(
            "NoxAeterna.App", "Tarot", "TarotWorkspaceControl.cs"));

        Assert.Contains("ui.tarot.interpretation.unavailable", presentation, StringComparison.Ordinal);
        Assert.Contains("InterpretationUnavailableKey", app, StringComparison.Ordinal);
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
