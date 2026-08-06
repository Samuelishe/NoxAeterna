using System.Collections.ObjectModel;
using NoxAeterna.Domain.Tarot;
using NoxAeterna.Presentation.Tarot;

namespace NoxAeterna.App.Tarot;

public enum TarotArtworkResolutionKind
{
    Prototype,
    Raster
}

/// <summary>Describes the visual source for one unchanged semantic card.</summary>
public sealed record TarotArtworkResolution(
    TarotCardDefinition Card,
    TarotArtworkResolutionKind Kind,
    TarotArtworkPackCardAsset? RasterAsset,
    bool IsPartialPackFallback);

/// <summary>Owns built-in artwork definitions and an explicit internal prototype resolution seam.</summary>
public sealed class TarotArtworkPackCatalog
{
    private readonly IReadOnlyDictionary<TarotArtworkPackId, TarotArtworkPackDefinition> packs;

    private TarotArtworkPackCatalog(
        IReadOnlyDictionary<TarotArtworkPackId, TarotArtworkPackDefinition> packs,
        string[] diagnostics)
    {
        this.packs = packs;
        Diagnostics = Array.AsReadOnly(diagnostics);
        AvailableOptions = TarotPrototypeSelections.ArtworkPacks;
        IsReady = packs.ContainsKey(TarotPrototypeSelections.LupusNoctisArtworkPackId);
    }

    public IReadOnlyList<TarotArtworkPackOption> AvailableOptions { get; }

    public IReadOnlyList<string> Diagnostics { get; }

    public bool IsReady { get; }

    public static TarotArtworkPackCatalog CreateBuiltIn() => CreateFromSource(
        new BuiltInLupusNoctisResourceSource());

    public static TarotArtworkPackCatalog CreateFromSource(ITarotArtworkPackResourceSource source)
    {
        ArgumentNullException.ThrowIfNull(source);
        var packs = new Dictionary<TarotArtworkPackId, TarotArtworkPackDefinition>();
        var diagnostics = new List<string>();
        try
        {
            var pack = TarotArtworkPackLoader.Load(
                source,
                StandardTarotCatalog.Deck);
            packs.Add(pack.Id, pack);
        }
        catch (TarotArtworkPackLoadException exception)
        {
            diagnostics.Add($"Lupus Noctis artwork pack was disabled: {exception.Message}");
        }

        return new TarotArtworkPackCatalog(
            new ReadOnlyDictionary<TarotArtworkPackId, TarotArtworkPackDefinition>(packs),
            diagnostics.ToArray());
    }

    public static TarotArtworkPackCatalog CreateForTests(TarotArtworkPackDefinition definition) =>
        new(
            new ReadOnlyDictionary<TarotArtworkPackId, TarotArtworkPackDefinition>(
                new Dictionary<TarotArtworkPackId, TarotArtworkPackDefinition>
                {
                    [definition.Id] = definition
                }),
            []);

    public TarotArtworkResolution Resolve(TarotArtworkPackId artworkPackId, TarotCardDefinition card)
    {
        ArgumentNullException.ThrowIfNull(artworkPackId);
        ArgumentNullException.ThrowIfNull(card);

        if (artworkPackId == TarotPrototypeSelections.PrototypeArtworkPackId)
        {
            return new TarotArtworkResolution(card, TarotArtworkResolutionKind.Prototype, null, false);
        }

        if (!packs.TryGetValue(artworkPackId, out var pack))
        {
            throw new ArgumentException("The artwork pack is not available.", nameof(artworkPackId));
        }

        if (pack.TryGetCard(card.Id, out var asset))
        {
            return new TarotArtworkResolution(card, TarotArtworkResolutionKind.Raster, asset, false);
        }

        if (!pack.IsPartial)
        {
            throw new InvalidOperationException(
                $"Complete artwork pack '{pack.Id.Value}' does not define '{card.Id.Value}'.");
        }

        return new TarotArtworkResolution(card, TarotArtworkResolutionKind.Prototype, null, true);
    }

    private sealed class BuiltInLupusNoctisResourceSource : ITarotArtworkPackResourceSource
    {
        private const string PackOutputPath = "resources/assets/tarot/artwork-packs/lupus-noctis";

        public Stream OpenManifest() => OpenRequired("artwork-pack.json");

        public Stream? OpenAsset(string validatedRelativePath)
        {
            var normalized = TarotArtworkPackLoader.ValidatePackageRelativePath(validatedRelativePath);
            var fullPath = ResolveWithinPack(normalized);
            return File.Exists(fullPath)
                ? File.Open(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read)
                : null;
        }

        private static Stream OpenRequired(string validatedRelativePath)
        {
            var fullPath = ResolveWithinPack(validatedRelativePath);
            return File.Open(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        private static string ResolveWithinPack(string relativePath)
        {
            var packRoot = Path.GetFullPath(Path.Combine(
                AppContext.BaseDirectory,
                PackOutputPath.Replace('/', Path.DirectorySeparatorChar)));
            var fullPath = Path.GetFullPath(Path.Combine(
                packRoot,
                relativePath.Replace('/', Path.DirectorySeparatorChar)));
            if (!fullPath.StartsWith(packRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            {
                throw new TarotArtworkPackLoadException("Artwork resource path escaped the built-in pack root.");
            }

            return fullPath;
        }
    }
}
