using System.Buffers.Binary;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using NoxAeterna.Domain.Tarot;
using NoxAeterna.Presentation.Localization;

namespace NoxAeterna.App.Tarot;

/// <summary>Supplies a built-in artwork-pack manifest and its constrained package-relative assets.</summary>
public interface ITarotArtworkPackResourceSource
{
    /// <summary>Opens the single manifest owned by this source.</summary>
    Stream OpenManifest();

    /// <summary>Opens one previously validated package-relative card asset, or returns null when absent.</summary>
    Stream? OpenAsset(string validatedRelativePath);
}

/// <summary>Reports a controlled, contextual built-in artwork-pack validation failure.</summary>
public sealed class TarotArtworkPackLoadException(string message, Exception? innerException = null)
    : Exception(message, innerException);

/// <summary>Represents one validated, accepted raster card asset.</summary>
public sealed class TarotArtworkPackCardAsset
{
    private readonly byte[] content;

    internal TarotArtworkPackCardAsset(
        TarotCardDefinition card,
        string assetPath,
        int width,
        int height,
        string sha256,
        string generationProvenanceReference,
        byte[] content)
    {
        Card = card;
        AssetPath = assetPath;
        Width = width;
        Height = height;
        Sha256 = sha256;
        GenerationProvenanceReference = generationProvenanceReference;
        this.content = content;
    }

    public TarotCardDefinition Card { get; }

    public string AssetPath { get; }

    public int Width { get; }

    public int Height { get; }

    public string Sha256 { get; }

    public string Status => "accepted";

    public string GenerationProvenanceReference { get; }

    public Stream OpenRead() => new MemoryStream(content, writable: false);
}

/// <summary>Immutable validated definition of one shipped artwork pack.</summary>
public sealed class TarotArtworkPackDefinition
{
    private readonly IReadOnlyDictionary<TarotCardId, TarotArtworkPackCardAsset> cardsById;

    internal TarotArtworkPackDefinition(
        int schemaVersion,
        TarotArtworkPackId id,
        TarotDeckId semanticDeckId,
        LocalizationKey displayNameLocalizationKey,
        int aspectRatioWidth,
        int aspectRatioHeight,
        int sourceWidth,
        int sourceHeight,
        bool isPartial,
        TarotArtworkPackCardAsset[] cards)
    {
        SchemaVersion = schemaVersion;
        Id = id;
        SemanticDeckId = semanticDeckId;
        DisplayNameLocalizationKey = displayNameLocalizationKey;
        AspectRatioWidth = aspectRatioWidth;
        AspectRatioHeight = aspectRatioHeight;
        SourceWidth = sourceWidth;
        SourceHeight = sourceHeight;
        IsPartial = isPartial;
        Cards = Array.AsReadOnly(cards);
        cardsById = new ReadOnlyDictionary<TarotCardId, TarotArtworkPackCardAsset>(
            cards.ToDictionary(static card => card.Card.Id));
    }

    public int SchemaVersion { get; }

    public TarotArtworkPackId Id { get; }

    public TarotDeckId SemanticDeckId { get; }

    public LocalizationKey DisplayNameLocalizationKey { get; }

    public int AspectRatioWidth { get; }

    public int AspectRatioHeight { get; }

    public int SourceWidth { get; }

    public int SourceHeight { get; }

    public bool IsPartial { get; }

    public IReadOnlyList<TarotArtworkPackCardAsset> Cards { get; }

    public bool TryGetCard(TarotCardId cardId, out TarotArtworkPackCardAsset? asset) =>
        cardsById.TryGetValue(cardId, out asset);
}

/// <summary>Parses and fully validates built-in artwork manifests and declared raster resources.</summary>
public static class TarotArtworkPackLoader
{
    private const int SupportedSchemaVersion = 1;
    private static readonly byte[] PngSignature = [137, 80, 78, 71, 13, 10, 26, 10];

    public static TarotArtworkPackDefinition Load(
        ITarotArtworkPackResourceSource source,
        TarotDeckDefinition semanticDeck)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(semanticDeck);

        ArtworkPackManifestDto manifest;
        try
        {
            using var manifestStream = source.OpenManifest();
            manifest = JsonSerializer.Deserialize<ArtworkPackManifestDto>(manifestStream)
                ?? throw new TarotArtworkPackLoadException("The artwork-pack manifest is empty.");
        }
        catch (TarotArtworkPackLoadException)
        {
            throw;
        }
        catch (Exception exception) when (exception is JsonException or IOException or UnauthorizedAccessException)
        {
            throw new TarotArtworkPackLoadException("The built-in artwork-pack manifest could not be read.", exception);
        }

        ValidateTopLevel(manifest, semanticDeck);
        var semanticCards = semanticDeck.Cards.ToDictionary(static card => card.Id);
        var seenCardIds = new HashSet<TarotCardId>();
        var seenPaths = new HashSet<string>(StringComparer.Ordinal);
        var cards = new List<TarotArtworkPackCardAsset>();

