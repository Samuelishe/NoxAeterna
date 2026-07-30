using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using NoxAeterna.Presentation.Astrology;
using NoxAeterna.Presentation.Localization;

namespace NoxAeterna.App.Astrology;

/// <summary>
/// Renders a compact localized ASC/MC summary below the positions table.
/// </summary>
public sealed class ChartAngleSummaryControl : UserControl
{
    private readonly ILocalizationProvider _localizationProvider;
    private readonly LanguageCode _applicationLanguage;
    private readonly Grid _contentGrid;
    private readonly TextBlock _statusText;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChartAngleSummaryControl"/> class.
    /// </summary>
    public ChartAngleSummaryControl(
        ILocalizationProvider localizationProvider,
        LanguageCode applicationLanguage,
        ChartAngleSummary summary)
    {
        _localizationProvider = localizationProvider ?? throw new ArgumentNullException(nameof(localizationProvider));
        _applicationLanguage = applicationLanguage;
        _contentGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto"),
            ColumnSpacing = 16
        };
        _statusText = new TextBlock
        {
            Opacity = 0.72d,
            TextWrapping = TextWrapping.Wrap
        };

        Content = new StackPanel
        {
            Spacing = 9,
            Children =
            {
                new TextBlock
                {
                    Text = Localize("ui.chart.angles.title"),
                    FontSize = 14,
                    FontWeight = FontWeight.SemiBold
                },
                _contentGrid,
                _statusText
            }
        };

        SetSummary(summary);
    }

    /// <summary>
    /// Replaces the visible rows or unavailable status.
    /// </summary>
    public void SetSummary(ChartAngleSummary summary)
    {
        ArgumentNullException.ThrowIfNull(summary);

        _contentGrid.Children.Clear();
        _contentGrid.RowDefinitions.Clear();
        _contentGrid.IsVisible = summary.IsAvailable;
        _statusText.IsVisible = !summary.IsAvailable;
        _statusText.Text = summary.UnavailableStatusKey is { } statusKey
            ? Localize(statusKey)
            : string.Empty;

        for (var index = 0; index < summary.Rows.Count; index++)
        {
            _contentGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            AddCell(Localize(summary.Rows[index].AngleLabelKey), index, 0, FontWeight.SemiBold);
            AddCell(Localize(summary.Rows[index].SignLabelKey), index, 1);
            AddCell(
                summary.Rows[index].PositionText,
                index,
                2,
                horizontalAlignment: HorizontalAlignment.Right);
        }
    }

    private void AddCell(
        string text,
        int row,
        int column,
        FontWeight? fontWeight = null,
        HorizontalAlignment horizontalAlignment = HorizontalAlignment.Left)
    {
        var cell = new TextBlock
        {
            Text = text,
            Margin = new Thickness(0, 3, 0, 3),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = horizontalAlignment
        };

        if (fontWeight is { } resolvedFontWeight)
        {
            cell.FontWeight = resolvedFontWeight;
        }

        Grid.SetRow(cell, row);
        Grid.SetColumn(cell, column);
        _contentGrid.Children.Add(cell);
    }

    private string Localize(string key) => Localize(new LocalizationKey(key));

    private string Localize(LocalizationKey key) =>
        _localizationProvider.Get(LocalizationScope.Ui, _applicationLanguage, key).Text;
}
