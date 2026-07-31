using System.Diagnostics;

namespace NoxAeterna.Tools.Repository.Analysis;

public interface IRepositoryFileReader
{
    long GetLength(string path);

    byte[] ReadAllBytes(string path);
}

public sealed class PhysicalRepositoryFileReader : IRepositoryFileReader
{
    public long GetLength(string path) => new FileInfo(path).Length;

    public byte[] ReadAllBytes(string path) => File.ReadAllBytes(path);
}

public sealed class GitRepositoryInventory(IRepositoryFileReader? fileReader = null)
{
    private readonly IRepositoryFileReader _fileReader = fileReader ?? new PhysicalRepositoryFileReader();

    public static string ResolveRoot(string? requestedRoot)
    {
        var candidate = Path.GetFullPath(string.IsNullOrWhiteSpace(requestedRoot)
            ? Environment.CurrentDirectory
            : requestedRoot);
        if (!Directory.Exists(candidate))
        {
            throw new ArgumentException($"Repository root does not exist: {requestedRoot}");
        }

        var result = RunGit(candidate, ["rev-parse", "--show-toplevel"]);
        if (result.ExitCode != 0 || string.IsNullOrWhiteSpace(result.Output))
        {
            throw new ArgumentException("The requested root is not inside a readable Git repository.");
        }

        return Path.GetFullPath(result.Output.Trim());
    }

    public RepositoryInventory Discover(string repositoryRoot, string? outputRelativePath = null)
    {
        var diagnostics = new List<RepositoryDiagnostic>();
        var candidates = new Dictionary<string, bool>(StringComparer.Ordinal);
        if (!ReadInventory(repositoryRoot, ["ls-files", "--cached", "-z"], true, candidates, diagnostics) ||
            !ReadInventory(repositoryRoot, ["ls-files", "--others", "--exclude-standard", "-z"], false, candidates, diagnostics))
        {
            return new RepositoryInventory([], OrderDiagnostics(diagnostics));
        }

        var files = new List<RepositoryFileEntry>();
        foreach (var pair in candidates.OrderBy(static item => item.Key, StringComparer.Ordinal))
        {
            var path = RepositoryPathPolicy.Normalize(pair.Key);
            if (!string.IsNullOrWhiteSpace(outputRelativePath) &&
                path.Equals(RepositoryPathPolicy.Normalize(outputRelativePath), StringComparison.OrdinalIgnoreCase))
            {
                diagnostics.Add(new RepositoryDiagnostic(
                    "output-excluded", "info", path, "The requested output target was excluded from inventory."));
                continue;
            }

            if (RepositoryPathPolicy.IsPrivateOrSensitive(path))
            {
                diagnostics.Add(new RepositoryDiagnostic(
                    "privacy-excluded", "warning", null,
                    "A private or sensitive inventory entry was excluded without reading its contents."));
                continue;
            }

            if (RepositoryPathPolicy.IsGenerated(path))
            {
                diagnostics.Add(new RepositoryDiagnostic(
                    "generated-excluded", "warning", path,
                    "A generated or runtime path was visible to Git inventory and excluded from rankings."));
                continue;
            }

            var absolutePath = Path.GetFullPath(Path.Combine(repositoryRoot, path.Replace('/', Path.DirectorySeparatorChar)));
            if (!IsInside(repositoryRoot, absolutePath) || !File.Exists(absolutePath))
            {
                diagnostics.Add(new RepositoryDiagnostic("file-missing", "warning", path, "Inventory file is unavailable."));
                continue;
            }

            try
            {
                var extension = Path.GetExtension(path).ToLowerInvariant();
                var bytes = _fileReader.GetLength(absolutePath);
                var isTextExtension = RepositoryPathPolicy.IsTextExtension(extension);
                var measurement = isTextExtension
                    ? RepositoryTextMetrics.Measure(_fileReader.ReadAllBytes(absolutePath))
                    : new RepositoryTextMeasurement(false, null, null);
                files.Add(new RepositoryFileEntry(
                    path,
                    extension,
                    bytes,
                    measurement.Lines,
                    measurement.Characters,
                    RepositoryPathPolicy.Classify(path),
                    RepositoryPathPolicy.GetProjectArea(path),
                    pair.Value,
                    measurement.IsText,
                    path.StartsWith("docs/archive/", StringComparison.OrdinalIgnoreCase)));
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                diagnostics.Add(new RepositoryDiagnostic("file-unreadable", "warning", path, "Public inventory file could not be read."));
            }
        }

        return new RepositoryInventory(files, OrderDiagnostics(diagnostics));
    }

    private static bool ReadInventory(
        string root,
        IReadOnlyList<string> arguments,
        bool tracked,
        IDictionary<string, bool> candidates,
        ICollection<RepositoryDiagnostic> diagnostics)
    {
        var result = RunGit(root, arguments);
        if (result.ExitCode != 0)
        {
            diagnostics.Add(new RepositoryDiagnostic(
                "git-inventory-failed", "error", null,
                "Git public-file inventory could not be read; no filesystem fallback was attempted."));
            return false;
        }

        foreach (var path in result.Output.Split('\0', StringSplitOptions.RemoveEmptyEntries))
        {
            var normalized = RepositoryPathPolicy.Normalize(path);
            if (!candidates.TryGetValue(normalized, out var existing) || tracked && !existing)
            {
                candidates[normalized] = tracked;
            }
        }

        return true;
    }

    private static (int ExitCode, string Output) RunGit(string root, IReadOnlyList<string> arguments)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "git",
            WorkingDirectory = root,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        startInfo.ArgumentList.Add("-C");
        startInfo.ArgumentList.Add(root);
        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        Process? started;
        try
        {
            started = Process.Start(startInfo);
        }
        catch (System.ComponentModel.Win32Exception)
        {
            return (-1, string.Empty);
        }

        using var process = started;
        if (process is null)
        {
            return (-1, string.Empty);
        }

        var output = process.StandardOutput.ReadToEnd();
        process.StandardError.ReadToEnd();
        process.WaitForExit();
        return (process.ExitCode, output);
    }

    private static bool IsInside(string root, string path)
    {
        var relative = Path.GetRelativePath(root, path);
        return relative != ".." &&
               !relative.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal) &&
               !Path.IsPathRooted(relative);
    }

    private static IReadOnlyList<RepositoryDiagnostic> OrderDiagnostics(IEnumerable<RepositoryDiagnostic> diagnostics) =>
        diagnostics
            .OrderBy(static diagnostic => diagnostic.Code, StringComparer.Ordinal)
            .ThenBy(static diagnostic => diagnostic.Path, StringComparer.Ordinal)
            .ThenBy(static diagnostic => diagnostic.Message, StringComparer.Ordinal)
            .ToArray();
}
