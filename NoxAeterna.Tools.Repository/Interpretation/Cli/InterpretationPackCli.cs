using NoxAeterna.Tools.Repository.Interpretation.Analysis;
using NoxAeterna.Tools.Repository.Interpretation.Compilation;
using NoxAeterna.Tools.Repository.Interpretation.Reports;

namespace NoxAeterna.Tools.Repository.Interpretation.Cli;

public enum InterpretationPackCommand { ValidateSource, Compile, InspectPackage, AuthoringStatus }

public sealed record InterpretationPackCliOptions(
    InterpretationPackCommand Command, string? SourceRoot, string? PackagePath, string? OutputPath,
    bool Check, bool Json, bool ShowHelp);

public sealed record InterpretationPackCliParseResult(InterpretationPackCliOptions? Options, string? Error)
{ public bool Succeeded => Options is not null && Error is null; }

public static class InterpretationPackCliParser
{
    public static InterpretationPackCliParseResult Parse(IReadOnlyList<string> arguments)
    {
        if (arguments.Count < 2 || arguments[0] != "interpretation-pack") return Fail("Usage requires 'interpretation-pack <subcommand>'.");
        if (arguments[1] is "--help" or "-h") return Ok(new(InterpretationPackCommand.ValidateSource,null,null,null,false,false,true));
        var command = arguments[1] switch { "validate-source"=>InterpretationPackCommand.ValidateSource,"compile"=>InterpretationPackCommand.Compile,"inspect-package"=>InterpretationPackCommand.InspectPackage,"authoring-status"=>InterpretationPackCommand.AuthoringStatus,_=>(InterpretationPackCommand?)null };
        if (command is null) return Fail($"Unknown interpretation-pack subcommand '{arguments[1]}'.");
        string? source=null; string? package=null; string? output=null; var check=false; var json=false; var help=false;
        for(var i=2;i<arguments.Count;i++) switch(arguments[i])
        {
            case "--help" or "-h": help=true; break;
            case "--json": json=true; break;
            case "--check" when command==InterpretationPackCommand.Compile: check=true; break;
            case "--source-root" when ++i<arguments.Count && source is null: source=arguments[i]; break;
            case "--package" when ++i<arguments.Count && package is null: package=arguments[i]; break;
            case "--output" when ++i<arguments.Count && output is null: output=arguments[i]; break;
            default:return Fail($"Unknown or repeated option '{arguments[i]}'.");
        }
        if(!help && command is InterpretationPackCommand.ValidateSource or InterpretationPackCommand.AuthoringStatus && source is null)return Fail("--source-root is required.");
        if(!help && command==InterpretationPackCommand.Compile && (source is null||output is null))return Fail("compile requires --source-root and --output.");
        if(!help && command==InterpretationPackCommand.InspectPackage && package is null)return Fail("--package is required.");
        return Ok(new(command.Value,source,package,output,check,json,help));
    }
    private static InterpretationPackCliParseResult Ok(InterpretationPackCliOptions x)=>new(x,null);
    private static InterpretationPackCliParseResult Fail(string x)=>new(null,x);
}

public static class InterpretationPackCli
{
    public static int Run(IReadOnlyList<string> arguments,TextWriter output,TextWriter error)
    {
        var parsed=InterpretationPackCliParser.Parse(arguments);if(!parsed.Succeeded){error.WriteLine($"Error: {parsed.Error}");error.WriteLine("Use 'interpretation-pack --help' for usage.");return 2;}
        var x=parsed.Options!;if(x.ShowHelp){output.Write(Help());return 0;}
        InterpretationToolReport report=x.Command switch
        {
            InterpretationPackCommand.ValidateSource or InterpretationPackCommand.AuthoringStatus=>new InterpretationPackValidator().Validate(x.SourceRoot!),
            InterpretationPackCommand.Compile=>new InterpretationPackageCompiler().Compile(x.SourceRoot!,x.OutputPath!,x.Check),
            InterpretationPackCommand.InspectPackage=>new InterpretationPackageCompiler().Inspect(x.PackagePath!),
            _=>throw new ArgumentOutOfRangeException()
        };
        output.Write(x.Json?InterpretationToolReportWriter.WriteJson(report):InterpretationToolReportWriter.WriteConsole(report));return report.Success?0:1;
    }
    public static string TopHelp()=>Help();
    private static string Help()=>"Tarot interpretation source/package tooling\n\nUsage:\n  interpretation-pack validate-source --source-root PATH [--json]\n  interpretation-pack compile --source-root PATH --output PATH [--check] [--json]\n  interpretation-pack inspect-package --package PATH [--json]\n  interpretation-pack authoring-status --source-root PATH [--json]\n";
}
