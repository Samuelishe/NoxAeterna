using NoxAeterna.Tools.Repository.Cli;

namespace NoxAeterna.Tests.ProjectStats;

public sealed class ProjectStatsCliParserTests
{
    [Fact]
    public void StatsDefaultsToConsoleAndCurrentRepository()
    {
        var result = ProjectStatsCliParser.Parse(["stats"]);

        Assert.True(result.Succeeded);
        Assert.Null(result.Options!.Root);
        Assert.Equal(ProjectStatsCliParser.DefaultTop, result.Options.Top);
        Assert.Equal(ProjectStatsOutputFormat.Console, result.Options.Format);
    }

    [Theory]
    [InlineData("--json", ProjectStatsOutputFormat.Json)]
    [InlineData("--markdown", ProjectStatsOutputFormat.Markdown)]
    public void FormatSwitchSelectsRequestedWriter(string argument, ProjectStatsOutputFormat expected)
    {
        var result = ProjectStatsCliParser.Parse(["stats", ".", argument]);

        Assert.True(result.Succeeded);
        Assert.Equal(expected, result.Options!.Format);
        Assert.Equal(".", result.Options.Root);
    }

    [Fact]
    public void OutputAndTopAreParsedTogether()
    {
        var result = ProjectStatsCliParser.Parse(["stats", ".", "--top", "25", "--output", "report.md"]);

        Assert.True(result.Succeeded);
        Assert.Equal(25, result.Options!.Top);
        Assert.Equal("report.md", result.Options.OutputPath);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("101")]
    [InlineData("nope")]
    public void InvalidTopIsRejected(string value)
    {
        var result = ProjectStatsCliParser.Parse(["stats", "--top", value]);

        Assert.False(result.Succeeded);
        Assert.Contains("1 through 100", result.Error, StringComparison.Ordinal);
    }

    [Fact]
    public void ConflictingFormatsAreRejected()
    {
        var result = ProjectStatsCliParser.Parse(["stats", "--json", "--markdown"]);

        Assert.False(result.Succeeded);
        Assert.Contains("cannot be used together", result.Error, StringComparison.Ordinal);
    }

    [Fact]
    public void UnknownArgumentIsRejected()
    {
        var result = ProjectStatsCliParser.Parse(["stats", "--filter", "anything"]);

        Assert.False(result.Succeeded);
        Assert.Contains("Unknown argument", result.Error, StringComparison.Ordinal);
    }

    [Fact]
    public void HelpDoesNotRequireCommand()
    {
        var result = ProjectStatsCliParser.Parse(["--help"]);

        Assert.True(result.Succeeded);
        Assert.True(result.Options!.ShowHelp);
    }

    [Fact]
    public void MissingCommandIsControlledError()
    {
        var result = ProjectStatsCliParser.Parse([]);

        Assert.False(result.Succeeded);
        Assert.Contains("Missing command", result.Error, StringComparison.Ordinal);
    }
}
