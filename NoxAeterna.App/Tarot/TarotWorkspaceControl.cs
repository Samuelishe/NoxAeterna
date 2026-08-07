using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using System.Globalization;
using NodaTime;
using NoxAeterna.Domain.Tarot;
using NoxAeterna.Presentation.Localization;
using NoxAeterna.Presentation.Tarot;
using ShapePath = Avalonia.Controls.Shapes.Path;
using AvaloniaGeometry = Avalonia.Media.Geometry;

namespace NoxAeterna.App.Tarot;

/// <summary>Hosts the playable in-memory Tarot workspace with built-in raster artwork.</summary>
public sealed class TarotWorkspaceControl : UserControl
{
    private const double TableauFallbackWidth = TarotTableauLayout.PreferredThreeCardContentWidth;
    private const double PositionLabelHeight = 34d;

    private readonly TarotWorkspaceViewModel viewModel;
    private readonly TarotArtworkPackCatalog artworkCatalog;
    private readonly TarotInterpretationPackCatalog interpretationPackCatalog;
    private readonly TarotWorkspaceInterpretationCoordinator interpretationCoordinator;
    private readonly ILocalizationProvider localizationProvider;
    private readonly LanguageCode applicationLanguage;
    private readonly Func<Instant> getCurrentInstant;
    private readonly ContentControl tableauStateHost;
    private readonly ContentControl interpretationHost;
    private readonly ContentControl readingLayoutHost;
    private readonly Dictionary<string, Bitmap> rasterImageCache = new(StringComparer.Ordinal);
    private ScrollViewer? tableauScrollViewer;
    private ReadingSurfaceComposition? readingSurfaceComposition;
    private TarotSingleCardReadingLayoutResult? singleCardReadingLayout;
    private Button? drawButton;
    private TextBlock? artworkDiagnostic;

    /// <summary>Initializes a real Tarot workspace over presentation-owned state.</summary>
    public TarotWorkspaceControl(
        TarotWorkspaceViewModel viewModel,
        TarotArtworkPackCatalog artworkCatalog,
        TarotInterpretationPackCatalog interpretationPackCatalog,
        TarotWorkspaceInterpretationCoordinator interpretationCoordinator,
        ILocalizationProvider localizationProvider,
        LanguageCode applicationLanguage,
        Func<Instant> getCurrentInstant)
    {
        this.viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        this.artworkCatalog = artworkCatalog ?? throw new ArgumentNullException(nameof(artworkCatalog));
        this.interpretationPackCatalog = interpretationPackCatalog ??
            throw new ArgumentNullException(nameof(interpretationPackCatalog));
        this.interpretationCoordinator = interpretationCoordinator ??
            throw new ArgumentNullException(nameof(interpretationCoordinator));
        this.localizationProvider = localizationProvider ?? throw new ArgumentNullException(nameof(localizationProvider));
        this.applicationLanguage = applicationLanguage;
        this.getCurrentInstant = getCurrentInstant ?? throw new ArgumentNullException(nameof(getCurrentInstant));

        tableauStateHost = new ContentControl { HorizontalContentAlignment = HorizontalAlignment.Stretch };
        interpretationHost = new ContentControl { HorizontalContentAlignment = HorizontalAlignment.Stretch };
        readingLayoutHost = new ContentControl
        {
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            VerticalContentAlignment = VerticalAlignment.Stretch
        };
        Content = BuildContent();
        viewModel.StateChanged += OnViewModelStateChanged;
        interpretationCoordinator.SnapshotChanged += OnInterpretationSnapshotChanged;
        DetachedFromVisualTree += (_, _) =>
        {
            viewModel.StateChanged -= OnViewModelStateChanged;
            interpretationCoordinator.SnapshotChanged -= OnInterpretationSnapshotChanged;
            foreach (var bitmap in rasterImageCache.Values)
            {
                bitmap.Dispose();
            }

            rasterImageCache.Clear();
        };
        RefreshWorkspaceState();
    }

