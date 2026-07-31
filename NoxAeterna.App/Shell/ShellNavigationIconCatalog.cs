using NoxAeterna.Presentation.Shell;

namespace NoxAeterna.App.Shell;

/// <summary>
/// Materializes the original project-owned shell vectors in one normalized 24-by-24 coordinate system.
/// </summary>
public static class ShellNavigationIconCatalog
{
    /// <summary>
    /// Gets the vector path data for an icon identifier.
    /// </summary>
    public static string GetPathData(ShellNavigationIconId iconId) => iconId switch
    {
        ShellNavigationIconId.Astrology =>
            "M12,3 A9,9 0 1 1 11.99,3 M12,7 L12,17 M7,12 L17,12",
        ShellNavigationIconId.Tarot =>
            "M6,3 L18,3 L18,21 L6,21 Z M12,7 L16,12 L12,17 L8,12 Z",
        ShellNavigationIconId.Archive =>
            "M3,4 L21,4 L21,8 L3,8 Z M5,8 L19,8 L19,20 L5,20 Z M9,12 L15,12",
        ShellNavigationIconId.Settings =>
            "M12,8 A4,4 0 1 1 11.99,8 M12,2 L12,6 M12,18 L12,22 M2,12 L6,12 M18,12 L22,12 M5,5 L8,8 M16,16 L19,19 M19,5 L16,8 M8,16 L5,19",
        ShellNavigationIconId.Collapse =>
            "M19,4 L19,20 M15,5 L8,12 L15,19",
        ShellNavigationIconId.Expand =>
            "M5,4 L5,20 M9,5 L16,12 L9,19",
        _ => throw new ArgumentOutOfRangeException(nameof(iconId), iconId, "Unknown shell navigation icon.")
    };

    /// <summary>
    /// Creates an Avalonia geometry for the requested icon.
    /// </summary>
    public static Avalonia.Media.Geometry CreateGeometry(ShellNavigationIconId iconId) =>
        Avalonia.Media.Geometry.Parse(GetPathData(iconId));
}
