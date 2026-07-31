using NoxAeterna.Tools.Repository.Analysis;
using NoxAeterna.Tools.Repository.Context.Routing;

namespace NoxAeterna.Tools.Repository.Context;

public sealed class ContextPlanner
{
    public const int MaximumBudgetCharacters = 500_000;

    public ContextPlanResult Plan(
        string repositoryRoot,
        ContextRouteRegistry registry,
        IReadOnlyList<RepositoryFileEntry> inventory,
        string taskKindName,
        IReadOnlyList<string> requestedPaths,
        int budgetCharacters)
    {
        if (budgetCharacters is <= 0 or > MaximumBudgetCharacters)
        {
            return InputFailure(taskKindName, budgetCharacters,
                $"Budget must be between 1 and {MaximumBudgetCharacters} characters.");
        }

        var task = registry.TaskKinds.FirstOrDefault(item => item.Name.Equals(taskKindName, StringComparison.Ordinal));
        if (task is null)
        {
            return InputFailure(taskKindName, budgetCharacters, $"Unknown task kind '{taskKindName}'.");
        }
        if (requestedPaths.Count == 0)
        {
            return InputFailure(taskKindName, budgetCharacters, "At least one target path is required.");
        }

        var files = inventory.ToDictionary(static file => file.Path, StringComparer.Ordinal);
        var canonicalTargets = new List<string>();
        var seenTargets = new HashSet<string>(StringComparer.Ordinal);
        var diagnostics = new List<RepositoryDiagnostic>();
        var fatalTarget = false;
        foreach (var raw in requestedPaths)
        {
            if (!ContextTargetPath.TryCanonicalize(raw, out var path, out var error))
            {
                diagnostics.Add(new RepositoryDiagnostic("target-invalid", "error", null, error!));
                fatalTarget = true;
                continue;
            }
            if (!seenTargets.Add(path))
            {
                continue;
            }
            if (ContextTargetPath.IsForbidden(path) || ContextTargetPath.IsIgnored(repositoryRoot, path))
            {
                diagnostics.Add(new RepositoryDiagnostic("target-refused", "error", null,
                    "A private, ignored, or generated target was refused without reading its contents."));
                fatalTarget = true;
                continue;
            }
            canonicalTargets.Add(path);
        }

        if (fatalTarget)
        {
            return Finish(task.Name, canonicalTargets, [], [], [], [], budgetCharacters, diagnostics, false);
        }

        var matchedRules = registry.PathRules
            .Where(rule => rule.TaskKinds.Contains(task.Name, StringComparer.Ordinal) &&
                           canonicalTargets.Any(target => rule.Patterns.Any(pattern => RepositoryGlob.IsMatch(pattern, target))))
            .OrderBy(static rule => rule.Priority)
            .ThenBy(static rule => rule.Name, StringComparer.Ordinal)
            .ToArray();

        var candidates = new Dictionary<string, Candidate>(StringComparer.Ordinal);
        var insertionOrder = 0;
        foreach (var target in canonicalTargets)
        {
            if (files.TryGetValue(target, out var file))
            {
                AddCandidate(file, true, file.IsText ? file.Characters ?? 0 : 0, !file.IsText,
                    "explicit-target", 0, insertionOrder++);
            }
            else
            {
                var absolute = Path.Combine(repositoryRoot, target.Replace('/', Path.DirectorySeparatorChar));
                diagnostics.Add(Directory.Exists(absolute)
                    ? new RepositoryDiagnostic("directory-routing-only", "info", target,
                        "Directory targets add routing only; children are not selected automatically.")
                    : new RepositoryDiagnostic("planned-target", "info", target,
                        "The planned public path participates in routing but is not a readable selected file."));
            }
        }

        AddDocument("docs/PROJECT-STATE.md", true, "task-mandatory", 1, 0);
        foreach (var document in task.MandatoryDocuments
                     .Where(static document => document.Path != "docs/PROJECT-STATE.md")
                     .OrderBy(static document => document.Priority)
                     .ThenBy(static document => document.Path, StringComparer.Ordinal))
        {
            AddDocument(document.Path, true, "task-mandatory", 2, document.Priority);
        }
        foreach (var rule in matchedRules)
        {
            foreach (var document in rule.Documents.Order(StringComparer.Ordinal))
            {
                AddDocument(document, true, $"path-rule:{rule.Name}", 3, rule.Priority);
            }
        }
        foreach (var document in task.RecommendedDocuments
                     .OrderBy(static document => document.Priority)
                     .ThenBy(static document => document.Path, StringComparer.Ordinal))
        {
            AddDocument(document.Path, false, "task-recommended", 4, document.Priority);
        }

        var mandatory = candidates.Values.Where(static item => item.Mandatory)
            .OrderBy(static item => item.Group).ThenBy(static item => item.Order)
            .ThenBy(static item => item.Path, StringComparer.Ordinal).ToArray();
        var mandatoryCharacters = mandatory.Sum(static item => item.Characters);
        if (mandatory.Length > task.MaxSelectedFiles || mandatoryCharacters > budgetCharacters)
        {
            diagnostics.Add(new RepositoryDiagnostic(
                mandatory.Length > task.MaxSelectedFiles ? "mandatory-file-limit" : "mandatory-budget",
                "error", null,
                $"Mandatory context requires {mandatory.Length} files and {mandatoryCharacters} characters; " +
                $"requested limits are {task.MaxSelectedFiles} files and {budgetCharacters} characters."));
            return Finish(task.Name, canonicalTargets, matchedRules, mandatory, [], CollectRoutes(matchedRules),
                budgetCharacters, diagnostics, false);
        }

        var selected = mandatory.ToList();
        var omitted = new List<string>();
        var selectedCharacters = mandatoryCharacters;
        foreach (var candidate in candidates.Values.Where(static item => !item.Mandatory)
                     .OrderBy(static item => item.Group).ThenBy(static item => item.Order)
                     .ThenBy(static item => item.Path, StringComparer.Ordinal))
        {
            if (selected.Count < task.MaxSelectedFiles && selectedCharacters + candidate.Characters <= budgetCharacters)
            {
                selected.Add(candidate);
                selectedCharacters += candidate.Characters;
            }
            else
            {
                omitted.Add(candidate.Path);
            }
        }

        return Finish(task.Name, canonicalTargets, matchedRules, selected, omitted, CollectRoutes(matchedRules),
            budgetCharacters, diagnostics, true);

        void AddDocument(string path, bool mandatoryDocument, string reason, int group, int order)
        {
            if (!files.TryGetValue(path, out var file))
            {
                diagnostics.Add(new RepositoryDiagnostic("owner-missing", "error", path,
                    "A validated context owner is absent from factual inventory."));
                return;
            }
            AddCandidate(file, mandatoryDocument, file.Characters ?? 0, false, reason, group, order);
        }

        void AddCandidate(RepositoryFileEntry file, bool required, int characters, bool metadataOnly,
            string reason, int group, int order)
        {
            if (candidates.TryGetValue(file.Path, out var existing))
            {
                candidates[file.Path] = existing with
                {
                    Mandatory = existing.Mandatory || required,
                    Reasons = existing.Reasons.Append(reason).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray(),
                    Group = Math.Min(existing.Group, group),
                    Order = Math.Min(existing.Order, order)
                };
                return;
            }
            candidates[file.Path] = new Candidate(file.Path, characters, required, metadataOnly, [reason], group, order);
        }
    }

