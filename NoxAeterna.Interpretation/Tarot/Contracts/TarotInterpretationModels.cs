using System.Collections.ObjectModel;
using NoxAeterna.Domain.Tarot;

namespace NoxAeterna.Interpretation.Tarot.Contracts;

/// <summary>Validated immutable schema-v2 authoring manifest.</summary>
public sealed class TarotInterpretationPackManifest
{
    public TarotInterpretationPackManifest(
        TarotInterpretationPackId packId,
        TarotDeckId semanticDeckId,
        TarotInterpretationLocale sourceLocale,
        int contentVersion,
        IEnumerable<TarotInterpretationLocale> declaredLocales,
        IReadOnlyDictionary<TarotInterpretationLocale, string> displayNames,
        IReadOnlyDictionary<TarotInterpretationMode, IReadOnlyDictionary<TarotInterpretationLocale, TarotInterpretationModule>> modules)
    {
        PackId = packId;
        SemanticDeckId = semanticDeckId;
        SourceLocale = sourceLocale;
        ContentVersion = contentVersion;
        DeclaredLocales = Array.AsReadOnly(declaredLocales.ToArray());
        DisplayNames = ReadOnly(displayNames);
        Modules = new ReadOnlyDictionary<TarotInterpretationMode, IReadOnlyDictionary<TarotInterpretationLocale, TarotInterpretationModule>>(
            modules.ToDictionary(static pair => pair.Key, static pair => (IReadOnlyDictionary<TarotInterpretationLocale, TarotInterpretationModule>)ReadOnly(pair.Value)));
    }

    public int SchemaVersion => 2;
    public TarotInterpretationPackId PackId { get; }
    public TarotDeckId SemanticDeckId { get; }
    public TarotInterpretationLocale SourceLocale { get; }
    public int ContentVersion { get; }
    public IReadOnlyList<TarotInterpretationLocale> DeclaredLocales { get; }
    public IReadOnlyDictionary<TarotInterpretationLocale, string> DisplayNames { get; }
    public IReadOnlyDictionary<TarotInterpretationMode, IReadOnlyDictionary<TarotInterpretationLocale, TarotInterpretationModule>> Modules { get; }

    private static IReadOnlyDictionary<TKey, TValue> ReadOnly<TKey, TValue>(IReadOnlyDictionary<TKey, TValue> values)
        where TKey : notnull => new ReadOnlyDictionary<TKey, TValue>(values.ToDictionary(static pair => pair.Key, static pair => pair.Value));
}

/// <summary>Validated immutable locale/mode declaration.</summary>
public sealed record TarotInterpretationModule(bool Ready, IReadOnlyList<TarotModuleDependency> Dependencies)
{
    public TarotInterpretationModule(bool ready, IEnumerable<TarotModuleDependency> dependencies)
        : this(ready, Array.AsReadOnly(dependencies.ToArray())) { }
}

/// <summary>Validated immutable pack-local label set.</summary>
public sealed class TarotLabels
{
    public TarotLabels(
        IReadOnlyDictionary<string, string> singleCardSections,
        IReadOnlyDictionary<string, string> threeCardPositions,
        IReadOnlyDictionary<string, string> relations)
    {
        SingleCardSections = Copy(singleCardSections);
        ThreeCardPositions = Copy(threeCardPositions);
        Relations = Copy(relations);
    }

    public int SchemaVersion => 1;
    public IReadOnlyDictionary<string, string> SingleCardSections { get; }
    public IReadOnlyDictionary<string, string> ThreeCardPositions { get; }
    public IReadOnlyDictionary<string, string> Relations { get; }

    private static IReadOnlyDictionary<string, string> Copy(IReadOnlyDictionary<string, string> source) =>
        new ReadOnlyDictionary<string, string>(source.ToDictionary(static pair => pair.Key, static pair => pair.Value, StringComparer.Ordinal));
}

public sealed record TarotVocabularyEntry(TarotTagConceptId ConceptId, string Label, string Meaning)
{
    public int SchemaVersion => 1;
}

public sealed record TarotTagAssignment(TarotTagConceptId ConceptId, int Valence, int Intensity);

public sealed class TarotSingleCardEntry
{
    public TarotSingleCardEntry(
        TarotCardId cardId, TarotCardOrientation orientation, IReadOnlyDictionary<string, string> sections,
        IEnumerable<TarotTagAssignment> tags, int overallValence, int overallIntensity,
        IEnumerable<TarotReversalMechanism> reversalMechanisms)
    {
        CardId = cardId;
        Orientation = orientation;
        Sections = new ReadOnlyDictionary<string, string>(sections.ToDictionary(static pair => pair.Key, static pair => pair.Value, StringComparer.Ordinal));
        Tags = Array.AsReadOnly(tags.ToArray());
        OverallValence = overallValence;
        OverallIntensity = overallIntensity;
        ReversalMechanisms = Array.AsReadOnly(reversalMechanisms.ToArray());
    }
    public int SchemaVersion => 1;
    public TarotCardId CardId { get; }
    public TarotCardOrientation Orientation { get; }
    public IReadOnlyDictionary<string, string> Sections { get; }
    public IReadOnlyList<TarotTagAssignment> Tags { get; }
    public int OverallValence { get; }
    public int OverallIntensity { get; }
    public IReadOnlyList<TarotReversalMechanism> ReversalMechanisms { get; }
}

public sealed class TarotOrientedPairEntry
{
    public TarotOrientedPairEntry(
        TarotCardId cardAId, TarotCardId cardBId, TarotOrientedPairState orientationState,
        string interaction, string direction, IEnumerable<TarotTagAssignment> tags,
        int overallValence, int overallIntensity)
    {
        CardAId = cardAId; CardBId = cardBId; OrientationState = orientationState;
        Interaction = interaction; Direction = direction; Tags = Array.AsReadOnly(tags.ToArray());
        OverallValence = overallValence; OverallIntensity = overallIntensity;
    }
    public int SchemaVersion => 1;
    public TarotCardId CardAId { get; }
    public TarotCardId CardBId { get; }
    public TarotOrientedPairState OrientationState { get; }
    public string Interaction { get; }
    public string Direction { get; }
    public IReadOnlyList<TarotTagAssignment> Tags { get; }
    public int OverallValence { get; }
    public int OverallIntensity { get; }
}

public sealed class TarotThreeCardPositionEntry
{
    public TarotThreeCardPositionEntry(
        TarotThreeCardPosition position, TarotCardId cardId, TarotCardOrientation orientation,
        string text, IEnumerable<TarotTagAssignment> tags, int overallValence, int overallIntensity)
    {
        Position = position; CardId = cardId; Orientation = orientation; Text = text;
        Tags = Array.AsReadOnly(tags.ToArray()); OverallValence = overallValence; OverallIntensity = overallIntensity;
    }
    public int SchemaVersion => 1;
    public TarotThreeCardPosition Position { get; }
    public TarotCardId CardId { get; }
    public TarotCardOrientation Orientation { get; }
    public string Text { get; }
    public IReadOnlyList<TarotTagAssignment> Tags { get; }
    public int OverallValence { get; }
    public int OverallIntensity { get; }
}

public sealed record TarotSynthesisResource(
    TarotSynthesisResourceType ResourceType,
    TarotSynthesisResourceId ResourceId,
    string CanonicalJson);
