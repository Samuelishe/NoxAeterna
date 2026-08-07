using System.Text.Json;
using System.Xml.Linq;
using NodaTime;
using NoxAeterna.App.Tarot;
using NoxAeterna.Domain.Tarot;
using NoxAeterna.Interpretation.Tarot.Contracts;
using NoxAeterna.Interpretation.Tarot.Resolution;
using NoxAeterna.Presentation.Localization;
using NoxAeterna.Presentation.Tarot;
using NoxAeterna.Tests.Tooling.Interpretation;
using NoxAeterna.Tools.Repository.Interpretation.Compilation;

namespace NoxAeterna.Tests.App;

public sealed class TarotInterpretationPackageIntegrationTests
{
    [Fact]
    public void PromotedCanonicalRussianSingleCardCorpusResolvesAndBuildsLocalizedPresentationWhileOtherModesStayUnready()
    {
        var sourceRoot = PathAt("resources", "interpretation", "tarot", "sources", "classic");
        using var output = BuiltInOutput.Create(sourceRoot);
        var stores = BuiltInTarotInterpretationPackStoreCatalog.Create(output.Root);
        var packId = new TarotInterpretationPackId("classic");
        var russian = new TarotInterpretationLocale("ru");
        var english = new TarotInterpretationLocale("en");

        Assert.Empty(stores.Diagnostics);
        Assert.True(stores.TryGetStore(packId, out var store));
        Assert.NotNull(store);
        Assert.True(store.Manifest.Modules[TarotInterpretationMode.SingleCard][russian].Ready);
        Assert.False(store.Manifest.Modules[TarotInterpretationMode.SingleCard][english].Ready);

        var resolver = new TarotInterpretationPackResolver(stores, StandardTarotCatalog.Deck);
        var labelSource = new TarotPackagePresentationLabelSource(stores);
        var presentationBuilder = new TarotSingleCardInterpretationPresentationBuilder();
        var card = StandardTarotCatalog.Deck.Cards.Single(static item => item.Id.Value == "major.fool");
        var expectedSectionLabels = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["situation"] = "Основная ситуация",
            ["development"] = "Развитие",
            ["risk"] = "Риск",
            ["outcome"] = "Возможный исход",
            ["advice"] = "Совет"
        };
        var expectedTagLabels = RussianVocabularyLabels(sourceRoot);

        foreach (var orientation in Enum.GetValues<TarotCardOrientation>())
        {
            var resolved = Assert.IsType<ResolvedTarotInterpretation<TarotSingleCardEntry>>(
                resolver.ResolveSingleCard(packId, russian, card.Id, orientation));
            Assert.Equal("ru", resolved.RequestedLocale.Value);
            Assert.Equal("ru", resolved.ResolvedLocale.Value);
            Assert.Equal(orientation, resolved.Content.Orientation);
            Assert.Equal(expectedSectionLabels.Keys.Order(StringComparer.Ordinal), resolved.Content.Sections.Keys.Order(StringComparer.Ordinal));
            Assert.All(resolved.Content.Sections.Values, static text => Assert.False(string.IsNullOrWhiteSpace(text)));
            Assert.NotEmpty(resolved.Content.Tags);

            var labels = labelSource.Resolve(packId, resolved.ContentVersion, resolved.ResolvedLocale);
            Assert.NotNull(labels);
            foreach (var expected in expectedSectionLabels)
            {
                Assert.Equal(expected.Value, labels.SectionLabels[expected.Key]);
            }
            Assert.Equal(expectedTagLabels.Count, labels.TagLabels.Count);
            foreach (var expected in expectedTagLabels)
            {
                Assert.Equal(expected.Value, labels.TagLabels[new(expected.Key)]);
            }

            var assignment = new TarotDrawnCard(
                StandardTarotSpreads.SingleCard.Positions.Single().Id,
                card,
                orientation);
            var reading = new TarotReading(
                StandardTarotCatalog.Deck.Id,
                StandardTarotSpreads.SingleCard.Id,
                Instant.FromUnixTimeTicks(17),
                [assignment]);
            var presentation = Assert.IsType<TarotSingleCardInterpretationPresentation>(
                presentationBuilder.Build(reading, resolved, labels));
            Assert.Equal(5, presentation.Sections.Count);
            Assert.All(presentation.Sections, static section =>
            {
                Assert.False(string.IsNullOrWhiteSpace(section.Label));
                Assert.False(string.IsNullOrWhiteSpace(section.Text));
            });
            Assert.NotEmpty(presentation.Tags);
            Assert.All(presentation.Tags, tag =>
            {
                Assert.Equal(expectedTagLabels[tag.ConceptId.Value], tag.Label);
                Assert.NotEqual(tag.ConceptId.Value, tag.Label);
            });
        }

        var englishFallback = Assert.IsType<ResolvedTarotInterpretation<TarotSingleCardEntry>>(
            resolver.ResolveSingleCard(packId, english, card.Id, TarotCardOrientation.Upright));
        Assert.Equal("en", englishFallback.RequestedLocale.Value);
        Assert.Equal("ru", englishFallback.ResolvedLocale.Value);

