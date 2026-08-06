using System.Security.Cryptography;
using NoxAeterna.Domain.Tarot;
using NoxAeterna.Interpretation.Tarot.Contracts;
using NoxAeterna.Interpretation.Tarot.Keys;
using NoxAeterna.Interpretation.Tarot.Serialization;
using NoxAeterna.Interpretation.Tarot.Sources;

namespace NoxAeterna.Tests.Interpretation.Tarot;

internal sealed class TarotInterpretationResolverTestSource :
    ITarotInterpretationPackSource,
    ITarotInterpretationPackSourceCatalog
{
    private readonly Dictionary<string, byte[]> files = new(StringComparer.Ordinal);
    private readonly List<string> reads = [];

    public TarotInterpretationResolverTestSource(string packId = "classic")
    {
        PackId = new TarotInterpretationPackId(packId);
        Manifest = CreateManifest(packId);
        PublishManifest();
    }

    public TarotInterpretationPackId PackId { get; }
    public string SnapshotId { get; private set; } = "snapshot-1";
    public TarotInterpretationPackDocument Manifest { get; }
    public IReadOnlyList<string> Reads => reads;

    public int ReadCount(string path) => reads.Count(item => item == path);

    public void SetReady(TarotInterpretationMode mode, string locale, bool ready = true) =>
        Manifest.Modules![TarotSchemaText.Get(mode, TarotSchemaText.Modes)]![locale]!.Ready = ready;

    public void AddSingleCardCorpus(
        string locale,
        TarotSingleCardDocument? target = null,
        string? targetPath = null,
        string? indexLocale = null)
    {
        target ??= SyntheticSingleCard();
        var targetKey = TarotInterpretationKeys.CreateSingleCard(
            new TarotCardId(target.CardId!),
            target.Orientation!.Value);
        targetPath ??= $"content/{locale}/modes/single-card/{target.CardId}/{TarotSchemaText.Get(target.Orientation.Value, TarotSchemaText.CardOrientations)}.json";
        var targetBytes = TarotInterpretationJson.Serialize(target);
        files[targetPath] = targetBytes;

        var entries = StandardTarotCatalog.Deck.Cards
            .SelectMany(card => Enum.GetValues<TarotCardOrientation>().Select(orientation =>
            {
                var key = TarotInterpretationKeys.CreateSingleCard(card.Id, orientation);
                var path = key == targetKey
                    ? targetPath
                    : $"content/{locale}/modes/single-card/{card.Id.Value}/{TarotSchemaText.Get(orientation, TarotSchemaText.CardOrientations)}.json";
                return Entry(key, path, key == targetKey ? Hash(targetBytes) : TarotInterpretationTestDocuments.Hash);
            }))
            .OrderBy(item => item.Key, StringComparer.Ordinal)
            .Cast<TarotGeneratedIndexEntryDocument?>()
            .ToList();
        AddIndex(locale, TarotInterpretationCorpus.SingleCard, entries, indexLocale: indexLocale);
    }

    public void AddOrientedPairCorpus(
        string locale,
        TarotOrientedPairDocument? target = null,
        string? targetPath = null,
        string? indexLocale = null)
    {
        target ??= SyntheticPair();
        var targetKey = TarotInterpretationKeys.CreateOrientedPair(
            new TarotCardId(target.CardAId!),
            new TarotCardId(target.CardBId!),
            target.OrientationState!.Value);
        targetPath ??= $"content/{locale}/shared/oriented-pairs/{target.CardAId}__{target.CardBId}/{TarotSchemaText.Get(target.OrientationState.Value, TarotSchemaText.PairStates)}.json";
        var targetBytes = TarotInterpretationJson.Serialize(target);
        files[targetPath] = targetBytes;

        var cards = StandardTarotCatalog.Deck.Cards.Select(card => card.Id)
            .OrderBy(card => card.Value, StringComparer.Ordinal).ToArray();
        var entries = new List<TarotGeneratedIndexEntryDocument?>(12012);
        for (var first = 0; first < cards.Length - 1; first++)
        {
            for (var second = first + 1; second < cards.Length; second++)
            {
                foreach (var state in Enum.GetValues<TarotOrientedPairState>())
                {
                    var key = TarotInterpretationKeys.CreateOrientedPair(cards[first], cards[second], state);
                    var path = key == targetKey
                        ? targetPath
                        : $"content/{locale}/shared/oriented-pairs/{cards[first].Value}__{cards[second].Value}/{TarotSchemaText.Get(state, TarotSchemaText.PairStates)}.json";
                    entries.Add(Entry(key, path, key == targetKey ? Hash(targetBytes) : TarotInterpretationTestDocuments.Hash));
                }
            }
        }

        AddIndex(
            locale,
            TarotInterpretationCorpus.OrientedPairs,
            entries.OrderBy(item => item!.Key, StringComparer.Ordinal).ToList(),
            expectedIdentityCount: 3003,
            indexLocale: indexLocale);
    }

    public void AddThreeCardCorpus(
        string locale,
        TarotThreeCardPositionDocument? target = null,
        string? targetPath = null,
        string? indexLocale = null)
    {
        target ??= SyntheticPosition();
        var targetKey = TarotInterpretationKeys.CreateThreeCardPosition(
            target.Position!.Value,
            new TarotCardId(target.CardId!),
            target.Orientation!.Value);
        targetPath ??= $"content/{locale}/modes/three-cards/positions/{TarotSchemaText.Get(target.Position.Value, TarotSchemaText.Positions)}/{target.CardId}/{TarotSchemaText.Get(target.Orientation.Value, TarotSchemaText.CardOrientations)}.json";
        var targetBytes = TarotInterpretationJson.Serialize(target);
        files[targetPath] = targetBytes;

        var entries = StandardTarotCatalog.Deck.Cards
            .SelectMany(card => Enum.GetValues<TarotThreeCardPosition>().SelectMany(position =>
                Enum.GetValues<TarotCardOrientation>().Select(orientation =>
                {
                    var key = TarotInterpretationKeys.CreateThreeCardPosition(position, card.Id, orientation);
                    var path = key == targetKey
                        ? targetPath
                        : $"content/{locale}/modes/three-cards/positions/{TarotSchemaText.Get(position, TarotSchemaText.Positions)}/{card.Id.Value}/{TarotSchemaText.Get(orientation, TarotSchemaText.CardOrientations)}.json";
                    return Entry(key, path, key == targetKey ? Hash(targetBytes) : TarotInterpretationTestDocuments.Hash);
                })))
            .OrderBy(item => item.Key, StringComparer.Ordinal)
            .Cast<TarotGeneratedIndexEntryDocument?>()
            .ToList();
        AddIndex(
            locale,
            TarotInterpretationCorpus.ThreeCards,
            entries,
            expectedPositionCount: 468,
            indexLocale: indexLocale);
    }

    public void PublishManifest(bool advanceSnapshot = false)
    {
        Manifest.IndexFiles = files
            .Where(item => item.Key.StartsWith("indexes/", StringComparison.Ordinal))
            .OrderBy(item => item.Key, StringComparer.Ordinal)
            .Select(item => (TarotInterpretationIndexFileDocument?)new TarotInterpretationIndexFileDocument
            {
                Path = item.Key,
                Sha256 = Hash(item.Value)
            })
            .ToList();
        files["interpretation-pack.json"] = TarotInterpretationJson.Serialize(Manifest);
        if (advanceSnapshot)
        {
            var number = int.Parse(SnapshotId.AsSpan("snapshot-".Length)) + 1;
            SnapshotId = $"snapshot-{number}";
        }
    }

    public void Remove(string path) => files.Remove(path);

    public void Replace(string path, byte[] bytes) => files[path] = bytes;

    public byte[] Get(string path) => files[path].ToArray();

    public void ClearReads() => reads.Clear();

    public TarotInterpretationSourceReadResult ReadManifest() => Read("interpretation-pack.json");

    public TarotInterpretationSourceReadResult ReadPackageFile(TarotPackageRelativePath path) => Read(path.Value);

    public bool TryGetSource(TarotInterpretationPackId packId, out ITarotInterpretationPackSource? source)
    {
        source = packId == PackId ? this : null;
        return source is not null;
    }

    public static TarotSingleCardDocument SyntheticSingleCard(
        string cardId = "major.fool",
        TarotCardOrientation orientation = TarotCardOrientation.Upright) => new()
    {
        SchemaVersion = 1,
        CardId = cardId,
        Orientation = orientation,
        Sections = new(StringComparer.Ordinal)
        {
            ["situation"] = $"Synthetic situation {cardId} {orientation}",
            ["development"] = $"Synthetic development {cardId} {orientation}",
            ["risk"] = $"Synthetic risk {cardId} {orientation}",
            ["outcome"] = $"Synthetic outcome {cardId} {orientation}",
            ["advice"] = $"Synthetic advice {cardId} {orientation}"
        },
        Tags = Tags(5),
        OverallValence = 0,
        OverallIntensity = 2,
        ReversalMechanisms = orientation == TarotCardOrientation.Upright ? [] : [TarotReversalMechanism.Blocked]
    };

    public static TarotOrientedPairDocument SyntheticPair(
        string cardA = "major.tower",
        string cardB = "major.world",
        TarotOrientedPairState state = TarotOrientedPairState.ReversedUpright) => new()
    {
        SchemaVersion = 1,
        CardAId = cardA,
        CardBId = cardB,
        OrientationState = state,
        Interaction = $"Synthetic interaction {cardA} {cardB}",
        Direction = $"Synthetic direction {cardA} {cardB}",
        Tags = Tags(6),
        OverallValence = -1,
        OverallIntensity = 3
    };

    public static TarotThreeCardPositionDocument SyntheticPosition(
        TarotThreeCardPosition position = TarotThreeCardPosition.Past,
        string cardId = "major.fool",
        TarotCardOrientation orientation = TarotCardOrientation.Upright) => new()
    {
        SchemaVersion = 1,
        Position = position,
        CardId = cardId,
        Orientation = orientation,
        Text = $"Synthetic position {position} {cardId} {orientation}",
        Tags = Tags(1),
        OverallValence = 0,
        OverallIntensity = 2
    };

    private TarotInterpretationSourceReadResult Read(string path)
    {
        reads.Add(path);
        return files.TryGetValue(path, out var bytes)
            ? TarotInterpretationSourceReadResult.Found(bytes)
            : TarotInterpretationSourceReadResult.Missing();
    }

    private void AddIndex(
        string pathLocale,
        TarotInterpretationCorpus corpus,
        List<TarotGeneratedIndexEntryDocument?> entries,
        int? expectedIdentityCount = null,
        int? expectedPositionCount = null,
        string? indexLocale = null)
    {
        var name = TarotSchemaText.Get(corpus, TarotSchemaText.Corpora);
        var path = $"indexes/{pathLocale}/{name}.json";
        var document = new TarotGeneratedIndexDocument
        {
            SchemaVersion = 1,
            PackId = PackId.Value,
            Locale = indexLocale ?? pathLocale,
            CorpusId = corpus,
            ContentVersion = Manifest.ContentVersion,
            ExpectedEntryCount = entries.Count,
            ExpectedIdentityCount = expectedIdentityCount,
            ExpectedPositionEntryCount = expectedPositionCount,
            Entries = entries
        };
        files[path] = TarotInterpretationJson.Serialize(document);
    }

    private static TarotGeneratedIndexEntryDocument Entry(string key, string path, string hash) => new()
    {
        Key = key,
        Path = path,
        Sha256 = hash
    };

    private static List<TarotTagAssignmentDocument?> Tags(int count) => Enumerable.Range(1, count)
        .Select(index => (TarotTagAssignmentDocument?)new TarotTagAssignmentDocument
        {
            ConceptId = $"synthetic-{index}",
            Valence = 0,
            Intensity = Math.Min(index, 3)
        }).ToList();

    private static string Hash(ReadOnlySpan<byte> bytes) => Convert.ToHexStringLower(SHA256.HashData(bytes));

    private static TarotInterpretationPackDocument CreateManifest(string packId)
    {
        var document = TarotInterpretationTestDocuments.Manifest();
        document.PackId = packId;
        return document;
    }
}

internal sealed class TarotInterpretationTestCatalog(params ITarotInterpretationPackSource[] sources)
    : ITarotInterpretationPackSourceCatalog
{
    private readonly IReadOnlyDictionary<TarotInterpretationPackId, ITarotInterpretationPackSource> items =
        sources.ToDictionary(item => item.PackId);

    public bool TryGetSource(TarotInterpretationPackId packId, out ITarotInterpretationPackSource? source) =>
        items.TryGetValue(packId, out source);
}
