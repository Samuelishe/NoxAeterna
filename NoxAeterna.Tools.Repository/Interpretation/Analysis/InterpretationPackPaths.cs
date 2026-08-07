namespace NoxAeterna.Tools.Repository.Interpretation.Analysis;

public sealed class InterpretationPackPaths
{
    public InterpretationPackPaths(string root, bool mustExist)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(root);
        var full = Path.GetFullPath(root);
        if (mustExist && !Directory.Exists(full))
        {
            throw new DirectoryNotFoundException($"Interpretation pack root does not exist: {full}");
        }

        Root = Directory.Exists(full)
            ? Path.GetFullPath(new DirectoryInfo(full).ResolveLinkTarget(returnFinalTarget: true)?.FullName ?? full)
            : full;
    }

    public string Root { get; }

    public string Resolve(string relativePath)
    {
        ValidateRelativePath(relativePath);
        var candidate = Path.GetFullPath(Path.Combine(
            Root,
            relativePath.Replace('/', Path.DirectorySeparatorChar)));
        var relative = Path.GetRelativePath(Root, candidate);
        if (Path.IsPathRooted(relative) || relative == ".." ||
            relative.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
        {
            throw new ArgumentException("Package path resolves outside the supplied pack root.", nameof(relativePath));
        }

        EnsureNoEscapingLink(relative, relativePath);

        return candidate;
    }

    public string Relative(string absolutePath)
    {
        var full = Path.GetFullPath(absolutePath);
        var relative = Path.GetRelativePath(Root, full);
        if (Path.IsPathRooted(relative) || relative == ".." ||
            relative.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
        {
            throw new ArgumentException("Path is outside the supplied pack root.", nameof(absolutePath));
        }

        return relative.Replace('\\', '/');
    }

    private void EnsureNoEscapingLink(string relative, string argument)
    {
        var current = Root;
        foreach (var segment in relative.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries))
        {
            current = Path.Combine(current, segment);
            FileSystemInfo? info = Directory.Exists(current)
                ? new DirectoryInfo(current)
                : File.Exists(current) ? new FileInfo(current) : null;
            if (info?.LinkTarget is null)
            {
                continue;
            }

            var target = info.ResolveLinkTarget(returnFinalTarget: true)?.FullName;
            if (target is null || !IsContained(Path.GetFullPath(target)))
            {
                throw new ArgumentException("Package path crosses a symbolic link outside the supplied pack root.", argument);
            }
        }
    }

    private bool IsContained(string candidate)
    {
        var relative = Path.GetRelativePath(Root, candidate);
        return !Path.IsPathRooted(relative) && relative != ".." &&
               !relative.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal);
    }

    private static void ValidateRelativePath(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if(value.Length==0||value!=value.Trim()||value.Contains('\\')||value.StartsWith('/')||value.EndsWith('/')||value.Contains("//",StringComparison.Ordinal)||value.Split('/').Any(static segment=>segment is "" or "." or "..")||Uri.TryCreate(value,UriKind.Absolute,out _))
            throw new ArgumentException("A source path must be a safe relative '/' path.",nameof(value));
    }
}
