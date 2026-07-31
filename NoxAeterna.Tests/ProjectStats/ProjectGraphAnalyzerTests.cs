using NoxAeterna.Tools.Repository.Analysis;

namespace NoxAeterna.Tests.ProjectStats;

public sealed class ProjectGraphAnalyzerTests
{
    [Fact]
    public void ValidGraphReportsFrameworksOutputPackagesSourcesAndEdges()
    {
        using var fixture = ProjectStatsTestFixture.Create();
        fixture.Write(
            "A/A.csproj",
            "<Project><PropertyGroup><TargetFrameworks>net9.0;net10.0</TargetFrameworks><OutputType>Exe</OutputType></PropertyGroup><ItemGroup><PackageReference Include=\"Example\" /><ProjectReference Include=\"../B/B.csproj\" /></ItemGroup></Project>");
        fixture.Write("A/One.cs", "line1\nline2\n");
        fixture.Write("B/B.csproj", "<Project><PropertyGroup><TargetFramework>net10.0</TargetFramework></PropertyGroup></Project>");
        fixture.Write("B/Two.cs", "line\n");
        var files = new[]
        {
            Entry("A/A.csproj"), Entry("A/One.cs", lines: 2),
            Entry("B/B.csproj"), Entry("B/Two.cs", lines: 1)
        };

        var result = new ProjectGraphAnalyzer().Analyze(fixture.Root, files);

        Assert.Empty(result.Diagnostics);
        var project = Assert.Single(result.Projects, item => item.Name == "A");
        Assert.Equal(new[] { "net10.0", "net9.0" }, project.TargetFrameworks);
        Assert.Equal("Exe", project.OutputType);
        Assert.Equal(1, project.PackageReferenceCount);
        Assert.Equal(1, project.SourceFileCount);
        Assert.Equal(2, project.SourceLines);
        Assert.Contains(result.Edges, edge => edge.From == "A/A.csproj" && edge.To == "B/B.csproj");
    }

    [Fact]
    public void MissingProjectReferenceProducesDiagnosticAndReportSurvives()
    {
        using var fixture = ProjectStatsTestFixture.Create();
        fixture.Write("A/A.csproj", "<Project><ItemGroup><ProjectReference Include=\"../Missing/Missing.csproj\" /></ItemGroup></Project>");

        var result = new ProjectGraphAnalyzer().Analyze(fixture.Root, [Entry("A/A.csproj")]);

        Assert.Single(result.Projects);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "project-reference-missing");
    }

    [Fact]
    public void SelfReferenceProducesDiagnosticWithoutGraphEdge()
    {
        using var fixture = ProjectStatsTestFixture.Create();
        fixture.Write("A/A.csproj", "<Project><ItemGroup><ProjectReference Include=\"A.csproj\" /></ItemGroup></Project>");

        var result = new ProjectGraphAnalyzer().Analyze(fixture.Root, [Entry("A/A.csproj")]);

        Assert.Empty(result.Edges);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "project-self-reference");
    }

    [Fact]
    public void ProjectCycleProducesDiagnosticAndRetainsEdges()
    {
        using var fixture = ProjectStatsTestFixture.Create();
        fixture.Write("A/A.csproj", "<Project><ItemGroup><ProjectReference Include=\"../B/B.csproj\" /></ItemGroup></Project>");
        fixture.Write("B/B.csproj", "<Project><ItemGroup><ProjectReference Include=\"../A/A.csproj\" /></ItemGroup></Project>");

        var result = new ProjectGraphAnalyzer().Analyze(
            fixture.Root,
            [Entry("A/A.csproj"), Entry("B/B.csproj")]);

        Assert.Equal(2, result.Edges.Count);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "project-cycle");
    }

    [Fact]
    public void MalformedProjectProducesDiagnosticAndOtherProjectsRemain()
    {
        using var fixture = ProjectStatsTestFixture.Create();
        fixture.Write("Bad/Bad.csproj", "<Project>");
        fixture.Write("Good/Good.csproj", "<Project><PropertyGroup><TargetFramework>net10.0</TargetFramework></PropertyGroup></Project>");

        var result = new ProjectGraphAnalyzer().Analyze(
            fixture.Root,
            [Entry("Bad/Bad.csproj"), Entry("Good/Good.csproj")]);

        Assert.Single(result.Projects, project => project.Name == "Good");
        Assert.Contains(result.Diagnostics, diagnostic =>
            diagnostic.Code == "project-malformed" && diagnostic.Path == "Bad/Bad.csproj");
    }

    private static RepositoryFileEntry Entry(string path, int lines = 1) => new(
        path,
        Path.GetExtension(path),
        1,
        lines,
        1,
        RepositoryPathPolicy.Classify(path),
        RepositoryPathPolicy.GetProjectArea(path),
        true,
        true,
        false);
}
