using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using NoxAeterna.Tools.Repository.Analysis;
using NoxAeterna.Tools.Repository.Stats;

namespace NoxAeterna.Tools.Repository.Cli;

public static class ProjectStatsWriters
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public static string WriteJson(ProjectStatsReport report) =>
        JsonSerializer.Serialize(report, JsonOptions) + Environment.NewLine;

    public static string WriteConsole(ProjectStatsReport report, int top)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Nox Aeterna Project Stats");
        builder.AppendLine($"Public files: {report.Repository.TotalPublicFiles}; bytes: {report.Repository.TotalBytes}; projects: {report.Repository.ProjectCount}");
        builder.AppendLine($"C#: {report.Repository.CSharp.Files} files / {report.Repository.CSharp.Lines} lines; AXAML/XAML: {report.Repository.Xaml.Files} / {report.Repository.Xaml.Lines}; Markdown: {report.Repository.Markdown.Files} / {report.Repository.Markdown.Lines}");
        builder.AppendLine($"Tests: {report.Tests.TestFileCount} files; classes {report.Tests.TestClassCount}; lexical Fact {report.Tests.LexicalFactCount}; Theory {report.Tests.LexicalTheoryCount}; test/production lines {report.Tests.TestToProductionCSharpLineRatio.ToString("0.000", CultureInfo.InvariantCulture)}");
        builder.AppendLine();
        builder.AppendLine("Projects:");
        foreach (var project in report.Projects)
        {
            builder.AppendLine($"- {project.Name}: {project.SourceFileCount} C# files / {project.SourceLines} lines; refs {project.ProjectReferences.Count}; packages {project.PackageReferenceCount}");
        }
        builder.AppendLine();
        builder.AppendLine($"Largest production C# (top {Math.Min(top, report.LargestFiles.ProductionCSharp.Count)}):");
        AppendRankings(builder, report.LargestFiles.ProductionCSharp);
        builder.AppendLine($"Largest test C# (top {Math.Min(top, report.LargestFiles.TestCSharp.Count)}):");
        AppendRankings(builder, report.LargestFiles.TestCSharp);
        builder.AppendLine($"Diagnostics: {report.Diagnostics.Count}");
        foreach (var diagnostic in report.Diagnostics.Take(top))
        {
            builder.AppendLine($"- {diagnostic.Severity}: {diagnostic.Code}{(diagnostic.Path is null ? string.Empty : $" [{diagnostic.Path}]")}: {diagnostic.Message}");
        }
        return builder.ToString();
    }

    public static string WriteMarkdown(ProjectStatsReport report)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# Project Stats");
        builder.AppendLine();
        builder.AppendLine("> Generated diagnostic report. Size rankings are signals, not quality verdicts.");
        builder.AppendLine();
        builder.AppendLine("## Repository Summary");
        builder.AppendLine();
        builder.AppendLine("| Public files | Bytes | Projects | C# lines | Test/production line ratio |");
        builder.AppendLine("| ---: | ---: | ---: | ---: | ---: |");
        builder.AppendLine($"| {report.Repository.TotalPublicFiles} | {report.Repository.TotalBytes} | {report.Repository.ProjectCount} | {report.Repository.CSharp.Lines} | {report.Tests.TestToProductionCSharpLineRatio.ToString("0.000", CultureInfo.InvariantCulture)} |");
        builder.AppendLine();
        builder.AppendLine("## Projects");
        builder.AppendLine();
        builder.AppendLine("| Project | Frameworks | Output | C# files | Lines | References | Packages |");
        builder.AppendLine("| --- | --- | --- | ---: | ---: | ---: | ---: |");
        foreach (var project in report.Projects)
        {
            builder.AppendLine($"| {Escape(project.Name)} | {Escape(string.Join(", ", project.TargetFrameworks))} | {Escape(project.OutputType)} | {project.SourceFileCount} | {project.SourceLines} | {project.ProjectReferences.Count} | {project.PackageReferenceCount} |");
        }
        AppendMarkdownRanking(builder, "Largest Production C#", report.LargestFiles.ProductionCSharp);
        AppendMarkdownRanking(builder, "Largest Test C#", report.LargestFiles.TestCSharp);
        AppendMarkdownRanking(builder, "Largest Tooling C#", report.LargestFiles.ToolingCSharp);
        AppendMarkdownRanking(builder, "Largest Markdown", report.LargestFiles.Markdown);
        builder.AppendLine();
        builder.AppendLine("## Test Topology");
        builder.AppendLine();
        builder.AppendLine($"- Test files: {report.Tests.TestFileCount}");
        builder.AppendLine($"- Test classes: {report.Tests.TestClassCount}");
        builder.AppendLine($"- Lexical `[Fact]`: {report.Tests.LexicalFactCount}");
        builder.AppendLine($"- Lexical `[Theory]`: {report.Tests.LexicalTheoryCount}");
        builder.AppendLine($"- Semantics: {report.Tests.CountSemantics}");
        builder.AppendLine();
        builder.AppendLine("## Documentation Budgets");
        builder.AppendLine();
        builder.AppendLine("| Path | Current | Soft | Hard | Status | Strategy |");
        builder.AppendLine("| --- | ---: | ---: | ---: | --- | --- |");
        foreach (var budget in report.DocumentationBudgets)
        {
            builder.AppendLine($"| {Escape(budget.Path)} | {budget.CurrentCharacters} | {budget.SoftThreshold} | {budget.HardThreshold} | {budget.Status} | {budget.OverflowStrategy} |");
        }
        builder.AppendLine();
        builder.AppendLine("## Diagnostics");
        builder.AppendLine();
        if (report.Diagnostics.Count == 0)
        {
            builder.AppendLine("No diagnostics.");
        }
        else
        {
            foreach (var diagnostic in report.Diagnostics)
            {
                builder.AppendLine($"- **{Escape(diagnostic.Severity)} / {Escape(diagnostic.Code)}**{(diagnostic.Path is null ? string.Empty : $" `{Escape(diagnostic.Path)}`")}: {Escape(diagnostic.Message)}");
            }
        }
        return builder.ToString();
    }

    private static void AppendRankings(StringBuilder builder, IEnumerable<RankedFile> rankings)
    {
        foreach (var file in rankings)
        {
            builder.AppendLine($"- {file.Path}: {file.Lines} lines / {file.Characters} chars{(file.RetainedHistory ? " [retained history]" : string.Empty)}");
        }
    }

    private static void AppendMarkdownRanking(StringBuilder builder, string title, IReadOnlyList<RankedFile> rankings)
    {
        builder.AppendLine();
        builder.AppendLine($"## {title}");
        builder.AppendLine();
        builder.AppendLine("| Path | Lines | Characters | Category | Note |");
        builder.AppendLine("| --- | ---: | ---: | --- | --- |");
        foreach (var file in rankings)
        {
            builder.AppendLine($"| {Escape(file.Path)} | {file.Lines} | {file.Characters} | {file.Category} | {(file.RetainedHistory ? "retained history" : string.Empty)} |");
        }
    }

    private static string Escape(string value) => value.Replace("|", "\\|", StringComparison.Ordinal);
}
