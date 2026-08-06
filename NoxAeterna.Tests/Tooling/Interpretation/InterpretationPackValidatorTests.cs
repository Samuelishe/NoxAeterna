using System.Security.Cryptography;
using System.Text.Json.Nodes;
using NoxAeterna.Tools.Repository.Interpretation.Analysis;
using NoxAeterna.Tools.Repository.Interpretation.Indexing;

namespace NoxAeterna.Tests.Tooling.Interpretation;

public sealed class InterpretationPackValidatorTests
{
    [Fact]
    public void ValidNotReadySkeletonAllowsIncompleteInventories()
    {
        using var fixture = InterpretationToolingFixture.CreateSkeleton();

        var report = new InterpretationPackValidator().Validate(fixture.Root);

        Assert.True(report.Success);
        Assert.Equal(1, report.Counts["acceptedContentFiles"]);
        Assert.Equal(0, report.Errors);
    }

    [Theory]
    [InlineData("missing")]
    [InlineData("malformed")]
    public void MissingOrMalformedManifestIsControlledValidationFailure(string scenario)
    {
        using var fixture = InterpretationToolingFixture.CreateSkeleton();
        if (scenario == "missing") File.Delete(fixture.ManifestPath);
        else File.WriteAllText(fixture.ManifestPath, "{");

        var report = new InterpretationPackValidator().Validate(fixture.Root);

        Assert.False(report.Success);
        Assert.Contains(report.Diagnostics, item => item.Code == $"pack.manifest-{(scenario == "missing" ? "missing" : "json")}");
    }

    [Theory]
    [InlineData("/absolute/index.json")]
    [InlineData("indexes\\ru\\single-card.json")]
    [InlineData("../outside.json")]
    public void UnsafeOrBackslashManifestPathIsRejected(string unsafePath)
    {
        using var fixture = InterpretationToolingFixture.CreateSkeleton();
        var manifest = ReadObject(fixture.ManifestPath);
        manifest["modules"]!["single-card"]!["ru"]!["indexPaths"]![0] = unsafePath;
        WriteNode(fixture.ManifestPath, manifest);

        var report = new InterpretationPackValidator().Validate(fixture.Root);

        Assert.False(report.Success);
        Assert.Contains(report.Diagnostics, item => item.Code.Contains("value.invalid", StringComparison.Ordinal) ||
                                                    item.Code.Contains("index-contract", StringComparison.Ordinal));
    }

    [Fact]
    public void UnknownAcceptedContentPathAndMalformedAcceptedJsonAreRejected()
    {
        using var fixture = InterpretationToolingFixture.CreateSkeleton();
        var unknown = Path.Combine(fixture.Root, "content", "ru", "unknown", "entry.json");
        Directory.CreateDirectory(Path.GetDirectoryName(unknown)!);
        File.WriteAllText(unknown, "{}");
        File.WriteAllText(
            Path.Combine(fixture.Root, "content", "ru", "vocabulary", "synthetic-alpha.json"),
            "{");

        var report = new InterpretationPackValidator().Validate(fixture.Root);

        Assert.False(report.Success);
        Assert.Contains(report.Diagnostics, item => item.Code == "content.path");
        Assert.Contains(report.Diagnostics, item => item.Code == "content.json");
    }

    [Fact]
    public void CompleteGeneratedSingleCardPackValidatesEndToEnd()
    {
        using var fixture = CompleteGeneratedPack();

        var report = new InterpretationPackValidator().Validate(fixture.Root);

        Assert.True(report.Success);
        Assert.Equal(156, report.Counts["indexedEntries"]);
        Assert.Equal(0, report.Errors);
    }

    [Fact]
    public void ManifestIndexHashMismatchIsDetected()
    {
        using var fixture = CompleteGeneratedPack();
        var manifest = ReadObject(fixture.ManifestPath);
        manifest["indexFiles"]![0]!["sha256"] = new string('0', 64);
        WriteNode(fixture.ManifestPath, manifest);

        var report = new InterpretationPackValidator().Validate(fixture.Root);

        Assert.False(report.Success);
        Assert.Contains(report.Diagnostics, item => item.Code == "index.hash");
    }

