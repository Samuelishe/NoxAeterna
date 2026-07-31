using System.Text.Json;
using NoxAeterna.Tools.Repository.Analysis;
using NoxAeterna.Tools.Repository.Context.Routing;

namespace NoxAeterna.Tools.Repository.Context.Evaluation;

public static class ContextEvalCli
{
    public static int Run(IReadOnlyList<string> arguments, TextWriter output, TextWriter error)
    {
        var parsed = Parse(arguments);
        if (parsed.Error is not null)
        {
            error.WriteLine($"Error: {parsed.Error}");
            return 2;
        }
        if (parsed.Help)
        {
            output.WriteLine("Usage: context-eval [--case NAME] [--root ROOT] [--routes PATH] [--evals PATH] [--json]");
            return 0;
        }
        try
        {
            var root = GitRepositoryInventory.ResolveRoot(parsed.Root);
            var inventory = new GitRepositoryInventory().Discover(root);
            var routes = ContextRegistryLoader.Load(root, parsed.Routes, inventory.Files);
            var runner = new ContextEvaluationRunner();
            var evals = runner.Load(root, parsed.Evals, routes, inventory.Files);
            var report = runner.Run(root, routes, evals, inventory.Files, parsed.Case);
            if (parsed.Json)
            {
                output.Write(JsonSerializer.Serialize(report, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                }));
                output.WriteLine();
            }
            else
            {
                foreach (var item in report.Cases)
                {
                    output.WriteLine($"{(item.Passed ? "PASS" : "FAIL")}: {item.Name}");
                    foreach (var difference in item.Differences) output.WriteLine($"  {difference}");
                }
                output.WriteLine($"Context evaluations: {report.Cases.Count(static item => item.Passed)}/{report.Cases.Count} passed.");
            }
            return report.Result == "pass" ? 0 : 1;
        }
        catch (Exception exception) when (exception is ArgumentException or IOException or UnauthorizedAccessException or ContextRegistryException or ContextEvaluationException)
        {
            error.WriteLine($"Error: {exception.Message}");
            return 2;
        }
    }

    private static Options Parse(IReadOnlyList<string> args)
    {
        var start = args.Count > 0 && args[0] == "context-eval" ? 1 : 0;
        string? caseName = null;
        string? root = null;
        var routes = "eng/context-routes.json";
        var evals = "eng/context-evals.json";
        var json = false;
        var help = false;
        for (var index = start; index < args.Count; index++)
        {
            switch (args[index])
            {
                case "--help" or "-h": help = true; break;
                case "--json": json = true; break;
                case "--case": if (!Next(out caseName)) return new Options(Error: "--case requires a value."); break;
                case "--root": if (!Next(out root)) return new Options(Error: "--root requires a value."); break;
                case "--routes":
                    if (!Next(out var routesValue)) return new Options(Error: "--routes requires a value.");
                    routes = routesValue!;
                    break;
                case "--evals":
                    if (!Next(out var evalsValue)) return new Options(Error: "--evals requires a value.");
                    evals = evalsValue!;
                    break;
                default: return new Options(Error: $"Unknown argument '{args[index]}'.");
            }
            bool Next(out string? value)
            {
                value = null;
                if (++index >= args.Count || string.IsNullOrWhiteSpace(args[index])) return false;
                value = args[index];
                return true;
            }
        }
        return new Options(caseName, root, routes, evals, json, help, null);
    }

    private sealed record Options(
        string? Case = null,
        string? Root = null,
        string Routes = "eng/context-routes.json",
        string Evals = "eng/context-evals.json",
        bool Json = false,
        bool Help = false,
        string? Error = null);
}
