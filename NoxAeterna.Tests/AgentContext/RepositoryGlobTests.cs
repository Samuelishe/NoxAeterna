using NoxAeterna.Tools.Repository.Context.Routing;

namespace NoxAeterna.Tests.AgentContext;

public sealed class RepositoryGlobTests
{
    [Theory]
    [InlineData("docs/INDEX.md", "docs/INDEX.md", true)]
    [InlineData("docs/*.md", "docs/INDEX.md", true)]
    [InlineData("docs/?.md", "docs/A.md", true)]
    [InlineData("docs/**", "docs", true)]
    [InlineData("docs/**", "docs/engines/A.md", true)]
    [InlineData("**/Charts/*.cs", "NoxAeterna.Rendering/Charts/A.cs", true)]
    [InlineData("docs/*.md", "docs/engines/A.md", false)]
    [InlineData("DOCS/**", "docs/A.md", false)]
    public void MatchesExactSegmentAndRecursiveContracts(string pattern, string path, bool expected) =>
        Assert.Equal(expected, RepositoryGlob.IsMatch(pattern, path));

    [Theory]
    [InlineData("docs\\**")]
    [InlineData("../docs/**")]
    [InlineData("docs/a**b")]
    [InlineData("docs//*.md")]
    public void MalformedPatternIsRejected(string pattern)
    {
        Assert.False(RepositoryGlob.IsValidPattern(pattern, out var error));
        Assert.False(string.IsNullOrWhiteSpace(error));
    }

    [Fact]
    public void RepeatedMatchIsDeterministic()
    {
        var results = Enumerable.Range(0, 10)
            .Select(_ => RepositoryGlob.IsMatch("NoxAeterna.*/**/*.cs", "NoxAeterna.Rendering/Charts/A.cs"))
            .Distinct().ToArray();
        Assert.Equal([true], results);
    }
}
