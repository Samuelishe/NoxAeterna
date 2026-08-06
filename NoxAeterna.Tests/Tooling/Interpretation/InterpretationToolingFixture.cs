using System.Text.Json.Nodes;
using NoxAeterna.Domain.Tarot;
using NoxAeterna.Interpretation.Tarot.Contracts;
using NoxAeterna.Interpretation.Tarot.Serialization;

namespace NoxAeterna.Tests.Tooling.Interpretation;

internal sealed class InterpretationToolingFixture : IDisposable
{
    private InterpretationToolingFixture(string root)
    {
        Root = root;
    }

    public string Root { get; }
    public string ManifestPath => Path.Combine(Root, "interpretation-pack.json");

    public static InterpretationToolingFixture CreateSkeleton()
    {
        var root = Path.Combine(Path.GetTempPath(), $"NoxAeterna-interpretation-tooling-{Guid.NewGuid():N}");
        CopyTree(SkeletonRoot, root);
        return new(root);
    }

    public void AddCompleteSingleCardCorpus()
    {
        foreach (var card in StandardTarotCatalog.Deck.Cards)
        {
            foreach (var orientation in Enum.GetValues<TarotCardOrientation>())
            {
                var orientationText = TarotSchemaText.Get(orientation, TarotSchemaText.CardOrientations);
                var path = Path.Combine(
                    Root,
                    "content",
                    "ru",
                    "modes",
                    "single-card",
                    card.Id.Value,
                    $"{orientationText}.json");
                var document = new TarotSingleCardDocument
                {
                    SchemaVersion = 1,
                    CardId = card.Id.Value,
                    Orientation = orientation,
                    Sections = new(StringComparer.Ordinal)
                    {
                        ["situation"] = $"Synthetic situation {card.Id.Value} {orientationText}",
                        ["development"] = $"Synthetic development {card.Id.Value} {orientationText}",
                        ["risk"] = $"Synthetic risk {card.Id.Value} {orientationText}",
                        ["outcome"] = $"Synthetic outcome {card.Id.Value} {orientationText}",
                        ["advice"] = $"Synthetic advice {card.Id.Value} {orientationText}"
                    },
                    Tags = Enumerable.Range(1, 5).Select(index => new TarotTagAssignmentDocument
                    {
                        ConceptId = $"synthetic-{index}",
                        Valence = index % 5 - 2,
                        Intensity = index % 3 + 1
                    }).Cast<TarotTagAssignmentDocument?>().ToList(),
                    OverallValence = 0,
                    OverallIntensity = 2,
                    ReversalMechanisms = orientation == TarotCardOrientation.Upright
                        ? []
                        : [TarotReversalMechanism.Blocked]
                };
                Write(path, TarotInterpretationJson.Serialize(document));
            }
        }
    }

    public void SetSingleCardReady(bool ready)
    {
        var node = JsonNode.Parse(File.ReadAllText(ManifestPath))!.AsObject();
        node["modules"]!["single-card"]!["ru"]!["ready"] = ready;
        Write(ManifestPath, System.Text.Encoding.UTF8.GetBytes(node.ToJsonString() + "\n"));
    }

    public string FirstSingleCardPath() => Path.Combine(
        Root,
        "content",
        "ru",
        "modes",
        "single-card",
        "major.fool",
        "upright.json");

    public void Dispose()
    {
        if (Directory.Exists(Root))
        {
            Directory.Delete(Root, recursive: true);
        }
    }

    public static string RepositoryRoot { get; } = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));

    public static string SkeletonRoot { get; } = Path.Combine(
        RepositoryRoot,
        "NoxAeterna.Tests",
        "TestData",
        "Interpretation",
        "not-ready-skeleton");

    public static string WorkingSkeletonRoot { get; } = Path.Combine(
        RepositoryRoot,
        "NoxAeterna.Tests",
        "TestData",
        "Interpretation",
        "working-skeleton");

    private static void CopyTree(string source, string destination)
    {
        Directory.CreateDirectory(destination);
        foreach (var directory in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(Path.Combine(destination, Path.GetRelativePath(source, directory)));
        }

        foreach (var file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
        {
            var target = Path.Combine(destination, Path.GetRelativePath(source, file));
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            File.Copy(file, target);
        }
    }

    private static void Write(string path, byte[] bytes)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllBytes(path, bytes);
    }
}
