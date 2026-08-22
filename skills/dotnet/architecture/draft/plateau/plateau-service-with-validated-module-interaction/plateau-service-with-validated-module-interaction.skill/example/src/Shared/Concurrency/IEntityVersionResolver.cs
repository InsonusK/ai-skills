namespace Shared.Concurrency;

public interface IEntityVersionResolver
{
    uint ResolveVersion<TEntity>(TEntity entity) where TEntity : class;
}
