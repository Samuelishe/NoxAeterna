# Archived Session Log — 2026-07-31, Part 01

This partial-day chunk retains completed historical evidence without rewriting its headings, summaries, or verification results. It is not current status; use [PROJECT-STATE.md](../../PROJECT-STATE.md) for the current checkpoint.

## 2026-07-31: Annotation-Safe Chart Polish and UnknownTime Editor Correction

Summary:

- Inspected the supplied Prague dark/light known-time screenshots and the light UnknownTime screenshot before editing; confirmed MC–IC, cusp, and connector intrusion in the lower annotation cluster plus the misleading disabled `13:45` display.
- Added a render-owned `ChartPlanetAnnotationLayout` pipeline with measured glyph/text bounds, combined visual and padded protected envelopes, final anchors, displacement state, and connector endpoints.
- Kept geometry source/display anchors, `ChartOrientation`, radial lanes, astronomy, Placidus, viewport fitting, and UnknownTime calculation policy unchanged.
- Made collision handling reuse the existing sub-lanes before a bounded deterministic angular adjustment, and kept the Prague lower and upper protected envelopes disjoint at representative sizes.
- Split minimal chart-background knockout between glyph and label bounds, drew it above cusps, axes, aspects, markers, ticks, and connectors, and terminated connectors at the protected envelope rather than its center.
- Added measured safe inset placement for ASC/DSC/MC/IC and removed the obsolete duplicate `ChartRenderScene.PlanetGlyphs` contract.
- Changed UnknownTime presentation so the disabled time editor is visually empty, while switching back to ExactTime or ApproximateTime restores the last entered session time.
- Added focused deterministic rendering, cluster, connector, angle-label, UnknownTime transition, pipeline, and Prague noon-fallback sanity coverage without raster screenshot tests.

Verification:

- Baseline matched `14f808e5cbf779d8759ba103e7a52330e69714e8`; the worktree was clean and restore/build plus 245/245 tests passed before edits.
- Current build succeeds without warnings and 255/255 tests pass.
- Windows Avalonia smoke covered fresh empty startup, Prague ExactTime at standard and maximized sizes in dark/light themes, Prague UnknownTime in dark/light themes, live resize through `1180x760`, `1650x900`, and `1360x860`, and ExactTime -> UnknownTime -> ExactTime restoration.
- Known-time smoke shows readable Sun/Jupiter/Venus/Mercury and Uranus/Neptune/Saturn clusters; MC–IC is hidden only under Saturn, connectors stop before annotations, and MC/IC remain inset from the rim and viewport.
- UnknownTime smoke shows no houses or angle axes, retains Aries-at-top orientation and the technical-noon planetary snapshot, displays neutral disabled time placeholders, and restores `13:45` when ExactTime returns.
- Temporary screenshots were written only under the local temp directory and were not added to the repository.
- No dependency, external asset, README, THIRD-PARTY, `.se1`, persistence, or interpretation changes were introduced.

Next actions:

- Request final known-time dark/light and UnknownTime screenshots from the user at their display scale.
- Do not advance to `.se1`, persistence, or interpretation until that final visual confirmation and the existing Prague numerical confirmation are explicit.

## 2026-07-31: V1 Visual Design Foundation

Summary:

- Inspected the supplied light-theme screenshot before editing and confirmed that glyph/label background rectangles appeared as opaque stickers across cusps, the planet lane, and the inner ring.
- Removed the annotation-background drawing stage while preserving measured glyph, label, visual, and protected bounds for collision, viewport, and connector contracts.
- Added a small render-owned parameter-interval occluder that returns deterministic visible portions of a straight segment outside protected rectangles, including ordered merging for multiple annotations.
- Routed ordinary house cusps, ASC–DSC and MC–IC axes, planet source ticks, and displacement connectors around measured annotation envelopes; aspect chords remain unchanged because they stop at the inner aspect circle.
- Replaced khaki/sepia chart colors with the role-based Astral Archive V1 palette: cool structure, amethyst/astral element tones, luminous planet annotations, solar principal axes, and theme-aware hard/trine/sextile/conjunction colors.
- Added canonical Obsidian and Porcelain semantic application colors to the Avalonia dictionaries without migrating the existing shell brushes; that app-wide migration remains V2.
- Created `VISUAL-DESIGN-SYSTEM.md` as the authoritative owner of exact semantic roles, paired dark/light tones, chart hierarchy, component states, contrast, effects, and palette evolution.
- Kept astronomy, SwissEphNet, Placidus, orientation, circular collision placement, measured annotation layout, viewport fitting, connector endpoints, empty startup, and UnknownTime policy unchanged.

