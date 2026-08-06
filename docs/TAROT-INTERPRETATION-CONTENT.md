# Tarot Interpretation Content

| Metadata | Definition |
| --- | --- |
| Role | Canonical authorial and single-card content architecture for Tarot interpretation packs. |
| Read when | Authoring, translating, reviewing, validating, or presenting Tarot interpretation content and semantic tags. |
| Authoritative for | Interpretation-pack authorial identity; Classic style guide; single-card content structure; upright/reversed requirements; semantic content sections and visible headings; tag concepts and labels; valence/intensity scales and overall metrics; deterministic tag presentation; Russian-source authoring; translation fidelity; authoring-time Codex use; draft/review/accepted direction; content-quality rules; and future interpretation typography/font direction. |
| Not authoritative for | Pack discovery, readiness, fallback, missing-content behavior, selector behavior, exact serialization, production paths/indexes, pairs, three-card synthesis, mode routing, Avalonia implementation, actual palette or font choice, artwork, or card backs. |

## Authorial Identity and Classic

Every interpretation pack owns an authorial identity: its meaning school, voice, tag vocabulary, localized visible labels, and editorial quality bar. Package identity, readiness, fallback, and selection remain owned by [TAROT-INTERPRETATION-PACKS.md](TAROT-INTERPRETATION-PACKS.md).

The first pack is:

```text
Stable ID: classic
RU display name: Классика
EN display name: Classic
```

Classic uses widely recognized traditional Tarot meanings and common interpretations of cards and combinations. It is not a separate psychological, meme, or author-mystical system and never depends on Lupus Noctis or another illustration. “Classic” describes the semantic foundation, not a dry or neutral voice.

Classic prose is original project editorial work. It must not copy modern websites verbatim, retain long borrowed formulations, or present another author's text as project-owned content. Future source curation may consult several compatible traditional and modern reference sources, but bibliography and provenance remain separately tracked work.

## Classic Voice

Classic speaks like a living interpreter rather than a reference manual. Its voice combines:

- emotional and expressive language;
- a living rhythm and prophetic confidence;
- literary clarity and concrete imagery;
- appropriate drama, tenderness, hope, darkness, or firmness;
- broad applicability to real situations without flattening the character of the card.

Prefer a direct interpretation such as “Вероятен конфликт, в котором уступка может обойтись дороже открытого сопротивления” over “Карта указывает на возможный конфликт.” Prefer “Ситуация требует выбора, хотя оба пути пока выглядят одинаково сомнительно” over a mechanical “Вам предстоит сделать выбор.” These are style examples, not production card entries.

The governing principle is **interpretive openness**: leave enough room for a reader to recognize their situation without retreating into lifeless universal wording. Breadth must preserve beauty, a concrete image, the card's character, emotional force, and predictive effect. A narrower formulation is allowed when it is materially stronger and more faithful to traditional meaning.

Second person is optional, not required or forbidden. Impersonal or situation-centered forms often preserve openness: “Ситуация требует выбора”, “Вероятен конфликт”, “Старый порядок подходит к концу.” Direct address is valid when it is the strongest natural form, for example “Не пытайся удержать то, что уже завершилось.”

### Forbidden Classic Voice

Classic does not use profanity, slang, internet memes, insults, humiliation, sarcasm, coarse mockery, bureaucratic prose, encyclopedic detachment, or recurring technical caveats. Avoid stock phrases such as:

- `карта указывает`;
- `данный аркан символизирует`;
- `возможно, эта карта может означать`;
- `следует учитывать`;
- `согласно традиционной трактовке`.

Literary sharpness, tension, a light barb without sarcasm or humiliation, direct warning, and a dark or severe tone remain valid when the card calls for them. This voice contract belongs to Classic only; a future meme, psychological, or mystical pack may deliberately use another tone.

## Single-Card Reading Contract

A single-card interpretation is a developed standalone reading. It is neither a five-word tag, a single short sentence, a dry keyword list, nor an enormous story. It should support thoughtful reading through the existing vertical scroll. Sentence and word counts are style targets, not hard schema validators; Codex retains creative freedom inside this content contract.

The semantic shape is:

```text
single-card entry
├── semantic card ID
├── orientation
├── sections
│   ├── situation
│   ├── development
│   ├── risk
│   ├── outcome
│   └── advice
├── candidate tag assignments
├── overall valence
└── overall intensity
```

This document owns the meaning and quality of those fields. Exact JSON names, paths, and serialization belong to [`TAROT-INTERPRETATION-IMPLEMENTATION.md`](TAROT-INTERPRETATION-IMPLEMENTATION.md) and the mode owner.

