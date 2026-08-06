using NoxAeterna.Interpretation.Tarot.Contracts;
using NoxAeterna.Interpretation.Tarot.Serialization;
using NoxAeterna.Interpretation.Tarot.Validation;
using NoxAeterna.Tools.Repository.Interpretation.Reports;

namespace NoxAeterna.Tools.Repository.Interpretation.Analysis;

public sealed class InterpretationPackValidator
{
    public InterpretationToolReport Validate(string packRoot)
    {
        var paths = new InterpretationPackPaths(packRoot, mustExist: true);
        var diagnostics = new InterpretationDiagnosticBag();
        var manifestPath = Path.Combine(paths.Root, "interpretation-pack.json");
        if (!File.Exists(manifestPath))
        {
            diagnostics.Error("pack.manifest-missing", "interpretation-pack.json", "Pack manifest is missing.");
            return new(diagnostics.Items);
        }

        var parsedManifest = TarotInterpretationJson.Parse<TarotInterpretationPackDocument>(File.ReadAllBytes(manifestPath));
        if (!parsedManifest.IsSuccess || parsedManifest.Document is null)
        {
            diagnostics.Error("pack.manifest-json", "interpretation-pack.json", parsedManifest.Failure?.Message ?? "Manifest JSON is malformed.");
            return new(diagnostics.Items);
        }

        var validatedManifest = TarotInterpretationValidator.ValidateManifest(parsedManifest.Document);
        diagnostics.AddValidation("interpretation-pack.json", validatedManifest.Diagnostics);
        if (!validatedManifest.IsValid || validatedManifest.Value is null)
        {
            return new(diagnostics.Items);
        }

        var manifest = validatedManifest.Value;
        var discovery = new AcceptedContentDiscovery().Discover(paths.Root);
        foreach (var diagnostic in discovery.Diagnostics)
        {
            Add(diagnostics, diagnostic);
        }

        var contentByPath = discovery.Files.ToDictionary(item => item.RelativePath, StringComparer.Ordinal);
        var indexedContent = new HashSet<string>(StringComparer.Ordinal);
        var listedIndexes = manifest.IndexFiles.Select(item => item.Path.Value).ToHashSet(StringComparer.Ordinal);
        var indexEntryCounts = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var reference in manifest.IndexFiles.OrderBy(item => item.Path.Value, StringComparer.Ordinal))
        {
            ValidateIndex(paths, manifest, reference, contentByPath, indexedContent, indexEntryCounts, diagnostics);
        }

        ValidateStaleIndexes(paths, listedIndexes, diagnostics);
        ValidateUnindexedContent(discovery.Files, listedIndexes, indexedContent, diagnostics);
        ValidateReadyModules(manifest, diagnostics);
        ValidateReadyInventories(manifest, discovery.Files, diagnostics);

