using NoxAeterna.Domain.Tarot;
using NoxAeterna.Presentation.Tarot;

namespace NoxAeterna.Tests.Interpretation.Tarot;

public sealed class TarotInterpretationPackIdentityTests
{
    [Fact]
    public void PackId_PreservesCanonicalValueEqualityAndText()
    {
        var first = new TarotInterpretationPackId("classic");
        var second = new TarotInterpretationPackId("classic");

        Assert.Equal("classic", first.Value);
        Assert.Equal("classic", first.ToString());
        Assert.Equal(first, second);
        Assert.NotEqual(first, new TarotInterpretationPackId("psychological"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("CLASSIC")]
    [InlineData("classic pack")]
    [InlineData("classic..pack")]
    [InlineData("classic-")]
    public void PackId_RejectsInvalidStableValues(string value)
    {
        Assert.Throws<ArgumentException>(() => new TarotInterpretationPackId(value));
    }

    [Fact]
    public void CurrentWorkspace_UsesIndependentClassicPackIdentity()
    {
        Assert.Equal("classic", TarotPrototypeSelections.InterpretationPackId.Value);
        Assert.NotEqual<object>(TarotPrototypeSelections.InterpretationPackId, StandardTarotCatalog.Deck.Id);
        Assert.NotEqual<object>(TarotPrototypeSelections.InterpretationPackId, TarotPrototypeSelections.DefaultArtworkPackId);
    }

    [Fact]
    public void ProductionAndTestSources_ContainNoPredecessorIdentityNamesOrValue()
    {
        var root = RepositoryRoot();
        var source = Directory.GetDirectories(root, "NoxAeterna.*", SearchOption.TopDirectoryOnly)
            .SelectMany(directory => Directory.GetFiles(directory, "*.cs", SearchOption.AllDirectories))
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal) &&
                           !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal) &&
                           !path.EndsWith(nameof(TarotInterpretationPackIdentityTests) + ".cs", StringComparison.Ordinal))
            .Select(File.ReadAllText)
            .ToArray();
        var formerType = string.Concat("TarotInterpretation", "SetId");
        var formerProperty = string.Concat("Interpretation", "SetId");
        var formerParameter = string.Concat("interpretation", "SetId");
        var formerValue = string.Concat("found", "ation");

        Assert.All(source, text =>
        {
            Assert.DoesNotContain(formerType, text, StringComparison.Ordinal);
            Assert.DoesNotContain(formerProperty, text, StringComparison.Ordinal);
            Assert.DoesNotContain(formerParameter, text, StringComparison.Ordinal);
        });
        Assert.DoesNotContain(source, text =>
            text.Contains($"new(\"{formerValue}\")", StringComparison.Ordinal) ||
            text.Contains($"new (\"{formerValue}\")", StringComparison.Ordinal));
    }

    private static string RepositoryRoot() => Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
}
