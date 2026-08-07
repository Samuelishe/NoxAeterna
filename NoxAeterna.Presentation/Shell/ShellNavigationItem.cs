using NoxAeterna.Presentation.Localization;

namespace NoxAeterna.Presentation.Shell;

/// <summary>
/// Represents one top-level shell navigation item.
/// </summary>
public sealed record ShellNavigationItem
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ShellNavigationItem"/> class.
    /// </summary>
    /// <param name="id">The shell section identifier.</param>
    /// <param name="labelKey">The localization key for the visible label.</param>
    /// <param name="iconId">The project-owned navigation icon identifier.</param>
    /// <param name="isTemporary">A value indicating whether the section is temporary infrastructure.</param>
    /// <param name="showHeader">A value indicating whether the workspace needs a shell-owned section header.</param>
    public ShellNavigationItem(
        ShellSectionId id,
        LocalizationKey labelKey,
        ShellNavigationIconId iconId,
        bool isTemporary = false,
        bool showHeader = true)
    {
        Id = id;
        LabelKey = labelKey;
        IconId = iconId;
        IsTemporary = isTemporary;
        ShowHeader = showHeader;
    }

    /// <summary>
    /// Gets the shell section identifier.
    /// </summary>
    public ShellSectionId Id { get; }

    /// <summary>
    /// Gets the localization key for the visible label.
    /// </summary>
    public LocalizationKey LabelKey { get; }

    /// <summary>
    /// Gets the project-owned navigation icon identifier.
    /// </summary>
    public ShellNavigationIconId IconId { get; }

    /// <summary>
    /// Gets a value indicating whether the section is temporary infrastructure rather than product UI.
    /// </summary>
    public bool IsTemporary { get; }

    /// <summary>Gets a value indicating whether the shell should reserve and show its section header.</summary>
    public bool ShowHeader { get; }
}
