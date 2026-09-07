using Ardalis.Result;
using MediatR;
using Shared.Concurrency;

namespace BuildingBlocks.MediatR;

// Registered after ValidationBehavior. Validates every version an IHasVersions command
// carries; short-circuits with Conflict / NotFound / Error before the handler runs.
public sealed class ConcurrencyBehavior<TRequest, TResponse>(IEntityVersionResolverFactory factory)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IHasVersions
    where TResponse : IResult
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        foreach (var (entityName, idVersions) in request.Versions)
        {
            var resolver = factory.GetFor(entityName);
            if (resolver is null)
                return Error($"Unknown entity: '{entityName}'.");

            foreach (var (id, expectedVersion) in idVersions)
            {
                var actual = await resolver.GetCurrentVersionForAsync(id, ct);
                if (actual == 0)
                    return NoArg(nameof(Result.NotFound));
                if ((uint)actual != expectedVersion)
                    return Params(nameof(Result.Conflict),
                        $"'{entityName}' {id} was modified by another user. Expected {expectedVersion}, found {actual}.");
            }
        }

        return await next();
    }

    // TResponse is Result or Result<T>; both expose their own static factory that returns the
    // right closed type. A plain (TResponse)Result.X(...) cast throws at runtime for Result<T>.
    private static TResponse Error(string message)
        => (TResponse)typeof(TResponse).GetMethod(nameof(Result.Error), [typeof(string)])!.Invoke(null, [message])!;

    private static TResponse NoArg(string method)
        => (TResponse)typeof(TResponse).GetMethod(method, Type.EmptyTypes)!.Invoke(null, [])!;

    private static TResponse Params(string method, string message)
        => (TResponse)typeof(TResponse).GetMethod(method, [typeof(string[])])!.Invoke(null, [new[] { message }])!;
}
