using NoxAeterna.Domain.Tarot;

namespace NoxAeterna.App.Tarot;

/// <summary>Describes the separately composed layers for one visible Tarot card face.</summary>
public sealed record TarotCardVisualPlan(
    TarotCardDefinition Card,
    TarotArtworkResolutionKind ArtworkKind,
    string? RasterAssetPath,
    string LocalizedTitle,
    string StructuralText,
    string? PrototypeFallbackText,
    double RotationDegrees)
{
    public bool HasProgrammaticFrame => true;

    public bool HasLocalizedTitleOverlay => true;

    public static TarotCardVisualPlan Create(
        TarotDrawnCard assignment,
        TarotArtworkResolution artwork,
        string localizedTitle,
        string structuralText,
        string? prototypeFallbackText) => new(
            assignment.Card,
            artwork.Kind,
            artwork.RasterAsset?.AssetPath,
            localizedTitle,
            structuralText,
            prototypeFallbackText,
            assignment.Orientation == TarotCardOrientation.Reversed ? 180d : 0d);
}
