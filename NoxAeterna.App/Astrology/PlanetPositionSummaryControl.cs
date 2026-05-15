using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using NoxAeterna.Presentation.Astrology;
using NoxAeterna.Presentation.Localization;

namespace NoxAeterna.App.Astrology;

/// <summary>
/// Renders a compact readable list of current planet positions.
/// </summary>
public sealed class PlanetPositionSummaryControl : UserControl
{
    private readonly ILocalizationProvider _localizationProvider;
    private readonly LanguageCode _applicationLanguage;
    private readonly StackPanel _rowsPanel;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlanetPositionSummaryControl"/> class.
    /// </summary>
    public PlanetPositionSummaryControl(
        ILocalizationProvider localizationProvider,
        LanguageCode applicationLanguage,
        IReadOnlyList<PlanetPositionSummaryRow> rows)
    {
        _localizationProvider = localizationProvider ?? throw new ArgumentNullException(nameof(localizationProvider));
        _applicationLanguage = applicationLanguage;
        _rowsPanel = new StackPanel
        {
            Spacing = 6
        };

        Content = new StackPanel
        {
            Spacing = 10,
            Children =
            {
                new TextBlock
                {
                    Text = Localize("ui.chart.positions.title"),
                    FontSize = 14,
                    FontWeight = FontWeight.SemiBold
                },
                _rowsPanel
            }
        };

        SetRows(rows);
    }

    /// <summary>
    /// Replaces the currently visible rows.
    /// </summary>
    public void SetRows(IReadOnlyList<PlanetPositionSummaryRow> rows)
    {
        ArgumentNullException.ThrowIfNull(rows);

        _rowsPanel.Children.Clear();

        foreach (var row in rows)
        {
            _rowsPanel.Children.Add(CreateRow(row));
        }
    }

    private Control CreateRow(PlanetPositionSummaryRow row)
    {
        var retrogradeMarker = row.IsRetrograde
            ? $" {Localize("ui.chart.positions.retrograde_marker")}"
            : string.Empty;

        return new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,Auto,*,Auto"),
            ColumnSpacing = 10,
            Children =
            {
                CreateCell(row.Glyph, 0, FontWeight.SemiBold),
                CreateCell(Localize(row.BodyLabelKey), 1),
                CreateCell($"{row.SignGlyph} {Localize(row.SignLabelKey)}", 2),
                CreateCell($"{row.DegreeText}{retrogradeMarker}", 3, horizontalAlignment: HorizontalAlignment.Right)
            }
        };
    }

    private TextBlock CreateCell(
        string text,
        int column,
        FontWeight? fontWeight = null,
        HorizontalAlignment horizontalAlignment = HorizontalAlignment.Left)
    {
        var textBlock = new TextBlock
        {
            Text = text,
            HorizontalAlignment = horizontalAlignment,
            Foreground = ResolveBrush("WorkspacePanelForegroundBrush", Brushes.Gainsboro)
        };

        if (fontWeight is { } resolvedFontWeight)
        {
            textBlock.FontWeight = resolvedFontWeight;
        }

        Grid.SetColumn(textBlock, column);
        return textBlock;
    }

    private string Localize(string key) => Localize(new LocalizationKey(key));

    private string Localize(LocalizationKey key) =>
        _localizationProvider.Get(LocalizationScope.Ui, _applicationLanguage, key).Text;

    private IBrush ResolveBrush(string resourceKey, IBrush fallbackBrush) =>
        Application.Current is { } application &&
        application.TryGetResource(resourceKey, ActualThemeVariant, out var resource) &&
        resource is IBrush brush
            ? brush
            : fallbackBrush;
}