        foreach (var entry in manifest.Cards!)
        {
            var cardId = CreateCardId(entry.CardId);
            if (!seenCardIds.Add(cardId))
            {
                throw new TarotArtworkPackLoadException($"Duplicate artwork card ID '{cardId.Value}'.");
            }

            if (!semanticCards.TryGetValue(cardId, out var semanticCard))
            {
                throw new TarotArtworkPackLoadException($"Unknown semantic card ID '{cardId.Value}'.");
            }

            var assetPath = ValidatePackageRelativePath(entry.AssetPath);
            if (!seenPaths.Add(assetPath))
            {
                throw new TarotArtworkPackLoadException($"Duplicate artwork asset path '{assetPath}'.");
            }

            ValidateCardMetadata(manifest, entry, cardId);
            byte[] content;
            try
            {
                using var assetStream = source.OpenAsset(assetPath)
                    ?? throw new TarotArtworkPackLoadException(
                        $"Accepted artwork asset '{assetPath}' for '{cardId.Value}' is missing.");
                using var buffer = new MemoryStream();
                assetStream.CopyTo(buffer);
                content = buffer.ToArray();
            }
            catch (TarotArtworkPackLoadException)
            {
                throw;
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                throw new TarotArtworkPackLoadException(
                    $"Accepted artwork asset '{assetPath}' for '{cardId.Value}' could not be read.",
                    exception);
            }

            var (actualWidth, actualHeight) = ReadPngDimensions(content, assetPath);
            if (actualWidth != entry.Width || actualHeight != entry.Height)
            {
                throw new TarotArtworkPackLoadException(
                    $"Artwork asset '{assetPath}' dimensions {actualWidth}x{actualHeight} do not match manifest dimensions {entry.Width}x{entry.Height}.");
            }

            var actualSha256 = Convert.ToHexStringLower(SHA256.HashData(content));
            if (!string.Equals(actualSha256, entry.Sha256, StringComparison.Ordinal))
            {
                throw new TarotArtworkPackLoadException($"Artwork asset '{assetPath}' SHA-256 does not match the manifest.");
            }

            cards.Add(new TarotArtworkPackCardAsset(
                semanticCard,
                assetPath,
                entry.Width,
                entry.Height,
                actualSha256,
                entry.GenerationProvenanceReference!,
                content));
        }

        if (!manifest.PartialPack && cards.Count != semanticDeck.Cards.Count)
        {
            throw new TarotArtworkPackLoadException(
                $"A complete artwork pack must declare {semanticDeck.Cards.Count} cards, but declared {cards.Count}.");
        }

