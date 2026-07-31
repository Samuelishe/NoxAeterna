using NoxAeterna.Tools.Repository.Analysis;
using NoxAeterna.Tools.Repository.Cli;
using NoxAeterna.Tools.Repository.Stats;

return Run(args);

static int Run(string[] args)
{
    var parsed = ProjectStatsCliParser.Parse(args);
    if (!parsed.Succeeded)
    {
        Console.Error.WriteLine($"Error: {parsed.Error}");
        Console.Error.WriteLine("Use '--help' for usage.");
        return 2;
    }

    var options = parsed.Options!;
    if (options.ShowHelp)
    {
        Console.Write(ProjectStatsHelp());
        return 0;
    }

    try
    {
        var root = GitRepositoryInventory.ResolveRoot(options.Root);
        var output = ResolveOutput(root, options.OutputPath);
        var report = new ProjectStatsAnalyzer().Analyze(root, options.Top, output.RelativePath);
        var content = options.Format switch
        {
            ProjectStatsOutputFormat.Json => ProjectStatsWriters.WriteJson(report),
            ProjectStatsOutputFormat.Markdown => ProjectStatsWriters.WriteMarkdown(report),
            _ => ProjectStatsWriters.WriteConsole(report, options.Top)
        };

        if (output.AbsolutePath is null)
        {
            Console.Write(content);
        }
        else
        {
            var directory = Path.GetDirectoryName(output.AbsolutePath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }
            File.WriteAllText(output.AbsolutePath, content);
            Console.WriteLine($"Project Stats written to {DisplayOutputPath(root, output.AbsolutePath)}");
        }
        return 0;
    }
    catch (Exception exception) when (exception is ArgumentException or IOException or UnauthorizedAccessException)
    {
        Console.Error.WriteLine($"Error: {exception.Message}");
        return 2;
    }
}

static (string? AbsolutePath, string? RelativePath) ResolveOutput(string root, string? requestedOutput)
{
    if (string.IsNullOrWhiteSpace(requestedOutput))
    {
        return (null, null);
    }

    var isAbsolute = Path.IsPathRooted(requestedOutput);
    var absolute = Path.GetFullPath(isAbsolute ? requestedOutput : Path.Combine(root, requestedOutput));
    var relative = Path.GetRelativePath(root, absolute);
    var inside = relative != ".." &&
                 !relative.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal) &&
                 !Path.IsPathRooted(relative);
    if (!isAbsolute && !inside)
    {
        throw new ArgumentException("Relative output path must remain inside the repository.");
    }

    return (absolute, inside ? RepositoryPathPolicy.Normalize(relative) : null);
}

static string DisplayOutputPath(string root, string absolutePath)
{
    var relative = Path.GetRelativePath(root, absolutePath);
    return relative != ".." && !relative.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
        ? RepositoryPathPolicy.Normalize(relative)
        : absolutePath;
}

static string ProjectStatsHelp() =>
    """
    Nox Aeterna factual repository diagnostics

    Usage:
      stats [repository-root] [--top N] [--json | --markdown] [--output PATH]
      --help

    Defaults to the current Git repository and bounded console output.
    --top accepts 1 through 100. Relative output paths must remain inside the repository.
    """ + Environment.NewLine;
