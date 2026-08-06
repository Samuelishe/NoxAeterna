# UI Vision

Nox Aeterna should feel like a serious desktop instrument and personal archive, not a mobile entertainment app.

This document owns product mood, UX rhythm, and high-level visual direction. Exact semantic colors, paired dark/light tones, chart hierarchy, component-state colors, contrast rules, and allowed effects belong to [`VISUAL-DESIGN-SYSTEM.md`](VISUAL-DESIGN-SYSTEM.md).

## Visual Identity

References:

- European occultism.
- Victorian occult revival.
- Old astronomical atlases.
- Dark academia.
- Gothic archive aesthetics.
- Occult manuscripts.
- Astrolabes.
- Ephemerides.
- Brass.
- Parchment.
- Matte black.
- Night sky.

The mood may learn from Cultist Simulator and Bloodborne-like atmosphere, but must not copy them or become game-like.

## UX Rhythm

The application should feel calm and deliberate.

Prefer:

- Dense but readable archive layouts.
- Structured panels.
- Clear navigation.
- Stable charts and tables.
- Saved history.
- Rich inspection views.
- Quiet transitions.

Avoid:

- Decorative clutter.
- Oversized marketing-style hero sections inside the app.
- Meme-like text.
- Neon purple.
- Cheap glow effects.
- Cartoon magic.
- Fantasy overdecoration.
- "AI mysticism" language.

## Typography Direction

Typography should feel archival and professional. Use display typography sparingly and keep data-heavy screens highly readable.

Text must not occlude charts, controls, or archive content.
All user-facing text should come from localization keys rather than hardcoded UI literals.

## Layout Principles

The UI should support repeated study and comparison:

- Profiles should be central and persistent.
- Charts should be inspectable.
- Transit and history views should support time navigation.
- Tarot sessions should be saved and revisitable.
- Interpretation should be structured, not dumped as one long paragraph.

Current shell direction:

- a thin presentation-led shell can exist before real product screens;
- top-level sections should be modeled explicitly rather than improvised in `App`;
- the first astrology workspace foundation can replace a raw debug-only host before full product screens exist;
- development-only sample data remains available to tests and explicit debug fixtures but must not drive normal visible startup.
- the astrology workspace may now host an initial birth-data input panel before real chart calculation is connected.
- validated birth-data input may now rebuild the visible chart through the real SwissEphNet-backed calculation path; Moshier fallback remains documented in project diagnostics and documentation instead of occupying the primary workspace.
- the chart is the dominant object in the left column and uses a viewport-fitted square surface constrained by both content width and the finite chart-column viewport height, with a deliberately high 1100 DIP ceiling.
- the entire wheel must be visible at the top of the left scroll viewport; scrolling moves from the complete chart to its tables rather than revealing one chart in pieces.
- each workspace column owns one vertical `ScrollViewer`; an explicit right gutter keeps overlay scrollbars away from chart graphics, table columns, and panel content.
- the current right-side birth-data panel remains usable at the default window size through column-level scrolling rather than requiring a maximized window.
- when native date or time picker controls are used, give them enough horizontal space to remain readable; do not compress them into tight multi-column groups that break segment layout.
- the main astrology workspace should be concise and tool-like; onboarding copy and persistent technical notices do not belong between the operator and the chart.
- the current readable chart foundation should stay legible before it becomes ornate; chart readability takes priority over decorative treatment.
- a planet's source dot/notch is the authoritative coordinate; its glyph and degree/retrograde label are annotations whose leaders must make the relationship legible without implying that aspects originate at displaced glyph centers.
- planet glyph movement should prefer radial stacking and remain inside the source sign and reliable source house; labels may move independently when measured text collides.

Current Tarot direction:

