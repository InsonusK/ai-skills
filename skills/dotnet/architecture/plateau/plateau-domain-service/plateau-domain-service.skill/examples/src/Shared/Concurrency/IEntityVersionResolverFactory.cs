namespace Shared.Concurrency;

// Routes a stable entity name to its resolver. Returns null for an unknown name.
public interface IEntityVersionResolverFactory
{
    IEntityVersionResolver? GetFor(string entityName);
}
