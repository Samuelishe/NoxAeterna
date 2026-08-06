using System.Collections.ObjectModel;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NoxAeterna.Tools.Repository.Interpretation.Reports;

public enum InterpretationToolSeverity
{
    Error,
    Warning
}

public sealed record InterpretationToolDiagnostic(
    string Code,
    InterpretationToolSeverity Severity,
    string Target,
    string Message);

public sealed class InterpretationToolReport
{
    public InterpretationToolReport(
        IEnumerable<InterpretationToolDiagnostic> diagnostics,
        IReadOnlyDictionary<string, int>? counts = null,
        IEnumerable<string>? generatedPaths = null,
        IEnumerable<string>? driftPaths = null)
    {
        Diagnostics = Array.AsReadOnly(diagnostics
            .OrderBy(item => item.Target, StringComparer.Ordinal)
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Message, StringComparer.Ordinal)
            .ToArray());
        Counts = new ReadOnlyDictionary<string, int>((counts ?? new Dictionary<string, int>())
            .OrderBy(pair => pair.Key, StringComparer.Ordinal)
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal));
        GeneratedPaths = Array.AsReadOnly((generatedPaths ?? [])
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray());
        DriftPaths = Array.AsReadOnly((driftPaths ?? [])
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray());
    }

    public bool Success => Diagnostics.All(item => item.Severity != InterpretationToolSeverity.Error) &&
                           DriftPaths.Count == 0;
    public int Errors => Diagnostics.Count(item => item.Severity == InterpretationToolSeverity.Error);
    public int Warnings => Diagnostics.Count(item => item.Severity == InterpretationToolSeverity.Warning);
    public IReadOnlyList<InterpretationToolDiagnostic> Diagnostics { get; }
    public IReadOnlyDictionary<string, int> Counts { get; }
    public IReadOnlyList<string> GeneratedPaths { get; }
    public IReadOnlyList<string> DriftPaths { get; }
}

public static class InterpretationToolReportWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.KebabCaseLower) }
    };

    public static string WriteJson(InterpretationToolReport report) =>
        JsonSerializer.Serialize(report, JsonOptions) + "\n";

    public static string WriteConsole(InterpretationToolReport report)
    {
        var builder = new StringBuilder();
        foreach (var diagnostic in report.Diagnostics)
        {
            builder.AppendLine($"{diagnostic.Severity.ToString().ToLowerInvariant()} {diagnostic.Code} [{diagnostic.Target}]: {diagnostic.Message}");
        }

        foreach (var path in report.GeneratedPaths)
        {
            builder.AppendLine($"generated [{path}]");
        }

        foreach (var path in report.DriftPaths)
        {
            builder.AppendLine($"drift [{path}]");
        }

        builder.AppendLine($"Summary: {(report.Success ? "success" : "failure")}; errors {report.Errors}; warnings {report.Warnings}; drift {report.DriftPaths.Count}.");
        return builder.ToString();
    }
}

internal sealed class InterpretationDiagnosticBag
{
    private readonly List<InterpretationToolDiagnostic> diagnostics = [];

    public IReadOnlyList<InterpretationToolDiagnostic> Items => diagnostics;
    public bool HasErrors => diagnostics.Any(item => item.Severity == InterpretationToolSeverity.Error);

    public void Error(string code, string target, string message) =>
        diagnostics.Add(new(code, InterpretationToolSeverity.Error, target, message));

    public void Warning(string code, string target, string message) =>
        diagnostics.Add(new(code, InterpretationToolSeverity.Warning, target, message));

    public void AddValidation(string target, IEnumerable<NoxAeterna.Interpretation.Tarot.Validation.TarotValidationDiagnostic> values)
    {
        foreach (var value in values)
        {
            diagnostics.Add(new(
                $"schema.{value.Code}",
                value.Severity == NoxAeterna.Interpretation.Tarot.Validation.TarotValidationSeverity.Error
                    ? InterpretationToolSeverity.Error
                    : InterpretationToolSeverity.Warning,
                string.IsNullOrEmpty(value.Field) ? target : $"{target}:{value.Field}",
                value.Message));
        }
    }
}
