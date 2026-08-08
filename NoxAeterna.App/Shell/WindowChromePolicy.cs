using Avalonia.Controls;

namespace NoxAeterna.App.Shell;

/// <summary>Identifies desktop platforms for the main-window decoration policy.</summary>
public enum DesktopWindowPlatform
{
    Windows,
    MacOS,
    Linux,
    Other
}

/// <summary>Applies project chrome only where the native integration contract is verified.</summary>
public static class WindowChromePolicy
{
    /// <summary>Gets the compact project caption height in DIPs.</summary>
    public const double CaptionHeight = 36d;

    /// <summary>Gets whether the platform uses project-owned extended-area chrome.</summary>
    public static bool UsesProjectOwnedChrome(DesktopWindowPlatform platform) =>
        platform == DesktopWindowPlatform.Windows;

    /// <summary>Detects the current desktop platform.</summary>
    public static DesktopWindowPlatform DetectCurrentPlatform()
    {
        if (OperatingSystem.IsWindows())
        {
            return DesktopWindowPlatform.Windows;
        }

        if (OperatingSystem.IsMacOS())
        {
            return DesktopWindowPlatform.MacOS;
        }

        if (OperatingSystem.IsLinux())
        {
            return DesktopWindowPlatform.Linux;
        }

        return DesktopWindowPlatform.Other;
    }

    /// <summary>Configures the main window while retaining native decorations on non-Windows desktops.</summary>
    public static bool Apply(Window window)
    {
        ArgumentNullException.ThrowIfNull(window);
        var useProjectChrome = UsesProjectOwnedChrome(DetectCurrentPlatform());
        window.WindowDecorations = WindowDecorations.Full;
        window.ExtendClientAreaToDecorationsHint = useProjectChrome;
        window.ExtendClientAreaTitleBarHeightHint = useProjectChrome ? CaptionHeight : -1d;
        window.ShowInTaskbar = true;
        return useProjectChrome;
    }
}
