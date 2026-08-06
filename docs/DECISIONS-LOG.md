# Decisions Log

Lightweight Architecture Decision Record log. New entries should include date, decision, reason, and consequences.

## 2026-05-14: Use .NET 10

Decision: Target .NET 10 for the application and libraries.

Reason: The project is starting fresh and should use the intended modern .NET stack.

Consequences: Dependencies must be verified for .NET 10 compatibility before adoption.

## 2026-05-14: Use Avalonia UI

Decision: Use Avalonia for the cross-platform desktop UI.

Reason: Avalonia supports cross-platform desktop development in C# and fits the target technology direction.

Consequences: UI and rendering code must still respect architectural boundaries and avoid leaking Avalonia into domain, astronomy, interpretation, or persistence.

## 2026-05-14: Use NodaTime

Decision: Use NodaTime as the main time model.

Reason: Birth data, historical timezone behavior, DST, ambiguous local times, and reproducibility require a robust time library.

Consequences: Date/time APIs should flow through NodaTime types. Direct `DateTime` use should be limited to interop boundaries.

## 2026-05-14: Hide Swiss Ephemeris Behind an Interface

Decision: Use Swiss Ephemeris or an equivalent wrapper behind an interface such as `IEphemerisCalculator`.

Reason: The rest of the system should not depend on a specific package or native interop detail.

Consequences: Package choice can change without rewriting UI, domain, geometry, or interpretation logic.

## 2026-05-14: Separate Astronomy, Geometry, Rendering, and Interpretation

Decision: Treat astronomy calculation, chart geometry, drawing, and symbolic interpretation as separate subsystems.

Reason: Each subsystem has different responsibilities, dependencies, and test strategies.

Consequences: More project boundaries are required, but the application remains testable and avoids god objects.

## 2026-05-14: Create a Dedicated Symbolics Layer

Decision: Add a Symbolics layer distinct from Interpretation.

Reason: Structured symbolic knowledge and user-facing interpretation composition are different responsibilities.

Consequences: Symbolic correspondences can be curated, tested, and reused without being buried in prose generation.

## 2026-05-14: Start With Documentation Before Code

Decision: Initialize agent-oriented documentation before writing application code.

Reason: Future autonomous sessions need continuity, boundaries, and product direction before implementation begins.

Consequences: The repository currently documents intent only. Implementation must not be assumed until later stages create code.

## 2026-05-14: Lock Structured-First Interpretation

Decision: Use a structured-first interpretation pipeline:
`SymbolicFactor[] -> MeaningFragment[] -> ContextModifier[] -> Tension/Reinforcement analysis -> InterpretationBlock -> Optional Narrative Layer`.

Reason: This avoids fragile prose-first logic and keeps interpretation explainable, testable, and compositional.

Consequences: Narrative output, including any future LLM usage, must remain optional and downstream from symbolic logic.

## 2026-05-14: Keep Symbolics as a Typed Catalog

Decision: Keep Symbolics as a structured symbolic catalog with typed relationships rather than a graph database system or flat prose repository.

Reason: The MVP needs explainability and maintainability without graph-system overhead or prose-only ambiguity.

Consequences: Symbolics should define typed entities and relationships such as symbols, meanings, relationship types, and source metadata.

## 2026-05-14: Use Conservative MVP Timezone Strategy

Decision: For MVP, allow explicit/manual timezone selection and prioritize reproducibility over automatic place-to-timezone automation.

Reason: Timezone history and location resolution are complex, and pretending they are solved early would create unreliable birth calculations.

Consequences: `BirthMoment` must preserve local time, timezone ID, UTC instant, ambiguity resolution, and source/confidence metadata.

## 2026-05-14: Lock Render-Independent Geometry Contract

Decision: Geometry must produce render-independent models, and rendering must convert prepared models into Avalonia drawing operations.

Reason: This keeps chart layout testable and prevents Avalonia leakage into geometry or domain logic.

Consequences: Geometry must not return Avalonia controls, brushes, pens, or UI objects. Rendering contracts should be explicit.

## 2026-05-14: Enforce Permanent Attribution Tracking

Decision: Every future session must document third-party libraries, frameworks, assets, fonts, rendering systems, datasets, ephemeris sources, tools, borrowed code, adapted code, and generated assets when relevant.

Reason: The project needs durable authorship, licensing, and provenance records from the start.

Consequences: `README.md`, `docs/AGENTS.md`, `docs/CODING-GUIDELINES.md`, and `docs/THIRD-PARTY.md` must be updated whenever new external material is introduced.

## 2026-05-14: Declare Repository Ready for Scaffold Startup

Decision: After this clarification pass, treat the repository as ready for solution scaffold and implementation startup.

Reason: Core philosophical, architectural, rendering, interpretation, attribution, and continuity questions have been documented to a sufficient level for coding to begin.

Consequences: The next major step should be .NET 10 solution scaffold creation and dependency graph setup rather than another large planning pass.

## 2026-05-14: Create Initial Solution Scaffold With Minimal Dependencies

