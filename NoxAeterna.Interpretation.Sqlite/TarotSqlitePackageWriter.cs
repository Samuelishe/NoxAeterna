using Microsoft.Data.Sqlite;
using NoxAeterna.Interpretation.Tarot.Contracts;
using NoxAeterna.Interpretation.Tarot.Serialization;

namespace NoxAeterna.Interpretation.Sqlite;

/// <summary>Writes and verifies one immutable SQLite interpretation package.</summary>
public sealed class TarotSqlitePackageWriter
{
    public void Write(TarotInterpretationCompilation compilation, string outputPath)
    {
        ArgumentNullException.ThrowIfNull(compilation);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);
        var target = Path.GetFullPath(outputPath);
        var directory = Path.GetDirectoryName(target) ?? throw new ArgumentException("Output must have a parent directory.", nameof(outputPath));
        Directory.CreateDirectory(directory);
        var temporary = Path.Combine(directory, $".{Path.GetFileName(target)}.{Guid.NewGuid():N}.tmp");
        try
        {
            using (var connection = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = temporary, Mode = SqliteOpenMode.ReadWriteCreate, Pooling = false }.ToString()))
            {
                connection.Open();
                Execute(connection, "PRAGMA journal_mode=OFF; PRAGMA synchronous=OFF; PRAGMA locking_mode=EXCLUSIVE;");
                Execute(connection, $"PRAGMA application_id={TarotSqliteSchema.ApplicationId}; PRAGMA user_version={TarotSqliteSchema.UserVersion};");
                Execute(connection, TarotSqliteSchema.Ddl);
                using var transaction = connection.BeginTransaction();
                InsertAll(connection, transaction, compilation);
                transaction.Commit();
                VerifyConnection(connection);
                Execute(connection, "VACUUM;");
            }
            File.Move(temporary, target, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
        }
    }

    public TarotSqlitePackageInspection Inspect(string packagePath)
    {
        using var connection = OpenReadOnly(packagePath);
        VerifyConnection(connection);
        string packId;
        string semanticDeckId;
        string sourceLocale;
        int contentVersion;
        string sourceDigest;
        using (var command = connection.CreateCommand())
        {
            command.CommandText = "SELECT pack_id,semantic_deck_id,source_locale,content_version,source_digest FROM pack_metadata WHERE singleton=1";
            using var metadata = command.ExecuteReader();
            if (!metadata.Read()) throw new InvalidDataException("Package metadata is missing.");
            packId = metadata.GetString(0);
            semanticDeckId = metadata.GetString(1);
            sourceLocale = metadata.GetString(2);
            contentVersion = metadata.GetInt32(3);
            sourceDigest = metadata.GetString(4);
        }
        return new TarotSqlitePackageInspection(
            packId, semanticDeckId, sourceLocale, contentVersion, sourceDigest,
            ScalarInt(connection, "SELECT count(*) FROM declared_locale"),
            ScalarInt(connection, "SELECT count(*) FROM label"),
            ScalarInt(connection, "SELECT count(*) FROM vocabulary"),
            ScalarInt(connection, "SELECT count(*) FROM single_card"),
            ScalarInt(connection, "SELECT count(*) FROM oriented_pair"),
            ScalarInt(connection, "SELECT count(*) FROM three_card_position"),
            ScalarInt(connection, "SELECT count(*) FROM synthesis_resource"));
    }

    public void Check(TarotInterpretationCompilation compilation, string packagePath)
    {
        var inspection = Inspect(packagePath);
        if (inspection.PackId != compilation.Manifest.PackId.Value ||
            inspection.SemanticDeckId != compilation.Manifest.SemanticDeckId.Value ||
            inspection.SourceLocale != compilation.Manifest.SourceLocale.Value ||
            inspection.ContentVersion != compilation.Manifest.ContentVersion ||
            inspection.SourceDigest != compilation.SourceDigest.Value ||
            inspection.DeclaredLocales != compilation.Manifest.DeclaredLocales.Count ||
            inspection.Labels != compilation.Labels.Sum(static pair => pair.Value.SingleCardSections.Count + pair.Value.ThreeCardPositions.Count + pair.Value.Relations.Count) ||
            inspection.Vocabulary != compilation.Vocabulary.Sum(static pair => pair.Value.Count) ||
            inspection.SingleCards != compilation.SingleCards.Sum(static pair => pair.Value.Count) ||
            inspection.OrientedPairs != compilation.OrientedPairs.Sum(static pair => pair.Value.Count) ||
            inspection.ThreeCardPositions != compilation.ThreeCardPositions.Sum(static pair => pair.Value.Count) ||
            inspection.SynthesisResources != compilation.SynthesisResources.Sum(static pair => pair.Value.Count))
            throw new InvalidDataException("Compiled package is stale or its semantic inventory differs from source.");
    }

    internal static SqliteConnection OpenReadOnly(string packagePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packagePath);
        var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = Path.GetFullPath(packagePath), Mode = SqliteOpenMode.ReadOnly, Cache = SqliteCacheMode.Private, Pooling = false
        }.ToString());
        connection.Open();
        Execute(connection, "PRAGMA query_only=ON; PRAGMA foreign_keys=ON;");
        return connection;
    }

    internal static void VerifyConnection(SqliteConnection connection)
    {
        if (ScalarInt(connection, "PRAGMA application_id") != TarotSqliteSchema.ApplicationId) throw new InvalidDataException("Wrong SQLite application_id.");
        if (ScalarInt(connection, "PRAGMA user_version") != TarotSqliteSchema.UserVersion) throw new InvalidDataException("Wrong SQLite user_version.");
        var quick = ScalarString(connection, "PRAGMA quick_check");
        if (quick != "ok") throw new InvalidDataException($"SQLite quick_check failed: {quick}");
        var required = new[] { "pack_metadata","declared_locale","display_name","module","module_dependency","label","vocabulary","single_card","single_card_reversal_mechanism","single_card_tag","oriented_pair","oriented_pair_tag","three_card_position","three_card_position_tag","synthesis_resource" };
        using var command = connection.CreateCommand(); command.CommandText = "SELECT name FROM sqlite_schema WHERE type='table'";
        using var reader = command.ExecuteReader(); var actual = new HashSet<string>(StringComparer.Ordinal); while (reader.Read()) actual.Add(reader.GetString(0));
        if (required.Any(name => !actual.Contains(name))) throw new InvalidDataException("The .noxinterp schema is incomplete.");
        reader.Close();
        if (ScalarInt(connection, "SELECT count(*) FROM pack_metadata WHERE singleton=1 AND package_schema_version=1") != 1)
            throw new InvalidDataException("Wrong package schema version or metadata cardinality.");
    }

    private static void InsertAll(SqliteConnection c, SqliteTransaction tx, TarotInterpretationCompilation x)
    {
        Insert(c, tx, "INSERT INTO pack_metadata VALUES(1,1,$p,$d,$s,$v,$h)", ("$p",x.Manifest.PackId.Value),("$d",x.Manifest.SemanticDeckId.Value),("$s",x.Manifest.SourceLocale.Value),("$v",x.Manifest.ContentVersion),("$h",x.SourceDigest.Value));
        foreach (var locale in x.Manifest.DeclaredLocales.OrderBy(static item => item.Value, StringComparer.Ordinal))
        {
            Insert(c,tx,"INSERT INTO declared_locale VALUES($l)",( "$l",locale.Value));
            Insert(c,tx,"INSERT INTO display_name VALUES($l,$v)",( "$l",locale.Value),("$v",x.Manifest.DisplayNames[locale]));
        }
        foreach (var mode in x.Manifest.Modules.OrderBy(static item => TarotSchemaText.Get(item.Key,TarotSchemaText.Modes),StringComparer.Ordinal))
        foreach (var locale in mode.Value.OrderBy(static item => item.Key.Value,StringComparer.Ordinal))
        {
            var m=TarotSchemaText.Get(mode.Key,TarotSchemaText.Modes); Insert(c,tx,"INSERT INTO module VALUES($m,$l,$r)",( "$m",m),("$l",locale.Key.Value),("$r",locale.Value.Ready?1:0));
            for(var i=0;i<locale.Value.Dependencies.Count;i++) Insert(c,tx,"INSERT INTO module_dependency VALUES($m,$l,$o,$d)",( "$m",m),("$l",locale.Key.Value),("$o",i),("$d",TarotSchemaText.Get(locale.Value.Dependencies[i],TarotSchemaText.Dependencies)));
        }
        foreach (var locale in x.Labels.OrderBy(static p=>p.Key.Value,StringComparer.Ordinal))
        {
            InsertLabels(c,tx,locale.Key.Value,"single-card-section",locale.Value.SingleCardSections);
            InsertLabels(c,tx,locale.Key.Value,"three-card-position",locale.Value.ThreeCardPositions);
            InsertLabels(c,tx,locale.Key.Value,"relation",locale.Value.Relations);
        }
        foreach(var locale in x.Vocabulary.OrderBy(static p=>p.Key.Value,StringComparer.Ordinal)) foreach(var item in locale.Value.OrderBy(static i=>i.ConceptId.Value,StringComparer.Ordinal))
            Insert(c,tx,"INSERT INTO vocabulary VALUES($l,$c,$v,$m)",( "$l",locale.Key.Value),("$c",item.ConceptId.Value),("$v",item.Label),("$m",item.Meaning));
        foreach(var locale in x.SingleCards.OrderBy(static p=>p.Key.Value,StringComparer.Ordinal)) foreach(var item in locale.Value.OrderBy(static i=>i.CardId.Value,StringComparer.Ordinal).ThenBy(static i=>i.Orientation))
        {
            var o=TarotSchemaText.Get(item.Orientation,TarotSchemaText.CardOrientations); Insert(c,tx,"INSERT INTO single_card VALUES($l,$c,$o,$s,$d,$r,$u,$a,$v,$i)",( "$l",locale.Key.Value),("$c",item.CardId.Value),("$o",o),("$s",item.Sections["situation"]),("$d",item.Sections["development"]),("$r",item.Sections["risk"]),("$u",item.Sections["outcome"]),("$a",item.Sections["advice"]),("$v",item.OverallValence),("$i",item.OverallIntensity));
            for(var n=0;n<item.ReversalMechanisms.Count;n++) Insert(c,tx,"INSERT INTO single_card_reversal_mechanism VALUES($l,$c,$o,$n,$m)",( "$l",locale.Key.Value),("$c",item.CardId.Value),("$o",o),("$n",n),("$m",TarotSchemaText.Get(item.ReversalMechanisms[n],TarotSchemaText.ReversalMechanisms)));
            InsertTags(c,tx,"single_card_tag",locale.Key.Value,[item.CardId.Value,o],item.Tags);
        }
        foreach(var locale in x.OrientedPairs.OrderBy(static p=>p.Key.Value,StringComparer.Ordinal)) foreach(var item in locale.Value.OrderBy(static i=>i.CardAId.Value,StringComparer.Ordinal).ThenBy(static i=>i.CardBId.Value,StringComparer.Ordinal).ThenBy(static i=>i.OrientationState))
        {
            var s=TarotSchemaText.Get(item.OrientationState,TarotSchemaText.PairStates); Insert(c,tx,"INSERT INTO oriented_pair VALUES($l,$a,$b,$s,$i,$d,$v,$n)",( "$l",locale.Key.Value),("$a",item.CardAId.Value),("$b",item.CardBId.Value),("$s",s),("$i",item.Interaction),("$d",item.Direction),("$v",item.OverallValence),("$n",item.OverallIntensity));
            InsertTags(c,tx,"oriented_pair_tag",locale.Key.Value,[item.CardAId.Value,item.CardBId.Value,s],item.Tags);
        }
        foreach(var locale in x.ThreeCardPositions.OrderBy(static p=>p.Key.Value,StringComparer.Ordinal)) foreach(var item in locale.Value.OrderBy(static i=>i.Position).ThenBy(static i=>i.CardId.Value,StringComparer.Ordinal).ThenBy(static i=>i.Orientation))
        {
            var p=TarotSchemaText.Get(item.Position,TarotSchemaText.Positions); var o=TarotSchemaText.Get(item.Orientation,TarotSchemaText.CardOrientations); Insert(c,tx,"INSERT INTO three_card_position VALUES($l,$p,$c,$o,$t,$v,$i)",( "$l",locale.Key.Value),("$p",p),("$c",item.CardId.Value),("$o",o),("$t",item.Text),("$v",item.OverallValence),("$i",item.OverallIntensity));
            InsertTags(c,tx,"three_card_position_tag",locale.Key.Value,[p,item.CardId.Value,o],item.Tags);
        }
        foreach(var locale in x.SynthesisResources.OrderBy(static p=>p.Key.Value,StringComparer.Ordinal)) foreach(var item in locale.Value.OrderBy(static i=>i.ResourceType).ThenBy(static i=>i.ResourceId.Value,StringComparer.Ordinal))
            Insert(c,tx,"INSERT INTO synthesis_resource VALUES($l,$t,$i,$j)",( "$l",locale.Key.Value),("$t",TarotSchemaText.Get(item.ResourceType,TarotSchemaText.SynthesisResourceTypes)),("$i",item.ResourceId.Value),("$j",item.CanonicalJson));
    }

    private static void InsertLabels(SqliteConnection c,SqliteTransaction tx,string locale,string category,IReadOnlyDictionary<string,string> labels){foreach(var p in labels.OrderBy(static p=>p.Key,StringComparer.Ordinal))Insert(c,tx,"INSERT INTO label VALUES($l,$c,$i,$v)",( "$l",locale),("$c",category),("$i",p.Key),("$v",p.Value));}
    private static void InsertTags(SqliteConnection c,SqliteTransaction tx,string table,string locale,string[] keys,IReadOnlyList<TarotTagAssignment> tags){for(var n=0;n<tags.Count;n++){var names=table switch{"single_card_tag"=>new[]{"card_id","orientation"},"oriented_pair_tag"=>new[]{"card_a_id","card_b_id","orientation_state"},_=>new[]{"position","card_id","orientation"}};var cols=string.Join(',',names);var pars=string.Join(',',Enumerable.Range(0,keys.Length).Select(i=>$"$k{i}"));var sql=$"INSERT INTO {table}(locale,{cols},ordinal,concept_id,valence,intensity) VALUES($l,{pars},$o,$c,$v,$i)";var args=new List<(string,object?)>{("$l",locale)};args.AddRange(keys.Select((v,i)=>($"$k{i}",(object?)v)));args.AddRange([("$o",n),("$c",tags[n].ConceptId.Value),("$v",tags[n].Valence),("$i",tags[n].Intensity)]);Insert(c,tx,sql,args.ToArray());}}
    private static void Insert(SqliteConnection c,SqliteTransaction tx,string sql,params (string Name,object? Value)[] args){using var cmd=c.CreateCommand();cmd.Transaction=tx;cmd.CommandText=sql;foreach(var a in args)cmd.Parameters.AddWithValue(a.Name,a.Value??DBNull.Value);cmd.ExecuteNonQuery();}
    private static void Execute(SqliteConnection c,string sql){using var cmd=c.CreateCommand();cmd.CommandText=sql;cmd.ExecuteNonQuery();}
    private static int ScalarInt(SqliteConnection c,string sql){using var cmd=c.CreateCommand();cmd.CommandText=sql;return Convert.ToInt32(cmd.ExecuteScalar());}
    private static string ScalarString(SqliteConnection c,string sql){using var cmd=c.CreateCommand();cmd.CommandText=sql;return Convert.ToString(cmd.ExecuteScalar(),System.Globalization.CultureInfo.InvariantCulture)??string.Empty;}
}

public sealed record TarotSqlitePackageInspection(
    string PackId, string SemanticDeckId, string SourceLocale, int ContentVersion, string SourceDigest,
    int DeclaredLocales, int Labels, int Vocabulary, int SingleCards, int OrientedPairs,
    int ThreeCardPositions, int SynthesisResources);
