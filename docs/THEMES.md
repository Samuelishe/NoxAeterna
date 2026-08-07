# Avalonia Theme Implementation

| Metadata | Definition |
| --- | --- |
| Role | Stable implementation owner for application themes in Avalonia. |
| Read when | Changing theme resources, application styles, control states, theme switching, or theme smoke coverage. |
| Authoritative for | Resource-dictionary topology, semantic color-to-brush mapping, application style ownership, selector and class strategy, theme-switch lifecycle, dark/light parity, materialization tests, and theme smoke procedure. |
| Not authoritative for | Exact palette values, product mood, chart geometry, astronomy, architecture dependencies, or current project status. |

## Theme Ownership

Theme responsibilities are deliberately split:

- [`UI-VISION.md`](UI-VISION.md) owns product mood and UX rhythm.
- [`VISUAL-DESIGN-SYSTEM.md`](VISUAL-DESIGN-SYSTEM.md) owns semantic roles, exact paired palette values, contrast targets, and allowed effects.
- this document owns how those roles become Avalonia resources and control states.
- `NoxAeterna.Rendering` owns the independent chart palette and must not read application-shell brushes.

`NoxAeterna.Presentation` may expose stable `ThemeId` metadata and preferences. It must not choose Avalonia colors or inspect dark/light booleans.

## Resource Dictionary Topology

Application theme resources live in paired dictionaries:

- `NoxAeterna.App/Themes/DarkThemeResources.axaml`;
- `NoxAeterna.App/Themes/LightThemeResources.axaml`.

Each dictionary contains the same ordered contract:

1. `Design*Color` resources for canonical semantic roles.
2. Matching `Design*Brush` resources whose colors reference those roles.
3. Narrow mappings to verified public Avalonia Fluent 12.0.2 resource seams needed by native templates.

`App.axaml` loads `FluentTheme` first and `SemanticControlStyles.axaml` second. The active paired dictionary is merged by `AppThemeController`. There is no second independent shell palette and no legacy shell-brush layer.

## Semantic Colors and Brushes

Every canonical color role has one project-owned brush with the same semantic stem:

- `DesignCanvasColor` -> `DesignCanvasBrush`;
- `DesignSurfaceSunkenColor` -> `DesignSurfaceSunkenBrush`;
- `DesignAccentPrimarySoftColor` -> `DesignAccentPrimarySoftBrush`;
- `DesignControlFillColor` -> `DesignControlFillBrush`;
- `DesignFocusRingColor` -> `DesignFocusRingBrush`.

The complete role list and exact Obsidian/Porcelain values belong only to [`VISUAL-DESIGN-SYSTEM.md`](VISUAL-DESIGN-SYSTEM.md) and the two dictionaries. Views consume brushes and never repeat RGB literals.

Verified Fluent mappings such as `SystemAccentColor`, `ComboBoxDropDownBackground`, picker presenter resources, and scrollbar state resources are compatibility seams, not new color owners. Each must point back to a `Design*` resource.

## Application Style Ownership

`SemanticControlStyles.axaml` owns the project-level visual surface for:

- windows and common text roles;
- navigation `ListBoxItem` states;
- the adaptive shell `SplitView`, compact navigation-item alignment, and navigation-toggle states;
- primary and secondary buttons;
- `TextBox`, `ComboBox`, `ComboBoxItem`, `DatePicker`, and `TimePicker`;
- ComboBox popup chrome;
- scrollbars;
- reusable surface cards, table separators, and validation/supporting-text classes.
- prototype Tarot face/back/ornament surfaces and native button hover, pressed, selected, and focus-visible states;
- single-card interpretation section typography, compact tag chips, five valence fills, and intensity-dot shapes.

Styles change visual properties while retaining Fluent control templates, keyboard navigation, picker mechanics, popup behavior, automation semantics, and accessibility behavior.

Reusable classes express local role rather than color:

- `navigation-list`;
- `compact` on the navigation list and `navigation-toggle` on the pane toggle;
- `surface-card`;
- `primary-action` together with the verified Fluent `accent` behavior;
- `supporting`, `subtle`, and `table-header`;
- `validation-success`, `validation-warning`, and `validation-error`.
- `tarot-card`, `tarot-face`, `tarot-back`, and `tarot-ornament`;
- `tarot-interpretation-section-heading`, `tarot-interpretation-section-body`, `tarot-interpretation-tag` with one exact `valence-*` class, and `tarot-interpretation-intensity-dot`.

Do not add new use of old `Shell*`, `Workspace*`, or `PreviewSurface*` keys.

## Control State Mapping

The application state language is:

