using NoxAeterna.Domain.Tarot;
using NoxAeterna.Interpretation.Tarot.Contracts;
using NoxAeterna.Interpretation.Tarot.Serialization;

namespace NoxAeterna.Interpretation.Tarot.Validation;

/// <summary>Validates schema-v2 authoring documents into immutable semantic contracts.</summary>
public static class TarotInterpretationValidator
{
    public static readonly string[] SingleCardSectionIds = ["situation", "development", "risk", "outcome", "advice"];
    public static readonly string[] ThreeCardPositionIds = ["past", "present", "future"];
    public static readonly string[] RelationIds = ["past-present", "present-future", "overall"];
    public static readonly string[] OrientationIds = ["upright", "reversed"];
    public static readonly string[] PairStateIds = ["upright-upright", "upright-reversed", "reversed-upright", "reversed-reversed"];

    public static TarotValidationResult<TarotInterpretationPackManifest> ValidateManifest(TarotInterpretationPackDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        var bag = new TarotDiagnosticBag();
        RequireVersion(document.SchemaVersion, 2, "schemaVersion", bag);
        var packId = Parse(() => new TarotInterpretationPackId(document.PackId!), document.PackId, "packId", bag);
        var deckId = Parse(() => new TarotDeckId(document.SemanticDeckId!), document.SemanticDeckId, "semanticDeckId", bag);
        var sourceLocale = Parse(() => new TarotInterpretationLocale(document.SourceLocale!), document.SourceLocale, "sourceLocale", bag);
        if (document.ContentVersion is null or <= 0) bag.Error("manifest.content-version", "contentVersion", "contentVersion must be positive.");

        var locales = new List<TarotInterpretationLocale>();
        if (document.DeclaredLocales is null || document.DeclaredLocales.Count == 0)
            bag.Error("manifest.locales", "declaredLocales", "At least one declared locale is required.");
        else foreach (var raw in document.DeclaredLocales)
        {
            var locale = Parse(() => new TarotInterpretationLocale(raw!), raw, "declaredLocales", bag);
            if (locale is not null && !locales.Contains(locale)) locales.Add(locale);
            else if (locale is not null) bag.Error("manifest.locale-duplicate", "declaredLocales", "Declared locales must be unique.");
        }
        if (sourceLocale is not null && !locales.Contains(sourceLocale)) bag.Error("manifest.source-locale", "sourceLocale", "sourceLocale must be declared.");

        var names = new Dictionary<TarotInterpretationLocale, string>();
        if (document.DisplayNames is null) bag.Error("manifest.display-names", "displayNames", "displayNames is required.");
        else foreach (var locale in locales)
        {
            if (!document.DisplayNames.TryGetValue(locale.Value, out var name) || string.IsNullOrWhiteSpace(name) || name != name.Trim())
                bag.Error("manifest.display-name", $"displayNames.{locale.Value}", "Every declared locale requires a trimmed display name.");
            else names[locale] = name;
        }
        if (document.DisplayNames is not null && document.DisplayNames.Keys.Any(key => locales.All(locale => locale.Value != key)))
            bag.Error("manifest.display-name-extra", "displayNames", "Display names may reference only declared locales.");

        var modules = new Dictionary<TarotInterpretationMode, IReadOnlyDictionary<TarotInterpretationLocale, TarotInterpretationModule>>();
        if (document.Modules is null) bag.Error("manifest.modules", "modules", "modules is required.");
        foreach (var (mode, modeText) in TarotSchemaText.Modes.OrderBy(static item => item.Value, StringComparer.Ordinal))
        {
            var byLocale = new Dictionary<TarotInterpretationLocale, TarotInterpretationModule>();
            if (document.Modules is null || !document.Modules.TryGetValue(modeText, out var rawLocales) || rawLocales is null)
            {
                bag.Error("manifest.mode", $"modules.{modeText}", "Every frozen mode is required.");
                continue;
            }
            foreach (var locale in locales)
            {
                if (!rawLocales.TryGetValue(locale.Value, out var raw) || raw is null)
                {
                    bag.Error("manifest.module", $"modules.{modeText}.{locale.Value}", "Every mode/locale module is required.");
                    continue;
                }
                if (raw.Ready is null) bag.Error("manifest.ready", $"modules.{modeText}.{locale.Value}.ready", "ready is required.");
                var deps = raw.Dependencies?.Where(static item => item.HasValue).Select(static item => item!.Value).ToArray();
                if (raw.Dependencies is null || deps is null || deps.Length != raw.Dependencies.Count || deps.Distinct().Count() != deps.Length)
                {
                    bag.Error("manifest.dependencies", $"modules.{modeText}.{locale.Value}.dependencies", "Dependencies must be a unique non-null array.");
                    deps = [];
                }
                var expected = ExpectedDependencies(mode);
                if (!deps.SequenceEqual(expected)) bag.Error("manifest.dependency-contract", $"modules.{modeText}.{locale.Value}.dependencies", "Dependencies do not match the frozen mode contract.");
                if (raw.Ready.HasValue) byLocale[locale] = new TarotInterpretationModule(raw.Ready.Value, deps);
            }
            if (rawLocales.Keys.Any(key => locales.All(locale => locale.Value != key)))
                bag.Error("manifest.module-locale-extra", $"modules.{modeText}", "Modules may reference only declared locales.");
            modules[mode] = byLocale;
        }
        if (document.Modules is not null && document.Modules.Keys.Any(key => !TarotSchemaText.Modes.Values.Contains(key, StringComparer.Ordinal)))
            bag.Error("manifest.mode-extra", "modules", "Unknown modes are not allowed.");

        var value = bag.HasErrors || packId is null || deckId is null || sourceLocale is null || document.ContentVersion is null
            ? null : new TarotInterpretationPackManifest(packId, deckId, sourceLocale, document.ContentVersion.Value, locales, names, modules);
        return TarotValidationResult<TarotInterpretationPackManifest>.Create(value, bag.Items);
    }

