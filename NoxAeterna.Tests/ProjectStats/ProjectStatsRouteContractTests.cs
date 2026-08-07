using System.Text.Json;
using System.Xml.Linq;

namespace NoxAeterna.Tests.ProjectStats;

public sealed class ProjectStatsRouteContractTests
{
    [Fact]
    public void DedicatedLeafIsNonOverlappingAndPrecedesAgentContextInRepositoryComposite()
    {
        var path = Path.Combine(ProjectStatsTestFixture.RepositoryRoot, "eng", "test-routes.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        var routes = document.RootElement.GetProperty("routes").EnumerateArray().ToArray();
        var leaf = Assert.Single(routes, route => route.GetProperty("name").GetString() == "Project-Stats");
        var composite = Assert.Single(routes, route => route.GetProperty("name").GetString() == "Repository-Verification");

        Assert.Equal("leaf", leaf.GetProperty("kind").GetString());
        Assert.Equal("FullyQualifiedName~NoxAeterna.Tests.ProjectStats", leaf.GetProperty("filter").GetString());
        Assert.False(leaf.GetProperty("milestoneOnly").GetBoolean());
        Assert.Equal(
            new[] { "Architecture-Boundaries", "Repository-Tooling", "Project-Stats", "Agent-Context" },
            composite.GetProperty("children").EnumerateArray().Select(static child => child.GetString()).ToArray());
    }

    [Fact]
    public void ToolProjectIsNetTenExecutableWithOnlyInterpretationCompilerBoundariesAndNoPackages()
    {
        var path = Path.Combine(
            ProjectStatsTestFixture.RepositoryRoot,
            "NoxAeterna.Tools.Repository",
            "NoxAeterna.Tools.Repository.csproj");
        var document = XDocument.Load(path);

        Assert.Equal("net10.0", document.Descendants("TargetFramework").Single().Value);
        Assert.Equal("Exe", document.Descendants("OutputType").Single().Value);
        Assert.Empty(document.Descendants("PackageReference"));
        Assert.Equal(
            new[]
            {
                @"..\NoxAeterna.Interpretation\NoxAeterna.Interpretation.csproj",
                @"..\NoxAeterna.Interpretation.Sqlite\NoxAeterna.Interpretation.Sqlite.csproj"
            },
            document.Descendants("ProjectReference").Select(reference => reference.Attribute("Include")?.Value).ToArray());
    }

    [Fact]
    public void GeneratedRootReportsAreIgnoredWithoutHidingCanonicalDocumentation()
    {
        var lines = File.ReadAllLines(Path.Combine(ProjectStatsTestFixture.RepositoryRoot, ".gitignore"));

        Assert.Contains("/project-stats.md", lines);
        Assert.Contains("/project-stats.json", lines);
        Assert.DoesNotContain("project-stats.md", lines);
        Assert.DoesNotContain("project-stats.json", lines);
        Assert.True(File.Exists(Path.Combine(ProjectStatsTestFixture.RepositoryRoot, "docs", "PROJECT-STATS.md")));
    }
}
