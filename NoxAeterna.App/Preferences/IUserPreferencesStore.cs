using NoxAeterna.Presentation.Preferences;

namespace NoxAeterna.App.Preferences;

/// <summary>Loads and atomically saves the one application preference document.</summary>
public interface IUserPreferencesStore
{
    /// <summary>Gets the concrete settings document path.</summary>
    string SettingsPath { get; }

    /// <summary>Loads normalized preferences or controlled defaults.</summary>
    UserPreferencesLoadResult Load();

    /// <summary>Saves one normalized preference snapshot.</summary>
    UserPreferencesSaveResult Save(UserPreferences preferences);
}
