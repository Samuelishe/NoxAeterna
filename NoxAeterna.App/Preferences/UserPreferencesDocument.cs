namespace NoxAeterna.App.Preferences;

/// <summary>Defines the App-owned versioned JSON transport document.</summary>
public sealed class UserPreferencesDocument
{
    public int SchemaVersion { get; set; }

    public string? ApplicationLanguage { get; set; }

    public string? InterpretationLanguage { get; set; }

    public string? Theme { get; set; }

    public TarotWorkspacePreferencesDocument? Tarot { get; set; }
}

/// <summary>Defines the simple JSON transport fields for the Tarot workspace.</summary>
public sealed class TarotWorkspacePreferencesDocument
{
    public string? SpreadId { get; set; }

    public string? ArtworkPackId { get; set; }

    public string? SelectedInterpretationPackId { get; set; }

    public string? BackVariantId { get; set; }

    public bool AllowReversed { get; set; }

    public bool AutoRevealCards { get; set; } = true;
}
