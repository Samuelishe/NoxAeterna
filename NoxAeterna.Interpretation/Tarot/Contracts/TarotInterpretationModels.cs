using System.Collections.ObjectModel;
using NoxAeterna.Domain.Tarot;

namespace NoxAeterna.Interpretation.Tarot.Contracts;

/// <summary>Validated immutable interpretation-pack manifest.</summary>
public sealed class TarotInterpretationPackManifest
{
    internal TarotInterpretationPackManifest(
        TarotInterpretationPackId packId,
        TarotDeckId semanticDeckId,
        TarotInterpretationLocale sourceLocale,
        int contentVersion,
        IEnumerable<TarotInterpretationLocale> declaredLocales,
        IReadOnlyDictionary<TarotInterpretationLocale, string> displayNames,
        IReadOnlyDictionary<TarotInterpretationMode, IReadOnlyDictionary<TarotInterpretationLocale, TarotInterpretationModule>> modules,
        IEnumerable<TarotInterpretationIndexFile> indexFiles)
    {
        PackId = packId;
        SemanticDeckId = semanticDeckId;
        SourceLocale = sourceLocale;
        ContentVersion = contentVersion;
        DeclaredLocales = Copy(declaredLocales);
        DisplayNames = Copy(displayNames);
        Modules = new ReadOnlyDictionary<TarotInterpretationMode, IReadOnlyDictionary<TarotInterpretationLocale, TarotInterpretationModule>>(
            modules.ToDictionary(pair => pair.Key, pair => (IReadOnlyDictionary<TarotInterpretationLocale, TarotInterpretationModule>)Copy(pair.Value)));
        IndexFiles = Copy(indexFiles);
    }

    public int SchemaVersion => 1;
    public TarotInterpretationPackId PackId { get; }
    public TarotDeckId SemanticDeckId { get; }
    public TarotInterpretationLocale SourceLocale { get; }
    public int ContentVersion { get; }
    public IReadOnlyList<TarotInterpretationLocale> DeclaredLocales { get; }
    public IReadOnlyDictionary<TarotInterpretationLocale, string> DisplayNames { get; }
    public IReadOnlyDictionary<TarotInterpretationMode, IReadOnlyDictionary<TarotInterpretationLocale, TarotInterpretationModule>> Modules { get; }
    public IReadOnlyList<TarotInterpretationIndexFile> IndexFiles { get; }

    private static IReadOnlyList<T> Copy<T>(IEnumerable<T> values) => Array.AsReadOnly(values.ToArray());

    private static IReadOnlyDictionary<TKey, TValue> Copy<TKey, TValue>(IReadOnlyDictionary<TKey, TValue> values)
        where TKey : notnull => new ReadOnlyDictionary<TKey, TValue>(
            values.ToDictionary(pair => pair.Key, pair => pair.Value));
}

/// <summary>Validated immutable locale/mode declaration.</summary>
public sealed record TarotInterpretationModule
{
    internal TarotInterpretationModule(
        bool ready,
        IEnumerable<TarotPackageRelativePath> indexPaths,
        IEnumerable<TarotModuleDependency> dependencies)
    {
        Ready = ready;
        IndexPaths = Array.AsReadOnly(indexPaths.ToArray());
        Dependencies = Array.AsReadOnly(dependencies.ToArray());
    }

    public bool Ready { get; }
    public IReadOnlyList<TarotPackageRelativePath> IndexPaths { get; }
    public IReadOnlyList<TarotModuleDependency> Dependencies { get; }
}

/// <summary>Validated immutable manifest reference to a generated index.</summary>
public sealed record TarotInterpretationIndexFile
{
    internal TarotInterpretationIndexFile(TarotPackageRelativePath path, TarotSha256 sha256)
    {
        Path = path;
        Sha256 = sha256;
    }

    public TarotPackageRelativePath Path { get; }
    public TarotSha256 Sha256 { get; }
}

/// <summary>Validated immutable vocabulary concept.</summary>
public sealed record TarotVocabularyEntry
{
    internal TarotVocabularyEntry(TarotTagConceptId conceptId, string label, string meaning)
    {
        ConceptId = conceptId;
        Label = label;
        Meaning = meaning;
    }

