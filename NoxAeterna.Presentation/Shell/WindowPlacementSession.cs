using NoxAeterna.Presentation.Preferences;

namespace NoxAeterna.Presentation.Shell;

/// <summary>Identifies the platform window states relevant to persisted startup behavior.</summary>
public enum WindowPlacementState
{
    Normal,
    Minimized,
    Maximized
}

/// <summary>Keeps the last normal bounds and last meaningful non-minimized state in memory.</summary>
public sealed class WindowPlacementSession
{
    private WindowPlacementPreference normalPlacement;
    private bool isMaximized;

    /// <summary>Creates a session from one already repaired normal placement.</summary>
    public WindowPlacementSession(WindowPlacementPreference repairedPlacement)
    {
        normalPlacement = (repairedPlacement ?? throw new ArgumentNullException(nameof(repairedPlacement))) with
        {
            IsMaximized = false
        };
        isMaximized = repairedPlacement.IsMaximized;
    }

    /// <summary>Updates normal geometry without changing the remembered meaningful window state.</summary>
    public void ObserveNormalPlacement(WindowPlacementPreference placement)
    {
        normalPlacement = (placement ?? throw new ArgumentNullException(nameof(placement))) with
        {
            IsMaximized = false
        };
    }

    /// <summary>Updates the last meaningful state; minimized is intentionally ignored.</summary>
    public void ObserveState(WindowPlacementState state)
    {
        switch (state)
        {
            case WindowPlacementState.Normal:
                isMaximized = false;
                break;
            case WindowPlacementState.Maximized:
                isMaximized = true;
                break;
            case WindowPlacementState.Minimized:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, "Unknown window placement state.");
        }
    }

    /// <summary>Creates the single preference snapshot to persist at a successful close.</summary>
    public WindowPlacementPreference CreatePreference() => normalPlacement with
    {
        IsMaximized = isMaximized
    };
}
