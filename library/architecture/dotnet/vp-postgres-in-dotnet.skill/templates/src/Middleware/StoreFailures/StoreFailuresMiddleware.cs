// Source: skill vp-postgres-in-dotnet

using MediatR;
using Shared.Persistence;

namespace Middleware.StoreFailures;

/// <summary>
/// The command middleware that tells a failure of the store in the error kinds:
/// a key already taken is a conflict, a row changed meanwhile a concurrent
/// change, a database that does not answer unavailable (<see cref="IStoreFailures"/>).
/// Every other failure passes as it is. Listed after the logging, so the log
/// writes the kind and, apart, the store's failure as its cause; and before the
/// unit of work, so a failed commit is told too.
/// </summary>
public sealed class StoreFailuresMiddleware<TCommand, TResult>(IStoreFailures failures) : IPipelineBehavior<TCommand, TResult>
    where TCommand : notnull
{
    public async Task<TResult> Handle(TCommand command, RequestHandlerDelegate<TResult> next, CancellationToken cancellationToken)
    {
        try
        {
            return await next(cancellationToken);
        }
        catch (Exception failure) when (failures.KindOf(failure) is { } told)
        {
            throw told;
        }
    }
}
