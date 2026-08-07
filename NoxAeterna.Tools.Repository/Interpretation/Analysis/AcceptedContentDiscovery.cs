using System.Text.Json;
using NoxAeterna.Domain.Tarot;
using NoxAeterna.Interpretation.Tarot.Contracts;
using NoxAeterna.Interpretation.Tarot.Keys;
using NoxAeterna.Interpretation.Tarot.Serialization;
using NoxAeterna.Interpretation.Tarot.Validation;
using NoxAeterna.Tools.Repository.Interpretation.Reports;

namespace NoxAeterna.Tools.Repository.Interpretation.Analysis;

public enum AcceptedContentKind
{
    Vocabulary,
    SingleCard,
    OrientedPair,
    ThreeCardPosition,
    Synthesis
}

public sealed record AcceptedContentFile(
    AcceptedContentKind Kind,
    TarotInterpretationLocale Locale,
    TarotInterpretationCorpus? Corpus,
    string? Key,
    string RelativePath,
    string Sha256,
    byte[] Bytes);

public sealed class AcceptedContentDiscoveryResult(
    IEnumerable<AcceptedContentFile> files,
    IEnumerable<InterpretationToolDiagnostic> diagnostics)
{
    public IReadOnlyList<AcceptedContentFile> Files { get; } = Array.AsReadOnly(files
        .OrderBy(item => item.RelativePath, StringComparer.Ordinal)
        .ToArray());
    public IReadOnlyList<InterpretationToolDiagnostic> Diagnostics { get; } = Array.AsReadOnly(diagnostics.ToArray());
    public bool Success => Diagnostics.All(item => item.Severity != InterpretationToolSeverity.Error);
}

public sealed class AcceptedContentDiscovery
{
    public AcceptedContentDiscoveryResult Discover(string packRoot)
    {
        var paths = new InterpretationPackPaths(packRoot, mustExist: true);
        var diagnostics = new InterpretationDiagnosticBag();
        var files = new List<AcceptedContentFile>();
        var contentRoot = Path.Combine(paths.Root, "content");

        if (Directory.Exists(Path.Combine(paths.Root, "authoring")))
        {
            diagnostics.Error("content.production-authoring", "authoring", "Production packs cannot contain an authoring subtree.");
        }

        if (!Directory.Exists(contentRoot))
        {
            return new(files, diagnostics.Items);
        }

        foreach (var path in Directory.EnumerateFiles(contentRoot, "*.json", SearchOption.AllDirectories)
                     .Order(StringComparer.Ordinal))
        {
            var relative = paths.Relative(path);
            try
            {
                var file = ReadOne(path, relative, diagnostics);
                if (file is not null)
                {
                    files.Add(file);
                }
            }
            catch (IOException exception)
            {
                diagnostics.Error("content.io", relative, exception.Message);
            }
            catch (UnauthorizedAccessException exception)
            {
                diagnostics.Error("content.io", relative, exception.Message);
            }
        }

        var duplicates = files.Where(item => item.Key is not null)
            .GroupBy(item => $"{item.Locale.Value}|{item.Corpus}|{item.Key}", StringComparer.Ordinal)
            .Where(group => group.Count() > 1);
        foreach (var duplicate in duplicates)
        {
            diagnostics.Error("content.duplicate-key", duplicate.Key, "Accepted content contains a duplicate canonical identity.");
        }

        return new(files, diagnostics.Items);
    }

    private static AcceptedContentFile? ReadOne(
        string absolutePath,
        string relativePath,
        InterpretationDiagnosticBag diagnostics)
    {
        var segments = relativePath.Split('/');
        if (segments.Length < 4 || segments[0] != "content")
        {
            diagnostics.Error("content.path", relativePath, "Accepted JSON is outside a recognized canonical content path.");
            return null;
        }

        TarotInterpretationLocale locale;
        try
        {
            locale = new TarotInterpretationLocale(segments[1]);
        }
        catch (ArgumentException exception)
        {
            diagnostics.Error("content.locale", relativePath, exception.Message);
            return null;
        }

        var bytes = File.ReadAllBytes(absolutePath);
        var hash = InterpretationContentHash.Sha256(bytes);
        if (segments.Length == 4 && segments[2] == "vocabulary")
        {
            return ReadVocabulary(segments, locale, relativePath, bytes, hash, diagnostics);
        }

        if (segments.Length == 6 && segments[2] == "modes" && segments[3] == "single-card")
        {
            return ReadSingle(segments, locale, relativePath, bytes, hash, diagnostics);
        }

        if (segments.Length == 6 && segments[2] == "shared" && segments[3] == "oriented-pairs")
        {
            return ReadPair(segments, locale, relativePath, bytes, hash, diagnostics);
        }

        if (segments.Length == 8 && segments[2] == "modes" && segments[3] == "three-cards" &&
            segments[4] == "positions")
        {
            return ReadPosition(segments, locale, relativePath, bytes, hash, diagnostics);
        }

        if (segments.Length == 6 && segments[2] == "modes" && segments[3] == "three-cards" &&
            segments[4] == "synthesis")
        {
            return ReadSynthesis(segments, locale, relativePath, bytes, hash, diagnostics);
        }

        diagnostics.Error("content.path", relativePath, "Accepted JSON does not match a canonical content path.");
        return null;
    }

