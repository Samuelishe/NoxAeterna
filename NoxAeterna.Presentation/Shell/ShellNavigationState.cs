namespace NoxAeterna.Presentation.Shell;

/// <summary>
/// Tracks the session-only wide-mode preference and effective responsive navigation state.
/// </summary>
public sealed class ShellNavigationState
{
    /// <summary>
    /// Gets the user's last expanded preference while the shell had a wide viewport.
    /// </summary>
    public bool WideModeExpandedPreference { get; private set; } = true;

    /// <summary>
    /// Gets whether the current viewport forces the compact rail.
    /// </summary>
    public bool IsCompactViewport { get; private set; }

    /// <summary>
    /// Gets whether the navigation pane is effectively expanded.
    /// </summary>
    public bool IsExpanded => !IsCompactViewport && WideModeExpandedPreference;

    /// <summary>
    /// Toggles the wide-mode preference. Compact viewports preserve rather than mutate it.
    /// </summary>
    public void Toggle()
    {
        if (IsCompactViewport)
        {
            return;
        }

        WideModeExpandedPreference = !WideModeExpandedPreference;
    }

    /// <summary>
    /// Applies a measured shell viewport width.
    /// </summary>
    public void UpdateViewportWidth(double width)
    {
        if (!double.IsFinite(width) || width < 0d)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "Viewport width must be finite and non-negative.");
        }

        IsCompactViewport = width < ShellNavigationLayout.CompactViewportThreshold;
    }
}
