using System.Text.Json;
using System.Xml.Linq;

namespace NoxAeterna.Tests.Tooling;

public sealed class CoverageAndCiContractTests
{
    private static string CoverageScriptPath => Path.Combine(
        ToolingTestSupport.RepositoryRoot,
        "eng",
        "coverage.ps1");

    [Fact]
    public void CoverletCollectorIsPrivateTestDependency()
    {
        var projectPath = Path.Combine(
            ToolingTestSupport.RepositoryRoot,
            "NoxAeterna.Tests",
            "NoxAeterna.Tests.csproj");
        var document = XDocument.Load(projectPath);
        var reference = Assert.Single(
            document.Descendants("PackageReference"),
            element => (string?)element.Attribute("Include") == "coverlet.collector");

        Assert.Equal("10.0.1", (string?)reference.Attribute("Version"));
        Assert.Equal("all", reference.Element("PrivateAssets")?.Value);
        Assert.Contains("buildtransitive", reference.Element("IncludeAssets")?.Value);
    }

    [Fact]
    public void CoverageDryRunHasStructuredCollectorArgumentsAndIgnoredOutput()
    {
        var result = ToolingTestSupport.RunPowerShell(
            CoverageScriptPath,
            ToolingTestSupport.RepositoryRoot,
            "-DryRun",
            "-Json",
            "-NoBuild");

        using var document = ToolingTestSupport.ParseJson(result);
        var root = document.RootElement;
        Assert.True(root.GetProperty("dryRun").GetBoolean());
        Assert.StartsWith(
            "TestResults/Coverage/",
            root.GetProperty("outputDirectory").GetString(),
            StringComparison.Ordinal);
        var arguments = root.GetProperty("arguments")
            .EnumerateArray()
            .Select(item => item.GetString())
            .ToArray();
        Assert.Contains("--no-build", arguments);
        Assert.Contains("XPlat Code Coverage", arguments);
        Assert.Contains("trx;LogFileName=coverage.trx", arguments);

        var script = File.ReadAllText(CoverageScriptPath);
        Assert.DoesNotContain("Remove-Item", script, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("/TestResults/", File.ReadAllText(
            Path.Combine(ToolingTestSupport.RepositoryRoot, ".gitignore")));
    }

    [Fact]
    public void CiWorkflowOwnsCrossPlatformReadOnlyMilestoneAndCoverageContracts()
    {
        var workflowPath = Path.Combine(
            ToolingTestSupport.RepositoryRoot,
            ".github",
            "workflows",
            "ci.yml");
        var source = File.ReadAllText(workflowPath);

        Assert.Contains("contents: read", source, StringComparison.Ordinal);
        Assert.Contains("windows-latest", source, StringComparison.Ordinal);
        Assert.Contains("ubuntu-latest", source, StringComparison.Ordinal);
        Assert.Contains("macos-latest", source, StringComparison.Ordinal);
        Assert.Contains("actions/checkout@v7", source, StringComparison.Ordinal);
        Assert.Contains("actions/setup-dotnet@v6", source, StringComparison.Ordinal);
        Assert.Contains("actions/upload-artifact@v7", source, StringComparison.Ordinal);
        Assert.Contains("pwsh eng/doc-check.ps1", source, StringComparison.Ordinal);
        Assert.Contains("run Full -Configuration Release -NoBuild -AllowMilestone", source, StringComparison.Ordinal);
        Assert.Contains("pwsh eng/coverage.ps1 -Configuration Release -NoBuild", source, StringComparison.Ordinal);
        Assert.DoesNotContain("UI-SMOKE", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("secrets.", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("deploy", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("contents: write", source, StringComparison.OrdinalIgnoreCase);
    }
}
