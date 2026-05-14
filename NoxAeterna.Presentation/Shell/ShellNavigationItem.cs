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
    /// <param name="isTemporary">A value indicating whether the section is temporary infrastructure.</param>
    public ShellNavigationItem(
        ShellSectionId id,
        LocalizationKey labelKey,
        bool isTemporary = false)
    {
        Id = id;
        LabelKey = labelKey;
        IsTemporary = isTemporary;
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
    /// Gets a value indicating whether the section is temporary infrastructure rather than product UI.
    /// </summary>
    public bool IsTemporary { get; }
}
