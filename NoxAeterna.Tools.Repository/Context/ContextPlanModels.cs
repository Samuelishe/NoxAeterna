using NoxAeterna.Tools.Repository.Analysis;

namespace NoxAeterna.Tools.Repository.Context;

public sealed record ContextSelectedFile(
    string Path,
    int Characters,
    bool Mandatory,
    bool MetadataOnly,
    IReadOnlyList<string> Reasons);

public sealed record ContextBudget(
    int RequestedCharacters,
    int MandatoryCharacters,
    int SelectedCharacters,
    int RemainingCharacters);

public sealed record ContextPlan(
    int SchemaVersion,
    string TaskKind,
    IReadOnlyList<string> RequestedPaths,
    IReadOnlyList<string> MatchedRules,
    IReadOnlyList<ContextSelectedFile> SelectedFiles,
    IReadOnlyList<string> OmittedRecommendedFiles,
    IReadOnlyList<string> TestRoutes,
    ContextBudget Budget,
    IReadOnlyList<RepositoryDiagnostic> Diagnostics);

public sealed record ContextPlanResult(ContextPlan Plan, bool Succeeded);
