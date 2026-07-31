using System.Text.Json;
using NoxAeterna.Tools.Repository.Analysis;
using NoxAeterna.Tools.Repository.Context.Routing;

namespace NoxAeterna.Tools.Repository.Context.Evaluation;

public sealed class ContextEvaluationRunner
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public ContextEvaluationRegistry Load(
        string repositoryRoot,
        string evalPath,
        ContextRouteRegistry routes,
        IReadOnlyList<RepositoryFileEntry> inventory)
    {
        ContextEvaluationRegistry registry;
        try
        {
            registry = JsonSerializer.Deserialize<ContextEvaluationRegistry>(
                           File.ReadAllText(ContextRegistryLoader.ResolveInputPath(repositoryRoot, evalPath)), JsonOptions)
                       ?? throw new ContextEvaluationException("Context evaluation registry is empty.");
        }
        catch (JsonException exception)
        {
            throw new ContextEvaluationException($"Context evaluation registry is not valid JSON: {exception.Message}");
        }

        if (registry.SchemaVersion != 1 || registry.Cases.Count == 0)
            throw new ContextEvaluationException("Context evaluation registry must use schemaVersion 1 and contain cases.");
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var taskNames = routes.TaskKinds.Select(static task => task.Name).ToHashSet(StringComparer.Ordinal);
        var ruleNames = routes.PathRules.Select(static rule => rule.Name).ToHashSet(StringComparer.Ordinal);
        var testRoutes = routes.PathRules.SelectMany(static rule => rule.TestRoutes).ToHashSet(StringComparer.Ordinal);
        var publicPaths = inventory.Select(static file => file.Path).ToHashSet(StringComparer.Ordinal);
        foreach (var item in registry.Cases)
        {
            if (string.IsNullOrWhiteSpace(item.Name) || !names.Add(item.Name))
                throw new ContextEvaluationException($"Duplicate or empty evaluation case '{item.Name}'.");
            if (!taskNames.Contains(item.Task))
                throw new ContextEvaluationException($"Evaluation '{item.Name}' references unknown task '{item.Task}'.");
            if (item.Paths.Count == 0 || item.BudgetChars <= 0 || item.MaxSelectedFiles <= 0 || item.MaxSelectedChars <= 0)
                throw new ContextEvaluationException($"Evaluation '{item.Name}' has invalid bounds or no target paths.");
            foreach (var required in item.MustInclude)
                if (!publicPaths.Contains(required))
                    throw new ContextEvaluationException($"Evaluation '{item.Name}' references missing mustInclude path '{required}'.");
            foreach (var pattern in item.MustExclude)
                if (!RepositoryGlob.IsValidPattern(pattern, out var error))
                    throw new ContextEvaluationException($"Evaluation '{item.Name}' has invalid exclusion '{pattern}': {error}");
            foreach (var rule in item.ExpectedMatchedRules)
                if (!ruleNames.Contains(rule))
                    throw new ContextEvaluationException($"Evaluation '{item.Name}' references unknown rule '{rule}'.");
            foreach (var route in item.ExpectedTestRoutes.Concat(item.ForbiddenTestRoutes))
                if (!testRoutes.Contains(route))
                    throw new ContextEvaluationException($"Evaluation '{item.Name}' references unknown test route '{route}'.");
        }
        return registry;
    }

    public ContextEvaluationReport Run(
        string repositoryRoot,
        ContextRouteRegistry routes,
        ContextEvaluationRegistry evaluations,
        IReadOnlyList<RepositoryFileEntry> inventory,
        string? caseName = null)
    {
        var cases = string.IsNullOrWhiteSpace(caseName)
            ? evaluations.Cases
            : evaluations.Cases.Where(item => item.Name.Equals(caseName, StringComparison.Ordinal)).ToArray();
        if (cases.Count == 0)
            throw new ContextEvaluationException($"Unknown context evaluation case '{caseName}'.");

        var results = new List<ContextEvaluationCaseResult>();
        foreach (var item in cases)
        {
            var plan = new ContextPlanner().Plan(repositoryRoot, routes, inventory, item.Task, item.Paths, item.BudgetChars);
            var selected = plan.Plan.SelectedFiles.Select(static file => file.Path).ToHashSet(StringComparer.Ordinal);
            var differences = new List<string>();
            if (!plan.Succeeded) differences.Add("Planner did not succeed.");
            foreach (var path in item.MustInclude.Where(path => !selected.Contains(path))) differences.Add($"Missing selected file: {path}");
            foreach (var pattern in item.MustExclude)
                foreach (var path in selected.Where(path => RepositoryGlob.IsMatch(pattern, path)))
                    differences.Add($"Forbidden selected file: {path} (pattern {pattern})");
            foreach (var rule in item.ExpectedMatchedRules.Where(rule => !plan.Plan.MatchedRules.Contains(rule, StringComparer.Ordinal)))
                differences.Add($"Missing matched rule: {rule}");
            foreach (var route in item.ExpectedTestRoutes.Where(route => !plan.Plan.TestRoutes.Contains(route, StringComparer.Ordinal)))
                differences.Add($"Missing test route: {route}");
            foreach (var route in item.ForbiddenTestRoutes.Where(route => plan.Plan.TestRoutes.Contains(route, StringComparer.Ordinal)))
                differences.Add($"Forbidden test route: {route}");
            if (plan.Plan.SelectedFiles.Count > item.MaxSelectedFiles)
                differences.Add($"Selected file count {plan.Plan.SelectedFiles.Count} exceeds {item.MaxSelectedFiles}.");
            if (plan.Plan.Budget.SelectedCharacters > item.MaxSelectedChars)
                differences.Add($"Selected characters {plan.Plan.Budget.SelectedCharacters} exceeds {item.MaxSelectedChars}.");
            results.Add(new ContextEvaluationCaseResult(item.Name, differences.Count == 0, differences));
        }
        return new ContextEvaluationReport(1, results.All(static result => result.Passed) ? "pass" : "fail", results);
    }
}
