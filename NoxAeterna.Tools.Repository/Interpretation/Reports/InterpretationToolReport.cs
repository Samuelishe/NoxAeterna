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

public sealed class InterpretationToolDiagnostic
{
    public InterpretationToolDiagnostic(
        string code,
        InterpretationToolSeverity severity,
        string target,
        string message,
        IEnumerable<string>? relatedTargets = null)
    {
        Code = code;
        Severity = severity;
        Target = target;
        Message = message;
        RelatedTargets = Array.AsReadOnly((relatedTargets ?? [])
            .Where(item => !string.Equals(item, target, StringComparison.Ordinal))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray());
    }

    public string Code { get; }
    public InterpretationToolSeverity Severity { get; }
    public string Target { get; }
    public string Message { get; }
    public IReadOnlyList<string> RelatedTargets { get; }
}

public sealed record InterpretationTextStatistics(
    int Count,
    int Minimum,
    int FirstQuartile,
    int Median,
    int ThirdQuartile,
    int Maximum);

public sealed class InterpretationToolReport
{
    public InterpretationToolReport(
        IEnumerable<InterpretationToolDiagnostic> diagnostics,
        IReadOnlyDictionary<string, int>? counts = null,
        IReadOnlyDictionary<string, string>? details = null,
        IEnumerable<string>? generatedPaths = null,
        IEnumerable<string>? driftPaths = null,
        IReadOnlyDictionary<string, IReadOnlyList<string>>? inventories = null,
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, int>>? distributions = null,
        IReadOnlyDictionary<string, InterpretationTextStatistics>? statistics = null)
    {
        Diagnostics = Array.AsReadOnly(diagnostics
            .OrderBy(item => item.Target, StringComparer.Ordinal)
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Message, StringComparer.Ordinal)
            .ToArray());
        Counts = new ReadOnlyDictionary<string, int>((counts ?? new Dictionary<string, int>())
            .OrderBy(pair => pair.Key, StringComparer.Ordinal)
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal));
        Details = new ReadOnlyDictionary<string, string>((details ?? new Dictionary<string, string>())
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
        Inventories = new ReadOnlyDictionary<string, IReadOnlyList<string>>((inventories ??
                new Dictionary<string, IReadOnlyList<string>>())
            .OrderBy(pair => pair.Key, StringComparer.Ordinal)
            .ToDictionary(
                pair => pair.Key,
                pair => (IReadOnlyList<string>)Array.AsReadOnly(pair.Value
                    .Distinct(StringComparer.Ordinal)
                    .Order(StringComparer.Ordinal)
                    .ToArray()),
                StringComparer.Ordinal));
        Distributions = new ReadOnlyDictionary<string, IReadOnlyDictionary<string, int>>((distributions ??
                new Dictionary<string, IReadOnlyDictionary<string, int>>())
            .OrderBy(pair => pair.Key, StringComparer.Ordinal)
            .ToDictionary(
                pair => pair.Key,
                pair => (IReadOnlyDictionary<string, int>)new ReadOnlyDictionary<string, int>(pair.Value
                    .OrderBy(item => item.Key, StringComparer.Ordinal)
                    .ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal)),
                StringComparer.Ordinal));
        Statistics = new ReadOnlyDictionary<string, InterpretationTextStatistics>((statistics ??
                new Dictionary<string, InterpretationTextStatistics>())
            .OrderBy(pair => pair.Key, StringComparer.Ordinal)
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal));
    }

    public bool Success => Diagnostics.All(item => item.Severity != InterpretationToolSeverity.Error) &&
                           DriftPaths.Count == 0;
    public int Errors => Diagnostics.Count(item => item.Severity == InterpretationToolSeverity.Error);
    public int Warnings => Diagnostics.Count(item => item.Severity == InterpretationToolSeverity.Warning);
    public IReadOnlyList<InterpretationToolDiagnostic> Diagnostics { get; }
    public IReadOnlyDictionary<string, int> Counts { get; }
    public IReadOnlyDictionary<string, string> Details { get; }
    public IReadOnlyList<string> GeneratedPaths { get; }
    public IReadOnlyList<string> DriftPaths { get; }
    public IReadOnlyDictionary<string, IReadOnlyList<string>> Inventories { get; }
    public IReadOnlyDictionary<string, IReadOnlyDictionary<string, int>> Distributions { get; }
    public IReadOnlyDictionary<string, InterpretationTextStatistics> Statistics { get; }
}

public static class InterpretationToolReportWriter
{
    private const int ConsoleDiagnosticLimit = 20;
    private const int ConsoleInventorySampleLimit = 12;
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
        foreach (var diagnostic in report.Diagnostics.Take(ConsoleDiagnosticLimit))
        {
            builder.AppendLine($"{diagnostic.Severity.ToString().ToLowerInvariant()} {diagnostic.Code} [{diagnostic.Target}]: {diagnostic.Message}");
        }
        if (report.Diagnostics.Count > ConsoleDiagnosticLimit)
        {
            builder.AppendLine($"diagnostics omitted: {report.Diagnostics.Count - ConsoleDiagnosticLimit}");
        }

        foreach (var path in report.GeneratedPaths)
        {
            builder.AppendLine($"generated [{path}]");
        }

        foreach (var detail in report.Details)
        {
            builder.AppendLine($"{detail.Key}: {detail.Value}");
        }

        foreach (var count in report.Counts)
        {
            builder.AppendLine($"{count.Key}: {count.Value}");
        }

        foreach (var inventory in report.Inventories)
        {
            var sample = inventory.Value.Take(ConsoleInventorySampleLimit).ToArray();
            builder.AppendLine($"{inventory.Key}: {inventory.Value.Count}; sample [{string.Join(", ", sample)}]");
            if (inventory.Value.Count > sample.Length)
            {
                builder.AppendLine($"{inventory.Key} omitted: {inventory.Value.Count - sample.Length}");
            }
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
