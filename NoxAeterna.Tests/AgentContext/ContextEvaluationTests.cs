using System.Text.Json;
using NoxAeterna.Tools.Repository.Context.Evaluation;

namespace NoxAeterna.Tests.AgentContext;

public sealed class ContextEvaluationTests
{
    [Fact]
    public void CurrentFifteenEvaluationCasesPass()
    {
        var (runner, routes, registry, inventory) = Load();

        var report = runner.Run(AgentContextTestSupport.Root, routes, registry, inventory.Files);

        Assert.Equal(15, report.Cases.Count);
        Assert.All(report.Cases, item => Assert.True(item.Passed, string.Join(Environment.NewLine, item.Differences)));
        Assert.Equal("pass", report.Result);
    }

    [Fact]
    public void OneCaseSelectionIsDeterministicJson()
    {
        var (runner, routes, registry, inventory) = Load();
        var first = runner.Run(AgentContextTestSupport.Root, routes, registry, inventory.Files, "rendering-code-change");
        var second = runner.Run(AgentContextTestSupport.Root, routes, registry, inventory.Files, "rendering-code-change");
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        Assert.Single(first.Cases);
        Assert.Equal(JsonSerializer.Serialize(first, options), JsonSerializer.Serialize(second, options));
    }

    [Theory]
    [InlineData("must-include")]
    [InlineData("must-exclude")]
    [InlineData("expected-route")]
    [InlineData("forbidden-route")]
    [InlineData("max-files")]
    [InlineData("max-chars")]
    public void MutatedExpectationProducesFailure(string mutation)
    {
        var (runner, routes, registry, inventory) = Load();
        var source = registry.Cases.Single(item => item.Name == "rendering-code-change");
        var changed = mutation switch
        {
            "must-include" => source with { MustInclude = source.MustInclude.Append("docs/TAROT-ENGINE.md").ToArray() },
            "must-exclude" => source with { MustExclude = source.MustExclude.Append("docs/RENDERING-ENGINE.md").ToArray() },
            "expected-route" => source with { ExpectedTestRoutes = ["Rendering", "Geometry"] },
            "forbidden-route" => source with { ForbiddenTestRoutes = ["Rendering"] },
            "max-files" => source with { MaxSelectedFiles = 1 },
            _ => source with { MaxSelectedChars = 1 }
        };
        var report = runner.Run(AgentContextTestSupport.Root, routes,
            registry with { Cases = [changed] }, inventory.Files);

        var result = Assert.Single(report.Cases);
        Assert.False(result.Passed);
        Assert.NotEmpty(result.Differences);
    }

    [Fact]
    public void UnknownCaseIsControlledError()
    {
        var (runner, routes, registry, inventory) = Load();

        Assert.Throws<ContextEvaluationException>(() =>
            runner.Run(AgentContextTestSupport.Root, routes, registry, inventory.Files, "missing"));
    }

    [Theory]
    [InlineData("duplicate")]
    [InlineData("unknown-task")]
    [InlineData("missing-must-include")]
    public void InvalidEvaluationRegistryIsRejected(string mutation)
    {
        var (runner, routes, registry, inventory) = Load();
        var first = registry.Cases[0];
        var cases = mutation switch
        {
            "duplicate" => registry.Cases.Append(first).ToArray(),
            "unknown-task" => [first with { Task = "Unknown" }],
            _ => [first with { MustInclude = ["docs/DOES-NOT-EXIST.md"] }]
        };
        var path = Path.Combine(Path.GetTempPath(), $"context-evals-{Guid.NewGuid():N}.json");
        try
        {
            File.WriteAllText(path, JsonSerializer.Serialize(registry with { Cases = cases }));
            Assert.Throws<ContextEvaluationException>(() =>
                runner.Load(AgentContextTestSupport.Root, path, routes, inventory.Files));
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    private static (ContextEvaluationRunner Runner,
        NoxAeterna.Tools.Repository.Context.Routing.ContextRouteRegistry Routes,
        ContextEvaluationRegistry Registry,
        NoxAeterna.Tools.Repository.Analysis.RepositoryInventory Inventory) Load()
    {
        var inventory = AgentContextTestSupport.Inventory();
        var routes = AgentContextTestSupport.Registry();
        var runner = new ContextEvaluationRunner();
        var registry = runner.Load(AgentContextTestSupport.Root, "eng/context-evals.json", routes, inventory.Files);
        return (runner, routes, registry, inventory);
    }
}
