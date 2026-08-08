using System.Text.Json;

namespace NoxAeterna.App.Preferences;

/// <summary>Defines the App-owned versioned JSON transport document.</summary>
public sealed class UserPreferencesDocument
{
    public int SchemaVersion { get; set; }

    public string? ApplicationLanguage { get; set; }

    public string? InterpretationLanguage { get; set; }

    public string? Theme { get; set; }

    public TarotWorkspacePreferencesDocument? Tarot { get; set; }

    public JsonElement? WindowPlacement { get; set; }
}

/// <summary>Defines schema 3 JSON transport fields for normal window placement.</summary>
public sealed class WindowPlacementPreferencesDocument
{
    public double NormalX { get; set; }

    public double NormalY { get; set; }

    public double NormalWidth { get; set; }

    public double NormalHeight { get; set; }

    public bool IsMaximized { get; set; }

    public string? ScreenId { get; set; }

    public int SourceWorkAreaX { get; set; }

    public int SourceWorkAreaY { get; set; }

    public int SourceWorkAreaWidth { get; set; }

    public int SourceWorkAreaHeight { get; set; }

    public double SourceScaling { get; set; }
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
