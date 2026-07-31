using NoxAeterna.Tools.Repository.Analysis;

namespace NoxAeterna.Tests.ProjectStats;

public sealed class MsBuildProjectPathResolverTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void EmptyIncludeIsRejected(string? include)
    {
        var result = MsBuildProjectPathResolver.Resolve("A/A.csproj", include);

        Assert.False(result.Succeeded);
        Assert.Null(result.Path);
        Assert.Equal(MsBuildProjectPathFailure.EmptyInclude, result.Failure);
    }

    [Theory]
    [InlineData(@"..\B\B.csproj")]
    [InlineData(@"..\\B\\B.csproj")]
    [InlineData("../B/B.csproj")]
    [InlineData(@"..\B/Sub\B.csproj", "B/Sub/B.csproj")]
    public void SlashStylesResolveToCanonicalRepositoryPath(string include, string expected = "B/B.csproj")
    {
        var result = MsBuildProjectPathResolver.Resolve("A/A.csproj", include);

        Assert.True(result.Succeeded);
        Assert.Equal(expected, result.Path);
        AssertCanonical(result.Path!);
    }

    [Fact]
    public void DotSegmentsAreRemovedLexically()
    {
        var result = MsBuildProjectPathResolver.Resolve("A/A.csproj", "./Sub/../B.csproj");

        Assert.True(result.Succeeded);
        Assert.Equal("A/B.csproj", result.Path);
        AssertCanonical(result.Path!);
    }

    [Fact]
    public void MultipleParentsCollapseWithoutUsingHostFilesystemSemantics()
    {
        var result = MsBuildProjectPathResolver.Resolve("A/Sub/Deep/A.csproj", @"..\..\..\B\B.csproj");

        Assert.True(result.Succeeded);
        Assert.Equal("B/B.csproj", result.Path);
        AssertCanonical(result.Path!);
    }

    [Fact]
    public void RootLevelProjectCanReferenceChildProject()
    {
        var result = MsBuildProjectPathResolver.Resolve("Root.csproj", @"Child\Child.csproj");

        Assert.True(result.Succeeded);
        Assert.Equal("Child/Child.csproj", result.Path);
        AssertCanonical(result.Path!);
    }

    [Fact]
    public void RepositoryEscapeIsRejectedWithoutReturningAnAbsolutePath()
    {
        var result = MsBuildProjectPathResolver.Resolve("A/A.csproj", "../../../Outside/Outside.csproj");

        Assert.False(result.Succeeded);
        Assert.Null(result.Path);
        Assert.Equal(MsBuildProjectPathFailure.RepositoryEscape, result.Failure);
    }

    [Theory]
    [InlineData("/Outside/Outside.csproj")]
    [InlineData(@"C:\Outside\Outside.csproj")]
    [InlineData(@"\\server\share\Outside.csproj")]
    public void AbsoluteReferencesAreRejected(string include)
    {
        var result = MsBuildProjectPathResolver.Resolve("A/A.csproj", include);

        Assert.False(result.Succeeded);
        Assert.Null(result.Path);
        Assert.Equal(MsBuildProjectPathFailure.AbsolutePath, result.Failure);
    }

    [Fact]
    public void UnsupportedMsBuildExpressionIsRejectedWithoutGuessing()
    {
        var result = MsBuildProjectPathResolver.Resolve("A/A.csproj", @"$(ProjectRoot)\B\B.csproj");

        Assert.False(result.Succeeded);
        Assert.Null(result.Path);
        Assert.Equal(MsBuildProjectPathFailure.UnsupportedExpression, result.Failure);
    }

    [Fact]
    public void RepeatedResolutionIsDeterministic()
    {
        var first = MsBuildProjectPathResolver.Resolve("A/Sub/A.csproj", @"..\B/./B.csproj");
        var second = MsBuildProjectPathResolver.Resolve("A/Sub/A.csproj", @"..\B/./B.csproj");

        Assert.Equal(first, second);
        Assert.True(first.Succeeded);
        AssertCanonical(first.Path!);
    }

    private static void AssertCanonical(string path)
    {
        Assert.DoesNotContain('\\', path);
        Assert.False(Path.IsPathRooted(path));
        Assert.DoesNotContain("/./", $"/{path}/", StringComparison.Ordinal);
        Assert.DoesNotContain("/../", $"/{path}/", StringComparison.Ordinal);
    }
}
