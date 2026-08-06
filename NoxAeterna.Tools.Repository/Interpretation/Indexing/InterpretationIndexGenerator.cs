using NoxAeterna.Interpretation.Tarot.Contracts;
using NoxAeterna.Interpretation.Tarot.Serialization;
using NoxAeterna.Interpretation.Tarot.Validation;
using NoxAeterna.Tools.Repository.Interpretation.Analysis;
using NoxAeterna.Tools.Repository.Interpretation.Reports;

namespace NoxAeterna.Tools.Repository.Interpretation.Indexing;

public sealed record PreparedInterpretationFile(string RelativePath, byte[] Bytes, bool IsManifest = false);

public interface IInterpretationPackFileWriter
{
    void Write(string packRoot, IReadOnlyList<PreparedInterpretationFile> files);
}

public sealed class AtomicInterpretationPackFileWriter : IInterpretationPackFileWriter
{
    public void Write(string packRoot, IReadOnlyList<PreparedInterpretationFile> files)
    {
        var paths = new InterpretationPackPaths(packRoot, mustExist: true);
        var staged = new List<(string Temporary, string Final, bool IsManifest)>();
        try
        {
            foreach (var file in files.OrderBy(item => item.IsManifest).ThenBy(item => item.RelativePath, StringComparer.Ordinal))
            {
                var final = paths.Resolve(file.RelativePath);
                var directory = Path.GetDirectoryName(final)!;
                Directory.CreateDirectory(directory);
                var temporary = Path.Combine(directory, $".{Path.GetFileName(final)}.{Guid.NewGuid():N}.tmp");
                using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    stream.Write(file.Bytes);
                    stream.Flush(flushToDisk: true);
                }

                staged.Add((temporary, final, file.IsManifest));
            }

            foreach (var file in staged.OrderBy(item => item.IsManifest).ThenBy(item => item.Final, StringComparer.Ordinal))
            {
                File.Move(file.Temporary, file.Final, overwrite: true);
            }
        }
        finally
        {
            foreach (var file in staged)
            {
                try
                {
                    if (File.Exists(file.Temporary))
                    {
                        File.Delete(file.Temporary);
                    }
                }
                catch (IOException)
                {
                }
                catch (UnauthorizedAccessException)
                {
                }
            }
        }
    }
}

