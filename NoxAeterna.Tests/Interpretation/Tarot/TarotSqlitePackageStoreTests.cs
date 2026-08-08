using Microsoft.Data.Sqlite;
using NoxAeterna.Domain.Tarot;
using NoxAeterna.Interpretation.Sqlite;
using NoxAeterna.Interpretation.Tarot.Contracts;
using NoxAeterna.Interpretation.Tarot.Resolution;
using NoxAeterna.Interpretation.Tarot.Storage;
using NoxAeterna.Tests.Tooling.Interpretation;
using NoxAeterna.Tools.Repository.Interpretation.Compilation;

namespace NoxAeterna.Tests.Interpretation.Tarot;

public sealed class TarotSqlitePackageStoreTests
{
    [Fact]
    public void CompiledStoreReadsMetadataLabelsAndExactSemanticRows()
    {
        using var fixture=InterpretationToolingFixture.CreateSkeleton();fixture.AddVocabulary("ru","transition");fixture.AddTaggedSingle("ru","major.fool","transition");fixture.AddPair("ru","major.fool","major.magician");fixture.AddPositions("ru","major.fool");fixture.AddSynthesis("ru",TarotSynthesisResourceType.TrajectoryProfile,"trajectory-profile",TarotThreeCardSynthesisContract.Improving);
        using var package=CompiledPackage.Create(fixture);
        var store=new TarotSqlitePackageStore(package.Path,new("classic"),StandardTarotCatalog.Deck.Id);

        Assert.Equal("classic",store.Manifest.PackId.Value);Assert.Matches("^[0-9a-f]{64}$",store.SourceDigest.Value);
        var labels=store.GetLabels(new("ru"));Assert.Equal(TarotInterpretationStoreStatus.Found,labels.Status);Assert.Equal(5,labels.Value!.Labels.SingleCardSections.Count);
        var single=store.GetSingleCard(new("ru"),new("major.fool"),TarotCardOrientation.Reversed);Assert.Equal(TarotInterpretationStoreStatus.Found,single.Status);Assert.Equal(TarotReversalMechanism.Blocked,Assert.Single(single.Value!.ReversalMechanisms));Assert.Equal("transition",Assert.Single(single.Value.Tags).ConceptId.Value);
        var pair=store.GetOrientedPair(new("ru"),new("major.fool"),new("major.magician"),TarotOrientedPairState.UprightReversed);Assert.Equal(TarotInterpretationStoreStatus.Found,pair.Status);Assert.Equal(TarotOrientedPairState.UprightReversed,pair.Value!.OrientationState);
        var position=store.GetThreeCardPosition(new("ru"),TarotThreeCardPosition.Future,new("major.fool"),TarotCardOrientation.Reversed);Assert.Equal(TarotInterpretationStoreStatus.Found,position.Status);Assert.Equal(TarotThreeCardPosition.Future,position.Value!.Position);
        var synthesis=store.GetSynthesisResource(new("ru"),TarotSynthesisResourceType.TrajectoryProfile,new(TarotThreeCardSynthesisContract.Improving));Assert.Equal(TarotInterpretationStoreStatus.Found,synthesis.Status);Assert.Equal("Fixture synthesis text",synthesis.Value!.Text);Assert.Equal("{\"text\":\"Fixture synthesis text\"}\n",synthesis.Value.CanonicalJson);
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

    [Theory]
    [InlineData("missing")]
    [InlineData("extra")]
    [InlineData("wrong-id")]
    [InlineData("malformed-payload")]
    public void ReadyThreeCardsRequiresExactTypedSynthesisInventory(string mutation)
    {
        using var fixture = InterpretationToolingFixture.CreateSkeleton();
        fixture.AddCompleteSynthesis("ru");
        using var package = CompiledPackage.Create(fixture);
        using (var connection = OpenWrite(package.Path))
        {
            PopulateReadyThreeCards(connection);
            using var command = connection.CreateCommand();
            command.CommandText = mutation switch
            {
                "missing" => $"DELETE FROM synthesis_resource WHERE locale='ru' AND resource_type='trajectory-profile' AND resource_id='{TarotThreeCardSynthesisContract.Improving}'",
                "extra" => "INSERT INTO synthesis_resource VALUES('ru','relation-label','overall','{\"text\":\"Extra\"}')",
                "wrong-id" => $"UPDATE synthesis_resource SET resource_id='unknown-profile' WHERE locale='ru' AND resource_type='trajectory-profile' AND resource_id='{TarotThreeCardSynthesisContract.Improving}'",
                "malformed-payload" => $"UPDATE synthesis_resource SET canonical_json='{{\"kind\":\"wrong\"}}' WHERE locale='ru' AND resource_type='trajectory-profile' AND resource_id='{TarotThreeCardSynthesisContract.Improving}'",
                _ => throw new ArgumentOutOfRangeException(nameof(mutation))
            };
            Assert.Equal(1, command.ExecuteNonQuery());
        }

        var store = new TarotSqlitePackageStore(package.Path, new("classic"), StandardTarotCatalog.Deck.Id);
        var result = store.ValidateReadyModule(new("ru"), TarotInterpretationMode.ThreeCards);

        Assert.Equal(TarotInterpretationStoreStatus.Missing, result.Status);
    }

    [Fact]
    public void ReadyThreeCardsAcceptsCompleteTypedSynthesisInventory()
    {
        using var fixture = InterpretationToolingFixture.CreateSkeleton();
        fixture.AddCompleteSynthesis("ru");
        using var package = CompiledPackage.Create(fixture);
        using (var connection = OpenWrite(package.Path)) PopulateReadyThreeCards(connection);

        var store = new TarotSqlitePackageStore(package.Path, new("classic"), StandardTarotCatalog.Deck.Id);
        var result = store.ValidateReadyModule(new("ru"), TarotInterpretationMode.ThreeCards);

        Assert.Equal(TarotInterpretationStoreStatus.Found, result.Status);
        Assert.Equal(
            [TarotInterpretationCorpus.OrientedPairs, TarotInterpretationCorpus.ThreeCards],
            result.Value);
    }

    [Fact]
    public void MissingRealPromotedPairRowIsBrokenReadyAndStopsEnglishRequestAtRussianModule()
    {
        var sourceRoot = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..",
            "resources", "interpretation", "tarot", "sources", "classic"));
        using var package = CompiledPackage.Create(sourceRoot);
        using (var connection = OpenWrite(package.Path))
        {
            using var command = connection.CreateCommand();
            command.CommandText = """
                                  PRAGMA foreign_keys=ON;
                                  DELETE FROM oriented_pair_tag
                                  WHERE locale='ru' AND card_a_id='major.chariot' AND card_b_id='major.death'
                                    AND orientation_state='upright-upright';
                                  DELETE FROM oriented_pair
                                  WHERE locale='ru' AND card_a_id='major.chariot' AND card_b_id='major.death'
                                    AND orientation_state='upright-upright';
                                  """;
            Assert.Equal(4, command.ExecuteNonQuery());
        }

