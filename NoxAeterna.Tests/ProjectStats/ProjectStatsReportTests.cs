using System.Text.Json;
using NoxAeterna.Tools.Repository.Cli;
using NoxAeterna.Tools.Repository.Stats;

namespace NoxAeterna.Tests.ProjectStats;

public sealed class ProjectStatsReportTests
{
    [Fact]
    public void ReportBuildsProjectGraphTestTopologyAndDocumentationSnapshot()
    {
        using var fixture = ProjectStatsTestFixture.Create();
        fixture.AddMinimalReportRepository();

        var report = new ProjectStatsAnalyzer().Analyze(fixture.Root, top: 5);

        Assert.Equal(2, report.Repository.ProjectCount);
        Assert.Equal(1, report.Tests.TestFileCount);
        Assert.Equal(1, report.Tests.TestClassCount);
        Assert.Equal(1, report.Tests.LexicalFactCount);
        Assert.Equal(1, report.Tests.LexicalTheoryCount);
        Assert.Contains("not guaranteed runtime", report.Tests.CountSemantics, StringComparison.Ordinal);
        Assert.Single(report.ProjectGraph, edge =>
            edge.From == "NoxAeterna.Tests/NoxAeterna.Tests.csproj" &&
            edge.To == "NoxAeterna.Domain/NoxAeterna.Domain.csproj");
        var budget = Assert.Single(report.DocumentationBudgets);
        Assert.Equal("AGENTS.md", budget.Path);
        Assert.Equal(50, budget.SoftThreshold);
        Assert.Equal("manual-reconcile", budget.OverflowStrategy);
    }