Verification:

- Baseline matched `2bc87730c28f037a2edb820e9a99edd0f8372d89`; the worktree was clean and restore/build plus 255/255 tests passed before edits.
- Current build succeeds without warnings and 273/273 tests pass.
- Focused tests cover no intersection, single and multiple occluders, fully covered and zero-length segments, boundary contact, overlap merging, order independence, horizontal/vertical/diagonal lines, Prague MC–IC protection, transparent annotation rendering contracts, semantic palette completeness, exact chart colors, theme-aware aspects, and text/chart contrast targets.
- Windows Avalonia smoke covered empty startup, Prague ExactTime in dark/light, Prague UnknownTime in dark/light, standard and maximized windows, live resize through `1180x760`, `1360x860`, and `1650x900`, plus ExactTime -> UnknownTime -> ExactTime restoration.
- ExactTime smoke shows no annotation background shapes; cusp and principal-axis gaps remain local to measured annotations, connectors stay outside foreign annotations, both Prague clusters remain readable, and the full wheel stays fitted.
- UnknownTime smoke keeps Aries at the top, no houses or principal axes, an empty disabled time editor, the technical-noon planet snapshot, and restored `13:45` after returning to ExactTime.
- ExactTime dark maximized, ExactTime light maximized, and UnknownTime light screenshots were generated only in the local temporary directory and were not added to Git.
- No dependency, external asset, README, THIRD-PARTY, `.se1`, persistence, profile, interpretation, Tarot, transit, synastry, geocoding, repository-tooling, or CI change was introduced.

Next actions:

- Request the three final V1 screenshots from the user at their display scale and obtain explicit visual approval.
- Do not start V2 app-wide semantic theme migration or repository tooling T1/T2 until V1 is approved.

## 2026-07-31: V2 Application Theme Migration

Summary:

- Inspected the supplied ExactTime dark/light and UnknownTime light popup screenshots before editing and confirmed that the V1 chart was cohesive while the shell still used neutral-black or beige/brown surfaces, red/salmon selections, and default Fluent control states.
- Expanded both Avalonia theme dictionaries to the complete paired Obsidian/Porcelain semantic color contract and added one project-owned brush for every semantic role.
- Removed the independent legacy `Shell*`, `Workspace*`, and `PreviewSurface*` brush system; production App views now consume semantic roles or reusable semantic classes without raw UI color literals.
- Added `SemanticControlStyles.axaml` after `FluentTheme` for project-owned navigation, button, editor, popup, picker, focus, disabled, validation, card, table, and scrollbar treatment while preserving native templates and behavior.
- Mapped verified public Avalonia Fluent 12.0.2 accent, ComboBox popup, DatePicker/TimePicker presenter, and scrollbar resource seams back to project semantic owners, preventing platform red/salmon leakage.
- Reworked the shell composition around Canvas plus real Surface cards, removed the redundant right-side outer frame, added restrained selected-navigation rails, styled the primary chart-build action, and applied the same card/control language to Settings.
- Kept the renderer-owned V1 chart palette, line occlusion, geometry, astronomy, Placidus, viewport fitting, and UnknownTime calculation behavior unchanged.
- Created `THEMES.md` as the Avalonia implementation owner while retaining `UI-VISION.md` for product direction and `VISUAL-DESIGN-SYSTEM.md` for exact semantic values.

Verification:

