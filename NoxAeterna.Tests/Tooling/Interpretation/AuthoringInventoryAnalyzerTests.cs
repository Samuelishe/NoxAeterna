using System.Text.Json.Nodes;
using NoxAeterna.Tools.Repository.Interpretation.Authoring;

namespace NoxAeterna.Tests.Tooling.Interpretation;

public sealed class AuthoringInventoryAnalyzerTests
{
    [Fact]
    public void DraftReviewedAcceptedEntriesProduceDeterministicProgressGroups()
    {
        using var working = CreateWorkingRoot();
        WriteInventory(working.Path,
        [
            Entry("draft", "batch-a", key: "major.fool|upright"),
            Entry("reviewed", "batch-a", reviewer: "reviewer", key: "major.fool|reversed"),
            Entry("accepted", "batch-b", reviewer: "owner", acceptedAt: "2026-08-06T10:00:00Z",
                key: "major.world|upright")
        ]);

        var report = new AuthoringInventoryAnalyzer().Analyze(working.Path);

        Assert.True(report.Success, string.Join(Environment.NewLine, report.Diagnostics));
        Assert.Equal(3, report.Counts["total"]);
        Assert.Equal(3, report.Counts["locale:ru"]);
        Assert.Equal(3, report.Counts["corpus:single-card"]);
        Assert.Equal(1, report.Counts["status:draft"]);
        Assert.Equal(1, report.Counts["status:reviewed"]);
        Assert.Equal(1, report.Counts["status:accepted"]);
        Assert.Equal(2, report.Counts["batch:batch-a"]);
    }

    [Theory]
    [InlineData("accepted", null, "2026-08-06T10:00:00Z", "authoring.reviewer")]
    [InlineData("accepted", "owner", "not-a-timestamp", "authoring.accepted-at")]
    [InlineData("reviewed", null, null, "authoring.reviewer")]
    [InlineData("draft", null, "2026-08-06T10:00:00Z", "authoring.accepted-at")]
    public void InvalidTransitionMetadataIsRejected(
        string status,
        string? reviewer,
        string? acceptedAt,
        string expectedCode)
    {
        using var working = CreateWorkingRoot();
        WriteInventory(working.Path, [Entry(status, "batch-a", reviewer, acceptedAt)]);

        var report = new AuthoringInventoryAnalyzer().Analyze(working.Path);

        Assert.False(report.Success);
        Assert.Contains(report.Diagnostics, item => item.Code == expectedCode);
    }

    [Fact]
    public void DuplicateKeyInvalidBatchAndStaleTranslationAreReported()
    {
        using var working = CreateWorkingRoot();
        var first = Entry("draft", "batch-a");
        first["sourceRevision"] = 2;
        first["translationRevision"] = 1;
        WriteInventory(working.Path,
        [
            first,
            Entry("draft", "batch-b"),
            Entry("draft", " ", key: "major.world|upright")
        ]);

        var report = new AuthoringInventoryAnalyzer().Analyze(working.Path);

        Assert.False(report.Success);
        Assert.Contains(report.Diagnostics, item => item.Code == "authoring.batch");
        Assert.Contains(report.Diagnostics, item => item.Code == "authoring.translation-stale");
        Assert.Contains(report.Diagnostics, item => item.Code == "authoring.duplicate");
    }

    [Fact]
    public void OptionalProductionComparisonFindsBothDirections()
    {
        using var working = CreateWorkingRoot();
        using var pack = InterpretationToolingFixture.CreateSkeleton();
        pack.AddCompleteSingleCardCorpus();
        WriteInventory(working.Path,
        [
            Entry("accepted", "batch-a", "owner", "2026-08-06T10:00:00Z",
                "synthesis|relation-label|synthetic-one", "three-cards")
        ]);

        var report = new AuthoringInventoryAnalyzer().Analyze(working.Path, pack.Root);

        Assert.False(report.Success);
        Assert.Contains(report.Diagnostics, item => item.Code == "authoring.accepted-missing-production");
        Assert.Contains(report.Diagnostics, item => item.Code == "authoring.production-missing-inventory");
    }

    private static JsonObject Entry(
        string status,
        string batchId,
        string? reviewer = null,
        string? acceptedAt = null,
        string key = "major.fool|upright",
        string corpusId = "single-card") => new()
    {
        ["locale"] = "ru",
        ["corpusId"] = corpusId,
        ["entryKey"] = key,
        ["status"] = status,
        ["batchId"] = batchId,
        ["sourceRevision"] = 1,
        ["translationRevision"] = null,
        ["reviewer"] = reviewer,
        ["acceptedAt"] = acceptedAt
    };

    private static void WriteInventory(string root, IEnumerable<JsonObject> entries)
    {
        var document = new JsonObject
        {
            ["schemaVersion"] = 1,
            ["packId"] = "synthetic-pack",
            ["entries"] = new JsonArray(entries.Select(item => (JsonNode)item).ToArray())
        };
        File.WriteAllText(Path.Combine(root, "authoring-inventory.json"), document.ToJsonString() + "\n");
    }

    private static TemporaryWorkingRoot CreateWorkingRoot()
    {
        var root = Path.Combine(Path.GetTempPath(), $"NoxAeterna-authoring-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        return new(root);
    }

    private sealed class TemporaryWorkingRoot(string path) : IDisposable
    {
        public string Path { get; } = path;

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
