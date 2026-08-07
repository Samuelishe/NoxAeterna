using NoxAeterna.Interpretation.Tarot.Contracts;

namespace NoxAeterna.Interpretation.Tarot.Resolution;

/// <summary>Defines the positive bound for optional immutable semantic entry caching.</summary>
public sealed record TarotInterpretationResolverOptions
{
    public TarotInterpretationResolverOptions(int entryCapacity = 256)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(entryCapacity);
        EntryCapacity = entryCapacity;
    }
    public int EntryCapacity { get; }
}

/// <summary>Validated same-locale module snapshot returned only by the non-UI availability seam.</summary>
public sealed class TarotResolvedModuleSnapshot
{
    internal TarotResolvedModuleSnapshot(IEnumerable<TarotInterpretationCorpus> corpora) =>
        Corpora = Array.AsReadOnly(corpora.Distinct().Order().ToArray());
    public IReadOnlyList<TarotInterpretationCorpus> Corpora { get; }
}
