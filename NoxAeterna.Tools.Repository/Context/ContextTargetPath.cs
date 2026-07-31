using System.Diagnostics;
using NoxAeterna.Tools.Repository.Analysis;

namespace NoxAeterna.Tools.Repository.Context;

public static class ContextTargetPath
{
    public static bool TryCanonicalize(string input, out string path, out string? error)
    {
        path = string.Empty;
        error = null;
        if (string.IsNullOrWhiteSpace(input) || input.StartsWith('/') || input.StartsWith('\\') || Path.IsPathRooted(input) ||
            input.Length >= 2 && char.IsAsciiLetter(input[0]) && input[1] == ':')
        {
            error = "Target paths must be repository-relative.";
            return false;
        }

        var segments = new List<string>();
        foreach (var segment in input.Replace('\\', '/').Split('/', StringSplitOptions.RemoveEmptyEntries))
        {
            if (segment == ".")
            {
                continue;
            }
            if (segment == "..")
            {
                if (segments.Count == 0)
                {
                    error = "Target path escapes the repository.";
                    return false;
                }
                segments.RemoveAt(segments.Count - 1);
                continue;
            }
            segments.Add(segment);
        }

        if (segments.Count == 0)
        {
            error = "Target path must identify a repository path.";
            return false;
        }
        path = string.Join('/', segments);
        return true;
    }

    public static bool IsIgnored(string repositoryRoot, string path)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "git",
            WorkingDirectory = repositoryRoot,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        startInfo.ArgumentList.Add("-C");
        startInfo.ArgumentList.Add(repositoryRoot);
        startInfo.ArgumentList.Add("check-ignore");
        startInfo.ArgumentList.Add("--quiet");
        startInfo.ArgumentList.Add("--");
        startInfo.ArgumentList.Add(path);
        try
        {
            using var process = Process.Start(startInfo);
            if (process is null)
            {
                return false;
            }
            process.StandardOutput.ReadToEnd();
            process.StandardError.ReadToEnd();
            process.WaitForExit();
            return process.ExitCode == 0;
        }
        catch (System.ComponentModel.Win32Exception)
        {
            return false;
        }
    }

    public static bool IsForbidden(string path) =>
        RepositoryPathPolicy.IsPrivateOrSensitive(path) || RepositoryPathPolicy.IsGenerated(path);
}
