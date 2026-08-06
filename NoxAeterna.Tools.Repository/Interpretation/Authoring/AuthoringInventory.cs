using System.Globalization;
using System.Text.Json.Serialization;
using NoxAeterna.Domain.Tarot;
using NoxAeterna.Interpretation.Tarot.Contracts;
using NoxAeterna.Interpretation.Tarot.Keys;
using NoxAeterna.Interpretation.Tarot.Serialization;
using NoxAeterna.Tools.Repository.Interpretation.Analysis;
using NoxAeterna.Tools.Repository.Interpretation.Reports;

namespace NoxAeterna.Tools.Repository.Interpretation.Authoring;

public enum AuthoringEntryStatus
{
    Draft,
    Reviewed,
    Accepted
}

public sealed class AuthoringInventoryDocument
{
    [JsonPropertyOrder(0)] public int? SchemaVersion { get; set; }
    [JsonPropertyOrder(1)] public string? PackId { get; set; }
    [JsonPropertyOrder(2)] public List<AuthoringInventoryEntryDocument?>? Entries { get; set; }
}

public sealed class AuthoringInventoryEntryDocument
{
    [JsonPropertyOrder(0)] public string? Locale { get; set; }
    [JsonPropertyOrder(1)] public string? CorpusId { get; set; }
    [JsonPropertyOrder(2)] public string? EntryKey { get; set; }
    [JsonPropertyOrder(3)] public string? Status { get; set; }
    [JsonPropertyOrder(4)] public string? BatchId { get; set; }
    [JsonPropertyOrder(5)] public int? SourceRevision { get; set; }
    [JsonPropertyOrder(6)] public int? TranslationRevision { get; set; }
    [JsonPropertyOrder(7)] public string? Reviewer { get; set; }
    [JsonPropertyOrder(8)] public string? AcceptedAt { get; set; }
}

public sealed record AuthoringInventoryEntry(
    TarotInterpretationLocale Locale,
    TarotInterpretationCorpus CorpusId,
    string EntryKey,
    AuthoringEntryStatus Status,
    string BatchId,
    int SourceRevision,
    int? TranslationRevision,
    string? Reviewer,
    DateTimeOffset? AcceptedAt);

public sealed class AuthoringInventoryAnalyzer
{
    public InterpretationToolReport Analyze(string workingRoot, string? packRoot = null)
    {
        var root = Path.GetFullPath(workingRoot);
        if (!Directory.Exists(root))
        {
            throw new DirectoryNotFoundException($"Authoring working root does not exist: {root}");
        }

        var path = Path.Combine(root, "authoring-inventory.json");
        var diagnostics = new InterpretationDiagnosticBag();
        if (!File.Exists(path))
        {
            diagnostics.Error("authoring.inventory-missing", "authoring-inventory.json", "Authoring inventory is missing.");
            return new(diagnostics.Items);
        }

        var parsed = TarotInterpretationJson.Parse<AuthoringInventoryDocument>(File.ReadAllBytes(path));
        if (!parsed.IsSuccess || parsed.Document is null)
        {
            diagnostics.Error("authoring.json", "authoring-inventory.json", parsed.Failure?.Message ?? "Inventory JSON is malformed.");
            return new(diagnostics.Items);
        }

        var entries = Validate(parsed.Document, diagnostics, out var packId);
        if (packRoot is not null && packId is not null)
        {
            CompareProduction(entries, packRoot, diagnostics);
        }

        var counts = BuildCounts(entries);
        return new(diagnostics.Items, counts);
    }

    private static IReadOnlyList<AuthoringInventoryEntry> Validate(
        AuthoringInventoryDocument document,
        InterpretationDiagnosticBag diagnostics,
        out TarotInterpretationPackId? packId)
    {
        packId = null;
        if (document.SchemaVersion != 1)
        {
            diagnostics.Error("authoring.schema", "schemaVersion", "Only schemaVersion 1 is supported.");
        }

        try
        {
            if (document.PackId is null)
            {
                throw new ArgumentException("packId is required.");
            }

            packId = new TarotInterpretationPackId(document.PackId);
            if (packId.Value != document.PackId)
            {
                throw new ArgumentException("packId must already be canonical.");
            }
        }
        catch (ArgumentException exception)
        {
            diagnostics.Error("authoring.pack-id", "packId", exception.Message);
        }

        if (document.Entries is null)
        {
            diagnostics.Error("authoring.entries", "entries", "Entries array is required.");
            return [];
        }

        var result = new List<AuthoringInventoryEntry>();
        var identities = new HashSet<string>(StringComparer.Ordinal);
        for (var index = 0; index < document.Entries.Count; index++)
        {
            var raw = document.Entries[index];
            if (raw is null)
            {
                diagnostics.Error("authoring.entry-null", $"entries[{index}]", "Inventory entry cannot be null.");
                continue;
            }

            var entry = ValidateEntry(raw, index, diagnostics);
            if (entry is null)
            {
                continue;
            }

            var identity = $"{entry.Locale.Value}|{TarotSchemaText.Get(entry.CorpusId, TarotSchemaText.Corpora)}|{entry.EntryKey}";
            if (!identities.Add(identity))
            {
                diagnostics.Error("authoring.duplicate", identity, "Locale/corpus/entryKey must be unique.");
                continue;
            }

            result.Add(entry);
        }

        return Array.AsReadOnly(result
            .OrderBy(item => item.Locale.Value, StringComparer.Ordinal)
            .ThenBy(item => TarotSchemaText.Get(item.CorpusId, TarotSchemaText.Corpora), StringComparer.Ordinal)
            .ThenBy(item => item.EntryKey, StringComparer.Ordinal)
            .ToArray());
    }

