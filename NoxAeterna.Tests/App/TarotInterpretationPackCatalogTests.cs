using NoxAeterna.App.Tarot;
using NoxAeterna.Domain.Tarot;
using NoxAeterna.Interpretation.Tarot.Contracts;
using NoxAeterna.Presentation.Localization;
using NoxAeterna.Presentation.Tarot;
using NoxAeterna.Tests.Interpretation.Tarot;

namespace NoxAeterna.Tests.App;

public sealed class TarotInterpretationPackCatalogTests
{
    [Fact]
    public void BuiltInCatalog_ExposesSelectableClassicWithManifestOwnedRuAndEnNames()
    {
        var composition = TarotInterpretationComposition.CreateBuiltIn();

        var option = Assert.Single(composition.PackCatalog.Options);
        Assert.Equal(new TarotInterpretationPackId("classic"), option.Id);
        Assert.Equal("Классика", composition.PackCatalog.ResolveDisplayName(option.Id, new LanguageCode("ru")));
        Assert.Equal("Classic", composition.PackCatalog.ResolveDisplayName(option.Id, new LanguageCode("en")));
        Assert.Empty(composition.PackCatalog.Diagnostics);
    }

    [Fact]
    public void AllFalseIncompleteManifest_RemainsVisibleAndSelectable()
    {
        var source = new TarotInterpretationResolverTestSource();
        source.PublishManifest();

        var catalog = Catalog(source);

        Assert.Equal(source.PackId, Assert.Single(catalog.Options).Id);
        Assert.All(
            source.Manifest.Modules!.Values.SelectMany(static modes => modes!.Values),
            module => Assert.False(module!.Ready));
    }

    [Fact]
    public void UnsupportedUiLocale_FallsBackToEnglishThenRussianThenPackId()
    {
        var bilingual = new TarotInterpretationResolverTestSource("bilingual");
        bilingual.PublishManifest();
        var russian = SourceWithLocales("russian", ("ru", "Русский пакет"));
        var french = SourceWithLocales("french", ("fr", "Paquet français"));

        Assert.Equal("Classic", Catalog(bilingual).ResolveDisplayName(bilingual.PackId, new("zh")));
        Assert.Equal("Русский пакет", Catalog(russian).ResolveDisplayName(russian.PackId, new("zh")));
        Assert.Equal("french", Catalog(french).ResolveDisplayName(french.PackId, new("zh")));
    }

    [Fact]
    public void DisplayName_DependsOnlyOnUiLocaleAndCatalogIsImmutable()
    {
        var source = new TarotInterpretationResolverTestSource();
        var catalog = Catalog(source);
        var options = Assert.IsAssignableFrom<ICollection<TarotInterpretationPackOption>>(catalog.Options);

        Assert.Equal("Классика", catalog.ResolveDisplayName(source.PackId, new("ru")));
        Assert.Equal("Classic", catalog.ResolveDisplayName(source.PackId, new("en")));
        Assert.Throws<NotSupportedException>(() => options.Add(new(new("other"))));
        Assert.Single(catalog.AvailablePackIds);
    }

    [Fact]
    public void DuplicatePackIds_AreRejectedBeforeSourceReads()
    {
        var source = new TarotInterpretationResolverTestSource();

        var exception = Assert.Throws<ArgumentException>(() => new TarotInterpretationPackCatalog(
            source,
            [source.PackId, source.PackId]));

        Assert.Equal("sourceIds", exception.ParamName);
        Assert.Empty(source.Reads);
    }

    [Fact]
    public void InvalidSource_ProducesOnlyInternalDiagnosticAndNoUserFacingOption()
    {
        var source = new TarotInterpretationResolverTestSource();
        source.Remove("interpretation-pack.json");

        var catalog = Catalog(source);

        Assert.Empty(catalog.Options);
        Assert.Empty(catalog.AvailablePackIds);
        Assert.Equal("manifest.missing", Assert.Single(catalog.Diagnostics).Code);
        Assert.Equal("classic", catalog.ResolveDisplayName(source.PackId, new("ru")));
    }

    [Fact]
    public void MissingDisplayName_KeepsOptionAndSilentlyUsesRemainingFallbackOrPackId()
    {
        var russianOnlyName = new TarotInterpretationResolverTestSource();
        russianOnlyName.Manifest.DisplayNames!.Remove("en");
        russianOnlyName.PublishManifest();
        var noNames = new TarotInterpretationResolverTestSource("nameless");
        noNames.Manifest.DisplayNames!.Clear();
        noNames.PublishManifest();

        var russianCatalog = Catalog(russianOnlyName);
        var namelessCatalog = Catalog(noNames);

        Assert.Equal("Классика", russianCatalog.ResolveDisplayName(russianOnlyName.PackId, new("zh")));
        Assert.Equal("Классика", russianCatalog.ResolveDisplayName(russianOnlyName.PackId, new("en")));
        Assert.Equal("nameless", namelessCatalog.ResolveDisplayName(noNames.PackId, new("zh")));
        Assert.Single(russianCatalog.Options);
        Assert.Single(namelessCatalog.Options);
        Assert.Contains(russianCatalog.Diagnostics, item => item.Code == "manifest.display-name-fallback");
    }

    private static TarotInterpretationPackCatalog Catalog(TarotInterpretationResolverTestSource source) =>
        new(source, [source.PackId]);

    private static TarotInterpretationResolverTestSource SourceWithLocales(
        string packId,
        params (string Locale, string Name)[] locales)
    {
        var source = new TarotInterpretationResolverTestSource(packId);
        var document = source.Manifest;
        document.SourceLocale = locales[0].Locale;
        document.DeclaredLocales = locales.Select(static item => (string?)item.Locale).ToList();
        document.DisplayNames = locales.ToDictionary(
            static item => item.Locale,
            static item => (string?)item.Name,
            StringComparer.Ordinal);
        foreach (var mode in document.Modules!.Keys.ToArray())
        {
            var template = document.Modules[mode]!["ru"]!;
            document.Modules[mode] = locales.ToDictionary(
                static item => item.Locale,
                item => (TarotInterpretationModuleDocument?)new TarotInterpretationModuleDocument
                {
                    Ready = false,
                    IndexPaths = template.IndexPaths!
                        .Select(path => (string?)path!.Replace("/ru/", $"/{item.Locale}/", StringComparison.Ordinal))
                        .ToList(),
                    Dependencies = template.Dependencies!.ToList()
                },
                StringComparer.Ordinal);
        }

        source.PublishManifest();
        return source;
    }
}
