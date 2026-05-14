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

## First User Flows

Likely early flows:

- Create or open personal profile.
- Enter birth data.
- View natal chart snapshot.
- View current transits or lunar event snapshot.
- Save a Tarot reading.
- Open profile archive/history.
