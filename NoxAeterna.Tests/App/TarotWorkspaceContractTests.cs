using System.Xml.Linq;
using NoxAeterna.App.Tarot;

namespace NoxAeterna.Tests.App;

public sealed class TarotWorkspaceContractTests
{
    [Fact]
    public void TarotSection_MaterializesRealWorkspaceInsteadOfPlaceholder()
    {
        var source = File.ReadAllText(AppPath("MainWindow.axaml.cs"));
        var tarotBranch = source.IndexOf("currentItem.Id == ShellSectionId.Tarot", StringComparison.Ordinal);
        var placeholder = source.IndexOf("ui.shell.placeholder.caption", tarotBranch, StringComparison.Ordinal);

        Assert.True(tarotBranch >= 0);
        Assert.Contains("new TarotWorkspaceControl(", source[tarotBranch..placeholder], StringComparison.Ordinal);
        Assert.DoesNotContain("new TextBlock", source[tarotBranch..placeholder], StringComparison.Ordinal);
    }

    [Fact]
    public void CompositionRoot_UsesDomainDrawEngineWithInfrastructureRandomAdapter()
    {
        var source = File.ReadAllText(AppPath("MainWindow.axaml.cs"));

        Assert.Contains("TarotWorkspaceViewModel.CreateFoundation", source, StringComparison.Ordinal);
        Assert.Contains("ITarotRandomSource tarotRandomSource = new SystemTarotRandomSource();", source, StringComparison.Ordinal);
        Assert.Contains("new TarotDrawEngine(tarotRandomSource)", source, StringComparison.Ordinal);
        Assert.Contains("DebugTarotSmokeRandomSource.CreateFromEnvironment() ?? tarotRandomSource", source, StringComparison.Ordinal);
        Assert.Contains("SystemClock.Instance.GetCurrentInstant", source, StringComparison.Ordinal);
    }