    public static TarotValidationResult<TarotLabels> ValidateLabels(TarotLabelsDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        var bag = new TarotDiagnosticBag();
        RequireVersion(document.SchemaVersion, 1, "schemaVersion", bag);
        var single = ExactLabels(document.SingleCardSections, SingleCardSectionIds, "singleCardSections", bag);
        var positions = ExactLabels(document.ThreeCardPositions, ThreeCardPositionIds, "threeCardPositions", bag);
        var relations = ExactLabels(document.Relations, RelationIds, "relations", bag);
        return TarotValidationResult<TarotLabels>.Create(bag.HasErrors ? null : new TarotLabels(single, positions, relations), bag.Items);
    }

    public static TarotValidationResult<TarotVocabularyEntry> ValidateVocabulary(TarotVocabularyDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        var bag = new TarotDiagnosticBag();
        RequireVersion(document.SchemaVersion, 1, "schemaVersion", bag);
        var id = Parse(() => new TarotTagConceptId(document.ConceptId!), document.ConceptId, "conceptId", bag);
        var label = Text(document.Label, "label", bag);
        var meaning = Text(document.Meaning, "meaning", bag);
        return TarotValidationResult<TarotVocabularyEntry>.Create(
            bag.HasErrors || id is null || label is null || meaning is null ? null : new TarotVocabularyEntry(id, label, meaning), bag.Items);
    }

    public static TarotValidationResult<IReadOnlyList<TarotSingleCardEntry>> ValidateSingleCardBundle(
        TarotSingleCardBundleDocument document, TarotDeckDefinition deck)
    {
        ArgumentNullException.ThrowIfNull(document); ArgumentNullException.ThrowIfNull(deck);
        var bag = new TarotDiagnosticBag(); RequireVersion(document.SchemaVersion, 1, "schemaVersion", bag);
        var card = Card(document.CardId, "cardId", deck, bag);
        ExactKeys(document.States, OrientationIds, "states", bag);
        var entries = new List<TarotSingleCardEntry>();
        if (card is not null && document.States is not null)
        foreach (var orientationText in OrientationIds)
        {
            if (!document.States.TryGetValue(orientationText, out var raw) || raw is null) continue;
            var orientation = orientationText == "upright" ? TarotCardOrientation.Upright : TarotCardOrientation.Reversed;
            var sections = ExactLabels(raw.Sections, SingleCardSectionIds, $"states.{orientationText}.sections", bag);
            var tags = Tags(raw.Tags, $"states.{orientationText}.tags", bag);
            Metrics(raw.OverallValence, raw.OverallIntensity, $"states.{orientationText}", bag);
            var mechanisms = raw.ReversalMechanisms?.Where(static item => item.HasValue).Select(static item => item!.Value).ToArray();
            if (raw.ReversalMechanisms is null || mechanisms is null || mechanisms.Length != raw.ReversalMechanisms.Count || mechanisms.Distinct().Count() != mechanisms.Length)
                bag.Error("single.reversal-mechanisms", $"states.{orientationText}.reversalMechanisms", "Reversal mechanisms must be a unique non-null array.");
            mechanisms ??= [];
            if (orientation == TarotCardOrientation.Upright && mechanisms.Length != 0)
                bag.Error("single.upright-mechanisms", $"states.{orientationText}.reversalMechanisms", "Upright state requires no reversal mechanisms.");
            if (orientation == TarotCardOrientation.Reversed && mechanisms.Length is < 1 or > 3)
                bag.Error("single.reversed-mechanisms", $"states.{orientationText}.reversalMechanisms", "Reversed state requires one to three mechanisms.");
            if (raw.OverallValence.HasValue && raw.OverallIntensity.HasValue)
                entries.Add(new TarotSingleCardEntry(card, orientation, sections, tags, raw.OverallValence.Value, raw.OverallIntensity.Value, mechanisms));
        }
        return TarotValidationResult<IReadOnlyList<TarotSingleCardEntry>>.Create(bag.HasErrors ? null : entries.AsReadOnly(), bag.Items);
    }

