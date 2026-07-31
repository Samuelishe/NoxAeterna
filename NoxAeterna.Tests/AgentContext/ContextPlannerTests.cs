using System.Text.Json;
using NoxAeterna.Tools.Repository.Analysis;
using NoxAeterna.Tools.Repository.Context;
using NoxAeterna.Tools.Repository.Context.Cli;

namespace NoxAeterna.Tests.AgentContext;

public sealed class ContextPlannerTests
{
    [Fact]
    public void RenderingPlanOrdersExplicitTargetFirstAndIncludesExactOwnersAndRoute()
    {
        var result = Plan("CodeChange", ["NoxAeterna.Rendering/Charts/CircularChartRenderer.cs"], 70_000);

        Assert.True(result.Succeeded);
        Assert.Equal("NoxAeterna.Rendering/Charts/CircularChartRenderer.cs", result.Plan.SelectedFiles[0].Path);
        Assert.Equal(1, result.Plan.SelectedFiles.Count(file => file.Path == "docs/PROJECT-STATE.md"));
        Assert.Contains("docs/RENDERING-ENGINE.md", result.Plan.SelectedFiles.Select(file => file.Path));
        Assert.Contains("docs/VISUAL-DESIGN-SYSTEM.md", result.Plan.SelectedFiles.Select(file => file.Path));
        Assert.Equal(["Rendering"], result.Plan.TestRoutes);
        Assert.DoesNotContain(result.Plan.SelectedFiles, file => file.Path.StartsWith("docs/archive/", StringComparison.Ordinal));
    }

    [Fact]
    public void MultipleRulesMergeDuplicateOwnersAndDeduplicateRoutes()
    {
        var result = Plan("CodeChange", [
            "NoxAeterna.Rendering/Charts/CircularChartRenderer.cs",
            "NoxAeterna.App/Astrology/AstrologyChartSurfaceControl.cs"], 120_000);

        Assert.True(result.Succeeded);
        Assert.Equal(["app-astrology-workspace", "chart-rendering"], result.Plan.MatchedRules);
        Assert.Equal(1, result.Plan.SelectedFiles.Count(file => file.Path == "docs/VISUAL-DESIGN-SYSTEM.md"));
        Assert.Equal(["App-Workspace", "Rendering"], result.Plan.TestRoutes);
    }

    [Theory]
    [InlineData("NoxAeterna.Rendering/Charts", "directory-routing-only")]
    [InlineData("NoxAeterna.Persistence/Charts/Future.cs", "planned-target")]
    public void NonFileTargetsRouteWithoutBecomingSelected(string path, string diagnostic)
    {
        var result = Plan("CodeChange", [path], 90_000);

        Assert.True(result.Succeeded);
        Assert.DoesNotContain(result.Plan.SelectedFiles, file => file.Path == path);
        Assert.Contains(result.Plan.Diagnostics, item => item.Code == diagnostic);
        Assert.NotEmpty(result.Plan.MatchedRules);
    }

    [Theory]
    [InlineData("C:/private.txt")]
    [InlineData("\\private.txt")]
    [InlineData("/private.txt")]
    [InlineData("../../private.txt")]
    [InlineData("docs/private/secret.md")]
    [InlineData("TestResults/result.trx")]
    [InlineData("context-secret.log")]
    public void UnsafeTargetIsRejectedWithoutPathDisclosure(string path)
    {
        var result = Plan("CodeChange", [path], 70_000);

        Assert.False(result.Succeeded);
        Assert.Empty(result.Plan.SelectedFiles);
        Assert.DoesNotContain(path, string.Join(' ', result.Plan.Diagnostics.Select(item => item.Message)), StringComparison.Ordinal);
    }

    [Fact]
    public void BinaryTargetIsMetadataOnlyAndConsumesNoCharacters()
    {
        var inventory = AgentContextTestSupport.Inventory();
        var fake = new RepositoryFileEntry("resources/cards/a.png", ".png", 50, null, null,
            RepositoryFileCategory.Resources, "resources", true, false, false);
        var result = new ContextPlanner().Plan(AgentContextTestSupport.Root, AgentContextTestSupport.Registry(),
            inventory.Files.Append(fake).ToArray(), "AssetChange", [fake.Path], 80_000);

        var selected = Assert.Single(result.Plan.SelectedFiles, file => file.Path == fake.Path);
        Assert.True(selected.MetadataOnly);
        Assert.Equal(0, selected.Characters);
    }

    [Fact]
    public void TooSmallBudgetReportsExactMandatoryMinimumWithoutPartialSuccess()
    {
        var result = Plan("CodeChange", ["NoxAeterna.Rendering/Charts/CircularChartRenderer.cs"], 10);

        Assert.False(result.Succeeded);
        Assert.True(result.Plan.Budget.MandatoryCharacters > 10);
        Assert.Contains(result.Plan.Diagnostics, item => item.Code == "mandatory-budget");
        Assert.Contains(result.Plan.SelectedFiles, file => file.Path == "NoxAeterna.Rendering/Charts/CircularChartRenderer.cs");
    }

