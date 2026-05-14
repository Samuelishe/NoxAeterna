# UI Vision

Nox Aeterna should feel like a serious desktop instrument and personal archive, not a mobile entertainment app.

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
- development-only sample data may still drive that workspace temporarily, but should stay out of the visible shell structure.

Current settings direction:

- the shell may host a minimal settings section before persistence exists;
- language and theme selection can exist as in-memory settings state before real storage is added;
- real settings UX can arrive incrementally rather than all at once;
- localization now loads from JSON UI catalogs;
- dark/light theme switching now works through `ThemeId`-driven Avalonia resource dictionaries;
- persistence and richer theme resources remain deferred.

Current localization direction:

- UI labels should now be sourced from flat JSON catalogs under `resources/localization/ui`;
- missing UI keys must degrade deterministically through fallback rather than disappearing.
- interpretation and symbolic localization are still deferred.

Current theme direction:

- shell and preview surfaces should use theme resources rather than hardcoded product colors where practical;
- the first real theme scope is intentionally small: window, panel, border, foreground-hint, and preview-surface brushes;
- workspace panels may use the same theme resource approach for basic structure and separation;
- chart rendering remains isolated and should not absorb app-level theme orchestration.

## Controls

Use familiar desktop controls for serious work:

- Tabs for major views.
- Lists and tables for archives.
- Toolbars for chart controls.
- Toggles and segmented controls for display modes.
- Sliders or numeric inputs for numeric settings.
- Context menus where useful.

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
