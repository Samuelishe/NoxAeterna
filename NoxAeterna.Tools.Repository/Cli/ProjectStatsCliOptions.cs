namespace NoxAeterna.Tools.Repository.Cli;

public enum ProjectStatsOutputFormat
{
    Console,
    Json,
    Markdown
}

public sealed record ProjectStatsCliOptions(
    string? Root,
    int Top,
    ProjectStatsOutputFormat Format,
    string? OutputPath,
    bool ShowHelp);

public sealed record ProjectStatsParseResult(ProjectStatsCliOptions? Options, string? Error)
{
    public bool Succeeded => Options is not null && Error is null;
}

public static class ProjectStatsCliParser
{
    public const int DefaultTop = 10;
    public const int MaximumTop = 100;

    public static ProjectStatsParseResult Parse(IReadOnlyList<string> arguments)
    {
        if (arguments.Count == 0)
        {
            return Failure("Missing command. Use 'stats' or '--help'.");
        }

        if (arguments.Count == 1 && arguments[0] is "--help" or "-h")
        {
            return Success(new ProjectStatsCliOptions(null, DefaultTop, ProjectStatsOutputFormat.Console, null, true));
        }

        if (!arguments[0].Equals("stats", StringComparison.OrdinalIgnoreCase))
        {
            return Failure($"Unknown command '{arguments[0]}'. Only 'stats' is supported.");
        }

        string? root = null;
        string? output = null;
        var top = DefaultTop;
        var json = false;
        var markdown = false;
        var help = false;
        for (var index = 1; index < arguments.Count; index++)
        {
            var argument = arguments[index];
            switch (argument)
            {
                case "--help" or "-h":
                    help = true;
                    break;
                case "--json":
                    json = true;
                    break;
                case "--markdown":
                    markdown = true;
                    break;
                case "--top":
                    if (++index >= arguments.Count || !int.TryParse(arguments[index], out top) || top < 1 || top > MaximumTop)
                    {
                        return Failure($"--top must be an integer from 1 through {MaximumTop}.");
                    }
                    break;
                case "--output":
                    if (++index >= arguments.Count || string.IsNullOrWhiteSpace(arguments[index]))
                    {
                        return Failure("--output requires a path.");
                    }
                    output = arguments[index];
                    break;
                default:
                    if (argument.StartsWith("-", StringComparison.Ordinal))
                    {
                        return Failure($"Unknown argument '{argument}'.");
                    }
                    if (root is not null)
                    {
                        return Failure("Only one positional repository root is allowed.");
                    }
                    root = argument;
                    break;
            }
        }

        if (json && markdown)
        {
            return Failure("--json and --markdown cannot be used together.");
        }

        var format = json
            ? ProjectStatsOutputFormat.Json
            : markdown
                ? ProjectStatsOutputFormat.Markdown
                : ProjectStatsOutputFormat.Console;
        return Success(new ProjectStatsCliOptions(root, top, format, output, help));
    }

    private static ProjectStatsParseResult Success(ProjectStatsCliOptions options) => new(options, null);

    private static ProjectStatsParseResult Failure(string error) => new(null, error);
}