    private static AcceptedContentFile? ReadVocabulary(
        string[] segments,
        TarotInterpretationLocale locale,
        string path,
        byte[] bytes,
        string hash,
        InterpretationDiagnosticBag diagnostics)
    {
        var parsed = TarotInterpretationJson.Parse<TarotVocabularyDocument>(bytes);
        if (!AddParse(parsed, path, bytes, diagnostics) || parsed.Document is null)
        {
            return null;
        }

        var validated = TarotInterpretationValidator.ValidateVocabulary(parsed.Document);
        diagnostics.AddValidation(path, validated.Diagnostics);
        if (!validated.IsValid)
        {
            return null;
        }

        var expected = $"{validated.Value!.ConceptId.Value}.json";
        if (segments[3] != expected)
        {
            diagnostics.Error("content.identity-path", path, "Vocabulary conceptId does not match its path.");
            return null;
        }

        return new(AcceptedContentKind.Vocabulary, locale, null, null, path, hash, bytes);
    }

    private static AcceptedContentFile? ReadSingle(
        string[] segments,
        TarotInterpretationLocale locale,
        string path,
        byte[] bytes,
        string hash,
        InterpretationDiagnosticBag diagnostics)
    {
        var parsed = TarotInterpretationJson.Parse<TarotSingleCardDocument>(bytes);
        if (!AddParse(parsed, path, bytes, diagnostics) || parsed.Document is null)
        {
            return null;
        }

        var validated = TarotInterpretationValidator.ValidateSingleCard(parsed.Document, StandardTarotCatalog.Deck);
        diagnostics.AddValidation(path, validated.Diagnostics);
        if (!validated.IsValid)
        {
            return null;
        }

        var value = validated.Value!;
        var key = TarotInterpretationKeys.CreateSingleCard(value.CardId, value.Orientation);
        var expected = $"content/{locale.Value}/modes/single-card/{value.CardId.Value}/{TarotSchemaText.Get(value.Orientation, TarotSchemaText.CardOrientations)}.json";
        if (path != expected)
        {
            diagnostics.Error("content.identity-path", path, "Single-card identity does not match its canonical path.");
            return null;
        }

        return new(AcceptedContentKind.SingleCard, locale, TarotInterpretationCorpus.SingleCard, key, path, hash, bytes);
    }

    private static AcceptedContentFile? ReadPair(
        string[] segments,
        TarotInterpretationLocale locale,
        string path,
        byte[] bytes,
        string hash,
        InterpretationDiagnosticBag diagnostics)
    {
        var parsed = TarotInterpretationJson.Parse<TarotOrientedPairDocument>(bytes);
        if (!AddParse(parsed, path, bytes, diagnostics) || parsed.Document is null)
        {
            return null;
        }

        var validated = TarotInterpretationValidator.ValidateOrientedPair(parsed.Document, StandardTarotCatalog.Deck);
        diagnostics.AddValidation(path, validated.Diagnostics);
        if (!validated.IsValid)
        {
            return null;
        }

        var value = validated.Value!;
        var state = TarotSchemaText.Get(value.OrientationState, TarotSchemaText.PairStates);
        var key = TarotInterpretationKeys.CreateOrientedPair(value.CardAId, value.CardBId, value.OrientationState);
        var expected = $"content/{locale.Value}/shared/oriented-pairs/{value.CardAId.Value}__{value.CardBId.Value}/{state}.json";
        if (path != expected)
        {
            diagnostics.Error("content.identity-path", path, "Oriented-pair identity does not match its canonical path.");
            return null;
        }

        return new(AcceptedContentKind.OrientedPair, locale, TarotInterpretationCorpus.OrientedPairs, key, path, hash, bytes);
    }

