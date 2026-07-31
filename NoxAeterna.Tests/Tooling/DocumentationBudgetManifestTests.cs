using System.Text.Json;

namespace NoxAeterna.Tests.Tooling;

public sealed class DocumentationBudgetManifestTests
{
    [Fact]
    public void ManifestHasValidUniqueExistingEntriesWithoutHardOverflow()
    {
        var manifestPath = Path.Combine(
            ToolingTestSupport.RepositoryRoot,
            "eng",
            "document-budgets.json");
        using var document = JsonDocument.Parse(File.ReadAllText(manifestPath));
        var root = document.RootElement;

        Assert.Equal(1, root.GetProperty("schemaVersion").GetInt32());
        var warningRatio = root.GetProperty("warningRatio").GetDouble();
        Assert.InRange(warningRatio, double.Epsilon, 0.999999d);

        var entries = root.GetProperty("documents").EnumerateArray().ToArray();
        Assert.NotEmpty(entries);
        var paths = entries
            .Select(entry => entry.GetProperty("path").GetString())
            .ToArray();
        Assert.DoesNotContain(paths, string.IsNullOrWhiteSpace);
        Assert.Equal(
            paths.Length,
            paths.Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.Equal(
            paths.OrderBy(static path => path, StringComparer.Ordinal).ToArray(),
            paths);

        foreach (var entry in entries)
        {
            var path = Assert.IsType<string>(entry.GetProperty("path").GetString());
            var absolutePath = Path.Combine(ToolingTestSupport.RepositoryRoot, path);
            var hardLimit = entry.GetProperty("hardLimit").GetInt32();
            var strategy = entry.GetProperty("overflowStrategy").GetString();

            Assert.True(File.Exists(absolutePath), path);
            Assert.True(hardLimit > 0, path);
            Assert.True(File.ReadAllText(absolutePath).Length <= hardLimit, path);
            Assert.Contains(strategy, new[] { "manual-reconcile", "rollover-archive" });

            if (strategy == "rollover-archive")
            {
                var destination = entry.GetProperty("archiveDestination").GetString();
                Assert.False(string.IsNullOrWhiteSpace(destination));
                Assert.True(Directory.Exists(Path.Combine(ToolingTestSupport.RepositoryRoot, destination!)));
            }
            else
            {
                Assert.False(entry.TryGetProperty("archiveDestination", out _));
            }
        }
    }
}

