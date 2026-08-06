using NoxAeterna.Domain.Tarot;
using NoxAeterna.Interpretation.Tarot.Contracts;
using NoxAeterna.Interpretation.Tarot.Keys;

namespace NoxAeterna.Tests.Interpretation.Tarot;

internal static class TarotInterpretationTestDocuments
{
    public const string Hash = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";

    public static TarotInterpretationPackDocument Manifest(bool readySingleCard = false)
    {
        var locales = new[] { "ru", "en" };
        var modules = new Dictionary<string, Dictionary<string, TarotInterpretationModuleDocument?>?>(StringComparer.Ordinal);
        foreach (var mode in new[] { "single-card", "two-cards", "three-cards", "celtic-cross" })
        {
            modules[mode] = locales.ToDictionary(
                locale => locale,
                locale => (TarotInterpretationModuleDocument?)Module(mode, locale, readySingleCard && mode == "single-card"),
                StringComparer.Ordinal);
        }

        return new TarotInterpretationPackDocument
        {
            SchemaVersion = 1,
            PackId = "classic",
            SemanticDeckId = "standard-78",
            SourceLocale = "ru",
            ContentVersion = 1,
            DeclaredLocales = ["ru", "en"],
            DisplayNames = new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                ["ru"] = "Классика",
                ["en"] = "Classic"
            },
            Modules = modules,
            IndexFiles = readySingleCard
                ? locales.Select(locale => (TarotInterpretationIndexFileDocument?)new TarotInterpretationIndexFileDocument
                {
                    Path = $"indexes/{locale}/single-card.json",
                    Sha256 = Hash
                }).ToList()
                : []
        };
    }

    public static TarotVocabularyDocument Vocabulary() => new()
    {
        SchemaVersion = 1,
        ConceptId = "failure",
        Label = "Неудача",
        Meaning = "Неблагоприятный исход."
    };

    public static TarotTagAssignmentDocument Tag(string conceptId = "conflict") => new()
    {
        ConceptId = conceptId,
        Valence = -1,
        Intensity = 2
    };

    public static TarotSingleCardDocument SingleCard(
        TarotCardOrientation orientation = TarotCardOrientation.Upright) => new()
    {
        SchemaVersion = 1,
        CardId = "major.fool",
        Orientation = orientation,
        Sections = new Dictionary<string, string?>(StringComparer.Ordinal)
        {
            ["situation"] = "Ситуация",
            ["development"] = "Развитие",
            ["risk"] = "Риск",
            ["outcome"] = "Исход",
            ["advice"] = "Совет"
        },
        Tags = [Tag("choice"), Tag("opportunity"), Tag("renewal"), Tag("release"), Tag("uncertainty")],
        OverallValence = 1,
        OverallIntensity = 2,
        ReversalMechanisms = orientation == TarotCardOrientation.Upright
            ? []
            : [TarotReversalMechanism.Blocked]
    };

    public static TarotOrientedPairDocument Pair() => new()
    {
        SchemaVersion = 1,
        CardAId = "major.tower",
        CardBId = "major.world",
        OrientationState = TarotOrientedPairState.ReversedUpright,
        Interaction = "Старый порядок рушится перед завершением пути.",
        Direction = "Прими перелом и доведи начатое до ясной точки.",
        Tags =
        [
            Tag("conflict"), Tag("release"), Tag("renewal"),
            Tag("uncertainty"), Tag("choice"), Tag("opportunity")
        ],
        OverallValence = -1,
        OverallIntensity = 3
    };

    public static TarotThreeCardPositionDocument Position() => new()
    {
        SchemaVersion = 1,
        Position = TarotThreeCardPosition.Past,
        CardId = "major.tower",
        Orientation = TarotCardOrientation.Upright,
        Text = "Прошлый перелом всё ещё определяет происходящее.",
        Tags = [Tag("release")],
        OverallValence = -1,
        OverallIntensity = 3
    };

    public static TarotGeneratedIndexDocument SingleCardIndex()
    {
        var entries = StandardTarotCatalog.Deck.Cards
            .SelectMany(card => Enum.GetValues<TarotCardOrientation>().Select(orientation =>
                new TarotGeneratedIndexEntryDocument
                {
                    Key = TarotInterpretationKeys.CreateSingleCard(card.Id, orientation),
                    Path = $"content/ru/modes/single-card/{card.Id.Value}/{orientation.ToString().ToLowerInvariant()}.json",
                    Sha256 = Hash
                }))
            .OrderBy(entry => entry.Key, StringComparer.Ordinal)
            .Select(entry => (TarotGeneratedIndexEntryDocument?)entry)
            .ToList();

        return new TarotGeneratedIndexDocument
        {
            SchemaVersion = 1,
            PackId = "classic",
            Locale = "ru",
            CorpusId = TarotInterpretationCorpus.SingleCard,
            ContentVersion = 1,
            ExpectedEntryCount = 156,
            Entries = entries
        };
    }

    private static TarotInterpretationModuleDocument Module(string mode, string locale, bool ready) => mode switch
    {
        "single-card" => new TarotInterpretationModuleDocument
        {
            Ready = ready,
            IndexPaths = [$"indexes/{locale}/single-card.json"],
            Dependencies = []
        },
        "two-cards" => new TarotInterpretationModuleDocument
        {
            Ready = false,
            IndexPaths = [$"indexes/{locale}/oriented-pairs.json"],
            Dependencies = [TarotModuleDependency.OrientedPairs]
        },
        "three-cards" => new TarotInterpretationModuleDocument
        {
            Ready = false,
            IndexPaths = [$"indexes/{locale}/oriented-pairs.json", $"indexes/{locale}/three-cards.json"],
            Dependencies =
            [
                TarotModuleDependency.OrientedPairs,
                TarotModuleDependency.ThreeCardPositions,
                TarotModuleDependency.ThreeCardSynthesis
            ]
        },
        _ => new TarotInterpretationModuleDocument { Ready = false, IndexPaths = [], Dependencies = [] }
    };
}