    [Fact]
    public void ContentHashMismatchAndMissingIndexedFileAreDetected()
    {
        using var fixture = CompleteGeneratedPack();
        File.AppendAllText(fixture.FirstSingleCardPath(), " ");

        var hashMismatch = new InterpretationPackValidator().Validate(fixture.Root);

        Assert.False(hashMismatch.Success);
        Assert.Contains(hashMismatch.Diagnostics, item => item.Code == "content.hash");

        File.Delete(fixture.FirstSingleCardPath());
        var missing = new InterpretationPackValidator().Validate(fixture.Root);
        Assert.Contains(missing.Diagnostics, item => item.Code == "content.missing");
    }

    [Fact]
    public void IdentityPathMismatchAndUnindexedAcceptedContentAreDetected()
    {
        using var fixture = CompleteGeneratedPack();
        var content = ReadObject(fixture.FirstSingleCardPath());
        content["cardId"] = "major.magician";
        WriteNode(fixture.FirstSingleCardPath(), content);

        var identity = new InterpretationPackValidator().Validate(fixture.Root);

        Assert.False(identity.Success);
        Assert.Contains(identity.Diagnostics, item => item.Code == "content.identity-path");

        using var second = CompleteGeneratedPack();
        var indexPath = Path.Combine(second.Root, "indexes", "ru", "single-card.json");
        var index = ReadObject(indexPath);
        index["entries"]!.AsArray().RemoveAt(0);
        WriteNode(indexPath, index);
        SynchronizeManifestIndexHash(second.ManifestPath, indexPath);
        var unindexed = new InterpretationPackValidator().Validate(second.Root);
        Assert.Contains(unindexed.Diagnostics, item => item.Code == "content.unindexed");
    }

    [Fact]
    public void CrossLocaleIndexEntryAndBrokenReadyModuleAreRejected()
    {
        using var fixture = CompleteGeneratedPack();
        var indexPath = Path.Combine(fixture.Root, "indexes", "ru", "single-card.json");
        var index = ReadObject(indexPath);
        index["entries"]![0]!["path"] = "content/en/modes/single-card/major.fool/upright.json";
        WriteNode(indexPath, index);
        SynchronizeManifestIndexHash(fixture.ManifestPath, indexPath);

        var mixed = new InterpretationPackValidator().Validate(fixture.Root);

        Assert.Contains(mixed.Diagnostics, item => item.Code == "index.locale-mix");

        using var ready = CompleteGeneratedPack();
        ready.SetSingleCardReady(true);
        File.Delete(Path.Combine(ready.Root, "indexes", "ru", "single-card.json"));
        var broken = new InterpretationPackValidator().Validate(ready.Root);
        Assert.False(broken.Success);
        Assert.Contains(broken.Diagnostics, item => item.Code == "index.missing");
    }

    [Fact]
    public void StaleIndexIsReportedAndNotDeleted()
    {
        using var fixture = InterpretationToolingFixture.CreateSkeleton();
        var stale = Path.Combine(fixture.Root, "indexes", "ru", "stale.json");
        Directory.CreateDirectory(Path.GetDirectoryName(stale)!);
        File.WriteAllText(stale, "{}");

        var report = new InterpretationPackValidator().Validate(fixture.Root);

        Assert.False(report.Success);
        Assert.Contains(report.Diagnostics, item => item.Code == "index.stale");
        Assert.True(File.Exists(stale));
    }

    private static InterpretationToolingFixture CompleteGeneratedPack()
    {
        var fixture = InterpretationToolingFixture.CreateSkeleton();
        fixture.AddCompleteSingleCardCorpus();
        var generated = new InterpretationIndexGenerator().Generate(fixture.Root, checkOnly: false);
        Assert.True(generated.Success, string.Join(Environment.NewLine, generated.Diagnostics));
        return fixture;
    }

    private static JsonObject ReadObject(string path) => JsonNode.Parse(File.ReadAllText(path))!.AsObject();

    private static void WriteNode(string path, JsonNode node) =>
        File.WriteAllText(path, node.ToJsonString() + "\n", new System.Text.UTF8Encoding(false));

    private static void SynchronizeManifestIndexHash(string manifestPath, string indexPath)
    {
        var manifest = ReadObject(manifestPath);
        manifest["indexFiles"]![0]!["sha256"] = Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(indexPath)));
        WriteNode(manifestPath, manifest);
    }
}
