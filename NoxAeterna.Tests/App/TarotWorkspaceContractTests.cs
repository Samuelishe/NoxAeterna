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

        Assert.Contains("TarotWorkspaceViewModel.CreateClassic", source, StringComparison.Ordinal);
        Assert.Contains("ITarotRandomSource tarotRandomSource = new SystemTarotRandomSource();", source, StringComparison.Ordinal);
        Assert.Contains("new TarotDrawEngine(tarotRandomSource)", source, StringComparison.Ordinal);
        Assert.Contains("DebugTarotSmokeRandomSource.CreateFromEnvironment() ?? tarotRandomSource", source, StringComparison.Ordinal);
        Assert.Contains("SystemClock.Instance.GetCurrentInstant", source, StringComparison.Ordinal);
    }

    [Fact]
    public void TarotWorkspace_UsesFixedControlPanelAndStretchingUnifiedReadingSurface()
    {
        var source = File.ReadAllText(AppPath("Tarot", "TarotWorkspaceControl.cs"));

        Assert.Contains("var root = new Grid", source, StringComparison.Ordinal);
        Assert.Contains("RowDefinitions = new RowDefinitions(\"Auto,*\")", source, StringComparison.Ordinal);
        Assert.Contains("var controlPanel = CreateControlPanel();", source, StringComparison.Ordinal);
        Assert.Contains("var readingSurface = CreateReadingSurface();", source, StringComparison.Ordinal);
        Assert.Contains("Grid.SetRow(readingSurface, 1);", source, StringComparison.Ordinal);
        Assert.Contains("VerticalAlignment = VerticalAlignment.Stretch", source, StringComparison.Ordinal);
        Assert.DoesNotContain("return new ScrollViewer", MethodSlice(source, "private Control BuildContent()", "private Control CreateControlPanel()"), StringComparison.Ordinal);
    }

    [Fact]
    public void ReadingSurfaceOwnsOnlyVerticalScrollAndTableauOwnsOnlyHorizontalScroll()
    {
        var source = File.ReadAllText(AppPath("Tarot", "TarotWorkspaceControl.cs"));
        var readingSurface = MethodSlice(source, "private Control CreateReadingSurface()", "private Control CreateLabeledControl");

        Assert.Contains("Name = \"TarotTableauScrollViewer\"", readingSurface, StringComparison.Ordinal);
        Assert.Contains("HorizontalScrollBarVisibility = ScrollBarVisibility.Auto", readingSurface, StringComparison.Ordinal);
        Assert.Contains("VerticalScrollBarVisibility = ScrollBarVisibility.Disabled", readingSurface, StringComparison.Ordinal);
        Assert.Contains("Name = \"TarotReadingScrollViewer\"", readingSurface, StringComparison.Ordinal);
        Assert.Contains("VerticalScrollBarVisibility = ScrollBarVisibility.Auto", readingSurface, StringComparison.Ordinal);
        Assert.Contains("HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled", readingSurface, StringComparison.Ordinal);
        Assert.Equal(1, CountOccurrences(source, "VerticalScrollBarVisibility = ScrollBarVisibility.Auto"));
        Assert.Equal(1, CountOccurrences(source, "HorizontalScrollBarVisibility = ScrollBarVisibility.Auto"));
    }

    [Fact]
    public void TableauAndInterpretationHost_ShareOneOrderedReadingSurface()
    {
        var source = File.ReadAllText(AppPath("Tarot", "TarotWorkspaceControl.cs"));
        var readingSurface = MethodSlice(source, "private Control CreateReadingSurface()", "private Control CreateLabeledControl");
        var tableauIndex = readingSurface.IndexOf("tableauScrollViewer,", StringComparison.Ordinal);
        var interpretationIndex = readingSurface.IndexOf("interpretationHost", tableauIndex, StringComparison.Ordinal);

        Assert.True(tableauIndex >= 0);
        Assert.True(interpretationIndex > tableauIndex);
        Assert.Contains("HorizontalContentAlignment = HorizontalAlignment.Stretch", source, StringComparison.Ordinal);
    }

    [Fact]
    public void SelectedCardInspector_IsNotMaterializedInProductionUi()
    {
        var source = File.ReadAllText(AppPath("Tarot", "TarotWorkspaceControl.cs"));

        Assert.DoesNotContain("inspectorHost", source, StringComparison.Ordinal);
        Assert.DoesNotContain("RefreshInspector", source, StringComparison.Ordinal);
        Assert.DoesNotContain("CreateInspectorRow", source, StringComparison.Ordinal);
        Assert.DoesNotContain("tarot-inspector", source, StringComparison.Ordinal);
        Assert.DoesNotContain("ui.tarot.inspector.", source, StringComparison.Ordinal);
    }

    [Fact]
    public void ReadingSurface_HasNoVisibleTableauOrInterpretationHeading()
    {
        var source = File.ReadAllText(AppPath("Tarot", "TarotWorkspaceControl.cs"));

        Assert.DoesNotContain("Text = Localize(\"ui.tarot.tableau.title\")", source, StringComparison.Ordinal);
        Assert.DoesNotContain("ui.tarot.interpretation.title", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Interpretation\"", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Толкование", source, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.SetName(tableauScrollViewer, Localize(\"ui.tarot.tableau.title\"))", source, StringComparison.Ordinal);
        Assert.Contains("CreateLabeledControl(\"ui.tarot.control.spread\", spreadSelector)", source, StringComparison.Ordinal);
    }

    [Fact]
    public void InterpretationHost_UsesCoordinatorPresentationAndStaysSilentWithoutIt()
    {
        var source = File.ReadAllText(AppPath("Tarot", "TarotWorkspaceControl.cs"));
        var refresh = MethodSlice(source, "private void RefreshInterpretation()", "private static TextBlock CreateStateText");

        Assert.Contains("interpretationHost.IsVisible = false", refresh, StringComparison.Ordinal);
        Assert.Contains("interpretationHost.Content = null", refresh, StringComparison.Ordinal);
        Assert.Contains("interpretationCoordinator.Current.SingleCardPresentation", refresh, StringComparison.Ordinal);
        Assert.Contains("interpretationHost.IsVisible = true", refresh, StringComparison.Ordinal);
        Assert.Contains("CreateInterpretationTag(tag)", refresh, StringComparison.Ordinal);
        Assert.Contains("CreateInterpretationSection(section)", refresh, StringComparison.Ordinal);
        Assert.True(
            refresh.IndexOf("content.Children.Add(tagRow)", StringComparison.Ordinal) <
            refresh.IndexOf("content.Children.Add(CreateInterpretationSection(section))", StringComparison.Ordinal));
        Assert.DoesNotContain("MinHeight", refresh, StringComparison.Ordinal);
        Assert.DoesNotContain("surface-card", refresh, StringComparison.Ordinal);
        Assert.DoesNotContain("ThreeCardPositions", refresh, StringComparison.Ordinal);
        Assert.DoesNotContain("ConceptId.Value", refresh, StringComparison.Ordinal);
    }

    [Fact]
    public void ControlSubscribesToTypedSnapshotAndUnsubscribesOnDetach()
    {
        var source = File.ReadAllText(AppPath("Tarot", "TarotWorkspaceControl.cs"));
        var window = File.ReadAllText(AppPath("MainWindow.axaml.cs"));

        Assert.Contains("TarotWorkspaceInterpretationCoordinator interpretationCoordinator", source, StringComparison.Ordinal);
        Assert.Contains("interpretationCoordinator.SnapshotChanged += OnInterpretationSnapshotChanged", source, StringComparison.Ordinal);
        Assert.Contains("interpretationCoordinator.SnapshotChanged -= OnInterpretationSnapshotChanged", source, StringComparison.Ordinal);
        Assert.Contains("_tarotInterpretationCoordinator,", window, StringComparison.Ordinal);
        Assert.DoesNotContain("ResolveSingleCard(", source, StringComparison.Ordinal);
        Assert.DoesNotContain("ResolveThreeCardPosition(", source, StringComparison.Ordinal);
    }

    [Fact]
    public void StructuredSingleCardRendererUsesFiveSectionBlocksThreeTagRolesAndIntensityDots()
    {
        var source = File.ReadAllText(AppPath("Tarot", "TarotWorkspaceControl.cs"));
        var styles = File.ReadAllText(AppPath("Themes", "SemanticControlStyles.axaml"));

        Assert.Contains("TarotInterpretationTagRow", source, StringComparison.Ordinal);
        Assert.Contains("tarot-interpretation-section-heading", source, StringComparison.Ordinal);
        Assert.Contains("tarot-interpretation-section-body", source, StringComparison.Ordinal);
        Assert.Contains("for (var index = 0; index < tag.Intensity; index++)", source, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.SetName(chip, tag.Label)", source, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.SetHelpText(", source, StringComparison.Ordinal);
        Assert.Contains("valence-negative-strong", styles, StringComparison.Ordinal);
        Assert.Contains("valence-negative", styles, StringComparison.Ordinal);
        Assert.Contains("valence-neutral", styles, StringComparison.Ordinal);
        Assert.Contains("valence-positive", styles, StringComparison.Ordinal);
        Assert.Contains("valence-positive-strong", styles, StringComparison.Ordinal);
        Assert.Contains("Ellipse.tarot-interpretation-intensity-dot", styles, StringComparison.Ordinal);
        Assert.DoesNotContain("FontFamily", source, StringComparison.Ordinal);
    }

    [Fact]
    public void AutoRevealToggle_IsLocalizedAccessibleAndBoundToPreference()
    {
        var source = File.ReadAllText(AppPath("Tarot", "TarotWorkspaceControl.cs"));

        Assert.Contains("Name = \"TarotAutoRevealToggle\"", source, StringComparison.Ordinal);
        Assert.Contains("Content = Localize(\"ui.tarot.control.auto-reveal\")", source, StringComparison.Ordinal);
        Assert.Contains("IsChecked = viewModel.AutoRevealCards", source, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.SetName(autoRevealToggle, Localize(\"ui.tarot.control.auto-reveal\"))", source, StringComparison.Ordinal);
        Assert.Contains("viewModel.SetAutoRevealCards(autoRevealToggle.IsChecked == true)", source, StringComparison.Ordinal);
        Assert.Contains("controls.Children.Add(autoRevealToggle)", source, StringComparison.Ordinal);
        Assert.DoesNotContain("IsTabStop = false", source, StringComparison.Ordinal);
    }

    [Fact]
    public void TableauFallbackWidth_UsesScaledPreferredThreeCardContentWidth()
    {
        var source = File.ReadAllText(AppPath("Tarot", "TarotWorkspaceControl.cs"));

        Assert.Contains(
            "private const double TableauFallbackWidth = TarotTableauLayout.PreferredThreeCardContentWidth;",
            source,
            StringComparison.Ordinal);
        Assert.DoesNotContain("TableauFallbackWidth = 760d", source, StringComparison.Ordinal);
    }

    [Fact]
    public void MainWindowContentHost_StretchesTarotWorkspaceInStarRow()
    {
        var document = XDocument.Load(AppPath("MainWindow.axaml"));
        XNamespace ns = "https://github.com/avaloniaui";
        var contentHost = document.Descendants(ns + "ContentControl")
            .Single(element => element.Attributes()
                .Any(attribute => attribute.Name.LocalName == "Name" && attribute.Value == "SectionContentHost"));

        Assert.Equal("2", (string?)contentHost.Attribute("Grid.Row"));
        Assert.Equal("Stretch", (string?)contentHost.Attribute("HorizontalContentAlignment"));
        Assert.Equal("Stretch", (string?)contentHost.Attribute("VerticalContentAlignment"));
        Assert.Equal("Auto,Auto,*", (string?)contentHost.Parent?.Attribute("RowDefinitions"));
    }

    [Fact]
    public void Startup_LoadsPreferencesAndAppliesPersistedThemeBeforeCreatingMainWindow()
    {
        var source = File.ReadAllText(AppPath("App.axaml.cs"));
        var loadIndex = source.IndexOf("preferencesStore.Load()", StringComparison.Ordinal);
        var themeIndex = source.IndexOf("ApplyTheme(preferencesCoordinator.Current.ThemeId)", StringComparison.Ordinal);
        var compositionIndex = source.IndexOf("TarotInterpretationComposition.CreateBuiltIn()", StringComparison.Ordinal);
        var windowIndex = source.IndexOf("desktop.MainWindow = new MainWindow(preferencesCoordinator, interpretation)", StringComparison.Ordinal);

        Assert.True(compositionIndex >= 0);
        Assert.True(loadIndex >= 0);
        Assert.True(loadIndex > compositionIndex);
        Assert.True(themeIndex > loadIndex);
        Assert.True(windowIndex > themeIndex);
    }

    [Fact]
    public void CompositionPassesPersistedTarotPreferencesAndDoesNotSaveOnGeneralStateChanged()
    {
        var source = File.ReadAllText(AppPath("MainWindow.axaml.cs"));

        Assert.Contains("_userPreferences.Tarot", source, StringComparison.Ordinal);
        Assert.Contains("_tarotWorkspaceViewModel.PreferencesChanged += OnTarotPreferencesChanged", source, StringComparison.Ordinal);
        Assert.Contains("ApplyTarotPreferences(preferences)", source, StringComparison.Ordinal);
        Assert.DoesNotContain("_tarotWorkspaceViewModel.StateChanged +=", source, StringComparison.Ordinal);
    }

    [Fact]
    public void SettingsUi_NoLongerClaimsMemoryOnlyBehaviorOrDuplicatesAutoReveal()
    {
        var settingsControl = File.ReadAllText(AppPath("Debug", "DebugSettingsControl.cs"));
        var russian = File.ReadAllText(RepositoryPath("resources", "localization", "ui", "ru.json"));
        var english = File.ReadAllText(RepositoryPath("resources", "localization", "ui", "en.json"));

        Assert.DoesNotContain("in-memory", settingsControl, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("auto-reveal", settingsControl, StringComparison.Ordinal);
        Assert.DoesNotContain("Изменения пока действуют только в памяти", russian, StringComparison.Ordinal);
        Assert.DoesNotContain("Changes currently apply in memory only", english, StringComparison.Ordinal);
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
    public void InterpretationUnavailablePlaceholder_IsAbsentFromActiveProductionAndUiCatalogs()
    {
        var productionSources = Directory.GetFiles(AppPath("Tarot"), "*.cs", SearchOption.AllDirectories)
            .Concat(Directory.GetFiles(PresentationPath("Tarot"), "*.cs", SearchOption.AllDirectories))
            .Select(File.ReadAllText)
            .ToArray();

        Assert.All(productionSources, source =>
        {
            Assert.DoesNotContain("Толкование для выбранного набора", source, StringComparison.Ordinal);
            Assert.DoesNotContain("Interpretation for the selected set", source, StringComparison.Ordinal);
            Assert.DoesNotContain("InterpretationUnavailableKey", source, StringComparison.Ordinal);
            Assert.DoesNotContain("ui.tarot.interpretation.unavailable", source, StringComparison.Ordinal);
        });
        Assert.DoesNotContain(
            "ui.tarot.interpretation.unavailable",
            File.ReadAllText(RepositoryPath("resources", "localization", "ui", "ru.json")),
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "ui.tarot.interpretation.unavailable",
            File.ReadAllText(RepositoryPath("resources", "localization", "ui", "en.json")),
            StringComparison.Ordinal);
    }

    [Fact]
    public void InterpretationPackSelector_IsVisibleLocalizedManifestNamedAndOrderedAfterArtwork()
    {
        var source = File.ReadAllText(AppPath("Tarot", "TarotWorkspaceControl.cs"));
        var spread = source.IndexOf("controls.Children.Add(CreateLabeledControl(\"ui.tarot.control.spread\"", StringComparison.Ordinal);
        var artwork = source.IndexOf("controls.Children.Add(CreateLabeledControl(\"ui.tarot.control.artwork\"", StringComparison.Ordinal);
        var interpretation = source.IndexOf("controls.Children.Add(CreateLabeledControl(\"ui.tarot.control.interpretation-pack\"", StringComparison.Ordinal);
        var back = source.IndexOf("controls.Children.Add(CreateLabeledControl(\"ui.tarot.control.back\"", StringComparison.Ordinal);

        Assert.Contains("Name = \"TarotInterpretationPackSelector\"", source, StringComparison.Ordinal);
        Assert.Contains("interpretationPackCatalog.ResolveDisplayName(option.Id, applicationLanguage)", source, StringComparison.Ordinal);
        Assert.Contains("IsVisible = true", source, StringComparison.Ordinal);
        Assert.Contains("IsEnabled = interpretationPackItems.Length > 0", source, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.SetName(", source, StringComparison.Ordinal);
        Assert.Contains("viewModel.SelectInterpretationPack(selected.Option.Id)", source, StringComparison.Ordinal);
        Assert.True(spread >= 0 && spread < artwork && artwork < interpretation && interpretation < back);
        Assert.DoesNotContain("Классика", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Classic", source, StringComparison.Ordinal);
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

    private static string MethodSlice(string source, string startMarker, string endMarker)
    {
        var start = source.IndexOf(startMarker, StringComparison.Ordinal);
        Assert.True(start >= 0, $"Start marker '{startMarker}' was not found.");
        var end = source.IndexOf(endMarker, start + startMarker.Length, StringComparison.Ordinal);
        Assert.True(end > start, $"End marker '{endMarker}' was not found after '{startMarker}'.");
        return source[start..end];
    }

    private static int CountOccurrences(string source, string value)
    {
        var count = 0;
        var offset = 0;
        while ((offset = source.IndexOf(value, offset, StringComparison.Ordinal)) >= 0)
        {
            count++;
            offset += value.Length;
        }

        return count;
    }
}