Decision: Create the initial .NET 10 solution scaffold with a minimal Avalonia app shell, class-library layer projects, xUnit test project, repository-level build props, and a narrow project reference graph.

Reason: The repository needed a real implementation starting point without introducing domain behavior, infrastructure behavior, or premature package sprawl.

Consequences: The next implementation step should focus on first domain primitives and tests rather than more scaffold work.

## 2026-05-14: Keep Birth-Time Types in Domain and Resolver in Astronomy

Decision: Place birth-time value objects and the `IBirthMomentResolver` contract in `NoxAeterna.Domain`, with the first TZDB-backed resolver implementation in `NoxAeterna.Astronomy`.

Reason: Birth-time representations are core domain concepts, while timezone-to-instant resolution is a time-conversion rule aligned with the astronomy layer.

Consequences: The domain stays explicit and NodaTime-based, while astronomy owns the first concrete time resolution behavior without introducing ephemeris coupling.

## 2026-05-14: Use Deterministic MVP Birth-Time Resolution

Decision: Use a deterministic MVP resolver strategy where ambiguous local times resolve to the earlier occurrence and invalid local times shift forward by the gap duration.

Reason: The first time model needs reproducible, testable behavior before more elaborate user-configurable resolution strategies exist.

Consequences: `BirthMoment` stores `TimeResolutionStatus`, and tests must cover normal, ambiguous, and invalid local-time cases explicitly.

## 2026-05-14: Keep Position Models in Domain and Calculation Boundary in Astronomy

Decision: Keep `CelestialBody` and `PlanetPosition` in `NoxAeterna.Domain`, while placing `ChartCalculationRequest`, `ChartCalculationResult`, and `IEphemerisCalculator` in `NoxAeterna.Astronomy`.

Reason: The position model is shared domain data, while calculation orchestration and provider abstraction belong to the astronomy layer.

Consequences: Future ephemeris implementations can stay astronomy-local, while domain models remain reusable by chart, transit, and archive features without provider leakage.

## 2026-05-15: Keep Natal Chart Snapshot and Aspect Detection in Domain

Decision: Keep the minimal `NatalChart`, `CalculatedAspect`, and `PlanetaryAspectCalculator` in `NoxAeterna.Domain`.

Reason: These types are pure domain snapshots and deterministic angle rules over already calculated positions. They do not need ephemeris provider details, UI concerns, or persistence behavior.

Consequences: Astronomy remains responsible for producing positions, while the domain can compose chart snapshots and major aspects without Swiss Ephemeris coupling.

## 2026-05-15: Keep Circular Chart Geometry Render-Independent

Decision: Keep the first circular chart layout contracts in `NoxAeterna.Geometry`, using chart-space angles, normalized radius ratios, and plain numeric geometry models with no Avalonia types.

Reason: Chart placement math must stay testable and reusable before any rendering code or UI surface is introduced.

Consequences: Rendering will later consume `CircularChartLayout` output rather than computing placement directly, and geometry conventions such as `0°` at the top and clockwise rotation are now explicit.

## 2026-05-15: Keep Localization and Preferences in Presentation

Decision: Keep localization contracts, fallback resolution, theme identifiers, and language/theme preferences in `NoxAeterna.Presentation` rather than `NoxAeterna.Domain`.

Reason: Domain models must remain language-neutral, while language selection, localization catalogs, and theme selection are application-facing concerns.

Consequences: Future UI, interpretation, and symbolic text should resolve through localization keys and providers, with a deterministic MVP fallback chain of `selected language -> ru -> key`.

## 2026-05-15: Keep the First Rendering Boundary Scene-Based

Decision: Make the first renderer consume `ChartRenderScene` derived from `CircularChartLayout`, instead of accepting `NatalChart` or astronomy-facing models directly.

Reason: Rendering should prove the Geometry -> Rendering handoff without backsliding into raw domain-model rendering or calculation leakage.

Consequences: Geometry remains the only source of chart placement, while rendering stays focused on Avalonia drawing commands, numeric options, and visual placeholder output.

## 2026-05-15: Start the App Shell in Presentation While Keeping Debug Preview Temporary

Decision: Start the application shell in `NoxAeterna.Presentation` with explicit section identifiers and localization-key-based navigation items, while continuing to host the chart preview as a temporary debug section.

Reason: The app now needs a stable shell shape for future real sections, but the current chart host still exists only to verify the rendering pipeline.

Consequences: The shell can grow into real sections without rewriting the window host from scratch, and the debug preview remains visibly temporary instead of being mistaken for the final Astrology UI.

## 2026-05-15: Keep the First Settings Foundation In Presentation and In Memory

Decision: Keep the first language and theme settings foundation in `NoxAeterna.Presentation`, with in-memory updates only and no persistence adapter yet.

Reason: The app needs a real settings shape before storage exists, but introducing persistence now would blur boundaries and overextend the current step.

Consequences: Settings can already model separate application and interpretation languages plus theme selection, but `settings.json` storage, app-data location handling, and real theme resource switching remain deferred.

## 2026-05-15: Use Flat JSON Catalogs for the First UI Localization Loader

