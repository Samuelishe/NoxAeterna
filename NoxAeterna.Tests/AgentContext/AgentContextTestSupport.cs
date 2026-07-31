using NoxAeterna.Tools.Repository.Analysis;
using NoxAeterna.Tools.Repository.Context.Routing;

namespace NoxAeterna.Tests.AgentContext;

internal static class AgentContextTestSupport
{
    public static string Root { get; } = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));

    public static RepositoryInventory Inventory() => new GitRepositoryInventory().Discover(Root);

    public static ContextRouteRegistry Registry()
    {
        var inventory = Inventory();
        return ContextRegistryLoader.Load(Root, "eng/context-routes.json", inventory.Files);
    }
}
