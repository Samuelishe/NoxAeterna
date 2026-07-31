namespace NoxAeterna.Tools.Repository.Context.Routing;

public static class RepositoryGlob
{
    public static bool IsValidPattern(string pattern, out string? error)
    {
        error = null;
        if (string.IsNullOrWhiteSpace(pattern))
        {
            error = "Pattern is empty.";
            return false;
        }

        if (pattern.Contains('\\') || pattern.StartsWith('/') || IsDrivePath(pattern))
        {
            error = "Pattern must be a repository-relative path using '/'.";
            return false;
        }

        foreach (var segment in pattern.Split('/', StringSplitOptions.None))
        {
            if (segment.Length == 0 || segment is "." or "..")
            {
                error = "Pattern contains an empty, dot, or parent segment.";
                return false;
            }

            if (segment.Contains("**", StringComparison.Ordinal) && segment != "**")
            {
                error = "Recursive wildcard '**' must occupy an entire segment.";
                return false;
            }
        }

        return true;
    }

    public static bool IsMatch(string pattern, string repositoryPath)
    {
        if (!IsValidPattern(pattern, out _))
        {
            return false;
        }

        var normalizedPath = repositoryPath.Replace('\\', '/').Trim('/');
        if (normalizedPath.Length == 0 || normalizedPath.Split('/').Any(static segment => segment is "." or ".."))
        {
            return false;
        }

        var patternSegments = pattern.Split('/');
        var pathSegments = normalizedPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var memo = new Dictionary<(int Pattern, int Path), bool>();
        return Match(0, 0);

        bool Match(int patternIndex, int pathIndex)
        {
            if (memo.TryGetValue((patternIndex, pathIndex), out var cached))
            {
                return cached;
            }

            bool result;
            if (patternIndex == patternSegments.Length)
            {
                result = pathIndex == pathSegments.Length;
            }
            else if (patternSegments[patternIndex] == "**")
            {
                result = Match(patternIndex + 1, pathIndex) ||
                         pathIndex < pathSegments.Length && Match(patternIndex, pathIndex + 1);
            }
            else
            {
                result = pathIndex < pathSegments.Length &&
                         MatchSegment(patternSegments[patternIndex], pathSegments[pathIndex]) &&
                         Match(patternIndex + 1, pathIndex + 1);
            }

            memo[(patternIndex, pathIndex)] = result;
            return result;
        }
    }

    private static bool MatchSegment(string pattern, string value)
    {
        var memo = new Dictionary<(int Pattern, int Value), bool>();
        return Match(0, 0);

        bool Match(int patternIndex, int valueIndex)
        {
            if (memo.TryGetValue((patternIndex, valueIndex), out var cached))
            {
                return cached;
            }

            bool result;
            if (patternIndex == pattern.Length)
            {
                result = valueIndex == value.Length;
            }
            else if (pattern[patternIndex] == '*')
            {
                result = Match(patternIndex + 1, valueIndex) ||
                         valueIndex < value.Length && Match(patternIndex, valueIndex + 1);
            }
            else
            {
                result = valueIndex < value.Length &&
                         (pattern[patternIndex] == '?' || pattern[patternIndex] == value[valueIndex]) &&
                         Match(patternIndex + 1, valueIndex + 1);
            }

            memo[(patternIndex, valueIndex)] = result;
            return result;
        }
    }

    private static bool IsDrivePath(string path) =>
        path.Length >= 2 && char.IsAsciiLetter(path[0]) && path[1] == ':';
}
