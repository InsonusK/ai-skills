// Source: skill vp-postgres-in-dotnet

using Ardalis.Specification;

namespace Shared.Persistence;

/// <summary>
/// The entities of one table, to read and to write: Ardalis's contract. A
/// handler takes it, picks entities by a specification, calls their methods,
/// and saves them. Each write saves before it returns, inside the unit of
/// work's transaction when one is begun.
/// </summary>
public interface IRepository<TEntity> : IRepositoryBase<TEntity> where TEntity : class
{
}

/// <summary>The entities of one table, to read only: what a handler that changes nothing takes.</summary>
public interface IReadRepository<TEntity> : IReadRepositoryBase<TEntity> where TEntity : class
{
}
