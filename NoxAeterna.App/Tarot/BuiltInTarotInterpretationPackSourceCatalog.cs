using System.Collections.ObjectModel;
using NoxAeterna.Domain.Tarot;
using NoxAeterna.Interpretation.Tarot.Sources;

namespace NoxAeterna.App.Tarot;

/// <summary>Immutable App-owned catalog of packaged built-in interpretation sources.</summary>
public sealed class BuiltInTarotInterpretationPackSourceCatalog : ITarotInterpretationPackSourceCatalog
{
    private readonly IReadOnlyDictionary<TarotInterpretationPackId, ITarotInterpretationPackSource> sources;

    public BuiltInTarotInterpretationPackSourceCatalog(IEnumerable<ITarotInterpretationPackSource> sources)
    {
        ArgumentNullException.ThrowIfNull(sources);
        var materialized = new Dictionary<TarotInterpretationPackId, ITarotInterpretationPackSource>();
        foreach (var source in sources)
        {
            ArgumentNullException.ThrowIfNull(source);
            if (!materialized.TryAdd(source.PackId, source))
            {
                throw new ArgumentException(
                    $"Duplicate interpretation pack source '{source.PackId.Value}'.",
                    nameof(sources));
            }
        }

        this.sources = new ReadOnlyDictionary<TarotInterpretationPackId, ITarotInterpretationPackSource>(materialized);
        PackIds = Array.AsReadOnly(materialized.Keys.OrderBy(item => item.Value, StringComparer.Ordinal).ToArray());
    }

    public IReadOnlyList<TarotInterpretationPackId> PackIds { get; }

    public static BuiltInTarotInterpretationPackSourceCatalog CreateDefault() =>
        new([new BuiltInClassicInterpretationPackSource()]);

    public bool TryGetSource(
        TarotInterpretationPackId packId,
        out ITarotInterpretationPackSource? source)
    {
        ArgumentNullException.ThrowIfNull(packId);
        return sources.TryGetValue(packId, out source);
    }
}