- the Tarot section is a real in-memory workspace using Domain-owned standard cards, built-in spreads, orientation policy, and draw engine;
- single-card presentation centers a `378 × 648` DIP 7:12 card, while three-card presentation keeps ordered past/present/future assignments at equal preferred `315 × 540` DIP sizes;
- compact widths preserve a `216` DIP card minimum and give horizontal overflow to the tableau instead of clipping the shell;
- Lupus Noctis is the sole visible artwork option and resolves all 78 standard cards to raster; required-pack damage disables Draw with a controlled localized diagnostic rather than exposing Classic;
- raster inner artwork never owns the programmatic frame, localized title, selection state, or reversal transform; Black Sun and Lunar Seal remain selectable prototype backs;
- a fixed control panel sits above one stretching vertically scrollable reading surface; the tableau owns only horizontal overflow and a full-width interpretation host follows it without visible tableau/interpretation headings;
- the selected-card metadata inspector is absent from production UI; selection remains available for card state, keyboard focus, accessibility, and future navigation;
- auto reveal defaults on; manual mode reveals one activated position at a time, and hidden cards never leak future meaning content;
- language, theme, spread, artwork, back, reversal, and auto-reveal preferences persist in AppData JSON, while current reading, revealed positions, and selection remain session-only.
- artwork, interpretation pack, back, spread, application theme, and interpretation language remain independent selections; changing pack or interpretation language will re-resolve currently visible meaning immediately without redrawing cards or resetting reveals;
- the future `TarotInterpretationPackSelector` stays visible in the control panel even with only Classic; its `Толкование / Interpretation` control label and pack display name follow UI language, while prose follows interpretation language. Exact selector and settings gates belong to [`TAROT-INTERPRETATION-IMPLEMENTATION.md`](TAROT-INTERPRETATION-IMPLEMENTATION.md).
- after real interpretation runtime begins, absent content is silent: the host is empty with no heading, empty surface, placeholder, fallback explanation, or diagnostic banner. The current unavailable placeholder is removed in INT0-I4; canonical resolution behavior belongs to [`TAROT-INTERPRETATION-PACKS.md`](TAROT-INTERPRETATION-PACKS.md).
- available interpretation content uses visible localized section headings for scanability, compact tags, and semantic typography rather than a headingless wall of text; headings are content structure, not implementation or fallback explanations. Content carries no hardcoded fonts, colors, or Avalonia styles; its contract belongs to [`TAROT-INTERPRETATION-CONTENT.md`](TAROT-INTERPRETATION-CONTENT.md).
- the content host has no top-level visible `Интерпретация` or `Толкование` heading; that text labels the selector, while single-card section and three-card position/relation headings remain useful navigation.

Current birth-input direction:

- date selection should use a real date picker instead of raw typed date strings;
- time selection should use a constrained control and degrade honestly when the user marks time as unknown;
- timezone selection should come from local TZDB data, not a raw free-text field;
- manual coordinates remain the offline-first fallback and are required for honest calculation later.
- validation errors should appear after an attempted build, while obvious date, time, location, and TZDB instructions should not remain as permanent paragraphs.
- the chart area now combines the accepted responsive zodiac/planet presentation with a restrained house structure when a trustworthy time is available.
- positions summaries are one shared four-column grid with localized `Planet`, `Sign`, `Position`, and `Retrograde` headers; position values keep minute precision and align right, while retrograde state has its own centered column.
- a compact localized `Chart angles` block below the positions table shows Ascendant and Midheaven sign plus degree/minute values; unknown time shows only a short unavailable status.
- the empty `Context and interpretation` card is not materialized until real content exists, although its future workspace metadata may remain.
- known and approximate times calculate houses; unknown time disables the time control and never presents noon-fallback houses.
- unknown time also clears the disabled time editor visually while preserving the last entered session value in memory; returning to exact or approximate time restores that value, while the short `Ввод времени отключён.` status remains the only explanatory copy.
- the UnknownTime wheel is intentionally distinct: it has no houses or principal angles, keeps the technical-noon planetary snapshot, and uses counterclockwise Aries-at-top orientation rather than retaining a false Ascendant orientation.
- the main positions table intentionally relies on localized names rather than platform astrology glyphs, so it remains understandable without font fallback.
- chart-local zodiac and planet symbols are original monochrome vector paths with deterministic bounds; they are functional technical graphics rather than a final artistic font.
- chart, table, and form should fit the default window without row overlap; the chart host follows the smaller of available width and reserved viewport height while the table remains below it through left-column scrolling.
- normal startup keeps the form empty and shows only a short localized empty state; chart and summaries appear together after a successful build.
- future persistence must restore corresponding birth inputs and chart state together, never a chart independently from an empty form.
- date/time controls remain typed Avalonia-native pickers; their Fluent segment resource strings are switched with the application language so RU never exposes English `day/month/year/hour/minute` placeholders.
- readability takes priority over compactness in the birth-data form; date and time inputs should prefer full-width rows over dense side-by-side packing when control chrome becomes fragile.

