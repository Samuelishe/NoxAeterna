using System.Xml.Linq;

namespace NoxAeterna.Tests.Tooling.Interpretation;

public sealed class InterpretationToolingBoundaryTests
{
    [Fact]
    public void SqliteAdapterAndToolingRespectTheDependencyDirection()
    {
        var tooling = XDocument.Load(PathAt("NoxAeterna.Tools.Repository", "NoxAeterna.Tools.Repository.csproj"));
        var references = tooling.Descendants("ProjectReference")
            .Select(ProjectReferenceName)
            .ToArray();
        Assert.Equal(new[] { "NoxAeterna.Interpretation", "NoxAeterna.Interpretation.Sqlite" }, references);
        Assert.DoesNotContain(references, name => name is "NoxAeterna.App" or "NoxAeterna.Presentation" or
            "NoxAeterna.Infrastructure" or "Avalonia");

        var sqlite = XDocument.Load(PathAt("NoxAeterna.Interpretation.Sqlite", "NoxAeterna.Interpretation.Sqlite.csproj"));
        Assert.Equal(new[] { "NoxAeterna.Interpretation" }, sqlite.Descendants("ProjectReference").Select(ProjectReferenceName));
    }

    [Fact]
    public void ProductionProjectsDoNotTakeRuntimeDependencyOnRepositoryTooling()
    {
        var projects = Directory.GetFiles(Root, "*.csproj", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}NoxAeterna.Tests{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .ToArray();

        foreach (var path in projects.Where(path => Path.GetFileNameWithoutExtension(path) != "NoxAeterna.Tools.Repository"))
        {
            var references = XDocument.Load(path).Descendants("ProjectReference")
                .Where(item => ((string?)item.Attribute("Include"))?.Contains("NoxAeterna.Tools.Repository", StringComparison.Ordinal) == true)
                .ToArray();
            foreach (var reference in references)
            {
                Assert.Equal("NoxAeterna.App", Path.GetFileNameWithoutExtension(path));
                Assert.Equal("false", (string?)reference.Attribute("ReferenceOutputAssembly"));
                Assert.Equal("all", (string?)reference.Attribute("PrivateAssets"));
            }
        }
    }

    [Fact]
    public void ToolingSource_HasNoAppDataUiSettingsOrRuntimeResolverOwnership()
    {
        var source = string.Join(
            Environment.NewLine,
            Directory.GetFiles(PathAt("NoxAeterna.Tools.Repository", "Interpretation"), "*.cs", SearchOption.AllDirectories)
                .Order(StringComparer.Ordinal)
                .Select(File.ReadAllText));

        Assert.DoesNotContain("AppData", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Avalonia", source, StringComparison.Ordinal);
        Assert.DoesNotContain("NoxAeterna.Presentation", source, StringComparison.Ordinal);
        Assert.DoesNotContain("NoxAeterna.App", source, StringComparison.Ordinal);
        Assert.DoesNotContain("TarotInterpretationPackSelector", source, StringComparison.Ordinal);
        Assert.DoesNotContain("JsonUserPreferencesStore", source, StringComparison.Ordinal);
        Assert.DoesNotContain("ResolvedTarotInterpretation", source, StringComparison.Ordinal);
        Assert.DoesNotContain("NoTarotInterpretationContent", source, StringComparison.Ordinal);
    }

    private static string ProjectReferenceName(XElement item)
    {
        var include = (string?)item.Attribute("Include")
            ?? throw new InvalidOperationException("ProjectReference Include is missing.");
        return Path.GetFileNameWithoutExtension(include.Replace('\\', '/'));
    }

    private static string PathAt(params string[] segments) => Path.Combine(new[] { Root }.Concat(segments).ToArray());

    private static string Root { get; } = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
}
