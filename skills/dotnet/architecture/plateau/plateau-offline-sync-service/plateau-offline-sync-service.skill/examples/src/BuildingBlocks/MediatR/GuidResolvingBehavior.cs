using MediatR;
using Shared.Guid;

namespace BuildingBlocks.MediatR;

// Registered after ConcurrencyBehavior, before UnitOfWorkBehavior. For an IHasGuid create
// command, ask the entity's resolver whether the Guid already exists; if so, return the
// resolver's ConflictResult and skip the handler (idempotent create).
public sealed class GuidResolvingBehavior<TRequest, TResponse>(IGuidResolver<TResponse> resolver)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IHasGuid
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var existing = await resolver.ResolveAsync(request.Guid, ct);
        return existing is not null ? existing : await next();
    }
}
