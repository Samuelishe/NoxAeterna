using System.Text.Json;
using System.Text.Json.Nodes;

namespace NoxAeterna.Tests.Tooling;

public sealed class TestRouteRegistryTests
{
    private static string RunnerPath => Path.Combine(
        ToolingTestSupport.RepositoryRoot,
        "eng",
        "test-route.ps1");

    [Fact]
    public void CurrentRegistryLoadsAndFullIsExplicitMilestone()
    {
        var result = Run("list", "-Json");

        using var document = ToolingTestSupport.ParseJson(result);
        var routes = document.RootElement.GetProperty("routes").EnumerateArray().ToArray();
        Assert.NotEmpty(routes);
        var full = Assert.Single(routes, route =>
            route.GetProperty("name").GetString() == "Full");
        Assert.Equal("leaf", full.GetProperty("kind").GetString());
        Assert.True(full.GetProperty("milestoneOnly").GetBoolean());
    }

    [Theory]
    [InlineData("duplicate", "Duplicate test route name")]
    [InlineData("missing-child", "references missing child")]
    [InlineData("cycle", "contains a cycle")]
    [InlineData("unknown-kind", "unknown kind")]
    [InlineData("missing-project", "references missing project")]
    [InlineData("outside-project", "must remain inside the repository")]
    [InlineData("invalid-timeout", "invalid timeout")]
    public void InvalidRegistryIsRejected(string mutation, string expectedMessage)
    {
        using var fixture = RegistryFixture.Create(mutation);

        var result = Run(
            "resolve",
            "Domain",
            "-Root",
            ToolingTestSupport.RepositoryRoot,
            "-Registry",
            fixture.Path);

        Assert.NotEqual(0, result.ExitCode);
        Assert.Contains(expectedMessage, result.CombinedOutput, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void LeafAndNestedCompositeResolveInStableOrderWithoutDuplicates()
    {
        using var leaf = ToolingTestSupport.ParseJson(Run("resolve", "Geometry", "-Json"));
        Assert.Equal(
            new[] { "Geometry" },
            ReadStrings(leaf.RootElement.GetProperty("resolvedLeaves")));

        using var composite = ToolingTestSupport.ParseJson(Run("resolve", "Desktop-UI", "-Json"));
        var leaves = ReadStrings(composite.RootElement.GetProperty("resolvedLeaves"));
        Assert.Equal(
            new[] { "Presentation", "Localization", "App-Workspace", "Theme-Resources" },
            leaves);
        Assert.Equal(leaves.Length, leaves.Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.DoesNotContain("Full", leaves, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void OrdinaryCompositeCannotHideMilestoneRoute()
    {
        using var fixture = RegistryFixture.Create("hidden-milestone");

        var result = Run(
            "resolve",
            "Desktop-UI",
            "-Root",
            ToolingTestSupport.RepositoryRoot,
            "-Registry",
            fixture.Path);

        Assert.NotEqual(0, result.ExitCode);
        Assert.Contains("hides milestone route", result.CombinedOutput, StringComparison.OrdinalIgnoreCase);
    }

    private static string[] ReadStrings(JsonElement element) =>
        element.EnumerateArray().Select(item => item.GetString()!).ToArray();

    private static ScriptResult Run(params string[] arguments) =>
        ToolingTestSupport.RunPowerShell(
            RunnerPath,
            ToolingTestSupport.RepositoryRoot,
            arguments);

    private sealed class RegistryFixture : IDisposable
    {
        private RegistryFixture(string path)
        {
            Path = path;
        }

        public string Path { get; }

        public static RegistryFixture Create(string mutation)
        {
            var sourcePath = System.IO.Path.Combine(
                ToolingTestSupport.RepositoryRoot,
                "eng",
                "test-routes.json");
            var root = JsonNode.Parse(File.ReadAllText(sourcePath))!.AsObject();
            var routes = root["routes"]!.AsArray();

            switch (mutation)
            {
                case "duplicate":
                    routes.Add(routes[0]!.DeepClone());
                    break;
                case "missing-child":
                    Find(routes, "Astrology-Core")!["children"]!.AsArray().Add("Missing");
                    break;
                case "cycle":
                    Find(routes, "Astrology-Core")!["children"]!.AsArray().Add("Astrology-Core");
                    break;
                case "unknown-kind":
                    Find(routes, "Domain")!["kind"] = "unknown";
                    break;
                case "missing-project":
                    Find(routes, "Domain")!["testProject"] = "missing.csproj";
                    break;
                case "outside-project":
                    Find(routes, "Domain")!["testProject"] = "../outside.csproj";
                    break;
                case "invalid-timeout":
                    Find(routes, "Domain")!["defaultTimeoutSeconds"] = 0;
                    break;
                case "hidden-milestone":
                    Find(routes, "Desktop-UI")!["children"]!.AsArray().Add("Full");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mutation));
            }

            var path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                $"NoxAeterna-test-routes-{Guid.NewGuid():N}.json");
            File.WriteAllText(path, root.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
            return new RegistryFixture(path);
        }

        public void Dispose()
        {
            if (File.Exists(Path))
            {
                File.Delete(Path);
            }
        }

        private static JsonObject? Find(JsonArray routes, string name) =>
            routes
                .Select(node => node!.AsObject())
                .Single(route => route["name"]!.GetValue<string>() == name);
    }
}
