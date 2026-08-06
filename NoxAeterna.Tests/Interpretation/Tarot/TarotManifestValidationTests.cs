using NoxAeterna.Interpretation.Tarot.Contracts;
using NoxAeterna.Interpretation.Tarot.Validation;

namespace NoxAeterna.Tests.Interpretation.Tarot;

public sealed class TarotManifestValidationTests
{
    [Fact]
    public void AllNotReadyRuEnManifest_ValidatesToImmutableContracts()
    {
        var document = TarotInterpretationTestDocuments.Manifest();

        var result = TarotInterpretationValidator.ValidateManifest(document);

        Assert.True(result.IsValid, Format(result.Diagnostics));
        Assert.Empty(result.Diagnostics);
        var manifest = Assert.IsType<TarotInterpretationPackManifest>(result.Value);
        Assert.Equal("classic", manifest.PackId.Value);
        Assert.Equal("standard-78", manifest.SemanticDeckId.Value);
        Assert.Equal("ru", manifest.SourceLocale.Value);
        Assert.Equal(4, manifest.Modules.Count);
        Assert.All(manifest.Modules.Values, modules => Assert.Equal(2, modules.Count));
        Assert.Empty(manifest.IndexFiles);
        document.DeclaredLocales!.Clear();
        document.DisplayNames!["ru"] = "changed";
        Assert.Equal(new[] { "ru", "en" }, manifest.DeclaredLocales.Select(locale => locale.Value));
        Assert.Equal("Классика", manifest.DisplayNames[new("ru")]);
        Assert.Throws<NotSupportedException>(() =>
            ((ICollection<TarotInterpretationLocale>)manifest.DeclaredLocales).Add(new("zh")));
    }

    [Fact]
    public void ReadyModule_ValidatesWhenEveryDeclaredIndexIsReferenced()
    {
        var result = TarotInterpretationValidator.ValidateManifest(
            TarotInterpretationTestDocuments.Manifest(readySingleCard: true));

        Assert.True(result.IsValid, Format(result.Diagnostics));
        Assert.True(result.Value!.Modules[TarotInterpretationMode.SingleCard][new("ru")].Ready);
        Assert.True(result.Value.Modules[TarotInterpretationMode.SingleCard][new("en")].Ready);
    }

    [Theory]
    [InlineData("duplicate-locale", "manifest.locale-duplicate")]
    [InlineData("missing-source-locale", "manifest.source-locale")]
    [InlineData("missing-display-name", "manifest.display-name")]
    [InlineData("missing-mode", "manifest.mode-missing")]
    [InlineData("missing-locale-module", "manifest.module-missing")]
    [InlineData("unsafe-path", "value.invalid")]
    [InlineData("backslash-path", "value.invalid")]
    [InlineData("invalid-hash", "value.invalid")]
    [InlineData("duplicate-index-path", "manifest.index-path-duplicate")]
    [InlineData("unknown-dependency", "manifest.dependency")]
    [InlineData("duplicate-dependency", "manifest.dependency-duplicate")]
    [InlineData("mixed-locale-casing", "manifest.module-locale")]
    [InlineData("ready-index-absent", "manifest.ready-index")]
    [InlineData("wrong-mode-contract", "manifest.index-contract")]
    public void InvalidManifestContracts_ReturnTypedDiagnostic(string mutation, string expectedCode)
    {
        var document = TarotInterpretationTestDocuments.Manifest();
        Mutate(document, mutation);

        var result = TarotInterpretationValidator.ValidateManifest(document);

        Assert.False(result.IsValid);
        Assert.Null(result.Value);
        Assert.Contains(result.Diagnostics, item =>
            item.Code == expectedCode && item.Severity == TarotValidationSeverity.Error);
    }

    [Fact]
    public void UnsupportedSchemaVersion_IsDistinctStructuralError()
    {
        var document = TarotInterpretationTestDocuments.Manifest();
        document.SchemaVersion = 2;

        var result = TarotInterpretationValidator.ValidateManifest(document);

        var diagnostic = Assert.Single(result.Diagnostics, item => item.Code == "schema.unsupported");
        Assert.Equal("schemaVersion", diagnostic.Field);
        Assert.False(result.IsValid);
    }

    private static void Mutate(TarotInterpretationPackDocument document, string mutation)
    {
        switch (mutation)
        {
            case "duplicate-locale":
                document.DeclaredLocales!.Add("ru");
                break;
            case "missing-source-locale":
                document.SourceLocale = "zh";
                break;
            case "missing-display-name":
                document.DisplayNames!.Remove("en");
                break;
            case "missing-mode":
                document.Modules!.Remove("celtic-cross");
                break;
            case "missing-locale-module":
                document.Modules!["three-cards"]!.Remove("en");
                break;
            case "unsafe-path":
                Module(document, "single-card", "ru").IndexPaths![0] = "../single-card.json";
                break;
            case "backslash-path":
                Module(document, "single-card", "ru").IndexPaths![0] = "indexes\\ru\\single-card.json";
                break;
            case "invalid-hash":
                document.IndexFiles!.Add(new TarotInterpretationIndexFileDocument
                {
                    Path = "indexes/ru/single-card.json",
                    Sha256 = "ABC"
                });
                break;
            case "duplicate-index-path":
                document.IndexFiles!.Add(new TarotInterpretationIndexFileDocument
                {
                    Path = "indexes/ru/single-card.json",
                    Sha256 = TarotInterpretationTestDocuments.Hash
                });
                document.IndexFiles.Add(new TarotInterpretationIndexFileDocument
                {
                    Path = "indexes/ru/single-card.json",
                    Sha256 = TarotInterpretationTestDocuments.Hash
                });
                break;
            case "unknown-dependency":
                Module(document, "two-cards", "ru").Dependencies = [(TarotModuleDependency)99];
                break;
            case "duplicate-dependency":
                Module(document, "two-cards", "ru").Dependencies =
                    [TarotModuleDependency.OrientedPairs, TarotModuleDependency.OrientedPairs];
                break;
            case "mixed-locale-casing":
                document.Modules!["single-card"]!["RU"] = Module(document, "single-card", "ru");
                break;
            case "ready-index-absent":
                Module(document, "single-card", "ru").Ready = true;
                break;
            case "wrong-mode-contract":
                Module(document, "three-cards", "ru").IndexPaths = ["indexes/ru/three-cards.json"];
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(mutation));
        }
    }

    private static TarotInterpretationModuleDocument Module(
        TarotInterpretationPackDocument document,
        string mode,
        string locale) => document.Modules![mode]![locale]!;

    private static string Format(IEnumerable<TarotValidationDiagnostic> diagnostics) =>
        string.Join(Environment.NewLine, diagnostics.Select(item => $"{item.Code}: {item.Message}"));
}
