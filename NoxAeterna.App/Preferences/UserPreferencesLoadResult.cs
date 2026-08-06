using NoxAeterna.Presentation.Preferences;

namespace NoxAeterna.App.Preferences;

/// <summary>Represents normalized preferences and an optional controlled load diagnostic.</summary>
public sealed record UserPreferencesLoadResult(
    UserPreferences Preferences,
    UserPreferencesDiagnostic? Diagnostic);

/// <summary>Represents the controlled outcome of an atomic settings save.</summary>
public sealed record UserPreferencesSaveResult(UserPreferencesDiagnostic? Diagnostic)
{
    /// <summary>Gets whether the save completed successfully.</summary>
    public bool IsSuccess => Diagnostic is null;

    /// <summary>Creates a successful result.</summary>
    public static UserPreferencesSaveResult Success { get; } = new((UserPreferencesDiagnostic?)null);
}
