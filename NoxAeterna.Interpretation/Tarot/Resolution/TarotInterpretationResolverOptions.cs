using NoxAeterna.Interpretation.Tarot.Contracts;

namespace NoxAeterna.Interpretation.Tarot.Resolution;

/// <summary>Defines positive bounded capacities for immutable validated resolver data.</summary>
public sealed record TarotInterpretationResolverOptions
{
    public TarotInterpretationResolverOptions(
        int manifestCapacity = 8,
        int indexCapacity = 32,
        int entryCapacity = 256)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(manifestCapacity);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(indexCapacity);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(entryCapacity);
        ManifestCapacity = manifestCapacity;
        IndexCapacity = indexCapacity;
        EntryCapacity = entryCapacity;
    }

    public int ManifestCapacity { get; }
    public int IndexCapacity { get; }
    public int EntryCapacity { get; }
}

/// <summary>Validated same-locale module snapshot returned only by the non-UI availability seam.</summary>
public sealed class TarotResolvedModuleSnapshot
{
    internal TarotResolvedModuleSnapshot(IEnumerable<TarotInterpretationCorpus> corpora) =>
        Corpora = Array.AsReadOnly(corpora.Distinct().Order().ToArray());

    public IReadOnlyList<TarotInterpretationCorpus> Corpora { get; }
}