        foreach (var mode in new[]
                 {
                     TarotInterpretationMode.TwoCards,
                     TarotInterpretationMode.ThreeCards,
                     TarotInterpretationMode.CelticCross
                 })
        {
            var noContent = Assert.IsType<NoTarotInterpretationContent<TarotResolvedModuleSnapshot>>(
                resolver.ResolveMode(packId, mode, russian));
            Assert.Equal(TarotNoContentReason.NoReadyLocale, noContent.Reason);
        }
    }

    [Fact]
    public void BuiltInSkeletonRegistersSelectorNamesAndRemainsSilentlyNotReady()
    {
        using var fixture=InterpretationToolingFixture.CreateSkeleton();using var output=BuiltInOutput.Create(fixture);
        var stores=BuiltInTarotInterpretationPackStoreCatalog.Create(output.Root);var catalog=new TarotInterpretationPackCatalog(stores,stores.PackIds);

        Assert.Empty(stores.Diagnostics);Assert.Equal(new[]{"classic"},catalog.AvailablePackIds.Select(static id=>id.Value));
        Assert.Equal("Классика",catalog.ResolveDisplayName(new("classic"),new("ru")));Assert.Equal("Classic",catalog.ResolveDisplayName(new("classic"),new("en")));Assert.Equal("Classic",catalog.ResolveDisplayName(new("classic"),new("zh")));
        var result=Assert.IsType<NoTarotInterpretationContent<TarotSingleCardEntry>>(new TarotInterpretationPackResolver(stores,StandardTarotCatalog.Deck).ResolveSingleCard(new("classic"),new("ru"),new("major.fool"),TarotCardOrientation.Upright));
        Assert.Equal(TarotNoContentReason.NoReadyLocale,result.Reason);
    }

    [Fact]
    public void BuiltInDamageIsControlledUnavailableAndLabelsComeFromSamePackageLocale()
    {
        using var fixture=InterpretationToolingFixture.CreateSkeleton();using var output=BuiltInOutput.Create(fixture);
        var valid=BuiltInTarotInterpretationPackStoreCatalog.Create(output.Root);var labels=new TarotPackagePresentationLabelSource(valid).Resolve(new("classic"),1,new("ru"));
        Assert.NotNull(labels);Assert.Equal("Label situation",labels.SectionLabels["situation"]);Assert.Empty(labels.TagLabels);

        File.WriteAllBytes(output.PackagePath,"corrupt"u8.ToArray());var damaged=BuiltInTarotInterpretationPackStoreCatalog.Create(output.Root);
        Assert.Empty(damaged.PackIds);Assert.Equal("package.unavailable",Assert.Single(damaged.Diagnostics).Code);
    }

    [Fact]
    public void AppBuildContractShipsOnePackageAndNoAuthoringJsonOrToolingRuntimeReference()
    {
        var project=XDocument.Load(PathAt("NoxAeterna.App","NoxAeterna.App.csproj"));var xml=project.ToString(SaveOptions.DisableFormatting);
        Assert.Contains("CompileBuiltInInterpretationPackage",xml,StringComparison.Ordinal);Assert.Contains("resources\\interpretation\\tarot\\sources\\classic",xml,StringComparison.Ordinal);Assert.Contains("classic.noxinterp",xml,StringComparison.Ordinal);
        Assert.DoesNotContain("resources\\interpretation\\tarot\\packs\\**",xml,StringComparison.Ordinal);Assert.DoesNotContain("content\\**\\*.json",xml,StringComparison.Ordinal);
        var tooling=project.Descendants("ProjectReference").Single(item=>((string?)item.Attribute("Include"))?.Contains("NoxAeterna.Tools.Repository",StringComparison.Ordinal)==true);
        Assert.Equal("false",(string?)tooling.Attribute("ReferenceOutputAssembly"));Assert.Equal("false",(string?)tooling.Attribute("Private"));Assert.Equal("all",(string?)tooling.Attribute("PrivateAssets"));

        var configuration=new DirectoryInfo(AppContext.BaseDirectory).Parent?.Name??"Debug";var output=PathAt("NoxAeterna.App","bin",configuration,"net10.0");
        var packages=Directory.GetFiles(output,"*.noxinterp",SearchOption.AllDirectories);Assert.Single(packages);Assert.EndsWith(Path.Combine("resources","interpretation","tarot","packs","classic.noxinterp"),packages[0],StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(Directory.GetFiles(output,"*.json",SearchOption.AllDirectories),path=>path.Contains($"{Path.DirectorySeparatorChar}interpretation{Path.DirectorySeparatorChar}",StringComparison.OrdinalIgnoreCase));
        Assert.False(File.Exists(Path.Combine(output,"NoxAeterna.Tools.Repository.dll")));
    }

    private static string PathAt(params string[] segments)=>Path.Combine(new[]{Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,"..","..","..",".."))}.Concat(segments).ToArray());

    private static Dictionary<string, string> RussianVocabularyLabels(string sourceRoot)
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var path in Directory.GetFiles(Path.Combine(sourceRoot, "content", "ru", "vocabulary"), "*.json"))
        {
            using var document = JsonDocument.Parse(File.ReadAllText(path));
            result.Add(
                document.RootElement.GetProperty("conceptId").GetString()!,
                document.RootElement.GetProperty("label").GetString()!);
        }
        return result;
    }

    private sealed class BuiltInOutput:IDisposable
    {
        private BuiltInOutput(string root,string path)=>(Root,PackagePath)=(root,path);public string Root{get;}public string PackagePath{get;}
        public static BuiltInOutput Create(InterpretationToolingFixture fixture)=>Create(fixture.Root);
        public static BuiltInOutput Create(string sourceRoot){var root=Path.Combine(Path.GetTempPath(),$"NoxAeterna-app-package-{Guid.NewGuid():N}");var path=Path.Combine(root,BuiltInTarotInterpretationPackStoreCatalog.ClassicPackageOutputPath.Replace('/',Path.DirectorySeparatorChar));var report=new InterpretationPackageCompiler().Compile(sourceRoot,path,false);Assert.True(report.Success,string.Join(Environment.NewLine,report.Diagnostics.Select(static item=>item.Message)));return new(root,path);}
        public void Dispose(){if(Directory.Exists(Root))Directory.Delete(Root,true);}
    }
}