Decision: Load UI localization catalogs from simple flat key-value JSON files and compose fallback behavior through `FallbackLocalizationProvider`.

Reason: The app needs real resource-backed localization without introducing a heavy localization framework or premature nested catalog complexity.

Consequences: UI localization can now be loaded from `resources/localization/ui/<language>.json`, while persistence, interpretation localization, and broader resource-loading architecture remain deferred.

## 2026-05-15: Keep the First Real Theme Switching in App and Theme Metadata in Presentation

Decision: Keep theme metadata and selection contracts in `NoxAeterna.Presentation`, and apply the active `ThemeId` through a small `AppThemeController` in `NoxAeterna.App` using Avalonia resource dictionaries.

Reason: Theme identity and preference state belong to presentation concerns, while actual Avalonia resource application belongs at the application host boundary.

Consequences: Dark/light switching now works in memory without polluting domain, rendering, or infrastructure. Persistence, richer theme catalogs, and broader design-system work remain deferred.

## 2026-05-15: Replace the Visible Debug Section With an Astrology Workspace Foundation

Decision: Replace the visible debug-preview shell route with a first reusable astrology workspace foundation, while keeping development-only sample chart generation in an internal sample area under `NoxAeterna.App`.

Reason: The app now needs a real workspace structure that can later host input, chart controls, and interpretation panels without presenting the chart area as pure debug infrastructure.

Consequences: The shell opens into the astrology workspace by default, rendering stays isolated behind `ChartRenderScene`, and development-only sample data remains an internal temporary source rather than a visible product section.

## 2026-05-15: Keep Birth-Data Parsing and Validation in Presentation

Decision: Keep the first structured birth-data input state, parsing, validation, and `BirthData` mapping in `NoxAeterna.Presentation`, then pass only validated values into `NoxAeterna.Domain`.

Reason: Input parsing rules, field errors, and localization-key-based validation feedback are presentation concerns, while domain types should continue receiving clean value objects instead of UI-specific text state.

Consequences: The astrology workspace can validate and map user input without leaking text parsing into domain models. Resolver and calculation wiring can be added later on top of already validated `BirthData`.

## 2026-05-15: Keep Birth Input Offline-First and TZDB-Based

Decision: Refine the initial birth-data input flow around an offline-first mode: date picker, constrained time input, manual coordinates, and timezone selection from the local TZDB list.

Reason: The MVP must remain honest and usable without network access, while still preparing for future online lookup and AppData-backed recent-place storage.

Consequences: Online geocoding, reverse lookup, and a bundled world-city database remain deferred. Manual coordinates stay as the current truthful fallback, and arbitrary timezone text is no longer accepted as the main UI path.

## 2026-05-15: Preserve Product Names Across Localizations and Apply App Language to Date-Time Controls

Decision: Keep `Nox Aeterna` and other intended Latin proper names untranslated in user-facing localization catalogs, and apply the selected application language to culture-sensitive desktop controls through an app-level culture boundary.

Reason: Product names should remain stable brand identifiers, while mixed-language date and time picker chrome becomes confusing if desktop controls do not follow the chosen UI language.

Consequences: Localization catalogs must preserve `Nox Aeterna` verbatim across languages, and `App` is responsible for applying culture to date and time controls without leaking localization behavior into domain models.

## 2026-05-15: Use an Input-Driven Demo Chart Pipeline Before Real Ephemerides

Decision: Connect validated birth-data input to the visible chart through `IBirthMomentResolver`, a clearly named fake deterministic `IEphemerisCalculator` implementation, `NatalChart`, geometry layout, and the existing renderer.

Reason: The app needs an honest end-to-end chart rebuild flow before Swiss Ephemeris integration, but that flow must stay visibly marked as demo-only and keep rendering isolated from calculation logic.

Consequences: `NoxAeterna.App` now acts as the composition root for the current development chart pipeline and references `NoxAeterna.Astronomy`. The visible chart can rebuild from user input, while real astronomy, persistence, and geocoding remain deferred.

## 2026-05-15: Place the First Real Swiss Ephemeris Adapter in Infrastructure

Decision: Put the first real `IEphemerisCalculator` implementation in `NoxAeterna.Infrastructure` as `SwissEphemerisCalculator`, using `SwissEphNet` while keeping all package-specific types inside the adapter boundary.

Reason: Swiss Ephemeris integration is an external dependency concern, not a domain, presentation, rendering, or geometry concern. The app already acts as the composition root and can swap implementations without changing core contracts.

Consequences: The live chart path now uses real planetary positions through `IEphemerisCalculator`, while the old `DevelopmentEphemerisCalculator` remains available as a fallback or test tool. External ephemeris data-file setup and formal repository license alignment remain follow-up items.

## 2026-05-15: Keep Chart Glyphs Universal and Localized Names in Presentation

Decision: Use universal Unicode zodiac and planetary glyphs inside the render scene for the first readable chart, while keeping localized planet and zodiac names in presentation-side position-summary models.

Reason: The chart needs to become recognizable without leaking localization into rendering math or making the low-level renderer depend on human-language resource lookup.

