using Microsoft.Data.Sqlite;
using NoxAeterna.Domain.Tarot;
using NoxAeterna.Interpretation.Tarot.Contracts;
using NoxAeterna.Interpretation.Tarot.Serialization;
using NoxAeterna.Interpretation.Tarot.Storage;
using NoxAeterna.Interpretation.Tarot.Validation;

namespace NoxAeterna.Interpretation.Sqlite;

/// <summary>Read-only SQLite implementation of one immutable interpretation package.</summary>
public sealed class TarotSqlitePackageStore : ITarotInterpretationPackStore
{
    private readonly string packagePath;

    public TarotSqlitePackageStore(string packagePath, TarotInterpretationPackId expectedPackId, TarotDeckId expectedDeckId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packagePath);
        ArgumentNullException.ThrowIfNull(expectedPackId);
        ArgumentNullException.ThrowIfNull(expectedDeckId);
        this.packagePath = Path.GetFullPath(packagePath);
        using var connection = TarotSqlitePackageWriter.OpenReadOnly(this.packagePath);
        TarotSqlitePackageWriter.VerifyConnection(connection);
        var metadata = ReadMetadata(connection);
        if (metadata.PackageSchemaVersion != TarotSqliteSchema.UserVersion) throw new InvalidDataException("Wrong package schema version.");
        if (metadata.Manifest.PackId != expectedPackId) throw new InvalidDataException("Package pack ID does not match registration identity.");
        if (metadata.Manifest.SemanticDeckId != expectedDeckId) throw new InvalidDataException("Package semantic deck does not match registration identity.");
        Manifest = metadata.Manifest;
        SourceDigest = metadata.SourceDigest;
    }

    public TarotInterpretationPackManifest Manifest { get; }
    public TarotSha256 SourceDigest { get; }

    public static bool TryOpen(string packagePath,TarotInterpretationPackId expectedPackId,TarotDeckId expectedDeckId,out ITarotInterpretationPackStore? store,out string? diagnostic)
    {
        try{store=new TarotSqlitePackageStore(packagePath,expectedPackId,expectedDeckId);diagnostic=null;return true;}
        catch(Exception exception) when(exception is IOException or InvalidDataException or SqliteException or UnauthorizedAccessException or ArgumentException)
        {store=null;diagnostic=exception.Message;return false;}
    }

    public TarotInterpretationStoreResult<IReadOnlyList<TarotInterpretationCorpus>> ValidateReadyModule(TarotInterpretationLocale locale, TarotInterpretationMode mode) =>
        Query(connection =>
        {
            if (!Manifest.Modules.TryGetValue(mode, out var byLocale) || !byLocale.TryGetValue(locale, out var module) || !module.Ready) return null;
            if (GetLabelsCore(connection, locale) is null) return null;
            var corpora = new List<TarotInterpretationCorpus>();
            if (mode == TarotInterpretationMode.SingleCard)
            {
                if (Count(connection, "SELECT count(*) FROM single_card WHERE locale=$l", ("$l", locale.Value)) != 156) return null;
                corpora.Add(TarotInterpretationCorpus.SingleCard);
            }
            if (mode is TarotInterpretationMode.TwoCards or TarotInterpretationMode.ThreeCards)
            {
                if (Count(connection, "SELECT count(*) FROM oriented_pair WHERE locale=$l", ("$l", locale.Value)) != 12012) return null;
                corpora.Add(TarotInterpretationCorpus.OrientedPairs);
            }
            if (mode == TarotInterpretationMode.ThreeCards)
            {
                if (Count(connection, "SELECT count(*) FROM three_card_position WHERE locale=$l", ("$l", locale.Value)) != 468 ||
                    !HasExactSynthesisInventory(connection, locale)) return null;
                corpora.Add(TarotInterpretationCorpus.ThreeCards);
            }
            return (IReadOnlyList<TarotInterpretationCorpus>)Array.AsReadOnly(corpora.ToArray());
        }, "store.ready-incomplete", "The ready module inventory is incomplete.");

    public TarotInterpretationStoreResult<TarotLocalizedInterpretationLabels> GetLabels(TarotInterpretationLocale locale) =>
        Query(connection => GetLabelsCore(connection, locale), "store.labels-missing", "Pack-local labels are missing.");

    public TarotInterpretationStoreResult<TarotSingleCardEntry> GetSingleCard(TarotInterpretationLocale locale, TarotCardId cardId, TarotCardOrientation orientation) =>
        Query(connection =>
        {
            using var command = Command(connection, "SELECT situation,development,risk,outcome,advice,overall_valence,overall_intensity FROM single_card WHERE locale=$l AND card_id=$c AND orientation=$o",
                ("$l",locale.Value),("$c",cardId.Value),("$o",Text(orientation, TarotSchemaText.CardOrientations)));
            using var reader = command.ExecuteReader();
            if (!reader.Read()) return null;
            var sections = new Dictionary<string,string>(StringComparer.Ordinal) { ["situation"]=reader.GetString(0),["development"]=reader.GetString(1),["risk"]=reader.GetString(2),["outcome"]=reader.GetString(3),["advice"]=reader.GetString(4) };
            var valence=reader.GetInt32(5);var intensity=reader.GetInt32(6);reader.Close();
            var tags=ReadTags(connection,"SELECT concept_id,valence,intensity FROM single_card_tag WHERE locale=$l AND card_id=$c AND orientation=$o ORDER BY ordinal",("$l",locale.Value),("$c",cardId.Value),("$o",Text(orientation,TarotSchemaText.CardOrientations)));
            var mechanisms=ReadEnums(connection,"SELECT mechanism FROM single_card_reversal_mechanism WHERE locale=$l AND card_id=$c AND orientation=$o ORDER BY ordinal",TarotSchemaText.ReversalMechanisms,("$l",locale.Value),("$c",cardId.Value),("$o",Text(orientation,TarotSchemaText.CardOrientations)));
            return new TarotSingleCardEntry(cardId,orientation,sections,tags,valence,intensity,mechanisms);
        }, "store.single-card-missing", "The ready single-card row is missing.");

    public TarotInterpretationStoreResult<TarotOrientedPairEntry> GetOrientedPair(TarotInterpretationLocale locale, TarotCardId cardAId, TarotCardId cardBId, TarotOrientedPairState state) =>
        Query(connection =>
        {
            var stateText=Text(state,TarotSchemaText.PairStates);
            using var command=Command(connection,"SELECT interaction,direction,overall_valence,overall_intensity FROM oriented_pair WHERE locale=$l AND card_a_id=$a AND card_b_id=$b AND orientation_state=$s",("$l",locale.Value),("$a",cardAId.Value),("$b",cardBId.Value),("$s",stateText));
            using var reader=command.ExecuteReader();if(!reader.Read())return null;var interaction=reader.GetString(0);var direction=reader.GetString(1);var valence=reader.GetInt32(2);var intensity=reader.GetInt32(3);reader.Close();
            var tags=ReadTags(connection,"SELECT concept_id,valence,intensity FROM oriented_pair_tag WHERE locale=$l AND card_a_id=$a AND card_b_id=$b AND orientation_state=$s ORDER BY ordinal",("$l",locale.Value),("$a",cardAId.Value),("$b",cardBId.Value),("$s",stateText));
            return new TarotOrientedPairEntry(cardAId,cardBId,state,interaction,direction,tags,valence,intensity);
        }, "store.oriented-pair-missing", "The ready oriented-pair row is missing.");

    public TarotInterpretationStoreResult<TarotThreeCardPositionEntry> GetThreeCardPosition(TarotInterpretationLocale locale, TarotThreeCardPosition position, TarotCardId cardId, TarotCardOrientation orientation) =>
        Query(connection =>
        {
            var positionText=Text(position,TarotSchemaText.Positions);var orientationText=Text(orientation,TarotSchemaText.CardOrientations);
            using var command=Command(connection,"SELECT text,overall_valence,overall_intensity FROM three_card_position WHERE locale=$l AND position=$p AND card_id=$c AND orientation=$o",("$l",locale.Value),("$p",positionText),("$c",cardId.Value),("$o",orientationText));
            using var reader=command.ExecuteReader();if(!reader.Read())return null;var content=reader.GetString(0);var valence=reader.GetInt32(1);var intensity=reader.GetInt32(2);reader.Close();
            var tags=ReadTags(connection,"SELECT concept_id,valence,intensity FROM three_card_position_tag WHERE locale=$l AND position=$p AND card_id=$c AND orientation=$o ORDER BY ordinal",("$l",locale.Value),("$p",positionText),("$c",cardId.Value),("$o",orientationText));
            return new TarotThreeCardPositionEntry(position,cardId,orientation,content,tags,valence,intensity);
        }, "store.three-card-position-missing", "The ready three-card-position row is missing.");

    public TarotInterpretationStoreResult<TarotSynthesisResource> GetSynthesisResource(TarotInterpretationLocale locale, TarotSynthesisResourceType resourceType, TarotSynthesisResourceId resourceId) =>
        Query(connection =>
        {
            using var command=Command(connection,"SELECT canonical_json FROM synthesis_resource WHERE locale=$l AND resource_type=$t AND resource_id=$i",("$l",locale.Value),("$t",Text(resourceType,TarotSchemaText.SynthesisResourceTypes)),("$i",resourceId.Value));
            var value=command.ExecuteScalar() as string;
            if (value is null || !TryParseSynthesisText(value, out var text)) return null;
            return new TarotSynthesisResource(resourceType,resourceId,text!,value);
        }, "store.synthesis-missing", "The ready synthesis row is missing.");

    private TarotInterpretationStoreResult<T> Query<T>(Func<SqliteConnection,T?> query,string missingCode,string missingMessage) where T:class
    {
        try { using var connection=TarotSqlitePackageWriter.OpenReadOnly(packagePath);TarotSqlitePackageWriter.VerifyConnection(connection);var value=query(connection);return value is null?TarotInterpretationStoreResult<T>.Missing():TarotInterpretationStoreResult<T>.Found(value); }
        catch(Exception exception) when(exception is IOException or InvalidDataException or SqliteException or ArgumentException)
        { return TarotInterpretationStoreResult<T>.Failed("store.package-failed",exception.Message); }
    }

    private static TarotLocalizedInterpretationLabels? GetLabelsCore(SqliteConnection connection,TarotInterpretationLocale locale)
    {
        var groups=new Dictionary<string,Dictionary<string,string>>(StringComparer.Ordinal) { ["single-card-section"]=new(StringComparer.Ordinal),["three-card-position"]=new(StringComparer.Ordinal),["relation"]=new(StringComparer.Ordinal) };
        using(var command=Command(connection,"SELECT category,label_id,value FROM label WHERE locale=$l ORDER BY category,label_id",("$l",locale.Value))) using(var reader=command.ExecuteReader()) while(reader.Read()) if(groups.TryGetValue(reader.GetString(0),out var group))group.Add(reader.GetString(1),reader.GetString(2));
        var document=new TarotLabelsDocument{SchemaVersion=1,SingleCardSections=groups["single-card-section"].ToDictionary(static p=>p.Key,static p=>(string?)p.Value,StringComparer.Ordinal),ThreeCardPositions=groups["three-card-position"].ToDictionary(static p=>p.Key,static p=>(string?)p.Value,StringComparer.Ordinal),Relations=groups["relation"].ToDictionary(static p=>p.Key,static p=>(string?)p.Value,StringComparer.Ordinal)};
        var validated=TarotInterpretationValidator.ValidateLabels(document);if(!validated.IsValid||validated.Value is null)return null;
        var tags=new Dictionary<TarotTagConceptId,string>();using(var command=Command(connection,"SELECT concept_id,label FROM vocabulary WHERE locale=$l ORDER BY concept_id",("$l",locale.Value)))using(var reader=command.ExecuteReader())while(reader.Read())tags.Add(new(reader.GetString(0)),reader.GetString(1));
        return new(validated.Value,tags);
    }

    private static (int PackageSchemaVersion,TarotInterpretationPackManifest Manifest,TarotSha256 SourceDigest) ReadMetadata(SqliteConnection connection)
    {
        int schema;string pack;string deck;string source;int version;string digest;
        using(var command=connection.CreateCommand()){command.CommandText="SELECT package_schema_version,pack_id,semantic_deck_id,source_locale,content_version,source_digest FROM pack_metadata WHERE singleton=1";using var reader=command.ExecuteReader();if(!reader.Read())throw new InvalidDataException("Package metadata is missing.");schema=reader.GetInt32(0);pack=reader.GetString(1);deck=reader.GetString(2);source=reader.GetString(3);version=reader.GetInt32(4);digest=reader.GetString(5);}
        var locales=new List<string?>();using(var command=connection.CreateCommand()){command.CommandText="SELECT locale FROM declared_locale ORDER BY locale";using var reader=command.ExecuteReader();while(reader.Read())locales.Add(reader.GetString(0));}
        var names=new Dictionary<string,string?>(StringComparer.Ordinal);using(var command=connection.CreateCommand()){command.CommandText="SELECT locale,value FROM display_name ORDER BY locale";using var reader=command.ExecuteReader();while(reader.Read())names.Add(reader.GetString(0),reader.GetString(1));}
        var modules=new Dictionary<string,Dictionary<string,TarotInterpretationModuleDocument?>?>(StringComparer.Ordinal);
        using(var command=connection.CreateCommand()){command.CommandText="SELECT mode,locale,ready FROM module ORDER BY mode,locale";using var reader=command.ExecuteReader();while(reader.Read()){var mode=reader.GetString(0);if(!modules.TryGetValue(mode,out var byLocale)){byLocale=new(StringComparer.Ordinal);modules.Add(mode,byLocale);}byLocale![reader.GetString(1)]=new(){Ready=reader.GetInt32(2)==1,Dependencies=[]};}}
        using(var command=connection.CreateCommand()){command.CommandText="SELECT mode,locale,dependency FROM module_dependency ORDER BY mode,locale,ordinal";using var reader=command.ExecuteReader();while(reader.Read()){if(!TarotSchemaText.TryParse(reader.GetString(2),TarotSchemaText.Dependencies,out TarotModuleDependency dependency))throw new InvalidDataException("Unknown module dependency.");modules[reader.GetString(0)]![reader.GetString(1)]!.Dependencies!.Add(dependency);}}
        var raw=new TarotInterpretationPackDocument{SchemaVersion=2,PackId=pack,SemanticDeckId=deck,SourceLocale=source,ContentVersion=version,DeclaredLocales=locales,DisplayNames=names,Modules=modules};
        var validated=TarotInterpretationValidator.ValidateManifest(raw);if(!validated.IsValid||validated.Value is null)throw new InvalidDataException("Package metadata manifest is invalid.");
        return(schema,validated.Value,new TarotSha256(digest));
    }

    private static IReadOnlyList<TarotTagAssignment> ReadTags(SqliteConnection connection,string sql,params (string,object)[] args){var result=new List<TarotTagAssignment>();using var command=Command(connection,sql,args);using var reader=command.ExecuteReader();while(reader.Read())result.Add(new(new(reader.GetString(0)),reader.GetInt32(1),reader.GetInt32(2)));return result.AsReadOnly();}
    private static bool HasExactSynthesisInventory(SqliteConnection connection, TarotInterpretationLocale locale)
    {
        var actual = new HashSet<TarotSynthesisResourceIdentity>();
        using var command = Command(connection, "SELECT resource_type,resource_id,canonical_json FROM synthesis_resource WHERE locale=$l ORDER BY resource_type,resource_id", ("$l", locale.Value));
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            if (!TarotSchemaText.TryParse(reader.GetString(0), TarotSchemaText.SynthesisResourceTypes, out TarotSynthesisResourceType type)) return false;
            TarotSynthesisResourceId id;
            try { id = new(reader.GetString(1)); }
            catch (ArgumentException) { return false; }
            if (!TarotThreeCardSynthesisContract.IsRequired(type, id) || !TryParseSynthesisText(reader.GetString(2), out _)) return false;
            if (!actual.Add(new(type, id))) return false;
        }
        return actual.SetEquals(TarotThreeCardSynthesisContract.RequiredResources);
    }
    private static bool TryParseSynthesisText(string canonicalJson, out string? text)
    {
        text = null;
        var parsed = TarotInterpretationJson.Parse<TarotSynthesisTextDocument>(canonicalJson);
        if (!parsed.IsSuccess || parsed.Document?.Text is not { } value || string.IsNullOrWhiteSpace(value) || value != value.Trim()) return false;
        if (!string.Equals(canonicalJson, TarotInterpretationJson.SerializeToString(parsed.Document), StringComparison.Ordinal)) return false;
        text = value;
        return true;
    }
    private static IReadOnlyList<TEnum> ReadEnums<TEnum>(SqliteConnection connection,string sql,IReadOnlyDictionary<TEnum,string> map,params (string,object)[] args) where TEnum:struct,Enum{var result=new List<TEnum>();using var command=Command(connection,sql,args);using var reader=command.ExecuteReader();while(reader.Read()){if(!TarotSchemaText.TryParse(reader.GetString(0),map,out TEnum value))throw new InvalidDataException("Unknown stored enum value.");result.Add(value);}return result.AsReadOnly();}
    private static int Count(SqliteConnection connection,string sql,params (string,object)[] args){using var command=Command(connection,sql,args);return Convert.ToInt32(command.ExecuteScalar(),System.Globalization.CultureInfo.InvariantCulture);}
    private static SqliteCommand Command(SqliteConnection connection,string sql,params (string Name,object Value)[] args){var command=connection.CreateCommand();command.CommandText=sql;foreach(var arg in args)command.Parameters.AddWithValue(arg.Name,arg.Value);return command;}
    private static string Text<TEnum>(TEnum value,IReadOnlyDictionary<TEnum,string> map) where TEnum:struct,Enum=>TarotSchemaText.Get(value,map);
}
