using NoxAeterna.Domain.Tarot;
using NoxAeterna.Interpretation.Tarot.Contracts;

namespace NoxAeterna.Interpretation.Tarot.Resolution;

/// <summary>Closed base contract for future interpretation resolution outcomes.</summary>
public abstract class TarotInterpretationResolution<TContent>
    where TContent : class
{
    private protected TarotInterpretationResolution()
    {
    }
}

/// <summary>Contains validated structured content and its resolution provenance.</summary>
public sealed class ResolvedTarotInterpretation<TContent> : TarotInterpretationResolution<TContent>
    where TContent : class
{
    public ResolvedTarotInterpretation(
        TarotInterpretationPackId packId,
        int contentVersion,
        TarotInterpretationMode modeId,
        TarotInterpretationLocale requestedLocale,
        TarotInterpretationLocale resolvedLocale,
        TContent content)
    {
        ArgumentNullException.ThrowIfNull(packId);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(contentVersion);
        ArgumentOutOfRangeException.ThrowIfNotEqual(Enum.IsDefined(modeId), true, nameof(modeId));
        ArgumentNullException.ThrowIfNull(requestedLocale);
        ArgumentNullException.ThrowIfNull(resolvedLocale);
        ArgumentNullException.ThrowIfNull(content);
        PackId = packId;
        ContentVersion = contentVersion;
        ModeId = modeId;
        RequestedLocale = requestedLocale;
        ResolvedLocale = resolvedLocale;
        Content = content;
    }

    public TarotInterpretationPackId PackId { get; }
    public int ContentVersion { get; }
    public TarotInterpretationMode ModeId { get; }
    public TarotInterpretationLocale RequestedLocale { get; }
    public TarotInterpretationLocale ResolvedLocale { get; }
    public TContent Content { get; }
}

/// <summary>Contains optional non-localized technical detail for a no-content result.</summary>
public sealed record TarotResolutionDiagnostic
{
    public TarotResolutionDiagnostic(string code, string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        Code = code;
        Message = message;
    }

    public string Code { get; }
    public string Message { get; }
}

/// <summary>Represents typed absence without fake prose or an empty content model.</summary>
public sealed class NoTarotInterpretationContent<TContent> : TarotInterpretationResolution<TContent>
    where TContent : class
{
    public NoTarotInterpretationContent(
        TarotNoContentReason reason,
        TarotResolutionDiagnostic? diagnostic = null)
    {
        ArgumentOutOfRangeException.ThrowIfNotEqual(Enum.IsDefined(reason), true, nameof(reason));
        Reason = reason;
        Diagnostic = diagnostic;
    }

    public TarotNoContentReason Reason { get; }
    public TarotResolutionDiagnostic? Diagnostic { get; }
}