Consequences: Rendering stays scene-driven and language-agnostic, while the readable positions table is built from presentation models using localization keys. Future font validation remains an open follow-up item.

## 2026-05-15: Use a Real Ephemeris-Backed Startup Chart Instead of Rounded Sample Positions

Decision: Build the initial visible workspace chart from deterministic sample birth data resolved through the live SwissEphNet-backed pipeline instead of integer-only hardcoded sample longitudes.

Reason: The previously visible rounded degree values looked like a formatting defect and obscured whether the real ephemeris adapter was actually active.

Consequences: Startup charts now reflect the current live astronomy path, readable summaries can show meaningful within-sign minutes, and the old hardcoded sample chart remains only as a fallback or dev artifact rather than the visible default.

## 2026-05-15: Keep Product Assets in the Repository and Runtime State in AppData

Decision: Treat shipped application assets as repository-owned resources, while keeping user-specific runtime data in AppData or the equivalent platform-specific user data directory.

Reason: Product resources must remain versioned, reviewable, and reproducible, while user settings, caches, history, and saved data must not leak into the repository or installation directory.

Consequences: Future icons, textures, Tarot art, theme assets, and custom glyph resources belong under versioned `resources/` paths, while preferences, saved charts, readings, and caches stay outside the repository.

## 2026-07-30: Separate Named Chart Radial Lanes

Decision: Make the circular geometry contract explicitly own non-overlapping zodiac, planet, aspect, and reserved future-house radial zones, including ordered planet sub-lanes.

Reason: Incidental radius constants allowed labels, glyphs, and aspect chords to compete for the same visual space and left rendering without safe semantic boundaries.

Consequences: Geometry remains viewport-independent but now guarantees zone ordering and anchor containment. Houses are not implemented; only their future ring interval is reserved.

## 2026-07-30: Preserve Source Angle Separately From Display Placement

Decision: Store source astronomical angle and collision-safe display angle separately on every planet glyph slot.

Reason: Deterministic cluster spreading must never rewrite the longitude used by astronomy summaries or aspect endpoint geometry.

Consequences: Planet glyphs may move within bounded display lanes, while source ticks and all aspect endpoints continue to use the original longitude.

## 2026-07-30: Own Functional Chart Glyphs as Project Vector Graphics

Decision: Render all current chart zodiac and planet symbols from original project-owned monochrome path definitions.

Reason: Unicode variation selectors and platform font fallback produced inconsistent emoji-style boxes and non-deterministic bounds.

Consequences: The chart no longer depends on emoji fonts, external fonts, or image assets. The 22 current paths are functional technical graphics and can receive later optical refinement without changing geometry contracts.

## 2026-07-30: Use Placidus as the Explicit First House System

Decision: Model house systems explicitly and implement `HouseSystem.Placidus` as the only current option without adding a selector or a silent fallback.

Reason: House results must be reproducible, and a provider error must not silently change the astrological method.

Consequences: Every house request carries its system. Unsupported geographic conditions yield a typed unavailable result while the planetary chart remains usable.

## 2026-07-30: Keep House Calculation Behind a Separate Astronomy Boundary

Decision: Keep `IEphemerisCalculator` focused on celestial positions and introduce `IHouseCalculator` with provider-independent request and result contracts.

Reason: Cusps and chart angles are location- and time-sensitive chart structure, not planets or synthetic celestial bodies.

Consequences: SwissEphNet constants, array indexing, system codes, and calls remain inside Infrastructure. Domain, geometry, rendering, and presentation receive only project-owned types.

## 2026-07-30: Orient House Charts With the Ascendant at Left

Decision: Apply one geometry-owned transform that places an available Ascendant at chart-space 270 degrees, while charts without houses remain Aries-at-top.

Reason: Zodiac sectors, planets, source ticks, aspects, cusps, and axes must rotate as one coherent chart instead of applying renderer-specific offsets.

Consequences: MC remains at its calculated transformed angle and is not forced to the top. Original zodiac longitudes remain unchanged and distinct from rotated chart angles.

## 2026-07-30: Unknown Birth Time Forbids Houses and Chart Angles

Decision: Never use the technical noon fallback to calculate or orient houses, ASC, MC, DSC, or IC.

Reason: Planet positions can use a documented approximation when time is absent, but houses and principal angles change too materially for noon data to be presented as natal structure.

Consequences: `UnknownTime` keeps planetary positions, stores an explicit unavailable status, renders no house geometry, preserves Aries-at-top orientation, and shows a short localized UI message.

## 2026-07-30: Correct Zodiac Projection Direction Without Changing Chart-Space Convention

Decision: Keep chart-space `0°` at the top with angles increasing clockwise, but project source zodiac longitude counterclockwise: Aries-at-top uses `normalize(-longitude)`, and Ascendant-at-left uses `normalize(270° + ascendant - longitude)`.

Reason: The earlier Ascendant-at-left decision placed ASC correctly but incorrectly added source longitude in clockwise screen space, mirroring the zodiac, houses, planets, and aspects.

