using System.Globalization;
using Microsoft.Data.Sqlite;
using NoxAeterna.Interpretation.Sqlite;
using NoxAeterna.Tools.Repository.Interpretation.Compilation;

namespace NoxAeterna.Tests.Tooling.Interpretation;

public sealed class InterpretationPackageCompilerTests
{
    [Fact]
    public void AllFalseSkeletonCompilesWithFrozenMetadataLabelsAndSchemaIdentity()
    {
        using var fixture = InterpretationToolingFixture.CreateSkeleton();
        using var package = TemporaryPackage.Create();

        var report = new InterpretationPackageCompiler().Compile(fixture.Root, package.Path, check: false);
        var inspection = new TarotSqlitePackageWriter().Inspect(package.Path);

        Assert.True(report.Success, Format(report));
        Assert.Equal("classic", inspection.PackId);
        Assert.Equal("standard-78", inspection.SemanticDeckId);
        Assert.Equal("ru", inspection.SourceLocale);
        Assert.Equal(2, inspection.DeclaredLocales);
        Assert.Equal(22, inspection.Labels);
        Assert.Equal(0, inspection.SingleCards);
        Assert.Matches("^[0-9a-f]{64}$", inspection.SourceDigest);
        using var connection = Open(package.Path);
        Assert.Equal(1313822793, TarotSqliteSchema.ApplicationId);
        Assert.Equal(1, TarotSqliteSchema.UserVersion);
        Assert.Equal(1313822793, ScalarInt(connection, "PRAGMA application_id"));
        Assert.Equal(1, ScalarInt(connection, "PRAGMA user_version"));
        Assert.Equal(8, ScalarInt(connection, "SELECT count(*) FROM module WHERE ready=0"));
        Assert.Equal("Classic", ScalarString(connection, "SELECT value FROM display_name WHERE locale='en'"));
        var schema = ScalarString(connection, "SELECT group_concat(sql, char(10)) FROM sqlite_schema WHERE sql IS NOT NULL");
        Assert.DoesNotContain("timestamp", schema, StringComparison.OrdinalIgnoreCase);
        var semanticText = ScalarString(connection, "SELECT group_concat(value, char(10)) FROM (SELECT pack_id AS value FROM pack_metadata UNION ALL SELECT semantic_deck_id FROM pack_metadata UNION ALL SELECT source_locale FROM pack_metadata UNION ALL SELECT value FROM display_name UNION ALL SELECT value FROM label)");
        Assert.DoesNotContain(fixture.Root, semanticText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void DigestAndSemanticInspectionAreStableAcrossCreationOrderAndCulture()
    {
        using var first = InterpretationToolingFixture.CreateSkeleton();
        using var second = InterpretationToolingFixture.CreateSkeleton(reverseCreationOrder: true);
        using var firstPackage = TemporaryPackage.Create();
        using var secondPackage = TemporaryPackage.Create();
        var priorCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");
            Assert.True(new InterpretationPackageCompiler().Compile(first.Root, firstPackage.Path, false).Success);
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("tr-TR");
            Assert.True(new InterpretationPackageCompiler().Compile(second.Root, secondPackage.Path, false).Success);
        }
        finally { CultureInfo.CurrentCulture = priorCulture; }

        var writer = new TarotSqlitePackageWriter();
        Assert.Equal(writer.Inspect(firstPackage.Path), writer.Inspect(secondPackage.Path));
    }

    [Fact]
    public void CheckDetectsStaleSourceAndFailedCompileLeavesExistingTargetUntouched()
    {
        using var fixture = InterpretationToolingFixture.CreateSkeleton();
        using var package = TemporaryPackage.Create();
        var compiler = new InterpretationPackageCompiler();
        Assert.True(compiler.Compile(fixture.Root, package.Path, false).Success);
        var accepted = File.ReadAllBytes(package.Path);

        fixture.AddSingle("ru", "major.fool");
        var stale = compiler.Compile(fixture.Root, package.Path, check: true);
        Assert.False(stale.Success);
        Assert.Equal(accepted, File.ReadAllBytes(package.Path));

        File.WriteAllText(fixture.ManifestPath, "{");
        var invalid = compiler.Compile(fixture.Root, package.Path, check: false);
        Assert.False(invalid.Success);
        Assert.Equal(accepted, File.ReadAllBytes(package.Path));
    }

    [Fact]
    public void InspectCorruptPackageIsControlledFailure()
    {
        using var package = TemporaryPackage.Create();
        File.WriteAllBytes(package.Path, "not sqlite"u8.ToArray());
        var report = new InterpretationPackageCompiler().Inspect(package.Path);
        Assert.False(report.Success);
        Assert.Contains(report.Diagnostics, item => item.Code == "package.inspect");
    }

    private static SqliteConnection Open(string path)
    {
        var connection = new SqliteConnection($"Data Source={path};Mode=ReadOnly;Pooling=False");
        connection.Open();
        return connection;
    }
    private static int ScalarInt(SqliteConnection connection, string sql) { using var command = connection.CreateCommand(); command.CommandText = sql; return Convert.ToInt32(command.ExecuteScalar()); }
    private static string ScalarString(SqliteConnection connection, string sql) { using var command = connection.CreateCommand(); command.CommandText = sql; return Convert.ToString(command.ExecuteScalar(), CultureInfo.InvariantCulture)!; }
    private static string Format(NoxAeterna.Tools.Repository.Interpretation.Reports.InterpretationToolReport report) => string.Join(Environment.NewLine, report.Diagnostics.Select(item => $"{item.Code}: {item.Message}"));

    private sealed class TemporaryPackage : IDisposable
    {
        private TemporaryPackage(string path) => Path = path;
        public string Path { get; }
        public static TemporaryPackage Create() => new(System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"NoxAeterna-package-{Guid.NewGuid():N}.noxinterp"));
        public void Dispose() { if (File.Exists(Path)) File.Delete(Path); }
    }
}
