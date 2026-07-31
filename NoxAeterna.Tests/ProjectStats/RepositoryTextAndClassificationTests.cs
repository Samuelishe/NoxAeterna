using System.Text;
using NoxAeterna.Tools.Repository.Analysis;

namespace NoxAeterna.Tests.ProjectStats;

public sealed class RepositoryTextAndClassificationTests
{
    [Theory]
    [InlineData("one\ntwo\n", 2, 8)]
    [InlineData("one\r\ntwo\r\n", 2, 10)]
    [InlineData("one", 1, 3)]
    [InlineData("", 0, 0)]
    public void TextMetricsUseLogicalLinesAndDotNetStringLength(string content, int lines, int characters)
    {
        var result = RepositoryTextMetrics.Measure(Encoding.UTF8.GetBytes(content));

        Assert.True(result.IsText);
        Assert.Equal(lines, result.Lines);
        Assert.Equal(characters, result.Characters);
    }

    [Fact]
    public void UnicodeCharacterCountUsesUtf16CodeUnits()
    {
        var result = RepositoryTextMetrics.Measure(Encoding.UTF8.GetBytes("☽😀"));

        Assert.Equal(3, result.Characters);
        Assert.Equal(1, result.Lines);
    }

    [Fact]
    public void NullByteMarksBinaryContent()
    {
        var result = RepositoryTextMetrics.Measure([65, 0, 66]);

        Assert.False(result.IsText);
        Assert.Null(result.Lines);
        Assert.Null(result.Characters);
    }

    [Theory]
    [InlineData("NoxAeterna.Domain/Value.cs", RepositoryFileCategory.Production)]
    [InlineData("NoxAeterna.Tests/ValueTests.cs", RepositoryFileCategory.Tests)]
    [InlineData("NoxAeterna.Tools.Repository/Program.cs", RepositoryFileCategory.Tooling)]
    [InlineData("eng/doc-check.ps1", RepositoryFileCategory.Tooling)]
    [InlineData("docs/PROJECT-STATS.md", RepositoryFileCategory.Documentation)]
    [InlineData("resources/localization/ui/en.json", RepositoryFileCategory.Resources)]
    [InlineData(".github/workflows/ci.yml", RepositoryFileCategory.Workflow)]
    [InlineData("Directory.Build.props", RepositoryFileCategory.Other)]
    public void ClassificationIsPathBasedAndDeterministic(string path, RepositoryFileCategory expected)
    {
        Assert.Equal(expected, RepositoryPathPolicy.Classify(path));
    }
}
