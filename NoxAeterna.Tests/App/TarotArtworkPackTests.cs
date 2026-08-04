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
    public void LupusNoctisManifest_DeclaresVersionedPartialPackWithFiftySevenExactAcceptedStandardCards()
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
                "f9c75b034362b396c00f616240cfa4548bb0efbdfe7633b012e54e34af54c222"),
            (
                "major.justice",
                "cards/major/justice.png",
                952,
                1632,
                "8dd09f388b84dad0cd69f2c4d97f7f3e7daf916b003a49dee2928d646b51ef27"),
            (
                "minor.pentacles.five",
                "cards/minor/pentacles/five.png",
                952,
                1632,
                "ef50bb642d36f1cc78fb4f97429f03c31ea5a0ca78d43acde2d3d3df18163829"),
            (
                "minor.cups.queen",
                "cards/minor/cups/queen.png",
                952,
                1632,
                "34e54fe087a9eebc554d47b7080a28451552a84ab729764b6fd4d8894b2774bd"),
            (
                "major.tower",
                "cards/major/tower.png",
                952,
                1632,
                "a4b64c2d460cedda35e019d3909431de27711b7e9c4b5b48f1986519322bbf2d"),
            (
                "minor.cups.eight",
                "cards/minor/cups/eight.png",
                952,
                1632,
                "9e7b7e6b4fa65f032b12313aa0bfc412ec94987b46261d4ae7f79287d8eea280"),
            (
                "minor.pentacles.three",
                "cards/minor/pentacles/three.png",
                952,
                1632,
                "fc746dce331e8c83799e5bb2184897048c8dc41b47c9671ec95ec49930bd7c51"),
            (
                "major.temperance",
                "cards/major/temperance.png",
                952,
                1632,
                "ccbe633ce578a5dcd3bfb3df801c840522e7cf5e488616b35e9bf85141826c80"),
            (
                "minor.swords.ten",
                "cards/minor/swords/ten.png",
                952,
                1632,
                "6d6ce84857c030261ea7433ed84acf95aa17d153c55a3fb91c0ac4d24ce91bc4"),
            (
                "minor.wands.queen",
                "cards/minor/wands/queen.png",
                952,
                1632,
                "3fb534d6f2271afe5ed6b57204902af9735c0578fd1e774cfc5b27a35059ad36"),
            (
                "major.strength",
                "cards/major/strength.png",
                952,
                1632,
                "8115c48e41e372d9e596a472199a71329baa92d80a065bd839f27e553ce99537"),
            (
                "minor.cups.four",
                "cards/minor/cups/four.png",
                952,
                1632,
                "3821ca6d0698c05928f9dcc83f9f403951e914832d4cc75dd0f1112252da8bff"),
            (
                "minor.pentacles.king",
                "cards/minor/pentacles/king.png",
                952,
                1632,
                "b2b99cd8e60a67922918df090cf03477f23e418e68e99e2f3dfadf1ee33c443c"),
            (
                "major.chariot",
                "cards/major/chariot.png",
                952,
                1632,
                "8b5b9310b7ee799e6f69974f7fde9da652c1e9bf4ecdca66255963c73adf3bc1"),
            (
                "minor.swords.two",
                "cards/minor/swords/two.png",
                952,
                1632,
                "63ae7faf384d4faf44b4a1089259b853d9fafe0db10b285aaddffffa47c750b2"),
            (
                "minor.cups.ten",
                "cards/minor/cups/ten.png",
                952,
                1632,
                "a5cba66ee9b4c0ef23ef1b5016b58540a0c55f06e23b0f2ac74ad90ea9bd7ce5"),
            (
                "major.hermit",
                "cards/major/hermit.png",
                952,
                1632,
                "1a02430815b1971cb43dd8c6976664215617d64ae891aad3b8c18858dc661364"),
            (
                "major.fool",
                "cards/major/fool.png",
                952,
                1632,
                "7ef794da86db7b41b3955b5e9da422b255ef5e853bc1c46cb8a23d20d2ff7fa6"),
            (
                "minor.pentacles.ace",
                "cards/minor/pentacles/ace.png",
                952,
                1632,
                "c315ddd92003346435ac649ce52c4a0f59b7ba0c095cce20f47d32e274e67023"),
            (
                "major.judgement",
                "cards/major/judgement.png",
                952,
                1632,
                "9616c30803a93fe87f0ade3152d86cee2bb54b0a33cac887f5baa9bef8431ed0"),
            (
                "minor.swords.six",
                "cards/minor/swords/six.png",
                952,
                1632,
                "47e59cda725d1f126b1ebbc7711e1f6a874490ac2e94a8aad652ef5e63384be9"),
            (
                "minor.wands.two",
                "cards/minor/wands/two.png",
                952,
                1632,
                "cd992efac6d41001d0bd0dcb115a94d7f1b98efaccae9a7893d86ec91ab20e2a"),
            (
                "major.high-priestess",
                "cards/major/high-priestess.png",
                952,
                1632,
                "6640a123b5d1e53fc10f580a0609f45d8ab9185abd8b776457923bce64d47b57"),
            (
                "minor.swords.seven",
                "cards/minor/swords/seven.png",
                952,
                1632,
                "188fe550a66d733b77bd61c0c12afb801f211bac94514a4bc38d2553c9de4e67"),
            (
                "minor.cups.ace",
                "cards/minor/cups/ace.png",
                952,
                1632,
                "62c015d4a670289e28fa1080a2acf9e0577abe381d738624d1c8861b55e4f1b4"),
            (
                "major.magician",
                "cards/major/magician.png",
                952,
                1632,
                "67abc56443e7dc2bde25f9bd491016386075a1d1b4f48ca182b3d36db60712a8"),
            (
                "minor.wands.knight",
                "cards/minor/wands/knight.png",
                952,
                1632,
                "890e4d1dfd6a4932dd4dce1cccde00b0a605632c8d604e094cea0cb164e94297"),
            (
                "minor.cups.page",
                "cards/minor/cups/page.png",
                952,
                1632,
                "ac0630371ac1e372feef352b89bdbb11c63ea61848dace26656e375f634e3dc9"),
            (
                "major.devil",
                "cards/major/devil.png",
                952,
                1632,
                "f10270282bce4dfd94174e1e2ac3f3c786c0b307ea5cdfef2a8b7d247aaf37df"),
            (
                "minor.cups.three",
                "cards/minor/cups/three.png",
                952,
                1632,
                "0817db00c2cf4346f42ba51dc716aa3ec62c2805c5d60f5270237486eda4d26d"),
            (
                "minor.swords.three",
                "cards/minor/swords/three.png",
                952,
                1632,
                "28089525ab5dd561a565baff6024a9008b829e492f4628713898a8bc3487eba6"),
            (
                "major.empress",
                "cards/major/empress.png",
                952,
                1632,
                "6b44ab1329ab9c1aeb665d5742a7577f6af9a439826c8ed97c7e15c0b6777aeb"),
            (
                "minor.swords.four",
                "cards/minor/swords/four.png",
                952,
                1632,
                "4db0c632b4da2b0b492db04c238cc89d236cdeb009db544b49296101b737183c"),
            (
                "minor.wands.eight",
                "cards/minor/wands/eight.png",
                952,
                1632,
                "3a8dbdd237647a8700201f9300afbbc6da42064a9eafc1fb9a9315e5bc2d5b55"),
            (
                "major.wheel-of-fortune",
                "cards/major/wheel-of-fortune.png",
                952,
                1632,
                "3835740df9e14a424614b3a763c17c7626743c105235e572f3d525eb0e1e2761"),
            (
                "minor.cups.king",
                "cards/minor/cups/king.png",
                952,
                1632,
                "c84a7fd9293474321cb9adf8d33ea3a479a6a7396be189e9939ebe24071acc26"),
            (
                "minor.pentacles.seven",
                "cards/minor/pentacles/seven.png",
                952,
                1632,
                "fde5402f988056ad84377a4fdf9ed18671bfccd0758c162758be4baa36cbc402"),
            (
                "major.hierophant",
                "cards/major/hierophant.png",
                952,
                1632,
                "d1414040e5440877f3481dd36288df383eaaed208880b91e7edc1c089aa096f7"),
            (
                "minor.cups.nine",
                "cards/minor/cups/nine.png",
                952,
                1632,
                "34d9ca7c3e817eeb2fcd75dbb58a7f36112a2b7571824dba3afd632417afd848"),
            (
                "minor.swords.queen",
                "cards/minor/swords/queen.png",
                952,
                1632,
                "8381a085cce80541903ba550b88f8c6a7c3c8845c9c7de599ff294f9ef643d31"),
            (
                "major.world",
                "cards/major/world.png",
                952,
                1632,
                "7c35333c054d12dafdee60dc0765ccf6f79582bf15ede8928ef060f39d613831"),
            (
                "minor.wands.six",
                "cards/minor/wands/six.png",
                952,
                1632,
                "f9c7ae9ad69c3a4840ed62216f57c73bc3cbb4fa384bce782b144426fa26612d"),
            (
                "minor.cups.two",
                "cards/minor/cups/two.png",
                952,
                1632,
                "3e40fbc3584f2a5f6d9a1f181566c1edfde60b721aa34b671fb19de83579a32b")
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
    [InlineData("major.justice", "cards/major/justice.png")]
    [InlineData("minor.pentacles.five", "cards/minor/pentacles/five.png")]
    [InlineData("minor.cups.queen", "cards/minor/cups/queen.png")]
    [InlineData("major.tower", "cards/major/tower.png")]
    [InlineData("minor.cups.eight", "cards/minor/cups/eight.png")]
    [InlineData("minor.pentacles.three", "cards/minor/pentacles/three.png")]
    [InlineData("major.temperance", "cards/major/temperance.png")]
    [InlineData("minor.swords.ten", "cards/minor/swords/ten.png")]
    [InlineData("minor.wands.queen", "cards/minor/wands/queen.png")]
    [InlineData("major.strength", "cards/major/strength.png")]
    [InlineData("minor.cups.four", "cards/minor/cups/four.png")]
    [InlineData("minor.pentacles.king", "cards/minor/pentacles/king.png")]
    [InlineData("major.chariot", "cards/major/chariot.png")]
    [InlineData("minor.swords.two", "cards/minor/swords/two.png")]
    [InlineData("minor.cups.ten", "cards/minor/cups/ten.png")]
    [InlineData("major.hermit", "cards/major/hermit.png")]
    [InlineData("major.fool", "cards/major/fool.png")]
    [InlineData("minor.pentacles.ace", "cards/minor/pentacles/ace.png")]
    [InlineData("major.judgement", "cards/major/judgement.png")]
    [InlineData("minor.swords.six", "cards/minor/swords/six.png")]
    [InlineData("minor.wands.two", "cards/minor/wands/two.png")]
    [InlineData("major.high-priestess", "cards/major/high-priestess.png")]
    [InlineData("minor.swords.seven", "cards/minor/swords/seven.png")]
    [InlineData("minor.cups.ace", "cards/minor/cups/ace.png")]
    [InlineData("major.magician", "cards/major/magician.png")]
    [InlineData("minor.wands.knight", "cards/minor/wands/knight.png")]
    [InlineData("minor.cups.page", "cards/minor/cups/page.png")]
    [InlineData("major.devil", "cards/major/devil.png")]
    [InlineData("minor.cups.three", "cards/minor/cups/three.png")]
    [InlineData("minor.swords.three", "cards/minor/swords/three.png")]
    [InlineData("major.empress", "cards/major/empress.png")]
    [InlineData("minor.swords.four", "cards/minor/swords/four.png")]
    [InlineData("minor.wands.eight", "cards/minor/wands/eight.png")]
    [InlineData("major.wheel-of-fortune", "cards/major/wheel-of-fortune.png")]
    [InlineData("minor.cups.king", "cards/minor/cups/king.png")]
    [InlineData("minor.pentacles.seven", "cards/minor/pentacles/seven.png")]
    [InlineData("major.hierophant", "cards/major/hierophant.png")]
    [InlineData("minor.cups.nine", "cards/minor/cups/nine.png")]
    [InlineData("minor.swords.queen", "cards/minor/swords/queen.png")]
    [InlineData("major.world", "cards/major/world.png")]
    [InlineData("minor.wands.six", "cards/minor/wands/six.png")]
    [InlineData("minor.cups.two", "cards/minor/cups/two.png")]
    public void PartialPackResolver_AllFiftySevenAcceptedCardsResolveRasterArtwork(string cardId, string assetPath)
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

        Assert.Equal(57, resolutions.Count(static resolution => resolution.Kind == TarotArtworkResolutionKind.Raster));
        Assert.Equal(21, resolutions.Count(static resolution => resolution.IsPartialPackFallback));
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
    public void PartialPackResolver_PromotedWorldUsesRasterArtwork()
    {
        var catalog = TarotArtworkPackCatalog.CreateForTests(TarotArtworkPackTestData.LoadRepositoryPack());
        var card = StandardTarotCatalog.Deck.Cards.Single(candidate => candidate.Id.Value == "major.world");

        var resolution = catalog.Resolve(new TarotArtworkPackId("lupus-noctis"), card);

        Assert.Same(card, resolution.Card);
        Assert.Equal("major.world", resolution.Card.Id.Value);
        Assert.Equal(TarotArtworkResolutionKind.Raster, resolution.Kind);
        Assert.False(resolution.IsPartialPackFallback);
        Assert.NotNull(resolution.RasterAsset);
        Assert.Equal("cards/major/world.png", resolution.RasterAsset!.AssetPath);
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
        var card = StandardTarotCatalog.Deck.Cards.Single(candidate => candidate.Id.Value == "minor.wands.ace");
        var artwork = catalog.Resolve(new TarotArtworkPackId("lupus-noctis"), card);
        var assignment = new TarotDrawnCard(new TarotSpreadPositionId("card"), card, TarotCardOrientation.Upright);

        var plan = TarotCardVisualPlan.Create(
            assignment,
            artwork,
            "Ace of Wands",
            "Wands",
            "Prototype fallback");

        Assert.Same(card, plan.Card);
        Assert.Equal(TarotArtworkResolutionKind.Prototype, plan.ArtworkKind);
        Assert.Null(plan.RasterAssetPath);
        Assert.Equal("Ace of Wands", plan.LocalizedTitle);
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