    public static TarotValidationResult<IReadOnlyList<TarotOrientedPairEntry>> ValidateOrientedPairBundle(
        TarotOrientedPairBundleDocument document, TarotDeckDefinition deck)
    {
        ArgumentNullException.ThrowIfNull(document); ArgumentNullException.ThrowIfNull(deck);
        var bag = new TarotDiagnosticBag(); RequireVersion(document.SchemaVersion, 1, "schemaVersion", bag);
        var a = Card(document.CardAId, "cardAId", deck, bag); var b = Card(document.CardBId, "cardBId", deck, bag);
        if (a is not null && b is not null && StringComparer.Ordinal.Compare(a.Value, b.Value) >= 0)
            bag.Error("pair.noncanonical", "cardAId", "cardAId must be ordinal-smaller than cardBId and distinct.");
        ExactKeys(document.States, PairStateIds, "states", bag);
        var entries = new List<TarotOrientedPairEntry>();
        if (a is not null && b is not null && document.States is not null)
        foreach (var stateText in PairStateIds)
        {
            if (!document.States.TryGetValue(stateText, out var raw) || raw is null) continue;
            TarotSchemaText.TryParse(stateText, TarotSchemaText.PairStates, out TarotOrientedPairState state);
            var interaction = Text(raw.Interaction, $"states.{stateText}.interaction", bag);
            var direction = Text(raw.Direction, $"states.{stateText}.direction", bag);
            var tags = Tags(raw.Tags, $"states.{stateText}.tags", bag);
            Metrics(raw.OverallValence, raw.OverallIntensity, $"states.{stateText}", bag);
            if (interaction is not null && direction is not null && raw.OverallValence.HasValue && raw.OverallIntensity.HasValue)
                entries.Add(new TarotOrientedPairEntry(a, b, state, interaction, direction, tags, raw.OverallValence.Value, raw.OverallIntensity.Value));
        }
        return TarotValidationResult<IReadOnlyList<TarotOrientedPairEntry>>.Create(bag.HasErrors ? null : entries.AsReadOnly(), bag.Items);
    }

    public static TarotValidationResult<IReadOnlyList<TarotThreeCardPositionEntry>> ValidateThreeCardPositionsBundle(
        TarotThreeCardPositionsBundleDocument document, TarotDeckDefinition deck)
    {
        ArgumentNullException.ThrowIfNull(document); ArgumentNullException.ThrowIfNull(deck);
        var bag = new TarotDiagnosticBag(); RequireVersion(document.SchemaVersion, 1, "schemaVersion", bag);
        var card = Card(document.CardId, "cardId", deck, bag);
        ExactKeys(document.States, ThreeCardPositionIds, "states", bag);
        var entries = new List<TarotThreeCardPositionEntry>();
        if (card is not null && document.States is not null)
        foreach (var positionText in ThreeCardPositionIds)
        {
            if (!document.States.TryGetValue(positionText, out var byOrientation) || byOrientation is null) continue;
            ExactKeys(byOrientation, OrientationIds, $"states.{positionText}", bag);
            TarotSchemaText.TryParse(positionText, TarotSchemaText.Positions, out TarotThreeCardPosition position);
            foreach (var orientationText in OrientationIds)
            {
                if (!byOrientation.TryGetValue(orientationText, out var raw) || raw is null) continue;
                var orientation = orientationText == "upright" ? TarotCardOrientation.Upright : TarotCardOrientation.Reversed;
                var content = Text(raw.Text, $"states.{positionText}.{orientationText}.text", bag);
                var tags = Tags(raw.Tags, $"states.{positionText}.{orientationText}.tags", bag);
                Metrics(raw.OverallValence, raw.OverallIntensity, $"states.{positionText}.{orientationText}", bag);
                if (content is not null && raw.OverallValence.HasValue && raw.OverallIntensity.HasValue)
                    entries.Add(new TarotThreeCardPositionEntry(position, card, orientation, content, tags, raw.OverallValence.Value, raw.OverallIntensity.Value));
            }
        }
        return TarotValidationResult<IReadOnlyList<TarotThreeCardPositionEntry>>.Create(bag.HasErrors ? null : entries.AsReadOnly(), bag.Items);
    }

