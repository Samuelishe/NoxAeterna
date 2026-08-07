# Tarot Interpretation Content

| Metadata | Definition |
| --- | --- |
| Role | Canonical editorial and authoring owner for Tarot interpretation content. |
| Read when | Researching, drafting, translating, reviewing, or auditing Tarot prose and semantic tags. |
| Authoritative for | Classic voice; original-prose policy; five single-card sections; upright/reversed independence; tag and metric meaning; Russian-source translation; stateless Codex continuation; research provenance; authoring QA. |
| Not authoritative for | Source paths, inventories, SQLite DDL, runtime fallback, UI layout, or readiness implementation. |

## Classic editorial identity

Classic expresses traditional/common Tarot meaning in a living, literary, direct, useful, and sometimes predictive voice. It is not a newly invented proprietary divination system. It avoids canned mysticism, repetitive filler, sterile keyword lists, medical/legal certainty, and deterministic claims that remove the reader's agency.

Russian is the source language. English is a literary semantic translation: it preserves meaning, emotional direction, warnings, advice, tags, valence, intensity, and structure without needing literal syntax. Material meaning changes begin in Russian and are synchronized to translations.

## Single-card structure

Every upright and reversed state is independently authored and contains exactly five non-empty sections:

| ID | RU label | EN label | Purpose |
| --- | --- | --- | --- |
| `situation` | Основная ситуация | Core situation | What is present now. |
| `development` | Развитие | Development | How the situation tends to unfold. |
| `risk` | Риск | Risk | The main danger, blind spot, or cost. |
| `outcome` | Возможный исход | Possible outcome | The likely result if the current line continues. |
| `advice` | Совет | Advice | A direct, actionable response. |

There is no top-level interpretation heading. Reversed text is not produced by negating upright text. It may express one to three explicit mechanisms from the frozen set: `blocked`, `delayed`, `internalized`, `excessive`, `distorted`, `resisted`, `depleted`. Upright states declare none. These mechanisms are authoring metadata, not visible diagnostic prose.

## Tags and semantic presentation

Tags use stable language-neutral `conceptId` values. Every locale owns a vocabulary file per concept containing its visible label and editorial meaning. An entry assigns its own valence `-2..2` and intensity `1..3`; the concept itself does not own universal valence. Duplicate concepts in one state are invalid.

Single-card authoring normally aims for a useful candidate pool, while schema validity is based on uniqueness, known vocabulary, and ranges rather than an arbitrary minimum. Presentation selects three tags deterministically, styles them by authored valence/intensity, and never exposes `conceptId` as fallback text. Overall valence and intensity describe the state as a whole and remain separate from individual tags.

## Stateless Codex continuation

Every authoring Codex session is assumed to remember nothing from any previous session. Before creating content it must:

1. run `pwsh eng/repo-baseline.ps1`;
2. run `pwsh eng/context-plan.ps1` for the exact interpretation source target;
3. read the returned canonical Tarot interpretation owners;
4. inspect current `docs/PROJECT-STATE.md` and relevant recent `docs/SESSION-LOG.md` chronology;
5. inspect the actual authoring source tree;
6. run source validation/status tooling;
7. determine which canonical target bundles already exist and are valid;
8. preserve every valid accepted entry unless the prompt explicitly requests revision;
9. follow the canonical directory and filename contract without inventing taxonomy;
10. create only missing canonical targets in the requested wave;
11. validate and audit again after generation;
12. finish only when that wave has zero missing, duplicate, invalid, or noncanonical identities.

Conflict authority is:

```text
canonical owner documents
→ actual source tree + validator
→ PROJECT-STATE
→ SESSION-LOG chronology
→ textual report from an earlier Codex session
```

`SESSION-LOG` records what happened; canonical owners specify how work is done; source files plus the validator establish what actually exists. No authoring-plan JSON, chunk-plan JSON, or other machine-readable memory substitute is created.

## Research and generation

Codex is the primary authoring agent. A major authoring run may research traditional/common meanings across multiple compatible sources, synthesize original Nox Aeterna prose, write canonical JSON directly, run repository QA, and correct its own corpus.

It must never copy modern-source prose verbatim. Research/provenance for a major wave is recorded in Markdown/session chronology under the repository attribution rules; no second research-plan format is introduced. The application remains offline and deterministic: it never calls a language model for a reading.

The required authoring sequence for a large wave is:

```text
research
→ original synthesis
→ canonical bundle creation
→ structural validation
→ style and repetition audit
→ correction pass
→ second validation
→ owner spot-check
```

The owner reviews statistics, suspicions, and representative samples. Owner acceptance remains required for promotion, but acceptance is not forced into tiny batches and does not create per-entry runtime readiness.

## Quality audit

Automated and editorial QA should detect or surface:

- missing/empty sections or state members;
- duplicated or nearly duplicated passages across cards/orientations;
- reversed text that merely negates upright text;
- repeated openings, conclusions, advice formulas, or mechanical sentence templates;
- contradictory section logic, valence, intensity, tags, or reversal mechanisms;
- raw concept IDs without vocabulary labels;
- locale leakage and translation drift;
- copied or overly source-like wording;
- prose that is generic enough to fit nearly every card.

Draft/review status may be recorded in normal Markdown chronology when useful. It never controls runtime fallback. Exact bundle files, inventories, wave sizes, and physical paths belong to [TAROT-INTERPRETATION-MODES.md](TAROT-INTERPRETATION-MODES.md).
