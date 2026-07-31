using System.Text.Json;

namespace NoxAeterna.Tests.Tooling;

public sealed class DocumentationCheckScriptTests
{
    private static string ScriptPath => Path.Combine(
        ToolingTestSupport.RepositoryRoot,
        "eng",
        "doc-check.ps1");

    [Fact]
    public void ValidFixtureExitsZero()
    {
        using var fixture = DocumentationFixture.Create();

        var result = fixture.Run();

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("0 error(s)", result.StandardOutput, StringComparison.Ordinal);
    }

    [Fact]
    public void SoftOverflowWarnsWithoutFailure()
    {
        using var fixture = DocumentationFixture.Create();
        fixture.SetBudget(hardLimit: fixture.BudgetLength + 20, warningRatio: 0.5d);

        var result = fixture.Run();

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("WARNING", result.StandardOutput, StringComparison.Ordinal);
    }

    [Fact]
    public void HardOverflowFails()
    {
        using var fixture = DocumentationFixture.Create();
        fixture.SetBudget(hardLimit: fixture.BudgetLength - 1);

        var result = fixture.Run();

        Assert.NotEqual(0, result.ExitCode);
        Assert.Contains("exceed hard limit", result.CombinedOutput, StringComparison.Ordinal);
    }

    [Fact]
    public void InvalidManifestFails()
    {
        using var fixture = DocumentationFixture.Create();
        File.WriteAllText(fixture.ManifestPath, "{ invalid");

        var result = fixture.Run();

        Assert.NotEqual(0, result.ExitCode);
        Assert.Contains("ERROR", result.CombinedOutput, StringComparison.Ordinal);
        Assert.Contains("eng/document-budgets.json", result.CombinedOutput, StringComparison.Ordinal);
    }

    [Fact]
    public void BrokenRelativeMarkdownLinkFails()
    {
        using var fixture = DocumentationFixture.Create();
        File.AppendAllText(
            Path.Combine(fixture.Root, "docs", "INDEX.md"),
            $"{Environment.NewLine}[Missing](missing.md){Environment.NewLine}");

        var result = fixture.Run();

        Assert.NotEqual(0, result.ExitCode);
        Assert.Contains("Missing local target", result.CombinedOutput, StringComparison.Ordinal);
    }

    [Fact]
    public void UnknownOverflowStrategyFails()
    {
        using var fixture = DocumentationFixture.Create();
        fixture.SetBudget(overflowStrategy: "invented-strategy");

        var result = fixture.Run();

        Assert.NotEqual(0, result.ExitCode);
        Assert.Contains("Unknown overflow strategy", result.CombinedOutput, StringComparison.Ordinal);
    }

    [Fact]
    public void MissingArchiveDestinationFails()
    {
        using var fixture = DocumentationFixture.Create();
        fixture.SetBudget(overflowStrategy: "rollover-archive");

        var result = fixture.Run();

        Assert.NotEqual(0, result.ExitCode);
        Assert.Contains("requires archiveDestination", result.CombinedOutput, StringComparison.Ordinal);
    }