    private static TarotModuleDependency[] ExpectedDependencies(TarotInterpretationMode mode) => mode switch
    {
        TarotInterpretationMode.SingleCard or TarotInterpretationMode.CelticCross => [],
        TarotInterpretationMode.TwoCards => [TarotModuleDependency.OrientedPairs],
        TarotInterpretationMode.ThreeCards => [TarotModuleDependency.OrientedPairs, TarotModuleDependency.ThreeCardPositions, TarotModuleDependency.ThreeCardSynthesis],
        _ => []
    };

    private static IReadOnlyDictionary<string, string> ExactLabels(Dictionary<string, string?>? raw, string[] expected, string field, TarotDiagnosticBag bag)
    {
        ExactKeys(raw, expected, field, bag);
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        if (raw is not null) foreach (var id in expected)
            if (raw.TryGetValue(id, out var value) && Text(value, $"{field}.{id}", bag) is { } text) result[id] = text;
        return result;
    }

    private static void ExactKeys<T>(Dictionary<string, T>? raw, string[] expected, string field, TarotDiagnosticBag bag)
    {
        if (raw is null) { bag.Error("bundle.states", field, "The object is required."); return; }
        var actual = raw.Keys.Order(StringComparer.Ordinal).ToArray();
        var wanted = expected.Order(StringComparer.Ordinal).ToArray();
        if (!actual.SequenceEqual(wanted, StringComparer.Ordinal))
            bag.Error("bundle.state-membership", field, $"Expected exactly: {string.Join(", ", wanted)}.");
    }

    private static IReadOnlyList<TarotTagAssignment> Tags(List<TarotTagAssignmentDocument?>? raw, string field, TarotDiagnosticBag bag)
    {
        if (raw is null) { bag.Error("tags.required", field, "tags is required."); return []; }
        var result = new List<TarotTagAssignment>(); var seen = new HashSet<TarotTagConceptId>();
        for (var i = 0; i < raw.Count; i++)
        {
            var item = raw[i]; if (item is null) { bag.Error("tag.null", $"{field}[{i}]", "Tag cannot be null."); continue; }
            var id = Parse(() => new TarotTagConceptId(item.ConceptId!), item.ConceptId, $"{field}[{i}].conceptId", bag);
            Metrics(item.Valence, item.Intensity, $"{field}[{i}]", bag);
            if (id is not null && !seen.Add(id)) bag.Error("tag.duplicate", $"{field}[{i}].conceptId", "Tag concepts must be unique in one state.");
            if (id is not null && item.Valence.HasValue && item.Intensity.HasValue) result.Add(new(id, item.Valence.Value, item.Intensity.Value));
        }
        return result.AsReadOnly();
    }

    private static void Metrics(int? valence, int? intensity, string field, TarotDiagnosticBag bag)
    {
        if (valence is null or < -2 or > 2) bag.Error("metric.valence", $"{field}.overallValence", "Valence must be in -2..2.");
        if (intensity is null or < 1 or > 3) bag.Error("metric.intensity", $"{field}.overallIntensity", "Intensity must be in 1..3.");
    }

    private static TarotCardId? Card(string? raw, string field, TarotDeckDefinition deck, TarotDiagnosticBag bag)
    {
        var card = Parse(() => new TarotCardId(raw!), raw, field, bag);
        if (card is not null && deck.Cards.All(item => item.Id != card)) { bag.Error("card.unknown", field, "Card ID is not in the semantic deck."); return null; }
        return card;
    }

    private static string? Text(string? value, string field, TarotDiagnosticBag bag)
    {
        if (string.IsNullOrWhiteSpace(value) || value != value.Trim()) { bag.Error("text.required", field, "A non-empty trimmed text value is required."); return null; }
        return value;
    }

    private static T? Parse<T>(Func<T> factory, string? raw, string field, TarotDiagnosticBag bag) where T : class
    {
        if (raw is null) { bag.Error("value.required", field, "A value is required."); return null; }
        try { return factory(); } catch (ArgumentException exception) { bag.Error("value.invalid", field, exception.Message); return null; }
    }

    private static void RequireVersion(int? actual, int expected, string field, TarotDiagnosticBag bag)
    {
        if (actual != expected) bag.Error("schema.version", field, $"schemaVersion must be {expected}.");
    }
}
