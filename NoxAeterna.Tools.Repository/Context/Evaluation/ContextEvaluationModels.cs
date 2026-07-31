using NoxAeterna.Tools.Repository.Context.Routing;

namespace NoxAeterna.Tools.Repository.Context.Evaluation;

public sealed record ContextEvaluationCase
{
    public string Name { get; init; } = string.Empty;
    public string Task { get; init; } = string.Empty;
    public IReadOnlyList<string> Paths { get; init; } = [];
    public int BudgetChars { get; init; }
    public IReadOnlyList<string> MustInclude { get; init; } = [];
    public IReadOnlyList<string> MustExclude { get; init; } = [];
    public IReadOnlyList<string> ExpectedMatchedRules { get; init; } = [];
    public IReadOnlyList<string> ExpectedTestRoutes { get; init; } = [];
    public IReadOnlyList<string> ForbiddenTestRoutes { get; init; } = [];
    public int MaxSelectedFiles { get; init; }
    public int MaxSelectedChars { get; init; }
}

public sealed record ContextEvaluationRegistry
{
    public int SchemaVersion { get; init; }
    public IReadOnlyList<ContextEvaluationCase> Cases { get; init; } = [];
}

public sealed record ContextEvaluationCaseResult(
    string Name,
    bool Passed,
    IReadOnlyList<string> Differences);

public sealed record ContextEvaluationReport(
    int SchemaVersion,
    string Result,
    IReadOnlyList<ContextEvaluationCaseResult> Cases);

public sealed class ContextEvaluationException(string message) : Exception(message);