    private Control BuildContent()
    {
        var root = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,*"),
            RowSpacing = 16
        };
        var controlPanel = CreateControlPanel();
        var readingSurface = CreateReadingSurface();
        Grid.SetRow(readingSurface, 1);
        root.Children.Add(controlPanel);
        root.Children.Add(readingSurface);

        return root;
    }

    private Control CreateControlPanel()
    {
        var spreadSelector = new ComboBox
        {
            Name = "TarotSpreadSelector",
            MinWidth = 190,
            ItemsSource = viewModel.SpreadOptions
                .Select(option => new LocalizedSpreadOption(option, Localize(option.LabelKey)))
                .ToArray()
        };
        spreadSelector.SelectedItem = ((LocalizedSpreadOption[])spreadSelector.ItemsSource!)
            .First(option => option.Option == viewModel.SelectedSpread);
        AutomationProperties.SetName(spreadSelector, Localize("ui.tarot.control.spread"));
        spreadSelector.SelectionChanged += (_, _) =>
        {
            if (spreadSelector.SelectedItem is LocalizedSpreadOption selected)
            {
                viewModel.SelectSpread(selected.Option.Definition.Id);
            }
        };

        var artworkSelector = new ComboBox
        {
            Name = "TarotArtworkSelector",
            MinWidth = 170,
            ItemsSource = viewModel.ArtworkPacks
                .Select(option => new LocalizedArtworkOption(option, Localize(option.LabelKey)))
                .ToArray(),
            IsVisible = true
        };
        artworkSelector.SelectedItem = ((LocalizedArtworkOption[])artworkSelector.ItemsSource!)
            .First(option => option.Option == viewModel.SelectedArtworkPack);
        AutomationProperties.SetName(artworkSelector, Localize("ui.tarot.control.artwork"));
        artworkSelector.SelectionChanged += (_, _) =>
        {
            if (artworkSelector.SelectedItem is LocalizedArtworkOption selected)
            {
                viewModel.SelectArtworkPack(selected.Option.Id);
            }
        };

        var interpretationPackItems = viewModel.InterpretationPacks
            .Select(option => new LocalizedInterpretationPackOption(
                option,
                interpretationPackCatalog.ResolveDisplayName(option.Id, applicationLanguage)))
            .ToArray();
        var interpretationPackSelector = new ComboBox
        {
            Name = "TarotInterpretationPackSelector",
            MinWidth = 150,
            ItemsSource = interpretationPackItems,
            IsVisible = true,
            IsEnabled = interpretationPackItems.Length > 0
        };
        interpretationPackSelector.SelectedItem = viewModel.SelectedInterpretationPack is { } selectedPack
            ? interpretationPackItems.FirstOrDefault(item => item.Option == selectedPack)
            : null;
        AutomationProperties.SetName(
            interpretationPackSelector,
            Localize("ui.tarot.control.interpretation-pack"));
        interpretationPackSelector.SelectionChanged += (_, _) =>
        {
            if (interpretationPackSelector.SelectedItem is LocalizedInterpretationPackOption selected)
            {
                viewModel.SelectInterpretationPack(selected.Option.Id);
            }
        };

        var orientationToggle = new CheckBox
        {
            Name = "TarotOrientationToggle",
            Content = Localize("ui.tarot.control.allow-reversed"),
            IsChecked = viewModel.AllowReversed,
            VerticalAlignment = VerticalAlignment.Center
        };
        AutomationProperties.SetName(orientationToggle, Localize("ui.tarot.control.allow-reversed"));
        orientationToggle.IsCheckedChanged += (_, _) =>
            viewModel.SetAllowReversed(orientationToggle.IsChecked == true);

        var autoRevealToggle = new CheckBox
        {
            Name = "TarotAutoRevealToggle",
            Content = Localize("ui.tarot.control.auto-reveal"),
            IsChecked = viewModel.AutoRevealCards,
            VerticalAlignment = VerticalAlignment.Center
        };
        AutomationProperties.SetName(autoRevealToggle, Localize("ui.tarot.control.auto-reveal"));
        autoRevealToggle.IsCheckedChanged += (_, _) =>
            viewModel.SetAutoRevealCards(autoRevealToggle.IsChecked == true);

        var backSelector = new ComboBox
        {
            Name = "TarotBackSelector",
            MinWidth = 170,
            ItemsSource = viewModel.BackVariants
                .Select(option => new LocalizedBackOption(option, Localize(option.LabelKey)))
                .ToArray()
        };
        backSelector.SelectedItem = ((LocalizedBackOption[])backSelector.ItemsSource!)
            .First(option => option.Option == viewModel.SelectedBackVariant);
        AutomationProperties.SetName(backSelector, Localize("ui.tarot.control.back"));
        backSelector.SelectionChanged += (_, _) =>
        {
            if (backSelector.SelectedItem is LocalizedBackOption selected)
            {
                viewModel.SelectBackVariant(selected.Option.Id);
            }
        };

        drawButton = new Button
        {
            Name = "TarotDrawButton",
            Content = Localize("ui.tarot.control.draw"),
            HorizontalAlignment = HorizontalAlignment.Left,
            IsEnabled = artworkCatalog.IsReady
        };
        drawButton.Classes.Add("primary-action");
        AutomationProperties.SetName(drawButton, Localize("ui.tarot.control.draw"));
        drawButton.Click += (_, _) => viewModel.Draw(getCurrentInstant());

        var controls = new WrapPanel { Orientation = Orientation.Horizontal };
        controls.Children.Add(CreateLabeledControl("ui.tarot.control.spread", spreadSelector));
        controls.Children.Add(CreateLabeledControl("ui.tarot.control.artwork", artworkSelector));
        controls.Children.Add(CreateLabeledControl("ui.tarot.control.interpretation-pack", interpretationPackSelector));
        controls.Children.Add(CreateLabeledControl("ui.tarot.control.back", backSelector));
        controls.Children.Add(orientationToggle);
        controls.Children.Add(autoRevealToggle);
        controls.Children.Add(drawButton);
        foreach (var control in controls.Children)
        {
            control.Margin = new Thickness(0, 0, 18, 12);
        }

        artworkDiagnostic = CreateStateText(Localize("ui.tarot.artwork.unavailable"), "validation-error");
        artworkDiagnostic.IsVisible = !artworkCatalog.IsReady;
        var panel = new Border
        {
            Padding = new Thickness(18, 16),
            Child = new StackPanel
            {
                Spacing = 4,
                Children =
                {
                    controls,
                    artworkDiagnostic
                }
            }
        };
        panel.Classes.Add("surface-card");
        return panel;
    }

    private Control CreateReadingSurface()
    {
        tableauScrollViewer = new ScrollViewer
        {
            Name = "TarotTableauScrollViewer",
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            Content = tableauStateHost
        };
        ScrollViewer.SetIsScrollChainingEnabled(tableauScrollViewer, true);
        AutomationProperties.SetName(tableauScrollViewer, Localize("ui.tarot.tableau.title"));
        tableauScrollViewer.SizeChanged += (_, _) => RefreshTableau();
        readingLayoutHost.SizeChanged += (_, _) =>
        {
            RefreshReadingLayout();
            RefreshTableau();
        };

        var surface = new Border
        {
            Name = "TarotReadingSurface",
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Padding = new Thickness(20, 16, 12, 20),
            Child = readingLayoutHost
        };
        surface.Classes.Add("surface-card");
        return surface;
    }

    private Control CreateStackedReadingSurface()
    {
        var readingContent = new StackPanel
        {
            Spacing = 14,
            Children =
            {
                tableauScrollViewer!,
                interpretationHost
            }
        };
        return new ScrollViewer
        {
            Name = "TarotReadingScrollViewer",
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            Content = readingContent
        };
    }

    private Control CreateWideSingleCardReadingSurface(double cardColumnWidth)
    {
        var columns = new ColumnDefinitions
        {
            new ColumnDefinition { Width = new GridLength(cardColumnWidth) },
            new ColumnDefinition { Width = GridLength.Star }
        };
        var grid = new Grid
        {
            Name = "TarotSingleCardReadingGrid",
            ColumnDefinitions = columns,
            ColumnSpacing = TarotReadingWorkspaceLayout.ColumnGap
        };
        var interpretationScrollViewer = new ScrollViewer
        {
            Name = "TarotInterpretationScrollViewer",
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            HorizontalContentAlignment = HorizontalAlignment.Left,
            Padding = new Thickness(0, 0, 8, 0),
            Content = interpretationHost
        };
        Grid.SetColumn(interpretationScrollViewer, 1);
        grid.Children.Add(tableauScrollViewer!);
        grid.Children.Add(interpretationScrollViewer);
        return grid;
    }

    private void RefreshReadingLayout()
    {
        var availableWidth = readingLayoutHost.Bounds.Width > 0d
            ? readingLayoutHost.Bounds.Width
            : TableauFallbackWidth;
        var availableHeight = readingLayoutHost.Bounds.Height > 0d
            ? readingLayoutHost.Bounds.Height
            : TarotTableauLayout.SingleCardWidth / TarotTableauLayout.CardAspectRatio;
        var isSingleCard = viewModel.SelectedSpread.Definition.Id == StandardTarotSpreads.SingleCard.Id;
        singleCardReadingLayout = isSingleCard
            ? TarotReadingWorkspaceLayout.CalculateSingleCard(availableWidth, availableHeight)
            : null;
        var desiredComposition = singleCardReadingLayout?.Composition ==
                                 TarotSingleCardReadingComposition.SideBySide
            ? ReadingSurfaceComposition.SideBySideSingleCard
            : ReadingSurfaceComposition.Stacked;

        interpretationHost.MaxWidth = TarotReadingWorkspaceLayout.MaximumInterpretationTextWidth;
        interpretationHost.HorizontalAlignment = HorizontalAlignment.Stretch;
        tableauScrollViewer!.HorizontalScrollBarVisibility = desiredComposition ==
                                                              ReadingSurfaceComposition.SideBySideSingleCard
            ? ScrollBarVisibility.Disabled
            : ScrollBarVisibility.Auto;

        if (readingSurfaceComposition == desiredComposition)
        {
            return;
        }

        DetachFromLayoutParent(tableauScrollViewer);
        DetachFromLayoutParent(interpretationHost);
        readingSurfaceComposition = desiredComposition;
        readingLayoutHost.Content = null;
        var newContent = desiredComposition == ReadingSurfaceComposition.SideBySideSingleCard
            ? CreateWideSingleCardReadingSurface(singleCardReadingLayout!.CardColumnWidth)
            : CreateStackedReadingSurface();
        readingLayoutHost.Content = newContent;
    }

    private static void DetachFromLayoutParent(Control control)
    {
        switch (control.Parent)
        {
            case Panel panel:
                panel.Children.Remove(control);
                break;
            case ContentControl contentControl when ReferenceEquals(contentControl.Content, control):
                contentControl.Content = null;
                break;
        }
    }

    private Control CreateLabeledControl(string labelKey, Control control) => new StackPanel
    {
        Spacing = 6,
        Children =
        {
            new TextBlock { Text = Localize(labelKey), Classes = { "supporting" } },
            control
        }
    };

    private void OnViewModelStateChanged(object? sender, EventArgs e) => RefreshWorkspaceState();

    private void OnInterpretationSnapshotChanged(object? sender, EventArgs e) => RefreshInterpretation();

    private void RefreshWorkspaceState()
    {
        if (drawButton is not null)
        {
            var actionKey = viewModel.CurrentReading is null
                ? "ui.tarot.control.draw"
                : "ui.tarot.control.redraw";
            drawButton.Content = Localize(actionKey);
            AutomationProperties.SetName(drawButton, Localize(actionKey));
        }

        RefreshReadingLayout();
        RefreshTableau();
        RefreshInterpretation();
    }

    private void RefreshTableau()
    {
        if (viewModel.CurrentFailure is not null)
        {
            tableauStateHost.Content = CreateStateText(Localize(viewModel.FailureStateKey), "validation-error");
            return;
        }

        if (viewModel.CurrentReading is not { } reading)
        {
            var preview = CreateCardBack(viewModel.SelectedBackVariant.Id, TarotTableauLayout.MinimumCardWidth);
            preview.IsHitTestVisible = false;
            tableauStateHost.Content = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                Spacing = 14,
                Children =
                {
                    preview,
                    CreateStateText(Localize(viewModel.EmptyStateKey), "subtle")
                }
            };
            return;
        }

        var availableWidth = tableauScrollViewer is { Bounds.Width: > 0d }
            ? tableauScrollViewer.Bounds.Width
            : singleCardReadingLayout?.Composition == TarotSingleCardReadingComposition.SideBySide
                ? singleCardReadingLayout.CardColumnWidth
                : TableauFallbackWidth;
        var layout = reading.Cards.Count == 1 &&
                     singleCardReadingLayout?.Composition == TarotSingleCardReadingComposition.SideBySide
            ? TarotTableauLayout.CalculateSingleCard(availableWidth, singleCardReadingLayout.CardWidth)
            : TarotTableauLayout.Calculate(availableWidth, reading.Cards.Count);
        var showPositionLabels = TarotReadingWorkspaceLayout.ShowPositionLabels(reading.SpreadId);
        var canvas = new Canvas
        {
            Width = layout.ContentWidth,
            Height = layout.ContentHeight + (showPositionLabels ? PositionLabelHeight : 0d)
        };

        for (var index = 0; index < reading.Cards.Count; index++)
        {
            var assignment = reading.Cards[index];
            var bounds = layout.CardBounds[index];
            var slot = CreateCardSlot(assignment, bounds.Width, bounds.Height, showPositionLabels);
            Canvas.SetLeft(slot, bounds.X);
            Canvas.SetTop(slot, bounds.Y);
            canvas.Children.Add(slot);
        }

        tableauStateHost.Content = canvas;
    }

    private Control CreateCardSlot(
        TarotDrawnCard assignment,
        double width,
        double height,
        bool showPositionLabel)
    {
        var isRevealed = viewModel.IsRevealed(assignment.PositionId);
        var button = new Button
        {
            Width = width,
            Height = height,
            Padding = new Thickness(0),
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            VerticalContentAlignment = VerticalAlignment.Stretch,
            Content = isRevealed
                ? CreateCardFace(assignment)
                : CreateCardBack(viewModel.SelectedBackVariant.Id, width)
        };
        button.Classes.Add("tarot-card");
        button.Classes.Set("selected", viewModel.SelectedCard == assignment);
        var cardName = isRevealed
            ? TarotCardTextResolver.GetCardName(assignment.Card, localizationProvider, applicationLanguage)
            : Localize(viewModel.SelectedBackVariant.LabelKey);
        AutomationProperties.SetName(button, $"{TarotCardTextResolver.GetPositionName(assignment.PositionId, localizationProvider, applicationLanguage)}: {cardName}");
        button.Click += (_, _) => viewModel.RevealAndSelect(assignment.PositionId);

        var slot = new StackPanel
        {
            Width = width,
            Spacing = 8
        };
        slot.Children.Add(button);
        if (showPositionLabel)
        {
            slot.Children.Add(new TextBlock
            {
                Text = TarotCardTextResolver.GetPositionName(
                    assignment.PositionId,
                    localizationProvider,
                    applicationLanguage),
                HorizontalAlignment = HorizontalAlignment.Center,
                FontWeight = FontWeight.SemiBold
            });
        }

        return slot;
    }

    private Control CreateCardFace(TarotDrawnCard assignment)
    {
        var title = TarotCardTextResolver.GetCardName(assignment.Card, localizationProvider, applicationLanguage);
        var structuralText = GetStructuralText(assignment);
        var artwork = artworkCatalog.Resolve(viewModel.SelectedArtworkPack.Id, assignment.Card);
        var plan = TarotCardVisualPlan.Create(
            assignment,
            artwork,
            title,
            structuralText,
            prototypeFallbackText: null);
        var visual = artwork.Kind == TarotArtworkResolutionKind.Raster
            ? CreateRasterArtwork(artwork.RasterAsset!)
            : CreatePrototypeArtwork(assignment);

        var titleOverlay = new Border
        {
            Padding = new Thickness(8, 6),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Top,
            Child = new TextBlock
            {
                Text = plan.LocalizedTitle,
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center,
                FontSize = 14,
                FontWeight = FontWeight.SemiBold
            }
        };
        titleOverlay.Classes.Add("tarot-card-overlay");

        var footerStack = new StackPanel
        {
            Spacing = 3,
            Children =
            {
                CreateFooterText(plan.StructuralText)
            }
        };
        if (plan.PrototypeFallbackText is not null)
        {
            var fallback = CreateFooterText(plan.PrototypeFallbackText);
            fallback.FontSize = 10;
            fallback.Classes.Add("subtle");
            footerStack.Children.Add(fallback);
        }

        var footerOverlay = new Border
        {
            Padding = new Thickness(8, 6),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Bottom,
            Child = footerStack
        };
        footerOverlay.Classes.Add("tarot-card-overlay");

        var layers = new Grid
        {
            Children =
            {
                visual,
                titleOverlay,
                footerOverlay
            }
        };
        var face = new Border
        {
            ClipToBounds = true,
            Child = layers
        };
        face.Classes.Add("tarot-card-face");

        if (plan.RotationDegrees != 0d)
        {
            face.RenderTransformOrigin = RelativePoint.Center;
            face.RenderTransform = new RotateTransform(plan.RotationDegrees);
        }

        return face;
    }

    private Control CreatePrototypeArtwork(TarotDrawnCard assignment)
    {
        var markerPath = assignment.Card.Arcana == TarotArcana.Major
            ? TarotPrototypeGeometryCatalog.GetMajorSealPathData()
            : TarotPrototypeGeometryCatalog.GetSuitPathData(assignment.Card.Suit!.Value);
        var marker = new ShapePath
        {
            Data = AvaloniaGeometry.Parse(markerPath),
            Stretch = Stretch.Uniform,
            Fill = Brushes.Transparent,
            StrokeThickness = 2,
            Width = 92,
            Height = 92,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        marker.Classes.Add("tarot-ornament");
        return marker;
    }

    private Image CreateRasterArtwork(TarotArtworkPackCardAsset asset)
    {
        if (!rasterImageCache.TryGetValue(asset.AssetPath, out var bitmap))
        {
            using var stream = asset.OpenRead();
            bitmap = new Bitmap(stream);
            rasterImageCache.Add(asset.AssetPath, bitmap);
        }

        return new Image
        {
            Source = bitmap,
            Stretch = Stretch.Uniform,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };
    }

    private string GetStructuralText(TarotDrawnCard assignment) => assignment.Card.Arcana == TarotArcana.Major
        ? TarotCardTextResolver.GetArcanaName(assignment.Card.Arcana, localizationProvider, applicationLanguage)
        : $"{TarotCardTextResolver.GetRankName(assignment.Card.Rank!.Value, localizationProvider, applicationLanguage)} · " +
          TarotCardTextResolver.GetSuitName(assignment.Card.Suit!.Value, localizationProvider, applicationLanguage);

    private static TextBlock CreateFooterText(string text)
    {
        var footer = new TextBlock
        {
            Text = text,
            TextAlignment = TextAlignment.Center,
            TextWrapping = TextWrapping.Wrap
        };
        footer.Classes.Add("supporting");
        return footer;
    }

    private Border CreateCardBack(TarotBackVariantId backVariantId, double width)
    {
        var ornament = new ShapePath
        {
            Data = AvaloniaGeometry.Parse(TarotPrototypeGeometryCatalog.GetBackPathData(backVariantId)),
            Stretch = Stretch.Uniform,
            Fill = Brushes.Transparent,
            StrokeThickness = 1.8,
            Margin = new Thickness(18)
        };
        ornament.Classes.Add("tarot-ornament");

        var back = new Border
        {
            Width = width,
            Height = width / TarotTableauLayout.CardAspectRatio,
            Padding = new Thickness(10),
            Child = ornament
        };
        back.Classes.Add("tarot-card-back");
        return back;
    }

    private void RefreshInterpretation()
    {
        interpretationHost.Content = null;
        interpretationHost.IsVisible = false;
        if (interpretationCoordinator.Current.SingleCardPresentation is not { } presentation)
        {
            return;
        }

        var content = new StackPanel { Spacing = 20 };
        if (presentation.Tags.Count > 0)
        {
            var tagRow = new WrapPanel
            {
                Name = "TarotInterpretationTagRow",
                Orientation = Orientation.Horizontal
            };
            foreach (var tag in presentation.Tags)
            {
                var chip = CreateInterpretationTag(tag);
                chip.Margin = new Thickness(0, 0, 10, 8);
                tagRow.Children.Add(chip);
            }

            content.Children.Add(tagRow);
        }

        foreach (var section in presentation.Sections)
        {
            content.Children.Add(CreateInterpretationSection(section));
        }

        interpretationHost.Content = content;
        interpretationHost.IsVisible = true;
    }

    private Border CreateInterpretationTag(TarotSingleCardInterpretationTag tag)
    {
        var intensity = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 3,
            VerticalAlignment = VerticalAlignment.Center
        };
        for (var index = 0; index < tag.Intensity; index++)
        {
            var dot = new Avalonia.Controls.Shapes.Ellipse { Width = 4, Height = 4 };
            dot.Classes.Add("tarot-interpretation-intensity-dot");
            intensity.Children.Add(dot);
        }

        var chip = new Border
        {
            Padding = new Thickness(10, 5),
            Child = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 8,
                Children =
                {
                    new TextBlock { Text = tag.Label, VerticalAlignment = VerticalAlignment.Center },
                    intensity
                }
            }
        };
        chip.Classes.Add("tarot-interpretation-tag");
        chip.Classes.Add(tag.Valence switch
        {
            -2 => "valence-negative-strong",
            -1 => "valence-negative",
            0 => "valence-neutral",
            1 => "valence-positive",
            2 => "valence-positive-strong",
            _ => throw new ArgumentOutOfRangeException(nameof(tag), tag.Valence, "Unknown interpretation valence.")
        });
        AutomationProperties.SetName(chip, tag.Label);
        AutomationProperties.SetHelpText(
            chip,
            string.Format(
                CultureInfo.GetCultureInfo(applicationLanguage.Value),
                Localize("ui.tarot.interpretation.intensity"),
                tag.Intensity));
        return chip;
    }

    private static Control CreateInterpretationSection(TarotSingleCardInterpretationSection section)
    {
        var heading = new TextBlock
        {
            Name = $"TarotInterpretationSectionHeading_{section.SectionId}",
            Text = section.Label,
            TextWrapping = TextWrapping.Wrap
        };
        heading.Classes.Add("tarot-interpretation-section-heading");
        var body = new TextBlock
        {
            Text = section.Text,
            TextWrapping = TextWrapping.Wrap
        };
        body.Classes.Add("tarot-interpretation-section-body");
        return new StackPanel
        {
            Spacing = 6,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Children = { heading, body }
        };
    }

    private static TextBlock CreateStateText(string text, string styleClass)
    {
        var textBlock = new TextBlock
        {
            Text = text,
            TextWrapping = TextWrapping.Wrap,
            TextAlignment = TextAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 12)
        };
        textBlock.Classes.Add(styleClass);
        return textBlock;
    }

    private string Localize(LocalizationKey key) =>
        localizationProvider.Get(LocalizationScope.Ui, applicationLanguage, key).Text;

    private string Localize(string key) => Localize(new LocalizationKey(key));

    private sealed record LocalizedSpreadOption(TarotSpreadOption Option, string Label)
    {
        public override string ToString() => Label;
    }

    private sealed record LocalizedBackOption(TarotBackVariantOption Option, string Label)
    {
        public override string ToString() => Label;
    }

    private sealed record LocalizedArtworkOption(TarotArtworkPackOption Option, string Label)
    {
        public override string ToString() => Label;
    }

    private sealed record LocalizedInterpretationPackOption(TarotInterpretationPackOption Option, string Label)
    {
        public override string ToString() => Label;
    }

    private enum ReadingSurfaceComposition
    {
        Stacked = 0,
        SideBySideSingleCard = 1
    }
}