### Stable Sections and Visible Headings

Every complete Classic single-card interpretation contains five non-empty sections with stable language-neutral IDs:

| Section ID | Meaning | RU default label | EN default label |
| --- | --- | --- | --- |
| `situation` | Main situation and central dynamic | Основная ситуация | Core situation |
| `development` | Probable development | Развитие | Development |
| `risk` | Danger, distortion, weakness, or price | Риск | Risk |
| `outcome` | Possible outcome if the current dynamic continues | Возможный исход | Possible outcome |
| `advice` | Practical direction | Совет | Advice |

All five sections are required, but emphasis and length may differ. One card may concentrate on risk, another on development, and another on advice. No required section may be empty.

Headings are useful content navigation and are visible to the user. Section IDs remain stable across packs, locales, orientations, and storage versions; each pack may localize or stylistically adapt its visible labels. A future meme pack may therefore use different display labels while retaining the same semantic IDs.

Presentation makes headings noticeable and easy to scan through semantic typography. Content never embeds a font family, font size, RGB/hex color, Avalonia class, or Markdown emphasis as a mandatory rendering contract.

### Independent Upright and Reversed Content

Each of the 78 cards receives two complete standalone single-card interpretations:

```text
78 cards × 2 orientations = 156 complete entries
```

Every upright and reversed entry has all five sections, its own candidate tag pool, its own overall valence, and its own overall intensity. Reversed content is not automatic negation, word rearrangement, mechanical insertion of `не`, or an automatically weakened upright entry. It is an independently authored interpretation of the traditional reversed meaning.

The optional internal reversed mechanisms `blocked`, `delayed`, `internalized`, `excessive`, `distorted`, `resisted`, and `depleted` are approved and owned by [`TAROT-INTERPRETATION-MODES.md`](TAROT-INTERPRETATION-MODES.md). They support consistency and synthesis without replacing independently authored reversed prose.

## Semantic Tags

A tag concept has a stable language-neutral semantic identity. Examples include `conflict`, `choice`, `renewal`, `failure`, `release`, `uncertainty`, and `opportunity`.

Visible tag labels belong to the interpretation pack and locale. For the concept `failure`, Classic may show `Неудача` in Russian and `Failure` in English. A future meme pack could render the same concept as `Это фиаско, братан` while retaining the internal meaning `failure`. D4 freezes `conceptId` as the serialized property name in the implementation owner.

There is no mandatory closed global vocabulary for every pack:

- packs may reuse a concept ID when they truly share the same meaning;
- a pack may define a package-specific concept ID for a unique meaning;
- equal visible labels do not prove equal concepts;
- different visible labels may represent the same concept;
- packs need not have equal vocabulary size;
- cross-pack comparison is not a primary product goal.

Each pack owns its tag vocabulary and localized display labels. A tag concept is a semantic meaning identity, not UI prose or a color.

### Pools and Deterministic Presentation

Each upright/reversed single-card entry has a candidate pool of roughly `5–10` semantic tag assignments. Tags must reflect the entry, differ meaningfully rather than pad the pool with synonyms, and match the orientation.

Single-card UI normally shows a compact subset of `2–4` tags rather than the full pool. These ranges are presentation defaults, not content-completeness validators; pair and three-card tag contracts belong to [`TAROT-INTERPRETATION-MODES.md`](TAROT-INTERPRETATION-MODES.md).

The subset may vary between different readings, but it is stable for one reading, pack, and content version. It does not change on resize, a UI redraw, tab switch, navigation away/back, language or theme switch, repeated layout, card click, or scroll. A language change preserves selected concept IDs and changes only localized labels. A pack change may select from the new pack's vocabulary while preserving cards and the semantic reading. The seed algorithm remains an implementation decision; non-flickering stability is the contract.

## Valence, Intensity, and Overall Vibe

Each tag assignment conceptually carries:

```text
concept ID
valence: -2..+2
intensity: 1..3
```

Valence uses this five-level scale:

- `-2` — strongly negative;
- `-1` — mostly negative;
- `0` — mixed or neutral;
- `+1` — mostly positive;
- `+2` — strongly positive.

Valence belongs to a concept's use in a specific interpretation, not permanently to the dictionary concept. `transformation` may be `-1` in one entry and `+1` in another.

Intensity uses this three-level scale:

- `1` — weak or background;
- `2` — noticeable;
- `3` — strong or dominant.

Intensity belongs to a tag assignment or to the interpretation as a whole and can distinguish mild concern, serious tension, and a crisis turning point.

