using NoxAeterna.Tools.Repository.Analysis;

namespace NoxAeterna.Tools.Repository.Stats;

public sealed record FileMetricSummary(int Files, long Bytes, int Lines, int Characters);

public sealed record RepositorySummary(
    int TotalPublicFiles,
    long TotalBytes,
    IReadOnlyDictionary<string, FileMetricSummary> FilesByExtension,
    IReadOnlyDictionary<string, FileMetricSummary> CategoryTotals,
    FileMetricSummary CSharp,
    FileMetricSummary Xaml,
    FileMetricSummary Markdown,
    FileMetricSummary Json,
    FileMetricSummary PowerShell,
    int ProjectCount);

public sealed record RankedFile(
    string Path,
    int Lines,
    int Characters,
    RepositoryFileCategory Category,
    bool RetainedHistory);

public sealed record LargestFiles(
    IReadOnlyList<RankedFile> ProductionCSharp,
    IReadOnlyList<RankedFile> TestCSharp,
    IReadOnlyList<RankedFile> ToolingCSharp,
    IReadOnlyList<RankedFile> Xaml,
    IReadOnlyList<RankedFile> Markdown,
    IReadOnlyList<RankedFile> PowerShell,
    IReadOnlyList<RankedFile> JsonConfiguration);

public sealed record FolderDensityEntry(
    string Group,
    int FileCount,
    int LineCount,
    int CharacterCount,
    IReadOnlyDictionary<string, int> CategoryMix);

public sealed record TestAreaSummary(string Area, int Files, int Lines);

public sealed record TestTopology(
    int TestFileCount,
    int TestClassCount,
    int LexicalFactCount,
    int LexicalTheoryCount,
    IReadOnlyList<TestAreaSummary> Areas,
    IReadOnlyList<RankedFile> LargestTestFiles,
    IReadOnlyList<string> RootTestFiles,
    double TestToProductionCSharpLineRatio,
    string CountSemantics);

public sealed record DocumentationBudgetSnapshot(
    string Path,
    int CurrentCharacters,
    int SoftThreshold,
    int HardThreshold,
    string Status,
    string OverflowStrategy);

public sealed record ProjectStatsReport(
    int SchemaVersion,
    RepositorySummary Repository,
    IReadOnlyList<ProjectStatsEntry> Projects,
    IReadOnlyList<ProjectReferenceEdge> ProjectGraph,
    LargestFiles LargestFiles,
    IReadOnlyList<FolderDensityEntry> FolderDensity,
    TestTopology Tests,
    IReadOnlyList<DocumentationBudgetSnapshot> DocumentationBudgets,
    IReadOnlyList<RepositoryDiagnostic> Diagnostics);
