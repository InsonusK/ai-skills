using Ardalis.Specification;

namespace Shared.Persistence;

/// <summary>
/// The stored roots of one aggregate, to read and to write: Ardalis's contract,
/// kept to aggregate roots. A handler takes it, loads a root by a specification,
/// calls the root's methods, and saves it. Each write saves before it returns.
/// The store behind it is the composition root's choice: in memory, or a
/// database, with no change to the handler.
/// </summary>
public interface IRepository<TRoot> : IRepositoryBase<TRoot> where TRoot : class, IAggregateRoot
{
}

/// <summary>The stored roots of one aggregate, to read only: what a handler that changes nothing takes.</summary>
public interface IReadRepository<TRoot> : IReadRepositoryBase<TRoot> where TRoot : class, IAggregateRoot
{
}
