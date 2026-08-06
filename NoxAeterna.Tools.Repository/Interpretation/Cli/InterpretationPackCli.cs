using NoxAeterna.Tools.Repository.Interpretation.Analysis;
using NoxAeterna.Tools.Repository.Interpretation.Authoring;
using NoxAeterna.Tools.Repository.Interpretation.Indexing;
using NoxAeterna.Tools.Repository.Interpretation.Reports;

namespace NoxAeterna.Tools.Repository.Interpretation.Cli;

public enum InterpretationPackCommand
{
    Validate,
    GenerateIndexes,
    AuthoringStatus
}

public sealed record InterpretationPackCliOptions(
    InterpretationPackCommand Command,
    string? PackRoot,
    string? WorkingRoot,
    bool Check,
    bool Json,
    bool ShowHelp);

public sealed record InterpretationPackCliParseResult(InterpretationPackCliOptions? Options, string? Error)
{
    public bool Succeeded => Options is not null && Error is null;
}

public static class InterpretationPackCliParser
{
    public static InterpretationPackCliParseResult Parse(IReadOnlyList<string> arguments)
    {
        if (arguments.Count < 2 || arguments[0] != "interpretation-pack")
        {
            return Failure("Usage requires 'interpretation-pack <subcommand>'.");
        }

        if (arguments[1] is "--help" or "-h")
        {
            return Success(new(InterpretationPackCommand.Validate, null, null, false, false, true));
        }

        var command = arguments[1] switch
        {
            "validate" => InterpretationPackCommand.Validate,
            "generate-indexes" => InterpretationPackCommand.GenerateIndexes,
            "authoring-status" => InterpretationPackCommand.AuthoringStatus,
            _ => (InterpretationPackCommand?)null
        };
        if (command is null)
        {
            return Failure($"Unknown interpretation-pack subcommand '{arguments[1]}'.");
        }

        string? packRoot = null;
        string? workingRoot = null;
        var check = false;
        var json = false;
        var help = false;
        for (var index = 2; index < arguments.Count; index++)
        {
            switch (arguments[index])
            {
                case "--help" or "-h":
                    help = true;
                    break;
                case "--json":
                    json = true;
                    break;
                case "--check" when command == InterpretationPackCommand.GenerateIndexes:
                    check = true;
                    break;
                case "--pack-root":
                    if (++index >= arguments.Count || string.IsNullOrWhiteSpace(arguments[index]) || packRoot is not null)
                    {
                        return Failure("--pack-root requires exactly one path.");
                    }
                    packRoot = arguments[index];
                    break;
                case "--working-root" when command == InterpretationPackCommand.AuthoringStatus:
                    if (++index >= arguments.Count || string.IsNullOrWhiteSpace(arguments[index]) || workingRoot is not null)
                    {
                        return Failure("--working-root requires exactly one path.");
                    }
                    workingRoot = arguments[index];
                    break;
                default:
                    return Failure($"Unknown option '{arguments[index]}'.");
            }
        }

        if (!help)
        {
            if ((command is InterpretationPackCommand.Validate or InterpretationPackCommand.GenerateIndexes) && packRoot is null)
            {
                return Failure("--pack-root is required.");
            }

            if (command == InterpretationPackCommand.AuthoringStatus && workingRoot is null)
            {
                return Failure("--working-root is required.");
            }
        }

        return Success(new(command.Value, packRoot, workingRoot, check, json, help));
    }

    private static InterpretationPackCliParseResult Success(InterpretationPackCliOptions options) => new(options, null);
    private static InterpretationPackCliParseResult Failure(string error) => new(null, error);
}

public static class InterpretationPackCli
{
    public static int Run(IReadOnlyList<string> arguments, TextWriter output, TextWriter error)
    {
        var parsed = InterpretationPackCliParser.Parse(arguments);
        if (!parsed.Succeeded)
        {
            error.WriteLine($"Error: {parsed.Error}");
            error.WriteLine("Use 'interpretation-pack --help' for usage.");
            return 2;
        }

        var options = parsed.Options!;
        if (options.ShowHelp)
        {
            output.Write(Help(options.Command, arguments.Count > 1 && arguments[1] is not "--help" and not "-h"));
            return 0;
        }

        try
        {
            var report = options.Command switch
            {
                InterpretationPackCommand.Validate => new InterpretationPackValidator().Validate(options.PackRoot!),
                InterpretationPackCommand.GenerateIndexes => new InterpretationIndexGenerator().Generate(options.PackRoot!, options.Check),
                InterpretationPackCommand.AuthoringStatus => new AuthoringInventoryAnalyzer().Analyze(options.WorkingRoot!, options.PackRoot),
                _ => throw new ArgumentOutOfRangeException()
            };
            output.Write(options.Json
                ? InterpretationToolReportWriter.WriteJson(report)
                : InterpretationToolReportWriter.WriteConsole(report));
            return report.Success ? 0 : 1;
        }
        catch (Exception exception) when (exception is ArgumentException or IOException or UnauthorizedAccessException)
        {
            error.WriteLine($"Error: {exception.Message}");
            return 2;
        }
    }

    public static string TopHelp() =>
        "Tarot interpretation pack repository tooling\n\n" +
        "Usage:\n" +
        "  interpretation-pack validate --pack-root PATH [--json]\n" +
        "  interpretation-pack generate-indexes --pack-root PATH [--check] [--json]\n" +
        "  interpretation-pack authoring-status --working-root PATH [--pack-root PATH] [--json]\n";

    private static string Help(InterpretationPackCommand command, bool focused) => focused
        ? command switch
        {
            InterpretationPackCommand.Validate => "Validate a Tarot interpretation pack without writing files.\nUsage: interpretation-pack validate --pack-root PATH [--json]\n",
            InterpretationPackCommand.GenerateIndexes => "Generate deterministic indexes or check drift without writes.\nUsage: interpretation-pack generate-indexes --pack-root PATH [--check] [--json]\n",
            InterpretationPackCommand.AuthoringStatus => "Validate and report authoring inventory progress without promotion.\nUsage: interpretation-pack authoring-status --working-root PATH [--pack-root PATH] [--json]\n",
            _ => throw new ArgumentOutOfRangeException(nameof(command))
        }
        : TopHelp();
}
