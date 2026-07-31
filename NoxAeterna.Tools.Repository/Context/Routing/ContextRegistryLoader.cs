using System.Text.Json;
using NoxAeterna.Tools.Repository.Analysis;

namespace NoxAeterna.Tools.Repository.Context.Routing;

public static class ContextRegistryLoader
{
    private static readonly HashSet<string> RequiredTaskKinds = new(StringComparer.Ordinal)
    {
        "CodeChange", "StructuralRefactor", "TestChange", "UiChange",
        "Documentation", "Tooling", "AssetChange"
    };

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static ContextRouteRegistry Load(
        string repositoryRoot,
        string registryPath,
        IReadOnlyList<RepositoryFileEntry> inventoryFiles,
        string testRoutesPath = "eng/test-routes.json")
    {
        var absoluteRegistry = ResolveInputPath(repositoryRoot, registryPath);
        ContextRouteRegistry registry;
        try
        {
            registry = JsonSerializer.Deserialize<ContextRouteRegistry>(File.ReadAllText(absoluteRegistry), JsonOptions)
                       ?? throw new ContextRegistryException("Context route registry is empty.");
        }
        catch (JsonException exception)
        {
            throw new ContextRegistryException($"Context route registry is not valid JSON: {exception.Message}");
        }

        var testRouteNames = ReadTestRouteNames(ResolveInputPath(repositoryRoot, testRoutesPath));
        Validate(registry, inventoryFiles, testRouteNames);
        return registry;
    }

    public static void Validate(
        ContextRouteRegistry registry,
        IReadOnlyList<RepositoryFileEntry> inventoryFiles,
        IReadOnlySet<string> testRouteNames)
    {
        if (registry.SchemaVersion != 1)
        {
            throw new ContextRegistryException("Only context route registry schemaVersion 1 is supported.");
        }

        var publicTextPaths = inventoryFiles
            .Where(static file => file.IsText)
            .Select(static file => file.Path)
            .ToHashSet(StringComparer.Ordinal);
        var taskNames = UniqueNames(registry.TaskKinds.Select(static task => task.Name), "task kind");
        if (!registry.TaskKinds.Select(static task => task.Name).ToHashSet(StringComparer.Ordinal).SetEquals(RequiredTaskKinds))
        {
            throw new ContextRegistryException("Context registry must declare exactly the supported initial task kinds.");
        }

        foreach (var task in registry.TaskKinds)
        {
            if (task.MaxSelectedFiles <= 0)
            {
                throw new ContextRegistryException($"Task kind '{task.Name}' requires a positive maxSelectedFiles.");
            }

            var ownerPaths = new HashSet<string>(StringComparer.Ordinal);
            ValidateDocuments(task.MandatoryDocuments, task.Name, ownerPaths, publicTextPaths);
            ValidateDocuments(task.RecommendedDocuments, task.Name, ownerPaths, publicTextPaths);
        }

        UniqueNames(registry.PathRules.Select(static rule => rule.Name), "path rule");
        foreach (var rule in registry.PathRules)
        {
            if (rule.Priority <= 0 || rule.Patterns.Count == 0 || rule.TaskKinds.Count == 0)
            {
                throw new ContextRegistryException($"Path rule '{rule.Name}' requires positive priority, patterns, and task kinds.");
            }

            foreach (var pattern in rule.Patterns)
            {
                if (!RepositoryGlob.IsValidPattern(pattern, out var error))
                {
                    throw new ContextRegistryException($"Path rule '{rule.Name}' has invalid pattern '{pattern}': {error}");
                }
            }

            foreach (var taskKind in rule.TaskKinds)
            {
                if (!taskNames.Contains(taskKind))
                {
                    throw new ContextRegistryException($"Path rule '{rule.Name}' references unknown task kind '{taskKind}'.");
                }
            }

            var documents = new HashSet<string>(StringComparer.Ordinal);
            foreach (var document in rule.Documents)
            {
                ValidateDocumentPath(document, $"path rule '{rule.Name}'", publicTextPaths);
                if (!documents.Add(document))
                {
                    throw new ContextRegistryException($"Path rule '{rule.Name}' repeats document '{document}'.");
                }
            }

            var routes = new HashSet<string>(StringComparer.Ordinal);
            foreach (var route in rule.TestRoutes)
            {
                if (!routes.Add(route))
                {
                    throw new ContextRegistryException($"Path rule '{rule.Name}' repeats test route '{route}'.");
                }
                if (!testRouteNames.Contains(route))
                {
                    throw new ContextRegistryException($"Path rule '{rule.Name}' references unknown test route '{route}'.");
                }
            }
        }
    }

    private static void ValidateDocuments(
        IReadOnlyList<ContextDocumentReference> documents,
        string owner,
        ISet<string> ownerPaths,
        IReadOnlySet<string> publicTextPaths)
    {
        foreach (var document in documents)
        {
            if (document.Priority <= 0)
            {
                throw new ContextRegistryException($"Task kind '{owner}' has a non-positive document priority.");
            }
            ValidateDocumentPath(document.Path, $"task kind '{owner}'", publicTextPaths);
            if (!ownerPaths.Add(document.Path))
            {
                throw new ContextRegistryException($"Task kind '{owner}' repeats document '{document.Path}'.");
            }
        }
    }

    private static void ValidateDocumentPath(
        string path,
        string owner,
        IReadOnlySet<string> publicTextPaths)
    {
        if (!IsCanonicalRelativePath(path) ||
            path.StartsWith("docs/archive/", StringComparison.Ordinal) ||
            RepositoryPathPolicy.IsPrivateOrSensitive(path) ||
            RepositoryPathPolicy.IsGenerated(path))
        {
            throw new ContextRegistryException($"{owner} contains unsafe context document '{path}'.");
        }
        if (!publicTextPaths.Contains(path))
        {
            throw new ContextRegistryException($"{owner} references missing public text document '{path}'.");
        }
    }

    public static bool IsCanonicalRelativePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || path.Contains('\\') || path.StartsWith('/') ||
            path.Length >= 2 && char.IsAsciiLetter(path[0]) && path[1] == ':')
        {
            return false;
        }

        return path.Split('/', StringSplitOptions.None).All(static segment => segment.Length > 0 && segment is not "." and not "..");
    }

    public static string ResolveInputPath(string root, string path) =>
        Path.GetFullPath(Path.IsPathRooted(path) ? path : Path.Combine(root, path));

    private static HashSet<string> ReadTestRouteNames(string path)
    {
        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(path));
            if (document.RootElement.GetProperty("schemaVersion").GetInt32() != 1)
            {
                throw new ContextRegistryException("Only test route registry schemaVersion 1 is supported.");
            }
            return document.RootElement.GetProperty("routes").EnumerateArray()
                .Select(static route => route.GetProperty("name").GetString() ?? string.Empty)
                .ToHashSet(StringComparer.Ordinal);
        }
        catch (Exception exception) when (exception is JsonException or InvalidOperationException or KeyNotFoundException)
        {
            throw new ContextRegistryException($"Test route registry is not valid JSON: {exception.Message}");
        }
    }

    private static HashSet<string> UniqueNames(IEnumerable<string> names, string kind)
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var name in names)
        {
            if (string.IsNullOrWhiteSpace(name) || !result.Add(name))
            {
                throw new ContextRegistryException($"Context registry contains an empty or duplicate {kind} name '{name}'.");
            }
        }
        return result;
    }
}
