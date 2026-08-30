using Ardalis.Result;
using MediatR;
using Shared.Concurrency;

namespace BuildingBlocks.MediatR;

public class ConcurrencyBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IHasVersions, IRequest<TResponse>
    where TResponse : IResult
{
    private readonly IEntityVersionResolverFactory _factory;

    public ConcurrencyBehavior(IEntityVersionResolverFactory factory) => _factory = factory;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        foreach (var (entityName, expected) in request.Versions)
        {
            if (!request.EntityIds.TryGetValue(entityName, out var entityId))
                throw new InvalidOperationException($"No entity id provided for '{entityName}'.");

            var resolver = _factory.GetFor(entityName);
            var current = await resolver.GetCurrentVersionForAsync(entityId, ct);

            if (current != expected)
                return (TResponse)Result.Conflict($"{entityName} was modified by another request.");
        }

        return await next();
    }
}
