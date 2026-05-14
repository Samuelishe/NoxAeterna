using System.Reflection;
using System.Xml.Linq;
using Avalonia;
using Avalonia.Media;
using NoxAeterna.Rendering.Charts;

namespace NoxAeterna.Tests.Rendering;

public sealed class RenderingBoundaryTests
{
    [Fact]
    public void RenderingProject_ReferencesGeometryAndAvalonia()
    {
        var projectDocument = LoadProjectDocument("NoxAeterna.Rendering", "NoxAeterna.Rendering.csproj");

        var projectReferences = projectDocument
            .Descendants("ProjectReference")
            .Select(element => (string?)element.Attribute("Include"))
            .Where(static value => value is not null)
            .ToArray();

        var packageReferences = projectDocument
            .Descendants("PackageReference")
            .Select(element => (string?)element.Attribute("Include"))
            .Where(static value => value is not null)
            .ToArray();

        Assert.Contains(projectReferences, path => path!.Contains("NoxAeterna.Geometry", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(packageReferences, package => string.Equals(package, "Avalonia", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void RenderingProject_DoesNotReferenceAstronomyOrInfrastructureProjects()
    {
        var projectDocument = LoadProjectDocument("NoxAeterna.Rendering", "NoxAeterna.Rendering.csproj");

        var projectReferences = projectDocument
            .Descendants("ProjectReference")
            .Select(element => (string?)element.Attribute("Include"))
            .Where(static value => value is not null)
            .ToArray();

        Assert.DoesNotContain(projectReferences, path => path!.Contains("NoxAeterna.Astronomy", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(projectReferences, path => path!.Contains("NoxAeterna.Infrastructure", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Renderer_RenderMethod_ConsumesRenderSceneInsteadOfNatalChart()
    {
        var renderMethod = typeof(CircularChartRenderer).GetMethod(
            nameof(CircularChartRenderer.Render),
            BindingFlags.Public | BindingFlags.Instance);

        Assert.NotNull(renderMethod);

        var parameters = renderMethod!.GetParameters();

        Assert.Collection(
            parameters,
            parameter => Assert.Equal(typeof(DrawingContext), parameter.ParameterType),
            parameter => Assert.Equal(typeof(Rect), parameter.ParameterType),
            parameter => Assert.Equal(typeof(ChartRenderScene), parameter.ParameterType),
            parameter => Assert.Equal(typeof(ChartRenderOptions), parameter.ParameterType));
    }

    private static XDocument LoadProjectDocument(string projectDirectory, string projectFileName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", projectDirectory, projectFileName);
        return XDocument.Load(Path.GetFullPath(path));
    }
}
