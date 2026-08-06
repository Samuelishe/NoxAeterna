using System.Collections.ObjectModel;
using System.Security.Cryptography;
using NoxAeterna.Domain.Tarot;
using NoxAeterna.Interpretation.Tarot.Contracts;
using NoxAeterna.Interpretation.Tarot.Keys;
using NoxAeterna.Interpretation.Tarot.Serialization;
using NoxAeterna.Interpretation.Tarot.Sources;
using NoxAeterna.Interpretation.Tarot.Validation;

namespace NoxAeterna.Interpretation.Tarot.Resolution;

/// <summary>Resolves exact validated Tarot entries through one locale and a package hash trust chain.</summary>
public sealed class TarotInterpretationPackResolver
{
    private static readonly TarotInterpretationLocale English = new("en");
    private static readonly TarotInterpretationLocale Russian = new("ru");

    private readonly ITarotInterpretationPackSourceCatalog sourceCatalog;
    private readonly TarotDeckDefinition semanticDeck;
    private readonly TarotBoundedLruCache<ManifestCacheKey, TarotInterpretationPackManifest> manifests;
    private readonly TarotBoundedLruCache<IndexCacheKey, CachedIndex> indexes;
    private readonly TarotBoundedLruCache<ContentCacheKey, object> entries;

    public TarotInterpretationPackResolver(
        ITarotInterpretationPackSourceCatalog sourceCatalog,
        TarotDeckDefinition semanticDeck,
        TarotInterpretationResolverOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(sourceCatalog);
        ArgumentNullException.ThrowIfNull(semanticDeck);
        this.sourceCatalog = sourceCatalog;
        this.semanticDeck = semanticDeck;
        options ??= new TarotInterpretationResolverOptions();
        manifests = new TarotBoundedLruCache<ManifestCacheKey, TarotInterpretationPackManifest>(options.ManifestCapacity);
        indexes = new TarotBoundedLruCache<IndexCacheKey, CachedIndex>(options.IndexCapacity);
        entries = new TarotBoundedLruCache<ContentCacheKey, object>(options.EntryCapacity);
    }

    public TarotInterpretationResolution<TarotSingleCardEntry> ResolveSingleCard(
        TarotInterpretationPackId packId,
        TarotInterpretationLocale requestedLocale,
        TarotCardId cardId,
        TarotCardOrientation orientation)
    {
        ArgumentNullException.ThrowIfNull(cardId);
        var key = TarotInterpretationKeys.CreateSingleCard(cardId, orientation);
        var preparation = Prepare(packId, requestedLocale, TarotInterpretationMode.SingleCard);
        if (preparation.Context is null)
        {
            return preparation.NoContent<TarotSingleCardEntry>();
        }

        var context = preparation.Context;
        var path = new TarotPackageRelativePath(
            $"content/{context.ResolvedLocale.Value}/modes/single-card/{cardId.Value}/{TarotSchemaText.Get(orientation, TarotSchemaText.CardOrientations)}.json");
        return ResolveEntry<TarotSingleCardDocument, TarotSingleCardEntry>(
            context,
            TarotInterpretationCorpus.SingleCard,
            key,
            path,
            TarotInterpretationValidator.ValidateSingleCard,
            entry => entry.CardId == cardId && entry.Orientation == orientation);
    }

