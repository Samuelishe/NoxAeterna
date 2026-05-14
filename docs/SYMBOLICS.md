# Symbolics Layer

The Symbolics layer stores structured symbolic knowledge. It is not the same as Interpretation.

Symbolics answers: "What symbolic meanings, correspondences, and relationships are known?"

Interpretation answers: "How do selected symbolic factors combine into user-facing meaning in this context?"

## Responsibilities

The Symbolics layer should eventually contain:

- Planetary archetypes.
- Zodiac archetypes.
- House meanings.
- Aspect meanings.
- Elements.
- Modalities.
- Rulerships.
- Tarot correspondences.
- Lunar correspondences.
- Mythological and historical references.
- Symbolic relationships between systems.

## Data Shape

Symbolic knowledge should be structured and typed. Avoid storing the core knowledge as only prose paragraphs.

Potential future structures:

- Symbol identifier.
- System, such as Astrology or Tarot.
- Category, such as Planet, Sign, House, Aspect, Card, Element, Modality.
- Keywords.
- Archetypal themes.
- Constructive expressions.
- Shadow expressions.
- Related symbols.
- Historical notes.
- Source metadata.

## Source Curation

Historical symbolic sources need careful curation. Do not add unsourced claims as authoritative.

When adding symbolic content, distinguish:

- Traditional correspondence.
- Modern interpretive convention.
- Project-specific editorial framing.
- Experimental or fictional extension.

## Astrology and Tarot Links

The layer should support future correspondences between astrology and Tarot, such as planets, signs, elements, modalities, lunar phases, and card archetypes.

These links should be data-driven and explainable, not hidden in UI text.

## Relationship to Interpretation

Interpretation consumes symbolic data and combines it with context. Symbolics should not directly decide how a full reading is phrased for a specific user.
