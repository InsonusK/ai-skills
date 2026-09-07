using App.Infrastructure.Persistence;
using Ardalis.Specification.EntityFrameworkCore;
using Shared.Repositories;

namespace App.Infrastructure.Repositories;

// One generic repository for every entity. Ardalis RepositoryBase runs the spec and
// applies AsNoTracking for reads. Never calls SaveChangesAsync.
public sealed class Repository<T>(AppDbContext dbContext) : RepositoryBase<T>(dbContext), IRepository<T>
    where T : class;