public sealed class InterpretationIndexGenerator(
    IInterpretationPackFileWriter? fileWriter = null)
{
    private readonly IInterpretationPackFileWriter fileWriter = fileWriter ?? new AtomicInterpretationPackFileWriter();

    public InterpretationToolReport Generate(string packRoot, bool checkOnly)
    {
        var paths = new InterpretationPackPaths(packRoot, mustExist: true);
        var diagnostics = new InterpretationDiagnosticBag();
        var manifestPath = Path.Combine(paths.Root, "interpretation-pack.json");
        if (!File.Exists(manifestPath))
        {
            diagnostics.Error("pack.manifest-missing", "interpretation-pack.json", "Pack manifest is missing.");
            return new(diagnostics.Items);
        }

        var originalManifestBytes = File.ReadAllBytes(manifestPath);
        var parsed = TarotInterpretationJson.Parse<TarotInterpretationPackDocument>(originalManifestBytes);
        if (!parsed.IsSuccess || parsed.Document is null)
        {
            diagnostics.Error("pack.manifest-json", "interpretation-pack.json", parsed.Failure?.Message ?? "Manifest JSON is malformed.");
            return new(diagnostics.Items);
        }

        var validation = TarotInterpretationValidator.ValidateManifest(parsed.Document);
        diagnostics.AddValidation("interpretation-pack.json", validation.Diagnostics);
        if (!validation.IsValid || validation.Value is null)
        {
            return new(diagnostics.Items);
        }

        var discovery = new AcceptedContentDiscovery().Discover(paths.Root);
        foreach (var diagnostic in discovery.Diagnostics)
        {
            Add(diagnostics, diagnostic);
        }

        if (diagnostics.HasErrors)
        {
            return new(diagnostics.Items);
        }

        var manifest = validation.Value;
        var generated = new List<PreparedInterpretationFile>();
        foreach (var locale in manifest.DeclaredLocales.OrderBy(item => item.Value, StringComparer.Ordinal))
        {
            PrepareCorpus(locale, TarotInterpretationCorpus.SingleCard, discovery.Files, manifest, diagnostics, generated);
            PrepareCorpus(locale, TarotInterpretationCorpus.OrientedPairs, discovery.Files, manifest, diagnostics, generated);
            PrepareCorpus(locale, TarotInterpretationCorpus.ThreeCards, discovery.Files, manifest, diagnostics, generated);
        }

        if (diagnostics.HasErrors)
        {
            return new(diagnostics.Items);
        }

        var expectedIndexPaths = generated.Select(item => item.RelativePath).ToHashSet(StringComparer.Ordinal);
        ValidateStaleIndexes(paths, expectedIndexPaths, diagnostics);
        if (diagnostics.HasErrors)
        {
            return new(diagnostics.Items);
        }

        parsed.Document.IndexFiles = generated
            .OrderBy(item => item.RelativePath, StringComparer.Ordinal)
            .Select(item => new TarotInterpretationIndexFileDocument
            {
                Path = item.RelativePath,
                Sha256 = InterpretationContentHash.Sha256(item.Bytes)
            })
            .Cast<TarotInterpretationIndexFileDocument?>()
            .ToList();

        var updatedValidation = TarotInterpretationValidator.ValidateManifest(parsed.Document);
        diagnostics.AddValidation("interpretation-pack.json", updatedValidation.Diagnostics);
        if (!updatedValidation.IsValid)
        {
            return new(diagnostics.Items);
        }

        var manifestBytes = TarotInterpretationJson.Serialize(parsed.Document);
        generated.Add(new("interpretation-pack.json", manifestBytes, IsManifest: true));
        var drift = Compare(paths, generated);
        if (checkOnly)
        {
            return new(diagnostics.Items, Counts(discovery.Files, generated), driftPaths: drift);
        }

        if (drift.Count > 0)
        {
            try
            {
                fileWriter.Write(paths.Root, generated);
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException)
            {
                diagnostics.Error("generate.write", "pack-root", exception.Message);
                return new(diagnostics.Items, Counts(discovery.Files, generated));
            }
        }

        return new(
            diagnostics.Items,
            Counts(discovery.Files, generated),
            generatedPaths: drift.Where(path => path != "interpretation-pack.json"));
    }

    private static void PrepareCorpus(
        TarotInterpretationLocale locale,
        TarotInterpretationCorpus corpus,
        IReadOnlyList<AcceptedContentFile> allFiles,
        TarotInterpretationPackManifest manifest,
        InterpretationDiagnosticBag diagnostics,
        ICollection<PreparedInterpretationFile> generated)
    {
        var entries = allFiles
            .Where(item => item.Locale == locale && item.Corpus == corpus && item.Key is not null)
            .OrderBy(item => item.Key, StringComparer.Ordinal)
            .ToArray();
        var required = IsRequired(manifest, locale, corpus);
        if (entries.Length == 0 && !required)
        {
            return;
        }

        var inventory = corpus switch
        {
            TarotInterpretationCorpus.SingleCard => InterpretationInventoryValidator.ValidateSingleCard(entries.Select(item => item.Key!)),
            TarotInterpretationCorpus.OrientedPairs => InterpretationInventoryValidator.ValidateOrientedPairs(entries.Select(item => item.Key!)),
            TarotInterpretationCorpus.ThreeCards => InterpretationInventoryValidator.ValidateThreeCardPositions(
                entries.Where(item => item.Kind == AcceptedContentKind.ThreeCardPosition).Select(item => item.Key!)),
            _ => throw new ArgumentOutOfRangeException(nameof(corpus))
        };

        if (!inventory.Success)
        {
            if (required)
            {
                foreach (var item in inventory.Diagnostics)
                {
                    diagnostics.Error(item.Code, $"{locale.Value}/{corpus}/{item.Target}", item.Message);
                }
            }
            else
            {
                diagnostics.Warning(
                    "generate.incomplete",
                    $"{locale.Value}/{TarotSchemaText.Get(corpus, TarotSchemaText.Corpora)}",
                    "Incomplete ready=false corpus was not indexed.");
            }

            return;
        }

        var document = new TarotGeneratedIndexDocument
        {
            SchemaVersion = 1,
            PackId = manifest.PackId.Value,
            Locale = locale.Value,
            CorpusId = corpus,
            ContentVersion = manifest.ContentVersion,
            ExpectedEntryCount = entries.Length,
            ExpectedIdentityCount = corpus == TarotInterpretationCorpus.OrientedPairs ? 3003 : null,
            ExpectedPositionEntryCount = corpus == TarotInterpretationCorpus.ThreeCards ? 468 : null,
            Entries = entries.Select(item => new TarotGeneratedIndexEntryDocument
            {
                Key = item.Key,
                Path = item.RelativePath,
                Sha256 = item.Sha256
            }).Cast<TarotGeneratedIndexEntryDocument?>().ToList()
        };
        var validated = TarotInterpretationValidator.ValidateGeneratedIndex(document);
        if (!validated.IsValid)
        {
            diagnostics.AddValidation($"{locale.Value}/{corpus}", validated.Diagnostics);
            return;
        }

        generated.Add(new(
            InterpretationPackValidator.ExpectedIndexPath(locale.Value, corpus),
            TarotInterpretationJson.Serialize(document)));
    }

    private static bool IsRequired(
        TarotInterpretationPackManifest manifest,
        TarotInterpretationLocale locale,
        TarotInterpretationCorpus corpus)
    {
        var modes = corpus switch
        {
            TarotInterpretationCorpus.SingleCard => new[] { TarotInterpretationMode.SingleCard },
            TarotInterpretationCorpus.OrientedPairs => new[] { TarotInterpretationMode.TwoCards, TarotInterpretationMode.ThreeCards },
            TarotInterpretationCorpus.ThreeCards => new[] { TarotInterpretationMode.ThreeCards },
            _ => throw new ArgumentOutOfRangeException(nameof(corpus))
        };
        return modes.Any(mode => manifest.Modules[mode][locale].Ready);
    }

    private static List<string> Compare(InterpretationPackPaths paths, IEnumerable<PreparedInterpretationFile> expected)
    {
        var drift = new List<string>();
        foreach (var file in expected.OrderBy(item => item.RelativePath, StringComparer.Ordinal))
        {
            var absolute = paths.Resolve(file.RelativePath);
            if (!File.Exists(absolute) || !File.ReadAllBytes(absolute).SequenceEqual(file.Bytes))
            {
                drift.Add(file.RelativePath);
            }
        }

        return drift;
    }

    private static void ValidateStaleIndexes(
        InterpretationPackPaths paths,
        IReadOnlySet<string> expected,
        InterpretationDiagnosticBag diagnostics)
    {
        var indexRoot = Path.Combine(paths.Root, "indexes");
        if (!Directory.Exists(indexRoot))
        {
            return;
        }

        foreach (var path in Directory.EnumerateFiles(indexRoot, "*.json", SearchOption.AllDirectories))
        {
            var relative = paths.Relative(path);
            if (!expected.Contains(relative))
            {
                diagnostics.Error("generate.stale-index", relative, "Stale index was reported and not deleted.");
            }
        }
    }

    private static IReadOnlyDictionary<string, int> Counts(
        IReadOnlyList<AcceptedContentFile> content,
        IReadOnlyCollection<PreparedInterpretationFile> generated) => new Dictionary<string, int>
    {
        ["acceptedContentFiles"] = content.Count,
        ["generatedIndexes"] = generated.Count(item => !item.IsManifest)
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
