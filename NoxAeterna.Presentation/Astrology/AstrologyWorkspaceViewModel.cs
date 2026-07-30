using NoxAeterna.Presentation.Localization;

namespace NoxAeterna.Presentation.Astrology;

/// <summary>
/// Represents the first reusable astrology workspace foundation.
/// </summary>
public sealed class AstrologyWorkspaceViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AstrologyWorkspaceViewModel"/> class.
    /// </summary>
    /// <param name="panels">The workspace panels.</param>
    public AstrologyWorkspaceViewModel(
        IEnumerable<AstrologyWorkspacePanel> panels,
        BirthDataInputViewModel birthDataInput)
    {
        var copiedPanels = (panels ?? throw new ArgumentNullException(nameof(panels))).ToArray();

        if (copiedPanels.Length == 0)
        {
            throw new ArgumentException("Astrology workspace must expose at least one panel.", nameof(panels));
        }

        Panels = Array.AsReadOnly(copiedPanels);
        BirthDataInput = birthDataInput ?? throw new ArgumentNullException(nameof(birthDataInput));
    }

    /// <summary>
    /// Gets the workspace panels in deterministic order.
    /// </summary>
    public IReadOnlyList<AstrologyWorkspacePanel> Panels { get; }

    /// <summary>
    /// Gets the current birth-data input foundation.
    /// </summary>
    public BirthDataInputViewModel BirthDataInput { get; }

    /// <summary>
    /// Creates the current default astrology workspace foundation.
    /// </summary>
    /// <returns>A deterministic astrology workspace view model.</returns>
    public static AstrologyWorkspaceViewModel CreateFoundation() =>
        new(
            new[]
            {
                new AstrologyWorkspacePanel(
                    AstrologyWorkspacePanelId.Chart,
                    new LocalizationKey("ui.astrology.panel.chart.title")),
                new AstrologyWorkspacePanel(
                    AstrologyWorkspacePanelId.BirthData,
                    new LocalizationKey("ui.astrology.panel.birth_data.title")),
                new AstrologyWorkspacePanel(
                    AstrologyWorkspacePanelId.Interpretation,
                    new LocalizationKey("ui.astrology.panel.interpretation.title"))
            },
            BirthDataInputViewModel.CreateDefault());
}
