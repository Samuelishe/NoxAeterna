using System.Diagnostics;
using NoxAeterna.Tools.Repository.Analysis;

namespace NoxAeterna.Tests.ProjectStats;

internal sealed class ProjectStatsTestFixture : IDisposable
{
    private ProjectStatsTestFixture(string root)
    {
        Root = root;
    }

    public string Root { get; }

    public static string RepositoryRoot { get; } = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));

    public static ProjectStatsTestFixture Create()
    {
        var root = Path.Combine(Path.GetTempPath(), $"NoxAeterna-project-stats-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        var fixture = new ProjectStatsTestFixture(root);
        fixture.RunGit("init", "--quiet");
        fixture.Write(
            ".gitignore",
            "bin/\nobj/\nTestResults/\ndocs/private/\ndocs/sensitive/\nproject-stats.md\nproject-stats.json\n",
            tracked: true);
        return fixture;
    }

    public string Write(string relativePath, string content, bool tracked = false, bool force = false)
    {
        var path = Path.Combine(Root, relativePath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content);
        if (tracked)
        {
            if (force)
            {
                RunGit("add", "--force", "--", relativePath);
            }
            else
            {
                RunGit("add", "--", relativePath);
            }
        }
        return path;
    }

    public void WriteBytes(string relativePath, byte[] content, bool tracked = false)
    {
        var path = Path.Combine(Root, relativePath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllBytes(path, content);
        if (tracked)
        {
            RunGit("add", "--", relativePath);
        }
    }

    public void AddMinimalReportRepository()
    {
        Write(
            "NoxAeterna.Domain/NoxAeterna.Domain.csproj",
            "<Project Sdk=\"Microsoft.NET.Sdk\"><PropertyGroup><TargetFramework>net10.0</TargetFramework></PropertyGroup></Project>",
            tracked: true);
        Write("NoxAeterna.Domain/Value.cs", "namespace Fixture;\npublic sealed class Value;\n", tracked: true);
        Write(
            "NoxAeterna.Tests/NoxAeterna.Tests.csproj",
            "<Project Sdk=\"Microsoft.NET.Sdk\"><PropertyGroup><TargetFramework>net10.0</TargetFramework></PropertyGroup><ItemGroup><ProjectReference Include=\"../NoxAeterna.Domain/NoxAeterna.Domain.csproj\" /></ItemGroup></Project>",
            tracked: true);
        Write(
            "NoxAeterna.Tests/Domain/ValueTests.cs",
            "namespace Fixture;\npublic sealed class ValueTests\n{\n    [Fact]\n    public void Works() { }\n    [Theory]\n    public void Varies() { }\n}\n",
            tracked: true);
        Write("AGENTS.md", "# Agents\n", tracked: true);
        Write(
            "eng/document-budgets.json",
            "{\"schemaVersion\":1,\"warningRatio\":0.5,\"documents\":[{\"path\":\"AGENTS.md\",\"hardLimit\":100,\"overflowStrategy\":\"manual-reconcile\"}]}",
            tracked: true);
    }

    public (int ExitCode, string Output, string Error) RunTool(params string[] arguments)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            WorkingDirectory = Root,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        startInfo.ArgumentList.Add(typeof(GitRepositoryInventory).Assembly.Location);
        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using var process = Process.Start(startInfo);
        Assert.NotNull(process);
        var output = process.StandardOutput.ReadToEndAsync();
        var error = process.StandardError.ReadToEndAsync();
        Assert.True(process.WaitForExit(20_000), "Project Stats CLI timed out.");
        return (process.ExitCode, output.GetAwaiter().GetResult(), error.GetAwaiter().GetResult());
    }

    public void Dispose()
    {
        var expectedPrefix = Path.Combine(Path.GetTempPath(), "NoxAeterna-project-stats-");
        if (Root.StartsWith(expectedPrefix, StringComparison.OrdinalIgnoreCase) && Directory.Exists(Root))
        {
            foreach (var file in Directory.EnumerateFiles(Root, "*", SearchOption.AllDirectories))
            {
                File.SetAttributes(file, FileAttributes.Normal);
            }
            Directory.Delete(Root, recursive: true);
        }
    }

    private void RunGit(params string[] arguments)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "git",
            WorkingDirectory = Root,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        startInfo.ArgumentList.Add("-c");
        startInfo.ArgumentList.Add("user.name=NoxAeterna Test");
        startInfo.ArgumentList.Add("-c");
        startInfo.ArgumentList.Add("user.email=nox-aeterna@example.invalid");
        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using var process = Process.Start(startInfo);
        Assert.NotNull(process);
        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();
        Assert.True(process.WaitForExit(10_000), "Fixture Git command timed out.");
        Assert.True(process.ExitCode == 0, $"{error}{Environment.NewLine}{output}");
    }
}

internal sealed class RecordingFileReader : IRepositoryFileReader
{
    private readonly PhysicalRepositoryFileReader _inner = new();
    private readonly string? _throwForSuffix;

    public RecordingFileReader(string? throwForSuffix = null)
    {
        _throwForSuffix = throwForSuffix;
    }

    public List<string> Reads { get; } = [];

    public long GetLength(string path)
    {
        Reads.Add(path);
        ThrowIfRequested(path);
        return _inner.GetLength(path);
    }

    public byte[] ReadAllBytes(string path)
    {
        Reads.Add(path);
        ThrowIfRequested(path);
        return _inner.ReadAllBytes(path);
    }

    private void ThrowIfRequested(string path)
    {
        if (_throwForSuffix is not null && path.EndsWith(_throwForSuffix, StringComparison.OrdinalIgnoreCase))
        {
            throw new IOException("Synthetic unreadable file.");
        }
    }
}
