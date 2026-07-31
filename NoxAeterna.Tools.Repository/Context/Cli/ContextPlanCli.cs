using System.Text.Json;
using NoxAeterna.Tools.Repository.Analysis;
using NoxAeterna.Tools.Repository.Context.Routing;

namespace NoxAeterna.Tools.Repository.Context.Cli;

public sealed record ContextPlanCliOptions(
    string Task,
    IReadOnlyList<string> Paths,
    int BudgetCharacters,
    string? Root,
    string Routes,
    bool Json,
    bool CompactJson,
    bool ShowHelp);

public sealed record ContextPlanParseResult(ContextPlanCliOptions? Options, string? Error)
{
    public bool Succeeded => Options is not null && Error is null;
}

public static class ContextPlanCliParser
{
    public static ContextPlanParseResult Parse(IReadOnlyList<string> arguments)
    {
        if (arguments.Count > 0 && arguments[0] == "context-plan")
        {
            arguments = arguments.Skip(1).ToArray();
        }
        string task = string.Empty;
        var paths = new List<string>();
        var budget = 0;
        string? root = null;
        var routes = "eng/context-routes.json";
        var json = false;
        var compact = false;
        var help = false;
        for (var index = 0; index < arguments.Count; index++)
        {
            var argument = arguments[index];
            switch (argument)
            {
                case "--help" or "-h": help = true; break;
                case "--json": json = true; break;
                case "--compact-json": compact = true; break;
                case "--task":
                    if (!ReadValue(arguments, ref index, out task)) return Fail("--task requires a value.");
                    break;
                case "--path":
                    if (!ReadValue(arguments, ref index, out var path)) return Fail("--path requires a value.");
                    paths.Add(path);
                    break;
                case "--budget-chars":
                    if (!ReadValue(arguments, ref index, out var rawBudget) || !int.TryParse(rawBudget, out budget))
                        return Fail("--budget-chars requires an integer.");
                    break;
                case "--root":
                    if (!ReadValue(arguments, ref index, out root)) return Fail("--root requires a path.");
                    break;
                case "--routes":
                    if (!ReadValue(arguments, ref index, out routes)) return Fail("--routes requires a path.");
                    break;
                default: return Fail($"Unknown argument '{argument}'.");
            }
        }
        if (json && compact) return Fail("--json and --compact-json cannot be used together.");
        if (help) return Success(new ContextPlanCliOptions(task, paths, budget, root, routes, json, compact, true));
        if (string.IsNullOrWhiteSpace(task)) return Fail("--task is required.");
        if (paths.Count == 0) return Fail("At least one --path is required.");
        if (budget <= 0) return Fail("--budget-chars must be a positive integer.");
        return Success(new ContextPlanCliOptions(task, paths, budget, root, routes, json, compact, false));
    }

    private static bool ReadValue(IReadOnlyList<string> args, ref int index, out string value)
    {
        value = string.Empty;
        if (++index >= args.Count || string.IsNullOrWhiteSpace(args[index])) return false;
        value = args[index];
        return true;
    }
    private static ContextPlanParseResult Success(ContextPlanCliOptions options) => new(options, null);
    private static ContextPlanParseResult Fail(string error) => new(null, error);
}

public static class ContextPlanCli
{
    public static int Run(IReadOnlyList<string> arguments, TextWriter output, TextWriter error)
    {
        var parsed = ContextPlanCliParser.Parse(arguments);
        if (!parsed.Succeeded)
        {
            error.WriteLine($"Error: {parsed.Error}");
            return 2;
        }
        var options = parsed.Options!;
        if (options.ShowHelp)
        {
            output.Write(Help());
            return 0;
        }
        try
        {
            var root = GitRepositoryInventory.ResolveRoot(options.Root);
            var inventory = new GitRepositoryInventory().Discover(root);
            var registry = ContextRegistryLoader.Load(root, options.Routes, inventory.Files);
            var result = new ContextPlanner().Plan(root, registry, inventory.Files, options.Task, options.Paths, options.BudgetCharacters);
            output.Write(options.Json || options.CompactJson
                ? WriteJson(result.Plan, options.CompactJson)
                : WriteConsole(result.Plan));
            return result.Succeeded ? 0 : 2;
        }
        catch (Exception exception) when (exception is ArgumentException or IOException or UnauthorizedAccessException or ContextRegistryException)
        {
            error.WriteLine($"Error: {exception.Message}");
            return 2;
        }
    }

    public static string WriteJson(ContextPlan plan, bool compact) =>
        JsonSerializer.Serialize(plan, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = !compact
        }) + Environment.NewLine;

    public static string WriteConsole(ContextPlan plan)
    {
        var writer = new StringWriter();
        writer.WriteLine($"Context plan: {plan.TaskKind}");
        writer.WriteLine($"Matched rules: {(plan.MatchedRules.Count == 0 ? "none" : string.Join(", ", plan.MatchedRules))}");
        writer.WriteLine("Selected files:");
        foreach (var file in plan.SelectedFiles)
            writer.WriteLine($"  {file.Path} ({file.Characters} chars; {string.Join(", ", file.Reasons)})");
        writer.WriteLine($"Test routes: {(plan.TestRoutes.Count == 0 ? "none" : string.Join(", ", plan.TestRoutes))}");
        writer.WriteLine($"Budget: {plan.Budget.SelectedCharacters}/{plan.Budget.RequestedCharacters} chars; mandatory minimum {plan.Budget.MandatoryCharacters}");
        foreach (var diagnostic in plan.Diagnostics)
            writer.WriteLine($"{diagnostic.Severity.ToUpperInvariant()}: {diagnostic.Message}");
        return writer.ToString();
    }

    public static string Help() =>
        "Usage: context-plan --task TASK --path PATH [--path PATH] --budget-chars N [--root ROOT] [--routes PATH] [--json | --compact-json]" + Environment.NewLine;
}
