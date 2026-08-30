using App.Infrastructure.Persistence;
using Shared.UnitOfWork;

namespace App.Infrastructure.UnitOfWork;

public sealed class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken ct) => context.SaveChangesAsync(ct);
}
