using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using NoxAeterna.Presentation.Astrology;
using NoxAeterna.Presentation.Localization;

namespace NoxAeterna.App.Astrology;

/// <summary>
/// Renders current planet positions in one shared, aligned four-column table.
/// </summary>
public sealed class PlanetPositionSummaryControl : UserControl
{
    private readonly ILocalizationProvider _localizationProvider;
    private readonly LanguageCode _applicationLanguage;
    private readonly Grid _table;

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
        _table = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("2*,1.5*,Auto,Auto"),
            ColumnSpacing = 16
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
                _table
            }
        };

        SetRows(rows);
    }

    /// <summary>
    /// Replaces the currently visible rows while preserving shared column definitions.
    /// </summary>
    public void SetRows(IReadOnlyList<PlanetPositionSummaryRow> rows)
    {
        ArgumentNullException.ThrowIfNull(rows);

        _table.Children.Clear();
        _table.RowDefinitions.Clear();
        AddHeaderRow();

        for (var index = 0; index < rows.Count; index++)
        {
            AddDataRow(rows[index], index + 1);
        }
    }

    private void AddHeaderRow()
    {
        _table.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        AddCell(Localize("ui.chart.positions.header.planet"), 0, 0, FontWeight.SemiBold, fontSize: 12d);
        AddCell(Localize("ui.chart.positions.header.sign"), 0, 1, FontWeight.SemiBold, fontSize: 12d);
        AddCell(
            Localize("ui.chart.positions.header.position"),
            0,
            2,
            FontWeight.SemiBold,
            HorizontalAlignment.Right,
            12d);
        AddCell(
            Localize("ui.chart.positions.header.retrograde"),
            0,
            3,
            FontWeight.SemiBold,
            HorizontalAlignment.Center,
            12d);
    }

    private void AddDataRow(PlanetPositionSummaryRow row, int rowIndex)
    {
        _table.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        AddCell(Localize(row.PlanetLabelKey), rowIndex, 0);
        AddCell(Localize(row.SignLabelKey), rowIndex, 1);
        AddCell(row.PositionText, rowIndex, 2, horizontalAlignment: HorizontalAlignment.Right);
        AddCell(
            row.IsRetrograde ? Localize("ui.chart.positions.retrograde_marker") : string.Empty,
            rowIndex,
            3,
            horizontalAlignment: HorizontalAlignment.Center);
    }

    private void AddCell(
        string text,
        int row,
        int column,
        FontWeight? fontWeight = null,
        HorizontalAlignment horizontalAlignment = HorizontalAlignment.Left,
        double? fontSize = null)
    {
        var textBlock = new TextBlock
        {
            Text = text,
            Margin = new Thickness(0, 4, 0, 4),
            HorizontalAlignment = horizontalAlignment,
            VerticalAlignment = VerticalAlignment.Center
        };

        if (fontWeight is { } resolvedFontWeight)
        {
            textBlock.FontWeight = resolvedFontWeight;
        }

        if (fontSize is { } resolvedFontSize)
        {
            textBlock.FontSize = resolvedFontSize;
        }

        Grid.SetRow(textBlock, row);
        Grid.SetColumn(textBlock, column);
        _table.Children.Add(textBlock);
    }

    private string Localize(string key) => Localize(new LocalizationKey(key));

    private string Localize(LocalizationKey key) =>
        _localizationProvider.Get(LocalizationScope.Ui, _applicationLanguage, key).Text;

}
