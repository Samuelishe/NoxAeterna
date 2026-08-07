using NoxAeterna.Interpretation.Tarot.Contracts;
using NoxAeterna.Interpretation.Tarot.Validation;

namespace NoxAeterna.Tests.Interpretation.Tarot;

public sealed class TarotSourceManifestV2Tests
{
    [Fact]
    public void SchemaV2KeepsSemanticMetadataAndExactModuleDependenciesWithoutIndexes()
    {
        var document = Manifest();
        var result = TarotInterpretationValidator.ValidateManifest(document);

        Assert.True(result.IsValid, Format(result.Diagnostics));
        Assert.Equal(2, result.Value!.SchemaVersion);
        Assert.Equal("classic", result.Value.PackId.Value);
        Assert.Equal("standard-78", result.Value.SemanticDeckId.Value);
        Assert.Equal("ru", result.Value.SourceLocale.Value);
        Assert.Equal("Классика", result.Value.DisplayNames[new("ru")]);
        Assert.Equal([TarotModuleDependency.OrientedPairs], result.Value.Modules[TarotInterpretationMode.TwoCards][new("ru")].Dependencies);
        Assert.Equal(3, result.Value.Modules[TarotInterpretationMode.ThreeCards][new("en")].Dependencies.Count);
        Assert.All(result.Value.Modules.Values.SelectMany(static value => value.Values), module => Assert.False(module.Ready));
    }

    [Theory]
    [InlineData("version", "schema.version")]
    [InlineData("dependency", "manifest.dependency-contract")]
    [InlineData("extra-mode", "manifest.mode-extra")]
    [InlineData("extra-locale", "manifest.module-locale-extra")]
    public void InvalidManifestV2IsRejected(string mutation, string expected)
    {
        var document = Manifest();
        switch (mutation)
        {
            case "version": document.SchemaVersion = 1; break;
            case "dependency": document.Modules!["two-cards"]!["ru"]!.Dependencies = []; break;
            case "extra-mode": document.Modules!["other"] = new(); break;
            case "extra-locale": document.Modules!["single-card"]!["zh"] = new() { Ready = false, Dependencies = [] }; break;
        }
        var result = TarotInterpretationValidator.ValidateManifest(document);
        Assert.False(result.IsValid);
        Assert.Contains(result.Diagnostics, item => item.Code == expected);
    }

    [Fact]
    public void LabelsRequireExactFrozenSectionPositionAndRelationIds()
    {
        var valid = Labels();
        var accepted = TarotInterpretationValidator.ValidateLabels(valid);
        Assert.True(accepted.IsValid, Format(accepted.Diagnostics));
        Assert.Equal(5, accepted.Value!.SingleCardSections.Count);
        Assert.Equal(3, accepted.Value.ThreeCardPositions.Count);
        Assert.Equal(3, accepted.Value.Relations.Count);

        valid.Relations!.Remove("overall");
        valid.SingleCardSections!["unknown"] = "Unknown";
        var rejected = TarotInterpretationValidator.ValidateLabels(valid);
        Assert.False(rejected.IsValid);
        Assert.Equal(2, rejected.Diagnostics.Count(item => item.Code == "bundle.state-membership"));
    }

    private static TarotInterpretationPackDocument Manifest()
    {
        var locales = new[] { "ru", "en" };
        var modules = new Dictionary<string, Dictionary<string, TarotInterpretationModuleDocument?>?>(StringComparer.Ordinal);
        foreach (var mode in new[] { "single-card", "two-cards", "three-cards", "celtic-cross" })
            modules[mode] = locales.ToDictionary(locale => locale, locale => (TarotInterpretationModuleDocument?)new()
            {
                Ready = false,
                Dependencies = mode switch
                {
                    "two-cards" => [TarotModuleDependency.OrientedPairs],
                    "three-cards" => [TarotModuleDependency.OrientedPairs, TarotModuleDependency.ThreeCardPositions, TarotModuleDependency.ThreeCardSynthesis],
                    _ => []
                }
            }, StringComparer.Ordinal);
        return new()
        {
            SchemaVersion = 2, PackId = "classic", SemanticDeckId = "standard-78", SourceLocale = "ru", ContentVersion = 1,
            DeclaredLocales = ["ru", "en"], DisplayNames = new(StringComparer.Ordinal) { ["ru"] = "Классика", ["en"] = "Classic" }, Modules = modules
        };
    }

    private static TarotLabelsDocument Labels() => new()
    {
        SchemaVersion = 1,
        SingleCardSections = Map("situation", "development", "risk", "outcome", "advice"),
        ThreeCardPositions = Map("past", "present", "future"), Relations = Map("past-present", "present-future", "overall")
    };
    private static Dictionary<string, string?> Map(params string[] ids) => ids.ToDictionary(id => id, id => (string?)id, StringComparer.Ordinal);
    private static string Format(IEnumerable<TarotValidationDiagnostic> diagnostics) => string.Join(Environment.NewLine, diagnostics.Select(item => $"{item.Code}: {item.Message}"));
}