    public TarotInterpretationResolution<TarotOrientedPairEntry> ResolveOrientedPair(
        TarotInterpretationPackId packId,
        TarotInterpretationMode modeId,
        TarotInterpretationLocale requestedLocale,
        TarotCardId firstCardId,
        TarotCardOrientation firstOrientation,
        TarotCardId secondCardId,
        TarotCardOrientation secondOrientation)
    {
        ArgumentNullException.ThrowIfNull(firstCardId);
        ArgumentNullException.ThrowIfNull(secondCardId);
        if (modeId is not (TarotInterpretationMode.TwoCards or TarotInterpretationMode.ThreeCards))
        {
            return NoContent<TarotOrientedPairEntry>(
                TarotNoContentReason.UnsupportedMode,
                "request.unsupported-mode",
                "Oriented-pair resolution supports only two-cards and three-cards.");
        }

        var canonical = TarotInterpretationKeys.CanonicalizePair(
            firstCardId,
            firstOrientation,
            secondCardId,
            secondOrientation);
        if (!canonical.IsValid || canonical.Value is null)
        {
            return NoContent<TarotOrientedPairEntry>(
                TarotNoContentReason.ValidationFailed,
                "request.invalid-pair",
                FirstDiagnostic(canonical.Diagnostics, "The requested pair is invalid."));
        }

        var pair = canonical.Value;
        var key = TarotInterpretationKeys.CreateOrientedPair(pair.CardAId, pair.CardBId, pair.OrientationState);
        var preparation = Prepare(packId, requestedLocale, modeId);
        if (preparation.Context is null)
        {
            return preparation.NoContent<TarotOrientedPairEntry>();
        }

        var context = preparation.Context;
        var state = TarotSchemaText.Get(pair.OrientationState, TarotSchemaText.PairStates);
        var path = new TarotPackageRelativePath(
            $"content/{context.ResolvedLocale.Value}/shared/oriented-pairs/{pair.CardAId.Value}__{pair.CardBId.Value}/{state}.json");
        return ResolveEntry<TarotOrientedPairDocument, TarotOrientedPairEntry>(
            context,
            TarotInterpretationCorpus.OrientedPairs,
            key,
            path,
            TarotInterpretationValidator.ValidateOrientedPair,
            entry => entry.CardAId == pair.CardAId && entry.CardBId == pair.CardBId &&
                     entry.OrientationState == pair.OrientationState);
    }

    public TarotInterpretationResolution<TarotThreeCardPositionEntry> ResolveThreeCardPosition(
        TarotInterpretationPackId packId,
        TarotInterpretationLocale requestedLocale,
        TarotThreeCardPosition position,
        TarotCardId cardId,
        TarotCardOrientation orientation)
    {
        ArgumentNullException.ThrowIfNull(cardId);
        var key = TarotInterpretationKeys.CreateThreeCardPosition(position, cardId, orientation);
        var preparation = Prepare(packId, requestedLocale, TarotInterpretationMode.ThreeCards);
        if (preparation.Context is null)
        {
            return preparation.NoContent<TarotThreeCardPositionEntry>();
        }

        var context = preparation.Context;
        var path = new TarotPackageRelativePath(
            $"content/{context.ResolvedLocale.Value}/modes/three-cards/positions/{TarotSchemaText.Get(position, TarotSchemaText.Positions)}/{cardId.Value}/{TarotSchemaText.Get(orientation, TarotSchemaText.CardOrientations)}.json");
        return ResolveEntry<TarotThreeCardPositionDocument, TarotThreeCardPositionEntry>(
            context,
            TarotInterpretationCorpus.ThreeCards,
            key,
            path,
            TarotInterpretationValidator.ValidateThreeCardPosition,
            entry => entry.Position == position && entry.CardId == cardId && entry.Orientation == orientation);
    }

    public TarotInterpretationResolution<TarotResolvedModuleSnapshot> ResolveMode(
        TarotInterpretationPackId packId,
        TarotInterpretationMode modeId,
        TarotInterpretationLocale requestedLocale)
    {
        if (!Enum.IsDefined(modeId))
        {
            return NoContent<TarotResolvedModuleSnapshot>(
                TarotNoContentReason.UnsupportedMode,
                "request.unsupported-mode",
                "The requested interpretation mode is not defined.");
        }

        var preparation = Prepare(packId, requestedLocale, modeId);
        if (preparation.Context is null)
        {
            return preparation.NoContent<TarotResolvedModuleSnapshot>();
        }

        var context = preparation.Context;
        return new ResolvedTarotInterpretation<TarotResolvedModuleSnapshot>(
            context.Manifest.PackId,
            context.Manifest.ContentVersion,
            context.Mode,
            context.RequestedLocale,
            context.ResolvedLocale,
            new TarotResolvedModuleSnapshot(context.Indexes.Keys));
    }

    public void InvalidatePack(TarotInterpretationPackId packId)
    {
        ArgumentNullException.ThrowIfNull(packId);
        manifests.RemoveWhere(key => key.PackId == packId);
        indexes.RemoveWhere(key => key.PackId == packId);
        entries.RemoveWhere(key => key.PackId == packId);
    }

