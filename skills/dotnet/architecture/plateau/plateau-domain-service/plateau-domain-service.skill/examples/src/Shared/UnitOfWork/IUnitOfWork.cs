namespace Shared.UnitOfWork;

// The single commit point. Only the UnitOfWork implementation calls SaveChangesAsync.
public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken ct = default);
}