        var store = new TarotSqlitePackageStore(package.Path, new("classic"), StandardTarotCatalog.Deck.Id);
        var validation = store.ValidateReadyModule(new("ru"), TarotInterpretationMode.TwoCards);
        Assert.Equal(TarotInterpretationStoreStatus.Missing, validation.Status);

        var resolver = new TarotInterpretationPackResolver(new SingleStoreCatalog(store), StandardTarotCatalog.Deck);
        var resolution = Assert.IsType<NoTarotInterpretationContent<TarotOrientedPairEntry>>(
            resolver.ResolveOrientedPair(
                new("classic"),
                TarotInterpretationMode.TwoCards,
                new("en"),
                new("major.chariot"),
                TarotCardOrientation.Upright,
                new("major.death"),
                TarotCardOrientation.Upright));
        Assert.Equal(TarotNoContentReason.BrokenReadyModule, resolution.Reason);
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

    private static void PopulateReadyThreeCards(SqliteConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = """
                              UPDATE module SET ready=1 WHERE mode='three-cards' AND locale='ru';
                              WITH RECURSIVE n(value) AS (SELECT 0 UNION ALL SELECT value+1 FROM n WHERE value<12011)
                              INSERT INTO oriented_pair(locale,card_a_id,card_b_id,orientation_state,interaction,direction,overall_valence,overall_intensity)
                              SELECT 'ru',printf('a%05d',value),'z',
                                     CASE value%4 WHEN 0 THEN 'upright-upright' WHEN 1 THEN 'upright-reversed' WHEN 2 THEN 'reversed-upright' ELSE 'reversed-reversed' END,
                                     'interaction','direction',0,2 FROM n;
                              WITH RECURSIVE n(value) AS (SELECT 0 UNION ALL SELECT value+1 FROM n WHERE value<467)
                              INSERT INTO three_card_position(locale,position,card_id,orientation,text,overall_valence,overall_intensity)
                              SELECT 'ru',CASE value%3 WHEN 0 THEN 'past' WHEN 1 THEN 'present' ELSE 'future' END,
                                     printf('card%05d',value),CASE value%2 WHEN 0 THEN 'upright' ELSE 'reversed' END,'text',0,2 FROM n;
                              """;
        Assert.Equal(12_481, command.ExecuteNonQuery());
    }

    private sealed class CompiledPackage:IDisposable
    {
        private CompiledPackage(string path)=>Path=path;public string Path{get;}
        public static CompiledPackage Create(InterpretationToolingFixture fixture){var path=System.IO.Path.Combine(System.IO.Path.GetTempPath(),$"NoxAeterna-store-{Guid.NewGuid():N}.noxinterp");var report=new InterpretationPackageCompiler().Compile(fixture.Root,path,false);Assert.True(report.Success,string.Join(Environment.NewLine,report.Diagnostics.Select(static item=>item.Message)));return new(path);}
        public static CompiledPackage Create(string sourceRoot){var path=System.IO.Path.Combine(System.IO.Path.GetTempPath(),$"NoxAeterna-store-{Guid.NewGuid():N}.noxinterp");var report=new InterpretationPackageCompiler().Compile(sourceRoot,path,false);Assert.True(report.Success,string.Join(Environment.NewLine,report.Diagnostics.Select(static item=>item.Message)));return new(path);}
        public void Dispose(){if(File.Exists(Path))File.Delete(Path);}
    }

    private sealed class SingleStoreCatalog(ITarotInterpretationPackStore store) : ITarotInterpretationPackStoreCatalog
    {
        public bool TryGetStore(TarotInterpretationPackId packId, out ITarotInterpretationPackStore? result)
        {
            result = packId == store.Manifest.PackId ? store : null;
            return result is not null;
        }
    }
}