    public void Clear()
    {
        manifests.Clear();
        indexes.Clear();
        entries.Clear();
    }

    private PreparationResult Prepare(
        TarotInterpretationPackId packId,
        TarotInterpretationLocale requestedLocale,
        TarotInterpretationMode mode)
    {
        ArgumentNullException.ThrowIfNull(packId);
        ArgumentNullException.ThrowIfNull(requestedLocale);
        if (!sourceCatalog.TryGetSource(packId, out var source) || source is null)
        {
            return PreparationResult.Failed(
                TarotNoContentReason.PackUnavailable,
                "source.pack-unavailable",
                "The requested interpretation pack source is unavailable.");
        }

        if (source.PackId != packId || string.IsNullOrWhiteSpace(source.SnapshotId))
        {
            return PreparationResult.Failed(
                TarotNoContentReason.ValidationFailed,
                "source.identity",
                "The source identity or snapshot identity is invalid.");
        }

        var manifestResult = LoadManifest(source);
        if (manifestResult.Value is null)
        {
            return PreparationResult.Failed(
                TarotNoContentReason.ValidationFailed,
                manifestResult.Code,
                manifestResult.Message);
        }

        var manifest = manifestResult.Value;
        foreach (var locale in LocaleChain(requestedLocale))
        {
            if (!manifest.DeclaredLocales.Contains(locale))
            {
                continue;
            }

            if (!manifest.Modules.TryGetValue(mode, out var localeModules) ||
                !localeModules.TryGetValue(locale, out var module))
            {
                return PreparationResult.Failed(
                    TarotNoContentReason.ValidationFailed,
                    "manifest.module-missing",
                    "A validated manifest did not contain the requested mode/locale module.");
            }

            if (!module.Ready)
            {
                continue;
            }

            var loadedIndexes = LoadRequiredIndexes(source, manifest, mode, locale, module);
            if (loadedIndexes.Value is null)
            {
                return PreparationResult.Failed(
                    TarotNoContentReason.BrokenReadyModule,
                    loadedIndexes.Code,
                    loadedIndexes.Message);
            }

            return PreparationResult.Resolved(new ResolutionContext(
                source,
                manifest,
                mode,
                requestedLocale,
                locale,
                loadedIndexes.Value));
        }

        return PreparationResult.Failed(
            TarotNoContentReason.NoReadyLocale,
            "locale.no-ready-module",
            "No locale in the resolution chain declares this mode ready.");
    }

    private LoadResult<TarotInterpretationPackManifest> LoadManifest(ITarotInterpretationPackSource source)
    {
        var key = new ManifestCacheKey(source.PackId, source.SnapshotId);
        if (manifests.TryGetValue(key, out var cached) && cached is not null)
        {
            return LoadResult<TarotInterpretationPackManifest>.Success(cached);
        }

        var read = source.ReadManifest();
        if (read.Status != TarotInterpretationSourceReadStatus.Found)
        {
            return LoadResult<TarotInterpretationPackManifest>.Failure(
                read.Diagnostic?.Code ?? "manifest.missing",
                read.Diagnostic?.Message ?? "The interpretation pack manifest is missing.");
        }

        var parsed = TarotInterpretationJson.Parse<TarotInterpretationPackDocument>(read.Bytes.Span);
        if (!parsed.IsSuccess || parsed.Document is null)
        {
            return LoadResult<TarotInterpretationPackManifest>.Failure(
                "manifest.json",
                parsed.Failure?.Message ?? "The interpretation pack manifest is malformed.");
        }

        var validated = TarotInterpretationValidator.ValidateManifest(parsed.Document);
        if (!validated.IsValid || validated.Value is null)
        {
            return LoadResult<TarotInterpretationPackManifest>.Failure(
                "manifest.validation",
                FirstDiagnostic(validated.Diagnostics, "The interpretation pack manifest is invalid."));
        }

        var manifest = validated.Value;
        if (manifest.PackId != source.PackId || manifest.SemanticDeckId != semanticDeck.Id)
        {
            return LoadResult<TarotInterpretationPackManifest>.Failure(
                "manifest.identity",
                "The manifest pack or semantic-deck identity does not match the selected source.");
        }

        manifests.Set(key, manifest);
        return LoadResult<TarotInterpretationPackManifest>.Success(manifest);
    }

