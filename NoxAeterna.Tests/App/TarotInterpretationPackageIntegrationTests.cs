using System.Xml.Linq;
using NoxAeterna.App.Tarot;
using NoxAeterna.Domain.Tarot;
using NoxAeterna.Interpretation.Tarot.Contracts;
using NoxAeterna.Interpretation.Tarot.Resolution;
using NoxAeterna.Presentation.Localization;
using NoxAeterna.Tests.Tooling.Interpretation;
using NoxAeterna.Tools.Repository.Interpretation.Compilation;

namespace NoxAeterna.Tests.App;

public sealed class TarotInterpretationPackageIntegrationTests
{
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
    private sealed class BuiltInOutput:IDisposable
    {
        private BuiltInOutput(string root,string path)=>(Root,PackagePath)=(root,path);public string Root{get;}public string PackagePath{get;}
        public static BuiltInOutput Create(InterpretationToolingFixture fixture){var root=Path.Combine(Path.GetTempPath(),$"NoxAeterna-app-package-{Guid.NewGuid():N}");var path=Path.Combine(root,BuiltInTarotInterpretationPackStoreCatalog.ClassicPackageOutputPath.Replace('/',Path.DirectorySeparatorChar));var report=new InterpretationPackageCompiler().Compile(fixture.Root,path,false);Assert.True(report.Success,string.Join(Environment.NewLine,report.Diagnostics.Select(static item=>item.Message)));return new(root,path);}
        public void Dispose(){if(Directory.Exists(Root))Directory.Delete(Root,true);}
    }
}