| State | Semantic mapping |
| --- | --- |
| Default editor | `ControlFill`, `Border`, `TextPrimary`, `TextMuted` placeholder |
| Editor hover | `ControlFillHover`, `BorderStrong` |
| Editor pressed/open | `ControlFillPressed`, visible focus treatment |
| Keyboard focus | `FocusRing`, with a 1.5–2 DIP-equivalent crisp border |
| Disabled | `DisabledFill`, `DisabledText`, normal opacity |
| Default button | `SurfaceRaised`, `BorderStrong`, `TextPrimary` |
| Primary action | `AccentPrimaryStrong`, `SelectionForeground` |
| Navigation hover | `SurfaceRaised` |
| Navigation selected | `AccentPrimarySoft` plus `AccentPrimaryStrong` left rail |
| Navigation toggle | `SurfaceRaised`/transparent treatment with `TextSecondary`, semantic hover/pressed/focus states |
| Popup selected item | `AccentPrimarySoft` plus a restrained strong indicator |
| Success / warning / error | Matching semantic validation role only |

Focus never uses glow. Selected and focused states remain independently recognizable. Disabled data must not resemble active data.

## Theme Switching Lifecycle

`AppThemeController` validates a requested `ThemeId`, removes the previous project theme dictionary, adds one newly materialized paired dictionary, sets `Application.RequestedThemeVariant`, and records the current theme.

Applying the already active theme is a no-op. Repeated dark/light switching must retain exactly one project theme dictionary and must resolve every required semantic brush after each switch.

The current settings flow persists `ThemeId` through the App-owned settings coordinator. Loading and validation remain outside the theme controller itself; App applies the loaded normalized theme before constructing MainWindow, and applying an already active theme remains a no-op.

Shell navigation follows the same session-only rule. `ShellNavigationState` owns the last wide-mode expanded preference and the effective compact state; `ShellNavigationLayout` owns pane lengths and the compact threshold. `MainWindow` maps that presentation state to native `SplitView` properties and semantic style classes. Theme switching changes brushes without reconstructing the selected workspace or navigation state.

## Chart/Application Boundary

Application resources own shell canvas, cards, controls, tables, validation, and the small surface outside a chart render pass.

`ChartRenderPalette` remains renderer-owned and theme-aware. It does not resolve `Design*Brush` resources, and shell code does not reconstruct chart colors. Theme switching selects the existing dark/light chart render options through the chart surface boundary without changing chart geometry or calculations.

## Test Requirements

Focused theme tests must cover:

- exact dark/light key parity for `Design*Color` and `Design*Brush`;
- brush/color type consistency and semantic reference ownership;
- specified component-state values and contrast;
- absence of legacy beige, brown, red, and salmon shell literals;
- required style selectors for navigation, buttons, editors, popups, pickers, and scrollbars;
- unique XAML keys;
- successful resource materialization;
- repeated switching without merged-dictionary duplication;
- absence of raw UI color literals in production App views outside approved paired theme dictionaries.

Tests must not assert private Fluent template structure beyond the verified public resource seams and the minimal popup selector required by Avalonia 12.0.2.

## Manual Smoke Matrix

Check both dark and light variants:

- empty startup;
- ExactTime and UnknownTime astrology charts;
- standard, maximized, and live-resized windows;
- navigation default, hover, selected, and keyboard focus;
- expanded, user-collapsed, forced-compact, and restored-wide shell navigation;
- localized navigation tooltips and accessible names in compact mode;
- primary button default, hover, pressed, focus, and disabled;
- focused text input;
- DatePicker and TimePicker presenters;
- ComboBox closed, open, hovered, selected, and focused;
- disabled UnknownTime editor;
- validation success and error;
- visible non-dominant scrollbars;
- settings page;
- Russian and English UI.
- Tarot single/three-card layouts, both backs, auto/manual reveal, selected/reversed cards, unified reading-surface scrolling, compact tableau overflow, and section reopen state.

Required evidence must include popup states because platform-accent leakage is most visible there.

## Adding a New Semantic Role

1. Confirm that an existing role cannot express the visual purpose.
2. Name the role by function, not hue or theme.
3. Add paired exact values to `VISUAL-DESIGN-SYSTEM.md`.
4. Add the same `Design*Color` and `Design*Brush` keys to both theme dictionaries.
5. Consume the brush from the smallest owning style or view.
6. Add parity, type, contrast, and materialization coverage appropriate to the role.
7. Verify dark/light screenshots before treating the role as stable.

## Forbidden Implementation Patterns

- Raw RGB or named product colors in views and controls.
- Platform accent leakage into selected, focused, or popup states.
- Duplicate independent dark/light brush systems.
- Hardcoded boolean dark-mode checks.
- Color decisions in Domain or Presentation.
- Chart rendering reading application-shell brushes.
- Shell controls reconstructing renderer palette values.
- Broad control-template replacement when a verified resource seam or small style is sufficient.
- Overrides that break native picker input, popup behavior, keyboard navigation, or accessibility.
