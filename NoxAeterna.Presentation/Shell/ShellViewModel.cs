using NoxAeterna.Presentation.Localization;

namespace NoxAeterna.Presentation.Shell;

/// <summary>
/// Represents the minimal application shell state.
/// </summary>
public sealed class ShellViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ShellViewModel"/> class.
    /// </summary>
    /// <param name="windowTitleKey">The localization key for the shell window title.</param>
    /// <param name="navigationItems">The top-level navigation items.</param>
    /// <param name="selectedSectionId">The initially selected shell section.</param>
    public ShellViewModel(
        LocalizationKey windowTitleKey,
        IEnumerable<ShellNavigationItem> navigationItems,
        ShellSectionId selectedSectionId)
    {
        var copiedItems = (navigationItems ?? throw new ArgumentNullException(nameof(navigationItems))).ToArray();

        if (copiedItems.Length == 0)
        {
            throw new ArgumentException("Shell navigation must contain at least one item.", nameof(navigationItems));
        }

        if (!copiedItems.Any(item => item.Id == selectedSectionId))
        {
            throw new ArgumentException("The selected shell section must exist in the navigation items.", nameof(selectedSectionId));
        }

        WindowTitleKey = windowTitleKey;
        NavigationItems = Array.AsReadOnly(copiedItems);
        SelectedSectionId = selectedSectionId;
    }

    /// <summary>
    /// Gets the localization key for the shell window title.
    /// </summary>
    public LocalizationKey WindowTitleKey { get; }

    /// <summary>
    /// Gets the top-level shell navigation items.
    /// </summary>
    public IReadOnlyList<ShellNavigationItem> NavigationItems { get; }

    /// <summary>
    /// Gets or sets the currently selected shell section.
    /// </summary>
    public ShellSectionId SelectedSectionId { get; set; }

    /// <summary>
    /// Creates the current default shell state.
    /// </summary>
    /// <returns>A minimal shell view model.</returns>
    public static ShellViewModel CreateDefault() =>
        new(
            new LocalizationKey("ui.shell.window_title"),
            new[]
            {
                new ShellNavigationItem(ShellSectionId.Astrology, new LocalizationKey("ui.shell.section.astrology")),
                new ShellNavigationItem(ShellSectionId.Tarot, new LocalizationKey("ui.shell.section.tarot")),
                new ShellNavigationItem(ShellSectionId.Archive, new LocalizationKey("ui.shell.section.archive")),
                new ShellNavigationItem(ShellSectionId.Settings, new LocalizationKey("ui.shell.section.settings"))
            },
            ShellSectionId.Astrology);
}
