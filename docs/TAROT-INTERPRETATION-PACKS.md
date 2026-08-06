# Tarot Interpretation Packs

| Metadata | Definition |
| --- | --- |
| Role | Canonical architecture for selectable Tarot interpretation data packages. |
| Read when | Designing interpretation-pack discovery, selection, localization, readiness, fallback, or missing-content behavior. |
| Authoritative for | Interpretation-pack identity and boundaries; package capabilities; locale/module readiness; locale fallback; content-absence behavior; pack discovery and selection direction; active-pack preference; package relationships with semantic decks and spread modes; runtime resolution semantics; future manifest direction; and partial-package behavior. |
| Not authoritative for | Exact interpretation text; authorial style guide; single-card sections, tags, metrics, or upright/reversed content; pair content; three-card synthesis; Avalonia layout; artwork; card backs; general persistence; or the authoring workflow. |

## Identity and Independent Selections

A Tarot interpretation pack is an independent, pluggable data package in one shared schema family. It contains one authored interpretation system for the locales and reading modes that it supports.

An interpretation pack is not an artwork pack, semantic card illustration set, presentation skin, card back, dark/light theme, application UI layout, Tarot visual pack, user profile, or current reading. The user selects the following dimensions independently:

- artwork pack;
- interpretation pack;
- card back;
- spread;
- application theme;
- interpretation language.

For example, this is one valid combination:

```text
Artwork: Lupus Noctis
Interpretation pack: Classic
Language: Russian
Back: Black Sun
Theme: Obsidian
```

No selection in that list implicitly changes another selection.

## Data-Package Contract

All interpretation packs use one schema family. A pack contains data, not arbitrary executable code, and conceptually consists of a manifest plus content modules. Packs may be installed progressively, and the selector may expose several packs at once.

The stable pack ID is language-neutral. Its display name is localized. The first pack has stable ID `classic`, Russian display name `Классика`, and English display name `Classic`; `classic` is the future default interpretation pack.

A pack binds meanings to stable semantic card identities and declares capabilities for reading modes. It does not create a semantic deck or a spread. A semantic reading therefore survives pack changes, and a newly implemented spread does not wait for pack content. The exact supported-deck declaration, manifest JSON, storage paths, and content-entry schema remain later decisions.

Partial packages are valid and remain selectable. A package may support only some locale/mode modules; incompleteness is represented at module readiness, not by hiding the package or language.

Conceptual package structure:

```text
classic
├── manifest
├── ru
│   ├── single-card
│   ├── two-card
│   ├── three-card
│   └── future modes
├── en
│   ├── single-card
│   ├── two-card
│   ├── three-card
│   └── future modes
└── future locale
    └── ...
```

This tree is conceptual and does not approve an exact filesystem layout or manifest shape.

## Locale Growth and Source Language

Adding an application UI locale immediately makes it a normal UI language and keeps interpretation-language selection available. Every installed interpretation pack receives a readiness-matrix entry for the new locale, while its content modules may remain unfinished. Neither the locale nor a partial pack is hidden, and publishing the UI locale does not require every pack to be fully translated.

Russian is the primary authoring locale. Other languages are semantic literary translations: they need not be literal, but they preserve meaning, emotional tone, valence, intensity, semantic tags/concepts, and content-entry structure. A material meaning change is made in the Russian source first and then synchronized into translations. The detailed authorial, single-card, tag, and translation contract belongs to [`TAROT-INTERPRETATION-CONTENT.md`](TAROT-INTERPRETATION-CONTENT.md).

## Coarse Module Readiness

Readiness belongs to exactly one coarse unit:

```text
interpretation pack + locale + reading mode
```

Examples include `classic / ru / single-card`, `classic / ru / two-card`, `classic / en / three-card`, `psychological / zh / two-card`, and `mystical / en / celtic-cross`.

Each unit has one manual owner-controlled declaration:

```text
module(packId, locale, mode).ready = true | false
```

There are no per-card, per-pair, per-section, or per-entry readiness flags.

- `ready = false` declares intentional incompleteness. Runtime continues through the locale fallback chain.
- `ready = true` declares the whole locale/mode module published. Runtime resolves content only from that locale module.

A human or future authoring workflow sets readiness explicitly. Runtime and validators neither infer readiness nor change the flag. A future validator may check consistency and emit technical diagnostics, but those diagnostics do not replace the owner-controlled declaration.

