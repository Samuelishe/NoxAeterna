namespace NoxAeterna.Domain.Tarot;

/// <summary>Identifies an artwork pack independently from a semantic Tarot deck.</summary>
public sealed record TarotArtworkPackId
{
    /// <summary>Initializes a validated stable artwork-pack identifier.</summary>
    public TarotArtworkPackId(string value) => Value = StableTarotId.Normalize(value, nameof(value));

    /// <summary>Gets the stable identifier value.</summary>
    public string Value { get; }

    /// <inheritdoc />
    public override string ToString() => Value;
}

/// <summary>Identifies a presentation skin independently from card artwork.</summary>
public sealed record TarotPresentationSkinId
{
    /// <summary>Initializes a validated stable presentation-skin identifier.</summary>
    public TarotPresentationSkinId(string value) => Value = StableTarotId.Normalize(value, nameof(value));

    /// <summary>Gets the stable identifier value.</summary>
    public string Value { get; }

    /// <inheritdoc />
    public override string ToString() => Value;
}

/// <summary>Identifies a selectable card-back variant independently from the deck and artwork.</summary>
public sealed record TarotBackVariantId
{
    /// <summary>Initializes a validated stable back-variant identifier.</summary>
    public TarotBackVariantId(string value) => Value = StableTarotId.Normalize(value, nameof(value));

    /// <summary>Gets the stable identifier value.</summary>
    public string Value { get; }

    /// <inheritdoc />
    public override string ToString() => Value;
}

/// <summary>Identifies an interpretation set independently from semantic and visual packs.</summary>
public sealed record TarotInterpretationSetId
{
    /// <summary>Initializes a validated stable interpretation-set identifier.</summary>
    public TarotInterpretationSetId(string value) => Value = StableTarotId.Normalize(value, nameof(value));

    /// <summary>Gets the stable identifier value.</summary>
    public string Value { get; }

    /// <inheritdoc />
    public override string ToString() => Value;
}
