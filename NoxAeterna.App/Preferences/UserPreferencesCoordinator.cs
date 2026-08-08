using NoxAeterna.Presentation.Preferences;

namespace NoxAeterna.App.Preferences;

/// <summary>Owns the single current preference snapshot and saves only actual changes.</summary>
public sealed class UserPreferencesCoordinator
{
    private readonly IUserPreferencesStore store;

    public UserPreferencesCoordinator(IUserPreferencesStore store, UserPreferencesLoadResult loadResult)
    {
        this.store = store ?? throw new ArgumentNullException(nameof(store));
        ArgumentNullException.ThrowIfNull(loadResult);
        Current = loadResult.Preferences;
        LastDiagnostic = loadResult.Diagnostic;
    }

    public event EventHandler<UserPreferences>? PreferencesChanged;

    public UserPreferences Current { get; private set; }

    public UserPreferencesDiagnostic? LastDiagnostic { get; private set; }

    public bool ApplyApplicationPreferences(UserPreferences source) => Update(current => current with
    {
        ApplicationLanguage = source.ApplicationLanguage,
        InterpretationLanguage = source.InterpretationLanguage,
        ThemeId = source.ThemeId
    });

    public bool ApplyTarotPreferences(TarotWorkspacePreferences tarot) =>
        Update(current => current with { Tarot = tarot });

    public bool ApplyWindowPlacement(WindowPlacementPreference placement)
    {
        ArgumentNullException.ThrowIfNull(placement);
        return Update(current => current with { WindowPlacement = placement });
    }

    private bool Update(Func<UserPreferences, UserPreferences> update)
    {
        var next = update(Current);
        if (next == Current)
        {
            return false;
        }

        Current = next;
        LastDiagnostic = store.Save(Current).Diagnostic;
        PreferencesChanged?.Invoke(this, Current);
        return true;
    }
}