    private static AuthoringInventoryEntry? ValidateEntry(
        AuthoringInventoryEntryDocument raw,
        int index,
        InterpretationDiagnosticBag diagnostics)
    {
        var field = $"entries[{index}]";
        TarotInterpretationLocale? locale = null;
        if (raw.Locale is not null)
        {
            try
            {
                locale = new(raw.Locale);
            }
            catch (ArgumentException exception)
            {
                diagnostics.Error("authoring.locale", $"{field}.locale", exception.Message);
            }
        }
        else
        {
            diagnostics.Error("authoring.locale", $"{field}.locale", "Locale is required.");
        }

        var corpus = default(TarotInterpretationCorpus);
        var corpusValid = raw.CorpusId is not null &&
                          TarotSchemaText.TryParse(raw.CorpusId, TarotSchemaText.Corpora, out corpus);
        if (!corpusValid)
        {
            diagnostics.Error("authoring.corpus", $"{field}.corpusId", "A known corpusId is required.");
        }

        var keyValid = raw.EntryKey is not null && corpus switch
        {
            TarotInterpretationCorpus.SingleCard => TarotInterpretationKeys.ParseSingleCard(raw.EntryKey).IsValid,
            TarotInterpretationCorpus.OrientedPairs => TarotInterpretationKeys.ParseOrientedPair(raw.EntryKey).IsValid,
            TarotInterpretationCorpus.ThreeCards => TarotInterpretationKeys.ParseThreeCardPosition(raw.EntryKey).IsValid ||
                                                     TarotInterpretationKeys.ParseSynthesisResource(raw.EntryKey).IsValid,
            _ => false
        };
        if (!keyValid)
        {
            diagnostics.Error("authoring.entry-key", $"{field}.entryKey", "entryKey is invalid for corpusId.");
        }

        var statusValid = TryStatus(raw.Status, out var status);
        if (!statusValid)
        {
            diagnostics.Error("authoring.status", $"{field}.status", "Status must be draft, reviewed, or accepted.");
        }

        if (string.IsNullOrWhiteSpace(raw.BatchId))
        {
            diagnostics.Error("authoring.batch", $"{field}.batchId", "batchId must be non-empty.");
        }

        if (raw.SourceRevision is null or <= 0)
        {
            diagnostics.Error("authoring.source-revision", $"{field}.sourceRevision", "sourceRevision must be positive.");
        }

        if (raw.TranslationRevision is <= 0)
        {
            diagnostics.Error("authoring.translation-revision", $"{field}.translationRevision", "translationRevision must be null or positive.");
        }

        if (status is AuthoringEntryStatus.Reviewed or AuthoringEntryStatus.Accepted && string.IsNullOrWhiteSpace(raw.Reviewer))
        {
            diagnostics.Error("authoring.reviewer", $"{field}.reviewer", "Reviewed and accepted entries require a reviewer.");
        }

        DateTimeOffset? acceptedAt = null;
        if (status == AuthoringEntryStatus.Accepted)
        {
            if (raw.AcceptedAt is null ||
                !DateTimeOffset.TryParse(raw.AcceptedAt, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsed) ||
                parsed.Offset != TimeSpan.Zero || !raw.AcceptedAt.EndsWith('Z'))
            {
                diagnostics.Error("authoring.accepted-at", $"{field}.acceptedAt", "Accepted entries require a UTC ISO-8601 acceptedAt value.");
            }
            else
            {
                acceptedAt = parsed;
            }
        }
        else if (raw.AcceptedAt is not null)
        {
            diagnostics.Error("authoring.accepted-at", $"{field}.acceptedAt", "Only accepted entries may declare acceptedAt.");
        }

        if (raw.TranslationRevision is { } translation && raw.SourceRevision is { } source && translation < source)
        {
            diagnostics.Warning("authoring.translation-stale", raw.EntryKey ?? field, "Translation revision is older than source revision.");
        }

        var reviewerValid = status is AuthoringEntryStatus.Draft || !string.IsNullOrWhiteSpace(raw.Reviewer);
        var acceptedAtValid = status == AuthoringEntryStatus.Accepted
            ? acceptedAt is not null
            : raw.AcceptedAt is null;
        return locale is null || !corpusValid || raw.EntryKey is null || !keyValid ||
               string.IsNullOrWhiteSpace(raw.BatchId) ||
               raw.SourceRevision is null || raw.SourceRevision <= 0 || raw.TranslationRevision is <= 0 ||
               !statusValid || !reviewerValid || !acceptedAtValid
            ? null
            : new(locale, corpus, raw.EntryKey, status, raw.BatchId, raw.SourceRevision.Value,
                raw.TranslationRevision, raw.Reviewer, acceptedAt);
    }

