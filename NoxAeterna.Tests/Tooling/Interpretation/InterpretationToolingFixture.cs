using NoxAeterna.Domain.Tarot;
using NoxAeterna.Interpretation.Tarot.Contracts;
using NoxAeterna.Interpretation.Tarot.Serialization;
using System.Text.Json;

namespace NoxAeterna.Tests.Tooling.Interpretation;

internal sealed class InterpretationToolingFixture : IDisposable
{
    private InterpretationToolingFixture(string root) => Root = root;

    public string Root { get; }
    public string ManifestPath => Path.Combine(Root, "interpretation-pack.json");

    public static InterpretationToolingFixture CreateSkeleton(bool reverseCreationOrder = false)
    {
        var root = Path.Combine(Path.GetTempPath(), $"NoxAeterna-source-{Guid.NewGuid():N}");
        var fixture = new InterpretationToolingFixture(root);
        Directory.CreateDirectory(root);
        if (reverseCreationOrder)
        {
            fixture.WriteLabels("en");
            fixture.WriteLabels("ru");
            fixture.Write("interpretation-pack.json", Manifest());
        }
        else
        {
            fixture.Write("interpretation-pack.json", Manifest());
            fixture.WriteLabels("ru");
            fixture.WriteLabels("en");
        }
        return fixture;
    }

    public void AddVocabulary(string locale, string conceptId) => Write(
        $"content/{locale}/vocabulary/{conceptId}.json",
        new TarotVocabularyDocument { SchemaVersion = 1, ConceptId = conceptId, Label = $"Label {conceptId}", Meaning = $"Meaning {conceptId}" });

    public void AddSingle(string locale, string cardId, string? filename = null)
        => AddSingleCore(locale, cardId, filename, conceptId: null);

    public void AddTaggedSingle(string locale, string cardId, string conceptId)
        => AddSingleCore(locale, cardId, filename: null, conceptId);

    public void AddSingleStates(
        string locale,
        string cardId,
        TarotSingleCardStateDocument upright,
        TarotSingleCardStateDocument reversed,
        string? filename = null)
    {
        Write($"content/{locale}/single-card/{filename ?? cardId}.json", new TarotSingleCardBundleDocument
        {
            SchemaVersion = 1,
            CardId = cardId,
            States = new Dictionary<string, TarotSingleCardStateDocument?>(StringComparer.Ordinal)
            {
                ["upright"] = upright,
                ["reversed"] = reversed
            }
        });
    }

    public static TarotSingleCardStateDocument CreateSingleState(
        IReadOnlyDictionary<string, string> sections,
        IReadOnlyList<TarotTagAssignmentDocument>? tags = null,
        int overallValence = 0,
        int overallIntensity = 2,
        IReadOnlyList<TarotReversalMechanism>? reversalMechanisms = null) => new()
    {
        Sections = sections.ToDictionary(pair => pair.Key, pair => (string?)pair.Value, StringComparer.Ordinal),
        Tags = tags?.Select(tag => (TarotTagAssignmentDocument?)tag).ToList() ?? [],
        OverallValence = overallValence,
        OverallIntensity = overallIntensity,
        ReversalMechanisms = reversalMechanisms?.Select(mechanism => (TarotReversalMechanism?)mechanism).ToList() ?? []
    };

    public void AddSynthesis(string locale, TarotSynthesisResourceType resourceType, string resourceTypePath, string resourceId)
    {
        using var data = JsonDocument.Parse("{\"kind\":\"fixture\"}");
        Write($"content/{locale}/synthesis/{resourceTypePath}/{resourceId}.json", new TarotSynthesisResourceDocument
        {
            SchemaVersion = 1, ResourceType = resourceType, ResourceId = resourceId, Data = data.RootElement.Clone()
        });
    }

    private void AddSingleCore(string locale, string cardId, string? filename, string? conceptId)
    {
        var states = new Dictionary<string, TarotSingleCardStateDocument?>(StringComparer.Ordinal)
        {
            ["upright"] = SingleState(false, conceptId),
            ["reversed"] = SingleState(true, conceptId)
        };
        Write($"content/{locale}/single-card/{filename ?? cardId}.json", new TarotSingleCardBundleDocument
        {
            SchemaVersion = 1, CardId = cardId, States = states
        });
    }

    public void AddPair(string locale, string cardAId, string cardBId, string? filename = null)
    {
        var states = new Dictionary<string, TarotOrientedPairStateDocument?>(StringComparer.Ordinal);
        foreach (var id in new[] { "upright-upright", "upright-reversed", "reversed-upright", "reversed-reversed" })
            states[id] = new() { Interaction = $"Interaction {id}", Direction = $"Direction {id}", Tags = [], OverallValence = 0, OverallIntensity = 2 };
        Write($"content/{locale}/oriented-pairs/{filename ?? $"{cardAId}__{cardBId}"}.json", new TarotOrientedPairBundleDocument
        {
            SchemaVersion = 1, CardAId = cardAId, CardBId = cardBId, States = states
        });
    }

