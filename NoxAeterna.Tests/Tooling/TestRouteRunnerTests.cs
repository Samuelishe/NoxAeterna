using System.Text.Json;

namespace NoxAeterna.Tests.Tooling;

public sealed class TestRouteRunnerTests
{
    private static string RunnerPath => Path.Combine(
        ToolingTestSupport.RepositoryRoot,
        "eng",
        "test-route.ps1");

    [Fact]
    public void ListAndResolveJsonAreParseable()
    {
        using var list = ToolingTestSupport.ParseJson(Run("list", "-Json"));
        using var resolve = ToolingTestSupport.ParseJson(Run("resolve", "Chart-Rendering", "-Json"));

        Assert.Equal(1, list.RootElement.GetProperty("schemaVersion").GetInt32());
        Assert.Equal(
            new[] { "Geometry", "Rendering" },
            resolve.RootElement
                .GetProperty("resolvedLeaves")
                .EnumerateArray()
                .Select(item => item.GetString())
                .ToArray());
    }

    [Fact]
    public void ResolveJsonIsDeterministic()
    {
        var first = Run("resolve", "Astrology-Core", "-Json");
        var second = Run("resolve", "Astrology-Core", "-Json");

        Assert.Equal(0, first.ExitCode);
        Assert.Equal(first.StandardOutput, second.StandardOutput);
        Assert.DoesNotContain(ToolingTestSupport.RepositoryRoot, first.StandardOutput, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void DryRunDoesNotExecuteTestsOrCreateResultDirectory()
    {
        var before = EnumerateRouteResultDirectories();

        var result = Run("run", "Geometry", "-DryRun", "-Json");

        using var document = ToolingTestSupport.ParseJson(result);
        Assert.True(document.RootElement.GetProperty("dryRun").GetBoolean());
        Assert.Equal(before, EnumerateRouteResultDirectories());
    }

    [Fact]
    public void UnknownRouteFailsAndListsSupportedNames()
    {
        var result = Run("resolve", "Not-A-Route");

        Assert.NotEqual(0, result.ExitCode);
        Assert.Contains("Supported routes", result.CombinedOutput, StringComparison.Ordinal);
        Assert.Contains("Geometry", result.CombinedOutput, StringComparison.Ordinal);
    }

    [Fact]
    public void FullRequiresExplicitMilestoneAuthorization()
    {
        var result = Run("run", "Full", "-NoBuild", "-Json");

        Assert.Equal(3, result.ExitCode);
        using var document = JsonDocument.Parse(result.StandardOutput);
        Assert.False(document.RootElement.GetProperty("milestoneAuthorized").GetBoolean());
        Assert.False(document.RootElement.GetProperty("succeeded").GetBoolean());
        Assert.Empty(document.RootElement.GetProperty("children").EnumerateArray());
    }

    [Fact]
    public void RawFilterCannotBeSupplied()
    {
        var result = Run("run", "Geometry", "-DryRun", "-Filter", "anything");

        Assert.NotEqual(0, result.ExitCode);
        Assert.Contains("Filter", result.CombinedOutput, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void NoBuildAndArgumentsAreRepresentedAsSeparatePlanEntries()
    {
        using var withNoBuild = ToolingTestSupport.ParseJson(
            Run("run", "Geometry", "-DryRun", "-NoBuild", "-Json"));
        using var withoutNoBuild = ToolingTestSupport.ParseJson(
            Run("run", "Geometry", "-DryRun", "-Json"));

        var arguments = ReadArguments(withNoBuild);
        Assert.Contains("--no-build", arguments);
        Assert.DoesNotContain("--no-build", ReadArguments(withoutNoBuild));
        var filterIndex = Array.IndexOf(arguments, "--filter");
        Assert.True(filterIndex >= 0);
        Assert.Equal(
            "FullyQualifiedName~NoxAeterna.Tests.Geometry",
            arguments[filterIndex + 1]);
    }

    [Fact]
    public void LogsAreScopedAndIgnoredAndRunnerKillsOnlyItsOwnTree()
    {
        var source = File.ReadAllText(RunnerPath);
        var ignore = File.ReadAllText(Path.Combine(ToolingTestSupport.RepositoryRoot, ".gitignore"));

        Assert.Contains("TestResults/RepoRoutes/", source, StringComparison.Ordinal);
        Assert.Contains("/TestResults/", ignore, StringComparison.Ordinal);
        Assert.Contains("$process.Kill($true)", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Get-Process", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Stop-Process", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Invoke-Expression", source, StringComparison.OrdinalIgnoreCase);
    }

    private static string[] ReadArguments(JsonDocument document) =>
        document.RootElement
            .GetProperty("plan")[0]
            .GetProperty("arguments")
            .EnumerateArray()
            .Select(item => item.GetString()!)
            .ToArray();

    private static string[] EnumerateRouteResultDirectories()
    {
        var root = Path.Combine(ToolingTestSupport.RepositoryRoot, "TestResults", "RepoRoutes");
        return Directory.Exists(root)
            ? Directory.GetDirectories(root).Order(StringComparer.Ordinal).ToArray()
            : [];
    }

    private static ScriptResult Run(params string[] arguments) =>
        ToolingTestSupport.RunPowerShell(
            RunnerPath,
            ToolingTestSupport.RepositoryRoot,
            arguments);
}