    private static void CompareProduction(
        IReadOnlyList<AuthoringInventoryEntry> entries,
        string packRoot,
        InterpretationDiagnosticBag diagnostics)
    {
        var production = new AcceptedContentDiscovery().Discover(packRoot);
        foreach (var diagnostic in production.Diagnostics)
        {
            if (diagnostic.Severity == InterpretationToolSeverity.Error)
            {
                diagnostics.Error(diagnostic.Code, diagnostic.Target, diagnostic.Message);
            }
            else
            {
                diagnostics.Warning(diagnostic.Code, diagnostic.Target, diagnostic.Message);
            }
        }

        var productionKeys = production.Files
            .Where(item => item.Corpus is not null && item.Key is not null)
            .Select(item => $"{item.Locale.Value}|{TarotSchemaText.Get(item.Corpus!.Value, TarotSchemaText.Corpora)}|{item.Key}")
            .ToHashSet(StringComparer.Ordinal);
        var acceptedKeys = entries.Where(item => item.Status == AuthoringEntryStatus.Accepted)
            .Select(item => $"{item.Locale.Value}|{TarotSchemaText.Get(item.CorpusId, TarotSchemaText.Corpora)}|{item.EntryKey}")
            .ToHashSet(StringComparer.Ordinal);

        foreach (var missing in acceptedKeys.Except(productionKeys, StringComparer.Ordinal).Order(StringComparer.Ordinal))
        {
            diagnostics.Error("authoring.accepted-missing-production", missing, "Accepted inventory entry is missing from production content.");
        }

        foreach (var missing in productionKeys.Except(acceptedKeys, StringComparer.Ordinal).Order(StringComparer.Ordinal))
        {
            diagnostics.Error("authoring.production-missing-inventory", missing, "Production accepted content is missing from accepted inventory.");
        }
    }

    private static IReadOnlyDictionary<string, int> BuildCounts(IReadOnlyList<AuthoringInventoryEntry> entries)
    {
        var counts = new Dictionary<string, int>(StringComparer.Ordinal)
        {
            ["total"] = entries.Count
        };
        AddGroups(counts, entries.GroupBy(item => $"locale:{item.Locale.Value}"));
        AddGroups(counts, entries.GroupBy(item => $"corpus:{TarotSchemaText.Get(item.CorpusId, TarotSchemaText.Corpora)}"));
        AddGroups(counts, entries.GroupBy(item => $"status:{StatusText(item.Status)}"));
        AddGroups(counts, entries.GroupBy(item => $"batch:{item.BatchId}"));
        return counts;
    }

    private static void AddGroups(
        IDictionary<string, int> target,
        IEnumerable<IGrouping<string, AuthoringInventoryEntry>> groups)
    {
        foreach (var group in groups.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            target[group.Key] = group.Count();
        }
    }

    private static bool TryStatus(string? value, out AuthoringEntryStatus status)
    {
        status = value switch
        {
            "draft" => AuthoringEntryStatus.Draft,
            "reviewed" => AuthoringEntryStatus.Reviewed,
            "accepted" => AuthoringEntryStatus.Accepted,
            _ => default
        };
        return value is "draft" or "reviewed" or "accepted";
    }

    private static string StatusText(AuthoringEntryStatus status) => status switch
    {
        AuthoringEntryStatus.Draft => "draft",
        AuthoringEntryStatus.Reviewed => "reviewed",
        AuthoringEntryStatus.Accepted => "accepted",
        _ => throw new ArgumentOutOfRangeException(nameof(status))
    };
}
