using NoxAeterna.Tools.Repository.Context.Routing;

namespace NoxAeterna.Tests.AgentContext;

public sealed class ContextRegistryTests
{
    [Fact]
    public void CurrentRegistryIsValidAndDeclaresExactTaskKinds()
    {
        var registry = AgentContextTestSupport.Registry();

        Assert.Equal(7, registry.TaskKinds.Count);
        Assert.Contains(registry.PathRules, rule => rule.Name == "agent-context");
    }

    [Theory]
    [InlineData("duplicate-task")]
    [InlineData("duplicate-rule")]
    [InlineData("unknown-task")]
    [InlineData("wrong-case-task")]
    [InlineData("invalid-priority")]
    [InlineData("invalid-limit")]
    [InlineData("invalid-glob")]
    [InlineData("archive-document")]
    [InlineData("missing-document")]
    [InlineData("private-document")]
    [InlineData("unknown-route")]
    public void InvalidRegistryMutationIsRejected(string mutation)
    {
        var inventory = AgentContextTestSupport.Inventory();
        var original = AgentContextTestSupport.Registry();
        var tasks = original.TaskKinds.ToList();
        var rules = original.PathRules.ToList();
        switch (mutation)
        {
            case "duplicate-task": tasks.Add(tasks[0]); break;
            case "duplicate-rule": rules.Add(rules[0]); break;
            case "unknown-task": rules[0] = rules[0] with { TaskKinds = ["Unknown"] }; break;
            case "wrong-case-task": tasks[0] = tasks[0] with { Name = tasks[0].Name.ToLowerInvariant() }; break;
            case "invalid-priority": rules[0] = rules[0] with { Priority = 0 }; break;
            case "invalid-limit": tasks[0] = tasks[0] with { MaxSelectedFiles = 0 }; break;
            case "invalid-glob": rules[0] = rules[0] with { Patterns = ["../**"] }; break;
            case "archive-document": rules[0] = rules[0] with { Documents = ["docs/archive/README.md"] }; break;
            case "missing-document": rules[0] = rules[0] with { Documents = ["docs/DOES-NOT-EXIST.md"] }; break;
            case "private-document": rules[0] = rules[0] with { Documents = ["docs/private/secret.md"] }; break;
            case "unknown-route": rules[0] = rules[0] with { TestRoutes = ["Invented"] }; break;
        }
        var registry = original with { TaskKinds = tasks, PathRules = rules };
        var routes = File.ReadAllText(Path.Combine(AgentContextTestSupport.Root, "eng/test-routes.json"));
        using var json = System.Text.Json.JsonDocument.Parse(routes);
        var names = json.RootElement.GetProperty("routes").EnumerateArray()
            .Select(item => item.GetProperty("name").GetString()!).ToHashSet(StringComparer.Ordinal);

        Assert.Throws<ContextRegistryException>(() => ContextRegistryLoader.Validate(registry, inventory.Files, names));
    }
}
