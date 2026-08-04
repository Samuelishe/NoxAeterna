# Repository-Owned Codex Skills

This directory is the canonical source for Codex skills owned by the repository. The matching user-level copy is only the installed runtime instance and must not be edited independently.

After `git pull` on a work or home computer, install and verify the repository-owned skills:

```powershell
pwsh eng/sync-codex-skills.ps1 -Install
pwsh eng/sync-codex-skills.ps1 -Check
```

Skills do not synchronize between computers automatically. Make changes in `eng/codex-skills/<skill-name>`, then run `-Install` again to update the user-level copy. By default, the destination is `<CODEX_HOME>/skills` when `CODEX_HOME` is set and non-empty; otherwise it is `<USERPROFILE>/.codex/skills`.

Installing a skill does not require a new Rider/Codex chat. A Codex turn already in progress does not receive newly discovered skill context while it is running. After that task finishes, send the next prompt in the same Rider chat; if needed, Codex can read the tracked `SKILL.md` directly from disk.
