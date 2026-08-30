using App.Infrastructure.Persistence;
using Ardalis.Specification.EntityFrameworkCore;
using Shared.Repositories;

namespace App.Infrastructure.Repositories;

public sealed class Repository<T>(AppDbContext context)
    : RepositoryBase<T>(context), IRepository<T> where T : class
{
}
