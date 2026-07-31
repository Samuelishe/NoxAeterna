using System.Text.Json;
using System.Text.RegularExpressions;
using NoxAeterna.Tools.Repository.Analysis;

namespace NoxAeterna.Tools.Repository.Stats;

public sealed class ProjectStatsAnalyzer(
    GitRepositoryInventory? inventory = null,
    ProjectGraphAnalyzer? projectAnalyzer = null)
{
    private static readonly Regex FactPattern = new(@"\[Fact(?:Attribute)?(?:\([^\]]*\))?\]", RegexOptions.CultureInvariant);
    private static readonly Regex TheoryPattern = new(@"\[Theory(?:Attribute)?(?:\([^\]]*\))?\]", RegexOptions.CultureInvariant);
    private static readonly Regex TestClassPattern = new(
        @"\b(?:public\s+|internal\s+)?(?:sealed\s+)?class\s+\w+Tests\b",
        RegexOptions.CultureInvariant);

    private readonly GitRepositoryInventory _inventory = inventory ?? new GitRepositoryInventory();
    private readonly ProjectGraphAnalyzer _projectAnalyzer = projectAnalyzer ?? new ProjectGraphAnalyzer();

    public ProjectStatsReport Analyze(string repositoryRoot, int top, string? outputRelativePath = null)
    {
        var inventoryResult = _inventory.Discover(repositoryRoot, outputRelativePath);
        var files = inventoryResult.Files;
        var projectResult = _projectAnalyzer.Analyze(repositoryRoot, files);
        var diagnostics = inventoryResult.Diagnostics.Concat(projectResult.Diagnostics).ToList();
        var budgets = ReadDocumentationBudgets(repositoryRoot, files, diagnostics);
        var productionLines = files.Where(static file =>
                file.Category == RepositoryFileCategory.Production && file.Extension == ".cs")
            .Sum(static file => file.Lines ?? 0);

        return new ProjectStatsReport(
            1,
            BuildSummary(files, projectResult.Projects.Count),
            projectResult.Projects,
            projectResult.Edges,
            BuildLargestFiles(files, top),
            BuildFolderDensity(files),
            BuildTestTopology(repositoryRoot, files, top, productionLines, diagnostics),
            budgets,
            diagnostics.OrderBy(static diagnostic => diagnostic.Code, StringComparer.Ordinal)
                .ThenBy(static diagnostic => diagnostic.Path, StringComparer.Ordinal)
                .ThenBy(static diagnostic => diagnostic.Message, StringComparer.Ordinal)
                .ToArray());
    }

    private static RepositorySummary BuildSummary(IReadOnlyList<RepositoryFileEntry> files, int projectCount) => new(
        files.Count,
        files.Sum(static file => file.Bytes),
        BuildMetricDictionary(files, static file => string.IsNullOrEmpty(file.Extension) ? "[none]" : file.Extension),
        BuildMetricDictionary(files, static file => file.Category.ToString()),
        Measure(files.Where(static file => file.Extension == ".cs")),
        Measure(files.Where(static file => file.Extension is ".axaml" or ".xaml")),
        Measure(files.Where(static file => file.Extension == ".md")),
        Measure(files.Where(static file => file.Extension == ".json")),
        Measure(files.Where(static file => file.Extension is ".ps1" or ".psm1")),
        projectCount);

    private static IReadOnlyDictionary<string, FileMetricSummary> BuildMetricDictionary(
        IEnumerable<RepositoryFileEntry> files,
        Func<RepositoryFileEntry, string> keySelector) =>
        new SortedDictionary<string, FileMetricSummary>(
            files.GroupBy(keySelector, StringComparer.Ordinal)
                .ToDictionary(static group => group.Key, static group => Measure(group), StringComparer.Ordinal),
            StringComparer.Ordinal);

    private static FileMetricSummary Measure(IEnumerable<RepositoryFileEntry> files)
    {
        var materialized = files.ToArray();
        return new FileMetricSummary(
            materialized.Length,
            materialized.Sum(static file => file.Bytes),
            materialized.Sum(static file => file.Lines ?? 0),
            materialized.Sum(static file => file.Characters ?? 0));
    }

    private static LargestFiles BuildLargestFiles(IReadOnlyList<RepositoryFileEntry> files, int top) => new(
        Rank(files.Where(static file => file.Category == RepositoryFileCategory.Production && file.Extension == ".cs"), top),
        Rank(files.Where(static file => file.Category == RepositoryFileCategory.Tests && file.Extension == ".cs"), top),
        Rank(files.Where(static file => file.Category == RepositoryFileCategory.Tooling && file.Extension == ".cs"), top),
        Rank(files.Where(static file => file.Extension is ".axaml" or ".xaml"), top),
        Rank(files.Where(static file => file.Extension == ".md"), top),
        Rank(files.Where(static file => file.Extension is ".ps1" or ".psm1"), top),
        Rank(files.Where(static file => file.Extension is ".json" or ".yml" or ".yaml" or ".props" or ".targets" or ".csproj" or ".sln"), top));

    private static IReadOnlyList<RankedFile> Rank(IEnumerable<RepositoryFileEntry> files, int top) => files
        .Where(static file => file.IsText)
        .OrderByDescending(static file => file.Lines ?? 0)
        .ThenByDescending(static file => file.Characters ?? 0)
        .ThenBy(static file => file.Path, StringComparer.Ordinal)
        .Take(top)
        .Select(static file => new RankedFile(
            file.Path, file.Lines ?? 0, file.Characters ?? 0, file.Category, file.IsRetainedHistory))
        .ToArray();

    private static IReadOnlyList<FolderDensityEntry> BuildFolderDensity(IReadOnlyList<RepositoryFileEntry> files) => files
        .Where(static file => file.Extension is ".cs" or ".axaml" or ".md" or ".ps1" or ".json")
        .GroupBy(static file => RepositoryPathPolicy.GetDensityGroup(file.Path), StringComparer.Ordinal)
        .OrderBy(static group => group.Key, StringComparer.Ordinal)
        .Select(group => new FolderDensityEntry(
            group.Key,
            group.Count(),
            group.Sum(static file => file.Lines ?? 0),
            group.Sum(static file => file.Characters ?? 0),
            new SortedDictionary<string, int>(
                group.GroupBy(static file => file.Category.ToString(), StringComparer.Ordinal)
                    .ToDictionary(static category => category.Key, static category => category.Count(), StringComparer.Ordinal),
                StringComparer.Ordinal)))
        .ToArray();

    private static TestTopology BuildTestTopology(
        string root,
        IReadOnlyList<RepositoryFileEntry> files,
        int top,
        int productionLines,
        ICollection<RepositoryDiagnostic> diagnostics)
    {
        var testSources = files.Where(static file =>
                file.Category == RepositoryFileCategory.Tests && file.Extension == ".cs")
            .OrderBy(static file => file.Path, StringComparer.Ordinal)
            .ToArray();
        var testFiles = testSources.Where(static file => file.Path.EndsWith("Tests.cs", StringComparison.Ordinal)).ToArray();
        var classes = 0;
        var facts = 0;
        var theories = 0;
        foreach (var file in testSources)
        {
            try
            {
                var content = File.ReadAllText(Path.Combine(root, file.Path.Replace('/', Path.DirectorySeparatorChar)));
                classes += TestClassPattern.Matches(content).Count;
                facts += FactPattern.Matches(content).Count;
                theories += TheoryPattern.Matches(content).Count;
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                diagnostics.Add(new RepositoryDiagnostic(
                    "test-topology-unreadable", "warning", file.Path, "Test source could not be read for lexical topology."));
            }
        }

        var areas = testFiles
            .GroupBy(static file => GetTestArea(file.Path), StringComparer.Ordinal)
            .OrderBy(static group => group.Key, StringComparer.Ordinal)
            .Select(group => new TestAreaSummary(
                group.Key, group.Count(), group.Sum(static file => file.Lines ?? 0)))
            .ToArray();
        var testLines = testSources.Sum(static file => file.Lines ?? 0);
        return new TestTopology(
            testFiles.Length,
            classes,
            facts,
            theories,
            areas,
            Rank(testFiles, top),
            testFiles.Where(static file => RepositoryPathPolicy.Normalize(file.Path).Split('/').Length == 2)
                .Select(static file => file.Path)
                .ToArray(),
            productionLines == 0 ? 0d : (double)testLines / productionLines,
            "Fact/Theory values are lexical attribute counts, not guaranteed runtime test-case counts.");
    }

    private static string GetTestArea(string path)
    {
        var segments = RepositoryPathPolicy.Normalize(path).Split('/');
        return segments.Length > 2 ? segments[1] : "[root]";
    }

    private static IReadOnlyList<DocumentationBudgetSnapshot> ReadDocumentationBudgets(
        string root,
        IReadOnlyList<RepositoryFileEntry> files,
        ICollection<RepositoryDiagnostic> diagnostics)
    {
        const string manifestPath = "eng/document-budgets.json";
        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "eng", "document-budgets.json")));
            if (document.RootElement.GetProperty("schemaVersion").GetInt32() != 1)
            {
                throw new InvalidOperationException("Unsupported documentation budget schema.");
            }

            var warningRatio = document.RootElement.GetProperty("warningRatio").GetDouble();
            if (!double.IsFinite(warningRatio) || warningRatio <= 0d || warningRatio >= 1d)
            {
                throw new InvalidOperationException("Documentation warning ratio must be between zero and one.");
            }

            var byPath = files.ToDictionary(static file => file.Path, StringComparer.OrdinalIgnoreCase);
            return document.RootElement.GetProperty("documents").EnumerateArray()
                .Select(entry =>
                {
                    var path = entry.GetProperty("path").GetString() ?? string.Empty;
                    var hard = entry.GetProperty("hardLimit").GetInt32();
                    if (string.IsNullOrWhiteSpace(path) || hard <= 0)
                    {
                        throw new InvalidOperationException("Documentation budget entries require a path and positive hard limit.");
                    }

                    var soft = (int)Math.Floor(hard * warningRatio);
                    var file = byPath.GetValueOrDefault(path);
                    if (file?.Characters is null)
                    {
                        diagnostics.Add(new RepositoryDiagnostic(
                            "documentation-budget-document-missing", "warning", path,
                            "The budgeted public text document was not available in the factual inventory; doc-check remains the validation owner."));
                    }

                    var current = file?.Characters ?? 0;
                    var status = current > hard ? "error" : current >= soft ? "warning" : "ok";
                    return new DocumentationBudgetSnapshot(
                        path, current, soft, hard, status,
                        entry.GetProperty("overflowStrategy").GetString() ?? string.Empty);
                })
                .OrderBy(static entry => entry.Path, StringComparer.Ordinal)
                .ToArray();
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException or InvalidOperationException)
        {
            diagnostics.Add(new RepositoryDiagnostic(
                "documentation-budget-malformed", "warning", manifestPath,
                "Documentation budget manifest could not be summarized; doc-check remains the validation owner."));
            return [];
        }
    }
}
