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
| `docs/ASTRONOMY-ENGINE.md` | Astronomy contracts | UI and layout | Astronomy |
| `docs/GEOMETRY-ENGINE.md` | Render-independent chart geometry | Avalonia styling | Geometry |
| `docs/RENDERING-ENGINE.md` | Renderer contracts | Astronomy | Rendering |
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
- `NEXT-STEPS.md` changes only when the immediate queue changes.

## Active Chronology and Archive

`SESSION-LOG.md` contains only the current wave. Completed dated ranges move intact to indexed chunks under `docs/archive/session-log/`. Archiving retains evidence; it does not summarize, rewrite, or delete history. Archived ranges must be parseable, indexed, and non-overlapping with one another and with active dated headings.

## Compactness

- Prefer links over copied paragraphs.
- Do not duplicate current status.
- File size is a context-cost signal, not an automatic quality verdict.
- Stable, single-purpose references may be larger than active chronology.
- Soft and hard thresholds are defined only by the machine-readable manifest.
- A hard overflow requires owner-guided reconciliation or an intentional archive rollover, never silent limit inflation.
