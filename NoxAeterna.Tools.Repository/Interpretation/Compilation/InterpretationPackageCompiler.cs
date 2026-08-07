using NoxAeterna.Interpretation.Sqlite;
using NoxAeterna.Tools.Repository.Interpretation.Analysis;
using NoxAeterna.Tools.Repository.Interpretation.Reports;

namespace NoxAeterna.Tools.Repository.Interpretation.Compilation;

public sealed class InterpretationPackageCompiler
{
    public InterpretationToolReport Compile(string sourceRoot, string outputPath, bool check)
    {
        var loaded = new InterpretationSourceLoader().Load(sourceRoot);
        if (!loaded.Report.Success || loaded.Compilation is null) return loaded.Report;
        try
        {
            var writer = new TarotSqlitePackageWriter();
            if (check)
            {
                writer.Check(loaded.Compilation, outputPath);
                return new InterpretationToolReport([], loaded.Report.Counts,
                    new Dictionary<string,string> { ["sourceDigest"] = loaded.Compilation.SourceDigest.Value });
            }
            writer.Write(loaded.Compilation, outputPath);
            return new InterpretationToolReport([], loaded.Report.Counts,
                new Dictionary<string,string> { ["sourceDigest"] = loaded.Compilation.SourceDigest.Value },
                [Path.GetFullPath(outputPath)]);
        }
        catch (Exception exception) when (exception is IOException or InvalidDataException or Microsoft.Data.Sqlite.SqliteException)
        {
            return new InterpretationToolReport([new("package.compile", InterpretationToolSeverity.Error, outputPath, exception.Message)], loaded.Report.Counts);
        }
    }

    public InterpretationToolReport Inspect(string packagePath)
    {
        try
        {
            var value = new TarotSqlitePackageWriter().Inspect(packagePath);
            return new InterpretationToolReport([], new Dictionary<string,int>
            {
                ["declaredLocales"] = value.DeclaredLocales, ["labels"] = value.Labels,
                ["vocabulary"] = value.Vocabulary, ["singleCardStates"] = value.SingleCards,
                ["orientedPairStates"] = value.OrientedPairs, ["threeCardPositionStates"] = value.ThreeCardPositions,
                ["synthesisResources"] = value.SynthesisResources
            }, new Dictionary<string,string>
            {
                ["packId"] = value.PackId, ["semanticDeckId"] = value.SemanticDeckId,
                ["sourceLocale"] = value.SourceLocale, ["contentVersion"] = value.ContentVersion.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["sourceDigest"] = value.SourceDigest, ["applicationId"] = TarotSqliteSchema.ApplicationId.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["userVersion"] = TarotSqliteSchema.UserVersion.ToString(System.Globalization.CultureInfo.InvariantCulture)
            });
        }
        catch (Exception exception) when (exception is IOException or InvalidDataException or Microsoft.Data.Sqlite.SqliteException)
        {
            return new InterpretationToolReport([new("package.inspect", InterpretationToolSeverity.Error, packagePath, exception.Message)]);
        }
    }
}

