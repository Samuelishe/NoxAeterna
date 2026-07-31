# Session Log

| Metadata | Definition |
| --- | --- |
| Role | Bounded current-wave chronology. |
| Read when | Recent provenance is explicitly needed. |
| Authoritative for | Recent session evidence and handoff chronology. |
| Not authoritative for | Current status, architecture, roadmap, or durable policy. |

Older retained evidence is indexed in [the documentation archive](archive/README.md). Use [PROJECT-STATE.md](PROJECT-STATE.md) for the current checkpoint.

## 2026-07-31: T1-B.2 Same-Day Session-Log Rollover

Summary:

- Extended the read-only documentation checker to recognize bounded partial-day session-log chunks while preserving the existing full-range archive contract.
- Added contiguous part numbering, exact normalized heading ownership, partial/full overlap, heading-date, archive-index, and typed JSON diagnostic validation.
- Moved the completed visual V1/V2 and repository T1-A/T1-B wave, including first hosted CI confirmation, intact into `SESSION-LOG_2026-07-31_part-01.md`.
- Kept the active log available for additional entries on the same calendar date without raising its hard budget.

Verification:

- `pwsh eng/doc-check.ps1`
- `pwsh eng/doc-check.ps1 -Json`
- `dotnet build NoxAeterna.sln -c Debug`
- `pwsh eng/test-route.ps1 run Repository-Verification -NoBuild`
