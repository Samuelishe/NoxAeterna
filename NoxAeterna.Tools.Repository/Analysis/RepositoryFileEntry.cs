namespace NoxAeterna.Tools.Repository.Analysis;

public enum RepositoryFileCategory
{
    Production,
    Tests,
    Tooling,
    Documentation,
    Resources,
    Workflow,
    Other
}

public sealed record RepositoryFileEntry(
    string Path,
    string Extension,
    long Bytes,
    int? Lines,
    int? Characters,
    RepositoryFileCategory Category,
    string ProjectArea,
    bool IsTracked,
    bool IsText,
    bool IsRetainedHistory);

public sealed record RepositoryDiagnostic(
    string Code,
    string Severity,
    string? Path,
    string Message);

public sealed record RepositoryInventory(
    IReadOnlyList<RepositoryFileEntry> Files,
    IReadOnlyList<RepositoryDiagnostic> Diagnostics);
