using MediatR;
using Shared.Access;

namespace Access;

/// <summary>
/// The decorator that decides by a value of the command: a command marked
/// <see cref="IBatch"/> whose size is above <see cref="AccessConfig.MaxBatch"/>
/// is rejected as resource exhausted before its handler runs; a batch within
/// the limit and every unmarked command pass unchanged.
/// </summary>
public sealed class BatchLimitBehavior<TCommand, TResult>(AccessConfig config) : IPipelineBehavior<TCommand, TResult>
    where TCommand : notnull
{
    private static readonly string Name = typeof(TCommand).Name.Replace("Command", "");

    public Task<TResult> Handle(TCommand command, RequestHandlerDelegate<TResult> next, CancellationToken cancellationToken) =>
        command is IBatch batch && batch.BatchSize > config.MaxBatch
            ? throw AccessErrors.BatchTooLarge(Name, config.MaxBatch, batch.BatchSize)
            : next(cancellationToken);
}