    [Fact]
    public void RecommendedDocumentIsSelectedOnlyWhenWholeFileFits()
    {
        var registry = AgentContextTestSupport.Registry();
        var task = registry.TaskKinds.Single(item => item.Name == "CodeChange");
        var mutated = registry with { TaskKinds = registry.TaskKinds.Select(item => item == task
            ? item with { RecommendedDocuments = [new() { Path = "docs/ARCHITECTURE.md", Priority = 50 }] }
            : item).ToArray() };
        var inventory = AgentContextTestSupport.Inventory();
        var baseline = new ContextPlanner().Plan(AgentContextTestSupport.Root, mutated, inventory.Files,
            "CodeChange", ["NoxAeterna.Domain/Astrology/NatalChart.cs"], 100_000);
        var minimum = baseline.Plan.Budget.MandatoryCharacters;
        var omitted = new ContextPlanner().Plan(AgentContextTestSupport.Root, mutated, inventory.Files,
            "CodeChange", ["NoxAeterna.Domain/Astrology/NatalChart.cs"], minimum);

        Assert.Contains(baseline.Plan.SelectedFiles, file => file.Path == "docs/ARCHITECTURE.md");
        Assert.Contains("docs/ARCHITECTURE.md", omitted.Plan.OmittedRecommendedFiles);
    }

    [Fact]
    public void FileCapAndRepeatedPlanAreDeterministic()
    {
        var registry = AgentContextTestSupport.Registry();
        var task = registry.TaskKinds.Single(item => item.Name == "CodeChange");
        var limited = registry with { TaskKinds = registry.TaskKinds.Select(item => item == task ? item with { MaxSelectedFiles = 1 } : item).ToArray() };
        var inventory = AgentContextTestSupport.Inventory();
        var failure = new ContextPlanner().Plan(AgentContextTestSupport.Root, limited, inventory.Files,
            "CodeChange", ["NoxAeterna.Domain/Astrology/NatalChart.cs"], 100_000);
        var first = Plan("CodeChange", ["NoxAeterna.Rendering/Charts/CircularChartRenderer.cs"], 70_000);
        var second = Plan("CodeChange", ["NoxAeterna.Rendering/Charts/CircularChartRenderer.cs"], 70_000);

        Assert.False(failure.Succeeded);
        Assert.Contains(failure.Plan.Diagnostics, item => item.Code == "mandatory-file-limit");
        Assert.Equal(ContextPlanCli.WriteJson(first.Plan, true), ContextPlanCli.WriteJson(second.Plan, true));
    }

    [Fact]
    public void JsonContainsPathsButNoContentsOrAbsoluteRoot()
    {
        var plan = Plan("CodeChange", ["NoxAeterna.Domain/Astrology/NatalChart.cs"], 70_000).Plan;
        var json = ContextPlanCli.WriteJson(plan, true);

        using var parsed = JsonDocument.Parse(json);
        Assert.Contains("NoxAeterna.Domain/Astrology/NatalChart.cs", json, StringComparison.Ordinal);
        Assert.DoesNotContain(AgentContextTestSupport.Root, json, StringComparison.OrdinalIgnoreCase);
        Assert.False(parsed.RootElement.TryGetProperty("contents", out _));
    }

    [Fact]
    public void BudgetUpperBoundaryIsInclusiveAndOverflowIsRejected()
    {
        var atLimit = Plan("CodeChange", ["NoxAeterna.Domain/Astrology/NatalChart.cs"], ContextPlanner.MaximumBudgetCharacters);
        var aboveLimit = Plan("CodeChange", ["NoxAeterna.Domain/Astrology/NatalChart.cs"], ContextPlanner.MaximumBudgetCharacters + 1);

        Assert.True(atLimit.Succeeded);
        Assert.False(aboveLimit.Succeeded);
        Assert.Contains(aboveLimit.Plan.Diagnostics, item => item.Code == "input-invalid");
    }

    [Fact]
    public void UnknownTaskKindIsControlledFailure()
    {
        var result = Plan("Unknown", ["NoxAeterna.Domain/Astrology/NatalChart.cs"], 70_000);

        Assert.False(result.Succeeded);
        Assert.Empty(result.Plan.RequestedPaths);
        Assert.Contains(result.Plan.Diagnostics, item => item.Code == "input-invalid" && item.Message.Contains("Unknown task kind", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("NoxAeterna.Rendering\\Charts\\Renderer.cs", "NoxAeterna.Rendering/Charts/Renderer.cs")]
    [InlineData("NoxAeterna.Rendering/Charts/../Renderer.cs", "NoxAeterna.Rendering/Renderer.cs")]
    public void TargetBoundaryCanonicalizesPortableRelativePaths(string input, string expected)
    {
        Assert.True(ContextTargetPath.TryCanonicalize(input, out var actual, out var error));
        Assert.Null(error);
        Assert.Equal(expected, actual);
    }

    private static ContextPlanResult Plan(string task, IReadOnlyList<string> paths, int budget)
    {
        var inventory = AgentContextTestSupport.Inventory();
        return new ContextPlanner().Plan(AgentContextTestSupport.Root, AgentContextTestSupport.Registry(),
            inventory.Files, task, paths, budget);
    }
}
