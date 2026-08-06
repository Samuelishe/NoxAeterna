using System.Xml.Linq;

namespace NoxAeterna.Tests.Tooling.Interpretation;

public sealed class InterpretationToolingBoundaryTests
{
    [Fact]
    public void ToolsRepository_ReferencesOnlyPureInterpretationWithoutPackagesOrRuntimeLayers()
    {
        var project = XDocument.Load(PathAt("NoxAeterna.Tools.Repository", "NoxAeterna.Tools.Repository.csproj"));
        var references = project.Descendants("ProjectReference")
            .Select(ProjectReferenceName)
            .ToArray();
        var packages = project.Descendants("PackageReference").ToArray();

        Assert.Equal(new[] { "NoxAeterna.Interpretation" }, references);
        Assert.Empty(packages);
        Assert.DoesNotContain(references, name => name is "NoxAeterna.App" or "NoxAeterna.Presentation" or
            "NoxAeterna.Infrastructure" or "Avalonia");
    }

    [Fact]
    public void InterpretationAndProductionProjects_DoNotReferenceRepositoryTooling()
    {
        var projects = Directory.GetFiles(Root, "*.csproj", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}NoxAeterna.Tests{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .ToArray();

        foreach (var path in projects.Where(path => Path.GetFileNameWithoutExtension(path) != "NoxAeterna.Tools.Repository"))
        {
            var references = XDocument.Load(path).Descendants("ProjectReference")
                .Select(item => (string?)item.Attribute("Include"))
                .Where(value => value is not null)
                .Cast<string>();
            Assert.DoesNotContain(references, value => value.Contains("NoxAeterna.Tools.Repository", StringComparison.Ordinal));
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
