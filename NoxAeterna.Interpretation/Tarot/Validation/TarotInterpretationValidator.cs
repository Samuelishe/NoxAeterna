using NoxAeterna.Domain.Tarot;
using NoxAeterna.Interpretation.Tarot.Contracts;
using NoxAeterna.Interpretation.Tarot.Keys;
using NoxAeterna.Interpretation.Tarot.Serialization;

namespace NoxAeterna.Interpretation.Tarot.Validation;

/// <summary>Pure structural and semantic validation for untrusted Tarot schema documents.</summary>
public static class TarotInterpretationValidator
{
    private static readonly string[] SectionIds = ["situation", "development", "risk", "outcome", "advice"];

    public static TarotValidationResult<TarotInterpretationPackManifest> ValidateManifest(
        TarotInterpretationPackDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        var diagnostics = new TarotDiagnosticBag();
        ValidateSchemaVersion(document.SchemaVersion, "schemaVersion", diagnostics);
        var packId = ParsePackId(document.PackId, "packId", diagnostics);
        var deckId = ParseDeckId(document.SemanticDeckId, "semanticDeckId", diagnostics);
        var sourceLocale = ParseLocale(document.SourceLocale, "sourceLocale", diagnostics);
        ValidatePositive(document.ContentVersion, "contentVersion", diagnostics);

        var locales = ValidateLocales(document.DeclaredLocales, diagnostics);
        if (sourceLocale is not null && !locales.Contains(sourceLocale))
        {
            diagnostics.Error("manifest.source-locale", "sourceLocale", "The source locale must be declared.");
        }

        var displayNames = ValidateDisplayNames(document.DisplayNames, locales, diagnostics);
        var indexFiles = ValidateIndexFiles(document.IndexFiles, diagnostics);
        var indexedPaths = indexFiles.Select(item => item.Path.Value).ToHashSet(StringComparer.Ordinal);
        var modules = ValidateModules(document.Modules, locales, indexedPaths, diagnostics);

        TarotInterpretationPackManifest? value = null;
        if (!diagnostics.HasErrors && packId is not null && deckId is not null && sourceLocale is not null &&
            document.ContentVersion is { } contentVersion)
        {
            value = new TarotInterpretationPackManifest(
                packId,
                deckId,
                sourceLocale,
                contentVersion,
                locales,
                displayNames,
                modules,
                indexFiles);
        }

        return TarotValidationResult<TarotInterpretationPackManifest>.Create(value, diagnostics.Items);
    }

    public static TarotValidationResult<TarotVocabularyEntry> ValidateVocabulary(TarotVocabularyDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        var diagnostics = new TarotDiagnosticBag();
        ValidateSchemaVersion(document.SchemaVersion, "schemaVersion", diagnostics);
        var conceptId = ParseConceptId(document.ConceptId, "conceptId", diagnostics);
        ValidateText(document.Label, "label", diagnostics);
        ValidateText(document.Meaning, "meaning", diagnostics);
        var value = diagnostics.HasErrors || conceptId is null
            ? null
            : new TarotVocabularyEntry(conceptId, document.Label!, document.Meaning!);
        return TarotValidationResult<TarotVocabularyEntry>.Create(value, diagnostics.Items);
    }

    public static TarotValidationResult<TarotTagAssignment> ValidateTagAssignment(TarotTagAssignmentDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        var diagnostics = new TarotDiagnosticBag();
        var value = ValidateTag(document, "tag", diagnostics);
        return TarotValidationResult<TarotTagAssignment>.Create(value, diagnostics.Items);
    }

    public static TarotValidationResult<TarotSingleCardEntry> ValidateSingleCard(
        TarotSingleCardDocument document,
        TarotDeckDefinition deck)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(deck);
        var diagnostics = new TarotDiagnosticBag();
        ValidateSchemaVersion(document.SchemaVersion, "schemaVersion", diagnostics);
        var cardId = ParseKnownCard(document.CardId, "cardId", deck, diagnostics);
        var orientation = ValidateEnum(document.Orientation, "orientation", diagnostics);
        var sections = ValidateSections(document.Sections, diagnostics);
        var tags = ValidateTags(document.Tags, "tags", 5, 10, diagnostics);
        ValidateValence(document.OverallValence, "overallValence", diagnostics);
        ValidateIntensity(document.OverallIntensity, "overallIntensity", diagnostics);
        var mechanisms = ValidateReversalMechanisms(document.ReversalMechanisms, orientation, diagnostics);