    [Fact]
    public void CardVisuals_AreAccessibleAndComposeRasterOrPrototypeBehindOneRotationContract()
    {
        var source = File.ReadAllText(AppPath("Tarot", "TarotWorkspaceControl.cs"));
        var geometry = File.ReadAllText(AppPath("Tarot", "TarotPrototypeGeometryCatalog.cs"));

        Assert.Contains("new Button", source, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.SetName", source, StringComparison.Ordinal);
        Assert.Contains("new RotateTransform(plan.RotationDegrees)", source, StringComparison.Ordinal);
        Assert.Contains("AvaloniaGeometry.Parse", source, StringComparison.Ordinal);
        Assert.Contains("new Image", source, StringComparison.Ordinal);
        Assert.Contains("var titleOverlay", source, StringComparison.Ordinal);
        Assert.Contains("visual,", source, StringComparison.Ordinal);
        Assert.Contains("titleOverlay,", source, StringComparison.Ordinal);
        Assert.DoesNotContain("FontFamily", source, StringComparison.Ordinal);
        Assert.DoesNotContain("http://", geometry, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("https://", geometry, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void BlackSunAndLunarSeal_HaveDistinctProjectOwnedVectorGeometry()
    {
        var blackSun = TarotPrototypeGeometryCatalog.GetBackPathData(new("black-sun"));
        var lunarSeal = TarotPrototypeGeometryCatalog.GetBackPathData(new("lunar-seal"));

        Assert.NotEqual(blackSun, lunarSeal);
        Assert.StartsWith("M", blackSun, StringComparison.Ordinal);
        Assert.StartsWith("M", lunarSeal, StringComparison.Ordinal);
        Assert.DoesNotContain("http", blackSun, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("http", lunarSeal, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TarotControlStyles_OwnHoverPressedSelectedAndFocusStates()
    {
        var styles = File.ReadAllText(AppPath("Themes", "SemanticControlStyles.axaml"));

        Assert.Contains("Button.tarot-card:pointerover", styles, StringComparison.Ordinal);
        Assert.Contains("Button.tarot-card:pressed", styles, StringComparison.Ordinal);
        Assert.Contains("Button.tarot-card.selected", styles, StringComparison.Ordinal);
        Assert.Contains("Button.tarot-card:focus-visible", styles, StringComparison.Ordinal);
        Assert.Contains("DesignFocusRingBrush", styles, StringComparison.Ordinal);
        Assert.DoesNotContain("Salmon", styles, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TarotWorkspace_AddsNoExternalAssetFontIconOrPackageDependency()
    {
        var project = XDocument.Load(AppPath("NoxAeterna.App.csproj"));
        var packageNames = project.Descendants("PackageReference")
            .Select(element => (string?)element.Attribute("Include"))
            .Where(static value => value is not null)
            .Cast<string>()
            .Order(StringComparer.Ordinal)
            .ToArray();
        var tarotFiles = Directory.GetFiles(AppPath("Tarot"), "*", SearchOption.AllDirectories);

        Assert.Equal(new[] { "Avalonia", "Avalonia.Desktop", "Avalonia.Themes.Fluent" }, packageNames);
        Assert.All(tarotFiles, path => Assert.EndsWith(".cs", path, StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(tarotFiles, path =>
            path.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(".svg", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(".ttf", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void InterpretationUnavailableCopy_LivesOnlyInLocalizationCatalogs()
    {
        var productionSources = Directory.GetFiles(AppPath("Tarot"), "*.cs", SearchOption.AllDirectories)
            .Concat(Directory.GetFiles(PresentationPath("Tarot"), "*.cs", SearchOption.AllDirectories))
            .Select(File.ReadAllText)
            .ToArray();

        Assert.All(productionSources, source =>
        {
            Assert.DoesNotContain("Толкование для выбранного набора", source, StringComparison.Ordinal);
            Assert.DoesNotContain("Interpretation for the selected set", source, StringComparison.Ordinal);
        });
    }

    [Fact]
    public void ArtworkSelector_RemainsVisibleWithSoleUserFacingLupusNoctisOption()
    {
        var source = File.ReadAllText(AppPath("Tarot", "TarotWorkspaceControl.cs"));

        Assert.Contains("Name = \"TarotArtworkSelector\"", source, StringComparison.Ordinal);
        Assert.Contains("ItemsSource = viewModel.ArtworkPacks", source, StringComparison.Ordinal);
        Assert.Contains("viewModel.SelectArtworkPack(selected.Option.Id)", source, StringComparison.Ordinal);
        Assert.Contains("IsVisible = true", source, StringComparison.Ordinal);
        Assert.Contains("controls.Children.Add(CreateLabeledControl(\"ui.tarot.control.artwork\", artworkSelector))", source, StringComparison.Ordinal);
        Assert.DoesNotContain("ui.tarot.prototype.notice", source, StringComparison.Ordinal);
        Assert.DoesNotContain("ui.tarot.artwork.partial-notice", source, StringComparison.Ordinal);
        Assert.DoesNotContain("ui.tarot.artwork.fallback", source, StringComparison.Ordinal);
        Assert.DoesNotContain("ForceCard", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Cheat", source, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RequiredArtworkFailure_DisablesDrawingAndShowsLocalizedDiagnosticWithoutClassicFallback()
    {
        var source = File.ReadAllText(AppPath("Tarot", "TarotWorkspaceControl.cs"));

        Assert.Contains("IsEnabled = artworkCatalog.IsReady", source, StringComparison.Ordinal);
        Assert.Contains("Localize(\"ui.tarot.artwork.unavailable\")", source, StringComparison.Ordinal);
        Assert.Contains("artworkDiagnostic.IsVisible = !artworkCatalog.IsReady", source, StringComparison.Ordinal);
        Assert.DoesNotContain("PrototypeArtworkPackId", source, StringComparison.Ordinal);
        Assert.DoesNotContain("prototype-symbolic", source, StringComparison.Ordinal);
    }

    [Fact]
    public void AppProject_PackagesOnlyStableLupusNoctisProductionResources()
    {
        var project = XDocument.Load(AppPath("NoxAeterna.App.csproj"));
        var resourceIncludes = project.Descendants("None")
            .Select(element => (string?)element.Attribute("Include"))
            .Where(static value => value is not null)
            .Cast<string>()
            .Where(value => value.Contains("lupus-noctis", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        Assert.Equal(2, resourceIncludes.Length);
        Assert.Contains(resourceIncludes, include => include.EndsWith("artwork-pack.json", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(resourceIncludes, include => include.Contains("cards\\**\\*.png", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(resourceIncludes, include => include.Contains("studies", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(resourceIncludes, Path.IsPathRooted);

        var manifestItem = project.Descendants("None").Single(element =>
            ((string?)element.Attribute("Include"))?.EndsWith("artwork-pack.json", StringComparison.OrdinalIgnoreCase) == true);
        var cardsItem = project.Descendants("None").Single(element =>
            ((string?)element.Attribute("Include"))?.Contains("cards\\**\\*.png", StringComparison.OrdinalIgnoreCase) == true);
        Assert.Equal("Always", (string?)manifestItem.Attribute("CopyToOutputDirectory"));
        Assert.Equal("Always", (string?)manifestItem.Attribute("CopyToPublishDirectory"));
        Assert.Equal("PreserveNewest", (string?)cardsItem.Attribute("CopyToOutputDirectory"));
        Assert.Equal("PreserveNewest", (string?)cardsItem.Attribute("CopyToPublishDirectory"));
    }

    private static string AppPath(params string[] segments) => RepositoryPath("NoxAeterna.App", segments);

    private static string PresentationPath(params string[] segments) => RepositoryPath("NoxAeterna.Presentation", segments);

    private static string RepositoryPath(string project, params string[] segments)
    {
        var pathSegments = new[] { AppContext.BaseDirectory, "..", "..", "..", "..", project }
            .Concat(segments)
            .ToArray();
        return Path.GetFullPath(Path.Combine(pathSegments));
    }
}
