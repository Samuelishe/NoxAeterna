using System.Text;
using NoxAeterna.App.Tarot;
using NoxAeterna.Domain.Tarot;
using NoxAeterna.Interpretation.Tarot.Contracts;
using NoxAeterna.Interpretation.Tarot.Resolution;
using NoxAeterna.Interpretation.Tarot.Sources;

namespace NoxAeterna.Tests.App;

public sealed class TarotInterpretationPackSourceTests : IDisposable
{
    private readonly string root = Path.Combine(
        Path.GetTempPath(), $"NoxAeterna-built-in-interpretation-{Guid.NewGuid():N}");

    [Fact]
    public void BuiltInSource_UsesExactClassicOutputRootAndReadsExactBytes()
    {
        var packRoot = CreatePackRoot();
        var expected = "{\"synthetic\":true}\n"u8.ToArray();
        var nested = Path.Combine(packRoot, "indexes", "ru", "synthetic.json");
        Directory.CreateDirectory(Path.GetDirectoryName(nested)!);
        File.WriteAllBytes(nested, expected);
        var source = new BuiltInClassicInterpretationPackSource(root);

        var manifest = source.ReadManifest();
        var nestedRead = source.ReadPackageFile(new("indexes/ru/synthetic.json"));

        Assert.Equal(new TarotInterpretationPackId("classic"), source.PackId);
        Assert.Equal(Path.GetFullPath(packRoot), source.PackRoot);
        Assert.Equal(TarotInterpretationSourceReadStatus.Found, manifest.Status);
        Assert.Equal(TarotInterpretationSourceReadStatus.Found, nestedRead.Status);
        Assert.Equal(expected, nestedRead.Bytes.ToArray());
    }

    [Fact]
    public void BuiltInSource_ReturnsControlledMissingAndFrozenPathRejectsEveryEscapeForm()
    {
        CreatePackRoot();
        var source = new BuiltInClassicInterpretationPackSource(root);

        var missing = source.ReadPackageFile(new("indexes/ru/missing.json"));

        Assert.Equal(TarotInterpretationSourceReadStatus.Missing, missing.Status);
        Assert.Empty(missing.Bytes.ToArray());
        Assert.Throws<ArgumentException>(() => new TarotPackageRelativePath("../outside.json"));
        Assert.Throws<ArgumentException>(() => new TarotPackageRelativePath("/absolute.json"));
        Assert.Throws<ArgumentException>(() => new TarotPackageRelativePath(@"indexes\ru\entry.json"));
        Assert.Throws<ArgumentException>(() => new TarotPackageRelativePath("C:/outside.json"));
    }

    [Fact]
    public void BuiltInSource_DependsOnlyOnSuppliedApplicationBaseNotCurrentDirectory()
    {
        var packRoot = CreatePackRoot();
        var source = new BuiltInClassicInterpretationPackSource(root);
        var sourceText = File.ReadAllText(RepositoryPath(
            "NoxAeterna.App", "Tarot", "BuiltInClassicInterpretationPackSource.cs"));

        Assert.Equal(Path.GetFullPath(packRoot), source.PackRoot);
        Assert.DoesNotContain("Environment.CurrentDirectory", sourceText, StringComparison.Ordinal);
        Assert.DoesNotContain("Directory.GetCurrentDirectory", sourceText, StringComparison.Ordinal);
        Assert.Contains("Path.GetRelativePath", sourceText, StringComparison.Ordinal);
        Assert.Contains("ResolveLinkTarget", sourceText, StringComparison.Ordinal);
    }

    [Fact]
    public void BuiltInCatalog_IsImmutableClassicOnlyAndRejectsDuplicateIds()
    {
        CreatePackRoot();
        var classic = new BuiltInClassicInterpretationPackSource(root);
        var catalog = new BuiltInTarotInterpretationPackSourceCatalog([classic]);
        var defaultCatalog = BuiltInTarotInterpretationPackSourceCatalog.CreateDefault();

        Assert.Equal(new[] { new TarotInterpretationPackId("classic") }, catalog.PackIds);
        Assert.Equal(new[] { new TarotInterpretationPackId("classic") }, defaultCatalog.PackIds);
        Assert.True(catalog.TryGetSource(new("classic"), out var found));
        Assert.Same(classic, found);
        Assert.False(catalog.TryGetSource(new("unknown"), out var missing));
        Assert.Null(missing);
        Assert.Throws<NotSupportedException>(() =>
            ((ICollection<TarotInterpretationPackId>)catalog.PackIds).Add(new("other")));
        Assert.Throws<ArgumentException>(() =>
            new BuiltInTarotInterpretationPackSourceCatalog([classic, classic]));

        var catalogSource = File.ReadAllText(RepositoryPath(
            "NoxAeterna.App", "Tarot", "BuiltInTarotInterpretationPackSourceCatalog.cs"));
        Assert.DoesNotContain("Artwork", catalogSource, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Theme", catalogSource, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("DisplayName", catalogSource, StringComparison.Ordinal);
        Assert.DoesNotContain("Option", catalogSource, StringComparison.Ordinal);
    }

    [Fact]
    public void CurrentDebugBuiltInSkeleton_ReturnsTypedNoReadyForRuAndZhAndPackUnavailableForUnknown()
    {
        var source = new BuiltInClassicInterpretationPackSource(AppContext.BaseDirectory);
        var catalog = new BuiltInTarotInterpretationPackSourceCatalog([source]);
        var resolver = new TarotInterpretationPackResolver(catalog, StandardTarotCatalog.Deck);

        var russian = resolver.ResolveSingleCard(new("classic"), new("ru"), new("major.fool"), TarotCardOrientation.Upright);
        var chinese = resolver.ResolveSingleCard(new("classic"), new("zh"), new("major.fool"), TarotCardOrientation.Upright);
        var unknown = resolver.ResolveSingleCard(new("unknown"), new("ru"), new("major.fool"), TarotCardOrientation.Upright);

        Assert.Equal(TarotNoContentReason.NoReadyLocale,
            Assert.IsType<NoTarotInterpretationContent<TarotSingleCardEntry>>(russian).Reason);
        Assert.Equal(TarotNoContentReason.NoReadyLocale,
            Assert.IsType<NoTarotInterpretationContent<TarotSingleCardEntry>>(chinese).Reason);
        Assert.Equal(TarotNoContentReason.PackUnavailable,
            Assert.IsType<NoTarotInterpretationContent<TarotSingleCardEntry>>(unknown).Reason);
        Assert.False(Directory.Exists(Path.Combine(source.PackRoot, "indexes")));
        Assert.False(Directory.Exists(Path.Combine(source.PackRoot, "content")));
    }

    public void Dispose()
    {
        if (Directory.Exists(root))
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private string CreatePackRoot()
    {
        var packRoot = Path.Combine(
            root,
            BuiltInClassicInterpretationPackSource.PackageOutputPath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(packRoot);
        File.WriteAllBytes(Path.Combine(packRoot, "interpretation-pack.json"), "{}\n"u8.ToArray());
        return packRoot;
    }

    private static string RepositoryPath(params string[] segments) => Path.GetFullPath(Path.Combine(
        new[] { AppContext.BaseDirectory, "..", "..", "..", ".." }.Concat(segments).ToArray()));
}