Consequences: ASC remains left and MC remains at its calculated transformed angle, while zodiac longitude now increases in the conventional counterclockwise screen direction. The same geometry-owned transform applies to every chart layer, and all source longitudes remain unchanged.

## 2026-07-30: Keep Normal Startup Empty and Restore Inputs With Chart State

Decision: Normal startup must not materialize a deterministic sample chart when the birth-data form is empty.

Reason: A real-looking Prague chart beside unrelated empty inputs falsely implies that the visible chart belongs to the current form.

Consequences: The coordinator may start without a chart or scene, and chart plus summaries appear only after a successful build. Sample factories remain test/debug fixtures. If persistence is added later, corresponding birth inputs and chart state must be restored together.

## 2026-07-30: Fit the Whole Wheel to Width and Viewport Height

Decision: Size the chart square from the smaller of available content width, finite chart-column viewport height after reserved chrome, and the 1100 DIP large-monitor cap.

Reason: Width-only sizing allowed a maximized window to create a wheel taller than the visible viewport.

Consequences: The full wheel is visible at the top of the left column, while the existing single column scrollbar moves to the position and angle summaries below it. Radius-responsive rendering also enforces readable minimum line weights and contrast.

## 2026-07-31: Keep Measured Planet Annotation Protection in Rendering

Decision: Treat each planet glyph, degree label, and optional retrograde marker as one render-owned protected visual whose measured DIP bounds control collision checks, line occlusion, and connector termination.

Reason: Geometry must continue to own astronomical source/display anchors without depending on Avalonia text metrics, while the renderer is the first layer that knows the responsive glyph size, font size, measured text, and viewport required for exact visual protection.

Consequences: Geometry and `ChartOrientation` contracts remain unchanged. Rendering may reuse existing radial sub-lanes and apply a small bounded deterministic angular correction. Planet annotations remain transparent, straight secondary lines omit measured protected intervals physically, connectors stop at the protected envelope, and `ChartRenderScene.PlanetAnnotations` remains the single canonical planet visual pipeline.

## 2026-07-31: Own Exact Visual Semantics in One Design-System Document

Decision: Make `VISUAL-DESIGN-SYSTEM.md` the canonical owner of semantic color roles, paired dark/light tone systems, chart hierarchy, component states, contrast principles, effects, and palette evolution.

Reason: Product mood and UX direction belong in `UI-VISION.md`, but exact palette tables duplicated across visual prose, resource dictionaries, and renderer notes would drift and leave later theme work without a stable owner.

Consequences: Obsidian and Porcelain use the same semantic roles and related hue families with theme-specific RGB values. Implementation names colors by purpose, focused tests enforce critical contrast and role coverage, and later palette changes update the design-system owner and both theme variants together.

## 2026-07-31: Keep Planet Annotations Transparent and Occlude Straight Lines

Decision: Preserve measured annotation envelopes as invisible render geometry and draw only the complementary visible intervals of straight structural lines outside those envelopes.

Reason: Background-colored glyph and label rectangles erased radial structure beyond the actual line intersection and appeared as opaque stickers, especially in the light theme.

Consequences: No annotation rectangle, pill, card, glow, or shadow is drawn. House cusps, principal axes, source ticks, and connectors use a small deterministic segment occluder; aspect chords remain unchanged while they stay inside the aspect circle. Rectangle order cannot affect the result, and Geometry receives no Avalonia measurement types.

## 2026-07-31: Own Application Themes Through Semantic Brushes and Narrow Avalonia Styles

Decision: Materialize every application semantic color as a paired project-owned brush, apply shell and control states through one application style resource, and map only verified public Fluent resource seams back to those semantic owners.

Reason: Independent shell literals and platform accent defaults made the application visually unrelated to the accepted chart palette and allowed red/salmon selection to leak into navigation and popups. Replacing native control templates broadly would create unnecessary accessibility and interaction risk.

Consequences: Obsidian and Porcelain dictionaries have exact role parity, views do not contain raw product colors, navigation/editors/buttons/pickers/popups/scrollbars share one state language, and platform accent cannot become a second palette. Native control behavior remains owned by Avalonia Fluent, while Nox Aeterna owns its visual surface.

## 2026-07-31: Separate Visual Design Ownership From Avalonia Theme Implementation

Decision: Keep `VISUAL-DESIGN-SYSTEM.md` authoritative for exact semantic values and visual rules, and make `THEMES.md` authoritative for Avalonia resource topology, style/class strategy, switching lifecycle, and theme tests.

Reason: Palette decisions and framework implementation evolve at different rates and should not be duplicated across product mood prose, XAML integration notes, and status logs.

Consequences: `UI-VISION.md` remains the high-level product direction, palette changes update the design-system owner and paired dictionaries, and Avalonia implementation changes update `THEMES.md` without redefining colors. Domain, Presentation, and chart Rendering remain free of shell color decisions.

## 2026-07-31: Establish a Compact Operational Repository Entry Point

Decision: Make root `AGENTS.md` the canonical operational entry point and keep `docs/AGENTS.md` as the extended product, domain-navigation, and attribution guide.

