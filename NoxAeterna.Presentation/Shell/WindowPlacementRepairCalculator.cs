using NoxAeterna.Presentation.Preferences;

namespace NoxAeterna.Presentation.Shell;

/// <summary>Describes one current monitor work area in physical pixels.</summary>
public sealed record WindowPlacementScreen(
    string? Id,
    int X,
    int Y,
    int Width,
    int Height,
    double Scaling,
    bool IsPrimary);

/// <summary>Defines the intended and minimum normal-window size in DIPs.</summary>
public sealed record WindowPlacementConstraints(
    double DefaultWidth,
    double DefaultHeight,
    double MinimumWidth,
    double MinimumHeight);

/// <summary>Provides one repaired startup placement with a physical-pixel position and DIP size.</summary>
public sealed record RepairedWindowPlacement(
    int X,
    int Y,
    double Width,
    double Height,
    double EffectiveMinimumWidth,
    double EffectiveMinimumHeight,
    bool IsMaximized,
    WindowPlacementScreen Screen);

/// <summary>Repairs persisted normal bounds against the current monitor topology.</summary>
public static class WindowPlacementRepairCalculator
{
    /// <summary>Creates a visible, work-area-contained startup placement.</summary>
    public static RepairedWindowPlacement Repair(
        WindowPlacementPreference? saved,
        IReadOnlyList<WindowPlacementScreen> screens,
        WindowPlacementConstraints constraints)
    {
        ArgumentNullException.ThrowIfNull(screens);
        ArgumentNullException.ThrowIfNull(constraints);
        if (screens.Count == 0)
        {
            throw new ArgumentException("At least one current screen is required.", nameof(screens));
        }

        ValidateConstraints(constraints);
        var hasUsableSavedPlacement = IsUsable(saved);
        var screen = SelectScreen(hasUsableSavedPlacement ? saved : null, screens);
        var workWidthDips = screen.Width / screen.Scaling;
        var workHeightDips = screen.Height / screen.Scaling;
        var effectiveMinimumWidth = Math.Min(constraints.MinimumWidth, workWidthDips);
        var effectiveMinimumHeight = Math.Min(constraints.MinimumHeight, workHeightDips);
        var requestedWidth = hasUsableSavedPlacement ? saved!.NormalWidth : constraints.DefaultWidth;
        var requestedHeight = hasUsableSavedPlacement ? saved!.NormalHeight : constraints.DefaultHeight;
        var width = Math.Clamp(requestedWidth, effectiveMinimumWidth, workWidthDips);
        var height = Math.Clamp(requestedHeight, effectiveMinimumHeight, workHeightDips);

        var availableWidthDips = Math.Max(0d, workWidthDips - width);
        var availableHeightDips = Math.Max(0d, workHeightDips - height);
        var relativeX = hasUsableSavedPlacement
            ? CalculateRelativeOffset(
                saved!.NormalX,
                (saved.SourceWorkAreaWidth / saved.SourceScaling) - saved.NormalWidth)
            : 0.5d;
        var relativeY = hasUsableSavedPlacement
            ? CalculateRelativeOffset(
                saved!.NormalY,
                (saved.SourceWorkAreaHeight / saved.SourceScaling) - saved.NormalHeight)
            : 0.5d;
        var x = screen.X + RoundToPixel(relativeX * availableWidthDips * screen.Scaling);
        var y = screen.Y + RoundToPixel(relativeY * availableHeightDips * screen.Scaling);

        return new RepairedWindowPlacement(
            x,
            y,
            width,
            height,
            effectiveMinimumWidth,
            effectiveMinimumHeight,
            hasUsableSavedPlacement && saved!.IsMaximized,
            screen);
    }

    private static WindowPlacementScreen SelectScreen(
        WindowPlacementPreference? saved,
        IReadOnlyList<WindowPlacementScreen> screens)
    {
        if (!string.IsNullOrWhiteSpace(saved?.ScreenId))
        {
            var matchingScreen = screens.FirstOrDefault(screen =>
                string.Equals(screen.Id, saved.ScreenId, StringComparison.Ordinal));
            if (matchingScreen is not null && IsUsable(matchingScreen))
            {
                return matchingScreen;
            }
        }

        if (saved is not null)
        {
            var sourceLeft = (long)saved.SourceWorkAreaX;
            var sourceTop = (long)saved.SourceWorkAreaY;
            var sourceRight = sourceLeft + saved.SourceWorkAreaWidth;
            var sourceBottom = sourceTop + saved.SourceWorkAreaHeight;
            var overlappingScreen = screens
                .Where(IsUsable)
                .Select(screen => new
                {
                    Screen = screen,
                    Area = IntersectionArea(
                        sourceLeft,
                        sourceTop,
                        sourceRight,
                        sourceBottom,
                        screen.X,
                        screen.Y,
                        (long)screen.X + screen.Width,
                        (long)screen.Y + screen.Height)
                })
                .OrderByDescending(candidate => candidate.Area)
                .FirstOrDefault(candidate => candidate.Area > 0L);
            if (overlappingScreen is not null)
            {
                return overlappingScreen.Screen;
            }
        }

        return screens.FirstOrDefault(screen => screen.IsPrimary && IsUsable(screen))
               ?? screens.First(IsUsable);
    }

    private static bool IsUsable(WindowPlacementPreference? saved) =>
        saved is not null &&
        double.IsFinite(saved.NormalX) &&
        double.IsFinite(saved.NormalY) &&
        double.IsFinite(saved.NormalWidth) &&
        saved.NormalWidth > 0d &&
        double.IsFinite(saved.NormalHeight) &&
        saved.NormalHeight > 0d &&
        saved.SourceWorkAreaWidth > 0 &&
        saved.SourceWorkAreaHeight > 0 &&
        double.IsFinite(saved.SourceScaling) &&
        saved.SourceScaling > 0d;

    private static bool IsUsable(WindowPlacementScreen screen) =>
        screen.Width > 0 &&
        screen.Height > 0 &&
        double.IsFinite(screen.Scaling) &&
        screen.Scaling > 0d;

    private static double CalculateRelativeOffset(double savedOffset, double savedAvailableLength)
    {
        if (!double.IsFinite(savedAvailableLength) || savedAvailableLength <= 0d)
        {
            return 0.5d;
        }

        return Math.Clamp(savedOffset / savedAvailableLength, 0d, 1d);
    }

    private static long IntersectionArea(
        long firstLeft,
        long firstTop,
        long firstRight,
        long firstBottom,
        long secondLeft,
        long secondTop,
        long secondRight,
        long secondBottom)
    {
        var width = Math.Max(0L, Math.Min(firstRight, secondRight) - Math.Max(firstLeft, secondLeft));
        var height = Math.Max(0L, Math.Min(firstBottom, secondBottom) - Math.Max(firstTop, secondTop));
        return width * height;
    }

    private static int RoundToPixel(double value) => checked((int)Math.Round(value));

    private static void ValidateConstraints(WindowPlacementConstraints constraints)
    {
        if (!double.IsFinite(constraints.DefaultWidth) || constraints.DefaultWidth <= 0d ||
            !double.IsFinite(constraints.DefaultHeight) || constraints.DefaultHeight <= 0d ||
            !double.IsFinite(constraints.MinimumWidth) || constraints.MinimumWidth <= 0d ||
            !double.IsFinite(constraints.MinimumHeight) || constraints.MinimumHeight <= 0d)
        {
            throw new ArgumentOutOfRangeException(nameof(constraints));
        }
    }
}
