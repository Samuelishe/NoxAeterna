using System.Text.Json;

namespace NoxAeterna.Tests.Tooling;

public sealed class RepositoryBaselineScriptTests
{
    private static string ScriptPath => Path.Combine(
        ToolingTestSupport.RepositoryRoot,
        "eng",
        "repo-baseline.ps1");

    [Fact]
    public void JsonReportContainsRequiredRepositoryState()
    {
        var result = ToolingTestSupport.RunPowerShell(
            ScriptPath,
            ToolingTestSupport.RepositoryRoot,
            "-Json");
        using var document = ToolingTestSupport.ParseJson(result);
        var root = document.RootElement;

        Assert.Equal(1, root.GetProperty("schemaVersion").GetInt32());
        Assert.Matches("^[0-9a-f]{40}$", root.GetProperty("headSha").GetString());
        Assert.False(string.IsNullOrWhiteSpace(root.GetProperty("branch").GetString()));
        Assert.Contains(
            root.GetProperty("worktreeClean").ValueKind,
            new[] { JsonValueKind.True, JsonValueKind.False });
        Assert.Equal(JsonValueKind.Array, root.GetProperty("statusEntries").ValueKind);
        Assert.Equal(JsonValueKind.Array, root.GetProperty("activeGitOperations").ValueKind);
        Assert.Equal(JsonValueKind.Array, root.GetProperty("repositoryLocalDotNetProcesses").ValueKind);
    }

    [Fact]
    public void ScriptDoesNotChangeHeadOrWorktreeStatus()
    {
        var headBefore = ToolingTestSupport.RunGit("rev-parse", "HEAD");
        var statusBefore = ToolingTestSupport.RunGit("status", "--short");

        var result = ToolingTestSupport.RunPowerShell(
            ScriptPath,
            ToolingTestSupport.RepositoryRoot,
            "-Json");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(headBefore, ToolingTestSupport.RunGit("rev-parse", "HEAD"));
        Assert.Equal(statusBefore, ToolingTestSupport.RunGit("status", "--short"));
    }

    [Fact]
    public void ScriptContainsNoMutationOrProcessTerminationCommands()
    {
        var source = File.ReadAllText(ScriptPath);

        Assert.DoesNotContain("Stop-Process", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("taskkill", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("git reset", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("git clean", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("git checkout", source, StringComparison.OrdinalIgnoreCase);
    }
}
