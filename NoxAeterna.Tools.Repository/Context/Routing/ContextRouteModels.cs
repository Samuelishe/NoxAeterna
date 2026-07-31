namespace NoxAeterna.Tools.Repository.Context.Routing;

public sealed record ContextDocumentReference
{
    public string Path { get; init; } = string.Empty;

    public int Priority { get; init; }
}

public sealed record ContextTaskKind
{
    public string Name { get; init; } = string.Empty;

    public IReadOnlyList<ContextDocumentReference> MandatoryDocuments { get; init; } = [];

    public IReadOnlyList<ContextDocumentReference> RecommendedDocuments { get; init; } = [];

    public int MaxSelectedFiles { get; init; }
}

public sealed record ContextPathRule
{
    public string Name { get; init; } = string.Empty;

    public int Priority { get; init; }

    public IReadOnlyList<string> Patterns { get; init; } = [];

    public IReadOnlyList<string> TaskKinds { get; init; } = [];

    public IReadOnlyList<string> Documents { get; init; } = [];

    public IReadOnlyList<string> TestRoutes { get; init; } = [];
}

public sealed record ContextRouteRegistry
{
    public int SchemaVersion { get; init; }

    public IReadOnlyList<ContextTaskKind> TaskKinds { get; init; } = [];

    public IReadOnlyList<ContextPathRule> PathRules { get; init; } = [];
}

public sealed class ContextRegistryException(string message) : Exception(message);