Reason: Automatically discovered session rules must be short and unambiguous, while product identity and detailed navigation need a stable home without competing Git or status policy.

Consequences: Every session starts with the read-only baseline and `PROJECT-STATE.md`; Git/privacy/verification rules belong at the root, and product guidance links back to that owner.

## 2026-07-31: Separate Current State, Ownership, and History

Decision: Give current handoff to `PROJECT-STATE.md`, documentation ownership to `DOCUMENTATION-GOVERNANCE.md`, recent chronology to a bounded `SESSION-LOG.md`, and completed chronology to indexed archive chunks.

Reason: Current truth, durable policy, and historical evidence have different reading and update costs and must not compete as interchangeable sources.

Consequences: Owners link rather than duplicate status, archived evidence is retained intact, ranges are machine-checked for overlap, and the active log remains current-wave context.

## 2026-07-31: Keep Documentation Budgets Machine-Readable and Checks Read-Only

Decision: Store exact documentation budgets and overflow strategies only in `eng/document-budgets.json`, and validate them plus repository state through read-only PowerShell scripts that never auto-fix.

Reason: Exact thresholds need one deterministic owner, while observability tooling must not rewrite history, raise limits, alter Git, or turn a dirty worktree into an error condition.

Consequences: Soft overflow warns, hard overflow and broken contracts fail, archive rollover remains an explicit human-owned operation, and both scripts expose stable console and JSON reports suitable for focused tests and later CI.

## 2026-07-31: Own Exact Test Selection Through Named Routes

Decision: Keep exact filters in `eng/test-routes.json`, human execution and evidence policy in `TEST-EXECUTION.md`, and execution in `test-route.ps1`.

Reason: Responsibility names remain stable and reviewable while raw shell filters, duplicated commands, and hidden full-suite runs are unsafe and prone to drift.

Consequences: Focused and area routes are deterministic and sequential. `Full` is milestone/CI evidence requiring explicit authorization, historical test totals are never acceptance gates, and runner timeouts terminate only the launched process tree.

## 2026-07-31: Keep UI Smoke as Real-Control Manual Evidence

Decision: Own manual interaction policy in `UI-SMOKE.md` and exact case data in `eng/ui-smoke-cases.json`; do not execute that catalog in headless T1-B CI.

Reason: Unit tests and golden fixtures cannot prove popup, focus, hover, DPI, resizing, theme switching, or perceived visual quality, while manual smoke cannot prove deterministic numerical contracts.

Consequences: Operators launch the actual Avalonia app and use real controls. Screenshots remain temporary untracked visual evidence, and reports distinguish automated, numerical, interaction, and screenshot evidence.

## 2026-07-31: Treat Coverage as Diagnostic and CI as Cross-Platform Evidence

Decision: Collect Cobertura through private `coverlet.collector` without a percentage gate, and run the Full registered route on Windows, Linux, and macOS with read-only GitHub Actions permissions.

Reason: Coverage identifies exercised paths but is not a correctness proof; the supported desktop target families need repeatable hosted evidence without deployment, secrets, or UI-smoke simulation.

Consequences: Coverage artifacts are unique and retained only as operational evidence. CI uses official actions, uploads short-lived TRX/log/coverage artifacts, and does not launch the application.

## 2026-07-31: Keep Dynamic Git State Out of Project Handoff

Decision: Branch, current HEAD, parent, operations, and worktree status belong to Git and `repo-baseline.ps1`; `PROJECT-STATE.md` may cite only completed checkpoint commits as provenance.

Reason: A committed document that claims to own a dynamic HEAD becomes stale immediately after the next commit.

Consequences: Session startup observes Git directly, while the handoff remains stable across commits and owns only checkpoint meaning, focus, preserved contracts, and blockers.

## 2026-07-31: Separate Factual Repository Inventory From Context Policy

Decision: Keep one BCL-only `NoxAeterna.Tools.Repository` executable as the reusable owner of Git-visible public-file inventory and diagnostic Project Stats, while reserving context routes, ranking, and character-budget policy for T2-B.

Reason: File metadata, project references, and lexical topology are reusable facts, but turning those facts into agent context is a separate policy problem. Splitting the tool into several projects or embedding RAG decisions now would create premature architecture.

Consequences: Reports are on-demand diagnostic signals rather than quality gates or automatic refactoring verdicts. Private/sensitive and generated/runtime paths are never read, output reports remain ignored, CI gains coverage through the normal test suite without publishing a new artifact, and future context planning reuses the factual inventory instead of creating another scanner.

## 2026-07-31: Use Exact Deterministic Routes for Bounded Agent Context

Decision: Build RAG-lite from one task kind, concrete repository-relative targets, exact additive path rules, canonical owners, named test routes, and an explicit character/file budget. Keep exact mappings in `eng/context-routes.json` and retrieval regressions in `eng/context-evals.json`.

Reason: Repository navigation needs repeatable precision and progressive disclosure, not embeddings, semantic guesses, background indexes, or a second scanner. The existing Git-visible factual inventory already owns public file discovery and character counts.

