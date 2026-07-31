using System.Globalization;
using System.Text.RegularExpressions;

namespace NoxAeterna.Tests.Tooling;

public sealed class DocumentationGovernanceTests
{
    private static readonly string[] MetadataLabels =
    [
        "Role",
        "Read when",
        "Authoritative for",
        "Not authoritative for"
    ];

    [Fact]
    public void OperationalAndCurrentStateOwnersExistWithRequiredMetadata()
    {
        foreach (var path in new[]
                 {
                     "AGENTS.md",
                     "docs/AGENTS.md",
                     "docs/PROJECT-STATE.md",
                     "docs/DOCUMENTATION-GOVERNANCE.md"
                 })
        {
            var absolutePath = Path.Combine(ToolingTestSupport.RepositoryRoot, path);
            Assert.True(File.Exists(absolutePath), path);
            var content = File.ReadAllText(absolutePath);
            Assert.All(MetadataLabels, label => Assert.Contains(label, content, StringComparison.Ordinal));
        }
    }

    [Fact]
    public void NavigationAndExtendedGuidePointToCanonicalOwners()
    {
        var index = File.ReadAllText(Path.Combine(
            ToolingTestSupport.RepositoryRoot,
            "docs",
            "INDEX.md"));
        Assert.Contains("../AGENTS.md", index, StringComparison.Ordinal);
        Assert.Contains("PROJECT-STATE.md", index, StringComparison.Ordinal);
        Assert.Contains("DOCUMENTATION-GOVERNANCE.md", index, StringComparison.Ordinal);
        Assert.Contains("../eng/README.md", index, StringComparison.Ordinal);
        Assert.Contains("archive/README.md", index, StringComparison.Ordinal);

        var extendedGuide = File.ReadAllText(Path.Combine(
            ToolingTestSupport.RepositoryRoot,
            "docs",
            "AGENTS.md"));
        Assert.Contains("../AGENTS.md", extendedGuide, StringComparison.Ordinal);
        Assert.Contains("PROJECT-STATE.md", extendedGuide, StringComparison.Ordinal);
        Assert.Contains("DOCUMENTATION-GOVERNANCE.md", extendedGuide, StringComparison.Ordinal);
    }

    [Fact]
    public void ActiveAndArchivedSessionRangesDoNotOverlapAndChunkIsIndexed()
    {
        var archiveDirectory = Path.Combine(
            ToolingTestSupport.RepositoryRoot,
            "docs",
            "archive",
            "session-log");
        var archiveIndex = File.ReadAllText(Path.Combine(
            ToolingTestSupport.RepositoryRoot,
            "docs",
            "archive",
            "README.md"));
        var rangePattern = new Regex(
            @"^SESSION-LOG_(?<start>\d{4}-\d{2}-\d{2})_to_(?<end>\d{4}-\d{2}-\d{2})\.md$",
            RegexOptions.CultureInvariant);
        var ranges = Directory.EnumerateFiles(archiveDirectory, "SESSION-LOG_*.md")
            .Select(path =>
            {
                var match = rangePattern.Match(Path.GetFileName(path));
                Assert.True(match.Success, path);
                Assert.Contains(Path.GetFileName(path), archiveIndex, StringComparison.Ordinal);
                return (
                    Start: DateOnly.ParseExact(match.Groups["start"].Value, "yyyy-MM-dd", CultureInfo.InvariantCulture),
                    End: DateOnly.ParseExact(match.Groups["end"].Value, "yyyy-MM-dd", CultureInfo.InvariantCulture));
            })
            .OrderBy(static range => range.Start)
            .ToArray();

        Assert.NotEmpty(ranges);
        for (var index = 1; index < ranges.Length; index++)
        {
            Assert.True(ranges[index - 1].End < ranges[index].Start);
        }

        var activeLog = File.ReadAllText(Path.Combine(
            ToolingTestSupport.RepositoryRoot,
            "docs",
            "SESSION-LOG.md"));
        var activeDates = Regex.Matches(
                activeLog,
                @"(?m)^##\s+(?<date>\d{4}-\d{2}-\d{2})(?::|\s|$)")
            .Select(match => DateOnly.ParseExact(
                match.Groups["date"].Value,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture));
        Assert.All(
            activeDates,
            date => Assert.DoesNotContain(ranges, range => date >= range.Start && date <= range.End));
    }
}

