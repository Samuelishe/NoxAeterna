using NoxAeterna.Presentation.Localization;

namespace NoxAeterna.Presentation.Astrology;

/// <summary>
/// Represents either compact ASC/MC rows or a localized unavailable status.
/// </summary>
public sealed record ChartAngleSummary
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChartAngleSummary"/> class.
    /// </summary>
    public ChartAngleSummary(
        IEnumerable<ChartAngleSummaryRow> rows,
        LocalizationKey? unavailableStatusKey = null)
    {
        Rows = Array.AsReadOnly(
            (rows ?? throw new ArgumentNullException(nameof(rows))).ToArray());
        UnavailableStatusKey = unavailableStatusKey;
    }

    /// <summary>
    /// Gets the available angle rows.
    /// </summary>
    public IReadOnlyList<ChartAngleSummaryRow> Rows { get; }

    /// <summary>
    /// Gets the localized status shown when rows are unavailable.
    /// </summary>
    public LocalizationKey? UnavailableStatusKey { get; }

    /// <summary>
    /// Gets whether calculated angle rows are available.
    /// </summary>
    public bool IsAvailable => Rows.Count > 0;
}
