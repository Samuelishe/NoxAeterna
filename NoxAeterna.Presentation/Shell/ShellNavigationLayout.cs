namespace NoxAeterna.Presentation.Shell;

/// <summary>
/// Owns the single adaptive shell navigation size contract.
/// </summary>
public static class ShellNavigationLayout
{
    /// <summary>
    /// Gets the expanded inline pane length in device-independent pixels.
    /// </summary>
    public const double ExpandedPaneLength = 232d;

    /// <summary>
    /// Gets the collapsed rail length in device-independent pixels.
    /// </summary>
    public const double CompactPaneLength = 68d;

    /// <summary>
    /// Gets the viewport width below which the pane is forced to its compact rail.
    /// </summary>
    public const double CompactViewportThreshold = 1280d;
}
