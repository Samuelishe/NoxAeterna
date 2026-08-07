using Microsoft.Data.Sqlite;
using NoxAeterna.Domain.Tarot;
using NoxAeterna.Interpretation.Sqlite;
using NoxAeterna.Interpretation.Tarot.Contracts;
using NoxAeterna.Interpretation.Tarot.Storage;
using NoxAeterna.Tests.Tooling.Interpretation;
using NoxAeterna.Tools.Repository.Interpretation.Compilation;

namespace NoxAeterna.Tests.Interpretation.Tarot;

public sealed class TarotSqlitePackageStoreTests
{
    [Fact]
    public void CompiledStoreReadsMetadataLabelsAndExactSemanticRows()
    {
        using var fixture=InterpretationToolingFixture.CreateSkeleton();fixture.AddVocabulary("ru","transition");fixture.AddTaggedSingle("ru","major.fool","transition");fixture.AddPair("ru","major.fool","major.magician");fixture.AddPositions("ru","major.fool");fixture.AddSynthesis("ru",TarotSynthesisResourceType.TrajectoryProfile,"trajectory-profile","basic");
        using var package=CompiledPackage.Create(fixture);
        var store=new TarotSqlitePackageStore(package.Path,new("classic"),StandardTarotCatalog.Deck.Id);

        Assert.Equal("classic",store.Manifest.PackId.Value);Assert.Matches("^[0-9a-f]{64}$",store.SourceDigest.Value);
        var labels=store.GetLabels(new("ru"));Assert.Equal(TarotInterpretationStoreStatus.Found,labels.Status);Assert.Equal(5,labels.Value!.Labels.SingleCardSections.Count);
        var single=store.GetSingleCard(new("ru"),new("major.fool"),TarotCardOrientation.Reversed);Assert.Equal(TarotInterpretationStoreStatus.Found,single.Status);Assert.Equal(TarotReversalMechanism.Blocked,Assert.Single(single.Value!.ReversalMechanisms));Assert.Equal("transition",Assert.Single(single.Value.Tags).ConceptId.Value);
        var pair=store.GetOrientedPair(new("ru"),new("major.fool"),new("major.magician"),TarotOrientedPairState.UprightReversed);Assert.Equal(TarotInterpretationStoreStatus.Found,pair.Status);Assert.Equal(TarotOrientedPairState.UprightReversed,pair.Value!.OrientationState);
        var position=store.GetThreeCardPosition(new("ru"),TarotThreeCardPosition.Future,new("major.fool"),TarotCardOrientation.Reversed);Assert.Equal(TarotInterpretationStoreStatus.Found,position.Status);Assert.Equal(TarotThreeCardPosition.Future,position.Value!.Position);
        var synthesis=store.GetSynthesisResource(new("ru"),TarotSynthesisResourceType.TrajectoryProfile,new("basic"));Assert.Equal(TarotInterpretationStoreStatus.Found,synthesis.Status);Assert.Equal("{\"kind\":\"fixture\"}\n",synthesis.Value!.CanonicalJson);
    }

    [Theory]
    [InlineData("application")]
    [InlineData("version")]
    [InlineData("pack")]
    [InlineData("deck")]
    [InlineData("schema")]
    public void WrongPackageIdentityIsRejectedAtRegistration(string mutation)
    {
        using var fixture=InterpretationToolingFixture.CreateSkeleton();using var package=CompiledPackage.Create(fixture);
        using(var connection=OpenWrite(package.Path)){using var command=connection.CreateCommand();command.CommandText=mutation switch{"application"=>"PRAGMA application_id=1","version"=>"PRAGMA user_version=99","pack"=>"UPDATE pack_metadata SET pack_id='other'","deck"=>"UPDATE pack_metadata SET semantic_deck_id='other'","schema"=>"PRAGMA ignore_check_constraints=ON; UPDATE pack_metadata SET package_schema_version=2",_=>throw new ArgumentOutOfRangeException()};command.ExecuteNonQuery();}
        Assert.ThrowsAny<Exception>(()=>new TarotSqlitePackageStore(package.Path,new("classic"),StandardTarotCatalog.Deck.Id));
    }

    [Fact]
    public void FrozenSqliteConstraintsRejectOutOfRangeSemanticRows()
    {
        using var fixture=InterpretationToolingFixture.CreateSkeleton();using var package=CompiledPackage.Create(fixture);
        using var connection=OpenWrite(package.Path);using var command=connection.CreateCommand();
        command.CommandText="INSERT INTO single_card VALUES('ru','major.fool','upright','s','d','r','o','a',3,2)";
        Assert.Throws<SqliteException>(()=>command.ExecuteNonQuery());
    }

    [Fact]
    public void MissingRequiredReadyInventoryIsControlledMissingAndDoesNotFallbackInsideStore()
    {
        using var fixture=InterpretationToolingFixture.CreateSkeleton();using var package=CompiledPackage.Create(fixture);
        using(var connection=OpenWrite(package.Path)){using var command=connection.CreateCommand();command.CommandText="UPDATE module SET ready=1 WHERE mode='single-card' AND locale='en'";command.ExecuteNonQuery();}
        var store=new TarotSqlitePackageStore(package.Path,new("classic"),StandardTarotCatalog.Deck.Id);
        var result=store.ValidateReadyModule(new("en"),TarotInterpretationMode.SingleCard);
        Assert.Equal(TarotInterpretationStoreStatus.Missing,result.Status);
    }

    [Fact]
    public void DamageAfterRegistrationBecomesControlledStoreFailure()
    {
        using var fixture=InterpretationToolingFixture.CreateSkeleton();using var package=CompiledPackage.Create(fixture);var store=new TarotSqlitePackageStore(package.Path,new("classic"),StandardTarotCatalog.Deck.Id);
        File.WriteAllBytes(package.Path,"damaged"u8.ToArray());
        var result=store.GetLabels(new("ru"));
        Assert.Equal(TarotInterpretationStoreStatus.Failed,result.Status);Assert.Equal("store.package-failed",result.Diagnostic!.Code);
    }

    private static SqliteConnection OpenWrite(string path){var connection=new SqliteConnection($"Data Source={path};Mode=ReadWrite;Pooling=False");connection.Open();return connection;}

    private sealed class CompiledPackage:IDisposable
    {
        private CompiledPackage(string path)=>Path=path;public string Path{get;}
        public static CompiledPackage Create(InterpretationToolingFixture fixture){var path=System.IO.Path.Combine(System.IO.Path.GetTempPath(),$"NoxAeterna-store-{Guid.NewGuid():N}.noxinterp");var report=new InterpretationPackageCompiler().Compile(fixture.Root,path,false);Assert.True(report.Success,string.Join(Environment.NewLine,report.Diagnostics.Select(static item=>item.Message)));return new(path);}
        public void Dispose(){if(File.Exists(Path))File.Delete(Path);}
    }
}