Every single-card interpretation also receives separately authored `overall valence` (`-2..+2`) and `overall intensity` (`1..3`). They express the overall vibe and are not a simple average of tag metrics. Opposing tags do not necessarily yield neutrality, and one dominant theme may control the reading.

Content stores no red/orange/yellow/green names, hex colors, brushes, emoji, PNG paths, fonts, or Avalonia styles. A later UI may express these scales through semantic colors, glyphs, icons, shapes, or another accessible representation.

## Russian Source and Literary Translation

Russian is the primary source locale. Authoring proceeds in this order:

1. Create and accept Russian source content.
2. Produce semantic literary translations.
3. Apply every material meaning change to the Russian source first.
4. Synchronize translations with the revised source.

Russian prose need not imitate English Tarot terminology or syntax; it must sound natural to a Russian reader.

A translation may change syntax, sentence length, and literary imagery, and need not be literal. It must preserve semantic card ID, orientation, all five section IDs, each section's central meaning, emotional tone, predictive force, concept IDs and tag assignments, tag valence/intensity, and overall valence/intensity. Visible section and tag labels are localized naturally.

A translation must not soften a dark source, turn a warning into advice, reduce conflict to neutral difficulty, or make a positive interpretation anxious. Authoring metadata may track source/translation revisions, stale translation, reviewer, and batch status. That metadata is not per-entry runtime readiness and never creates per-entry fallback; coarse readiness remains `pack + locale + mode` under the pack owner.

## Codex, Lifecycle, and Quality

Codex is the primary creative authoring tool and may draft Russian content and translations, propose alternatives, edit style, assign candidate tags and metrics, audit consistency, reduce repetition, and work in reviewable batches.

The production application remains offline, deterministic, and corpus-backed. It never calls an LLM per reading, requires a cloud model, invents meaning after Draw, or replaces absent content with dynamic prose. The general rule is that an LLM is not the runtime interpretation engine; it does not prohibit authoring-time Codex from creating curated repository content.

Conceptual lifecycle:

```text
draft -> reviewed -> accepted production
```

Codex may create drafts; the owner remains the acceptance authority for reviewable batches. Acceptance does not require one runtime flag per line. Production content is separated from incomplete drafts, mechanically checkable, versioned in Git, and never appears in the app merely because it exists in a draft location. Production/working roots, bounded-file direction, inventories, and batching belong to [`TAROT-INTERPRETATION-MODES.md`](TAROT-INTERPRETATION-MODES.md).

Future validation and authoring tools should detect at least:

- missing or empty mandatory sections;
- unknown card IDs, orientations, or concepts;
- unexplained duplicate concepts in one pool;
- tag or overall valence outside `-2..+2`;
- tag or overall intensity outside `1..3`;
- missing overall metrics or locale labels;
- source/translation structural mismatch;
- forbidden Classic voice patterns or accidental raw HTML/UI markup;
- copied or duplicated boilerplate and excessive cross-card repetition;
- suspiciously identical upright/reversed content;
- a tag pool conspicuously below the `5–10` authoring target, reported as a quality suspicion rather than a completeness/readiness failure.

Classic style checks should flag `карта указывает`, `данный аркан`, `возможно, эта карта может`, bureaucratic templates, profanity, slang, memes, insults, sarcasm, empty universality, identical openings across the 156 entries, mechanical reversed negation, five-word fragments, and unjustifiably long essays. Automation finds suspicions; regex cannot judge literary quality completely. No validator is implemented by INT0-D2.

## Future Typography and Fonts

Interpretation UI implementation includes a separate typography stage for section headings, body text, and tags, which may use different semantic typography roles. Candidate fonts must support Russian and English, anticipate future scripts, permit redistribution with the application, and have safe glyph fallback behavior.

Selected font files become repository-owned shipped assets. Their provenance and redistribution license must be recorded in `README.md`, [THIRD-PARTY.md](THIRD-PARTY.md), and asset documentation. INT0-D2 selects and downloads no font, changes no palette, and adds no hypothetical third-party entry.

## Relationship to Mode and Implementation Architecture

[`TAROT-INTERPRETATION-MODES.md`](TAROT-INTERPRETATION-MODES.md) owns production path patterns, unordered canonical pairs, all 12,012 independently authored orientation states, pair/multi-card tag counts, three-card position/relation/synthesis architecture, Celtic Cross composition direction, indexes, and lazy routing. [`TAROT-INTERPRETATION-IMPLEMENTATION.md`](TAROT-INTERPRETATION-IMPLEMENTATION.md) owns exact JSON fields, versioning, layers, migration, and staged handoff. This document continues to own authorial quality and the complete single-card content contract.