    private LoadResult<IReadOnlyDictionary<TarotInterpretationCorpus, CachedIndex>> LoadRequiredIndexes(
        ITarotInterpretationPackSource source,
        TarotInterpretationPackManifest manifest,
        TarotInterpretationMode mode,
        TarotInterpretationLocale locale,
        TarotInterpretationModule module)
    {
        var specifications = ExpectedIndexes(mode, locale);
        if (!module.IndexPaths.Select(path => path.Value)
                .SequenceEqual(specifications.Select(item => item.Path.Value), StringComparer.Ordinal))
        {
            return LoadResult<IReadOnlyDictionary<TarotInterpretationCorpus, CachedIndex>>.Failure(
                "module.index-contract",
                "The ready module index declarations do not match the mode contract.");
        }

        var loaded = new Dictionary<TarotInterpretationCorpus, CachedIndex>();
        foreach (var specification in specifications)
        {
            var reference = manifest.IndexFiles.SingleOrDefault(item => item.Path == specification.Path);
            if (reference is null)
            {
                return LoadResult<IReadOnlyDictionary<TarotInterpretationCorpus, CachedIndex>>.Failure(
                    "index.reference-missing",
                    $"Required index '{specification.Path.Value}' is absent from the manifest trust chain.");
            }

            var indexResult = LoadIndex(source, manifest, locale, specification, reference);
            if (indexResult.Value is null)
            {
                return LoadResult<IReadOnlyDictionary<TarotInterpretationCorpus, CachedIndex>>.Failure(
                    indexResult.Code,
                    indexResult.Message);
            }

            loaded.Add(specification.Corpus, indexResult.Value);
        }

        return LoadResult<IReadOnlyDictionary<TarotInterpretationCorpus, CachedIndex>>.Success(
            new ReadOnlyDictionary<TarotInterpretationCorpus, CachedIndex>(loaded));
    }

    private LoadResult<CachedIndex> LoadIndex(
        ITarotInterpretationPackSource source,
        TarotInterpretationPackManifest manifest,
        TarotInterpretationLocale locale,
        IndexSpecification specification,
        TarotInterpretationIndexFile reference)
    {
        var key = new IndexCacheKey(
            manifest.PackId,
            source.SnapshotId,
            manifest.ContentVersion,
            locale.Value,
            specification.Path.Value,
            reference.Sha256.Value);
        if (indexes.TryGetValue(key, out var cached) && cached is not null)
        {
            return LoadResult<CachedIndex>.Success(cached);
        }

        var read = source.ReadPackageFile(specification.Path);
        if (read.Status != TarotInterpretationSourceReadStatus.Found)
        {
            return LoadResult<CachedIndex>.Failure(
                read.Diagnostic?.Code ?? "index.missing",
                read.Diagnostic?.Message ?? $"Required index '{specification.Path.Value}' is missing.");
        }

        if (!Hash(read.Bytes.Span).Equals(reference.Sha256.Value, StringComparison.Ordinal))
        {
            return LoadResult<CachedIndex>.Failure("index.hash", "Index bytes do not match the manifest SHA-256.");
        }

        var parsed = TarotInterpretationJson.Parse<TarotGeneratedIndexDocument>(read.Bytes.Span);
        if (!parsed.IsSuccess || parsed.Document is null)
        {
            return LoadResult<CachedIndex>.Failure(
                "index.json",
                parsed.Failure?.Message ?? "The generated index is malformed.");
        }

        var validated = TarotInterpretationValidator.ValidateGeneratedIndex(parsed.Document);
        if (!validated.IsValid || validated.Value is null)
        {
            return LoadResult<CachedIndex>.Failure(
                "index.validation",
                FirstDiagnostic(validated.Diagnostics, "The generated index is invalid."));
        }

        var index = validated.Value;
        if (index.PackId != manifest.PackId || index.Locale != locale ||
            index.ContentVersion != manifest.ContentVersion || index.CorpusId != specification.Corpus)
        {
            return LoadResult<CachedIndex>.Failure(
                "index.identity",
                "The generated index identity does not match the selected manifest module.");
        }

        var expectedPrefix = $"content/{locale.Value}/";
        if (index.Entries.Any(entry => !entry.Path.Value.StartsWith(expectedPrefix, StringComparison.Ordinal)))
        {
            return LoadResult<CachedIndex>.Failure(
                "index.locale-integrity",
                "Every generated-index entry must remain inside the resolved locale.");
        }

        var materialized = new CachedIndex(index, reference.Sha256);
        indexes.Set(key, materialized);
        return LoadResult<CachedIndex>.Success(materialized);
    }

