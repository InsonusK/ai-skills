using App.Infrastructure.Persistence;
using Shared.UnitOfWork;

namespace App.Infrastructure.UnitOfWork;

// The single place DbContext.SaveChangesAsync is called.
public sealed class UnitOfWork(AppDbContext dbContext) : IUnitOfWork
{
    public Task SaveChangesAsync(CancellationToken ct = default) => dbContext.SaveChangesAsync(ct);
}
