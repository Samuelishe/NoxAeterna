using System.Runtime.InteropServices;
using Avalonia.Controls;

namespace NoxAeterna.App.Shell;

/// <summary>Restores the native Alt+Space system menu for Windows extended-client-area chrome.</summary>
public sealed class WindowsSystemMenuCoordinator : IDisposable
{
    private readonly Window window;
    private readonly Win32Properties.CustomWndProcHookCallback wndProcHook;
    private bool disposed;

    public WindowsSystemMenuCoordinator(Window window)
    {
        this.window = window ?? throw new ArgumentNullException(nameof(window));
        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException("The native system-menu coordinator is Windows-only.");
        }

        wndProcHook = OnWndProc;
        Win32Properties.AddWndProcHookCallback(window, wndProcHook);
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;
        Win32Properties.RemoveWndProcHookCallback(window, wndProcHook);
    }

    private IntPtr OnWndProc(IntPtr windowHandle, uint message, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        const uint windowMessageSystemCommand = 0x0112;
        const long systemCommandMask = 0xFFF0;
        const long systemCommandKeyMenu = 0xF100;
        if (message != windowMessageSystemCommand ||
            (wParam.ToInt64() & systemCommandMask) != systemCommandKeyMenu)
        {
            return IntPtr.Zero;
        }

        handled = true;
        WindowsSystemMenu.Show(windowHandle, window.RenderScaling, WindowChromePolicy.CaptionHeight);
        return IntPtr.Zero;
    }

    private static class WindowsSystemMenu
    {
        private const uint TrackPopupReturnCommand = 0x0100;
        private const uint TrackPopupRightButton = 0x0002;
        private const uint WindowMessageSystemCommand = 0x0112;
        private const uint WindowMessageNull = 0x0000;
        private const uint MenuFlagByCommand = 0x0000;
        private const uint MenuFlagEnabled = 0x0000;
        private const uint MenuFlagGrayed = 0x0001;
        private const uint SystemCommandSize = 0xF000;
        private const uint SystemCommandMove = 0xF010;
        private const uint SystemCommandMinimize = 0xF020;
        private const uint SystemCommandMaximize = 0xF030;
        private const uint SystemCommandClose = 0xF060;
        private const uint SystemCommandRestore = 0xF120;

        public static void Show(IntPtr windowHandle, double scaling, double captionHeight)
        {
            var menu = GetSystemMenu(windowHandle, revert: false);
            if (menu == IntPtr.Zero || !GetWindowRect(windowHandle, out var bounds))
            {
                return;
            }

            SetForegroundWindow(windowHandle);
            var isMinimized = IsIconic(windowHandle);
            var isMaximized = IsZoomed(windowHandle);
            SetEnabled(menu, SystemCommandRestore, isMinimized || isMaximized);
            SetEnabled(menu, SystemCommandMove, !isMinimized && !isMaximized);
            SetEnabled(menu, SystemCommandSize, !isMinimized && !isMaximized);
            SetEnabled(menu, SystemCommandMinimize, !isMinimized);
            SetEnabled(menu, SystemCommandMaximize, !isMaximized);
            SetEnabled(menu, SystemCommandClose, enabled: true);
            var x = bounds.Left + Math.Max(1, checked((int)Math.Round(8d * scaling)));
            var y = bounds.Top + Math.Max(1, checked((int)Math.Round(captionHeight * scaling)));
            var command = TrackPopupMenuEx(
                menu,
                TrackPopupReturnCommand | TrackPopupRightButton,
                x,
                y,
                windowHandle,
                IntPtr.Zero);
            if (command != 0)
            {
                PostMessage(windowHandle, WindowMessageSystemCommand, (IntPtr)command, IntPtr.Zero);
            }

            PostMessage(windowHandle, WindowMessageNull, IntPtr.Zero, IntPtr.Zero);
        }

        private static void SetEnabled(IntPtr menu, uint command, bool enabled) =>
            EnableMenuItem(
                menu,
                command,
                MenuFlagByCommand | (enabled ? MenuFlagEnabled : MenuFlagGrayed));

        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr windowHandle, [MarshalAs(UnmanagedType.Bool)] bool revert);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowRect(IntPtr windowHandle, out NativeRect rectangle);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr windowHandle);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsIconic(IntPtr windowHandle);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsZoomed(IntPtr windowHandle);

        [DllImport("user32.dll")]
        private static extern uint EnableMenuItem(IntPtr menu, uint item, uint enableFlags);

        [DllImport("user32.dll")]
        private static extern uint TrackPopupMenuEx(
            IntPtr menu,
            uint flags,
            int x,
            int y,
            IntPtr windowHandle,
            IntPtr parameters);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool PostMessage(
            IntPtr windowHandle,
            uint message,
            IntPtr wParam,
            IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        private struct NativeRect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
    }
}
