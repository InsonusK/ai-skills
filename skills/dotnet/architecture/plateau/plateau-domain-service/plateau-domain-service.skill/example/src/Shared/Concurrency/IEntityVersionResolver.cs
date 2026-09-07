namespace Shared.Concurrency;

// Reads the current version for one versioned entity. Returns 0 when absent.
public interface IEntityVersionResolver
{
    Task<int> GetCurrentVersionForAsync(int id, CancellationToken ct = default);
}