    [Fact]
    public void TheoryCountIsLexicalAndNotPresentedAsRuntimeCaseTotal()
    {
        using var fixture = ProjectStatsTestFixture.Create();
        fixture.AddMinimalReportRepository();

        var report = new ProjectStatsAnalyzer().Analyze(fixture.Root, top: 5);

        Assert.Equal(1, report.Tests.LexicalTheoryCount);
        Assert.DoesNotContain("runtime test cases: 1", report.Tests.CountSemantics, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void MalformedBudgetManifestProducesDiagnosticWithoutFailingReport()
    {
        using var fixture = ProjectStatsTestFixture.Create();
        fixture.AddMinimalReportRepository();
        fixture.Write("eng/document-budgets.json", "{ broken", tracked: true);

        var report = new ProjectStatsAnalyzer().Analyze(fixture.Root, top: 5);

        Assert.Empty(report.DocumentationBudgets);
        Assert.Contains(report.Diagnostics, diagnostic => diagnostic.Code == "documentation-budget-malformed");
        Assert.True(report.Repository.TotalPublicFiles > 0);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void InvalidBudgetWarningRatioProducesControlledDiagnostic(double warningRatio)
    {
        using var fixture = ProjectStatsTestFixture.Create();
        fixture.AddMinimalReportRepository();
        fixture.Write(
            "eng/document-budgets.json",
            $$"""{"schemaVersion":1,"warningRatio":{{warningRatio}},"documents":[{"path":"AGENTS.md","hardLimit":100,"overflowStrategy":"manual-reconcile"}]}""",
            tracked: true);

        var report = new ProjectStatsAnalyzer().Analyze(fixture.Root, top: 5);

        Assert.Empty(report.DocumentationBudgets);
        Assert.Contains(report.Diagnostics, diagnostic => diagnostic.Code == "documentation-budget-malformed");
    }

    [Fact]
    public void MissingBudgetOwnerDocumentIsNotSilentlyReportedAsARealZeroMeasurement()
    {
        using var fixture = ProjectStatsTestFixture.Create();
        fixture.AddMinimalReportRepository();
        fixture.Write(
            "eng/document-budgets.json",
            "{\"schemaVersion\":1,\"warningRatio\":0.5,\"documents\":[{\"path\":\"docs/MISSING.md\",\"hardLimit\":100,\"overflowStrategy\":\"manual-reconcile\"}]}",
            tracked: true);

        var report = new ProjectStatsAnalyzer().Analyze(fixture.Root, top: 5);

        var budget = Assert.Single(report.DocumentationBudgets);
        Assert.Equal("docs/MISSING.md", budget.Path);
        Assert.Contains(
            report.Diagnostics,
            diagnostic => diagnostic.Code == "documentation-budget-document-missing" &&
                          diagnostic.Path == "docs/MISSING.md");
    }

    [Fact]
    public void JsonWriterIsParseableDeterministicAndContainsNoAbsoluteRoot()
    {
        using var fixture = ProjectStatsTestFixture.Create();
        fixture.AddMinimalReportRepository();
        var report = new ProjectStatsAnalyzer().Analyze(fixture.Root, top: 5);

        var first = ProjectStatsWriters.WriteJson(report);
        var second = ProjectStatsWriters.WriteJson(report);

        Assert.Equal(first, second);
        Assert.DoesNotContain(fixture.Root, first, StringComparison.OrdinalIgnoreCase);
        using var document = JsonDocument.Parse(first);
        Assert.Equal(1, document.RootElement.GetProperty("schemaVersion").GetInt32());
        Assert.True(document.RootElement.TryGetProperty("largestFiles", out _));
        Assert.True(document.RootElement.TryGetProperty("folderDensity", out _));
    }

    [Fact]
    public void MarkdownWriterUsesTablesRelativePathsAndDiagnosticDisclaimer()
    {
        using var fixture = ProjectStatsTestFixture.Create();
        fixture.AddMinimalReportRepository();
        var report = new ProjectStatsAnalyzer().Analyze(fixture.Root, top: 5);

        var markdown = ProjectStatsWriters.WriteMarkdown(report);

        Assert.Contains("| Project | Frameworks |", markdown, StringComparison.Ordinal);
        Assert.Contains("Size rankings are signals, not quality verdicts", markdown, StringComparison.Ordinal);
        Assert.Contains("NoxAeterna.Domain", markdown, StringComparison.Ordinal);
        Assert.DoesNotContain(fixture.Root, markdown, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ConsoleWriterIsBoundedByPreparedRankings()
    {
        using var fixture = ProjectStatsTestFixture.Create();
        fixture.AddMinimalReportRepository();
        var report = new ProjectStatsAnalyzer().Analyze(fixture.Root, top: 1);

        var console = ProjectStatsWriters.WriteConsole(report, top: 1);

        Assert.Contains("Nox Aeterna Project Stats", console, StringComparison.Ordinal);
        Assert.True(console.Split('\n').Length < 40, console);
        Assert.DoesNotContain(fixture.Root, console, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void OutputTargetIsExcludedAndJsonStdoutHasNoContamination()
    {
        using var fixture = ProjectStatsTestFixture.Create();
        fixture.AddMinimalReportRepository();
        fixture.Write("report.json", "old report");

        var result = fixture.RunTool("stats", fixture.Root, "--json", "--output", "report.json");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("Project Stats written to report.json", result.Output.Trim());
        var content = File.ReadAllText(Path.Combine(fixture.Root, "report.json"));
        using var document = JsonDocument.Parse(content);
        Assert.Contains(
            document.RootElement.GetProperty("diagnostics").EnumerateArray(),
            diagnostic => diagnostic.GetProperty("code").GetString() == "output-excluded");
        Assert.DoesNotContain(fixture.Root, content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RelativeOutputPathCannotEscapeRepository()
    {
        using var fixture = ProjectStatsTestFixture.Create();
        fixture.AddMinimalReportRepository();

        var result = fixture.RunTool("stats", fixture.Root, "--markdown", "--output", "../outside.md");

        Assert.NotEqual(0, result.ExitCode);
        Assert.Contains("must remain inside", result.Error, StringComparison.Ordinal);
        Assert.DoesNotContain(" at ", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CliJsonModeWritesOnlyValidJsonToStdout()
    {
        using var fixture = ProjectStatsTestFixture.Create();
        fixture.AddMinimalReportRepository();

        var result = fixture.RunTool("stats", fixture.Root, "--json", "--top", "3");

        Assert.Equal(0, result.ExitCode);
        Assert.True(string.IsNullOrWhiteSpace(result.Error), result.Error);
        using var document = JsonDocument.Parse(result.Output);
        Assert.Equal(1, document.RootElement.GetProperty("schemaVersion").GetInt32());
    }
}