Consequences: Plans are read-only paths and metadata rather than file contents, archives are never implicit, mandatory evidence cannot be silently dropped to fit, and evals detect route inclusion/exclusion regressions. Planner output is navigation rather than a quality gate or permission to ignore code and architecture owners.

## 2026-07-31: Make Planet Source Markers Authoritative and Annotations Semantically Bounded

Decision: Treat each exact source dot/notch as the planet coordinate, while the glyph and degree/retrograde label are separate render-owned annotations. Place glyphs radial-first, limit longitude adjustment to eight degrees, forbid crossing the source sign or reliable source house, and lay out labels independently without Cartesian glyph fallback.

Reason: A technically exact tick is insufficient when a distant combined glyph/label group can imply another sign, house, or aspect origin. Rendering is the first layer that knows measured viewport bounds, but source longitude, house membership, and aspect endpoints must remain geometry/domain facts.

Consequences: Geometry supplies exact source angles, deterministic clusters, preferred radial lanes, and source-house metadata without pixel collision spreading. Rendering always draws an exact marker and source-to-glyph leader, uses an optional quieter glyph-to-label leader only for non-adjacent labels, preserves source-based aspects, and accepts controlled crowding before violating sign/house semantics or hiding a planet.

## 2026-07-31: Keep Shell Navigation Adaptive, Session-Scoped, and Workspace-Neutral

Decision: Use a native compact-inline Avalonia `SplitView` with a user-controlled wide-mode preference, a forced compact viewport mode, and project-owned vector navigation icons shared by every workspace.

Reason: A permanently labeled 240-DIP pane spends scarce horizontal space that future Tarot and other workspaces need, while mobile bottom navigation or a top dropdown would weaken the desktop information architecture.

Consequences: Presentation owns navigation preference and viewport state, App maps it to the real control, compact mode retains localized tooltips and accessibility names, and returning to a wide window restores the prior preference. State remains in memory, section order and selection are preserved, and no Tarot-specific dependency enters the shell.

## 2026-08-03: Separate Tarot Semantics From Every Visual Pack Layer

Decision: Keep stable Tarot deck/card structure in Domain, independent from artwork packs, presentation skins, and selectable back variants. Do not derive Major identities or display meaning from enum ordinals, filenames, localized strings, or one historical numbering tradition.

Reason: Readings, persistence, alternate artwork, user packs, and future asset tooling must refer to the same semantic cards without turning every visual treatment into a different deck.

Consequences: T0-A provides a standard 78-card structural catalog, typed spreads, and injected in-memory drawing only. Future deck definitions own explicit display numbering/order metadata; shipped curated artwork stays project-owned, user visual packs belong in AppData, and raw generations never enter the repository.

## 2026-08-03: Keep Tarot Drawing Deterministic and Ambient-State-Free

Decision: Draw without replacement through a project-owned injected index source, require the caller to supply the reading timestamp, and represent an undersized deck as a typed failure.

Reason: UI, persistence, system time, and framework-global randomness must not determine or hide domain behavior.

Consequences: Upright-only and upright/reversed policies are explicit, replayable fake sequences reproduce assignments, invalid RNG output is surfaced, and T0-A performs no automatic save or AppData write.

## 2026-08-03: Keep the First Tarot Slice Prototype-Honest and Pack-Independent

Decision: Drive the first playable Tarot workspace from the existing semantic draw engine, model artwork, skin, back, and interpretation selection with independent typed identities, and render only project-owned programmatic prototype cards at a canonical 7:12 ratio.

Reason: Real interaction and responsive composition must be validated before choosing an artwork pipeline, while alternate visuals and future meanings must never fork semantic card identity.

Consequences: Presentation owns in-memory workspace state, Infrastructure supplies runtime randomness behind the Domain interface, App owns temporary vector surfaces, and only the two real Black Sun/Lunar Seal back choices are exposed. T1 adds no final illustrations, fabricated interpretation prose, persistence, assets, packages, or Tarot-specific project.

## 2026-08-03: Separate Versioned Asset Sources From AppData Runtime Packs

Decision: Keep curated built-in packs as versioned repository/installation seed sources, synchronize managed files into AppData without destructive mirroring, and make AppData the eventual runtime discovery root for built-in and user packs. Use exact semantic stems, separate fingerprint/validation state, normalized PNG cache, and Classic placeholder fallback; do not trust a user manifest to declare itself validated.

Reason: Shipped assets must remain reproducible and reviewable, while installed updates, user packs, caches, validation evidence, and platform paths require a writable runtime boundary. Fuzzy discovery, destructive sync, and duplicated runtime/tool validators would make pack identity and user ownership unsafe.

Consequences: ART-LN can continue in the accepted repository structure before AppData work. AP1–AP5 stage seeding, registry/normalization, tooling, manual discovery, and import UI; PKG1 verifies the same seed contract across published platforms. Detailed contracts belong to `ASSET-PACK-RUNTIME.md`.

## 2026-08-05: Return Tarot Artwork Creation to the Owner

