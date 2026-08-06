using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using NoxAeterna.Domain.Tarot;
using NoxAeterna.Interpretation.Tarot.Contracts;

namespace NoxAeterna.Interpretation.Tarot.Serialization;

/// <summary>Classifies controlled in-memory JSON parse failures.</summary>
public enum TarotJsonParseFailureKind
{
    MalformedJson,
    DuplicateProperty,
    UnsupportedValue
}

/// <summary>Describes a controlled in-memory JSON parse failure.</summary>
public sealed record TarotJsonParseFailure(TarotJsonParseFailureKind Kind, string Path, string Message);

/// <summary>Separates an untrusted raw document from a controlled parse failure.</summary>
public sealed class TarotJsonParseResult<TDocument>
    where TDocument : class
{
    private TarotJsonParseResult(TDocument? document, TarotJsonParseFailure? failure)
    {
        Document = document;
        Failure = failure;
    }

    public bool IsSuccess => Document is not null;
    public TDocument? Document { get; }
    public TarotJsonParseFailure? Failure { get; }

    internal static TarotJsonParseResult<TDocument> Success(TDocument document) => new(document, null);
    internal static TarotJsonParseResult<TDocument> Failed(TarotJsonParseFailure failure) => new(null, failure);
}

/// <summary>Owns exact in-memory JSON parsing and project-owned serialization conventions.</summary>
public static class TarotInterpretationJson
{
    private static readonly JsonDocumentOptions DocumentOptions = new()
    {
        AllowTrailingCommas = false,
        CommentHandling = JsonCommentHandling.Disallow
    };

    private static readonly JsonSerializerOptions SerializerOptions = CreateOptions();

    public static byte[] Serialize<T>(T value)
    {
        ArgumentNullException.ThrowIfNull(value);
        var json = JsonSerializer.SerializeToUtf8Bytes(value, SerializerOptions);
        var result = new byte[json.Length + 1];
        json.CopyTo(result, 0);
        result[^1] = (byte)'\n';
        return result;
    }

    public static string SerializeToString<T>(T value) => Encoding.UTF8.GetString(Serialize(value));

    public static void Serialize<T>(Stream destination, T value)
    {
        ArgumentNullException.ThrowIfNull(destination);
        destination.Write(Serialize(value));
    }

    public static TarotJsonParseResult<TDocument> Parse<TDocument>(string json)
        where TDocument : class
    {
        ArgumentNullException.ThrowIfNull(json);
        return Parse<TDocument>(Encoding.UTF8.GetBytes(json));
    }

    public static TarotJsonParseResult<TDocument> Parse<TDocument>(ReadOnlySpan<byte> utf8Json)
        where TDocument : class
    {
        if (utf8Json.StartsWith(Encoding.UTF8.Preamble))
        {
            return Failed<TDocument>(TarotJsonParseFailureKind.UnsupportedValue, "$", "UTF-8 BOM is not allowed.");
        }

        try
        {
            using var parsed = JsonDocument.Parse(utf8Json.ToArray(), DocumentOptions);
            var duplicatePath = FindDuplicateProperty(parsed.RootElement, "$", out var propertyName);
            if (duplicatePath is not null)
            {
                return Failed<TDocument>(
                    TarotJsonParseFailureKind.DuplicateProperty,
                    duplicatePath,
                    $"Duplicate property '{propertyName}' is not allowed.");
            }

            var document = JsonSerializer.Deserialize<TDocument>(utf8Json, SerializerOptions);
            return document is null
                ? Failed<TDocument>(TarotJsonParseFailureKind.MalformedJson, "$", "JSON did not contain a document.")
                : TarotJsonParseResult<TDocument>.Success(document);
        }
        catch (JsonException exception)
        {
            return Failed<TDocument>(
                exception.Message.Contains("Unknown Tarot", StringComparison.Ordinal)
                    ? TarotJsonParseFailureKind.UnsupportedValue
                    : TarotJsonParseFailureKind.MalformedJson,
                exception.Path ?? "$",
                exception.Message);
        }
    }

