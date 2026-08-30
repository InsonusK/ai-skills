namespace Shared.Concurrency;

public interface IVersioned
{
    uint Version { get; }
}

public interface IHasVersions
{
    IReadOnlyDictionary<string, uint> Versions { get; }
    IReadOnlyDictionary<string, int> EntityIds { get; }
}

public interface IEntityVersionResolver
{
    string VersionedEntityName { get; }
    Task<uint?> GetCurrentVersionForAsync(int id, CancellationToken ct);
}

public interface IEntityVersionResolverFactory
{
    IEntityVersionResolver GetFor(string entityName);
}
