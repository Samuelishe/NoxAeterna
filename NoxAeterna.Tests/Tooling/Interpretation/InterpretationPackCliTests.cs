using System.Text.Json;
using NoxAeterna.Tools.Repository.Cli;
using NoxAeterna.Tools.Repository.Interpretation.Analysis;
using NoxAeterna.Tools.Repository.Interpretation.Cli;
using NoxAeterna.Tools.Repository.Interpretation.Reports;

namespace NoxAeterna.Tests.Tooling.Interpretation;

public sealed class InterpretationPackCliTests
{
    [Fact]
    public void HelpListsSourceCompilerInspectionStatusAndAuditWithoutGeneratedIndexes()
    {
        var top = Run(["--help"]);
        var pack = Run(["interpretation-pack", "--help"]);

        Assert.Equal(0, top.ExitCode);
        Assert.Contains("interpretation-pack", top.Output, StringComparison.Ordinal);
        Assert.Contains("validate-source", pack.Output, StringComparison.Ordinal);
        Assert.Contains("compile", pack.Output, StringComparison.Ordinal);
        Assert.Contains("inspect-package", pack.Output, StringComparison.Ordinal);
        Assert.Contains("authoring-status", pack.Output, StringComparison.Ordinal);
        Assert.Contains("audit-content", pack.Output, StringComparison.Ordinal);
        Assert.Contains("--locale LOCALE --corpus CORPUS", pack.Output, StringComparison.Ordinal);
        Assert.Contains("single-card", pack.Output, StringComparison.Ordinal);
        Assert.Contains("oriented-pairs", pack.Output, StringComparison.Ordinal);
        Assert.Contains("three-card-positions", pack.Output, StringComparison.Ordinal);
        Assert.DoesNotContain("generate-indexes", pack.Output, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("authoring-status", "single-card", InterpretationAuthoringCorpus.SingleCard)]
    [InlineData("authoring-status", "oriented-pairs", InterpretationAuthoringCorpus.OrientedPairs)]
    [InlineData("audit-content", "three-card-positions", InterpretationAuthoringCorpus.ThreeCardPositions)]
    public void ScopedCommandsParseLocaleCorpusAndJson(
        string command,
        string corpus,
        InterpretationAuthoringCorpus expectedCorpus)
    {
        var parsed = InterpretationPackCliParser.Parse(
            ["interpretation-pack", command, "--source-root", "source", "--locale", "ru", "--corpus", corpus, "--json"]);

        Assert.True(parsed.Succeeded, parsed.Error);
        Assert.Equal("source", parsed.Options!.SourceRoot);
        Assert.Equal("ru", parsed.Options.Locale);
        Assert.Equal(expectedCorpus, parsed.Options.Corpus);
        Assert.True(parsed.Options.Json);
    }

    [Theory]
    [InlineData("interpretation-pack", "audit-content", "--source-root", "source", "--locale", "ru")]
    [InlineData("interpretation-pack", "audit-content", "--source-root", "source", "--corpus", "single-card")]
    [InlineData("interpretation-pack", "authoring-status", "--source-root", "source", "--locale", "ru")]
    [InlineData("interpretation-pack", "authoring-status", "--source-root", "source", "--corpus", "single-card")]
    [InlineData("interpretation-pack", "audit-content", "--source-root", "source", "--locale", "ru", "--locale", "en", "--corpus", "single-card")]
    [InlineData("interpretation-pack", "audit-content", "--source-root", "source", "--locale", "ru", "--corpus", "single-card", "--corpus", "oriented-pairs")]
    [InlineData("interpretation-pack", "audit-content", "--source-root", "source", "--locale", "ru", "--corpus", "unknown")]
    [InlineData("interpretation-pack", "audit-content", "--locale", "ru", "--corpus", "single-card")]
    public void MissingRepeatedOrUnknownScopeOptionsReturnExitTwo(params string[] arguments)
    {
        var result = Run(arguments);

        Assert.Equal(2, result.ExitCode);
        Assert.Contains("Error:", result.Error, StringComparison.Ordinal);
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
            Assert.Equal(0, Run(["interpretation-pack", "authoring-status", "--source-root", fixture.Root, "--locale", "ru", "--corpus", "single-card"]).ExitCode);
            Assert.Equal(0, Run(["interpretation-pack", "audit-content", "--source-root", fixture.Root, "--locale", "ru", "--corpus", "single-card"]).ExitCode);
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

    [Fact]
    public void ScopedStatusJsonContainsCompleteDeterministicMissingInventory()
    {
        using var fixture = InterpretationToolingFixture.CreateSkeleton();
        var arguments = new[]
        {
            "interpretation-pack", "authoring-status", "--source-root", fixture.Root,
            "--locale", "ru", "--corpus", "single-card", "--json"
        };

        var first = Run(arguments);
        var second = Run(arguments);

        Assert.Equal(0, first.ExitCode);
        Assert.Equal(first.Output, second.Output);
        using var json = JsonDocument.Parse(first.Output);
        Assert.Equal(78, json.RootElement.GetProperty("counts").GetProperty("expectedBundles").GetInt32());
        Assert.Equal(156, json.RootElement.GetProperty("counts").GetProperty("expectedStates").GetInt32());
        Assert.Equal(78, json.RootElement.GetProperty("inventories").GetProperty("missingIdentities").GetArrayLength());
        Assert.DoesNotContain(fixture.Root, first.Output, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("timestamp", first.Output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AuditWarningsReturnZeroAndStableCompleteJsonWithoutMachineData()
    {
        using var fixture = InterpretationToolingFixture.CreateSkeleton();
        var arguments = new[]
        {
            "interpretation-pack", "audit-content", "--source-root", fixture.Root,
            "--locale", "ru", "--corpus", "single-card", "--json"
        };

        var first = Run(arguments);
        var second = Run(arguments);

        Assert.Equal(0, first.ExitCode);
        Assert.Equal(first.Output, second.Output);
        using var json = JsonDocument.Parse(first.Output);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.True(json.RootElement.GetProperty("warnings").GetInt32() > 0);
        Assert.Equal("audit.empty-corpus", json.RootElement.GetProperty("diagnostics")[0].GetProperty("code").GetString());
        Assert.Equal(0, json.RootElement.GetProperty("counts").GetProperty("textUnits").GetInt32());
        Assert.DoesNotContain(fixture.Root, first.Output, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("timestamp", first.Output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AuditUnknownLocaleReturnsOneAndParseFailureReturnsTwo()
    {
        using var fixture = InterpretationToolingFixture.CreateSkeleton();

        var unknownLocale = Run([
            "interpretation-pack", "audit-content", "--source-root", fixture.Root,
            "--locale", "de", "--corpus", "single-card", "--json"]);
        var parseFailure = Run([
            "interpretation-pack", "audit-content", "--source-root", fixture.Root,
            "--locale", "ru", "--corpus", "not-a-corpus"]);

        Assert.Equal(1, unknownLocale.ExitCode);
        Assert.Contains("audit.locale-unknown", unknownLocale.Output, StringComparison.Ordinal);
        Assert.Equal(2, parseFailure.ExitCode);
        Assert.Contains("Unknown corpus", parseFailure.Error, StringComparison.Ordinal);
    }

    [Fact]
    public void ScopedStatusUnknownLocaleReturnsOne()
    {
        using var fixture = InterpretationToolingFixture.CreateSkeleton();

        var result = Run([
            "interpretation-pack", "authoring-status", "--source-root", fixture.Root,
            "--locale", "de", "--corpus", "single-card", "--json"]);

        Assert.Equal(1, result.ExitCode);
        Assert.Contains("authoring.locale-unknown", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void AuditStructuralFailureReturnsOneWithoutHeuristicConclusions()
    {
        using var fixture = InterpretationToolingFixture.CreateSkeleton();
        File.WriteAllText(fixture.ManifestPath, "{");

        var result = Run([
            "interpretation-pack", "audit-content", "--source-root", fixture.Root,
            "--locale", "ru", "--corpus", "single-card", "--json"]);

        Assert.Equal(1, result.ExitCode);
        Assert.Contains("source.json", result.Output, StringComparison.Ordinal);
        Assert.DoesNotContain("audit.text.", result.Output, StringComparison.Ordinal);
        Assert.DoesNotContain("audit.single.", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void AuditConsoleBoundsDiagnosticsAndReportsOmittedCount()
    {
        var report = new InterpretationToolReport(Enumerable.Range(0, 25).Select(index =>
            new InterpretationToolDiagnostic(
                $"audit.test.{index:D2}",
                InterpretationToolSeverity.Warning,
                $"ru/single-card/card-{index:D2}/upright:advice",
                $"Diagnostic {index:D2}.")));

        var console = InterpretationToolReportWriter.WriteConsole(report);

        Assert.Contains("diagnostics omitted: 5", console, StringComparison.Ordinal);
        Assert.Contains("audit.test.19", console, StringComparison.Ordinal);
        Assert.DoesNotContain("audit.test.20", console, StringComparison.Ordinal);
        Assert.DoesNotContain("audit.test.24", console, StringComparison.Ordinal);
        Assert.Equal(22, console.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).Length);
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
