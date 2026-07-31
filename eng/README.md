# Repository Checks

| Metadata | Definition |
| --- | --- |
| Role | Operator guide for implemented repository checks. |
| Read when | Running the T1-A repository baseline or documentation validation. |
| Authoritative for | Available T1-A commands and their execution contract. |
| Not authoritative for | Product status, documentation budgets, test routes, or future tooling behavior. |

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

The documentation check validates the machine-readable budget manifest, required owners and metadata, relative Markdown links, archive ranges, and current-state headings. Warnings do not fail; errors do.

Both scripts are read-only. They do not mutate Git, edit documentation, run the application or tests, or read AppData. Generated console or JSON output is operational evidence, not product documentation.

## Planned, Not Implemented

- Named tests, UI smoke routes, coverage, and CI belong to T1-B.
- Project Stats and context planning belong to T2.