- Baseline matched `22939be1b4b2f78444038087bdf7aa28aa2802bf`; the worktree was clean and restore/build plus 273/273 tests passed before edits.
- Current restore/build succeeds without warnings and 281/281 tests pass.
- Focused coverage checks exact dark/light resource parity, component-state values, brush/color consistency, contrast, legacy-palette removal, unique XAML keys, semantic Fluent mappings, required control styles, repeated theme switching, resource resolution, and the absence of raw App-view color literals.
- Windows Avalonia smoke covered empty startup, Prague ExactTime and UnknownTime in dark/light themes, maximized layout, live shell/theme rematerialization, navigation selection/focus, primary actions, ComboBox and picker popups, disabled UnknownTime input, success/error validation, scrollbars, Settings, and RU/EN switching.
- Popup smoke shows no red or salmon selection; light surfaces are Porcelain rather than beige/brown, dark surfaces are Obsidian rather than unrelated neutral black, and focus uses the astral-cyan role.
- ExactTime and UnknownTime screenshots confirm that V1 chart colors, orientations, houses/angle policy, line occlusion, numerical values, and viewport fitting did not regress.
- Five requested V2 screenshots were generated only under the local temporary directory and were not added to Git.
- No dependency, font, asset, README, THIRD-PARTY, `.se1`, persistence, profile, interpretation, Tarot, geocoding, transit, synastry, repository-tooling, or CI change was introduced.

Next actions:

- Request the five final V2 screenshots from the user at their display scale and obtain explicit visual approval.
- After approval, decide whether concrete screenshot evidence warrants a small V3 optical pass; otherwise proceed directly to T1 repository foundation.
- Do not begin V3 or repository tooling before V2 confirmation.

## 2026-07-31: T1-A Documentation and Agent Foundation

Summary:

- Added a compact root guide, current-state handoff, and explicit documentation ownership.
- Kept all chronology through `2026-07-30` intact in one indexed archive chunk while retaining the three `2026-07-31` visual-stage entries in the bounded active log.
- Added machine-readable document budgets, read-only documentation validation, and a read-only Git/.NET repository baseline with console and JSON output.
- Added focused manifest, governance, script-fixture, archive-overlap, and baseline immutability tests.
- Updated documentation routes, immediate repository-foundation priorities, and scoped generated-output ignores without changing application behavior or dependencies.

Verification:

- `eng/repo-baseline.ps1` and `eng/doc-check.ps1` pass in console and JSON modes.
- Restore/build complete without warnings and the full test suite passes.
- Archived chronology matches the original `HEAD` text for its date range; active and archived ranges do not overlap.

Next actions:

- Implement T1-B named test routes, real-control UI smoke catalog, coverage, and CI.
- Do not begin T2 or a new product feature automatically.

## 2026-07-31: T1-B Repository Verification Foundation

Summary:

- Added the test-execution owner, responsibility-oriented leaf/composite registry, and a PowerShell runner that accepts registered names only, enforces bounded timeouts, stops only its own process tree, and requires explicit milestone authorization for `Full`.
- Added a real-control UI smoke owner and validated case catalog with temporary untracked evidence filenames.
- Added private Coverlet collection, unique diagnostic coverage output, and read-only GitHub Actions jobs for documentation, Windows/Linux/macOS milestone tests, and coverage.
- Extended documentation validation to routes, UI cases, CI, and coverage; corrected `PROJECT-STATE.md` so dynamic Git state belongs to the baseline tool.
- Added focused registry, resolution, runner, catalog, coverage, CI, and checker contract tests without changing production behavior.

Verification:

- Baseline and documentation checks pass; named focused/area plans resolve deterministically.
- Debug restore/build, Repository-Verification, relevant area routes, Full milestone suite, and Release coverage complete successfully.
- Generated route, TRX, and coverage artifacts remain ignored.

Next actions:

- Confirm the workflow on its first hosted post-commit run, then begin T2 Project Stats and RAG-lite only on explicit continuation.

## 2026-07-31: T1-B.1 Hosted CI Activation Verification

- Rechecked official stable releases and confirmed `actions/checkout@v7`, `actions/setup-dotnet@v6`, and `actions/upload-artifact@v7`; no workflow rollback was required.
- Verified hosted push run `30619151450` for checkpoint `f61184df0fe395b846bcd66334104fbad409e971`: documentation, Windows/Linux/macOS milestone tests, and diagnostic coverage all passed and published their expected artifacts.
- Kept workflow topology and focused contracts unchanged; session-log rollover is required before T2.
