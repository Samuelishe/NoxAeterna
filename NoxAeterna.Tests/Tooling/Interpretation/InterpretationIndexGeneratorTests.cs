using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using NoxAeterna.Tools.Repository.Interpretation.Indexing;

namespace NoxAeterna.Tests.Tooling.Interpretation;

public sealed class InterpretationIndexGeneratorTests
{
    [Fact]
    public void CompleteSingleCorpusGeneratesDeterministicProseFreeIndexAndSynchronizesManifestHash()
    {
        using var fixture = InterpretationToolingFixture.CreateSkeleton();
        fixture.AddCompleteSingleCardCorpus();
        var manifestBefore = JsonDocument.Parse(File.ReadAllText(fixture.ManifestPath));
        var readyBefore = manifestBefore.RootElement.GetProperty("modules")
            .GetProperty("single-card").GetProperty("ru").GetProperty("ready").GetBoolean();

        var first = new InterpretationIndexGenerator().Generate(fixture.Root, checkOnly: false);
        var indexPath = Path.Combine(fixture.Root, "indexes", "ru", "single-card.json");
        var firstBytes = File.ReadAllBytes(indexPath);
        var second = new InterpretationIndexGenerator().Generate(fixture.Root, checkOnly: false);
        var secondBytes = File.ReadAllBytes(indexPath);

        Assert.True(first.Success, string.Join(Environment.NewLine, first.Diagnostics));
        Assert.True(second.Success, string.Join(Environment.NewLine, second.Diagnostics));
        Assert.Equal(firstBytes, secondBytes);
        Assert.False(firstBytes.AsSpan().StartsWith(Encoding.UTF8.Preamble));
        Assert.Equal((byte)'\n', firstBytes[^1]);
        Assert.NotEqual((byte)'\n', firstBytes[^2]);
        var indexText = Encoding.UTF8.GetString(firstBytes);
        Assert.DoesNotContain("Synthetic situation", indexText, StringComparison.Ordinal);

        using var index = JsonDocument.Parse(firstBytes);
        var entries = index.RootElement.GetProperty("entries").EnumerateArray().ToArray();
        Assert.Equal(156, entries.Length);
        Assert.Equal("major.chariot|reversed", entries[0].GetProperty("key").GetString());
        Assert.Equal(
            entries.Select(item => item.GetProperty("key").GetString()).Order(StringComparer.Ordinal),
            entries.Select(item => item.GetProperty("key").GetString()));
        var firstEntry = entries.Single(item => item.GetProperty("key").GetString() == "major.fool|upright");
        Assert.Equal(
            Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(fixture.FirstSingleCardPath()))),
            firstEntry.GetProperty("sha256").GetString());

        using var manifest = JsonDocument.Parse(File.ReadAllText(fixture.ManifestPath));
        var reference = Assert.Single(manifest.RootElement.GetProperty("indexFiles").EnumerateArray().ToArray());
        Assert.Equal("indexes/ru/single-card.json", reference.GetProperty("path").GetString());
        Assert.Equal(Convert.ToHexStringLower(SHA256.HashData(firstBytes)), reference.GetProperty("sha256").GetString());
        Assert.Equal(readyBefore, manifest.RootElement.GetProperty("modules")
            .GetProperty("single-card").GetProperty("ru").GetProperty("ready").GetBoolean());
    }

    [Fact]
    public void CheckModeIsGreenWhenCurrentAndReportsDriftWithoutWrites()
    {
        using var fixture = InterpretationToolingFixture.CreateSkeleton();
        fixture.AddCompleteSingleCardCorpus();
        Assert.True(new InterpretationIndexGenerator().Generate(fixture.Root, checkOnly: false).Success);
        var indexPath = Path.Combine(fixture.Root, "indexes", "ru", "single-card.json");

        var current = new InterpretationIndexGenerator().Generate(fixture.Root, checkOnly: true);
        var indexBefore = File.ReadAllBytes(indexPath);
        var manifestBefore = File.ReadAllBytes(fixture.ManifestPath);
        File.AppendAllText(fixture.FirstSingleCardPath(), " ");
        var drift = new InterpretationIndexGenerator().Generate(fixture.Root, checkOnly: true);

        Assert.True(current.Success);
        Assert.Empty(current.DriftPaths);
        Assert.False(drift.Success);
        Assert.Contains("indexes/ru/single-card.json", drift.DriftPaths);
        Assert.Contains("interpretation-pack.json", drift.DriftPaths);
        Assert.Equal(indexBefore, File.ReadAllBytes(indexPath));
        Assert.Equal(manifestBefore, File.ReadAllBytes(fixture.ManifestPath));
    }

    [Fact]
    public void FailedWriteDoesNotPartiallyUpdateManifestOrCreateIndex()
    {
        using var fixture = InterpretationToolingFixture.CreateSkeleton();
        fixture.AddCompleteSingleCardCorpus();
        var manifestBefore = File.ReadAllBytes(fixture.ManifestPath);
        var generator = new InterpretationIndexGenerator(new FailingWriter());

        var report = generator.Generate(fixture.Root, checkOnly: false);

        Assert.False(report.Success);
        Assert.Contains(report.Diagnostics, item => item.Code == "generate.write");
        Assert.Equal(manifestBefore, File.ReadAllBytes(fixture.ManifestPath));
        Assert.False(File.Exists(Path.Combine(fixture.Root, "indexes", "ru", "single-card.json")));
    }

    [Fact]
    public void InvalidOrIncompleteNotReadyContentDoesNotGenerateEmptyIndex()
    {
        using var fixture = InterpretationToolingFixture.CreateSkeleton();
        var partialPath = Path.Combine(
            fixture.Root, "content", "ru", "modes", "single-card", "major.fool", "upright.json");
        Directory.CreateDirectory(Path.GetDirectoryName(partialPath)!);
        File.WriteAllText(partialPath, "{}");

        var invalid = new InterpretationIndexGenerator().Generate(fixture.Root, checkOnly: false);

        Assert.False(invalid.Success);
        Assert.False(File.Exists(Path.Combine(fixture.Root, "indexes", "ru", "single-card.json")));
        Assert.Equal(0, JsonDocument.Parse(File.ReadAllText(fixture.ManifestPath))
            .RootElement.GetProperty("indexFiles").GetArrayLength());
    }

    [Fact]
    public void StaleIndexIsReportedAndNeverDeletedByGeneration()
    {
        using var fixture = InterpretationToolingFixture.CreateSkeleton();
        var stale = Path.Combine(fixture.Root, "indexes", "ru", "stale.json");
        Directory.CreateDirectory(Path.GetDirectoryName(stale)!);
        File.WriteAllText(stale, "{}");

        var report = new InterpretationIndexGenerator().Generate(fixture.Root, checkOnly: false);

        Assert.False(report.Success);
        Assert.Contains(report.Diagnostics, item => item.Code == "generate.stale-index");
        Assert.True(File.Exists(stale));
    }

    private sealed class FailingWriter : IInterpretationPackFileWriter
    {
        public void Write(string packRoot, IReadOnlyList<PreparedInterpretationFile> files) =>
            throw new IOException("Synthetic atomic write failure.");
    }
}
