namespace Shared.Guid;

public interface IHasGuid
{
    System.Guid Guid { get; }
}

public interface IGuidResolver<TResponse>
{
    Task<TResponse?> ResolveAsync(System.Guid guid, CancellationToken ct);
}