        var counts = new Dictionary<string, int>(StringComparer.Ordinal)
        {
            ["acceptedContentFiles"] = discovery.Files.Count,
            ["indexFiles"] = manifest.IndexFiles.Count,
            ["indexedEntries"] = indexEntryCounts.Values.Sum()
        };
        return new(diagnostics.Items, counts);
    }

    private static void ValidateIndex(
        InterpretationPackPaths paths,
        TarotInterpretationPackManifest manifest,
        TarotInterpretationIndexFile reference,
        IReadOnlyDictionary<string, AcceptedContentFile> contentByPath,
        ISet<string> indexedContent,
        IDictionary<string, int> indexEntryCounts,
        InterpretationDiagnosticBag diagnostics)
    {
        var relative = reference.Path.Value;
        string absolute;
        try
        {
            absolute = paths.Resolve(reference.Path);
        }
        catch (ArgumentException exception)
        {
            diagnostics.Error("index.path", relative, exception.Message);
            return;
        }

        if (!File.Exists(absolute))
        {
            diagnostics.Error("index.missing", relative, "Manifest-listed index file is missing.");
            return;
        }

        var bytes = File.ReadAllBytes(absolute);
        if (InterpretationContentHash.Sha256(bytes) != reference.Sha256.Value)
        {
            diagnostics.Error("index.hash", relative, "Index SHA-256 does not match the manifest reference.");
        }

        var parsed = TarotInterpretationJson.Parse<TarotGeneratedIndexDocument>(bytes);
        if (!parsed.IsSuccess || parsed.Document is null)
        {
            diagnostics.Error("index.json", relative, parsed.Failure?.Message ?? "Index JSON is malformed.");
            return;
        }

        var validated = TarotInterpretationValidator.ValidateGeneratedIndex(parsed.Document);
        diagnostics.AddValidation(relative, validated.Diagnostics);
        if (!validated.IsValid || validated.Value is null)
        {
            return;
        }

        var index = validated.Value;
        if (index.PackId != manifest.PackId)
        {
            diagnostics.Error("index.pack", relative, "Index packId does not match the manifest.");
        }

        if (index.ContentVersion != manifest.ContentVersion)
        {
            diagnostics.Error("index.content-version", relative, "Index contentVersion does not match the manifest.");
        }

        var expectedPath = ExpectedIndexPath(index.Locale.Value, index.CorpusId);
        if (relative != expectedPath)
        {
            diagnostics.Error("index.identity-path", relative, "Index locale/corpus identity does not match its canonical path.");
        }

        indexEntryCounts[relative] = index.Entries.Count;
        foreach (var entry in index.Entries)
        {
            var entryPath = entry.Path.Value;
            if (!entryPath.StartsWith($"content/{index.Locale.Value}/", StringComparison.Ordinal))
            {
                diagnostics.Error("index.locale-mix", entryPath, "Index entry must remain in the index locale.");
            }

            string absoluteContent;
            try
            {
                absoluteContent = paths.Resolve(entry.Path);
            }
            catch (ArgumentException exception)
            {
                diagnostics.Error("content.path", entryPath, exception.Message);
                continue;
            }

            if (!File.Exists(absoluteContent))
            {
                diagnostics.Error("content.missing", entryPath, "Indexed accepted-content file is missing.");
                continue;
            }

            var contentBytes = File.ReadAllBytes(absoluteContent);
            if (InterpretationContentHash.Sha256(contentBytes) != entry.Sha256.Value)
            {
                diagnostics.Error("content.hash", entryPath, "Content SHA-256 does not match its index entry.");
            }

            if (!contentByPath.TryGetValue(entryPath, out var discovered))
            {
                diagnostics.Error("content.invalid-indexed", entryPath, "Indexed content is not a valid canonical accepted-content file.");
                continue;
            }

            if (discovered.Corpus != index.CorpusId || discovered.Key != entry.Key)
            {
                diagnostics.Error("content.key", entryPath, "Index key/corpus does not match content identity.");
            }

            indexedContent.Add(entryPath);
        }
    }

    private static void ValidateStaleIndexes(
        InterpretationPackPaths paths,
        IReadOnlySet<string> listed,
        InterpretationDiagnosticBag diagnostics)
    {
        var root = Path.Combine(paths.Root, "indexes");
        if (!Directory.Exists(root))
        {
            return;
        }

        foreach (var file in Directory.EnumerateFiles(root, "*.json", SearchOption.AllDirectories))
        {
            var relative = paths.Relative(file);
            if (!listed.Contains(relative))
            {
                diagnostics.Error("index.stale", relative, "Generated index exists but is not listed by the manifest; it was not deleted.");
            }
        }
    }

    private static void ValidateUnindexedContent(
        IReadOnlyList<AcceptedContentFile> content,
        IReadOnlySet<string> listedIndexes,
        IReadOnlySet<string> indexedContent,
        InterpretationDiagnosticBag diagnostics)
    {
        foreach (var entry in content.Where(item => item.Corpus is not null && item.Key is not null))
        {
            var indexPath = ExpectedIndexPath(entry.Locale.Value, entry.Corpus!.Value);
            if (listedIndexes.Contains(indexPath) && !indexedContent.Contains(entry.RelativePath))
            {
                diagnostics.Error("content.unindexed", entry.RelativePath, "Accepted content is absent from its generated index.");
            }
            else if (!listedIndexes.Contains(indexPath))
            {
                diagnostics.Warning("content.incomplete", entry.RelativePath, "Accepted content belongs to an incomplete not-indexed corpus.");
            }
        }
    }

    private static void ValidateReadyModules(
        TarotInterpretationPackManifest manifest,
        InterpretationDiagnosticBag diagnostics)
    {
        foreach (var (mode, locales) in manifest.Modules)
        {
            foreach (var (locale, module) in locales.Where(pair => pair.Value.Ready))
            {
                foreach (var path in module.IndexPaths)
                {
                    if (manifest.IndexFiles.All(item => item.Path != path))
                    {
                        diagnostics.Error("module.ready-index", $"{mode}/{locale.Value}", $"Ready module lacks index reference {path.Value}.");
                    }
                }
            }
        }
    }

    private static void ValidateReadyInventories(
        TarotInterpretationPackManifest manifest,
        IReadOnlyList<AcceptedContentFile> content,
        InterpretationDiagnosticBag diagnostics)
    {
        foreach (var locale in manifest.DeclaredLocales)
        {
            var modules = manifest.Modules.ToDictionary(
                pair => pair.Key,
                pair => pair.Value[locale].Ready);
            if (modules[TarotInterpretationMode.SingleCard])
            {
                AddInventory(InterpretationInventoryValidator.ValidateSingleCard(content
                    .Where(item => item.Locale == locale && item.Kind == AcceptedContentKind.SingleCard)
                    .Select(item => item.Key!)), locale.Value, diagnostics);
            }

            if (modules[TarotInterpretationMode.TwoCards] || modules[TarotInterpretationMode.ThreeCards])
            {
                AddInventory(InterpretationInventoryValidator.ValidateOrientedPairs(content
                    .Where(item => item.Locale == locale && item.Kind == AcceptedContentKind.OrientedPair)
                    .Select(item => item.Key!)), locale.Value, diagnostics);
            }

            if (modules[TarotInterpretationMode.ThreeCards])
            {
                AddInventory(InterpretationInventoryValidator.ValidateThreeCardPositions(content
                    .Where(item => item.Locale == locale && item.Kind == AcceptedContentKind.ThreeCardPosition)
                    .Select(item => item.Key!)), locale.Value, diagnostics);
            }
        }
    }

    private static void AddInventory(
        InterpretationToolReport report,
        string locale,
        InterpretationDiagnosticBag diagnostics)
    {
        foreach (var diagnostic in report.Diagnostics)
        {
            diagnostics.Error(diagnostic.Code, $"{locale}/{diagnostic.Target}", diagnostic.Message);
        }
    }

    internal static string ExpectedIndexPath(string locale, TarotInterpretationCorpus corpus) => corpus switch
    {
        TarotInterpretationCorpus.SingleCard => $"indexes/{locale}/single-card.json",
        TarotInterpretationCorpus.OrientedPairs => $"indexes/{locale}/oriented-pairs.json",
        TarotInterpretationCorpus.ThreeCards => $"indexes/{locale}/three-cards.json",
        _ => throw new ArgumentOutOfRangeException(nameof(corpus))
    };

    private static void Add(InterpretationDiagnosticBag bag, InterpretationToolDiagnostic diagnostic)
    {
        if (diagnostic.Severity == InterpretationToolSeverity.Error)
        {
            bag.Error(diagnostic.Code, diagnostic.Target, diagnostic.Message);
        }
        else
        {
            bag.Warning(diagnostic.Code, diagnostic.Target, diagnostic.Message);
        }
    }
}
