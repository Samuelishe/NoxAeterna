using NoxAeterna.Tools.Repository.Analysis;
using NoxAeterna.Tools.Repository.Context.Cli;
using NoxAeterna.Tools.Repository.Stats;

namespace NoxAeterna.Tools.Repository.Cli;

public static class RepositoryCommandDispatcher
{
    public static int Run(IReadOnlyList<string> args, TextWriter output, TextWriter error)
    {
        if (args.Count == 0 || args[0] is "--help" or "-h")
        {
            output.Write(TopHelp());
            return 0;
        }
        return args[0] switch
        {
            "stats" => RunStats(args, output, error),
            "context-plan" => ContextPlanCli.Run(args, output, error),
            "context-eval" => Context.Evaluation.ContextEvalCli.Run(args, output, error),
            _ => Unknown(args[0], error)
        };
    }

    private static int RunStats(IReadOnlyList<string> args, TextWriter output, TextWriter error)
    {
        var parsed = ProjectStatsCliParser.Parse(args);
        if (!parsed.Succeeded)
        {
            error.WriteLine($"Error: {parsed.Error}");
            error.WriteLine("Use '--help' for usage.");
            return 2;
        }
        var options = parsed.Options!;
        if (options.ShowHelp)
        {
            output.Write(StatsHelp());
            return 0;
        }
        try
        {
            var root = GitRepositoryInventory.ResolveRoot(options.Root);
            var resolvedOutput = ResolveOutput(root, options.OutputPath);
            var report = new ProjectStatsAnalyzer().Analyze(root, options.Top, resolvedOutput.RelativePath);
            var content = options.Format switch
            {
                ProjectStatsOutputFormat.Json => ProjectStatsWriters.WriteJson(report),
                ProjectStatsOutputFormat.Markdown => ProjectStatsWriters.WriteMarkdown(report),
                _ => ProjectStatsWriters.WriteConsole(report, options.Top)
            };
            if (resolvedOutput.AbsolutePath is null) output.Write(content);
            else
            {
                var directory = Path.GetDirectoryName(resolvedOutput.AbsolutePath);
                if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);
                File.WriteAllText(resolvedOutput.AbsolutePath, content);
                output.WriteLine($"Project Stats written to {DisplayOutputPath(root, resolvedOutput.AbsolutePath)}");
            }
            return 0;
        }
        catch (Exception exception) when (exception is ArgumentException or IOException or UnauthorizedAccessException)
        {
            error.WriteLine($"Error: {exception.Message}");
            return 2;
        }
    }

    private static (string? AbsolutePath, string? RelativePath) ResolveOutput(string root, string? requestedOutput)
    {
        if (string.IsNullOrWhiteSpace(requestedOutput)) return (null, null);
        var isAbsolute = Path.IsPathRooted(requestedOutput);
        var absolute = Path.GetFullPath(isAbsolute ? requestedOutput : Path.Combine(root, requestedOutput));
        var relative = Path.GetRelativePath(root, absolute);
        var inside = relative != ".." && !relative.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal) && !Path.IsPathRooted(relative);
        if (!isAbsolute && !inside) throw new ArgumentException("Relative output path must remain inside the repository.");
        return (absolute, inside ? RepositoryPathPolicy.Normalize(relative) : null);
    }

    private static string DisplayOutputPath(string root, string absolutePath)
    {
        var relative = Path.GetRelativePath(root, absolutePath);
        return relative != ".." && !relative.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
            ? RepositoryPathPolicy.Normalize(relative) : absolutePath;
    }

    private static int Unknown(string command, TextWriter error)
    {
        error.WriteLine($"Error: Unknown command '{command}'.");
        return 2;
    }

    private static string TopHelp() => "Nox Aeterna repository tools\n\nCommands:\n  stats\n  context-plan\n  context-eval\n";
    private static string StatsHelp() =>
        "Nox Aeterna factual repository diagnostics\n\nUsage:\n  stats [repository-root] [--top N] [--json | --markdown] [--output PATH]\n  --help\n\nDefaults to the current Git repository and bounded console output.\n--top accepts 1 through 100. Relative output paths must remain inside the repository.\n";
}
