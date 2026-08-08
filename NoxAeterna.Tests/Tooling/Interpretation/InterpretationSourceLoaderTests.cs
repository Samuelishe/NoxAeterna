using NoxAeterna.Tools.Repository.Interpretation.Analysis;
using NoxAeterna.Interpretation.Tarot.Contracts;

namespace NoxAeterna.Tests.Tooling.Interpretation;

public sealed class InterpretationSourceLoaderTests
{
    [Fact]
    public void AllFalseSkeletonReportsValidExistingAndCanonicalMissingInventories()
    {
        using var fixture = InterpretationToolingFixture.CreateSkeleton();

        var result = new InterpretationSourceLoader().Load(fixture.Root);

        Assert.True(result.Report.Success, Format(result));
        Assert.NotNull(result.Compilation);
        Assert.Equal(2, result.Report.Counts["validBundles"]);
        Assert.Equal(0, result.Report.Counts["invalidBundles"]);
        Assert.Equal(156, result.Report.Counts["missingSingleCardBundles"]);
        Assert.Equal(6006, result.Report.Counts["missingOrientedPairBundles"]);
        Assert.Equal(156, result.Report.Counts["missingThreeCardPositionBundles"]);
    }

    [Fact]
    public void ExistingCompleteBundleIsKeptAndSecondValidationNeedsNoRegeneration()
    {
        using var fixture = InterpretationToolingFixture.CreateSkeleton();
        fixture.AddSingle("ru", "major.fool");

        var first = new InterpretationSourceLoader().Load(fixture.Root);
        var second = new InterpretationSourceLoader().Load(fixture.Root);

        Assert.True(first.Report.Success, Format(first));
        Assert.Equal(first.Report.Counts, second.Report.Counts);
        Assert.Equal(first.Compilation!.SourceDigest, second.Compilation!.SourceDigest);
        Assert.Equal(1, first.Report.Counts["singleCardBundles"]);
        Assert.Equal(2, first.Report.Counts["singleCardStates"]);
        Assert.Equal(155, first.Report.Counts["missingSingleCardBundles"]);
    }

    [Fact]
    public void CanonicalBytePathIdentityAndUnknownTaxonomyFailuresAreReportedWithoutOverwrite()
    {
        using var fixture = InterpretationToolingFixture.CreateSkeleton();
        fixture.AddSingle("ru", "major.fool", "major.magician");
        var mismatched = Path.Combine(fixture.Root, "content", "ru", "single-card", "major.magician.json");
        var original = File.ReadAllBytes(mismatched);
        File.WriteAllBytes(fixture.ManifestPath, ReplaceLfWithCrlf(File.ReadAllBytes(fixture.ManifestPath)));
        fixture.Write("content/ru/unexpected/item.json", new { schemaVersion = 1 });

        var result = new InterpretationSourceLoader().Load(fixture.Root);

        Assert.False(result.Report.Success);
        Assert.Contains(result.Report.Diagnostics, item => item.Code == "source.canonical-bytes");
        Assert.Contains(result.Report.Diagnostics, item => item.Code == "source.identity-path");
        Assert.Contains(result.Report.Diagnostics, item => item.Code == "source.taxonomy");
        Assert.Equal(original, File.ReadAllBytes(mismatched));
        Assert.True(result.Report.Counts["invalidBundles"] >= 2);
        Assert.True(result.Report.Counts["noncanonicalIdentities"] >= 1);
    }

    [Fact]
    public void FilenameCollisionReportsDuplicateAndNoncanonicalSemanticIdentity()
    {
        using var fixture = InterpretationToolingFixture.CreateSkeleton();
        fixture.AddSingle("ru", "major.fool");
        fixture.AddSingle("ru", "major.fool", "major.magician");

        var result = new InterpretationSourceLoader().Load(fixture.Root);

        Assert.False(result.Report.Success);
        Assert.Equal(1, result.Report.Counts["duplicateIdentities"]);
        Assert.Equal(1, result.Report.Counts["noncanonicalIdentities"]);
    }

    [Theory]
    [InlineData("pair")]
    [InlineData("position")]
    public void EveryBundledCorpusEnforcesFilenameIdentity(string corpus)
    {
        using var fixture = InterpretationToolingFixture.CreateSkeleton();
        if (corpus == "pair") fixture.AddPair("ru", "major.fool", "major.magician", "major.fool__major.high-priestess");
        else fixture.AddPositions("ru", "major.fool", "major.magician");

        var result = new InterpretationSourceLoader().Load(fixture.Root);

        Assert.False(result.Report.Success);
        Assert.Equal(1, result.Report.Counts["noncanonicalIdentities"]);
        Assert.Contains(result.Report.Diagnostics, item => item.Code == "source.identity-path");
    }

    [Fact]
    public void SynthesisSourceRejectsIdentityOutsideFrozenInventory()
    {
        using var fixture = InterpretationToolingFixture.CreateSkeleton();
        fixture.AddSynthesis("ru", TarotSynthesisResourceType.TrajectoryProfile, "trajectory-profile", "unknown-profile");

        var result = new InterpretationSourceLoader().Load(fixture.Root);

        Assert.False(result.Report.Success);
        Assert.Contains(result.Report.Diagnostics, item => item.Code == "schema.synthesis.inventory");
        Assert.Equal(1, result.Report.Counts["invalidBundles"]);
    }

    [Fact]
    public void SynthesisSourceRejectsUnknownPayloadMembers()
    {
        using var fixture = InterpretationToolingFixture.CreateSkeleton();
        fixture.Write(
            $"content/ru/synthesis/trajectory-profile/{TarotThreeCardSynthesisContract.Improving}.json",
            new
            {
                schemaVersion = 1,
                resourceType = "trajectory-profile",
                resourceId = TarotThreeCardSynthesisContract.Improving,
                data = new { text = "Valid text", rule = "not-owned-by-source" }
            });

        var result = new InterpretationSourceLoader().Load(fixture.Root);

        Assert.False(result.Report.Success);
        Assert.Contains(result.Report.Diagnostics, item => item.Code == "schema.synthesis.payload");
        Assert.Equal(1, result.Report.Counts["invalidBundles"]);
    }

    [Fact]
    public void ReadyThreeCardsSourceRequiresEveryFrozenSynthesisIdentity()
    {
        using var fixture = InterpretationToolingFixture.CreateSkeleton();
        fixture.AddCompleteThreeCardDependencies("ru");
        foreach (var identity in TarotThreeCardSynthesisContract.RequiredResources.SkipLast(1))
        {
            var type = NoxAeterna.Interpretation.Tarot.Serialization.TarotSchemaText.Get(
                identity.ResourceType,
                NoxAeterna.Interpretation.Tarot.Serialization.TarotSchemaText.SynthesisResourceTypes);
            fixture.AddSynthesis("ru", identity.ResourceType, type, identity.ResourceId.Value);
        }
        fixture.SetReady("ru", "three-cards", true);

        var result = new InterpretationSourceLoader().Load(fixture.Root);

        Assert.False(result.Report.Success);
        Assert.Contains(result.Report.Diagnostics, item => item.Code == "ready.synthesis");
        Assert.Equal(12_012, result.Report.Counts["orientedPairStates"]);
        Assert.Equal(468, result.Report.Counts["threeCardPositionStates"]);
    }

    private static byte[] ReplaceLfWithCrlf(byte[] bytes) => bytes[..^1].Concat("\r\n"u8.ToArray()).ToArray();
    private static string Format(InterpretationSourceCompilationResult result) =>
        string.Join(Environment.NewLine, result.Report.Diagnostics.Select(item => $"{item.Code}: {item.Message}"));
}
