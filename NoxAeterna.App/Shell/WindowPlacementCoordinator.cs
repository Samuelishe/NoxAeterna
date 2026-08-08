using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using NoxAeterna.App.Preferences;
using NoxAeterna.Presentation.Preferences;
using NoxAeterna.Presentation.Shell;

namespace NoxAeterna.App.Shell;

/// <summary>Adapts Avalonia window and screen lifecycle data to persisted placement preferences.</summary>
public sealed class WindowPlacementCoordinator : IDisposable
{
    private readonly Window window;
    private readonly UserPreferencesCoordinator preferencesCoordinator;
    private readonly double intendedMinimumWidth;
    private readonly double intendedMinimumHeight;
    private WindowPlacementSession? session;
    private bool disposed;

    public WindowPlacementCoordinator(
        Window window,
        UserPreferencesCoordinator preferencesCoordinator)
    {
        this.window = window ?? throw new ArgumentNullException(nameof(window));
        this.preferencesCoordinator = preferencesCoordinator
            ?? throw new ArgumentNullException(nameof(preferencesCoordinator));
        intendedMinimumWidth = window.MinWidth;
        intendedMinimumHeight = window.MinHeight;

        RestoreInitialPlacement();
        window.PositionChanged += OnPositionChanged;
        window.SizeChanged += OnSizeChanged;
        window.PropertyChanged += OnWindowPropertyChanged;
    }

    /// <summary>Persists one final semantic placement snapshot after close cancellation is resolved.</summary>
    public bool PersistAtClose()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (session is null)
        {
            return false;
        }

        if (window.WindowState == WindowState.Normal)
        {
            CaptureNormalPlacement();
        }

        return preferencesCoordinator.ApplyWindowPlacement(session.CreatePreference());
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;
        window.PositionChanged -= OnPositionChanged;
        window.SizeChanged -= OnSizeChanged;
        window.PropertyChanged -= OnWindowPropertyChanged;
    }

    private void RestoreInitialPlacement()
    {
        var screens = GetCurrentScreens();
        if (screens.Count == 0)
        {
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            return;
        }

        var repaired = WindowPlacementRepairCalculator.Repair(
            preferencesCoordinator.Current.WindowPlacement,
            screens,
            new WindowPlacementConstraints(
                window.Width,
                window.Height,
                intendedMinimumWidth,
                intendedMinimumHeight));
        window.MinWidth = repaired.EffectiveMinimumWidth;
        window.MinHeight = repaired.EffectiveMinimumHeight;
        window.Width = repaired.Width;
        window.Height = repaired.Height;
        window.WindowStartupLocation = WindowStartupLocation.Manual;
        window.Position = new PixelPoint(repaired.X, repaired.Y);

        var repairedPreference = CreatePreference(
            repaired.X,
            repaired.Y,
            repaired.Width,
            repaired.Height,
            repaired.Screen,
            repaired.IsMaximized);
        session = new WindowPlacementSession(repairedPreference);
        window.WindowState = repaired.IsMaximized
            ? WindowState.Maximized
            : WindowState.Normal;
    }

    private void OnPositionChanged(object? sender, PixelPointEventArgs e) => CaptureNormalPlacement();

    private void OnSizeChanged(object? sender, SizeChangedEventArgs e) => CaptureNormalPlacement();

    private void OnWindowPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property != Window.WindowStateProperty || session is null)
        {
            return;
        }

        session.ObserveState(window.WindowState switch
        {
            WindowState.Minimized => WindowPlacementState.Minimized,
            WindowState.Maximized or WindowState.FullScreen => WindowPlacementState.Maximized,
            _ => WindowPlacementState.Normal
        });
    }

    private void CaptureNormalPlacement()
    {
        if (disposed || session is null || window.WindowState != WindowState.Normal)
        {
            return;
        }

        var width = window.Bounds.Width;
        var height = window.Bounds.Height;
        if (!double.IsFinite(width) || width <= 0d || !double.IsFinite(height) || height <= 0d)
        {
            return;
        }

        var screen = window.Screens.ScreenFromWindow(window);
        if (screen is null)
        {
            return;
        }

        var placementScreen = CreateScreen(screen);
        window.MinWidth = Math.Min(intendedMinimumWidth, placementScreen.Width / placementScreen.Scaling);
        window.MinHeight = Math.Min(intendedMinimumHeight, placementScreen.Height / placementScreen.Scaling);
        session.ObserveNormalPlacement(CreatePreference(
            window.Position.X,
            window.Position.Y,
            width,
            height,
            placementScreen,
            isMaximized: false));
    }

    private IReadOnlyList<WindowPlacementScreen> GetCurrentScreens() =>
        window.Screens.All.Select(CreateScreen).ToArray();

    private static WindowPlacementScreen CreateScreen(Screen screen) => new(
        screen.DisplayName,
        screen.WorkingArea.X,
        screen.WorkingArea.Y,
        screen.WorkingArea.Width,
        screen.WorkingArea.Height,
        screen.Scaling,
        screen.IsPrimary);

    private static WindowPlacementPreference CreatePreference(
        int positionX,
        int positionY,
        double width,
        double height,
        WindowPlacementScreen screen,
        bool isMaximized) => new(
        (positionX - screen.X) / screen.Scaling,
        (positionY - screen.Y) / screen.Scaling,
        width,
        height,
        isMaximized,
        screen.Id,
        screen.X,
        screen.Y,
        screen.Width,
        screen.Height,
        screen.Scaling);
}
