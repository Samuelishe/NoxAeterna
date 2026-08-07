using System.Text.Json;
using NoxAeterna.Tools.Repository.Cli;

namespace NoxAeterna.Tests.Tooling.Interpretation;

public sealed class InterpretationPackCliTests
{
    [Fact]
    public void HelpListsSourceCompilerInspectionAndStatusWithoutGeneratedIndexes()
    {
        var top = Run(["--help"]);
        var pack = Run(["interpretation-pack", "--help"]);

        Assert.Equal(0, top.ExitCode);
        Assert.Contains("interpretation-pack", top.Output, StringComparison.Ordinal);
        Assert.Contains("validate-source", pack.Output, StringComparison.Ordinal);
        Assert.Contains("compile", pack.Output, StringComparison.Ordinal);
        Assert.Contains("inspect-package", pack.Output, StringComparison.Ordinal);
        Assert.Contains("authoring-status", pack.Output, StringComparison.Ordinal);
        Assert.DoesNotContain("generate-indexes", pack.Output, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("unknown")]
    [InlineData("missing-source")]
    [InlineData("missing-output")]
    public void InvalidCliInputReturnsExitTwo(string scenario)
    {
        string[] arguments = scenario switch
        {
            "unknown" => ["interpretation-pack", "unknown"],
            "missing-source" => ["interpretation-pack", "validate-source"],
            _ => ["interpretation-pack", "compile", "--source-root", "."]
        };
        var result = Run(arguments);
        Assert.Equal(2, result.ExitCode);
        Assert.Contains("Error:", result.Error, StringComparison.Ordinal);
    }

    [Fact]
    public void SourceCompileInspectAndCheckCommandsSucceed()
    {
        using var fixture = InterpretationToolingFixture.CreateSkeleton();
        var package = Path.Combine(Path.GetTempPath(), $"NoxAeterna-cli-{Guid.NewGuid():N}.noxinterp");
        try
        {
            Assert.Equal(0, Run(["interpretation-pack", "validate-source", "--source-root", fixture.Root]).ExitCode);
            Assert.Equal(0, Run(["interpretation-pack", "authoring-status", "--source-root", fixture.Root]).ExitCode);
            Assert.Equal(0, Run(["interpretation-pack", "compile", "--source-root", fixture.Root, "--output", package]).ExitCode);
            Assert.Equal(0, Run(["interpretation-pack", "inspect-package", "--package", package]).ExitCode);
            Assert.Equal(0, Run(["interpretation-pack", "compile", "--source-root", fixture.Root, "--output", package, "--check"]).ExitCode);
        }
        finally
        {
            if (File.Exists(package)) File.Delete(package);
        }
    }

    [Fact]
    public void JsonValidationOutputIsDeterministicAndContainsNoMachinePathOrTimestamp()
    {
        using var fixture = InterpretationToolingFixture.CreateSkeleton();
        var first = Run(["interpretation-pack", "validate-source", "--source-root", fixture.Root, "--json"]);
        var second = Run(["interpretation-pack", "validate-source", "--source-root", fixture.Root, "--json"]);

        Assert.Equal(0, first.ExitCode);
        Assert.Equal(first.Output, second.Output);
        using var json = JsonDocument.Parse(first.Output);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.DoesNotContain("timestamp", first.Output, StringComparison.OrdinalIgnoreCase);
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