    public static TarotJsonParseResult<TDocument> Parse<TDocument>(Stream source)
        where TDocument : class
    {
        ArgumentNullException.ThrowIfNull(source);
        using var buffer = new MemoryStream();
        source.CopyTo(buffer);
        return Parse<TDocument>(buffer.ToArray());
    }

    private static TarotJsonParseResult<TDocument> Failed<TDocument>(
        TarotJsonParseFailureKind kind,
        string path,
        string message)
        where TDocument : class => TarotJsonParseResult<TDocument>.Failed(new(kind, path, message));

    private static string? FindDuplicateProperty(JsonElement element, string path, out string? duplicateName)
    {
        duplicateName = null;
        if (element.ValueKind == JsonValueKind.Object)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (var property in element.EnumerateObject())
            {
                if (!names.Add(property.Name))
                {
                    duplicateName = property.Name;
                    return $"{path}.{property.Name}";
                }

                var nestedPath = FindDuplicateProperty(property.Value, $"{path}.{property.Name}", out duplicateName);
                if (nestedPath is not null)
                {
                    return nestedPath;
                }
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            var index = 0;
            foreach (var item in element.EnumerateArray())
            {
                var nestedPath = FindDuplicateProperty(item, $"{path}[{index}]", out duplicateName);
                if (nestedPath is not null)
                {
                    return nestedPath;
                }

                index++;
            }
        }

        return null;
    }

    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DictionaryKeyPolicy = null,
            AllowTrailingCommas = false,
            ReadCommentHandling = JsonCommentHandling.Disallow,
            UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
            WriteIndented = false
        };

        options.Converters.Add(new ExactEnumJsonConverter<TarotCardOrientation>(TarotSchemaText.CardOrientations));
        options.Converters.Add(new ExactEnumJsonConverter<TarotInterpretationMode>(TarotSchemaText.Modes));
        options.Converters.Add(new ExactEnumJsonConverter<TarotOrientedPairState>(TarotSchemaText.PairStates));
        options.Converters.Add(new ExactEnumJsonConverter<TarotThreeCardPosition>(TarotSchemaText.Positions));
        options.Converters.Add(new ExactEnumJsonConverter<TarotReversalMechanism>(TarotSchemaText.ReversalMechanisms));
        options.Converters.Add(new ExactEnumJsonConverter<TarotSynthesisResourceType>(TarotSchemaText.SynthesisResourceTypes));
        options.Converters.Add(new ExactEnumJsonConverter<TarotModuleDependency>(TarotSchemaText.Dependencies));
        options.Converters.Add(new ExactEnumJsonConverter<TarotInterpretationCorpus>(TarotSchemaText.Corpora));
        options.Converters.Add(new ExactEnumJsonConverter<TarotNoContentReason>(TarotSchemaText.NoContentReasons));
        return options;
    }
}

/// <summary>Maps closed schema values to exact language-neutral strings.</summary>
public static class TarotSchemaText
{
    public static IReadOnlyDictionary<TarotCardOrientation, string> CardOrientations { get; } =
        Map((TarotCardOrientation.Upright, "upright"), (TarotCardOrientation.Reversed, "reversed"));

    public static IReadOnlyDictionary<TarotInterpretationMode, string> Modes { get; } = Map(
        (TarotInterpretationMode.SingleCard, "single-card"),
        (TarotInterpretationMode.TwoCards, "two-cards"),
        (TarotInterpretationMode.ThreeCards, "three-cards"),
        (TarotInterpretationMode.CelticCross, "celtic-cross"));

    public static IReadOnlyDictionary<TarotOrientedPairState, string> PairStates { get; } = Map(
        (TarotOrientedPairState.UprightUpright, "upright-upright"),
        (TarotOrientedPairState.UprightReversed, "upright-reversed"),
        (TarotOrientedPairState.ReversedUpright, "reversed-upright"),
        (TarotOrientedPairState.ReversedReversed, "reversed-reversed"));

