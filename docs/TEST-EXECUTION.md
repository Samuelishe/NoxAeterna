# Test Execution

| Metadata | Definition |
| --- | --- |
| Role | Human policy for selecting and executing repository verification. |
| Read when | Choosing tests during implementation, preparing milestone evidence, or changing the test runner. |
| Authoritative for | Validation tiers, named-route selection, build/NoBuild policy, timeout and process ownership, evidence boundaries, coverage policy, and CI/manual-smoke separation. |
| Not authoritative for | Exact filters, current test totals, product roadmap, UI smoke case data, or code-coverage percentage targets. |

Exact executable filters belong only to [`eng/test-routes.json`](../eng/test-routes.json). Invoke them through [`eng/test-route.ps1`](../eng/test-route.ps1); do not copy raw filters into prompts or policy documents.

## Validation Tiers

### Tier F — Focused

Run the smallest leaf route that owns the changed responsibility. Use it while implementing and immediately after a local correction.

### Tier A — Area

Run one bounded composite for the affected area after the change is coherent, before relevant UI smoke, or when neighboring owners were integrated. Composite children run sequentially in independent `dotnet test` processes and stop on the first failure or timeout.

### Tier M — Milestone

Run `Full` for a meaningful repository milestone, CI, transition to another major wave, or an explicit user request. `Full` requires `-AllowMilestone`; it is never hidden inside an ordinary composite. The suite is currently fast enough that an additional final milestone run is reasonable, but it is not required after every small edit.

## Named Routes

Discover routes with:

```powershell
pwsh eng/test-route.ps1 list
pwsh eng/test-route.ps1 resolve Desktop-UI
```

Run a focused or area route:

```powershell
pwsh eng/test-route.ps1 run Geometry
pwsh eng/test-route.ps1 run Interpretation
pwsh eng/test-route.ps1 run Repository-Tooling
pwsh eng/test-route.ps1 run Project-Stats
pwsh eng/test-route.ps1 run Agent-Context
pwsh eng/test-route.ps1 run Repository-Verification -NoBuild
```

Run the milestone suite only deliberately:

```powershell
pwsh eng/test-route.ps1 run Full -NoBuild -AllowMilestone
```

The registry is responsibility-oriented, not stage-oriented. A leaf route must remain bounded and non-overlapping with its peers. A composite is an ordered plan, not a parallel scheduler.

`Interpretation` owns the focused `NoxAeterna.Tests.Interpretation` namespace for pack identities, schema contracts, JSON, canonical keys, and pure validation. `Repository-Tooling` owns `NoxAeterna.Tests.Tooling.Interpretation` for explicit-root filesystem validation, generated indexes/check mode, and authoring reports in addition to its existing tooling scope. `Project-Stats` and `Agent-Context` own separate non-overlapping namespaces. `Repository-Verification` executes architecture boundaries, repository tooling, Project Stats, and Agent Context in that order.

Tooling CLI examples use an explicit synthetic or future pack root:

```powershell
dotnet run --project NoxAeterna.Tools.Repository -- interpretation-pack validate --pack-root <path>
dotnet run --project NoxAeterna.Tools.Repository -- interpretation-pack generate-indexes --pack-root <path> --check
dotnet run --project NoxAeterna.Tools.Repository -- interpretation-pack authoring-status --working-root <path>
```

## Build and `-NoBuild`

Omit `-NoBuild` unless the requested configuration was successfully built after the latest relevant source or project-file change. CI restores once, builds Release with `--no-restore`, and then runs the Full route with `-Configuration Release -NoBuild`.

The runner accepts only registered names and structured options. It does not accept raw filters or arbitrary commands.

## Timeout and Process Ownership

Each leaf owns a positive bounded timeout in the registry. On timeout, the runner terminates only the process tree it started. It does not scan for or stop unrelated `dotnet`, Rider, ReSharper, MSBuild server, or design-time processes. There is no blind retry.

Route logs and TRX files are written below the ignored `TestResults/RepoRoutes/` scope. Results report the requested route, resolved leaves, durations, timeouts, exit codes, and repository-relative artifact paths.

## Evidence Boundaries

These evidence classes complement rather than replace each other:

- unit and contract tests prove deterministic code and repository contracts;
- cross-platform CI proves those checks execute on the three desktop target OS families;
- coverage shows which production paths executed, but does not prove correctness;
- manual real-control UI smoke proves interaction and visual states in a running Avalonia application;
- screenshot review records visual evidence but is not numerical proof.

Passing tests do not prove popup, focus, hover, DPI, resizing, theme-switch, or perceived visual quality. Manual smoke does not replace deterministic tests. Golden numerical tests do not replace visual review.

## Completion Criteria

Do not use a historical fixed test count as an acceptance condition. A route is successful when its process exits successfully, does not time out, completes the expected registered plan, and reports no runner error. The current discovered tests are the evidence for that run.

## Coverage

Coverage is a diagnostic milestone report. Run:

```powershell
pwsh eng/coverage.ps1
```

The script produces TRX and Cobertura artifacts under a unique ignored directory and never deletes previous results. T1-B defines no percentage gate. Coverage is not a correctness proof and must not encourage low-value tests written only to increase a number.

## CI and Manual UI Smoke

CI runs documentation validation, the Full registered route across Windows/Linux/macOS, and one diagnostic coverage job. It does not launch the application or execute the manual UI smoke catalog. See [`UI-SMOKE.md`](UI-SMOKE.md) for the real-control workflow.
