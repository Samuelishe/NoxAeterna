# Symbolics Layer

The Symbolics layer stores structured symbolic knowledge. It is not the same as Interpretation.

Symbolics answers: "What symbolic meanings, correspondences, and relationships are known?"

Interpretation answers: "How do selected symbolic factors combine into user-facing meaning in this context?"

## Core Decision

The Symbolics layer is not a graph database system and not a flat prose database.

It is a structured symbolic catalog with typed relationships.

The MVP should stay explainable and lightweight. Do not introduce graph-database complexity unless a later, concrete need proves it necessary.

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

Likely future structures:

- `Symbol`
- `SymbolCategory`
- `SymbolSystem`
- `SymbolMeaning`
- `SymbolRelationship`
- `RelationshipType`
- `SourceMetadata`

Supporting fields may include:

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

## Typed Relationships

Relationships should be typed and explainable.

Examples:

- Mars rules Aries.
- Mars corresponds to Tower.
- Mars corresponds to iron.
- Mars corresponds to Tuesday.

These relationships should be stored as structured data rather than buried in prose paragraphs.

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

Symbolics may provide keywords, themes, meaning fragments, and relationship metadata. It must not become a prose-generation layer.