    public static IReadOnlyDictionary<TarotThreeCardPosition, string> Positions { get; } = Map(
        (TarotThreeCardPosition.Past, "past"),
        (TarotThreeCardPosition.Present, "present"),
        (TarotThreeCardPosition.Future, "future"));

    public static IReadOnlyDictionary<TarotReversalMechanism, string> ReversalMechanisms { get; } = Map(
        (TarotReversalMechanism.Blocked, "blocked"),
        (TarotReversalMechanism.Delayed, "delayed"),
        (TarotReversalMechanism.Internalized, "internalized"),
        (TarotReversalMechanism.Excessive, "excessive"),
        (TarotReversalMechanism.Distorted, "distorted"),
        (TarotReversalMechanism.Resisted, "resisted"),
        (TarotReversalMechanism.Depleted, "depleted"));

    public static IReadOnlyDictionary<TarotSynthesisResourceType, string> SynthesisResourceTypes { get; } = Map(
        (TarotSynthesisResourceType.ThreeCardPosition, "three-card-position"),
        (TarotSynthesisResourceType.TrajectoryProfile, "trajectory-profile"),
        (TarotSynthesisResourceType.SynthesisFragment, "synthesis-fragment"),
        (TarotSynthesisResourceType.RelationLabel, "relation-label"));

    public static IReadOnlyDictionary<TarotModuleDependency, string> Dependencies { get; } = Map(
        (TarotModuleDependency.OrientedPairs, "oriented-pairs"),
        (TarotModuleDependency.ThreeCardPositions, "three-card-positions"),
        (TarotModuleDependency.ThreeCardSynthesis, "three-card-synthesis"));

    public static IReadOnlyDictionary<TarotInterpretationCorpus, string> Corpora { get; } = Map(
        (TarotInterpretationCorpus.SingleCard, "single-card"),
        (TarotInterpretationCorpus.OrientedPairs, "oriented-pairs"),
        (TarotInterpretationCorpus.ThreeCards, "three-cards"));

    public static IReadOnlyDictionary<TarotNoContentReason, string> NoContentReasons { get; } = Map(
        (TarotNoContentReason.PackUnavailable, "pack-unavailable"),
        (TarotNoContentReason.NoReadyLocale, "no-ready-locale"),
        (TarotNoContentReason.BrokenReadyModule, "broken-ready-module"),
        (TarotNoContentReason.UnsupportedMode, "unsupported-mode"),
        (TarotNoContentReason.ValidationFailed, "validation-failed"));

    public static string Get<TEnum>(TEnum value, IReadOnlyDictionary<TEnum, string> mapping)
        where TEnum : struct, Enum => mapping.TryGetValue(value, out var text)
            ? text
            : throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown schema enum value.");

    public static bool TryParse<TEnum>(string value, IReadOnlyDictionary<TEnum, string> mapping, out TEnum result)
        where TEnum : struct, Enum
    {
        foreach (var pair in mapping)
        {
            if (string.Equals(pair.Value, value, StringComparison.Ordinal))
            {
                result = pair.Key;
                return true;
            }
        }

        result = default;
        return false;
    }

    private static IReadOnlyDictionary<TEnum, string> Map<TEnum>(params (TEnum Value, string Text)[] values)
        where TEnum : struct, Enum => values.ToDictionary(pair => pair.Value, pair => pair.Text);
}

internal sealed class ExactEnumJsonConverter<TEnum>(IReadOnlyDictionary<TEnum, string> mapping) : JsonConverter<TEnum>
    where TEnum : struct, Enum
{
    public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String ||
            reader.GetString() is not { } value ||
            !TarotSchemaText.TryParse(value, mapping, out var result))
        {
            throw new JsonException($"Unknown {typeof(TEnum).Name} value.");
        }

        return result;
    }

    public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options) =>
        writer.WriteStringValue(TarotSchemaText.Get(value, mapping));
}