    [Fact]
    public void OverlappingArchiveRangesFail()
    {
        using var fixture = DocumentationFixture.Create();
        fixture.AddArchiveChunk("SESSION-LOG_2026-01-15_to_2026-02-05.md");

        var result = fixture.Run();

        Assert.NotEqual(0, result.ExitCode);
        Assert.Contains("overlap", result.CombinedOutput, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void InvalidRouteRegistryFails()
    {
        using var fixture = DocumentationFixture.Create();
        File.WriteAllText(
            Path.Combine(fixture.Root, "eng", "test-routes.json"),
            """{"schemaVersion":1,"routes":[]}""");

        var result = fixture.Run();

        Assert.NotEqual(0, result.ExitCode);
        Assert.Contains("test route registry", result.CombinedOutput, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void InvalidUiSmokeCatalogFails()
    {
        using var fixture = DocumentationFixture.Create();
        var catalogPath = Path.Combine(fixture.Root, "eng", "ui-smoke-cases.json");
        File.WriteAllText(
            catalogPath,
            File.ReadAllText(catalogPath).Replace(
                "\"trackScreenshot\": false",
                "\"trackScreenshot\": true",
                StringComparison.Ordinal));

        var result = fixture.Run();

        Assert.NotEqual(0, result.ExitCode);
        Assert.Contains("trackScreenshot", result.CombinedOutput, StringComparison.Ordinal);
    }

    [Fact]
    public void DuplicateUiSmokeScreenshotWithinEvidenceSetFails()
    {
        using var fixture = DocumentationFixture.Create();
        File.WriteAllText(
            Path.Combine(fixture.Root, "eng", "ui-smoke-cases.json"),
            """
            {
              "schemaVersion": 1,
              "cases": [
                {
                  "id": "first",
                  "title": "First",
                  "area": "fixture",
                  "requiredFor": ["milestone"],
                  "preconditions": ["Ready."],
                  "input": {},
                  "actions": ["Act."],
                  "expected": ["Observe."],
                  "themes": ["dark"],
                  "languages": ["en"],
                  "windowModes": ["standard"],
                  "screenshotFileName": "duplicate.png",
                  "trackScreenshot": false
                },
                {
                  "id": "second",
                  "title": "Second",
                  "area": "fixture",
                  "requiredFor": ["milestone"],
                  "preconditions": ["Ready."],
                  "input": {},
                  "actions": ["Act."],
                  "expected": ["Observe."],
                  "themes": ["light"],
                  "languages": ["ru"],
                  "windowModes": ["maximized"],
                  "screenshotFileName": "duplicate.png",
                  "trackScreenshot": false
                }
              ]
            }
            """);

        var result = fixture.Run();

        Assert.NotEqual(0, result.ExitCode);
        Assert.Contains("duplicated in evidence set", result.CombinedOutput, StringComparison.Ordinal);
    }

    [Fact]
    public void JsonOutputIsParseableAndDeterministic()
    {
        using var fixture = DocumentationFixture.Create();

        var first = fixture.Run("-Json");
        var second = fixture.Run("-Json");

        Assert.Equal(0, first.ExitCode);
        Assert.Equal(first.StandardOutput, second.StandardOutput);
        using var document = JsonDocument.Parse(first.StandardOutput);
        Assert.Equal(1, document.RootElement.GetProperty("schemaVersion").GetInt32());
        Assert.Equal("ok", document.RootElement.GetProperty("result").GetString());
        Assert.Equal(
            "docs/BUDGET.md",
            document.RootElement
                .GetProperty("measuredDocuments")[0]
                .GetProperty("path")
                .GetString());
    }

    [Fact]
    public void ScriptContainsNoMutationOrAutoFixCommands()
    {
        var source = File.ReadAllText(ScriptPath);

        Assert.DoesNotContain("Set-Content", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Add-Content", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Remove-Item", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Move-Item", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("git reset", source, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class DocumentationFixture : IDisposable
    {
        private const string Metadata = """
            | Metadata | Definition |
            | --- | --- |
            | Role | Fixture. |
            | Read when | Testing. |
            | Authoritative for | Fixture behavior. |
            | Not authoritative for | Product behavior. |
            """;

        private DocumentationFixture(string root)
        {
            Root = root;
        }

        public string Root { get; }

        public string ManifestPath => Path.Combine(Root, "eng", "document-budgets.json");

        public int BudgetLength => File.ReadAllText(Path.Combine(Root, "docs", "BUDGET.md")).Length;

        public static DocumentationFixture Create()
        {
            var root = Path.Combine(
                Path.GetTempPath(),
                $"NoxAeterna-doc-check-{Guid.NewGuid():N}");
            Directory.CreateDirectory(Path.Combine(root, "eng"));
            Directory.CreateDirectory(Path.Combine(root, "docs", "archive", "session-log"));

            Write(root, "AGENTS.md", $"# Agents{Environment.NewLine}{Environment.NewLine}{Metadata}");
            Write(root, "docs/AGENTS.md", $"# Extended Guide{Environment.NewLine}{Environment.NewLine}{Metadata}");
            Write(root, "docs/TEST-EXECUTION.md", $"# Test Execution{Environment.NewLine}{Environment.NewLine}{Metadata}");
            Write(root, "docs/UI-SMOKE.md", $"# UI Smoke{Environment.NewLine}{Environment.NewLine}{Metadata}");
            Write(
                root,
                "docs/PROJECT-STATE.md",
                $"""
                 # Project State

                 {Metadata}

                 ## Current Checkpoint
                 Fixture.

                 ## Current Focus
                 Fixture.

                 ## Preserved Contracts
                 Fixture.

                 ## Active Blockers
                 None.
                 """);
            Write(root, "docs/DOCUMENTATION-GOVERNANCE.md", $"# Governance{Environment.NewLine}{Environment.NewLine}{Metadata}");
            Write(root, "docs/INDEX.md", "# Index");
            Write(
                root,
                "docs/SESSION-LOG.md",
                "# Session Log" + Environment.NewLine + Environment.NewLine + "## 2026-02-10: Active");
            Write(
                root,
                "docs/archive/README.md",
                $"""
                 # Archive

                 {Metadata}

                 [January](session-log/SESSION-LOG_2026-01-01_to_2026-01-31.md)
                 """);
            Write(
                root,
                "docs/archive/session-log/SESSION-LOG_2026-01-01_to_2026-01-31.md",
                "# Archived Session Log");
            Write(root, "docs/BUDGET.md", new string('x', 120));
            Write(
                root,
                "NoxAeterna.Tests/NoxAeterna.Tests.csproj",
                """
                <Project Sdk="Microsoft.NET.Sdk">
                  <ItemGroup>
                    <PackageReference Include="coverlet.collector" Version="10.0.1" />
                  </ItemGroup>
                </Project>
                """);
            Write(
                root,
                ".github/workflows/ci.yml",
                "name: Fixture CI");
            File.Copy(
                Path.Combine(ToolingTestSupport.RepositoryRoot, "eng", "RepoVerification.psm1"),
                Path.Combine(root, "eng", "RepoVerification.psm1"));
            Write(
                root,
                "eng/test-routes.json",
                """
                {
                  "schemaVersion": 1,
                  "unsupportedFilters": [],
                  "routes": [
                    {
                      "name": "Fixture",
                      "description": "Fixture route.",
                      "kind": "leaf",
                      "category": "fixture",
                      "tags": ["offline"],
                      "testProject": "NoxAeterna.Tests/NoxAeterna.Tests.csproj",
                      "filter": "FullyQualifiedName~Fixture",
                      "defaultTimeoutSeconds": 30,
                      "hardwareEvidence": false,
                      "milestoneOnly": false
                    }
                  ]
                }
                """);
            Write(
                root,
                "eng/ui-smoke-cases.json",
                """
                {
                  "schemaVersion": 1,
                  "cases": [
                    {
                      "id": "fixture",
                      "title": "Fixture",
                      "area": "fixture",
                      "requiredFor": ["fixture"],
                      "preconditions": ["Ready."],
                      "input": {},
                      "actions": ["Act."],
                      "expected": ["Observe."],
                      "themes": ["dark"],
                      "languages": ["en"],
                      "windowModes": ["standard"],
                      "screenshotFileName": "fixture.png",
                      "trackScreenshot": false
                    }
                  ]
                }
                """);

            var fixture = new DocumentationFixture(root);
            fixture.SetBudget();
            return fixture;
        }

        public void SetBudget(
            int? hardLimit = null,
            double warningRatio = 0.85d,
            string overflowStrategy = "manual-reconcile",
            string? archiveDestination = null)
        {
            var entry = new Dictionary<string, object?>
            {
                ["path"] = "docs/BUDGET.md",
                ["hardLimit"] = hardLimit ?? 1000,
                ["overflowStrategy"] = overflowStrategy
            };
            if (archiveDestination is not null)
            {
                entry["archiveDestination"] = archiveDestination;
            }

            var manifest = new Dictionary<string, object?>
            {
                ["schemaVersion"] = 1,
                ["warningRatio"] = warningRatio,
                ["documents"] = new[] { entry }
            };
            File.WriteAllText(
                ManifestPath,
                JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true }));
        }

        public void AddArchiveChunk(string fileName)
        {
            Write(Root, $"docs/archive/session-log/{fileName}", "# Archive");
            File.AppendAllText(
                Path.Combine(Root, "docs", "archive", "README.md"),
                $"{Environment.NewLine}[Overlap](session-log/{fileName}){Environment.NewLine}");
        }

        public ScriptResult Run(params string[] arguments)
        {
            var allArguments = new List<string> { "-Root", Root };
            allArguments.AddRange(arguments);
            return ToolingTestSupport.RunPowerShell(
                ScriptPath,
                ToolingTestSupport.RepositoryRoot,
                allArguments.ToArray());
        }

        public void Dispose()
        {
            var expectedPrefix = Path.Combine(Path.GetTempPath(), "NoxAeterna-doc-check-");
            if (Root.StartsWith(expectedPrefix, StringComparison.OrdinalIgnoreCase) &&
                Directory.Exists(Root))
            {
                Directory.Delete(Root, recursive: true);
            }
        }

        private static void Write(string root, string relativePath, string content)
        {
            var path = Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, content);
        }
    }
}
