using System.Text.Json;
using NoxAeterna.Tools.Repository.Cli;
using NoxAeterna.Tools.Repository.Context.Cli;

namespace NoxAeterna.Tests.AgentContext;

public sealed class ContextCliTests
{
    [Fact]
    public void StatsParserAndDispatcherRemainAvailable()
    {
        var parsed = ProjectStatsCliParser.Parse(["stats", ".", "--top", "5"]);
        using var output = new StringWriter();
        using var error = new StringWriter();
        var exitCode = RepositoryCommandDispatcher.Run(["--help"], output, error);

        Assert.True(parsed.Succeeded);
        Assert.Equal(0, exitCode);
        Assert.Contains("stats", output.ToString(), StringComparison.Ordinal);
        Assert.Contains("context-plan", output.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void PlanParserAcceptsRepeatedPathsAndCompactJson()
    {
        var result = ContextPlanCliParser.Parse([
            "context-plan", "--task", "CodeChange", "--path", "one.cs", "--path", "two.cs",
            "--budget-chars", "123", "--compact-json"]);

        Assert.True(result.Succeeded);
        Assert.Equal(["one.cs", "two.cs"], result.Options!.Paths);
        Assert.True(result.Options.CompactJson);
    }

    [Theory]
    [InlineData("missing-task")]
    [InlineData("missing-path")]
    [InlineData("missing-budget")]
    [InlineData("conflicting-json")]
    [InlineData("unknown")]
    public void PlanParserRejectsIncompleteOrUnknownInput(string scenario)
    {
        string[] arguments = scenario switch
        {
            "missing-task" => ["context-plan", "--path", "a", "--budget-chars", "5"],
            "missing-path" => ["context-plan", "--task", "CodeChange", "--budget-chars", "5"],
            "missing-budget" => ["context-plan", "--task", "CodeChange", "--path", "a"],
            "conflicting-json" => ["context-plan", "--task", "CodeChange", "--path", "a", "--budget-chars", "5", "--json", "--compact-json"],
            _ => ["context-plan", "--unknown"]
        };

        var result = ContextPlanCliParser.Parse(arguments);

        Assert.False(result.Succeeded);
        Assert.False(string.IsNullOrWhiteSpace(result.Error));
    }

    [Fact]
    public void RealPlanJsonIsParseableAndStdoutContainsOnlyJson()
    {
        using var output = new StringWriter();
        using var error = new StringWriter();
        var exitCode = ContextPlanCli.Run([
            "context-plan", "--task", "CodeChange", "--path", "NoxAeterna.Domain/Astrology/NatalChart.cs",
            "--budget-chars", "70000", "--root", AgentContextTestSupport.Root, "--json"], output, error);

        Assert.Equal(0, exitCode);
        using var document = JsonDocument.Parse(output.ToString());
        Assert.Equal("CodeChange", document.RootElement.GetProperty("taskKind").GetString());
        Assert.Equal(string.Empty, error.ToString());
    }
}