        return new TarotArtworkPackDefinition(
            manifest.SchemaVersion,
            new TarotArtworkPackId(manifest.ArtworkPackId!),
            new TarotDeckId(manifest.SemanticDeckId!),
            new LocalizationKey(manifest.DisplayNameLocalizationKey!),
            manifest.CanonicalAspectRatio!.Width,
            manifest.CanonicalAspectRatio.Height,
            manifest.ExpectedSourceDimensions!.Width,
            manifest.ExpectedSourceDimensions.Height,
            manifest.PartialPack,
            cards.ToArray());
    }

    public static string ValidatePackageRelativePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) ||
            Path.IsPathRooted(path) ||
            path.StartsWith('/') ||
            path.StartsWith('\\') ||
            path.Contains(':'))
        {
            throw new TarotArtworkPackLoadException("Artwork asset paths must be non-empty package-relative paths.");
        }

        var segments = path.Replace('\\', '/').Split('/', StringSplitOptions.None);
        if (segments.Any(static segment => string.IsNullOrWhiteSpace(segment) || segment is "." or ".."))
        {
            throw new TarotArtworkPackLoadException($"Artwork asset path '{path}' contains traversal or empty segments.");
        }

        var normalized = string.Join('/', segments);
        if (!normalized.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
        {
            throw new TarotArtworkPackLoadException($"Artwork asset path '{path}' must identify a PNG file.");
        }

        return normalized;
    }

    private static void ValidateTopLevel(ArtworkPackManifestDto manifest, TarotDeckDefinition semanticDeck)
    {
        if (manifest.SchemaVersion != SupportedSchemaVersion)
        {
            throw new TarotArtworkPackLoadException(
                $"Unsupported artwork-pack schema version '{manifest.SchemaVersion}'.");
        }

        _ = new TarotArtworkPackId(Require(manifest.ArtworkPackId, "artworkPackId"));
        var deckId = new TarotDeckId(Require(manifest.SemanticDeckId, "semanticDeckId"));
        if (deckId != semanticDeck.Id)
        {
            throw new TarotArtworkPackLoadException(
                $"Artwork pack targets semantic deck '{deckId.Value}', expected '{semanticDeck.Id.Value}'.");
        }

        _ = new LocalizationKey(Require(manifest.DisplayNameLocalizationKey, "displayNameLocalizationKey"));
        var ratio = manifest.CanonicalAspectRatio
            ?? throw new TarotArtworkPackLoadException("Manifest field 'canonicalAspectRatio' is required.");
        var dimensions = manifest.ExpectedSourceDimensions
            ?? throw new TarotArtworkPackLoadException("Manifest field 'expectedSourceDimensions' is required.");
        if (ratio.Width != 7 || ratio.Height != 12)
        {
            throw new TarotArtworkPackLoadException("The canonical artwork aspect ratio must be 7:12.");
        }

        if (dimensions.Width != 952 || dimensions.Height != 1632 ||
            dimensions.Width * ratio.Height != dimensions.Height * ratio.Width)
        {
            throw new TarotArtworkPackLoadException("The expected artwork source dimensions must be 952x1632 at 7:12.");
        }

        if (manifest.Cards is null || manifest.Cards.Count == 0)
        {
            throw new TarotArtworkPackLoadException("The artwork-pack manifest must declare at least one card.");
        }
    }

    private static void ValidateCardMetadata(
        ArtworkPackManifestDto manifest,
        ArtworkPackCardDto entry,
        TarotCardId cardId)
    {
        var dimensions = manifest.ExpectedSourceDimensions!;
        var ratio = manifest.CanonicalAspectRatio!;
        if (entry.Width != dimensions.Width || entry.Height != dimensions.Height ||
            entry.Width * ratio.Height != entry.Height * ratio.Width)
        {
            throw new TarotArtworkPackLoadException(
                $"Artwork card '{cardId.Value}' must declare {dimensions.Width}x{dimensions.Height} at {ratio.Width}:{ratio.Height}.");
        }

        if (!string.Equals(entry.Status, "accepted", StringComparison.Ordinal))
        {
            throw new TarotArtworkPackLoadException($"Artwork card '{cardId.Value}' must have accepted status.");
        }

        if (entry.Sha256 is null || entry.Sha256.Length != 64 ||
            entry.Sha256.Any(static character => !Uri.IsHexDigit(character)) ||
            !string.Equals(entry.Sha256, entry.Sha256.ToLowerInvariant(), StringComparison.Ordinal))
        {
            throw new TarotArtworkPackLoadException($"Artwork card '{cardId.Value}' has an invalid lowercase SHA-256.");
        }

        _ = Require(entry.GenerationProvenanceReference, "generationProvenanceReference");
    }

    private static TarotCardId CreateCardId(string? value)
    {
        try
        {
            return new TarotCardId(Require(value, "cardId"));
        }
        catch (ArgumentException exception)
        {
            throw new TarotArtworkPackLoadException("Manifest cardId is invalid.", exception);
        }
    }

    private static string Require(string? value, string fieldName) =>
        !string.IsNullOrWhiteSpace(value)
            ? value
            : throw new TarotArtworkPackLoadException($"Manifest field '{fieldName}' is required.");

    private static (int Width, int Height) ReadPngDimensions(byte[] content, string assetPath)
    {
        if (content.Length < 24 || !content.AsSpan(0, 8).SequenceEqual(PngSignature) ||
            !content.AsSpan(12, 4).SequenceEqual("IHDR"u8))
        {
            throw new TarotArtworkPackLoadException($"Artwork asset '{assetPath}' is not a valid PNG header.");
        }

        var width = BinaryPrimitives.ReadInt32BigEndian(content.AsSpan(16, 4));
        var height = BinaryPrimitives.ReadInt32BigEndian(content.AsSpan(20, 4));
        if (width <= 0 || height <= 0)
        {
            throw new TarotArtworkPackLoadException($"Artwork asset '{assetPath}' has invalid PNG dimensions.");
        }

        return (width, height);
    }

    private sealed class ArtworkPackManifestDto
    {
        [JsonPropertyName("schemaVersion")]
        public int SchemaVersion { get; init; }

        [JsonPropertyName("artworkPackId")]
        public string? ArtworkPackId { get; init; }

        [JsonPropertyName("semanticDeckId")]
        public string? SemanticDeckId { get; init; }

        [JsonPropertyName("displayNameLocalizationKey")]
        public string? DisplayNameLocalizationKey { get; init; }

        [JsonPropertyName("canonicalAspectRatio")]
        public SizeDto? CanonicalAspectRatio { get; init; }

        [JsonPropertyName("expectedSourceDimensions")]
        public SizeDto? ExpectedSourceDimensions { get; init; }

        [JsonPropertyName("partialPack")]
        public bool PartialPack { get; init; }

        [JsonPropertyName("cards")]
        public List<ArtworkPackCardDto>? Cards { get; init; }
    }

    private sealed class SizeDto
    {
        [JsonPropertyName("width")]
        public int Width { get; init; }

        [JsonPropertyName("height")]
        public int Height { get; init; }
    }

    private sealed class ArtworkPackCardDto
    {
        [JsonPropertyName("cardId")]
        public string? CardId { get; init; }

        [JsonPropertyName("assetPath")]
        public string? AssetPath { get; init; }

        [JsonPropertyName("width")]
        public int Width { get; init; }

        [JsonPropertyName("height")]
        public int Height { get; init; }

        [JsonPropertyName("sha256")]
        public string? Sha256 { get; init; }

        [JsonPropertyName("status")]
        public string? Status { get; init; }

        [JsonPropertyName("generationProvenanceReference")]
        public string? GenerationProvenanceReference { get; init; }
    }
}
