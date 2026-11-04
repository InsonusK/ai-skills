// Source: skill vp-unit-of-work-in-dotnet

using MediatR;
using Shared.UnitOfWork;

namespace Middleware.UnitOfWork;

/// <summary>
/// The command middleware that keeps a command's writes together: the outermost
/// command of a scope begins the unit of work and ends it, a commit when its
/// handler returned and a rollback when it threw; a nested command only joins.
/// Listed last, so a command the middleware before it reject opens nothing and a
/// failed commit reaches the log as the command's failure.
/// </summary>
public sealed class UnitOfWorkMiddleware<TCommand, TResult>(IUnitOfWork unitOfWork) : IPipelineBehavior<TCommand, TResult>
    where TCommand : notnull
{
    public async Task<TResult> Handle(TCommand command, RequestHandlerDelegate<TResult> next, CancellationToken cancellationToken)
    {
        if (!unitOfWork.TryBegin())
        {
            return await next(cancellationToken);
        }
        try
        {
            var result = await next(cancellationToken);
            // CancellationToken.None: the work is done, and a caller that left now
            // must not undo it. A rollback must run for a cancelled caller too.
            await unitOfWork.CommitAsync(CancellationToken.None);
            return result;
        }
        catch
        {
            await unitOfWork.RollbackAsync(CancellationToken.None);
            throw;
        }
    }
}
