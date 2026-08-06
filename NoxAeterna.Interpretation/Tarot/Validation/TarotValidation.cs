namespace NoxAeterna.Interpretation.Tarot.Validation;

/// <summary>Classifies a pure validation diagnostic.</summary>
public enum TarotValidationSeverity
{
    Error,
    Warning
}

/// <summary>Provides stable technical evidence for one validation finding.</summary>
public sealed record TarotValidationDiagnostic
{
    public TarotValidationDiagnostic(string code, string field, string message, TarotValidationSeverity severity)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(field);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        ArgumentOutOfRangeException.ThrowIfNotEqual(Enum.IsDefined(severity), true, nameof(severity));
        Code = code;
        Field = field;
        Message = message;
        Severity = severity;
    }

    public string Code { get; }
    public string Field { get; }
    public string Message { get; }
    public TarotValidationSeverity Severity { get; }
}

/// <summary>Returns either one trusted immutable value or ordered diagnostics explaining failure.</summary>
public sealed class TarotValidationResult<TValue>
    where TValue : class
{
    private TarotValidationResult(TValue? value, IEnumerable<TarotValidationDiagnostic> diagnostics)
    {
        Value = value;
        Diagnostics = Array.AsReadOnly(diagnostics.ToArray());
        IsValid = value is not null && Diagnostics.All(item => item.Severity != TarotValidationSeverity.Error);
    }

    public bool IsValid { get; }
    public TValue? Value { get; }
    public IReadOnlyList<TarotValidationDiagnostic> Diagnostics { get; }

    internal static TarotValidationResult<TValue> Create(
        TValue? value,
        IEnumerable<TarotValidationDiagnostic> diagnostics)
    {
        ArgumentNullException.ThrowIfNull(diagnostics);
        var copied = diagnostics.ToArray();
        var hasErrors = copied.Any(item => item.Severity == TarotValidationSeverity.Error);
        return new TarotValidationResult<TValue>(hasErrors ? null : value, copied);
    }
}

internal sealed class TarotDiagnosticBag
{
    private readonly List<TarotValidationDiagnostic> diagnostics = [];

    public IReadOnlyList<TarotValidationDiagnostic> Items => diagnostics;
    public bool HasErrors => diagnostics.Any(item => item.Severity == TarotValidationSeverity.Error);

    public void Error(string code, string field, string message) =>
        diagnostics.Add(new TarotValidationDiagnostic(code, field, message, TarotValidationSeverity.Error));

    public void Warning(string code, string field, string message) =>
        diagnostics.Add(new TarotValidationDiagnostic(code, field, message, TarotValidationSeverity.Warning));
}
