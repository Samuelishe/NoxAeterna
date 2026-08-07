using System.Diagnostics;
using System.Text;
using System.Xml.Linq;
using NoxAeterna.Interpretation.Tarot.Contracts;
using NoxAeterna.Interpretation.Tarot.Serialization;
using NoxAeterna.Interpretation.Tarot.Validation;
using NoxAeterna.Tools.Repository.Interpretation.Analysis;

namespace NoxAeterna.Tests.App;

[Collection("Interpretation packaging builds")]
public sealed class TarotInterpretationPackagingTests
{
    [Fact]
    public void RepositoryAttributes_KeepInterpretationJsonCanonicalAcrossCheckouts()
    {
        var attributesPath = RepositoryPath(".gitattributes");
        Assert.True(File.Exists(attributesPath));
        var lines = File.ReadAllLines(attributesPath);

        Assert.Contains("resources/interpretation/**/*.json text eol=lf", lines);
        Assert.Contains("NoxAeterna.Tests/TestData/Interpretation/**/*.json text eol=lf", lines);

        var jsonFiles = Directory.GetFiles(RepositoryPath("resources", "interpretation"), "*.json", SearchOption.AllDirectories)
            .Concat(Directory.GetFiles(RepositoryPath("NoxAeterna.Tests", "TestData", "Interpretation"), "*.json", SearchOption.AllDirectories));
        Assert.All(jsonFiles, path =>
        {
            var bytes = File.ReadAllBytes(path);
            Assert.False(bytes.AsSpan().StartsWith(Encoding.UTF8.Preamble));
            Assert.False(bytes.AsSpan().IndexOf("\r\n"u8) >= 0);
            Assert.Equal((byte)'\n', bytes[^1]);
            Assert.NotEqual((byte)'\n', bytes[^2]);
        });
    }

    [Fact]
    public void RepositoryClassicSkeleton_IsExactValidNotReadyManifestAndOnlyProductionFile()
    {
        var root = RepositoryPackRoot();
        var manifestPath = Path.Combine(root, "interpretation-pack.json");
        var bytes = File.ReadAllBytes(manifestPath);
        var parsed = TarotInterpretationJson.Parse<TarotInterpretationPackDocument>(bytes);

        Assert.True(parsed.IsSuccess, parsed.Failure?.Message);
        var validation = TarotInterpretationValidator.ValidateManifest(parsed.Document!);
        Assert.True(validation.IsValid, string.Join(Environment.NewLine, validation.Diagnostics.Select(item => item.Message)));
        var manifest = validation.Value!;
        Assert.Equal("classic", manifest.PackId.Value);
        Assert.Equal("standard-78", manifest.SemanticDeckId.Value);
        Assert.Equal(1, manifest.ContentVersion);
        Assert.Empty(manifest.IndexFiles);
        Assert.All(manifest.Modules.Values.SelectMany(item => item.Values), module => Assert.False(module.Ready));
        Assert.True(bytes.SequenceEqual(TarotInterpretationJson.Serialize(parsed.Document!)));
        Assert.False(bytes.AsSpan().StartsWith(Encoding.UTF8.Preamble));
        Assert.Equal((byte)'\n', bytes[^1]);
        Assert.NotEqual((byte)'\n', bytes[^2]);
        Assert.Equal(new[] { manifestPath }, Directory.GetFiles(root, "*", SearchOption.AllDirectories));
        Assert.True(new InterpretationPackValidator().Validate(root).Success);
    }