## Locale Resolution

For each individual reading-mode module, runtime builds this ordered chain and removes repeated locales:

```text
requested locale
-> English, when its module is ready
-> Russian, when its module is ready
-> no displayable content
```

Examples:

```text
requested Russian: ru -> en -> no content
requested English: en -> ru -> no content
requested Chinese: zh -> en -> ru -> no content
```

Each `ready = false` candidate is skipped and resolution continues. The first ready candidate resolves the entire result for that mode from one locale. Runtime never assembles a three-card result from, for example, Chinese position one, English position two, and Russian synthesis.

### Intentional Incompleteness Versus Damage

`ready = false` is intentional incompleteness and permits fallback.

`ready = true` with unreadable or incomplete content is package damage. Missing expected files, folders, required sections or keys, unreadable JSON, or a route to a missing entry all produce no displayable interpretation content from that resolution attempt. Runtime must not fall back to another locale and thereby mask the broken published module. The application remains operational, while internal diagnostics and tests may identify the damage.

The future API may represent absence with a typed result, `null`, or an empty display model. This architecture does not require a raw empty-string sentinel.

## Silent Presentation Policy

When no displayable interpretation content exists, the interpretation host is absent or empty. It has no empty bordered surface, heading without content, placeholder, unavailable message, or diagnostic banner.

The user is never shown fallback or implementation explanations such as “translation missing,” “English was used,” “pack is partial,” “interpretation unavailable,” “mode unsupported,” or “Russian fallback was used.” Requested/resolved locales, readiness flags, missing-file details, and other diagnostics are not user-facing copy.

Resolved locale and technical diagnostics may remain available to automated tests, debugging, saved-reading provenance, and technical reports. This silent policy applies only to absence and fallback of interpretation content; it does not redefine safety-critical or actionable failures in other subsystems.

The current post-T-UX1A UI still shows the localized `ui.tarot.interpretation.unavailable` placeholder after a reveal because no interpretation runtime exists. That is an honest current implementation baseline, not the target contract. The first real interpretation implementation stage must remove that placeholder from production UI and leave the host empty when resolution yields no displayable content.

## Reading-Mode Independence

A reading mode becomes available as soon as its Domain, Presentation, and UI behavior is implemented. It does not wait for interpretation modules or pack completeness and is never hidden from the spread selector for lack of meanings. This applies to single-card, two-card, three-card, future Celtic Cross and relationship spreads, and any later mode. With no displayable interpretation content, the cards still work and the interpretation host remains empty.

## Selection and Immediate Re-Resolution

Changing visual and meaning selections preserves the current semantic reading:

- switching artwork pack refreshes illustrations immediately and changes no interpretation content;
- switching interpretation pack preserves artwork, drawn cards, revealed state, and hidden-card visibility rules, then immediately re-resolves visible interpretation text and tags without a new Draw;
- switching interpretation language preserves the cards and revealed state, then immediately re-resolves visible content through the silent locale fallback chain.

Any selection change that affects visible UI refreshes immediately rather than waiting for another click, Draw, navigation cycle, or control recreation.

## Discovery and Preference Direction

Future discovery reads manifests and exposes valid installed packages, including partial packages. No selector or manifest is implemented by INT0-D1.

The future `selectedInterpretationPackId` preference lives in the versioned AppData `settings.json`, defaults to `classic`, restores at startup, and updates when the user selects a pack. Settings do not persist current interpretation text or fallback locale, because the latter is a runtime resolution result rather than a user preference. General settings storage and the shared Reset settings/Open AppData actions belong to [PERSISTENCE.md](PERSISTENCE.md).

## Deferred Content Decisions

INT0-D1 deliberately does not own single-card content, tags, voice, pair identity or counts, oriented-pair strategy, three-card synthesis, exact manifest JSON, or exact storage paths. INT0-D2 content decisions now belong to [`TAROT-INTERPRETATION-CONTENT.md`](TAROT-INTERPRETATION-CONTENT.md); pair and multi-card routing proceed through INT0-D3 and the final INT0-D4 reconciliation in [ROADMAP.md](ROADMAP.md). General structured-first rules remain owned by [INTERPRETATION-ENGINE.md](INTERPRETATION-ENGINE.md); spread semantics remain owned by [TAROT-ENGINE.md](TAROT-ENGINE.md).
