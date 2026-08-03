using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using NoxAeterna.App.Tarot;
using NoxAeterna.Domain.Tarot;

namespace NoxAeterna.Tests.App;

public sealed class TarotArtworkPackTests
{
    [Fact]
    public void LupusNoctisManifest_DeclaresVersionedPartialPackWithThreeAcceptedStandardCards()
    {
        var definition = TarotArtworkPackTestData.LoadRepositoryPack();

        Assert.Equal(1, definition.SchemaVersion);
        Assert.Equal("lupus-noctis", definition.Id.Value);
        Assert.Equal(StandardTarotCatalog.Deck.Id, definition.SemanticDeckId);
        Assert.Equal("ui.tarot.artwork.lupus-noctis", definition.DisplayNameLocalizationKey.Value);
        Assert.Equal((7, 12), (definition.AspectRatioWidth, definition.AspectRatioHeight));
        Assert.Equal((952, 1632), (definition.SourceWidth, definition.SourceHeight));
        Assert.True(definition.IsPartial);
        Assert.Equal(
            ["major.death", "minor.cups.six", "major.star"],
            definition.Cards.Select(static asset => asset.Card.Id.Value));
        Assert.All(definition.Cards, asset =>
        {
            Assert.Equal("accepted", asset.Status);
            Assert.Contains(asset.Card, StandardTarotCatalog.Deck.Cards);
            Assert.False(string.IsNullOrWhiteSpace(asset.GenerationProvenanceReference));
        });
        Assert.True(((IList<TarotArtworkPackCardAsset>)definition.Cards).IsReadOnly);
    }

    [Fact]
    public void LupusNoctisManifest_DeclaredAssetsExistWithExactDimensionsAndSha256()
    {
        var definition = TarotArtworkPackTestData.LoadRepositoryPack();

        Assert.All(definition.Cards, asset =>
        {
            Assert.Equal((952, 1632), (asset.Width, asset.Height));
            Assert.Equal(7 * asset.Height, 12 * asset.Width);
            using var content = asset.OpenRead();
            var actualHash = Convert.ToHexStringLower(SHA256.HashData(content));
            Assert.Equal(asset.Sha256, actualHash);
            Assert.True(File.Exists(Path.Combine(
                TarotArtworkPackTestData.PackRoot,
                asset.AssetPath.Replace('/', Path.DirectorySeparatorChar))));
        });
    }

