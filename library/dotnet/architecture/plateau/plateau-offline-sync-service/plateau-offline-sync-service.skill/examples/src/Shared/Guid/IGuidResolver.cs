namespace Shared.Guid;

// Per-entity: has this Guid been created before? null = no (first request), non-null = the
// existing response, marked as a conflict.
public interface IGuidResolver<TResponse>
{
    Task<TResponse?> ResolveAsync(System.Guid guid, CancellationToken ct);
}