    private static AcceptedContentFile? ReadPosition(
        string[] segments,
        TarotInterpretationLocale locale,
        string path,
        byte[] bytes,
        string hash,
        InterpretationDiagnosticBag diagnostics)
    {
        var parsed = TarotInterpretationJson.Parse<TarotThreeCardPositionDocument>(bytes);
        if (!AddParse(parsed, path, bytes, diagnostics) || parsed.Document is null)
        {
            return null;
        }

        var validated = TarotInterpretationValidator.ValidateThreeCardPosition(parsed.Document, StandardTarotCatalog.Deck);
        diagnostics.AddValidation(path, validated.Diagnostics);
        if (!validated.IsValid)
        {
            return null;
        }

        var value = validated.Value!;
        var position = TarotSchemaText.Get(value.Position, TarotSchemaText.Positions);
        var orientation = TarotSchemaText.Get(value.Orientation, TarotSchemaText.CardOrientations);
        var key = TarotInterpretationKeys.CreateThreeCardPosition(value.Position, value.CardId, value.Orientation);
        var expected = $"content/{locale.Value}/modes/three-cards/positions/{position}/{value.CardId.Value}/{orientation}.json";
        if (path != expected)
        {
            diagnostics.Error("content.identity-path", path, "Three-card position identity does not match its canonical path.");
            return null;
        }

        return new(AcceptedContentKind.ThreeCardPosition, locale, TarotInterpretationCorpus.ThreeCards, key, path, hash, bytes);
    }

    private static AcceptedContentFile? ReadSynthesis(
        string[] segments,
        TarotInterpretationLocale locale,
        string path,
        byte[] bytes,
        string hash,
        InterpretationDiagnosticBag diagnostics)
    {
        try
        {
            using var json = JsonDocument.Parse(bytes, new JsonDocumentOptions
            {
                AllowTrailingCommas = false,
                CommentHandling = JsonCommentHandling.Disallow
            });
            var root = json.RootElement;
            InterpretationPackValidator.ValidateCanonicalBytes(
                bytes,
                TarotInterpretationJson.Serialize(root),
                "content.canonical-bytes",
                path,
                diagnostics);
            if (root.ValueKind != JsonValueKind.Object ||
                !root.TryGetProperty("schemaVersion", out var schema) || schema.GetInt32() != 1 ||
                !root.TryGetProperty("resourceType", out var typeValue) || typeValue.GetString() is not { } typeText ||
                !root.TryGetProperty("resourceId", out var idValue) || idValue.GetString() is not { } idText ||
                !TarotSchemaText.TryParse(typeText, TarotSchemaText.SynthesisResourceTypes, out var type))
            {
                diagnostics.Error("content.synthesis-schema", path, "Synthesis resource requires schemaVersion 1, resourceType, and resourceId.");
                return null;
            }

            var id = new TarotSynthesisResourceId(idText);
            var expected = $"content/{locale.Value}/modes/three-cards/synthesis/{id.Value}.json";
            if (path != expected)
            {
                diagnostics.Error("content.identity-path", path, "Synthesis resource identity does not match its canonical path.");
                return null;
            }

            var key = TarotInterpretationKeys.CreateSynthesisResource(type, id);
            return new(AcceptedContentKind.Synthesis, locale, TarotInterpretationCorpus.ThreeCards, key, path, hash, bytes);
        }
        catch (Exception exception) when (exception is JsonException or ArgumentException or InvalidOperationException)
        {
            diagnostics.Error("content.synthesis-schema", path, exception.Message);
            return null;
        }
    }

    private static bool AddParse<T>(
        TarotJsonParseResult<T> parsed,
        string path,
        byte[] bytes,
        InterpretationDiagnosticBag diagnostics)
        where T : class
    {
        if (parsed.IsSuccess)
        {
            InterpretationPackValidator.ValidateCanonicalBytes(
                bytes,
                TarotInterpretationJson.Serialize(parsed.Document!),
                "content.canonical-bytes",
                path,
                diagnostics);
            return true;
        }

        diagnostics.Error("content.json", path, parsed.Failure?.Message ?? "JSON parsing failed.");
        return false;
    }
}
