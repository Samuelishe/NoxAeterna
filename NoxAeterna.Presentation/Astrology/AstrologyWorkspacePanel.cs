using NoxAeterna.Presentation.Localization;

namespace NoxAeterna.Presentation.Astrology;

/// <summary>
/// Represents one panel inside the astrology workspace foundation.
/// </summary>
public sealed record AstrologyWorkspacePanel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AstrologyWorkspacePanel"/> class.
    /// </summary>
    /// <param name="id">The workspace panel identifier.</param>
    /// <param name="titleKey">The localization key for the panel title.</param>
    /// <param name="descriptionKey">The localization key for the current panel description.</param>
    public AstrologyWorkspacePanel(
        AstrologyWorkspacePanelId id,
        LocalizationKey titleKey,
        LocalizationKey descriptionKey)
    {
        Id = id;
        TitleKey = titleKey;
        DescriptionKey = descriptionKey;
    }

    /// <summary>
    /// Gets the workspace panel identifier.
    /// </summary>
    public AstrologyWorkspacePanelId Id { get; }

    /// <summary>
    /// Gets the localization key for the panel title.
    /// </summary>
    public LocalizationKey TitleKey { get; }

    /// <summary>
    /// Gets the localization key for the current panel description.
    /// </summary>
    public LocalizationKey DescriptionKey { get; }
}