    public int SchemaVersion => 1;
    public TarotTagConceptId ConceptId { get; }
    public string Label { get; }
    public string Meaning { get; }
}

/// <summary>Validated immutable authored tag assignment.</summary>
public sealed record TarotTagAssignment
{
    internal TarotTagAssignment(TarotTagConceptId conceptId, int valence, int intensity)
    {
        ConceptId = conceptId;
        Valence = valence;
        Intensity = intensity;
    }

    public TarotTagConceptId ConceptId { get; }
    public int Valence { get; }
    public int Intensity { get; }
}

/// <summary>Validated immutable single-card entry.</summary>
public sealed class TarotSingleCardEntry
{
    internal TarotSingleCardEntry(
        TarotCardId cardId,
        TarotCardOrientation orientation,
        IReadOnlyDictionary<string, string> sections,
        IEnumerable<TarotTagAssignment> tags,
        int overallValence,
        int overallIntensity,
        IEnumerable<TarotReversalMechanism> reversalMechanisms)
    {
        CardId = cardId;
        Orientation = orientation;
        Sections = new ReadOnlyDictionary<string, string>(
            sections.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal));
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

/// <summary>Validated immutable oriented-pair entry.</summary>
public sealed class TarotOrientedPairEntry
{
    internal TarotOrientedPairEntry(
        TarotCardId cardAId,
        TarotCardId cardBId,
        TarotOrientedPairState orientationState,
        string interaction,
        string direction,
        IEnumerable<TarotTagAssignment> tags,
        int overallValence,
        int overallIntensity)
    {
        CardAId = cardAId;
        CardBId = cardBId;
        OrientationState = orientationState;
        Interaction = interaction;
        Direction = direction;
        Tags = Array.AsReadOnly(tags.ToArray());
        OverallValence = overallValence;
        OverallIntensity = overallIntensity;
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

/// <summary>Validated immutable three-card position entry.</summary>
public sealed class TarotThreeCardPositionEntry
{
    internal TarotThreeCardPositionEntry(
        TarotThreeCardPosition position,
        TarotCardId cardId,
        TarotCardOrientation orientation,
        string text,
        IEnumerable<TarotTagAssignment> tags,
        int overallValence,
        int overallIntensity)
    {
        Position = position;
        CardId = cardId;
        Orientation = orientation;
        Text = text;
        Tags = Array.AsReadOnly(tags.ToArray());
        OverallValence = overallValence;
        OverallIntensity = overallIntensity;
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

/// <summary>Validated immutable generated-index entry.</summary>
public sealed record TarotGeneratedIndexEntry
{
    internal TarotGeneratedIndexEntry(string key, TarotPackageRelativePath path, TarotSha256 sha256)
    {
        Key = key;
        Path = path;
        Sha256 = sha256;
    }

    public string Key { get; }
    public TarotPackageRelativePath Path { get; }
    public TarotSha256 Sha256 { get; }
}

/// <summary>Validated immutable generated index.</summary>
public sealed class TarotGeneratedIndex
{
    internal TarotGeneratedIndex(
        TarotInterpretationPackId packId,
        TarotInterpretationLocale locale,
        TarotInterpretationCorpus corpusId,
        int contentVersion,
        int expectedEntryCount,
        int? expectedIdentityCount,
        int? expectedPositionEntryCount,
        IEnumerable<TarotGeneratedIndexEntry> entries)
    {
        PackId = packId;
        Locale = locale;
        CorpusId = corpusId;
        ContentVersion = contentVersion;
        ExpectedEntryCount = expectedEntryCount;
        ExpectedIdentityCount = expectedIdentityCount;
        ExpectedPositionEntryCount = expectedPositionEntryCount;
        Entries = Array.AsReadOnly(entries.ToArray());
    }

    public int SchemaVersion => 1;
    public TarotInterpretationPackId PackId { get; }
    public TarotInterpretationLocale Locale { get; }
    public TarotInterpretationCorpus CorpusId { get; }
    public int ContentVersion { get; }
    public int ExpectedEntryCount { get; }
    public int? ExpectedIdentityCount { get; }
    public int? ExpectedPositionEntryCount { get; }
    public IReadOnlyList<TarotGeneratedIndexEntry> Entries { get; }
}
