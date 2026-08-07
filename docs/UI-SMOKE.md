# UI Smoke

| Metadata | Definition |
| --- | --- |
| Role | Policy for real-control manual UI smoke and visual evidence. |
| Read when | A meaningful Avalonia UI, rendering, theme, localization, or responsive-layout change needs acceptance evidence. |
| Authoritative for | Real-control UI smoke policy, visual evidence classes, operator workflow, screenshot handling, required states, acceptance, and reporting. |
| Not authoritative for | Exact case data, automated unit-test filters, the current visual palette, or product roadmap. |

Exact cases belong to [`eng/ui-smoke-cases.json`](../eng/ui-smoke-cases.json). The catalog is machine-readable evidence planning; it is not an automated headless UI suite.

## Real-Control Policy

Launch the actual `NoxAeterna.App` process. Enter data through real Avalonia controls and exercise the relevant pointer, keyboard, popup, focus, disabled, selected, resize, and scroll states. Calling coordinator or pipeline methods directly is contract testing, not UI smoke.

Use only cases relevant to the change, plus the milestone set when the stage requires complete UI evidence. Verify dark/light, RU/EN, window modes, and control states named by those cases. Numerical expectations remain owned by golden tests and engine documentation.

## Operator Workflow

1. Build the intended configuration and run the relevant deterministic route.
2. Launch `dotnet run --project NoxAeterna.App/NoxAeterna.App.csproj`.
3. Follow the catalog’s preconditions, inputs, actions, and expected observations using real controls.
4. Capture only the requested visual evidence.
5. Record what was actually observed, including any environment limitation.
6. Close only the application process started for this smoke.

Manual smoke must never silently substitute direct view-model or coordinator calls for real interaction.

## Screenshot Handling

Screenshots are temporary operator evidence. Store them in `%TEMP%` or the ignored `artifacts/ui-smoke/` scope, never in arbitrary repository locations. Catalog filenames are relative basenames, contain no machine path, and set `trackScreenshot` to `false`.

Do not commit screenshots unless a future shipped-asset or explicit reference-image decision assigns repository ownership. Screenshot review is visual evidence, not proof of astronomy values.

## Required Visual States

Select relevant cases that cover:

- empty startup and successful chart generation;
- dark/light application and chart integration;
- RU/EN localization;
- standard, maximized, and live-resize behavior;
- default, hover, pressed/open, selected, focused, disabled, success, and error control states;
- ComboBox popup ownership without platform accent leakage;
- UnknownTime hiding/restoring the entered time as specified;
- chart annotation, axis, aspect, and viewport readability when rendering changed.
- authoritative source markers, source-to-glyph leaders, sign/house membership, independent labels, and crowded-cluster readability when planet layout changed.
- expanded and collapsed shell navigation, pointer and keyboard toggle activation, selection preservation, localized compact tooltips/accessibility, and forced-collapse restoration across the responsive threshold when shell navigation changed.
- Tarot single/three-card draw and redraw, upright-only and reversed-enabled policies, auto/manual reveal, both prototype backs, card pointer/keyboard activation, RU/EN, Obsidian/Porcelain, compact/maximized/live-resize reading-surface behavior, persistence restart, and navigation away/back.

For Tarot workspace changes, perform draws only through the real controls. Verify equal ordered three-card bounds, 7:12 surfaces, clear reversed transformation, selected/hover/focus states, the absence of the selected-card inspector and visible tableau/interpretation headings, reading-surface vertical scrolling, and tableau-owned horizontal overflow without shell clipping. Auto reveal must show faces immediately; manual mode must start with backs and reveal one activated position without exposing hidden-card meaning. Use an injected temporary DEBUG AppData root for persistence smoke and restart from a fresh process.

For INT1 single-card presentation smoke, first launch normal Debug without a preview variable and prove the all-not-ready Classic skeleton stays silent. Then opt in only on Debug with `NOXAETERNA_DEBUG_INTERPRETATION_PREVIEW=resolved`: verify RU/Obsidian and EN/Porcelain pack-local labels, exactly three chips, distinct semantic valence treatments, one-to-three intensity dots, five ordered sections, and one shared outer vertical scroller. Use `resolved-then-none` for the explicit stale-content removal transition. These strings are visual fixtures only and never authorize production readiness or corpus fallback.

For adaptive-shell changes, select every section through the real navigation control, toggle through pointer and keyboard paths, resize below and back above the compact threshold, and verify the previous wide preference returns. Inspect both themes and both UI languages; the compact rail must leave the active workspace and its own vertical scroll ownership intact without creating horizontal clipping.

For planet-semantic changes, verify the wheel without relying on the summary table first. The marker must identify exact longitude, the glyph must remain in the source sign and available source house, the label must stay visibly associated, and aspect chords must still terminate at their source-coordinate interior points.

## Acceptance and Reporting

Report the application configuration, cases exercised, themes/languages/window modes, observations, and screenshot filenames. Explicitly distinguish:

- deterministic tests and golden values;
- real-control interaction evidence;
- screenshot-based visual judgment.

UI smoke passes only when expected states were actually observed in the running application. It does not run in headless T1-B CI.
