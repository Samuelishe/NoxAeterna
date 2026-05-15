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
    /// <param name="workspaceHintKey">The localization key describing the current workspace state.</param>
    /// <param name="panels">The workspace panels.</param>
    public AstrologyWorkspaceViewModel(
        LocalizationKey workspaceHintKey,
        IEnumerable<AstrologyWorkspacePanel> panels,
        BirthDataInputViewModel birthDataInput)
    {
        var copiedPanels = (panels ?? throw new ArgumentNullException(nameof(panels))).ToArray();

        if (copiedPanels.Length == 0)
        {
            throw new ArgumentException("Astrology workspace must expose at least one panel.", nameof(panels));
        }

        WorkspaceHintKey = workspaceHintKey;
        Panels = Array.AsReadOnly(copiedPanels);
        BirthDataInput = birthDataInput ?? throw new ArgumentNullException(nameof(birthDataInput));
    }

    /// <summary>
    /// Gets the localization key describing the current workspace state.
    /// </summary>
    public LocalizationKey WorkspaceHintKey { get; }

    /// <summary>
    /// Gets the workspace panels in deterministic order.
    /// </summary>
    public IReadOnlyList<AstrologyWorkspacePanel> Panels { get; }

    /// <summary>
    /// Gets the localized notice explaining that the current chart calculation is still demo-only.
    /// </summary>
    public LocalizationKey DemoCalculationNoticeKey { get; } = new("ui.astrology.demo_calculation_notice");

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
            new LocalizationKey("ui.astrology.workspace.hint"),
            new[]
            {
                new AstrologyWorkspacePanel(
                    AstrologyWorkspacePanelId.Chart,
                    new LocalizationKey("ui.astrology.panel.chart.title"),
                    new LocalizationKey("ui.astrology.panel.chart.description")),
                new AstrologyWorkspacePanel(
                    AstrologyWorkspacePanelId.BirthData,
                    new LocalizationKey("ui.astrology.panel.birth_data.title"),
                    new LocalizationKey("ui.astrology.panel.birth_data.description")),
                new AstrologyWorkspacePanel(
                    AstrologyWorkspacePanelId.Interpretation,
                    new LocalizationKey("ui.astrology.panel.interpretation.title"),
                    new LocalizationKey("ui.astrology.panel.interpretation.description"))
            },
            BirthDataInputViewModel.CreateDefault());
}
