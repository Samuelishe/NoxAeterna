namespace NoxAeterna.Tools.Repository.Analysis;

public enum MsBuildProjectPathFailure
{
    None,
    EmptyInclude,
    UnsupportedExpression,
    AbsolutePath,
    RepositoryEscape,
    InvalidProjectPath
}

public readonly record struct MsBuildProjectPathResolution(
    string? Path,
    MsBuildProjectPathFailure Failure)
{
    public bool Succeeded => Path is not null && Failure == MsBuildProjectPathFailure.None;
}

public static class MsBuildProjectPathResolver
{
    public static MsBuildProjectPathResolution Resolve(string currentProjectPath, string? include)
    {
        if (string.IsNullOrWhiteSpace(include))
        {
            return Failure(MsBuildProjectPathFailure.EmptyInclude);
        }

        var trimmedInclude = include.Trim();
        if (ContainsUnsupportedExpression(trimmedInclude))
        {
            return Failure(MsBuildProjectPathFailure.UnsupportedExpression);
        }

        if (IsAbsolute(trimmedInclude))
        {
            return Failure(MsBuildProjectPathFailure.AbsolutePath);
        }

        var projectSegments = new List<string>();
        if (!AppendSegments(projectSegments, currentProjectPath, allowParents: false) || projectSegments.Count == 0)
        {
            return Failure(MsBuildProjectPathFailure.InvalidProjectPath);
        }

        projectSegments.RemoveAt(projectSegments.Count - 1);
        if (!AppendSegments(projectSegments, trimmedInclude, allowParents: true) || projectSegments.Count == 0)
        {
            return Failure(MsBuildProjectPathFailure.RepositoryEscape);
        }

        return new MsBuildProjectPathResolution(string.Join('/', projectSegments), MsBuildProjectPathFailure.None);
    }

    private static bool AppendSegments(List<string> resolved, string path, bool allowParents)
    {
        foreach (var segment in path.Replace('\\', '/').Split('/', StringSplitOptions.RemoveEmptyEntries))
        {
            if (segment == ".")
            {
                continue;
            }

            if (segment == "..")
            {
                if (!allowParents || resolved.Count == 0)
                {
                    return false;
                }

                resolved.RemoveAt(resolved.Count - 1);
                continue;
            }

            resolved.Add(segment);
        }

        return true;
    }

    private static bool ContainsUnsupportedExpression(string include) =>
        include.Contains("$(", StringComparison.Ordinal) ||
        include.Contains("@(", StringComparison.Ordinal) ||
        include.Contains("%(", StringComparison.Ordinal) ||
        include.Contains('*') ||
        include.Contains('?');

    private static bool IsAbsolute(string include)
    {
        var normalized = include.Replace('\\', '/');
        return normalized.StartsWith('/') ||
               normalized.Length >= 2 && char.IsAsciiLetter(normalized[0]) && normalized[1] == ':';
    }

    private static MsBuildProjectPathResolution Failure(MsBuildProjectPathFailure failure) => new(null, failure);
}
