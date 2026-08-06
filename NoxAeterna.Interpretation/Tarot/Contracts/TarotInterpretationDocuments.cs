using System.Text.Json.Serialization;
using NoxAeterna.Domain.Tarot;

namespace NoxAeterna.Interpretation.Tarot.Contracts;

/// <summary>Raw serializer-facing interpretation-pack manifest.</summary>
public sealed class TarotInterpretationPackDocument
{
    [JsonPropertyOrder(0)] public int? SchemaVersion { get; set; }
    [JsonPropertyOrder(1)] public string? PackId { get; set; }
    [JsonPropertyOrder(2)] public string? SemanticDeckId { get; set; }
    [JsonPropertyOrder(3)] public string? SourceLocale { get; set; }
    [JsonPropertyOrder(4)] public int? ContentVersion { get; set; }
    [JsonPropertyOrder(5)] public List<string?>? DeclaredLocales { get; set; }
    [JsonPropertyOrder(6)] public Dictionary<string, string?>? DisplayNames { get; set; }
    [JsonPropertyOrder(7)] public Dictionary<string, Dictionary<string, TarotInterpretationModuleDocument?>?>? Modules { get; set; }
    [JsonPropertyOrder(8)] public List<TarotInterpretationIndexFileDocument?>? IndexFiles { get; set; }
}

/// <summary>Raw serializer-facing locale/mode declaration.</summary>
public sealed class TarotInterpretationModuleDocument
{
    [JsonPropertyOrder(0)] public bool? Ready { get; set; }
    [JsonPropertyOrder(1)] public List<string?>? IndexPaths { get; set; }
    [JsonPropertyOrder(2)] public List<TarotModuleDependency?>? Dependencies { get; set; }
}

/// <summary>Raw serializer-facing generated-index reference.</summary>
public sealed class TarotInterpretationIndexFileDocument
{
    [JsonPropertyOrder(0)] public string? Path { get; set; }
    [JsonPropertyOrder(1)] public string? Sha256 { get; set; }
}

/// <summary>Raw serializer-facing vocabulary concept.</summary>
public sealed class TarotVocabularyDocument
{
    [JsonPropertyOrder(0)] public int? SchemaVersion { get; set; }
    [JsonPropertyOrder(1)] public string? ConceptId { get; set; }
    [JsonPropertyOrder(2)] public string? Label { get; set; }
    [JsonPropertyOrder(3)] public string? Meaning { get; set; }
}

/// <summary>Raw serializer-facing nested tag assignment.</summary>
public sealed class TarotTagAssignmentDocument
{
    [JsonPropertyOrder(0)] public string? ConceptId { get; set; }
    [JsonPropertyOrder(1)] public int? Valence { get; set; }
    [JsonPropertyOrder(2)] public int? Intensity { get; set; }
}

/// <summary>Raw serializer-facing single-card entry.</summary>
public sealed class TarotSingleCardDocument
{
    [JsonPropertyOrder(0)] public int? SchemaVersion { get; set; }
    [JsonPropertyOrder(1)] public string? CardId { get; set; }
    [JsonPropertyOrder(2)] public TarotCardOrientation? Orientation { get; set; }
    [JsonPropertyOrder(3)] public Dictionary<string, string?>? Sections { get; set; }
    [JsonPropertyOrder(4)] public List<TarotTagAssignmentDocument?>? Tags { get; set; }
    [JsonPropertyOrder(5)] public int? OverallValence { get; set; }
    [JsonPropertyOrder(6)] public int? OverallIntensity { get; set; }
    [JsonPropertyOrder(7)] public List<TarotReversalMechanism?>? ReversalMechanisms { get; set; }
}

/// <summary>Raw serializer-facing oriented-pair entry.</summary>
public sealed class TarotOrientedPairDocument
{
    [JsonPropertyOrder(0)] public int? SchemaVersion { get; set; }
    [JsonPropertyOrder(1)] public string? CardAId { get; set; }
    [JsonPropertyOrder(2)] public string? CardBId { get; set; }
    [JsonPropertyOrder(3)] public TarotOrientedPairState? OrientationState { get; set; }
    [JsonPropertyOrder(4)] public string? Interaction { get; set; }
    [JsonPropertyOrder(5)] public string? Direction { get; set; }
    [JsonPropertyOrder(6)] public List<TarotTagAssignmentDocument?>? Tags { get; set; }
    [JsonPropertyOrder(7)] public int? OverallValence { get; set; }
    [JsonPropertyOrder(8)] public int? OverallIntensity { get; set; }
}

/// <summary>Raw serializer-facing three-card position entry.</summary>
public sealed class TarotThreeCardPositionDocument
{
    [JsonPropertyOrder(0)] public int? SchemaVersion { get; set; }
    [JsonPropertyOrder(1)] public TarotThreeCardPosition? Position { get; set; }
    [JsonPropertyOrder(2)] public string? CardId { get; set; }
    [JsonPropertyOrder(3)] public TarotCardOrientation? Orientation { get; set; }
    [JsonPropertyOrder(4)] public string? Text { get; set; }
    [JsonPropertyOrder(5)] public List<TarotTagAssignmentDocument?>? Tags { get; set; }
    [JsonPropertyOrder(6)] public int? OverallValence { get; set; }
    [JsonPropertyOrder(7)] public int? OverallIntensity { get; set; }
}

/// <summary>Raw serializer-facing generated index.</summary>
public sealed class TarotGeneratedIndexDocument
{
    [JsonPropertyOrder(0)] public int? SchemaVersion { get; set; }
    [JsonPropertyOrder(1)] public string? PackId { get; set; }
    [JsonPropertyOrder(2)] public string? Locale { get; set; }
    [JsonPropertyOrder(3)] public TarotInterpretationCorpus? CorpusId { get; set; }
    [JsonPropertyOrder(4)] public int? ContentVersion { get; set; }
    [JsonPropertyOrder(5)] public int? ExpectedEntryCount { get; set; }
    [JsonPropertyOrder(6)] public int? ExpectedIdentityCount { get; set; }
    [JsonPropertyOrder(7)] public int? ExpectedPositionEntryCount { get; set; }
    [JsonPropertyOrder(8)] public List<TarotGeneratedIndexEntryDocument?>? Entries { get; set; }
}

/// <summary>Raw serializer-facing generated-index route.</summary>
public sealed class TarotGeneratedIndexEntryDocument
{
    [JsonPropertyOrder(0)] public string? Key { get; set; }
    [JsonPropertyOrder(1)] public string? Path { get; set; }
    [JsonPropertyOrder(2)] public string? Sha256 { get; set; }
}