    public void AddPairStates(
        string locale,
        string cardAId,
        string cardBId,
        IReadOnlyDictionary<string, TarotOrientedPairStateDocument> states,
        string? filename = null) => Write(
        $"content/{locale}/oriented-pairs/{filename ?? $"{cardAId}__{cardBId}"}.json",
        new TarotOrientedPairBundleDocument
        {
            SchemaVersion = 1,
            CardAId = cardAId,
            CardBId = cardBId,
            States = states.ToDictionary(pair => pair.Key, pair => (TarotOrientedPairStateDocument?)pair.Value, StringComparer.Ordinal)
        });

    public void AddPositions(string locale, string cardId, string? filename = null)
    {
        var states = new Dictionary<string, Dictionary<string, TarotThreeCardPositionStateDocument?>?>(StringComparer.Ordinal);
        foreach (var position in new[] { "past", "present", "future" })
            states[position] = new(StringComparer.Ordinal)
            {
                ["upright"] = PositionState(position, "upright"),
                ["reversed"] = PositionState(position, "reversed")
            };
        Write($"content/{locale}/three-card-positions/{filename ?? cardId}.json", new TarotThreeCardPositionsBundleDocument
        {
            SchemaVersion = 1, CardId = cardId, States = states
        });
    }

    public void AddPositionStates(
        string locale,
        string cardId,
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, TarotThreeCardPositionStateDocument>> states,
        string? filename = null) => Write(
        $"content/{locale}/three-card-positions/{filename ?? cardId}.json",
        new TarotThreeCardPositionsBundleDocument
        {
            SchemaVersion = 1,
            CardId = cardId,
            States = states.ToDictionary(
                pair => pair.Key,
                pair => (Dictionary<string, TarotThreeCardPositionStateDocument?>?)pair.Value.ToDictionary(
                    state => state.Key,
                    state => (TarotThreeCardPositionStateDocument?)state.Value,
                    StringComparer.Ordinal),
                StringComparer.Ordinal)
        });

    public void Write<T>(string relativePath, T value)
    {
        var path = Path.Combine(Root, relativePath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllBytes(path, TarotInterpretationJson.Serialize(value));
    }

    public void Dispose()
    {
        if (Directory.Exists(Root)) Directory.Delete(Root, recursive: true);
    }

    private void WriteLabels(string locale) => Write($"content/{locale}/labels.json", new TarotLabelsDocument
    {
        SchemaVersion = 1,
        SingleCardSections = Labels("situation", "development", "risk", "outcome", "advice"),
        ThreeCardPositions = Labels("past", "present", "future"),
        Relations = Labels("past-present", "present-future", "overall")
    });

    private static TarotInterpretationPackDocument Manifest()
    {
        var locales = new[] { "ru", "en" };
        var modules = new Dictionary<string, Dictionary<string, TarotInterpretationModuleDocument?>?>(StringComparer.Ordinal);
        foreach (var mode in new[] { "single-card", "two-cards", "three-cards", "celtic-cross" })
            modules[mode] = locales.ToDictionary(locale => locale, locale => (TarotInterpretationModuleDocument?)new()
            {
                Ready = false,
                Dependencies = mode switch
                {
                    "two-cards" => [TarotModuleDependency.OrientedPairs],
                    "three-cards" => [TarotModuleDependency.OrientedPairs, TarotModuleDependency.ThreeCardPositions, TarotModuleDependency.ThreeCardSynthesis],
                    _ => []
                }
            }, StringComparer.Ordinal);
        return new()
        {
            SchemaVersion = 2, PackId = "classic", SemanticDeckId = "standard-78", SourceLocale = "ru", ContentVersion = 1,
            DeclaredLocales = ["ru", "en"], DisplayNames = new(StringComparer.Ordinal) { ["ru"] = "Классика", ["en"] = "Classic" }, Modules = modules
        };
    }

    private static Dictionary<string, string?> Labels(params string[] ids) => ids.ToDictionary(id => id, id => (string?)$"Label {id}", StringComparer.Ordinal);
    private static TarotSingleCardStateDocument SingleState(bool reversed, string? conceptId) => new()
    {
        Sections = Labels("situation", "development", "risk", "outcome", "advice"),
        Tags = conceptId is null ? [] : [new TarotTagAssignmentDocument { ConceptId = conceptId, Valence = reversed ? -1 : 1, Intensity = 2 }],
        OverallValence = reversed ? -1 : 1,
        OverallIntensity = 2, ReversalMechanisms = reversed ? [TarotReversalMechanism.Blocked] : []
    };
    private static TarotThreeCardPositionStateDocument PositionState(string position, string orientation) => new()
    {
        Text = $"Position {position} {orientation}", Tags = [], OverallValence = 0, OverallIntensity = 2
    };
}
