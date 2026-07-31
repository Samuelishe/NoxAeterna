using NoxAeterna.Tools.Repository.Analysis;

namespace NoxAeterna.Tests.ProjectStats;

public sealed class RepositoryInventoryTests
{
    [Fact]
    public void TrackedAndPublicUntrackedFilesAreDiscoveredWithStableState()
    {
        using var fixture = ProjectStatsTestFixture.Create();
        fixture.Write("NoxAeterna.Domain/Tracked.cs", "class Tracked;\n", tracked: true);
        fixture.Write("docs/untracked.md", "# Public\n");

        var result = new GitRepositoryInventory().Discover(fixture.Root);

        Assert.Contains(result.Files, file => file.Path == "NoxAeterna.Domain/Tracked.cs" && file.IsTracked);
        Assert.Contains(result.Files, file => file.Path == "docs/untracked.md" && !file.IsTracked);
    }

    [Fact]
    public void IgnoredAndGeneratedPathsDoNotEnterOrdinaryInventory()
    {
        using var fixture = ProjectStatsTestFixture.Create();
        fixture.Write("bin/generated.cs", "private data");
        fixture.Write("obj/generated.cs", "private data");
        fixture.Write("TestResults/result.txt", "private data");
        fixture.Write("NoxAeterna.Domain/Public.cs", "class Public;", tracked: true);

        var result = new GitRepositoryInventory().Discover(fixture.Root);

        Assert.Single(result.Files, file => file.Path == "NoxAeterna.Domain/Public.cs");
        Assert.DoesNotContain(result.Files, file => RepositoryPathPolicy.IsGenerated(file.Path));
    }

    [Fact]
    public void TrackedGeneratedPathProducesDiagnosticWithoutEnteringRankings()
    {
        using var fixture = ProjectStatsTestFixture.Create();
        fixture.Write("bin/tracked-generated.cs", "generated", tracked: true, force: true);

        var result = new GitRepositoryInventory().Discover(fixture.Root);

        Assert.DoesNotContain(result.Files, file => file.Path == "bin/tracked-generated.cs");
        Assert.Contains(result.Diagnostics, diagnostic =>
            diagnostic.Code == "generated-excluded" && diagnostic.Path == "bin/tracked-generated.cs");
    }

    [Fact]
    public void PrivateAndSensitiveTrackedFilesAreExcludedBeforeContentRead()
    {
        using var fixture = ProjectStatsTestFixture.Create();
        var privatePath = fixture.Write("docs/private/secret.md", "DO-NOT-READ", tracked: true, force: true);
        fixture.Write("docs/public.md", "# Public", tracked: true);
        var reader = new RecordingFileReader();

        var result = new GitRepositoryInventory(reader).Discover(fixture.Root);

        Assert.DoesNotContain(result.Files, file => file.Path.Contains("secret", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(reader.Reads, path => path.Equals(privatePath, StringComparison.OrdinalIgnoreCase));
        var diagnostic = Assert.Single(result.Diagnostics, item => item.Code == "privacy-excluded");
        Assert.Null(diagnostic.Path);
        Assert.DoesNotContain("secret", diagnostic.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ExplicitOutputTargetIsExcludedFromItsOwnScan()
    {
        using var fixture = ProjectStatsTestFixture.Create();
        fixture.Write("reports/stats.md", "old report");
        fixture.Write("README.md", "# Public", tracked: true);

        var result = new GitRepositoryInventory().Discover(fixture.Root, "reports/stats.md");

        Assert.DoesNotContain(result.Files, file => file.Path == "reports/stats.md");
        Assert.Contains(result.Diagnostics, diagnostic =>
            diagnostic.Code == "output-excluded" && diagnostic.Path == "reports/stats.md");
    }

    [Fact]
    public void UnreadablePublicFileProducesDiagnosticAndScanContinues()
    {
        using var fixture = ProjectStatsTestFixture.Create();
        fixture.Write("docs/unreadable.md", "content", tracked: true);
        fixture.Write("docs/readable.md", "content", tracked: true);
        var reader = new RecordingFileReader("unreadable.md");

        var result = new GitRepositoryInventory(reader).Discover(fixture.Root);

        Assert.DoesNotContain(result.Files, file => file.Path == "docs/unreadable.md");
        Assert.Contains(result.Files, file => file.Path == "docs/readable.md");
        Assert.Contains(result.Diagnostics, diagnostic =>
            diagnostic.Code == "file-unreadable" && diagnostic.Path == "docs/unreadable.md");
    }

    [Fact]
    public void BinaryFileContributesBytesWithoutTextMetrics()
    {
        using var fixture = ProjectStatsTestFixture.Create();
        fixture.WriteBytes("resources/blob.png", [1, 0, 2, 3], tracked: true);

        var result = new GitRepositoryInventory().Discover(fixture.Root);

        var file = Assert.Single(result.Files, file => file.Path == "resources/blob.png");
        Assert.Equal(4, file.Bytes);
        Assert.False(file.IsText);
        Assert.Null(file.Lines);
        Assert.Equal(RepositoryFileCategory.Resources, file.Category);
    }

    [Fact]
    public void PathsAndOrderingAreNormalizedAndDeterministic()
    {
        using var fixture = ProjectStatsTestFixture.Create();
        fixture.Write("zeta/file.md", "z", tracked: true);
        fixture.Write("alpha/file.md", "a", tracked: true);
        var inventory = new GitRepositoryInventory();

        var first = inventory.Discover(fixture.Root).Files.Select(static file => file.Path).ToArray();
        var second = inventory.Discover(fixture.Root).Files.Select(static file => file.Path).ToArray();

        Assert.Equal(first.OrderBy(static path => path, StringComparer.Ordinal), first);
        Assert.Equal(first, second);
        Assert.All(first, path => Assert.DoesNotContain('\\', path));
    }

    [Fact]
    public void MissingRepositoryRootIsControlledError()
    {
        var missing = Path.Combine(Path.GetTempPath(), $"missing-{Guid.NewGuid():N}");

        var exception = Assert.Throws<ArgumentException>(() => GitRepositoryInventory.ResolveRoot(missing));

        Assert.Contains("does not exist", exception.Message, StringComparison.Ordinal);
    }
}
