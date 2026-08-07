using System.Collections.ObjectModel;
using NoxAeterna.Domain.Tarot;
using NoxAeterna.Interpretation.Tarot.Contracts;

namespace NoxAeterna.Interpretation.Tarot.Storage;

public enum TarotInterpretationStoreStatus { Found, Missing, Failed }

/// <summary>Technical package-store diagnostic that is never rendered as user-facing text.</summary>
public sealed record TarotInterpretationStoreDiagnostic
{
    public TarotInterpretationStoreDiagnostic(string code, string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        Code = code;
        Message = message;
    }
    public string Code { get; }
    public string Message { get; }
}

/// <summary>Controlled exact package-store lookup result.</summary>
public sealed class TarotInterpretationStoreResult<T> where T : class
{
    private TarotInterpretationStoreResult(TarotInterpretationStoreStatus status, T? value, TarotInterpretationStoreDiagnostic? diagnostic) =>
        (Status, Value, Diagnostic) = (status, value, diagnostic);

    public TarotInterpretationStoreStatus Status { get; }
    public T? Value { get; }
    public TarotInterpretationStoreDiagnostic? Diagnostic { get; }
    public static TarotInterpretationStoreResult<T> Found(T value) => new(TarotInterpretationStoreStatus.Found, value ?? throw new ArgumentNullException(nameof(value)), null);
    public static TarotInterpretationStoreResult<T> Missing() => new(TarotInterpretationStoreStatus.Missing, null, null);
    public static TarotInterpretationStoreResult<T> Failed(string code, string message) => new(TarotInterpretationStoreStatus.Failed, null, new(code, message));
}

/// <summary>Trusted same-locale labels and vocabulary labels loaded from one package.</summary>
public sealed class TarotLocalizedInterpretationLabels
{
    public TarotLocalizedInterpretationLabels(TarotLabels labels, IReadOnlyDictionary<TarotTagConceptId, string> tagLabels)
    {
        Labels = labels ?? throw new ArgumentNullException(nameof(labels));
        TagLabels = new ReadOnlyDictionary<TarotTagConceptId, string>(tagLabels.ToDictionary(static pair => pair.Key, static pair => pair.Value));
    }
    public TarotLabels Labels { get; }
    public IReadOnlyDictionary<TarotTagConceptId, string> TagLabels { get; }
}

/// <summary>Immutable validated semantic access to one interpretation package.</summary>
public interface ITarotInterpretationPackStore
{
    TarotInterpretationPackManifest Manifest { get; }
    TarotSha256 SourceDigest { get; }
    TarotInterpretationStoreResult<IReadOnlyList<TarotInterpretationCorpus>> ValidateReadyModule(TarotInterpretationLocale locale, TarotInterpretationMode mode);
    TarotInterpretationStoreResult<TarotLocalizedInterpretationLabels> GetLabels(TarotInterpretationLocale locale);
    TarotInterpretationStoreResult<TarotSingleCardEntry> GetSingleCard(TarotInterpretationLocale locale, TarotCardId cardId, TarotCardOrientation orientation);
    TarotInterpretationStoreResult<TarotOrientedPairEntry> GetOrientedPair(TarotInterpretationLocale locale, TarotCardId cardAId, TarotCardId cardBId, TarotOrientedPairState state);
    TarotInterpretationStoreResult<TarotThreeCardPositionEntry> GetThreeCardPosition(TarotInterpretationLocale locale, TarotThreeCardPosition position, TarotCardId cardId, TarotCardOrientation orientation);
    TarotInterpretationStoreResult<TarotSynthesisResource> GetSynthesisResource(TarotInterpretationLocale locale, TarotSynthesisResourceType resourceType, TarotSynthesisResourceId resourceId);
}

/// <summary>Resolves exact immutable package stores without SQL or filesystem details.</summary>
public interface ITarotInterpretationPackStoreCatalog
{
    bool TryGetStore(TarotInterpretationPackId packId, out ITarotInterpretationPackStore? store);
}
