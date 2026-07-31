namespace NoxAeterna.Tests.AgentContext;

public sealed class ContextToolingContractTests
{
    [Fact]
    public void WrappersAreReadOnlyBoundedAndDoNotRestore()
    {
        foreach (var name in new[] { "context-plan.ps1", "context-eval.ps1" })
        {
            var text = File.ReadAllText(Path.Combine(AgentContextTestSupport.Root, "eng", name));
            Assert.Contains("--no-restore", text, StringComparison.Ordinal);
            Assert.Contains("-NoBuild", text, StringComparison.Ordinal);
            Assert.DoesNotContain("Invoke-Expression", text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("git reset", text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("Out-File", text, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void AgentContextLeafIsLastRepositoryVerificationChild()
    {
        var json = File.ReadAllText(Path.Combine(AgentContextTestSupport.Root, "eng", "test-routes.json"));
        using var document = System.Text.Json.JsonDocument.Parse(json);
        var routes = document.RootElement.GetProperty("routes").EnumerateArray().ToArray();
        var leaf = routes.Single(item => item.GetProperty("name").GetString() == "Agent-Context");
        var composite = routes.Single(item => item.GetProperty("name").GetString() == "Repository-Verification");

        Assert.Equal("leaf", leaf.GetProperty("kind").GetString());
        Assert.Contains("NoxAeterna.Tests.AgentContext", leaf.GetProperty("filter").GetString(), StringComparison.Ordinal);
        Assert.Equal("Agent-Context", composite.GetProperty("children").EnumerateArray().Last().GetString());
    }

    [Fact]
    public async Task PlanWrapperNoBuildFailsCleanlyWhenToolOutputIsMissingWithoutCreatingFiles()
    {
        var root = Path.Combine(Path.GetTempPath(), $"NoxAeterna-context-wrapper-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "pwsh",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            foreach (var argument in new[]
                     {
                         "-NoProfile", "-File", Path.Combine(AgentContextTestSupport.Root, "eng", "context-plan.ps1"),
                         "-Task", "CodeChange", "-Path", "Future.cs", "-BudgetChars", "1000", "-Root", root, "-NoBuild"
                     })
                startInfo.ArgumentList.Add(argument);

            using var process = System.Diagnostics.Process.Start(startInfo);
            Assert.NotNull(process);
            var output = process.StandardOutput.ReadToEndAsync();
            var error = process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(20));
            Assert.NotEqual(0, process.ExitCode);
            Assert.Contains("output is missing", await error, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(string.Empty, await output);
            Assert.Empty(Directory.EnumerateFileSystemEntries(root));
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }
}