Decision: Retire the repository-owned and user-level Tarot generation skill. The owner creates and artistically accepts future illustrations with ChatGPT outside Codex; the detailed handoff contract belongs to `resources/assets/tarot/artwork-packs/lupus-noctis/LUPUS-NOCTIS.md`.

Reason: Codex-driven generation and artistic review consumed disproportionate time and limits, encouraged unnecessary regeneration, and did not provide reliable visual judgment.

Consequences: Codex starts only from owner-approved PNG batches and owns their technical import and repository integration. The dedicated skill source, installer, installed copy, and active installation or invocation instructions are removed.

## 2026-08-06: Store User Preferences in Versioned AppData JSON

Decision: Store application language, interpretation language, theme, and Tarot workspace selections in one App-owned schema-versioned JSON document under the platform LocalApplicationData directory.

Reason: Preferences need restart persistence without introducing SQLite, saved-reading storage, or file/JSON dependencies into Presentation and Domain.

Consequences: App owns path resolution, DTO mapping, validation, atomic save, and diagnostics. Presentation owns typed immutable preference state; actual changes save once, while readings, reveals, selections, and scroll state never persist.

## 2026-08-06: Keep Tarot Reveal State as Presentation Visibility Policy

Decision: Keep revealed positions and auto-reveal behavior in Presentation, independently from the immutable Domain reading.

Reason: Reveal controls when information becomes visible; it does not change which semantic cards were drawn or their orientation.

Consequences: Auto reveal affects only subsequent draws, manual reveal exposes one position, and preference changes never mutate the current reading retroactively.

## 2026-08-06: Forbid Hidden Cards From Influencing Visible Interpretation

Decision: A hidden card cannot influence visible interpretation text, titles, keywords, transitions, advice, synthesis, or diagnostics.

Reason: Manual reveal is meaningful only if unrevealed information cannot leak through adjacent content.

Consequences: Single-card content waits for its reveal, future pair content waits for both cards, three-card fragments are reveal-gated, transitions require both involved cards, and final synthesis requires all cards.

## 2026-08-06: Treat Required Lupus Noctis Failure as Unavailable

Decision: Keep Lupus Noctis as the sole user-facing built-in Tarot artwork pack and never create a user-facing Classic selection when that required pack fails.

Reason: Silent prototype fallback would misrepresent a damaged required built-in contract and weaken the accepted TAROT-ART-RUNTIME-1 behavior.

Consequences: Draw is disabled with a localized controlled diagnostic; `prototype-symbolic` remains only an internal test/diagnostic seam.

## 2026-08-06: Ship Tarot Interpretations as Independent Selectable Data Packs

Decision: Use non-executable interpretation data packs independently from artwork, semantic deck, skin, back, theme, profile, reading, and UI layout; make `classic` (`Классика` / `Classic`) the future default.

Reason: Meaning systems and visual/application selections must evolve and switch independently.

Consequences: Stable language-neutral pack IDs and localized names support multiple selectable packs without changing drawn cards.

## 2026-08-06: Declare Locale Readiness Per Pack and Mode

Decision: Declare one manual `ready` flag per interpretation pack, locale, and reading-mode module, with no per-entry readiness.

Reason: Intentional partial publication needs an explicit coarse owner-controlled boundary.

Consequences: Runtime and validators never infer or mutate readiness; partial packs and unfinished locales remain selectable.

## 2026-08-06: Resolve Tarot Interpretation Locales Through English and Russian

Decision: Resolve each mode module through the deduplicated chain `requested locale -> English -> Russian -> no displayable content`.

Reason: Predictable fallback must support partial locale progress without mixing languages inside one result.

Consequences: One resolved mode result uses one locale for all of its content.

## 2026-08-06: Do Not Fall Back From a Damaged Ready Module

Decision: If a module declared ready has missing, unreadable, or incomplete required content, return no displayable interpretation and do not try another locale.

Reason: Locale fallback must not mask a broken published module.

Consequences: Internal diagnostics may identify the damage, while the application remains operational.

## 2026-08-06: Keep Missing Tarot Interpretation Content Silent

Decision: Show no placeholder, unavailable message, fallback explanation, empty surface, heading, or technical banner when interpretation content is absent.

Reason: Package and localization implementation details are not useful reading content.

Consequences: The interpretation host stays empty; resolved locale and diagnostics remain internal evidence only.

## 2026-08-06: Keep Reading Modes Independent From Interpretation Completeness

Decision: Make a spread available when its Domain, Presentation, and UI behavior is implemented, regardless of pack content.

Reason: Card-reading capability must not be blocked by an incomplete meaning corpus.

Consequences: Unsupported modes still draw and present cards while their interpretation host remains empty.

## 2026-08-06: Persist Interpretation-Pack Selection and Re-Resolve Immediately

Decision: Persist the selected interpretation-pack ID and immediately re-resolve visible content when the pack or interpretation language changes.

Reason: Preference continuity and responsive selection must not require a new draw.

Consequences: The current semantic reading, artwork, reveal state, and hidden-card policy remain unchanged; fallback locale and rendered prose are not settings.
