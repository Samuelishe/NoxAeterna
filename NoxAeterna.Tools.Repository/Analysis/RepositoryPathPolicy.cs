namespace NoxAeterna.Tools.Repository.Analysis;

public static class RepositoryPathPolicy
{
    private static readonly HashSet<string> GeneratedSegments = new(StringComparer.OrdinalIgnoreCase)
    {
        ".git", "bin", "obj", "TestResults", ".codex-cache", ".testagent",
        ".idea", ".vs", ".vscode", "artifacts", "publish"
    };

    private static readonly HashSet<string> PrivateSegments = new(StringComparer.OrdinalIgnoreCase)
    {
        "private", "sensitive", "local"
    };

    private static readonly HashSet<string> TextExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".cs", ".axaml", ".xaml", ".md", ".json", ".ps1", ".psm1", ".yml",
        ".yaml", ".props", ".targets", ".csproj", ".sln", ".txt"
    };

    public static string Normalize(string path) => path.Replace('\\', '/').TrimStart('/');

    public static bool IsTextExtension(string extension) => TextExtensions.Contains(extension);

    public static bool IsGenerated(string path)
    {
        var normalized = Normalize(path);
        var segments = normalized.Split('/', StringSplitOptions.RemoveEmptyEntries);
        return segments.Any(GeneratedSegments.Contains) ||
               normalized.Equals("project-stats.md", StringComparison.OrdinalIgnoreCase) ||
               normalized.Equals("project-stats.json", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsPrivateOrSensitive(string path)
    {
        var normalized = Normalize(path);
        var segments = normalized.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var fileName = segments.LastOrDefault() ?? string.Empty;
        return segments.Any(PrivateSegments.Contains) ||
               fileName.Contains(".local.", StringComparison.OrdinalIgnoreCase) ||
               fileName.EndsWith(".user", StringComparison.OrdinalIgnoreCase) ||
               fileName.EndsWith(".suo", StringComparison.OrdinalIgnoreCase) ||
               fileName.EndsWith(".tmp", StringComparison.OrdinalIgnoreCase) ||
               fileName.EndsWith(".temp", StringComparison.OrdinalIgnoreCase);
    }

    public static RepositoryFileCategory Classify(string path)
    {
        var normalized = Normalize(path);
        if (normalized.StartsWith(".github/", StringComparison.OrdinalIgnoreCase))
        {
            return RepositoryFileCategory.Workflow;
        }

        if (normalized.StartsWith("NoxAeterna.Tests/", StringComparison.OrdinalIgnoreCase))
        {
            return RepositoryFileCategory.Tests;
        }

        if (normalized.StartsWith("NoxAeterna.Tools.Repository/", StringComparison.OrdinalIgnoreCase) ||
            normalized.StartsWith("eng/", StringComparison.OrdinalIgnoreCase))
        {
            return RepositoryFileCategory.Tooling;
        }

        if (normalized.StartsWith("docs/", StringComparison.OrdinalIgnoreCase) ||
            normalized.Equals("README.md", StringComparison.OrdinalIgnoreCase) ||
            normalized.Equals("AGENTS.md", StringComparison.OrdinalIgnoreCase))
        {
            return RepositoryFileCategory.Documentation;
        }

        if (normalized.StartsWith("resources/", StringComparison.OrdinalIgnoreCase) ||
            IsAssetExtension(Path.GetExtension(normalized)))
        {
            return RepositoryFileCategory.Resources;
        }

        if (normalized.StartsWith("NoxAeterna.", StringComparison.OrdinalIgnoreCase))
        {
            return RepositoryFileCategory.Production;
        }

        return RepositoryFileCategory.Other;
    }

    public static string GetProjectArea(string path)
    {
        var segments = Normalize(path).Split('/', StringSplitOptions.RemoveEmptyEntries);
        return segments.Length == 0 ? "." : segments[0];
    }

    public static string GetDensityGroup(string path)
    {
        var segments = Normalize(path).Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length == 0)
        {
            return ".";
        }

        if (segments[0].StartsWith("NoxAeterna.", StringComparison.OrdinalIgnoreCase) || segments.Length == 1)
        {
            return segments[0];
        }

        return $"{segments[0]}/{segments[1]}";
    }

    private static bool IsAssetExtension(string extension) => extension.ToLowerInvariant() is
        ".png" or ".jpg" or ".jpeg" or ".gif" or ".webp" or ".svg" or ".ico" or ".ttf" or ".otf";
}
