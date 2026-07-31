# Repository Checks

| Metadata | Definition |
| --- | --- |
| Role | Operator guide for implemented repository verification commands. |
| Read when | Running the baseline, documentation validation, named tests, or diagnostic coverage. |
| Authoritative for | Available commands and their operator-facing execution contract. |
| Not authoritative for | Product status, documentation budgets, exact route filters, or future tooling behavior. |

PowerShell 7 is required.

## Baseline

```powershell
pwsh eng/repo-baseline.ps1
pwsh eng/repo-baseline.ps1 -Json
```

The baseline reports Git and best-effort repository-local .NET process state. It is read-only and does not require a clean worktree.

## Documentation

```powershell
pwsh eng/doc-check.ps1
pwsh eng/doc-check.ps1 -Json
```

The documentation check validates budgets, owners, local links, full-range and partial-day archive chunks, named-route and UI-smoke registries, CI presence, and coverage collector declaration. Full archives close complete dates; bounded partial-day parts preserve exact completed entries while allowing new active headings on the same date. Duplicate heading ownership, sequence gaps, and full/partial overlap fail validation. Warnings do not fail; errors do.

Both scripts are read-only. They do not mutate Git, edit documentation, run the application or tests, or read AppData.

## Named Test Routes

```powershell
pwsh eng/test-route.ps1 list
pwsh eng/test-route.ps1 list -Json
pwsh eng/test-route.ps1 resolve Chart-Rendering
pwsh eng/test-route.ps1 run Geometry
pwsh eng/test-route.ps1 run Desktop-UI -NoBuild
pwsh eng/test-route.ps1 run Full -NoBuild -AllowMilestone
```

The runner accepts registered names only, applies bounded leaf timeouts, writes ignored logs below `TestResults/RepoRoutes/`, and stops only the process tree it starts. `resolve` and `-DryRun` never execute tests. Exact filters belong to `test-routes.json`; policy belongs to [`docs/TEST-EXECUTION.md`](../docs/TEST-EXECUTION.md).

## Coverage

```powershell
pwsh eng/coverage.ps1
pwsh eng/coverage.ps1 -NoBuild -Json
```

Coverage runs the full suite with `XPlat Code Coverage`, producing TRX and Cobertura beneath a unique ignored `TestResults/Coverage/` directory. It is diagnostic and has no percentage gate.

## Project Stats

```powershell
dotnet run --project NoxAeterna.Tools.Repository -- stats .
dotnet run --project NoxAeterna.Tools.Repository -- stats . --json
dotnet run --project NoxAeterna.Tools.Repository -- stats . --markdown --output project-stats.md
```

Project Stats is a BCL-only, on-demand factual report over Git-visible public files. It does not read AppData or private/sensitive paths, does not infer code quality, and does not generate CI artifacts. See [`docs/PROJECT-STATS.md`](../docs/PROJECT-STATS.md).

The real-control UI catalog is in `ui-smoke-cases.json`; it is manual and is not launched by repository checks or CI. See [`docs/UI-SMOKE.md`](../docs/UI-SMOKE.md).

Generated output is operational evidence, not product documentation. Context routes and planning remain planned for T2-B.
