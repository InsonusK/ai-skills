using Ardalis.Specification;

namespace Shared.Repositories;

public interface IRepository<T> : IRepositoryBase<T>, IReadRepository<T> where T : class;
