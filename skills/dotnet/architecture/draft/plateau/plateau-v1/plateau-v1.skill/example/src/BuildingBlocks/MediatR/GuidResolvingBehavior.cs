using MediatR;
using Shared.Guid;

namespace BuildingBlocks.MediatR;

public class GuidResolvingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IHasGuid, IRequest<TResponse>
{
    private readonly IGuidResolver<TResponse> _resolver;

    public GuidResolvingBehavior(IGuidResolver<TResponse> resolver) => _resolver = resolver;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var existing = await _resolver.ResolveAsync(request.Guid, ct);
        return existing is not null ? existing : await next();
    }
}