    private TarotInterpretationResolution<TContent> ResolveEntry<TDocument, TContent>(
        ResolutionContext context,
        TarotInterpretationCorpus corpus,
        string key,
        TarotPackageRelativePath expectedPath,
        Func<TDocument, TarotDeckDefinition, TarotValidationResult<TContent>> validator,
        Func<TContent, bool> identityMatches)
        where TDocument : class
        where TContent : class
    {
        if (!context.Indexes.TryGetValue(corpus, out var index) ||
            !index.Entries.TryGetValue(key, out var route))
        {
            return Broken<TContent>("content.route-missing", "The canonical entry key is absent from the required index.");
        }

        if (route.Path != expectedPath)
        {
            return Broken<TContent>("content.path-identity", "The indexed path does not match the canonical entry identity.");
        }

        var cacheKey = new ContentCacheKey(
            context.Manifest.PackId,
            context.Source.SnapshotId,
            context.Manifest.ContentVersion,
            context.ResolvedLocale.Value,
            context.Mode,
            corpus,
            key,
            index.Hash.Value);
        if (entries.TryGetValue(cacheKey, out var cached) && cached is TContent cachedContent)
        {
            return Resolved(context, cachedContent);
        }

        var read = context.Source.ReadPackageFile(route.Path);
        if (read.Status != TarotInterpretationSourceReadStatus.Found)
        {
            return Broken<TContent>(
                read.Diagnostic?.Code ?? "content.missing",
                read.Diagnostic?.Message ?? "The indexed interpretation entry is missing.");
        }

        if (!Hash(read.Bytes.Span).Equals(route.Sha256.Value, StringComparison.Ordinal))
        {
            return Broken<TContent>("content.hash", "Interpretation entry bytes do not match the index SHA-256.");
        }

        var parsed = TarotInterpretationJson.Parse<TDocument>(read.Bytes.Span);
        if (!parsed.IsSuccess || parsed.Document is null)
        {
            return Broken<TContent>(
                "content.json",
                parsed.Failure?.Message ?? "The interpretation entry is malformed.");
        }

        var validated = validator(parsed.Document, semanticDeck);
        if (!validated.IsValid || validated.Value is null)
        {
            return Broken<TContent>(
                "content.validation",
                FirstDiagnostic(validated.Diagnostics, "The interpretation entry is invalid."));
        }

        if (!identityMatches(validated.Value))
        {
            return Broken<TContent>("content.identity", "The interpretation entry identity does not match its canonical key.");
        }

        entries.Set(cacheKey, validated.Value);
        return Resolved(context, validated.Value);
    }

    private static ResolvedTarotInterpretation<TContent> Resolved<TContent>(
        ResolutionContext context,
        TContent content)
        where TContent : class => new(
        context.Manifest.PackId,
        context.Manifest.ContentVersion,
        context.Mode,
        context.RequestedLocale,
        context.ResolvedLocale,
        content);

    private static NoTarotInterpretationContent<TContent> Broken<TContent>(string code, string message)
        where TContent : class => NoContent<TContent>(TarotNoContentReason.BrokenReadyModule, code, message);

    private static NoTarotInterpretationContent<TContent> NoContent<TContent>(
        TarotNoContentReason reason,
        string code,
        string message)
        where TContent : class => new(reason, new TarotResolutionDiagnostic(code, message));

    private static IReadOnlyList<TarotInterpretationLocale> LocaleChain(TarotInterpretationLocale requestedLocale) =>
        new[] { requestedLocale, English, Russian }
            .Distinct()
            .ToArray();

