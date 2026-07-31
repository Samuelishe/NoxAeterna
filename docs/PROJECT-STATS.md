# Project Stats

| Metadata | Definition |
| --- | --- |
| Role | Operator and maintenance contract for factual repository diagnostics. |
| Read when | Measuring repository structure, reviewing Project Stats output, or changing its inventory/report behavior. |
| Authoritative for | Project Stats purpose, invocation, report semantics, exclusions, formats, interpretation rules, and maintenance procedure. |
| Not authoritative for | Documentation budgets, executable test policy, coverage thresholds, architecture rules, context routing, RAG, or current project status. |

## Purpose

Project Stats answers factual structural questions about the current public worktree: file and text volume, project references, test topology, large-file rankings, folder density, and the current read-only documentation-budget snapshot. It is on-demand diagnostics, not an analyzer or quality gate.

Size is a signal, not a verdict. A large source file is not automatically defective, an archive document is retained history rather than cleanup debt, lexical test counts do not prove test quality, and the test-to-production line ratio does not replace coverage or assertions.

## Commands

```powershell
dotnet run --project NoxAeterna.Tools.Repository -- stats .
dotnet run --project NoxAeterna.Tools.Repository -- stats . --top 25
dotnet run --project NoxAeterna.Tools.Repository -- stats . --json
dotnet run --project NoxAeterna.Tools.Repository -- stats . --markdown
dotnet run --project NoxAeterna.Tools.Repository -- stats . --json --output project-stats.json
dotnet run --project NoxAeterna.Tools.Repository -- stats . --markdown --output project-stats.md
```

The repository root is optional and defaults to the current Git repository. Console output is bounded by `--top`; JSON and Markdown expose the complete typed report prepared for that top value. JSON written to standard output contains JSON only.

Relative output paths must remain inside the repository. An explicit absolute path may be used for local operator output. Report content never embeds the absolute repository root, and the exact output target is excluded from its own inventory.

## Public Inventory Boundary

The tool obtains tracked files and public non-ignored untracked files from argument-based Git commands. It does not recursively scan parent directories, access the network, or fall back to arbitrary filesystem discovery when Git is unavailable.

The scanner never reads content below private, sensitive, local-ownership, IDE, generated, runtime, build, result, cache, artifact, or publish paths. Private/sensitive entries unexpectedly visible to Git produce a redacted diagnostic without exposing or reading the filename. Public unreadable files produce a controlled diagnostic while the remaining report continues.

Binary files contribute file count and byte size only. Supported public text formats include C#, AXAML/XAML, Markdown, JSON, PowerShell, YAML, MSBuild/project/solution files, and plain text. Character counts use .NET `string.Length` UTF-16 code units. An empty text file has zero lines; otherwise logical line count treats a terminal newline as a terminator rather than an extra empty line. LF and CRLF therefore produce the same logical line count.

## Classification and Density

Classification is deterministic and path-based:

- `NoxAeterna.Tests/**` is Tests;
- `NoxAeterna.Tools.Repository/**` and `eng/**` are Tooling;
- other `NoxAeterna.*` project files are Production;
- `docs/**`, root `README.md`, and root `AGENTS.md` are Documentation;
- `resources/**` and shipped asset extensions are Resources;
- `.github/**` is Workflow;
- remaining public files are Other.

This is inventory grouping, not semantic architecture analysis. Folder density groups a `NoxAeterna.*` project at its project root and other files by their first two meaningful path segments.

## Report Semantics

Schema version 1 contains:

- repository totals and extension/category summaries;
- public `.csproj` metadata and explicit project-reference edges;
- separate largest-file rankings for production, tests, tooling, markup, documentation, PowerShell, and configuration;
- structural folder density;
- lexical test topology by thematic folder;
- a read-only snapshot of `eng/document-budgets.json`;
- controlled diagnostics.

Project references come only from project XML. MSBuild `/` and `\` separators are interpreted lexically and independently of the host OS, then emitted as canonical repository-relative `/` paths. Raw `Include` values are never passed directly to host path-combination APIs. References that escape the repository or use unsupported MSBuild expressions produce controlled diagnostics rather than guessed paths. Property expansion, conditional item evaluation, imports, and a full MSBuild graph remain out of scope.

Missing references, self-references, cycles, and malformed projects are diagnostics; existing architecture tests remain the quality gate.

`[Fact]`, `[Theory]`, and test-class counts are lexical statistics. A theory may create multiple runtime cases, and executable evidence continues to belong to named routes in [`TEST-EXECUTION.md`](TEST-EXECUTION.md). Documentation budget values remain owned and validated by `eng/document-budgets.json` and `eng/doc-check.ps1`; Project Stats only displays their current snapshot.

## Output and Maintenance

Console output is a short operator summary. JSON is deterministic camelCase schema data. Markdown is a readable diagnostic report with repository-relative paths and no volatile timestamp. Generated `project-stats.md` and `project-stats.json` are ignored operational output, not project documentation.

When changing the tool:

1. Preserve the Git/public/privacy boundary before adding a metric.
2. Keep factual inventory reusable and independent of output writers.
3. Add focused `Project-Stats` route coverage for parser, privacy, ordering, graph, metric, or writer changes.
4. Run the CLI in console, JSON, and Markdown modes against the real repository.
5. Do not add automatic refactoring conclusions or CI artifact generation.

T2-B may reuse the factual `RepositoryFileEntry` inventory for context planning. It must add its own exact route, ranking, and budget policy rather than embedding that policy into Project Stats; T2-B is not implemented here.