    private static IReadOnlyList<string> CollectRoutes(IEnumerable<ContextPathRule> rules) =>
        rules.SelectMany(static rule => rule.TestRoutes).Distinct(StringComparer.Ordinal).ToArray();

    private static ContextPlanResult Finish(
        string taskKind,
        IReadOnlyList<string> targets,
        IReadOnlyList<ContextPathRule> rules,
        IReadOnlyList<Candidate> selected,
        IReadOnlyList<string> omitted,
        IReadOnlyList<string> routes,
        int budget,
        IReadOnlyList<RepositoryDiagnostic> diagnostics,
        bool succeeded)
    {
        var selectedCharacters = selected.Sum(static candidate => candidate.Characters);
        var mandatoryCharacters = selected.Where(static candidate => candidate.Mandatory).Sum(static candidate => candidate.Characters);
        return new ContextPlanResult(new ContextPlan(
            1,
            taskKind,
            targets,
            rules.Select(static rule => rule.Name).ToArray(),
            selected.Select(static candidate => new ContextSelectedFile(
                candidate.Path, candidate.Characters, candidate.Mandatory, candidate.MetadataOnly, candidate.Reasons)).ToArray(),
            omitted,
            routes,
            new ContextBudget(budget, mandatoryCharacters, selectedCharacters, Math.Max(0, budget - selectedCharacters)),
            diagnostics.OrderBy(static diagnostic => diagnostic.Code, StringComparer.Ordinal)
                .ThenBy(static diagnostic => diagnostic.Path, StringComparer.Ordinal).ToArray()),
            succeeded && diagnostics.All(static diagnostic => diagnostic.Severity != "error"));
    }

    private static ContextPlanResult InputFailure(
        string taskKind, int budget, string message) =>
        Finish(taskKind, [], [], [], [], [], budget,
            [new RepositoryDiagnostic("input-invalid", "error", null, message)], false);

    private sealed record Candidate(
        string Path,
        int Characters,
        bool Mandatory,
        bool MetadataOnly,
        IReadOnlyList<string> Reasons,
        int Group,
        int Order);
}
