using NoxAeterna.Tools.Repository.Interpretation.Analysis;
using NoxAeterna.Tools.Repository.Interpretation.Compilation;
using NoxAeterna.Tools.Repository.Interpretation.Reports;

namespace NoxAeterna.Tools.Repository.Interpretation.Cli;

public enum InterpretationPackCommand { ValidateSource, Compile, InspectPackage, AuthoringStatus, AuditContent }

public sealed record InterpretationPackCliOptions(
    InterpretationPackCommand Command, string? SourceRoot, string? PackagePath, string? OutputPath,
    string? Locale, InterpretationAuthoringCorpus? Corpus, bool Check, bool Json, bool ShowHelp);

public sealed record InterpretationPackCliParseResult(InterpretationPackCliOptions? Options, string? Error)
{ public bool Succeeded => Options is not null && Error is null; }

public static class InterpretationPackCliParser
{
    public static InterpretationPackCliParseResult Parse(IReadOnlyList<string> arguments)
    {
        if (arguments.Count < 2 || arguments[0] != "interpretation-pack") return Fail("Usage requires 'interpretation-pack <subcommand>'.");
        if (arguments[1] is "--help" or "-h") return Ok(new(InterpretationPackCommand.ValidateSource,null,null,null,null,null,false,false,true));
        var command = arguments[1] switch { "validate-source"=>InterpretationPackCommand.ValidateSource,"compile"=>InterpretationPackCommand.Compile,"inspect-package"=>InterpretationPackCommand.InspectPackage,"authoring-status"=>InterpretationPackCommand.AuthoringStatus,"audit-content"=>InterpretationPackCommand.AuditContent,_=>(InterpretationPackCommand?)null };
        if (command is null) return Fail($"Unknown interpretation-pack subcommand '{arguments[1]}'.");
        string? source=null; string? package=null; string? output=null; string? locale=null; InterpretationAuthoringCorpus? corpus=null;
        var check=false; var json=false; var help=false; var seen=new HashSet<string>(StringComparer.Ordinal);
        for(var i=2;i<arguments.Count;i++)
        {
            var option=arguments[i];
            if(!seen.Add(option)) return Fail($"Unknown or repeated option '{option}'.");
            switch(option)
            {
                case "--help" or "-h": help=true; break;
                case "--json": json=true; break;
                case "--check" when command==InterpretationPackCommand.Compile: check=true; break;
                case "--source-root" when command is InterpretationPackCommand.ValidateSource or InterpretationPackCommand.Compile or InterpretationPackCommand.AuthoringStatus or InterpretationPackCommand.AuditContent:
                    if(!TryValue(arguments,ref i,out source))return Fail("--source-root requires a value.");break;
                case "--package" when command==InterpretationPackCommand.InspectPackage:
                    if(!TryValue(arguments,ref i,out package))return Fail("--package requires a value.");break;
                case "--output" when command==InterpretationPackCommand.Compile:
                    if(!TryValue(arguments,ref i,out output))return Fail("--output requires a value.");break;
                case "--locale" when command is InterpretationPackCommand.AuthoringStatus or InterpretationPackCommand.AuditContent:
                    if(!TryValue(arguments,ref i,out locale))return Fail("--locale requires a value.");break;
                case "--corpus" when command is InterpretationPackCommand.AuthoringStatus or InterpretationPackCommand.AuditContent:
                    if(!TryValue(arguments,ref i,out var rawCorpus))return Fail("--corpus requires a value.");
                    if(!InterpretationAuthoringCorpusNames.TryParse(rawCorpus!,out var parsedCorpus))return Fail($"Unknown corpus '{rawCorpus}'.");
                    corpus=parsedCorpus;break;
                default:return Fail($"Unknown or repeated option '{option}'.");
            }
        }
        if(!help && command is InterpretationPackCommand.ValidateSource or InterpretationPackCommand.AuthoringStatus && source is null)return Fail("--source-root is required.");
        if(!help && command==InterpretationPackCommand.AuditContent && source is null)return Fail("--source-root is required.");
        if(!help && command==InterpretationPackCommand.Compile && (source is null||output is null))return Fail("compile requires --source-root and --output.");
        if(!help && command==InterpretationPackCommand.InspectPackage && package is null)return Fail("--package is required.");
        if(!help && command==InterpretationPackCommand.AuthoringStatus && (locale is null)!=(corpus is null))return Fail("authoring-status requires --locale and --corpus together for scoped output.");
        if(!help && command==InterpretationPackCommand.AuditContent && (locale is null||corpus is null))return Fail("audit-content requires --locale and --corpus.");
        return Ok(new(command.Value,source,package,output,locale,corpus,check,json,help));
    }
    private static bool TryValue(IReadOnlyList<string> arguments,ref int index,out string? value){if(index+1>=arguments.Count||arguments[index+1].StartsWith("--",StringComparison.Ordinal)){value=null;return false;}value=arguments[++index];return true;}
    private static InterpretationPackCliParseResult Ok(InterpretationPackCliOptions x)=>new(x,null);
    private static InterpretationPackCliParseResult Fail(string x)=>new(null,x);
}