    [Fact]
    public void AppProject_UsesAlwaysForManifestAndIndexesPreserveNewestForContentAndNeverPackagesWorking()
    {
        var project = XDocument.Load(RepositoryPath("NoxAeterna.App", "NoxAeterna.App.csproj"));
        var items = project.Descendants("None").ToArray();
        var manifest = Item(items, "interpretation-pack.json");
        var indexes = Item(items, @"indexes\**\*.json");
        var content = Item(items, @"content\**\*.json");

        Assert.Equal("Always", (string?)manifest.Attribute("CopyToOutputDirectory"));
        Assert.Equal("Always", (string?)manifest.Attribute("CopyToPublishDirectory"));
        Assert.Equal("Always", (string?)indexes.Attribute("CopyToOutputDirectory"));
        Assert.Equal("Always", (string?)indexes.Attribute("CopyToPublishDirectory"));
        Assert.Equal("PreserveNewest", (string?)content.Attribute("CopyToOutputDirectory"));
        Assert.Equal("PreserveNewest", (string?)content.Attribute("CopyToPublishDirectory"));
        Assert.DoesNotContain(items, item => ((string?)item.Attribute("Include"))?.Contains("working", StringComparison.OrdinalIgnoreCase) == true);
        Assert.Contains(project.Descendants("ProjectReference"), item =>
            ((string?)item.Attribute("Include"))?.Replace('\\', '/').EndsWith("NoxAeterna.Interpretation/NoxAeterna.Interpretation.csproj", StringComparison.Ordinal) == true);
    }

    [Fact]
    public void DebugAndReleaseOutputsContainExactManifestAndAlwaysReconstructFutureDatedStaleOutput()
    {
        var source = File.ReadAllBytes(Path.Combine(RepositoryPackRoot(), "interpretation-pack.json"));
        foreach (var configuration in new[] { "Debug", "Release" })
        {
            BuildApp(configuration);
            var output = OutputManifest(configuration);
            Assert.Equal(source, File.ReadAllBytes(output));
            AssertOutputInventory(configuration);

            File.WriteAllBytes(output, "{\"stale\":true}\n"u8.ToArray());
            File.SetLastWriteTimeUtc(output, DateTime.UtcNow.AddDays(1));
            BuildApp(configuration);

            Assert.Equal(source, File.ReadAllBytes(output));
        }
    }

    private static XElement Item(IEnumerable<XElement> items, string fragment) =>
        Assert.Single(items, item =>
            ((string?)item.Attribute("Include"))?.Contains(fragment, StringComparison.Ordinal) == true);

    private static void AssertOutputInventory(string configuration)
    {
        var root = Path.GetDirectoryName(OutputManifest(configuration))!;
        Assert.Equal(
            new[] { Path.Combine(root, "interpretation-pack.json") },
            Directory.GetFiles(root, "*.json", SearchOption.AllDirectories));
        Assert.False(Directory.Exists(Path.Combine(root, "working")));
        Assert.False(Directory.Exists(Path.Combine(root, "content")));
        Assert.False(Directory.Exists(Path.Combine(root, "indexes")));
        Assert.False(Directory.Exists(Path.Combine(
            RepositoryPath("NoxAeterna.App", "bin", configuration, "net10.0"),
            "TestData", "Interpretation")));
    }

    private static void BuildApp(string configuration)
    {
        using var process = Process.Start(new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = RepositoryPath(),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            ArgumentList =
            {
                "build",
                RepositoryPath("NoxAeterna.App", "NoxAeterna.App.csproj"),
                "-c", configuration,
                "--no-restore",
                "--nologo"
            }
        }) ?? throw new InvalidOperationException("Could not start dotnet build.");
        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();
        if (!process.WaitForExit(120_000))
        {
            process.Kill(entireProcessTree: true);
            throw new TimeoutException("Nested App packaging build timed out.");
        }

        Assert.True(process.ExitCode == 0, output + Environment.NewLine + error);
    }

    private static string OutputManifest(string configuration) => Path.Combine(
        RepositoryPath("NoxAeterna.App", "bin", configuration, "net10.0"),
        "resources", "interpretation", "tarot", "packs", "classic", "interpretation-pack.json");

    private static string RepositoryPackRoot() => RepositoryPath(
        "resources", "interpretation", "tarot", "packs", "classic");

    private static string RepositoryPath(params string[] segments) => Path.GetFullPath(Path.Combine(
        new[] { AppContext.BaseDirectory, "..", "..", "..", ".." }.Concat(segments).ToArray()));
}

[CollectionDefinition("Interpretation packaging builds", DisableParallelization = true)]
public sealed class InterpretationPackagingBuildCollection;
