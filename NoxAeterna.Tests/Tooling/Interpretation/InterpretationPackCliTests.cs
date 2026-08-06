using System.Text.Json;
using NoxAeterna.Tools.Repository.Cli;

namespace NoxAeterna.Tests.Tooling.Interpretation;

public sealed class InterpretationPackCliTests
{
    [Fact]
    public void TopHelpListsInterpretationPackAndFocusedHelpListsEverySubcommand()
    {
        var top = Run(["--help"]);
        var pack = Run(["interpretation-pack", "--help"]);
        var validate = Run(["interpretation-pack", "validate", "--help"]);
        var generate = Run(["interpretation-pack", "generate-indexes", "--help"]);
        var authoring = Run(["interpretation-pack", "authoring-status", "--help"]);

        Assert.Equal(0, top.ExitCode);
        Assert.Contains("interpretation-pack", top.Output, StringComparison.Ordinal);
        Assert.Contains("validate", pack.Output, StringComparison.Ordinal);
        Assert.Contains("generate-indexes", pack.Output, StringComparison.Ordinal);
        Assert.Contains("authoring-status", pack.Output, StringComparison.Ordinal);
        Assert.Contains("without writing", validate.Output, StringComparison.Ordinal);
        Assert.Contains("check drift", generate.Output, StringComparison.Ordinal);
        Assert.Contains("without promotion", authoring.Output, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("unknown")]
    [InlineData("missing-root")]
    [InlineData("invalid-option")]
    public void InvalidCliInputReturnsExitTwo(string scenario)
    {
        string[] arguments = scenario switch
        {
            "unknown" => ["interpretation-pack", "unknown"],
            "missing-root" => ["interpretation-pack", "validate"],
            _ => ["interpretation-pack", "validate", "--pack-root", ".", "--unknown"]
        };

        var result = Run(arguments);

        Assert.Equal(2, result.ExitCode);
        Assert.Contains("Error:", result.Error, StringComparison.Ordinal);
    }

    [Fact]
    public void ValidNotReadyPackAndAuthoringInventoryReturnExitZero()
    {
        using var fixture = InterpretationToolingFixture.CreateSkeleton();

        var validation = Run(["interpretation-pack", "validate", "--pack-root", fixture.Root]);
        var authoring = Run([
            "interpretation-pack", "authoring-status", "--working-root",
            InterpretationToolingFixture.WorkingSkeletonRoot]);

        Assert.Equal(0, validation.ExitCode);
        Assert.Contains("Summary: success", validation.Output, StringComparison.Ordinal);
        Assert.Equal(0, authoring.ExitCode);
        Assert.Contains("Summary: success", authoring.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void InvalidPackReturnsExitOneAndMissingRootReturnsExitTwo()
    {
        using var fixture = InterpretationToolingFixture.CreateSkeleton();
        File.WriteAllText(fixture.ManifestPath, "{");

        var invalid = Run(["interpretation-pack", "validate", "--pack-root", fixture.Root]);
        var missing = Run([
            "interpretation-pack", "validate", "--pack-root",
            Path.Combine(fixture.Root, "missing")]);

        Assert.Equal(1, invalid.ExitCode);
        Assert.Contains("pack.manifest-json", invalid.Output, StringComparison.Ordinal);
        Assert.Equal(2, missing.ExitCode);
        Assert.Contains("does not exist", missing.Error, StringComparison.Ordinal);
    }

    [Fact]
    public void JsonOutputIsValidDeterministicAndContainsNoTimestampOrRandomId()
    {
        using var fixture = InterpretationToolingFixture.CreateSkeleton();

        var first = Run(["interpretation-pack", "validate", "--pack-root", fixture.Root, "--json"]);
        var second = Run(["interpretation-pack", "validate", "--pack-root", fixture.Root, "--json"]);

        Assert.Equal(0, first.ExitCode);
        Assert.Equal(first.Output, second.Output);
        using var json = JsonDocument.Parse(first.Output);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.DoesNotContain("timestamp", first.Output, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("guid", first.Output, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(fixture.Root, first.Output, StringComparison.OrdinalIgnoreCase);
    }

    private static CliResult Run(IReadOnlyList<string> arguments)
    {
        using var output = new StringWriter();
        using var error = new StringWriter();
        var exitCode = RepositoryCommandDispatcher.Run(arguments, output, error);
        return new(exitCode, output.ToString(), error.ToString());
    }

    private sealed record CliResult(int ExitCode, string Output, string Error);
}
