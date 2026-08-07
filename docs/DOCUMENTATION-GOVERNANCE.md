# Documentation Governance

| Metadata | Definition |
| --- | --- |
| Role | Canonical governance policy for repository documentation. |
| Read when | Creating, moving, validating, or changing the ownership of documentation and repository tooling. |
| Authoritative for | Document ownership, reading routes, conflict resolution, update triggers, archive policy, and compactness rules. |
| Not authoritative for | Current project status, product behavior, exact budget values, implementation chronology, or executable test filters. |

## Canonical Ownership

| Document | Authoritative for | Not authoritative for | Read when |
| --- | --- | --- | --- |
| root `AGENTS.md` | Operational routing and Git/privacy rules | Status and history | Every session |
| `docs/PROJECT-STATE.md` | Current checkpoint, focus, and blockers | Chronology and roadmap | Task start |
| `docs/AGENTS.md` | Product identity and extended agent guide | Operational Git/test policy | Product or domain orientation |
| `docs/INDEX.md` | Documentation navigation | Status and exhaustive source map | Discovery |
| `docs/ARCHITECTURE.md` | Stable architecture | Current stage | Dependency changes |
| `docs/ARCHITECTURAL-BOUNDARIES.md` | Hard layer rules | History | Code-boundary work |
| `docs/CODING-GUIDELINES.md` | Coding contracts | Documentation ownership and test filters | C# or XAML work |
| `docs/UI-VISION.md` | Product mood and UX direction | Exact palette | UI design |
| `docs/VISUAL-DESIGN-SYSTEM.md` | Semantic visual roles and exact palette | Avalonia implementation | Visual decisions |
| `docs/THEMES.md` | Avalonia theme implementation | Exact palette values | Theme/resource work |
| `docs/TEST-EXECUTION.md` | Test tiers, execution policy, and evidence boundaries | Exact filters and current totals | Test selection or runner work |
| `docs/UI-SMOKE.md` | Real-control manual UI smoke policy | Exact case data and automated filters | UI acceptance work |
| `docs/PROJECT-STATS.md` | Project Stats purpose, report semantics, exclusions, and interpretation | Documentation budgets, architecture, context routing, and RAG | Repository analysis |
| `docs/CONTEXT-ROUTING.md` | Context planning, task/budget semantics, progressive disclosure, and retrieval-eval policy | Exact mappings, test filters, and project status | Nontrivial task navigation or context-tool changes |
| `docs/ASTRONOMY-ENGINE.md` | Astronomy contracts | UI and layout | Astronomy |
| `docs/GEOMETRY-ENGINE.md` | Render-independent chart geometry | Avalonia styling | Geometry |
| `docs/RENDERING-ENGINE.md` | Renderer contracts | Astronomy | Rendering |
| `docs/TAROT-INTERPRETATION-PACKS.md` | Tarot interpretation-pack identity/boundaries, capabilities, readiness, locale resolution, selection, and partial/missing-content behavior | Authored content, mode composition, production manifest/index/storage structure, Avalonia layout, artwork, and general persistence | Tarot interpretation-pack or locale-resolution work |
| `docs/TAROT-INTERPRETATION-CONTENT.md` | Interpretation-pack authorial identity, Classic voice, single-card sections and orientation content, tags and metrics, translation fidelity, authoring lifecycle, quality, and typography direction | Pack discovery/readiness/fallback, exact serialization/storage, pair or multi-card routing, Avalonia implementation, palette, actual fonts, artwork, and card backs | Tarot content authoring, translation, tags, metrics, or interpretation typography work |
| `docs/TAROT-INTERPRETATION-MODES.md` | Stable Tarot interpretation modes, oriented-pair corpus, multi-card composition, source/storage paths, manifest/index routing, authoring inventory, dependencies, validation, and batching | Pack readiness/fallback, Classic voice, single-card prose, actual content, Avalonia layout, palette/fonts, artwork, settings, and exact implementation classes | Two-card or multi-card interpretation, corpus storage/indexing, authoring inventory, or mode-routing work |
| `docs/TAROT-INTERPRETATION-IMPLEMENTATION.md` | Exact Tarot pack JSON/version/hash contracts, entry/index schemas, typed resolution/cache, project allocation, Set-to-Pack migration, settings/selector gates, staged implementation, and decision coverage | Pack fallback policy, Classic prose, mode semantics, actual content, artwork, fonts/colors, or exact Avalonia design | Tarot interpretation serialization, validation, migration, layer allocation, or implementation staging |
| `docs/NEXT-STEPS.md` | Immediate forward queue | Current checkpoint and history | Planning |
| `docs/DECISIONS-LOG.md` | Durable decisions | Chronology and current status | Decision review |
| `docs/SESSION-LOG.md` | Bounded current-wave chronology | Current truth and architecture | Recent provenance |
| `docs/archive/**` | Retained historical evidence | Current state | Explicit historical need |
| `docs/KNOWN-PROBLEMS.md` | Unresolved defects and risks | Completed history | Risk review |
| `docs/TECHNICAL-DEBT.md` | Intentional shortcuts and debt | Generic roadmap | Cleanup |
| `docs/THIRD-PARTY.md` | Provenance and licenses | Implementation status | External material |
| `eng/document-budgets.json` | Exact budgets and overflow strategies | Human ownership policy | Validation |
| `eng/doc-check.ps1` | Read-only documentation validation | Budget values and content fixes | Documentation validation |
| `eng/test-routes.json` | Exact named test filters and route graph | Human execution policy | Test-route execution |
| `eng/ui-smoke-cases.json` | Exact manual smoke case data | UI smoke policy | Manual UI evidence |
| `eng/context-routes.json` | Exact task/path context mappings | Human workflow and factual inventory | Context planning |
| `eng/context-evals.json` | Exact retrieval regression cases | Context policy prose | Context-route verification |
| `NoxAeterna.Tools.Repository` | Factual inventory, Project Stats, and context-tool implementation | Human policy, quality verdicts, and product behavior | Repository diagnostics or context execution |

