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

For planet-semantic changes, verify the wheel without relying on the summary table first. The marker must identify exact longitude, the glyph must remain in the source sign and available source house, the label must stay visibly associated, and aspect chords must still terminate at their source-coordinate interior points.

## Acceptance and Reporting

Report the application configuration, cases exercised, themes/languages/window modes, observations, and screenshot filenames. Explicitly distinguish:

- deterministic tests and golden values;
- real-control interaction evidence;
- screenshot-based visual judgment.

UI smoke passes only when expected states were actually observed in the running application. It does not run in headless T1-B CI.
