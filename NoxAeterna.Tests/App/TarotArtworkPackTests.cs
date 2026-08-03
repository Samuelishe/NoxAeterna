using System.Buffers.Binary;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using NoxAeterna.App.Tarot;
using NoxAeterna.Domain.Tarot;

namespace NoxAeterna.Tests.App;

public sealed class TarotArtworkPackTests
{
    [Fact]
    public void LupusNoctisManifest_DeclaresVersionedPartialPackWithFifteenExactAcceptedStandardCards()
    {
        var definition = TarotArtworkPackTestData.LoadRepositoryPack();
        (string CardId, string AssetPath, int Width, int Height, string Sha256)[] expectedCards =
        [
            (
                "major.death",
                "cards/major/death.png",
                952,
                1632,
                "b5bb6ea0d42adc2d195494bb737b03d72a3c950ce90b2878bee974c9213dadc5"),
            (
                "minor.cups.six",
                "cards/minor/cups/six.png",
                952,
                1632,
                "ad71eb1e48abe8155aa6272164607c3005d7e4ad08569856274fd3f49557c0d5"),
            (
                "major.star",
                "cards/major/star.png",
                952,
                1632,
                "79cb0c40e926c2acafffd80fa622385b5a6d7534518d8050114027bb7be5ee95"),
            (
                "major.sun",
                "cards/major/sun.png",
                952,
                1632,
                "d4b0e233f966b60c9541184a59e7e591ffba3f0117902557e0832ad52342b034"),
            (
                "minor.swords.five",
                "cards/minor/swords/five.png",
                952,
                1632,
                "0fffd6ec8c95f9ded1fe566e46e9a2340190261e4e678ea3d17684a46e58a124"),
            (
                "major.moon",
                "cards/major/moon.png",
                952,
                1632,
                "5bf8f1d8436249b5a14791794cf062181e4050556fe2685f9567d60550a79b50"),
            (
                "major.hanged-man",
                "cards/major/hanged-man.png",
                952,
                1632,
                "ebf3f8529df4053ec46a136c3066e74c53fc15afa9185a0d3e888d5589892597"),
            (
                "minor.pentacles.eight",
                "cards/minor/pentacles/eight.png",
                952,
                1632,
                "d2d5f8a2ca2c7e2459e4fabdbb386818e34b4613bbc7f0bb0fa81116c5d956f7"),
            (
                "minor.wands.four",
                "cards/minor/wands/four.png",
                952,
                1632,
                "fdfbec46ea62ff46bd85a9fa1972c784392eb5b02a3310352860aa71aa849f51"),
            (
                "major.lovers",
                "cards/major/lovers.png",
                952,
                1632,
                "3182833ff748bcbd881a9016ac2812555a9aa85a83f88f94731d672b7f9fe181"),
            (
                "minor.swords.nine",
                "cards/minor/swords/nine.png",
                952,
                1632,
                "7b817375e7e862fbeba9752468fc7147bc3f26525d259bd2560eb14a0fb40364"),
            (
                "minor.pentacles.two",
                "cards/minor/pentacles/two.png",
                952,
                1632,
                "148ffbef955a279ebda241c6f528ec9bcb9c539a5f8e1b0fb3edf07bd43a8f2c"),
            (
                "major.emperor",
                "cards/major/emperor.png",
                952,
                1632,
                "75a27aa6df945af1ef4037b9082366d5236856e1234f3ede5d9caa55fedfee1b"),
            (
                "minor.cups.five",
                "cards/minor/cups/five.png",
                952,
                1632,
                "15e0798c23efd278c90345c509349be8463168eff500f83bcfd3f101f25e73ae"),
            (
                "minor.wands.page",
                "cards/minor/wands/page.png",
                952,
                1632,
                "f9c75b034362b396c00f616240cfa4548bb0efbdfe7633b012e54e34af54c222")
        ];

        Assert.Equal(1, definition.SchemaVersion);
        Assert.Equal("lupus-noctis", definition.Id.Value);
        Assert.Equal(StandardTarotCatalog.Deck.Id, definition.SemanticDeckId);
        Assert.Equal("ui.tarot.artwork.lupus-noctis", definition.DisplayNameLocalizationKey.Value);
        Assert.Equal((7, 12), (definition.AspectRatioWidth, definition.AspectRatioHeight));
        Assert.Equal((952, 1632), (definition.SourceWidth, definition.SourceHeight));
        Assert.True(definition.IsPartial);
        Assert.Equal(
            expectedCards,
            definition.Cards.Select(static asset => (
                asset.Card.Id.Value,
                asset.AssetPath,
                asset.Width,
                asset.Height,
                asset.Sha256)));
        Assert.All(definition.Cards, asset =>
        {
            Assert.Equal("accepted", asset.Status);
            Assert.Contains(asset.Card, StandardTarotCatalog.Deck.Cards);
            Assert.Equal(7 * asset.Height, 12 * asset.Width);
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
    public void LupusNoctisManifest_OriginalSixRgbaProductionPngsRemainDecodableAtExpectedDimensions()
    {
        var definition = TarotArtworkPackTestData.LoadRepositoryPack();
        string[] originalRgbaCardIds =
        [
            "major.death",
            "minor.cups.six",
            "major.star",
            "major.sun",
            "minor.swords.five",
            "major.moon"
        ];
        var originalRgbaAssets = definition.Cards
            .Where(asset => originalRgbaCardIds.Contains(asset.Card.Id.Value, StringComparer.Ordinal))
            .ToArray();

        Assert.Equal(6, originalRgbaAssets.Length);
        Assert.All(originalRgbaAssets, asset =>
        {
            using var content = asset.OpenRead();
            var decodedSize = TarotArtworkPackTestData.DecodeRgbaPng(content);
            Assert.Equal((952, 1632), decodedSize);
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
    [InlineData("major.sun", "cards/major/sun.png")]
    [InlineData("minor.swords.five", "cards/minor/swords/five.png")]
    [InlineData("major.moon", "cards/major/moon.png")]
    [InlineData("major.hanged-man", "cards/major/hanged-man.png")]
    [InlineData("minor.pentacles.eight", "cards/minor/pentacles/eight.png")]
    [InlineData("minor.wands.four", "cards/minor/wands/four.png")]
    [InlineData("major.lovers", "cards/major/lovers.png")]
    [InlineData("minor.swords.nine", "cards/minor/swords/nine.png")]
    [InlineData("minor.pentacles.two", "cards/minor/pentacles/two.png")]
    [InlineData("major.emperor", "cards/major/emperor.png")]
    [InlineData("minor.cups.five", "cards/minor/cups/five.png")]
    [InlineData("minor.wands.page", "cards/minor/wands/page.png")]
    public void PartialPackResolver_AllFifteenAcceptedCardsResolveRasterArtwork(string cardId, string assetPath)
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

        Assert.Equal(15, resolutions.Count(static resolution => resolution.Kind == TarotArtworkResolutionKind.Raster));
        Assert.Equal(63, resolutions.Count(static resolution => resolution.IsPartialPackFallback));
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
    public void PartialPackResolver_OmittedSixteenthCardStillUsesPrototypeFallback()
    {
        var catalog = TarotArtworkPackCatalog.CreateForTests(TarotArtworkPackTestData.LoadRepositoryPack());
        var card = StandardTarotCatalog.Deck.Cards.Single(candidate => candidate.Id.Value == "major.fool");

        var resolution = catalog.Resolve(new TarotArtworkPackId("lupus-noctis"), card);

        Assert.Same(card, resolution.Card);
        Assert.Equal("major.fool", resolution.Card.Id.Value);
        Assert.Equal(TarotArtworkResolutionKind.Prototype, resolution.Kind);
        Assert.True(resolution.IsPartialPackFallback);
        Assert.Null(resolution.RasterAsset);
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

    public static (int Width, int Height) DecodeRgbaPng(Stream stream)
    {
        using var encoded = new MemoryStream();
        stream.CopyTo(encoded);
        var content = encoded.ToArray();
        ReadOnlySpan<byte> pngSignature = [137, 80, 78, 71, 13, 10, 26, 10];
        if (content.Length < 33 || !content.AsSpan(0, 8).SequenceEqual(pngSignature))
        {
            throw new InvalidDataException("The artwork is not a PNG stream.");
        }

        var offset = 8;
        var width = 0;
        var height = 0;
        var foundHeader = false;
        var foundEnd = false;
        using var compressedPixels = new MemoryStream();
        while (offset <= content.Length - 12)
        {
            var chunkLength = BinaryPrimitives.ReadInt32BigEndian(content.AsSpan(offset, 4));
            if (chunkLength < 0 || offset + 12L + chunkLength > content.Length)
            {
                throw new InvalidDataException("The PNG contains a truncated chunk.");
            }

            var chunkType = content.AsSpan(offset + 4, 4);
            var chunkData = content.AsSpan(offset + 8, chunkLength);
            if (chunkType.SequenceEqual("IHDR"u8))
            {
                if (foundHeader || chunkLength != 13)
                {
                    throw new InvalidDataException("The PNG must contain one 13-byte IHDR chunk.");
                }

                foundHeader = true;
                width = BinaryPrimitives.ReadInt32BigEndian(chunkData[..4]);
                height = BinaryPrimitives.ReadInt32BigEndian(chunkData.Slice(4, 4));
                if (width <= 0 || height <= 0 ||
                    chunkData[8] != 8 || chunkData[9] != 6 ||
                    chunkData[10] != 0 || chunkData[11] != 0 || chunkData[12] != 0)
                {
                    throw new InvalidDataException("The artwork must be a non-interlaced 8-bit RGBA PNG.");
                }
            }
            else if (chunkType.SequenceEqual("IDAT"u8))
            {
                compressedPixels.Write(chunkData);
            }
            else if (chunkType.SequenceEqual("IEND"u8))
            {
                if (chunkLength != 0)
                {
                    throw new InvalidDataException("The PNG IEND chunk must be empty.");
                }

                foundEnd = true;
                break;
            }

            offset += 12 + chunkLength;
        }

        if (!foundHeader || !foundEnd || compressedPixels.Length == 0)
        {
            throw new InvalidDataException("The PNG is missing required image chunks.");
        }

        var bytesPerPixel = 4;
        var stride = checked(width * bytesPerPixel);
        var scanlineLength = checked(stride + 1);
        var filteredPixels = new byte[checked(scanlineLength * height)];
        compressedPixels.Position = 0;
        using (var inflater = new ZLibStream(compressedPixels, CompressionMode.Decompress, leaveOpen: true))
        {
            inflater.ReadExactly(filteredPixels);
            if (inflater.ReadByte() != -1)
            {
                throw new InvalidDataException("The PNG contains excess decompressed pixel data.");
            }
        }

        var decodedPixels = new byte[checked(stride * height)];
        for (var row = 0; row < height; row++)
        {
            var filter = filteredPixels[row * scanlineLength];
            if (filter > 4)
            {
                throw new InvalidDataException($"The PNG scanline uses unsupported filter '{filter}'.");
            }

            var sourceOffset = row * scanlineLength + 1;
            var destinationOffset = row * stride;
            for (var column = 0; column < stride; column++)
            {
                var raw = filteredPixels[sourceOffset + column];
                var left = column >= bytesPerPixel
                    ? decodedPixels[destinationOffset + column - bytesPerPixel]
                    : 0;
                var above = row > 0
                    ? decodedPixels[destinationOffset - stride + column]
                    : 0;
                var upperLeft = row > 0 && column >= bytesPerPixel
                    ? decodedPixels[destinationOffset - stride + column - bytesPerPixel]
                    : 0;
                var predictor = filter switch
                {
                    0 => 0,
                    1 => left,
                    2 => above,
                    3 => (left + above) / 2,
                    4 => PaethPredictor(left, above, upperLeft),
                    _ => throw new InvalidDataException("Unsupported PNG filter.")
                };
                decodedPixels[destinationOffset + column] = unchecked((byte)(raw + predictor));
            }
        }

        return (width, height);
    }

    private static int PaethPredictor(int left, int above, int upperLeft)
    {
        var prediction = left + above - upperLeft;
        var leftDistance = Math.Abs(prediction - left);
        var aboveDistance = Math.Abs(prediction - above);
        var upperLeftDistance = Math.Abs(prediction - upperLeft);
        if (leftDistance <= aboveDistance && leftDistance <= upperLeftDistance)
        {
            return left;
        }

        return aboveDistance <= upperLeftDistance ? above : upperLeft;
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
