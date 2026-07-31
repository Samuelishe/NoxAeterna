using NoxAeterna.Tools.Repository.Analysis;
using NoxAeterna.Tools.Repository.Cli;
using NoxAeterna.Tools.Repository.Stats;

namespace NoxAeterna.Tests.ProjectStats;

public sealed class CurrentRepositoryProjectStatsTests
{
    [Fact]
    public void CurrentRepositoryReportDiscoversExpectedProjectsWithoutUnsafeRankings()
    {
        var root = GitRepositoryInventory.ResolveRoot(ProjectStatsTestFixture.RepositoryRoot);

        var report = new ProjectStatsAnalyzer().Analyze(root, top: 5);

        Assert.Contains(report.Projects, project => project.Name == "NoxAeterna.App");
        Assert.Contains(report.Projects, project => project.Name == "NoxAeterna.Tests");
        Assert.Contains(report.Projects, project => project.Name == "NoxAeterna.Tools.Repository");
        Assert.All(report.Projects.SelectMany(static project => project.ProjectReferences), reference =>
            Assert.Contains(report.Projects, project => project.Path == reference));
        Assert.DoesNotContain(AllRankedPaths(report), RepositoryPathPolicy.IsGenerated);
        Assert.DoesNotContain(AllRankedPaths(report), RepositoryPathPolicy.IsPrivateOrSensitive);
        Assert.DoesNotContain(report.Diagnostics, diagnostic =>
            diagnostic.Code is "project-reference-missing" or "project-cycle" or "privacy-excluded");
    }

    [Fact]
    public void CurrentRepositoryReportIsDeterministicAndUsesRelativePaths()
    {
        var root = GitRepositoryInventory.ResolveRoot(ProjectStatsTestFixture.RepositoryRoot);
        var analyzer = new ProjectStatsAnalyzer();

        var first = ProjectStatsWriters.WriteJson(analyzer.Analyze(root, top: 5));
        var second = ProjectStatsWriters.WriteJson(analyzer.Analyze(root, top: 5));

        Assert.Equal(first, second);
        Assert.DoesNotContain(root, first, StringComparison.OrdinalIgnoreCase);
    }

    private static IEnumerable<string> AllRankedPaths(ProjectStatsReport report) =>
        report.LargestFiles.ProductionCSharp
            .Concat(report.LargestFiles.TestCSharp)
            .Concat(report.LargestFiles.ToolingCSharp)
            .Concat(report.LargestFiles.Xaml)
            .Concat(report.LargestFiles.Markdown)
            .Concat(report.LargestFiles.PowerShell)
            .Concat(report.LargestFiles.JsonConfiguration)
            .Select(static file => file.Path);
}
