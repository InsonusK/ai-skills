using Shared.Concurrency;

namespace App.Infrastructure.Concurrency;

public sealed class EntityVersionResolverFactory(IServiceProvider services) : IEntityVersionResolverFactory
{
    private readonly Dictionary<string, IEntityVersionResolver> _resolvers =
        services.GetServices<IEntityVersionResolver>().ToDictionary(r => r.VersionedEntityName);

    public IEntityVersionResolver GetFor(string entityName) =>
        _resolvers.TryGetValue(entityName, out var resolver)
            ? resolver
            : throw new InvalidOperationException($"No IEntityVersionResolver registered for '{entityName}'.");
}