    private static IndexSpecification[] ExpectedIndexes(
        TarotInterpretationMode mode,
        TarotInterpretationLocale locale) => mode switch
    {
        TarotInterpretationMode.SingleCard =>
        [
            new(TarotInterpretationCorpus.SingleCard, new TarotPackageRelativePath($"indexes/{locale.Value}/single-card.json"))
        ],
        TarotInterpretationMode.TwoCards =>
        [
            new(TarotInterpretationCorpus.OrientedPairs, new TarotPackageRelativePath($"indexes/{locale.Value}/oriented-pairs.json"))
        ],
        TarotInterpretationMode.ThreeCards =>
        [
            new(TarotInterpretationCorpus.OrientedPairs, new TarotPackageRelativePath($"indexes/{locale.Value}/oriented-pairs.json")),
            new(TarotInterpretationCorpus.ThreeCards, new TarotPackageRelativePath($"indexes/{locale.Value}/three-cards.json"))
        ],
        TarotInterpretationMode.CelticCross => [],
        _ => throw new ArgumentOutOfRangeException(nameof(mode))
    };

    private static string Hash(ReadOnlySpan<byte> bytes) => Convert.ToHexStringLower(SHA256.HashData(bytes));

    private static string FirstDiagnostic(
        IReadOnlyList<TarotValidationDiagnostic> diagnostics,
        string fallback) => diagnostics.FirstOrDefault(item => item.Severity == TarotValidationSeverity.Error)?.Message ?? fallback;

    private sealed record ManifestCacheKey(TarotInterpretationPackId PackId, string SnapshotId);

    private sealed record IndexCacheKey(
        TarotInterpretationPackId PackId,
        string SnapshotId,
        int ContentVersion,
        string Locale,
        string Path,
        string Hash);

    private sealed record ContentCacheKey(
        TarotInterpretationPackId PackId,
        string SnapshotId,
        int ContentVersion,
        string Locale,
        TarotInterpretationMode Mode,
        TarotInterpretationCorpus Corpus,
        string EntryKey,
        string IndexHash);

    private sealed class CachedIndex
    {
        public CachedIndex(TarotGeneratedIndex index, TarotSha256 hash)
        {
            Index = index;
            Hash = hash;
            Entries = new ReadOnlyDictionary<string, TarotGeneratedIndexEntry>(
                index.Entries.ToDictionary(item => item.Key, StringComparer.Ordinal));
        }

        public TarotGeneratedIndex Index { get; }
        public TarotSha256 Hash { get; }
        public IReadOnlyDictionary<string, TarotGeneratedIndexEntry> Entries { get; }
    }

    private sealed record IndexSpecification(
        TarotInterpretationCorpus Corpus,
        TarotPackageRelativePath Path);

    private sealed record ResolutionContext(
        ITarotInterpretationPackSource Source,
        TarotInterpretationPackManifest Manifest,
        TarotInterpretationMode Mode,
        TarotInterpretationLocale RequestedLocale,
        TarotInterpretationLocale ResolvedLocale,
        IReadOnlyDictionary<TarotInterpretationCorpus, CachedIndex> Indexes);

    private sealed class PreparationResult
    {
        private PreparationResult(
            ResolutionContext? context,
            TarotNoContentReason? reason,
            string? code,
            string? message)
        {
            Context = context;
            Reason = reason;
            Code = code;
            Message = message;
        }

        public ResolutionContext? Context { get; }
        private TarotNoContentReason? Reason { get; }
        private string? Code { get; }
        private string? Message { get; }

        public static PreparationResult Resolved(ResolutionContext context) => new(context, null, null, null);

        public static PreparationResult Failed(TarotNoContentReason reason, string code, string message) =>
            new(null, reason, code, message);

        public NoTarotInterpretationContent<TContent> NoContent<TContent>()
            where TContent : class => TarotInterpretationPackResolver.NoContent<TContent>(Reason!.Value, Code!, Message!);
    }

    private sealed class LoadResult<TValue>
        where TValue : class
    {
        private LoadResult(TValue? value, string code, string message) =>
            (Value, Code, Message) = (value, code, message);

        public TValue? Value { get; }
        public string Code { get; }
        public string Message { get; }

        public static LoadResult<TValue> Success(TValue value) => new(value, string.Empty, string.Empty);
        public static LoadResult<TValue> Failure(string code, string message) => new(null, code, message);
    }
}
