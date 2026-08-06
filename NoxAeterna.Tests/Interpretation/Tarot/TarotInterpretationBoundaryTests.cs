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
            .Select(item => Path.GetFileNameWithoutExtension((string?)item.Attribute("Include")))
            .Order(StringComparer.Ordinal)
            .ToArray();
        var packages = project.Descendants("PackageReference").ToArray();

        Assert.Equal(new[] { "NoxAeterna.Domain", "NoxAeterna.Symbolics" }, references);
        Assert.Empty(packages);
        Assert.DoesNotContain(references, name => name is "NoxAeterna.App" or "NoxAeterna.Presentation" or "NoxAeterna.Infrastructure");
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
    public void I1_CreatesNoProductionPackResourcesOrFixturePack()
    {
        Assert.False(Directory.Exists(RepositoryPath("resources", "interpretation")));
        Assert.DoesNotContain(
            Directory.GetDirectories(RepositoryPath("NoxAeterna.Tests"), "*", SearchOption.AllDirectories),
            path => path.Contains("fixture-pack", StringComparison.OrdinalIgnoreCase));
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

    private static string RepositoryPath(params string[] segments)
    {
        var all = new[] { AppContext.BaseDirectory, "..", "..", "..", ".." }.Concat(segments).ToArray();
        return Path.GetFullPath(Path.Combine(all));
    }
}
