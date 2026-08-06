using NoxAeterna.Domain.Tarot;
using NoxAeterna.Interpretation.Tarot.Contracts;
using NoxAeterna.Interpretation.Tarot.Sources;

namespace NoxAeterna.App.Tarot;

/// <summary>Reads the immutable built-in Classic package rooted under the application output.</summary>
public sealed class BuiltInClassicInterpretationPackSource : ITarotInterpretationPackSource
{
    public const string PackageOutputPath = "resources/interpretation/tarot/packs/classic";
    private static readonly TarotPackageRelativePath ManifestPath = new("interpretation-pack.json");
    private readonly string packRoot;

    public BuiltInClassicInterpretationPackSource()
        : this(AppContext.BaseDirectory)
    {
    }

    public BuiltInClassicInterpretationPackSource(string applicationBaseDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(applicationBaseDirectory);
        var baseDirectory = Path.GetFullPath(applicationBaseDirectory);
        packRoot = Path.GetFullPath(Path.Combine(
            baseDirectory,
            PackageOutputPath.Replace('/', Path.DirectorySeparatorChar)));
    }

    public TarotInterpretationPackId PackId { get; } = new("classic");

    public string SnapshotId => "built-in-classic-v1";

    public string PackRoot => packRoot;

    public TarotInterpretationSourceReadResult ReadManifest() => Read(ManifestPath);

    public TarotInterpretationSourceReadResult ReadPackageFile(TarotPackageRelativePath path)
    {
        ArgumentNullException.ThrowIfNull(path);
        return Read(path);
    }

    private TarotInterpretationSourceReadResult Read(TarotPackageRelativePath path)
    {
        string fullPath;
        try
        {
            fullPath = ResolveWithinPack(path);
        }
        catch (ArgumentException exception)
        {
            return TarotInterpretationSourceReadResult.Failed("source.path", exception.Message);
        }

        try
        {
            using var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var buffer = new MemoryStream();
            stream.CopyTo(buffer);
            return TarotInterpretationSourceReadResult.Found(buffer.ToArray());
        }
        catch (Exception exception) when (exception is FileNotFoundException or DirectoryNotFoundException)
        {
            return TarotInterpretationSourceReadResult.Missing();
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return TarotInterpretationSourceReadResult.Failed(
                "source.io",
                "The built-in interpretation package file could not be read.");
        }
    }

    private string ResolveWithinPack(TarotPackageRelativePath path)
    {
        var candidate = Path.GetFullPath(Path.Combine(
            packRoot,
            path.Value.Replace('/', Path.DirectorySeparatorChar)));
        var relative = Path.GetRelativePath(packRoot, candidate);
        if (Path.IsPathRooted(relative) || relative == ".." ||
            relative.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
        {
            throw new ArgumentException("Package path resolves outside the built-in Classic root.", nameof(path));
        }

        EnsureNoEscapingLink(relative, path);
        return candidate;
    }

    private void EnsureNoEscapingLink(string relative, TarotPackageRelativePath path)
    {
        var current = packRoot;
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

            var resolved = info.ResolveLinkTarget(returnFinalTarget: true)?.FullName;
            if (resolved is null || !IsContained(Path.GetFullPath(resolved)))
            {
                throw new ArgumentException("Package path crosses a link outside the built-in Classic root.", nameof(path));
            }
        }
    }

    private bool IsContained(string candidate)
    {
        var relative = Path.GetRelativePath(packRoot, candidate);
        return !Path.IsPathRooted(relative) && relative != ".." &&
               !relative.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal);
    }
}
