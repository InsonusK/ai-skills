using MediatR;
using Middleware.Access;
using Shared.Access;

namespace Middleware.BatchLimit;

/// <summary>
/// The batch limit, the command middleware that decides by a value of the
/// command: a command marked <see cref="IBatch"/> whose size is above
/// <see cref="AccessConfig.MaxBatch"/> is rejected as resource exhausted
/// before its handler runs; a batch within the limit and every unmarked
/// command pass unchanged.
/// </summary>
public sealed class BatchLimitMiddleware<TCommand, TResult>(AccessConfig config) : IPipelineBehavior<TCommand, TResult>
    where TCommand : notnull
{
    private static readonly string Name = typeof(TCommand).Name.Replace("Command", "");

    public Task<TResult> Handle(TCommand command, RequestHandlerDelegate<TResult> next, CancellationToken cancellationToken) =>
        command is IBatch batch && batch.BatchSize > config.MaxBatch
            ? throw AccessErrors.BatchTooLarge(Name, config.MaxBatch, batch.BatchSize)
            : next(cancellationToken);
}
