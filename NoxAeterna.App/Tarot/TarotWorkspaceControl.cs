using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
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
    private const double TableauFallbackWidth = 760d;
    private const double PositionLabelHeight = 34d;

    private readonly TarotWorkspaceViewModel viewModel;
    private readonly TarotArtworkPackCatalog artworkCatalog;
    private readonly ILocalizationProvider localizationProvider;
    private readonly LanguageCode applicationLanguage;
    private readonly Func<Instant> getCurrentInstant;
    private readonly ContentControl tableauStateHost;
    private readonly ContentControl inspectorHost;
    private readonly Dictionary<string, Bitmap> rasterImageCache = new(StringComparer.Ordinal);
    private ScrollViewer? tableauScrollViewer;
    private Button? drawButton;
    private TextBlock? artworkDiagnostic;

    /// <summary>Initializes a real Tarot workspace over presentation-owned state.</summary>
    public TarotWorkspaceControl(
        TarotWorkspaceViewModel viewModel,
        TarotArtworkPackCatalog artworkCatalog,
        ILocalizationProvider localizationProvider,
        LanguageCode applicationLanguage,
        Func<Instant> getCurrentInstant)
    {
        this.viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        this.artworkCatalog = artworkCatalog ?? throw new ArgumentNullException(nameof(artworkCatalog));
        this.localizationProvider = localizationProvider ?? throw new ArgumentNullException(nameof(localizationProvider));
        this.applicationLanguage = applicationLanguage;
        this.getCurrentInstant = getCurrentInstant ?? throw new ArgumentNullException(nameof(getCurrentInstant));

        tableauStateHost = new ContentControl { HorizontalContentAlignment = HorizontalAlignment.Stretch };
        inspectorHost = new ContentControl { HorizontalContentAlignment = HorizontalAlignment.Stretch };
        Content = BuildContent();
        viewModel.StateChanged += OnViewModelStateChanged;
        DetachedFromVisualTree += (_, _) =>
        {
            viewModel.StateChanged -= OnViewModelStateChanged;
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
        var root = new StackPanel
        {
            Spacing = 16,
            Children =
            {
                CreateControlPanel(),
                CreateTableauPanel(),
                inspectorHost
            }
        };

        return new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Padding = new Thickness(0, 0, 16, 8),
            Content = root
        };
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
        controls.Children.Add(CreateLabeledControl("ui.tarot.control.back", backSelector));
        controls.Children.Add(orientationToggle);
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

    private Control CreateTableauPanel()
    {
        tableauScrollViewer = new ScrollViewer
        {
            Name = "TarotTableauScrollViewer",
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            Content = tableauStateHost
        };
        AutomationProperties.SetName(tableauScrollViewer, Localize("ui.tarot.tableau.title"));
        tableauScrollViewer.SizeChanged += (_, _) => RefreshTableau();

        var panel = new Border
        {
            Padding = new Thickness(20, 16, 20, 20),
            MinHeight = 360,
            Child = new StackPanel
            {
                Spacing = 14,
                Children =
                {
                    new TextBlock
                    {
                        Text = Localize("ui.tarot.tableau.title"),
                        FontSize = 16,
                        FontWeight = FontWeight.SemiBold
                    },
                    tableauScrollViewer
                }
            }
        };
        panel.Classes.Add("surface-card");
        return panel;
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

        RefreshTableau();
        RefreshInspector();
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
            : TableauFallbackWidth;
        var layout = TarotTableauLayout.Calculate(availableWidth, reading.Cards.Count);
        var canvas = new Canvas
        {
            Width = layout.ContentWidth,
            Height = layout.ContentHeight + PositionLabelHeight
        };

        for (var index = 0; index < reading.Cards.Count; index++)
        {
            var assignment = reading.Cards[index];
            var bounds = layout.CardBounds[index];
            var slot = CreateCardSlot(assignment, bounds.Width, bounds.Height);
            Canvas.SetLeft(slot, bounds.X);
            Canvas.SetTop(slot, bounds.Y);
            canvas.Children.Add(slot);
        }

        tableauStateHost.Content = canvas;
    }

    private Control CreateCardSlot(TarotDrawnCard assignment, double width, double height)
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

        return new StackPanel
        {
            Width = width,
            Spacing = 8,
            Children =
            {
                button,
                new TextBlock
                {
                    Text = TarotCardTextResolver.GetPositionName(assignment.PositionId, localizationProvider, applicationLanguage),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    FontWeight = FontWeight.SemiBold
                }
            }
        };
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

    private void RefreshInspector()
    {
        if (viewModel.SelectedCard is not { } selected)
        {
            inspectorHost.IsVisible = false;
            inspectorHost.Content = null;
            return;
        }

        inspectorHost.IsVisible = true;
        var details = new StackPanel
        {
            Spacing = 9,
            Children =
            {
                CreateInspectorRow("ui.tarot.inspector.card", TarotCardTextResolver.GetCardName(selected.Card, localizationProvider, applicationLanguage)),
                CreateInspectorRow("ui.tarot.inspector.position", TarotCardTextResolver.GetPositionName(selected.PositionId, localizationProvider, applicationLanguage)),
                CreateInspectorRow("ui.tarot.inspector.orientation", TarotCardTextResolver.GetOrientationName(selected.Orientation, localizationProvider, applicationLanguage)),
                CreateInspectorRow("ui.tarot.inspector.arcana", TarotCardTextResolver.GetArcanaName(selected.Card.Arcana, localizationProvider, applicationLanguage))
            }
        };
        if (selected.Card.Arcana == TarotArcana.Minor)
        {
            details.Children.Add(CreateInspectorRow("ui.tarot.inspector.suit", TarotCardTextResolver.GetSuitName(selected.Card.Suit!.Value, localizationProvider, applicationLanguage)));
            details.Children.Add(CreateInspectorRow("ui.tarot.inspector.rank", TarotCardTextResolver.GetRankName(selected.Card.Rank!.Value, localizationProvider, applicationLanguage)));
        }

        var unavailable = CreateStateText(Localize(viewModel.InterpretationUnavailableKey), "subtle");
        details.Children.Add(unavailable);

        var inspector = new Border
        {
            Padding = new Thickness(20, 16),
            Child = new StackPanel
            {
                Spacing = 12,
                Children =
                {
                    new TextBlock
                    {
                        Text = Localize("ui.tarot.inspector.title"),
                        FontSize = 16,
                        FontWeight = FontWeight.SemiBold
                    },
                    details
                }
            }
        };
        inspector.Classes.Add("surface-card");
        inspector.Classes.Add("tarot-inspector");
        inspectorHost.Content = inspector;
    }

    private Control CreateInspectorRow(string labelKey, string value) => new Grid
    {
        ColumnDefinitions = new ColumnDefinitions("150,*"),
        ColumnSpacing = 12,
        Children =
        {
            new TextBlock { Text = Localize(labelKey), Classes = { "supporting" } },
            CreateInspectorValue(value)
        }
    };

    private static TextBlock CreateInspectorValue(string value)
    {
        var text = new TextBlock { Text = value, TextWrapping = TextWrapping.Wrap };
        Grid.SetColumn(text, 1);
        return text;
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
}