public static class InterpretationPackCli
{
    public static int Run(IReadOnlyList<string> arguments,TextWriter output,TextWriter error)
    {
        var parsed=InterpretationPackCliParser.Parse(arguments);if(!parsed.Succeeded){error.WriteLine($"Error: {parsed.Error}");error.WriteLine("Use 'interpretation-pack --help' for usage.");return 2;}
        var x=parsed.Options!;if(x.ShowHelp){output.Write(Help());return 0;}
        InterpretationToolReport report;
        try
        {
            report=x.Command switch
            {
                InterpretationPackCommand.ValidateSource=>new InterpretationPackValidator().Validate(x.SourceRoot!),
                InterpretationPackCommand.AuthoringStatus when x.Locale is not null && x.Corpus is not null=>new InterpretationAuthoringInventoryAnalyzer().Analyze(x.SourceRoot!,x.Locale,x.Corpus.Value),
                InterpretationPackCommand.AuthoringStatus=>new InterpretationPackValidator().Validate(x.SourceRoot!),
                InterpretationPackCommand.AuditContent=>new InterpretationContentAuditor().Audit(x.SourceRoot!,x.Locale!,x.Corpus!.Value),
                InterpretationPackCommand.Compile=>new InterpretationPackageCompiler().Compile(x.SourceRoot!,x.OutputPath!,x.Check),
                InterpretationPackCommand.InspectPackage=>new InterpretationPackageCompiler().Inspect(x.PackagePath!),
                _=>throw new ArgumentOutOfRangeException()
            };
        }
        catch(Exception exception) when(exception is ArgumentException or IOException or UnauthorizedAccessException)
        {
            report=new InterpretationToolReport([new InterpretationToolDiagnostic("tool.execution",InterpretationToolSeverity.Error,InterpretationAuthoringCorpusNames.Get(x.Corpus??InterpretationAuthoringCorpus.SingleCard),$"The requested source or package could not be processed ({exception.GetType().Name}).")]);
        }
        output.Write(x.Json?InterpretationToolReportWriter.WriteJson(report):InterpretationToolReportWriter.WriteConsole(report));return report.Success?0:1;
    }
    public static string TopHelp()=>Help();
    private static string Help()=>"Tarot interpretation source/package tooling\n\nUsage:\n  interpretation-pack validate-source --source-root PATH [--json]\n  interpretation-pack compile --source-root PATH --output PATH [--check] [--json]\n  interpretation-pack inspect-package --package PATH [--json]\n  interpretation-pack authoring-status --source-root PATH [--locale LOCALE --corpus CORPUS] [--json]\n  interpretation-pack audit-content --source-root PATH --locale LOCALE --corpus CORPUS [--json]\n\nCorpora:\n  single-card\n  oriented-pairs\n  three-card-positions\n  three-card-synthesis\n";
}
