namespace NoxAeterna.App.Preferences;

/// <summary>Resolves the platform user-data location for settings.</summary>
public static class UserPreferencesPathResolver
{
    /// <summary>Gets the settings path below a supplied or platform LocalApplicationData root.</summary>
    public static string GetSettingsPath(string? localApplicationDataRoot = null)
    {
        var root = localApplicationDataRoot ?? Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (string.IsNullOrWhiteSpace(root))
        {
            throw new InvalidOperationException("The local application data directory is unavailable.");
        }

        return Path.GetFullPath(Path.Combine(root, "NoxAeterna", "settings.json"));
    }
}
