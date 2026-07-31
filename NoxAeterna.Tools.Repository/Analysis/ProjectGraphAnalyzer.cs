using System.Xml.Linq;

namespace NoxAeterna.Tools.Repository.Analysis;

public sealed record ProjectStatsEntry(
    string Name,
    string Path,
    IReadOnlyList<string> TargetFrameworks,
    string OutputType,
    IReadOnlyList<string> ProjectReferences,
    int PackageReferenceCount,
    int SourceFileCount,
    int SourceLines);

public sealed record ProjectReferenceEdge(string From, string To);

public sealed record ProjectGraphResult(
    IReadOnlyList<ProjectStatsEntry> Projects,
    IReadOnlyList<ProjectReferenceEdge> Edges,
    IReadOnlyList<RepositoryDiagnostic> Diagnostics);

public sealed class ProjectGraphAnalyzer
{
    public ProjectGraphResult Analyze(string root, IReadOnlyList<RepositoryFileEntry> files)
    {
        var diagnostics = new List<RepositoryDiagnostic>();
        var parsed = new List<ParsedProject>();
        foreach (var file in files
                     .Where(static file => file.Extension == ".csproj")
                     .OrderBy(static file => file.Path, StringComparer.Ordinal))
        {
            try
            {
                var absolutePath = Path.Combine(root, file.Path.Replace('/', Path.DirectorySeparatorChar));
                var document = XDocument.Load(absolutePath, LoadOptions.None);
                parsed.Add(ParseProject(root, file.Path, document, files));
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or System.Xml.XmlException)
            {
                diagnostics.Add(new RepositoryDiagnostic(
                    "project-malformed", "warning", file.Path, "Project XML could not be read."));
            }
        }

        var known = parsed.Select(static project => project.Path).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var edges = new List<ProjectReferenceEdge>();
        foreach (var project in parsed)
        {
            foreach (var reference in project.References)
            {
                if (reference.Equals(project.Path, StringComparison.OrdinalIgnoreCase))
                {
                    diagnostics.Add(new RepositoryDiagnostic(
                        "project-self-reference", "warning", project.Path, "Project references itself."));
                    continue;
                }

                if (!known.Contains(reference))
                {
                    diagnostics.Add(new RepositoryDiagnostic(
                        "project-reference-missing", "warning", project.Path,
                        $"Referenced project is missing from public inventory: {reference}"));
                    continue;
                }

                edges.Add(new ProjectReferenceEdge(project.Path, reference));
            }
        }

        DetectCycles(parsed.Select(static project => project.Path), edges, diagnostics);
        return new ProjectGraphResult(
            parsed.Select(static project => project.ToStats()).ToArray(),
            edges.OrderBy(static edge => edge.From, StringComparer.Ordinal)
                .ThenBy(static edge => edge.To, StringComparer.Ordinal)
                .ToArray(),
            diagnostics.OrderBy(static diagnostic => diagnostic.Code, StringComparer.Ordinal)
                .ThenBy(static diagnostic => diagnostic.Path, StringComparer.Ordinal)
                .ToArray());
    }

    private static ParsedProject ParseProject(
        string root,
        string relativePath,
        XDocument document,
        IReadOnlyList<RepositoryFileEntry> files)
    {
        var directory = RepositoryPathPolicy.Normalize(Path.GetDirectoryName(relativePath) ?? string.Empty).TrimEnd('/');
        var frameworks = document.Descendants()
            .Where(static element => element.Name.LocalName is "TargetFramework" or "TargetFrameworks")
            .SelectMany(static element => element.Value.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(static value => value, StringComparer.Ordinal)
            .ToArray();
        var outputType = document.Descendants()
            .FirstOrDefault(static element => element.Name.LocalName == "OutputType")?.Value.Trim();
        var references = document.Descendants()
            .Where(static element => element.Name.LocalName == "ProjectReference")
            .Select(element => NormalizeReference(root, relativePath, (string?)element.Attribute("Include")))
            .Where(static path => path is not null)
            .Cast<string>()
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(static value => value, StringComparer.Ordinal)
            .ToArray();
        var sources = files.Where(file =>
                file.Extension == ".cs" &&
                file.Path.StartsWith(directory + "/", StringComparison.OrdinalIgnoreCase))
            .ToArray();
        return new ParsedProject(
            Path.GetFileNameWithoutExtension(relativePath),
            relativePath,
            frameworks,
            string.IsNullOrWhiteSpace(outputType) ? "Library" : outputType,
            references,
            document.Descendants().Count(static element => element.Name.LocalName == "PackageReference"),
            sources.Length,
            sources.Sum(static source => source.Lines ?? 0));
    }

    private static string? NormalizeReference(string root, string projectPath, string? include)
    {
        if (string.IsNullOrWhiteSpace(include))
        {
            return null;
        }

        var projectDirectory = Path.GetDirectoryName(Path.Combine(root, projectPath.Replace('/', Path.DirectorySeparatorChar)))!;
        var absolute = Path.GetFullPath(Path.Combine(projectDirectory, include));
        var relative = Path.GetRelativePath(root, absolute);
        return RepositoryPathPolicy.Normalize(relative);
    }

    private static void DetectCycles(
        IEnumerable<string> projects,
        IReadOnlyList<ProjectReferenceEdge> edges,
        ICollection<RepositoryDiagnostic> diagnostics)
    {
        var adjacency = edges.GroupBy(static edge => edge.From, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                static group => group.Key,
                static group => group.Select(static edge => edge.To).ToArray(),
                StringComparer.OrdinalIgnoreCase);
        var visiting = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        bool Visit(string project)
        {
            if (!visiting.Add(project))
            {
                return true;
            }

            if (visited.Contains(project))
            {
                visiting.Remove(project);
                return false;
            }

            foreach (var reference in adjacency.GetValueOrDefault(project) ?? [])
            {
                if (Visit(reference))
                {
                    return true;
                }
            }

            visiting.Remove(project);
            visited.Add(project);
            return false;
        }

        foreach (var project in projects.OrderBy(static path => path, StringComparer.Ordinal))
        {
            visiting.Clear();
            if (Visit(project))
            {
                diagnostics.Add(new RepositoryDiagnostic(
                    "project-cycle", "warning", project, "Project reference graph contains a cycle."));
                return;
            }
        }
    }

    private sealed record ParsedProject(
        string Name,
        string Path,
        IReadOnlyList<string> Frameworks,
        string OutputType,
        IReadOnlyList<string> References,
        int PackageCount,
        int SourceCount,
        int SourceLines)
    {
        public ProjectStatsEntry ToStats() => new(
            Name, Path, Frameworks, OutputType, References, PackageCount, SourceCount, SourceLines);
    }
}