    [Fact]
    public void ManifestLoader_RejectsDuplicateCardIds()
    {
        var png = TarotArtworkPackTestData.CreatePngHeader(952, 1632);
        var source = new TarotArtworkPackTestData.InMemorySource(
            TarotArtworkPackTestData.CreateManifest(
                TarotArtworkPackTestData.Entry("major.death", "cards/death-a.png", png),
                TarotArtworkPackTestData.Entry("major.death", "cards/death-b.png", png)),
            new Dictionary<string, byte[]>
            {
                ["cards/death-a.png"] = png,
                ["cards/death-b.png"] = png
            });

        var exception = Assert.Throws<TarotArtworkPackLoadException>(() =>
            TarotArtworkPackLoader.Load(source, StandardTarotCatalog.Deck));

        Assert.Contains("Duplicate artwork card ID 'major.death'", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ManifestLoader_RejectsDuplicateAssetPaths()
    {
        var png = TarotArtworkPackTestData.CreatePngHeader(952, 1632);
        var source = new TarotArtworkPackTestData.InMemorySource(
            TarotArtworkPackTestData.CreateManifest(
                TarotArtworkPackTestData.Entry("major.death", "cards/shared.png", png),
                TarotArtworkPackTestData.Entry("major.star", "cards/shared.png", png)),
            new Dictionary<string, byte[]> { ["cards/shared.png"] = png });

        var exception = Assert.Throws<TarotArtworkPackLoadException>(() =>
            TarotArtworkPackLoader.Load(source, StandardTarotCatalog.Deck));

        Assert.Contains("Duplicate artwork asset path 'cards/shared.png'", exception.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("../death.png")]
    [InlineData("cards/../death.png")]
    [InlineData("cards\\..\\death.png")]
    [InlineData("cards//death.png")]
    [InlineData("cards/./death.png")]
    [InlineData("C:\\cards\\death.png")]
    [InlineData("/cards/death.png")]
    [InlineData("\\\\server\\cards\\death.png")]
    public void ManifestLoader_RejectsRootedAndTraversalPaths(string path)
    {
        Assert.Throws<TarotArtworkPackLoadException>(() =>
            TarotArtworkPackLoader.ValidatePackageRelativePath(path));
    }

    [Fact]
    public void ManifestLoader_RejectsUnknownSemanticCardIdBeforeOpeningAsset()
    {
        var png = TarotArtworkPackTestData.CreatePngHeader(952, 1632);
        var source = new TarotArtworkPackTestData.InMemorySource(
            TarotArtworkPackTestData.CreateManifest(
                TarotArtworkPackTestData.Entry("major.unknown", "cards/unknown.png", png)),
            new Dictionary<string, byte[]> { ["cards/unknown.png"] = png });

        var exception = Assert.Throws<TarotArtworkPackLoadException>(() =>
            TarotArtworkPackLoader.Load(source, StandardTarotCatalog.Deck));

        Assert.Contains("Unknown semantic card ID 'major.unknown'", exception.Message, StringComparison.Ordinal);
        Assert.Empty(source.OpenedAssetPaths);
    }

    [Fact]
    public void ManifestLoader_RejectsMismatchedSemanticDeckBeforeOpeningAsset()
    {
        var png = TarotArtworkPackTestData.CreatePngHeader(952, 1632);
        var manifest = TarotArtworkPackTestData.CreateManifest(
                TarotArtworkPackTestData.Entry("major.death", "cards/death.png", png))
            .Replace("standard-78", "different-deck", StringComparison.Ordinal);
        var source = new TarotArtworkPackTestData.InMemorySource(
            manifest,
            new Dictionary<string, byte[]> { ["cards/death.png"] = png });

        var exception = Assert.Throws<TarotArtworkPackLoadException>(() =>
            TarotArtworkPackLoader.Load(source, StandardTarotCatalog.Deck));

        Assert.Contains("targets semantic deck 'different-deck'", exception.Message, StringComparison.Ordinal);
        Assert.Empty(source.OpenedAssetPaths);
    }

    [Fact]
    public void ManifestLoader_RejectsIncorrectCanonicalRatioOrSourceDimensions()
    {
        var png = TarotArtworkPackTestData.CreatePngHeader(952, 1632);
        var entry = TarotArtworkPackTestData.Entry("major.death", "cards/death.png", png);
        var invalidManifests = new[]
        {
            TarotArtworkPackTestData.CreateManifest(entry, ratioWidth: 8),
            TarotArtworkPackTestData.CreateManifest(entry, sourceWidth: 951),
            TarotArtworkPackTestData.CreateManifest(entry, sourceHeight: 1631)
        };

        Assert.All(invalidManifests, manifest =>
        {
            var source = new TarotArtworkPackTestData.InMemorySource(
                manifest,
                new Dictionary<string, byte[]>());
            Assert.Throws<TarotArtworkPackLoadException>(() =>
                TarotArtworkPackLoader.Load(source, StandardTarotCatalog.Deck));
        });
    }

    [Fact]
    public void ManifestLoader_RejectsCardMetadataThatDoesNotMatchCanonicalDimensions()
    {
        var png = TarotArtworkPackTestData.CreatePngHeader(951, 1632);
        var entry = TarotArtworkPackTestData.Entry(
            "major.death",
            "cards/death.png",
            png,
            width: 951,
            height: 1632);
        var source = new TarotArtworkPackTestData.InMemorySource(
            TarotArtworkPackTestData.CreateManifest(entry),
            new Dictionary<string, byte[]> { ["cards/death.png"] = png });

        var exception = Assert.Throws<TarotArtworkPackLoadException>(() =>
            TarotArtworkPackLoader.Load(source, StandardTarotCatalog.Deck));

        Assert.Contains("must declare 952x1632 at 7:12", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ManifestLoader_RejectsAssetWhoseActualDimensionsDoNotMatchManifest()
    {
        var png = TarotArtworkPackTestData.CreatePngHeader(951, 1632);
        var source = new TarotArtworkPackTestData.InMemorySource(
            TarotArtworkPackTestData.CreateManifest(
                TarotArtworkPackTestData.Entry("major.death", "cards/death.png", png)),
            new Dictionary<string, byte[]> { ["cards/death.png"] = png });

        var exception = Assert.Throws<TarotArtworkPackLoadException>(() =>
            TarotArtworkPackLoader.Load(source, StandardTarotCatalog.Deck));

        Assert.Contains("dimensions 951x1632 do not match manifest dimensions 952x1632", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ManifestLoader_RejectsAssetWhoseSha256DoesNotMatchManifest()
    {
        var png = TarotArtworkPackTestData.CreatePngHeader(952, 1632);
        var validManifest = TarotArtworkPackTestData.CreateManifest(
            TarotArtworkPackTestData.Entry("major.death", "cards/death.png", png));
        var actualHash = Convert.ToHexStringLower(SHA256.HashData(png));
        var manifest = validManifest.Replace(actualHash, new string('0', 64), StringComparison.Ordinal);
        var source = new TarotArtworkPackTestData.InMemorySource(
            manifest,
            new Dictionary<string, byte[]> { ["cards/death.png"] = png });

        var exception = Assert.Throws<TarotArtworkPackLoadException>(() =>
            TarotArtworkPackLoader.Load(source, StandardTarotCatalog.Deck));

        Assert.Contains("SHA-256 does not match", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ManifestLoader_RejectsCardWithoutAcceptedStatus()
    {
        var png = TarotArtworkPackTestData.CreatePngHeader(952, 1632);
        var manifest = TarotArtworkPackTestData.CreateManifest(
                TarotArtworkPackTestData.Entry("major.death", "cards/death.png", png))
            .Replace("\"status\":\"accepted\"", "\"status\":\"candidate\"", StringComparison.Ordinal);
        var source = new TarotArtworkPackTestData.InMemorySource(
            manifest,
            new Dictionary<string, byte[]> { ["cards/death.png"] = png });

        var exception = Assert.Throws<TarotArtworkPackLoadException>(() =>
            TarotArtworkPackLoader.Load(source, StandardTarotCatalog.Deck));

        Assert.Contains("must have accepted status", exception.Message, StringComparison.Ordinal);
        Assert.Empty(source.OpenedAssetPaths);
    }

    [Fact]
    public void ManifestLoader_ReportsMalformedManifestAsControlledDiagnostic()
    {
        var source = new TarotArtworkPackTestData.InMemorySource(
            "{ not valid json",
            new Dictionary<string, byte[]>());

        var exception = Assert.Throws<TarotArtworkPackLoadException>(() =>
            TarotArtworkPackLoader.Load(source, StandardTarotCatalog.Deck));

        Assert.Contains("manifest could not be read", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.IsType<JsonException>(exception.InnerException);
    }

    [Fact]
    public void ManifestLoader_ReportsMissingDeclaredAcceptedAsset()
    {
        var png = TarotArtworkPackTestData.CreatePngHeader(952, 1632);
        var source = new TarotArtworkPackTestData.InMemorySource(
            TarotArtworkPackTestData.CreateManifest(
                TarotArtworkPackTestData.Entry("major.death", "cards/death.png", png)),
            new Dictionary<string, byte[]>());

        var exception = Assert.Throws<TarotArtworkPackLoadException>(() =>
            TarotArtworkPackLoader.Load(source, StandardTarotCatalog.Deck));

        Assert.Contains("is missing", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void BuiltInCatalog_OrdersClassicBeforeLupusNoctisWithoutDiagnostics()
    {
        var catalog = TarotArtworkPackCatalog.CreateBuiltIn();

        Assert.Empty(catalog.Diagnostics);
        Assert.Equal(
            ["prototype-symbolic", "lupus-noctis"],
            catalog.AvailableOptions.Select(static option => option.Id.Value));
    }

    [Theory]
    [InlineData("major.death", "cards/major/death.png")]
    [InlineData("minor.cups.six", "cards/minor/cups/six.png")]
    [InlineData("major.star", "cards/major/star.png")]
    public void PartialPackResolver_AcceptedCardsResolveRasterArtwork(string cardId, string assetPath)
    {
        var catalog = TarotArtworkPackCatalog.CreateForTests(TarotArtworkPackTestData.LoadRepositoryPack());
        var card = StandardTarotCatalog.Deck.Cards.Single(candidate => candidate.Id.Value == cardId);

        var resolution = catalog.Resolve(new TarotArtworkPackId("lupus-noctis"), card);

        Assert.Same(card, resolution.Card);
        Assert.Equal(TarotArtworkResolutionKind.Raster, resolution.Kind);
        Assert.False(resolution.IsPartialPackFallback);
        Assert.NotNull(resolution.RasterAsset);
        Assert.Equal(assetPath, resolution.RasterAsset!.AssetPath);
    }

    [Fact]
    public void PartialPackResolver_AllOtherStandardCardsResolveControlledPrototypeFallback()
    {
        var catalog = TarotArtworkPackCatalog.CreateForTests(TarotArtworkPackTestData.LoadRepositoryPack());
        var resolutions = StandardTarotCatalog.Deck.Cards
            .Select(card => catalog.Resolve(new TarotArtworkPackId("lupus-noctis"), card))
            .ToArray();

        Assert.Equal(3, resolutions.Count(static resolution => resolution.Kind == TarotArtworkResolutionKind.Raster));
        Assert.Equal(75, resolutions.Count(static resolution => resolution.IsPartialPackFallback));
        Assert.All(resolutions, resolution =>
        {
            Assert.Contains(resolution.Card, StandardTarotCatalog.Deck.Cards);
            if (resolution.IsPartialPackFallback)
            {
                Assert.Equal(TarotArtworkResolutionKind.Prototype, resolution.Kind);
                Assert.Null(resolution.RasterAsset);
            }
        });
        Assert.Equal(
            StandardTarotCatalog.Deck.Cards.Select(static card => card.Id),
            resolutions.Select(static resolution => resolution.Card.Id));
    }

    [Fact]
    public void RasterCardVisualPlan_KeepsLocalizedTitleAsOverlayAndRotatesComposedFace()
    {
        var catalog = TarotArtworkPackCatalog.CreateForTests(TarotArtworkPackTestData.LoadRepositoryPack());
        var card = StandardTarotCatalog.Deck.Cards.Single(candidate => candidate.Id.Value == "major.death");
        var artwork = catalog.Resolve(new TarotArtworkPackId("lupus-noctis"), card);
        var assignment = new TarotDrawnCard(new TarotSpreadPositionId("card"), card, TarotCardOrientation.Reversed);

        var plan = TarotCardVisualPlan.Create(
            assignment,
            artwork,
            "Death",
            "Major Arcana",
            prototypeFallbackText: null);

        Assert.Equal(TarotArtworkResolutionKind.Raster, plan.ArtworkKind);
        Assert.Equal("cards/major/death.png", plan.RasterAssetPath);
        Assert.Equal("Death", plan.LocalizedTitle);
        Assert.True(plan.HasLocalizedTitleOverlay);
        Assert.True(plan.HasProgrammaticFrame);
        Assert.Null(plan.PrototypeFallbackText);
        Assert.Equal(180d, plan.RotationDegrees);

        var manifest = File.ReadAllText(Path.Combine(TarotArtworkPackTestData.PackRoot, "artwork-pack.json"));
        Assert.DoesNotContain("cardTitle", manifest, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("cardNumber", manifest, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("generationPrompt", manifest, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void PrototypeFallbackVisualPlan_PreservesSemanticCardAndHonestFallbackLabel()
    {
        var catalog = TarotArtworkPackCatalog.CreateForTests(TarotArtworkPackTestData.LoadRepositoryPack());
        var card = StandardTarotCatalog.Deck.Cards.Single(candidate => candidate.Id.Value == "major.fool");
        var artwork = catalog.Resolve(new TarotArtworkPackId("lupus-noctis"), card);
        var assignment = new TarotDrawnCard(new TarotSpreadPositionId("card"), card, TarotCardOrientation.Upright);

        var plan = TarotCardVisualPlan.Create(
            assignment,
            artwork,
            "The Fool",
            "Major Arcana",
            "Prototype fallback");

        Assert.Same(card, plan.Card);
        Assert.Equal(TarotArtworkResolutionKind.Prototype, plan.ArtworkKind);
        Assert.Null(plan.RasterAssetPath);
        Assert.Equal("The Fool", plan.LocalizedTitle);
        Assert.Equal("Prototype fallback", plan.PrototypeFallbackText);
        Assert.Equal(0d, plan.RotationDegrees);
        Assert.True(plan.HasLocalizedTitleOverlay);
        Assert.True(plan.HasProgrammaticFrame);
    }
}

internal static class TarotArtworkPackTestData
{
    public static string PackRoot { get; } = Path.GetFullPath(Path.Combine(
        AppContext.BaseDirectory,
        "..", "..", "..", "..",
        "resources", "assets", "tarot", "artwork-packs", "lupus-noctis"));

    public static TarotArtworkPackDefinition LoadRepositoryPack() => TarotArtworkPackLoader.Load(
        new RepositoryPackSource(PackRoot),
        StandardTarotCatalog.Deck);

    public static byte[] CreatePngHeader(int width, int height)
    {
        var content = new byte[24];
        new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }.CopyTo(content, 0);
        Encoding.ASCII.GetBytes("IHDR").CopyTo(content, 12);
        BinaryPrimitives.WriteInt32BigEndian(content.AsSpan(16, 4), width);
        BinaryPrimitives.WriteInt32BigEndian(content.AsSpan(20, 4), height);
        return content;
    }

    public static object Entry(
        string cardId,
        string assetPath,
        byte[] content,
        int width = 952,
        int height = 1632) => new
    {
        cardId,
        assetPath,
        width,
        height,
        sha256 = Convert.ToHexStringLower(SHA256.HashData(content)),
        status = "accepted",
        generationProvenanceReference = $"LUPUS-NOCTIS.md#{cardId}"
    };

    public static string CreateManifest(
        object card,
        int ratioWidth = 7,
        int ratioHeight = 12,
        int sourceWidth = 952,
        int sourceHeight = 1632) => CreateManifest(
        [card],
        ratioWidth,
        ratioHeight,
        sourceWidth,
        sourceHeight);

    public static string CreateManifest(params object[] cards) => CreateManifest(cards, 7, 12, 952, 1632);

    private static string CreateManifest(
        object[] cards,
        int ratioWidth,
        int ratioHeight,
        int sourceWidth,
        int sourceHeight) => JsonSerializer.Serialize(new
    {
        schemaVersion = 1,
        artworkPackId = "lupus-noctis",
        semanticDeckId = "standard-78",
        displayNameLocalizationKey = "ui.tarot.artwork.lupus-noctis",
        canonicalAspectRatio = new { width = ratioWidth, height = ratioHeight },
        expectedSourceDimensions = new { width = sourceWidth, height = sourceHeight },
        partialPack = true,
        cards
    });

    internal sealed class InMemorySource(
        string manifest,
        IReadOnlyDictionary<string, byte[]> assets) : ITarotArtworkPackResourceSource
    {
        public List<string> OpenedAssetPaths { get; } = [];

        public Stream OpenManifest() => new MemoryStream(Encoding.UTF8.GetBytes(manifest), writable: false);

        public Stream? OpenAsset(string validatedRelativePath)
        {
            OpenedAssetPaths.Add(validatedRelativePath);
            return assets.TryGetValue(validatedRelativePath, out var content)
                ? new MemoryStream(content, writable: false)
                : null;
        }
    }

    private sealed class RepositoryPackSource(string packRoot) : ITarotArtworkPackResourceSource
    {
        public Stream OpenManifest() => File.OpenRead(Path.Combine(packRoot, "artwork-pack.json"));

        public Stream? OpenAsset(string validatedRelativePath)
        {
            var normalized = TarotArtworkPackLoader.ValidatePackageRelativePath(validatedRelativePath);
            var fullPath = Path.GetFullPath(Path.Combine(
                packRoot,
                normalized.Replace('/', Path.DirectorySeparatorChar)));
            var rootPrefix = Path.GetFullPath(packRoot) + Path.DirectorySeparatorChar;
            if (!fullPath.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Test resource escaped the artwork-pack root.");
            }

            return File.Exists(fullPath) ? File.OpenRead(fullPath) : null;
        }
    }
}
