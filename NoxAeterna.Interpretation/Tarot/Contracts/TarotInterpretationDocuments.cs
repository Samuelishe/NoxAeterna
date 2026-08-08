using System.Text.Json;
using System.Text.Json.Serialization;

namespace NoxAeterna.Interpretation.Tarot.Contracts;

/// <summary>Raw serializer-facing schema-v2 authoring manifest.</summary>
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
}

/// <summary>Raw serializer-facing locale/mode declaration.</summary>
public sealed class TarotInterpretationModuleDocument
{
    [JsonPropertyOrder(0)] public bool? Ready { get; set; }
    [JsonPropertyOrder(1)] public List<TarotModuleDependency?>? Dependencies { get; set; }
}

/// <summary>Raw serializer-facing pack-local visible labels.</summary>
public sealed class TarotLabelsDocument
{
    [JsonPropertyOrder(0)] public int? SchemaVersion { get; set; }
    [JsonPropertyOrder(1)] public Dictionary<string, string?>? SingleCardSections { get; set; }
    [JsonPropertyOrder(2)] public Dictionary<string, string?>? ThreeCardPositions { get; set; }
    [JsonPropertyOrder(3)] public Dictionary<string, string?>? Relations { get; set; }
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

/// <summary>Raw serializer-facing state shared by single-card bundles.</summary>
public sealed class TarotSingleCardStateDocument
{
    [JsonPropertyOrder(0)] public Dictionary<string, string?>? Sections { get; set; }
    [JsonPropertyOrder(1)] public List<TarotTagAssignmentDocument?>? Tags { get; set; }
    [JsonPropertyOrder(2)] public int? OverallValence { get; set; }
    [JsonPropertyOrder(3)] public int? OverallIntensity { get; set; }
    [JsonPropertyOrder(4)] public List<TarotReversalMechanism?>? ReversalMechanisms { get; set; }
}

/// <summary>One card and its exact upright/reversed authoring states.</summary>
public sealed class TarotSingleCardBundleDocument
{
    [JsonPropertyOrder(0)] public int? SchemaVersion { get; set; }
    [JsonPropertyOrder(1)] public string? CardId { get; set; }
    [JsonPropertyOrder(2)] public Dictionary<string, TarotSingleCardStateDocument?>? States { get; set; }
}

/// <summary>Raw serializer-facing state shared by oriented-pair bundles.</summary>
public sealed class TarotOrientedPairStateDocument
{
    [JsonPropertyOrder(0)] public string? Interaction { get; set; }
    [JsonPropertyOrder(1)] public string? Direction { get; set; }
    [JsonPropertyOrder(2)] public List<TarotTagAssignmentDocument?>? Tags { get; set; }
    [JsonPropertyOrder(3)] public int? OverallValence { get; set; }
    [JsonPropertyOrder(4)] public int? OverallIntensity { get; set; }
}

/// <summary>One canonical pair and all four orientation states.</summary>
public sealed class TarotOrientedPairBundleDocument
{
    [JsonPropertyOrder(0)] public int? SchemaVersion { get; set; }
    [JsonPropertyOrder(1)] public string? CardAId { get; set; }
    [JsonPropertyOrder(2)] public string? CardBId { get; set; }
    [JsonPropertyOrder(3)] public Dictionary<string, TarotOrientedPairStateDocument?>? States { get; set; }
}

/// <summary>Raw serializer-facing position/orientation state.</summary>
public sealed class TarotThreeCardPositionStateDocument
{
    [JsonPropertyOrder(0)] public string? Text { get; set; }
    [JsonPropertyOrder(1)] public List<TarotTagAssignmentDocument?>? Tags { get; set; }
    [JsonPropertyOrder(2)] public int? OverallValence { get; set; }
    [JsonPropertyOrder(3)] public int? OverallIntensity { get; set; }
}

/// <summary>One card and its exact three-position/two-orientation authoring states.</summary>
public sealed class TarotThreeCardPositionsBundleDocument
{
    [JsonPropertyOrder(0)] public int? SchemaVersion { get; set; }
    [JsonPropertyOrder(1)] public string? CardId { get; set; }
    [JsonPropertyOrder(2)] public Dictionary<string, Dictionary<string, TarotThreeCardPositionStateDocument?>?>? States { get; set; }
}

/// <summary>Typed synthesis resource envelope retained as validated JSON data.</summary>
public sealed class TarotSynthesisResourceDocument
{
    [JsonPropertyOrder(0)] public int? SchemaVersion { get; set; }
    [JsonPropertyOrder(1)] public TarotSynthesisResourceType? ResourceType { get; set; }
    [JsonPropertyOrder(2)] public string? ResourceId { get; set; }
    [JsonPropertyOrder(3)] public JsonElement? Data { get; set; }
}

/// <summary>Exact localized payload shared by production synthesis resource types.</summary>
public sealed class TarotSynthesisTextDocument
{
    [JsonPropertyOrder(0)] public string? Text { get; set; }
}
