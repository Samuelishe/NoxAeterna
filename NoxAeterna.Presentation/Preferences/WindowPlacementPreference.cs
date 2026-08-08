namespace NoxAeterna.Presentation.Preferences;

/// <summary>
/// Stores normal window geometry independently from the current desktop materialization.
/// </summary>
/// <param name="NormalX">Normal-window X offset from the source work area in DIPs.</param>
/// <param name="NormalY">Normal-window Y offset from the source work area in DIPs.</param>
/// <param name="NormalWidth">Normal-window width in DIPs.</param>
/// <param name="NormalHeight">Normal-window height in DIPs.</param>
/// <param name="IsMaximized">Whether the last meaningful non-minimized state was maximized.</param>
/// <param name="ScreenId">Optional platform display identity.</param>
/// <param name="SourceWorkAreaX">Source work-area X in physical pixels.</param>
/// <param name="SourceWorkAreaY">Source work-area Y in physical pixels.</param>
/// <param name="SourceWorkAreaWidth">Source work-area width in physical pixels.</param>
/// <param name="SourceWorkAreaHeight">Source work-area height in physical pixels.</param>
/// <param name="SourceScaling">Source monitor physical-pixels-per-DIP scaling.</param>
public sealed record WindowPlacementPreference(
    double NormalX,
    double NormalY,
    double NormalWidth,
    double NormalHeight,
    bool IsMaximized,
    string? ScreenId,
    int SourceWorkAreaX,
    int SourceWorkAreaY,
    int SourceWorkAreaWidth,
    int SourceWorkAreaHeight,
    double SourceScaling);
