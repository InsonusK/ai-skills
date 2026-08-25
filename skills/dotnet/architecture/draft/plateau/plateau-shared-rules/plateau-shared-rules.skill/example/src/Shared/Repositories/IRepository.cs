using Ardalis.Specification;

namespace Shared.Repositories;

public interface IReadRepository<T> : IReadRepositoryBase<T> where T : class { }

public interface IRepository<T> : IRepositoryBase<T>, IReadRepository<T> where T : class { }