        TarotSingleCardEntry? value = null;
        if (!diagnostics.HasErrors && cardId is not null && orientation is not null &&
            document.OverallValence is { } valence && document.OverallIntensity is { } intensity)
        {
            value = new TarotSingleCardEntry(cardId, orientation.Value, sections, tags, valence, intensity, mechanisms);
        }

        return TarotValidationResult<TarotSingleCardEntry>.Create(value, diagnostics.Items);
    }

    public static TarotValidationResult<TarotOrientedPairEntry> ValidateOrientedPair(
        TarotOrientedPairDocument document,
        TarotDeckDefinition deck)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(deck);
        var diagnostics = new TarotDiagnosticBag();
        ValidateSchemaVersion(document.SchemaVersion, "schemaVersion", diagnostics);
        var cardA = ParseKnownCard(document.CardAId, "cardAId", deck, diagnostics);
        var cardB = ParseKnownCard(document.CardBId, "cardBId", deck, diagnostics);
        if (cardA is not null && cardB is not null)
        {
            var comparison = StringComparer.Ordinal.Compare(cardA.Value, cardB.Value);
            if (comparison == 0)
            {
                diagnostics.Error("pair.self", "cardBId", "An oriented pair cannot contain the same card twice.");
            }
            else if (comparison > 0)
            {
                diagnostics.Error("pair.noncanonical", "cardAId", "cardAId must be ordinal-smaller than cardBId.");
            }
        }

        var state = ValidateEnum(document.OrientationState, "orientationState", diagnostics);
        ValidateText(document.Interaction, "interaction", diagnostics);
        ValidateText(document.Direction, "direction", diagnostics);
        var tags = ValidateTags(document.Tags, "tags", 6, 10, diagnostics);
        ValidateValence(document.OverallValence, "overallValence", diagnostics);
        ValidateIntensity(document.OverallIntensity, "overallIntensity", diagnostics);

        TarotOrientedPairEntry? value = null;
        if (!diagnostics.HasErrors && cardA is not null && cardB is not null && state is not null &&
            document.OverallValence is { } valence && document.OverallIntensity is { } intensity)
        {
            value = new TarotOrientedPairEntry(
                cardA,
                cardB,
                state.Value,
                document.Interaction!,
                document.Direction!,
                tags,
                valence,
                intensity);
        }

        return TarotValidationResult<TarotOrientedPairEntry>.Create(value, diagnostics.Items);
    }

    public static TarotValidationResult<TarotThreeCardPositionEntry> ValidateThreeCardPosition(
        TarotThreeCardPositionDocument document,
        TarotDeckDefinition deck)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(deck);
        var diagnostics = new TarotDiagnosticBag();
        ValidateSchemaVersion(document.SchemaVersion, "schemaVersion", diagnostics);
        var position = ValidateEnum(document.Position, "position", diagnostics);
        var cardId = ParseKnownCard(document.CardId, "cardId", deck, diagnostics);
        var orientation = ValidateEnum(document.Orientation, "orientation", diagnostics);
        ValidateText(document.Text, "text", diagnostics);
        var tags = ValidateTags(document.Tags, "tags", minimum: null, maximum: null, diagnostics);
        ValidateValence(document.OverallValence, "overallValence", diagnostics);
        ValidateIntensity(document.OverallIntensity, "overallIntensity", diagnostics);

        TarotThreeCardPositionEntry? value = null;
        if (!diagnostics.HasErrors && position is not null && cardId is not null && orientation is not null &&
            document.OverallValence is { } valence && document.OverallIntensity is { } intensity)
        {
            value = new TarotThreeCardPositionEntry(
                position.Value,
                cardId,
                orientation.Value,
                document.Text!,
                tags,
                valence,
                intensity);
        }

        return TarotValidationResult<TarotThreeCardPositionEntry>.Create(value, diagnostics.Items);
    }

    public static TarotValidationResult<TarotGeneratedIndex> ValidateGeneratedIndex(TarotGeneratedIndexDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        var diagnostics = new TarotDiagnosticBag();
        ValidateSchemaVersion(document.SchemaVersion, "schemaVersion", diagnostics);
        var packId = ParsePackId(document.PackId, "packId", diagnostics);
        var locale = ParseLocale(document.Locale, "locale", diagnostics);
        var corpus = ValidateEnum(document.CorpusId, "corpusId", diagnostics);
        ValidatePositive(document.ContentVersion, "contentVersion", diagnostics);
        ValidateNonNegative(document.ExpectedEntryCount, "expectedEntryCount", diagnostics);

        var entries = ValidateIndexEntries(document.Entries, corpus, diagnostics);
        if (document.ExpectedEntryCount is { } expected && entries.Count != expected)
        {
            diagnostics.Error("index.count", "entries", "entries count must match expectedEntryCount.");
        }

        ValidateCorpusCounts(document, corpus, entries, diagnostics);
        TarotGeneratedIndex? value = null;
        if (!diagnostics.HasErrors && packId is not null && locale is not null && corpus is not null &&
            document.ContentVersion is { } contentVersion && document.ExpectedEntryCount is { } expectedEntryCount)
        {
            value = new TarotGeneratedIndex(
                packId,
                locale,
                corpus.Value,
                contentVersion,
                expectedEntryCount,
                document.ExpectedIdentityCount,
                document.ExpectedPositionEntryCount,
                entries);
        }

        return TarotValidationResult<TarotGeneratedIndex>.Create(value, diagnostics.Items);
    }

    private static IReadOnlyList<TarotInterpretationLocale> ValidateLocales(
        List<string?>? rawLocales,
        TarotDiagnosticBag diagnostics)
    {
        if (rawLocales is null || rawLocales.Count == 0)
        {
            diagnostics.Error("manifest.locales", "declaredLocales", "At least one locale must be declared.");
            return [];
        }

        var locales = new List<TarotInterpretationLocale>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        for (var index = 0; index < rawLocales.Count; index++)
        {
            var locale = ParseLocale(rawLocales[index], $"declaredLocales[{index}]", diagnostics);
            if (locale is null)
            {
                continue;
            }

            if (!seen.Add(locale.Value))
            {
                diagnostics.Error("manifest.locale-duplicate", $"declaredLocales[{index}]", "Declared locales must be unique.");
                continue;
            }

            locales.Add(locale);
        }

        return Array.AsReadOnly(locales.ToArray());
    }

    private static IReadOnlyDictionary<TarotInterpretationLocale, string> ValidateDisplayNames(
        Dictionary<string, string?>? rawNames,
        IReadOnlyList<TarotInterpretationLocale> locales,
        TarotDiagnosticBag diagnostics)
    {
        var result = new Dictionary<TarotInterpretationLocale, string>();
        if (rawNames is null)
        {
            diagnostics.Error("manifest.display-names", "displayNames", "Display names are required.");
            return result;
        }

        foreach (var locale in locales)
        {
            if (!rawNames.TryGetValue(locale.Value, out var name) || string.IsNullOrWhiteSpace(name))
            {
                diagnostics.Error("manifest.display-name", $"displayNames.{locale.Value}", "A non-empty display name is required.");
                continue;
            }

            result.Add(locale, name);
        }

        foreach (var locale in rawNames.Keys.Where(key => locales.All(item => item.Value != key)))
        {
            diagnostics.Error("manifest.display-name-locale", $"displayNames.{locale}", "Display-name locale is not declared.");
        }

        return result;
    }

    private static IReadOnlyList<TarotInterpretationIndexFile> ValidateIndexFiles(
        List<TarotInterpretationIndexFileDocument?>? rawFiles,
        TarotDiagnosticBag diagnostics)
    {
        if (rawFiles is null)
        {
            diagnostics.Error("manifest.index-files", "indexFiles", "The indexFiles array is required, even when empty.");
            return [];
        }

        var result = new List<TarotInterpretationIndexFile>();
        var paths = new HashSet<string>(StringComparer.Ordinal);
        for (var index = 0; index < rawFiles.Count; index++)
        {
            var raw = rawFiles[index];
            if (raw is null)
            {
                diagnostics.Error("manifest.index-file-null", $"indexFiles[{index}]", "Index-file references cannot be null.");
                continue;
            }

            var path = ParsePath(raw.Path, $"indexFiles[{index}].path", diagnostics);
            var hash = ParseHash(raw.Sha256, $"indexFiles[{index}].sha256", diagnostics);
            if (path is not null && !paths.Add(path.Value))
            {
                diagnostics.Error("manifest.index-path-duplicate", $"indexFiles[{index}].path", "Index-file paths must be unique.");
            }

            if (path is not null && hash is not null)
            {
                result.Add(new TarotInterpretationIndexFile(path, hash));
            }
        }

        return Array.AsReadOnly(result.ToArray());
    }

    private static IReadOnlyDictionary<TarotInterpretationMode, IReadOnlyDictionary<TarotInterpretationLocale, TarotInterpretationModule>> ValidateModules(
        Dictionary<string, Dictionary<string, TarotInterpretationModuleDocument?>?>? rawModules,
        IReadOnlyList<TarotInterpretationLocale> locales,
        IReadOnlySet<string> indexedPaths,
        TarotDiagnosticBag diagnostics)
    {
        var result = new Dictionary<TarotInterpretationMode, IReadOnlyDictionary<TarotInterpretationLocale, TarotInterpretationModule>>();
        if (rawModules is null)
        {
            diagnostics.Error("manifest.modules", "modules", "The module matrix is required.");
            return result;
        }

        foreach (var unknown in rawModules.Keys.Where(key =>
                     !TarotSchemaText.TryParse(key, TarotSchemaText.Modes, out _)))
        {
            diagnostics.Error("manifest.mode", $"modules.{unknown}", "Unknown interpretation mode.");
        }

        foreach (var mode in Enum.GetValues<TarotInterpretationMode>())
        {
            var modeText = TarotSchemaText.Get(mode, TarotSchemaText.Modes);
            if (!rawModules.TryGetValue(modeText, out var rawLocales) || rawLocales is null)
            {
                diagnostics.Error("manifest.mode-missing", $"modules.{modeText}", "Every schema-v1 mode is required.");
                continue;
            }

            foreach (var extraLocale in rawLocales.Keys.Where(key => locales.All(locale => locale.Value != key)))
            {
                diagnostics.Error("manifest.module-locale", $"modules.{modeText}.{extraLocale}", "Module locale is not declared.");
            }

            var localeModules = new Dictionary<TarotInterpretationLocale, TarotInterpretationModule>();
            foreach (var locale in locales)
            {
                if (!rawLocales.TryGetValue(locale.Value, out var raw) || raw is null)
                {
                    diagnostics.Error("manifest.module-missing", $"modules.{modeText}.{locale.Value}", "Every declared locale requires a module entry.");
                    continue;
                }

                var module = ValidateModule(raw, mode, locale, indexedPaths, diagnostics);
                if (module is not null)
                {
                    localeModules.Add(locale, module);
                }
            }

            result[mode] = localeModules;
        }

        return result;
    }

    private static TarotInterpretationModule? ValidateModule(
        TarotInterpretationModuleDocument raw,
        TarotInterpretationMode mode,
        TarotInterpretationLocale locale,
        IReadOnlySet<string> indexedPaths,
        TarotDiagnosticBag diagnostics)
    {
        var field = $"modules.{TarotSchemaText.Get(mode, TarotSchemaText.Modes)}.{locale.Value}";
        if (raw.Ready is null)
        {
            diagnostics.Error("manifest.ready", $"{field}.ready", "A manual readiness flag is required.");
        }

        var paths = new List<TarotPackageRelativePath>();
        if (raw.IndexPaths is null)
        {
            diagnostics.Error("manifest.index-paths", $"{field}.indexPaths", "indexPaths is required.");
        }
        else
        {
            for (var index = 0; index < raw.IndexPaths.Count; index++)
            {
                var path = ParsePath(raw.IndexPaths[index], $"{field}.indexPaths[{index}]", diagnostics);
                if (path is not null)
                {
                    paths.Add(path);
                }
            }
        }

        var dependencies = new List<TarotModuleDependency>();
        if (raw.Dependencies is null)
        {
            diagnostics.Error("manifest.dependencies", $"{field}.dependencies", "dependencies is required.");
        }
        else
        {
            for (var index = 0; index < raw.Dependencies.Count; index++)
            {
                var dependency = raw.Dependencies[index];
                if (dependency is null || !Enum.IsDefined(dependency.Value))
                {
                    diagnostics.Error("manifest.dependency", $"{field}.dependencies[{index}]", "Unknown module dependency.");
                    continue;
                }

                if (dependencies.Contains(dependency.Value))
                {
                    diagnostics.Error("manifest.dependency-duplicate", $"{field}.dependencies[{index}]", "Module dependencies must be unique.");
                    continue;
                }

                dependencies.Add(dependency.Value);
            }
        }

        var expectedPaths = ExpectedIndexPaths(mode, locale.Value);
        var expectedDependencies = ExpectedDependencies(mode);
        if (!paths.Select(path => path.Value).SequenceEqual(expectedPaths, StringComparer.Ordinal))
        {
            diagnostics.Error("manifest.index-contract", $"{field}.indexPaths", "Index paths do not match the schema-v1 mode contract.");
        }

        if (!dependencies.SequenceEqual(expectedDependencies))
        {
            diagnostics.Error("manifest.dependency-contract", $"{field}.dependencies", "Dependencies do not match the schema-v1 mode contract.");
        }

        if (raw.Ready == true)
        {
            foreach (var path in expectedPaths.Where(path => !indexedPaths.Contains(path)))
            {
                diagnostics.Error("manifest.ready-index", field, $"Ready module index '{path}' is absent from indexFiles.");
            }
        }

        return raw.Ready is null || raw.IndexPaths is null || raw.Dependencies is null
            ? null
            : new TarotInterpretationModule(raw.Ready.Value, paths, dependencies);
    }

    private static IReadOnlyList<TarotTagAssignment> ValidateTags(
        List<TarotTagAssignmentDocument?>? rawTags,
        string field,
        int? minimum,
        int? maximum,
        TarotDiagnosticBag diagnostics)
    {
        if (rawTags is null)
        {
            diagnostics.Error("tags.missing", field, "A tags array is required.");
            return [];
        }

        var tags = new List<TarotTagAssignment>();
        var concepts = new HashSet<string>(StringComparer.Ordinal);
        for (var index = 0; index < rawTags.Count; index++)
        {
            if (rawTags[index] is not { } raw)
            {
                diagnostics.Error("tags.null", $"{field}[{index}]", "Tag assignments cannot be null.");
                continue;
            }

            var tag = ValidateTag(raw, $"{field}[{index}]", diagnostics);
            if (tag is null)
            {
                continue;
            }

            if (!concepts.Add(tag.ConceptId.Value))
            {
                diagnostics.Error("tags.duplicate", $"{field}[{index}].conceptId", "A tag pool cannot repeat conceptId.");
                continue;
            }

            tags.Add(tag);
        }

        if ((minimum is { } min && rawTags.Count < min) || (maximum is { } max && rawTags.Count > max))
        {
            diagnostics.Warning("tags.authoring-target", field, $"The authored tag-pool target is {minimum}–{maximum} entries.");
        }

        return Array.AsReadOnly(tags.ToArray());
    }

    private static TarotTagAssignment? ValidateTag(
        TarotTagAssignmentDocument document,
        string field,
        TarotDiagnosticBag diagnostics)
    {
        var conceptId = ParseConceptId(document.ConceptId, $"{field}.conceptId", diagnostics);
        ValidateValence(document.Valence, $"{field}.valence", diagnostics);
        ValidateIntensity(document.Intensity, $"{field}.intensity", diagnostics);
        return conceptId is null || document.Valence is not { } valence || document.Intensity is not { } intensity ||
               valence is < -2 or > 2 || intensity is < 1 or > 3
            ? null
            : new TarotTagAssignment(conceptId, valence, intensity);
    }

    private static IReadOnlyDictionary<string, string> ValidateSections(
        Dictionary<string, string?>? rawSections,
        TarotDiagnosticBag diagnostics)
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        if (rawSections is null)
        {
            diagnostics.Error("sections.missing", "sections", "The sections object is required.");
            return result;
        }

        foreach (var extra in rawSections.Keys.Where(key => !SectionIds.Contains(key, StringComparer.Ordinal)))
        {
            diagnostics.Error("sections.unknown", $"sections.{extra}", "Unknown single-card section.");
        }

        foreach (var section in SectionIds)
        {
            if (!rawSections.TryGetValue(section, out var text) || string.IsNullOrWhiteSpace(text))
            {
                diagnostics.Error("sections.required", $"sections.{section}", "Every single-card section must be non-empty.");
                continue;
            }

            result.Add(section, text);
        }

        return result;
    }

    private static IReadOnlyList<TarotReversalMechanism> ValidateReversalMechanisms(
        List<TarotReversalMechanism?>? rawMechanisms,
        TarotCardOrientation? orientation,
        TarotDiagnosticBag diagnostics)
    {
        if (rawMechanisms is null)
        {
            diagnostics.Error("reversal.missing", "reversalMechanisms", "The reversalMechanisms array is required.");
            return [];
        }

        var mechanisms = new List<TarotReversalMechanism>();
        for (var index = 0; index < rawMechanisms.Count; index++)
        {
            var mechanism = rawMechanisms[index];
            if (mechanism is null || !Enum.IsDefined(mechanism.Value))
            {
                diagnostics.Error("reversal.unknown", $"reversalMechanisms[{index}]", "Unknown reversal mechanism.");
                continue;
            }

            if (mechanisms.Contains(mechanism.Value))
            {
                diagnostics.Error("reversal.duplicate", $"reversalMechanisms[{index}]", "Reversal mechanisms must be distinct.");
                continue;
            }

            mechanisms.Add(mechanism.Value);
        }

        if (orientation == TarotCardOrientation.Upright && mechanisms.Count != 0)
        {
            diagnostics.Error("reversal.upright", "reversalMechanisms", "Upright entries require no reversal mechanisms.");
        }
        else if (orientation == TarotCardOrientation.Reversed && mechanisms.Count is < 1 or > 3)
        {
            diagnostics.Error("reversal.count", "reversalMechanisms", "Reversed entries require one to three mechanisms.");
        }

        return Array.AsReadOnly(mechanisms.ToArray());
    }

    private static IReadOnlyList<TarotGeneratedIndexEntry> ValidateIndexEntries(
        List<TarotGeneratedIndexEntryDocument?>? rawEntries,
        TarotInterpretationCorpus? corpus,
        TarotDiagnosticBag diagnostics)
    {
        if (rawEntries is null)
        {
            diagnostics.Error("index.entries", "entries", "The entries array is required.");
            return [];
        }

        var result = new List<TarotGeneratedIndexEntry>();
        var keys = new HashSet<string>(StringComparer.Ordinal);
        var paths = new HashSet<string>(StringComparer.Ordinal);
        string? previousKey = null;
        for (var index = 0; index < rawEntries.Count; index++)
        {
            var raw = rawEntries[index];
            if (raw is null)
            {
                diagnostics.Error("index.entry-null", $"entries[{index}]", "Index entries cannot be null.");
                continue;
            }

            var key = raw.Key;
            if (string.IsNullOrEmpty(key))
            {
                diagnostics.Error("index.key", $"entries[{index}].key", "An index key is required.");
            }
            else
            {
                if (previousKey is not null && StringComparer.Ordinal.Compare(previousKey, key) >= 0)
                {
                    diagnostics.Error("index.order", $"entries[{index}].key", "Index keys must be strictly sorted ordinally.");
                }

                previousKey = key;
                if (!keys.Add(key))
                {
                    diagnostics.Error("index.key-duplicate", $"entries[{index}].key", "Index keys must be unique.");
                }

                ValidateIndexKey(key, corpus, $"entries[{index}].key", diagnostics);
            }

            var path = ParsePath(raw.Path, $"entries[{index}].path", diagnostics);
            var hash = ParseHash(raw.Sha256, $"entries[{index}].sha256", diagnostics);
            if (path is not null && !paths.Add(path.Value))
            {
                diagnostics.Error("index.path-duplicate", $"entries[{index}].path", "Index paths must be unique.");
            }

            if (key is not null && path is not null && hash is not null)
            {
                result.Add(new TarotGeneratedIndexEntry(key, path, hash));
            }
        }

        return Array.AsReadOnly(result.ToArray());
    }

    private static void ValidateIndexKey(
        string key,
        TarotInterpretationCorpus? corpus,
        string field,
        TarotDiagnosticBag diagnostics)
    {
        bool valid;
        if (corpus == TarotInterpretationCorpus.SingleCard)
        {
            valid = TarotInterpretationKeys.ParseSingleCard(key).IsValid;
        }
        else if (corpus == TarotInterpretationCorpus.OrientedPairs)
        {
            valid = TarotInterpretationKeys.ParseOrientedPair(key).IsValid;
        }
        else if (corpus == TarotInterpretationCorpus.ThreeCards)
        {
            valid = key.StartsWith("position|", StringComparison.Ordinal)
                ? TarotInterpretationKeys.ParseThreeCardPosition(key).IsValid
                : TarotInterpretationKeys.ParseSynthesisResource(key).IsValid;
        }
        else
        {
            return;
        }

        if (!valid)
        {
            diagnostics.Error("index.key-format", field, "Index key does not match its corpus contract.");
        }
    }

    private static void ValidateCorpusCounts(
        TarotGeneratedIndexDocument document,
        TarotInterpretationCorpus? corpus,
        IReadOnlyList<TarotGeneratedIndexEntry> entries,
        TarotDiagnosticBag diagnostics)
    {
        switch (corpus)
        {
            case TarotInterpretationCorpus.SingleCard:
                if (document.ExpectedEntryCount != 156)
                {
                    diagnostics.Error("index.single-count", "expectedEntryCount", "single-card requires 156 entries.");
                }

                if (document.ExpectedIdentityCount is not null || document.ExpectedPositionEntryCount is not null)
                {
                    diagnostics.Error("index.single-extra-count", "expectedEntryCount", "single-card has no corpus-specific count fields.");
                }
                break;
            case TarotInterpretationCorpus.OrientedPairs:
                if (document.ExpectedIdentityCount != 3003)
                {
                    diagnostics.Error("index.pair-identities", "expectedIdentityCount", "oriented-pairs requires 3003 identities.");
                }

                if (document.ExpectedEntryCount != 12012)
                {
                    diagnostics.Error("index.pair-count", "expectedEntryCount", "oriented-pairs requires 12012 entries.");
                }

                if (document.ExpectedPositionEntryCount is not null)
                {
                    diagnostics.Error("index.pair-position-count", "expectedPositionEntryCount", "oriented-pairs has no position count.");
                }

                var identities = entries.Select(entry => entry.Key.Split('|')[0]).Distinct(StringComparer.Ordinal).Count();
                if (entries.Count == 12012 && identities != 3003)
                {
                    diagnostics.Error("index.pair-identity-inventory", "entries", "The oriented-pair entries do not contain 3003 identities.");
                }
                break;
            case TarotInterpretationCorpus.ThreeCards:
                if (document.ExpectedPositionEntryCount != 468)
                {
                    diagnostics.Error("index.position-count", "expectedPositionEntryCount", "three-cards requires 468 position entries.");
                }

                if (document.ExpectedIdentityCount is not null)
                {
                    diagnostics.Error("index.three-identity-count", "expectedIdentityCount", "three-cards has no pair identity count.");
                }

                var positionCount = entries.Count(entry => entry.Key.StartsWith("position|", StringComparison.Ordinal));
                if (entries.Count == document.ExpectedEntryCount && positionCount != 468)
                {
                    diagnostics.Error("index.position-inventory", "entries", "The three-cards index requires exactly 468 position entries.");
                }
                break;
        }
    }

    private static TarotInterpretationPackId? ParsePackId(string? value, string field, TarotDiagnosticBag diagnostics) =>
        ParseStable(value, field, diagnostics, static item => new TarotInterpretationPackId(item));

    private static TarotDeckId? ParseDeckId(string? value, string field, TarotDiagnosticBag diagnostics) =>
        ParseStable(value, field, diagnostics, static item => new TarotDeckId(item));

    private static TarotCardId? ParseKnownCard(
        string? value,
        string field,
        TarotDeckDefinition deck,
        TarotDiagnosticBag diagnostics)
    {
        var id = ParseStable(value, field, diagnostics, static item => new TarotCardId(item));
        if (id is not null && deck.Cards.All(card => card.Id != id))
        {
            diagnostics.Error("card.unknown", field, "The semantic card ID is not present in the supplied deck.");
            return null;
        }

        return id;
    }

    private static TarotTagConceptId? ParseConceptId(string? value, string field, TarotDiagnosticBag diagnostics) =>
        ParseStable(value, field, diagnostics, static item => new TarotTagConceptId(item));

    private static T? ParseStable<T>(
        string? value,
        string field,
        TarotDiagnosticBag diagnostics,
        Func<string, T> factory)
        where T : class
    {
        if (value is null)
        {
            diagnostics.Error("value.missing", field, "A value is required.");
            return null;
        }

        try
        {
            var result = factory(value);
            if (result.ToString() != value)
            {
                diagnostics.Error("value.normalized", field, "Schema values must already be canonical.");
                return null;
            }

            return result;
        }
        catch (ArgumentException exception)
        {
            diagnostics.Error("value.invalid", field, exception.Message);
            return null;
        }
    }

    private static TarotInterpretationLocale? ParseLocale(string? value, string field, TarotDiagnosticBag diagnostics) =>
        ParseStable(value, field, diagnostics, static item => new TarotInterpretationLocale(item));

    private static TarotPackageRelativePath? ParsePath(string? value, string field, TarotDiagnosticBag diagnostics) =>
        ParseStable(value, field, diagnostics, static item => new TarotPackageRelativePath(item));

    private static TarotSha256? ParseHash(string? value, string field, TarotDiagnosticBag diagnostics) =>
        ParseStable(value, field, diagnostics, static item => new TarotSha256(item));

    private static TEnum? ValidateEnum<TEnum>(TEnum? value, string field, TarotDiagnosticBag diagnostics)
        where TEnum : struct, Enum
    {
        if (value is null || !Enum.IsDefined(value.Value))
        {
            diagnostics.Error("enum.invalid", field, $"A defined {typeof(TEnum).Name} value is required.");
            return null;
        }

        return value;
    }

    private static void ValidateSchemaVersion(int? value, string field, TarotDiagnosticBag diagnostics)
    {
        if (value != 1)
        {
            diagnostics.Error("schema.unsupported", field, "Only schemaVersion 1 is supported.");
        }
    }

    private static void ValidatePositive(int? value, string field, TarotDiagnosticBag diagnostics)
    {
        if (value is null or <= 0)
        {
            diagnostics.Error("number.positive", field, "A positive integer is required.");
        }
    }

    private static void ValidateNonNegative(int? value, string field, TarotDiagnosticBag diagnostics)
    {
        if (value is null or < 0)
        {
            diagnostics.Error("number.non-negative", field, "A non-negative integer is required.");
        }
    }

    private static void ValidateText(string? value, string field, TarotDiagnosticBag diagnostics)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            diagnostics.Error("text.empty", field, "A non-empty plain-text value is required.");
        }
    }

    private static void ValidateValence(int? value, string field, TarotDiagnosticBag diagnostics)
    {
        if (value is null or < -2 or > 2)
        {
            diagnostics.Error("metric.valence", field, "Valence must be between -2 and 2.");
        }
    }

    private static void ValidateIntensity(int? value, string field, TarotDiagnosticBag diagnostics)
    {
        if (value is null or < 1 or > 3)
        {
            diagnostics.Error("metric.intensity", field, "Intensity must be between 1 and 3.");
        }
    }

    private static string[] ExpectedIndexPaths(TarotInterpretationMode mode, string locale) => mode switch
    {
        TarotInterpretationMode.SingleCard => [$"indexes/{locale}/single-card.json"],
        TarotInterpretationMode.TwoCards => [$"indexes/{locale}/oriented-pairs.json"],
        TarotInterpretationMode.ThreeCards =>
            [$"indexes/{locale}/oriented-pairs.json", $"indexes/{locale}/three-cards.json"],
        TarotInterpretationMode.CelticCross => [],
        _ => throw new ArgumentOutOfRangeException(nameof(mode))
    };

    private static TarotModuleDependency[] ExpectedDependencies(TarotInterpretationMode mode) => mode switch
    {
        TarotInterpretationMode.SingleCard => [],
        TarotInterpretationMode.TwoCards => [TarotModuleDependency.OrientedPairs],
        TarotInterpretationMode.ThreeCards =>
        [
            TarotModuleDependency.OrientedPairs,
            TarotModuleDependency.ThreeCardPositions,
            TarotModuleDependency.ThreeCardSynthesis
        ],
        TarotInterpretationMode.CelticCross => [],
        _ => throw new ArgumentOutOfRangeException(nameof(mode))
    };
}
