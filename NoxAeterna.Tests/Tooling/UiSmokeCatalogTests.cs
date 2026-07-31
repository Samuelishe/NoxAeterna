using System.Text.Json;

namespace NoxAeterna.Tests.Tooling;

public sealed class UiSmokeCatalogTests
{
    private static string CatalogPath => Path.Combine(
        ToolingTestSupport.RepositoryRoot,
        "eng",
        "ui-smoke-cases.json");

    [Fact]
    public void CatalogHasSupportedSchemaUniqueCasesAndKnownDimensions()
    {
        using var document = JsonDocument.Parse(File.ReadAllText(CatalogPath));
        var root = document.RootElement;
        Assert.Equal(1, root.GetProperty("schemaVersion").GetInt32());
        var cases = root.GetProperty("cases").EnumerateArray().ToArray();
        Assert.NotEmpty(cases);
        Assert.Equal(
            cases.Length,
            cases.Select(item => item.GetProperty("id").GetString())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count());

        foreach (var item in cases)
        {
            Assert.All(
                item.GetProperty("themes").EnumerateArray(),
                value => Assert.Contains(value.GetString(), new[] { "dark", "light" }));
            Assert.All(
                item.GetProperty("languages").EnumerateArray(),
                value => Assert.Contains(value.GetString(), new[] { "ru", "en" }));
            Assert.NotEmpty(item.GetProperty("actions").EnumerateArray());
            Assert.NotEmpty(item.GetProperty("expected").EnumerateArray());
        }
    }

    [Fact]
    public void ScreenshotsAreRelativeTemporaryEvidence()
    {
        using var document = JsonDocument.Parse(File.ReadAllText(CatalogPath));

        foreach (var item in document.RootElement.GetProperty("cases").EnumerateArray())
        {
            var fileName = item.GetProperty("screenshotFileName").GetString()!;
            Assert.False(Path.IsPathRooted(fileName));
            Assert.Equal(Path.GetFileName(fileName), fileName);
            Assert.DoesNotMatch(@"\(\d+\)", fileName);
            Assert.False(item.GetProperty("trackScreenshot").GetBoolean());
        }
    }

    [Fact]
    public void PragueFixtureAndOwnerReferencesArePresent()
    {
        var source = File.ReadAllText(CatalogPath);

        Assert.Contains("\"date\": \"1990-07-14\"", source, StringComparison.Ordinal);
        Assert.Contains("\"time\": \"13:45\"", source, StringComparison.Ordinal);
        Assert.Contains("\"latitude\": 50.0755", source, StringComparison.Ordinal);
        Assert.Contains("\"longitude\": 14.4378", source, StringComparison.Ordinal);
        Assert.Contains("\"timezone\": \"Europe/Prague\"", source, StringComparison.Ordinal);
        Assert.Contains("Golden numerical evidence remains owned by the Prague fixture tests.", source, StringComparison.Ordinal);
        Assert.True(File.Exists(Path.Combine(ToolingTestSupport.RepositoryRoot, "docs", "UI-SMOKE.md")));
        Assert.True(File.Exists(Path.Combine(ToolingTestSupport.RepositoryRoot, "docs", "ASTRONOMY-ENGINE.md")));
    }
}
