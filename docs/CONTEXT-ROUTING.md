# Context Routing

| Metadata | Value |
| --- | --- |
| Role | Canonical policy for deterministic, bounded repository context planning. |
| Read when | Selecting initial evidence for a nontrivial code, test, UI, documentation, tooling, or asset task. |
| Authoritative for | Context-planning workflow, task-kind semantics, character budgets, progressive disclosure, retrieval evaluations, and degraded fallback. |
| Not authoritative for | Exact route values, test filters, project status, architecture contracts, or Project Stats measurements. |

## Ownership

Exact task and path mappings belong to `eng/context-routes.json`. Exact retrieval cases belong to `eng/context-evals.json`. The implementation lives in `NoxAeterna.Tools.Repository`; `eng/context-plan.ps1` and `eng/context-eval.ps1` are the operator interfaces. Named test filters remain owned by `eng/test-routes.json`.

The planner reuses the Git-visible factual inventory documented in [PROJECT-STATS.md](PROJECT-STATS.md). Inventory answers which public files exist and how large text files are. Routing policy answers which owners and test routes are relevant. Neither source performs semantic or similarity search.

## Workflow

1. Start with the root `AGENTS.md`, the repository baseline, and `PROJECT-STATE.md`.
2. Choose exactly one task kind and pass concrete repository-relative target paths.
3. Request the smallest realistic character budget.
4. Read selected files first and run returned test route names through `eng/test-route.ps1`.
5. Expand manually only for an unresolved dependency, uncertain symbol ownership, a specific contract risk, or explicit historical provenance.

The initial task kinds are `CodeChange`, `StructuralRefactor`, `TestChange`, `UiChange`, `Documentation`, `Tooling`, and `AssetChange`. Engine ownership is selected by additive path rules, not by creating an engine-specific task kind.

## Exact selection

Evidence comes only from explicit existing target files, task-kind owner documents, matching path-rule documents, and task recommendations. Rules are additive; more-specific and general rules may both contribute. Paths and reasons are deduplicated deterministically.

Existing text targets are mandatory context. Binary targets are metadata-only and consume no character budget. A directory contributes routing only; its children are not selected. A planned public path may match routes without becoming a readable file. Private, sensitive, ignored, generated, absolute, or repository-escaping targets are refused without reading their contents.

The plan returns paths, sizes, reasons, diagnostics, and exact named test routes. It never returns file contents, absolute machine paths, timestamps, embeddings, inferred dependencies, or generated explanations. Archive material is never selected implicitly.

## Budgets and bounds

Character counts use the same factual `.NET string.Length` measurement as Project Stats. Files are indivisible: no partial content is selected. Explicit targets and mandatory owners cannot be dropped to fit a budget. If mandatory context exceeds the requested character budget or the task-kind file limit, planning fails and reports the exact mandatory minimum. Recommendations are added only when the complete file fits both remaining bounds; omitted recommendations remain visible.

The planner is read-only navigation, not a correctness or quality gate. Its budget is a context bound, not an assessment of file quality.

## Retrieval evaluations

`context-eval` runs deterministic cases with must-include, must-exclude, matched-rule, test-route, file-count, and character-count expectations. A route or owner regression must be fixed with concrete evaluation evidence rather than broad heuristic inclusion. Evaluations do not run product tests or the application.

## Progressive disclosure

Do not request a large budget merely for convenience and do not fall back to reading the whole repository. If selected evidence exposes a concrete missing relation, inspect the smallest exact additional owner or source path and consider adding a focused route/evaluation only when the relation is durable.

History and `docs/archive/**` are opt-in. An explicit archive target may be inspected for provenance, but no task kind or path rule owns archived context.

## Degraded fallback

If the planner is temporarily unavailable, retain its diagnostic and use the root `AGENTS.md`, `PROJECT-STATE.md`, the exact owner route table, bounded `rg`/Git discovery, and named tests. Do not perform an automatic restore, read the whole repository, or block an otherwise safe task solely because the navigation helper is unavailable.

## Commands

```powershell
pwsh eng/context-plan.ps1 -Task CodeChange -Path NoxAeterna.Rendering/Charts/CircularChartRenderer.cs -BudgetChars 50000
pwsh eng/context-plan.ps1 -Task Tooling -Path NoxAeterna.Tools.Repository/Context/ContextPlanner.cs -BudgetChars 70000 -CompactJson
pwsh eng/context-eval.ps1
pwsh eng/context-eval.ps1 -Case rendering-code-change -Json
```