Current settings direction:

- the shell hosts a minimal settings section whose language and theme changes save immediately through one App-owned preference coordinator;
- settings use a versioned platform AppData JSON document and restore theme before MainWindow construction;
- real settings UX can continue to arrive incrementally without duplicating Tarot-local controls;
- future settings add one confirmed `Сбросить настройки` action for all preferences and one `Открыть папку данных приложения` action for the `<LocalApplicationData>/NoxAeterna` root; Open AppData remains the existing AP1 deliverable rather than a second plan;
- localization now loads from JSON UI catalogs;
- dark/light theme switching now works through `ThemeId`-driven Avalonia resource dictionaries;
- saved readings, profiles, history, SQLite, and other persistence remain deferred; settings use the same semantic cards, editors, popup states, and focus language as the main workspace.

Current localization direction:

- UI labels should now be sourced from flat JSON catalogs under `resources/localization/ui`;
- missing UI keys must degrade deterministically through fallback rather than disappearing.
- interpretation and symbolic localization are still deferred.
- validation feedback inside the astrology workspace must also resolve through localization keys.
- the product name `Nox Aeterna` and intended Latin proper names should remain untranslated across all UI languages.
- built-in date and time picker chrome should follow the selected application culture; known Fluent segment placeholders are explicitly localized through their dynamic resource keys when culture alone is insufficient.

Current theme direction:

- V1 established the paired Obsidian/Porcelain chart palette and transparent annotation treatment;
- V2 applies the same semantic families to the shell, navigation, workspace cards, settings, editors, popups, buttons, validation, disabled states, focus, and scrollbars;
- application surfaces use project-owned semantic brushes rather than independent shell literals or the platform accent;
- navigation selection uses restrained amethyst support and a left rail rather than a dominant solid accent block;
- shell navigation adapts between a labeled desktop pane and a compact icon rail so workspaces retain horizontal room without changing section order or selection semantics;
- the right content region avoids an unnecessary outer frame when real workspace cards already provide structure;
- chart rendering remains isolated and does not absorb application-level brush orchestration;
- exact roles belong to [`VISUAL-DESIGN-SYSTEM.md`](VISUAL-DESIGN-SYSTEM.md), while Avalonia dictionary and style implementation belongs to [`THEMES.md`](THEMES.md).

## Controls

Use familiar desktop controls for serious work:

- Tabs for major views.
- Lists and tables for archives.
- Toolbars for chart controls.
- Toggles and segmented controls for display modes.
- Sliders or numeric inputs for numeric settings.
- Context menus where useful.

The primary section navigation remains a left-side desktop rail. On wide windows it may show localized labels; on compact widths it collapses to centered project-owned icons with localized tooltips and accessible names. A user wide-mode preference is restored after temporary forced collapse, and navigation must never introduce a shell-level horizontal scrollbar.

## Localization and Themes

The UI architecture should support language and theme switching before large screen implementation begins.

Rules:

- Application UI text must use localization keys.
- Interpretation language must be selectable independently from application UI language.
- Symbolic and interpretation text should eventually resolve through localization catalogs, not embedded strings in domain types.
- Theme selection should use stable theme identifiers such as `dark`, `light`, `parchment`, or `obsidian`, not a single `IsDarkTheme` flag.

Intended resource direction:

- JSON-based localization catalogs under `/resources/localization/...`
- JSON-based user preferences in the eventual user app-data directory
- Avalonia resource dictionaries for active app themes

## First User Flows

Likely early flows:

- Create or open personal profile.
- Enter birth data.
- View natal chart snapshot.
- View current transits or lunar event snapshot.
- Save a Tarot reading.
- Open profile archive/history.
