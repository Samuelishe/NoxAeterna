namespace NoxAeterna.App.Preferences;

/// <summary>Identifies a controlled settings persistence failure.</summary>
public enum UserPreferencesDiagnosticCode
{
    MalformedJson,
    UnsupportedSchemaVersion,
    ReadFailure,
    SaveFailure
}

/// <summary>Provides structured, stack-trace-free settings diagnostics.</summary>
public sealed record UserPreferencesDiagnostic(UserPreferencesDiagnosticCode Code, string Message);
