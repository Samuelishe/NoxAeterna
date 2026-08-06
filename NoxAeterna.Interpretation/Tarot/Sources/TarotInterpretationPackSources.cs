using NoxAeterna.Domain.Tarot;
using NoxAeterna.Interpretation.Tarot.Contracts;

namespace NoxAeterna.Interpretation.Tarot.Sources;

/// <summary>Classifies a controlled exact-byte package read.</summary>
public enum TarotInterpretationSourceReadStatus
{
    Found,
    Missing,
    Failed
}

/// <summary>Provides technical source evidence without exposing filesystem ownership to Interpretation.</summary>
public sealed record TarotInterpretationSourceDiagnostic
{
    public TarotInterpretationSourceDiagnostic(string code, string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        Code = code;
        Message = message;
    }

    public string Code { get; }
    public string Message { get; }
}

/// <summary>Returns immutable exact bytes, controlled absence, or a controlled source failure.</summary>
public sealed class TarotInterpretationSourceReadResult
{
    private readonly byte[]? bytes;

    private TarotInterpretationSourceReadResult(
        TarotInterpretationSourceReadStatus status,
        byte[]? bytes,
        TarotInterpretationSourceDiagnostic? diagnostic)
    {
        Status = status;
        this.bytes = bytes;
        Diagnostic = diagnostic;
    }

    public TarotInterpretationSourceReadStatus Status { get; }
    public ReadOnlyMemory<byte> Bytes => bytes ?? ReadOnlyMemory<byte>.Empty;
    public TarotInterpretationSourceDiagnostic? Diagnostic { get; }

    public static TarotInterpretationSourceReadResult Found(ReadOnlySpan<byte> bytes) =>
        new(TarotInterpretationSourceReadStatus.Found, bytes.ToArray(), null);

    public static TarotInterpretationSourceReadResult Missing() =>
        new(TarotInterpretationSourceReadStatus.Missing, null, null);

    public static TarotInterpretationSourceReadResult Failed(string code, string message) =>
        new(
            TarotInterpretationSourceReadStatus.Failed,
            null,
            new TarotInterpretationSourceDiagnostic(code, message));
}

/// <summary>Supplies one immutable snapshot of a Tarot interpretation package.</summary>
public interface ITarotInterpretationPackSource
{
    TarotInterpretationPackId PackId { get; }

    /// <summary>Opaque source-owned snapshot identity used to prevent stale cache reuse.</summary>
    string SnapshotId { get; }

    TarotInterpretationSourceReadResult ReadManifest();

    TarotInterpretationSourceReadResult ReadPackageFile(TarotPackageRelativePath path);
}

/// <summary>Resolves exact pack sources without discovery or fuzzy matching.</summary>
public interface ITarotInterpretationPackSourceCatalog
{
    bool TryGetSource(
        TarotInterpretationPackId packId,
        out ITarotInterpretationPackSource? source);
}
