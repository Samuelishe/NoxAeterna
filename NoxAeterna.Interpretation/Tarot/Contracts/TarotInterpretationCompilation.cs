namespace NoxAeterna.Interpretation.Tarot.Contracts;

/// <summary>Validated normalized immutable input to a runtime-package writer.</summary>
public sealed record TarotInterpretationCompilation(
    TarotInterpretationPackManifest Manifest,
    TarotSha256 SourceDigest,
    IReadOnlyDictionary<TarotInterpretationLocale, TarotLabels> Labels,
    IReadOnlyDictionary<TarotInterpretationLocale, IReadOnlyList<TarotVocabularyEntry>> Vocabulary,
    IReadOnlyDictionary<TarotInterpretationLocale, IReadOnlyList<TarotSingleCardEntry>> SingleCards,
    IReadOnlyDictionary<TarotInterpretationLocale, IReadOnlyList<TarotOrientedPairEntry>> OrientedPairs,
    IReadOnlyDictionary<TarotInterpretationLocale, IReadOnlyList<TarotThreeCardPositionEntry>> ThreeCardPositions,
    IReadOnlyDictionary<TarotInterpretationLocale, IReadOnlyList<TarotSynthesisResource>> SynthesisResources);