## Conflict Resolution

The user prompt and verified current code or runtime evidence override stale documentation. Among documents, prefer the canonical owner in the table above. Correct a non-owner to link to the owner instead of preserving two competing policies.

Dynamic branch, HEAD, parent, operation markers, and worktree state belong to Git and `eng/repo-baseline.ps1`. `PROJECT-STATE.md` may cite a completed checkpoint SHA as historical provenance, but it must not claim to own the current remote or local HEAD.

## Selective Reading

1. Read root `AGENTS.md` and `PROJECT-STATE.md`.
2. Read only the task owners routed from those documents.
3. Do not load every engine document by default.
4. Open archive chunks only for an explicit provenance need.
5. Do not use `SESSION-LOG.md` as routine architecture context.

## Update Triggers

- A meaningful stage updates `PROJECT-STATE.md` and adds one bounded `SESSION-LOG.md` entry.
- An architecture or dependency-direction change updates its architecture owner.
- A durable decision updates `DECISIONS-LOG.md`.
- A visual semantic-role change updates `VISUAL-DESIGN-SYSTEM.md`.
- An Avalonia resource-topology change updates `THEMES.md`.
- A dependency or shipped-asset change updates `README.md` and `THIRD-PARTY.md`.
- Test-route policy changes update `TEST-EXECUTION.md`; exact route/filter changes update only the registry.
- Manual smoke policy changes update `UI-SMOKE.md`; exact case changes update only the catalog.
- Project Stats semantics or exclusions update `PROJECT-STATS.md`; factual implementation changes update the standalone tool without creating competing context policy.
- Context workflow changes update `CONTEXT-ROUTING.md`; exact mappings and retrieval expectations update only their registries.
- `NEXT-STEPS.md` changes only when the immediate queue changes.

## Persistent Artifact Discipline

- Add a tracked document, registry, or report only when it owns an independent durable responsibility that cannot be reconstructed from canonical source, repository state, existing tooling/context owners, `PROJECT-STATE`, and bounded `SESSION-LOG` chronology.
- Use those repository owners as agent memory. Do not create surrogate `progress.json`, `plan.json`, `chunk-plan.json`, duplicate planning Markdown, or generated reports merely to remember recoverable work state between sessions.
- Transient operator reports belong in ignored output or a system temporary directory. Generated output is not a second documentation owner.

## Active Chronology and Archive

`SESSION-LOG.md` contains only the current wave. Completed entries move intact to indexed chunks under `docs/archive/session-log/`; archiving retains exact headings and evidence rather than summarizing, rewriting, or deleting history.

Use full-date or full-range chunks only when every entry in those dates is closed; their ranges cannot overlap one another or active dated headings. Use a partial-day chunk when several meaningful stages share a calendar date and later active entries may use that same date. Partial parts are contiguous from 01, contain only headings for their named date, and cannot fall inside a full archived range. Exact normalized session headings have one owner across the active log and every archive chunk, while different headings on the same partial date may remain active. `eng/doc-check.ps1` owns the executable filename and validation details.

## Compactness

- Prefer links over copied paragraphs.
- Do not duplicate current status.
- File size is a context-cost signal, not an automatic quality verdict.
- Stable, single-purpose references may be larger than active chronology.
- Soft and hard thresholds are defined only by the machine-readable manifest.
- A hard overflow requires owner-guided reconciliation or an intentional archive rollover, never silent limit inflation.
