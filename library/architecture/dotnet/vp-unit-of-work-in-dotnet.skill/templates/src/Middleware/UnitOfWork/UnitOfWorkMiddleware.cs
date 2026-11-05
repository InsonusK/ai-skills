// Source: skill vp-unit-of-work-in-dotnet

using MediatR;
using Shared.Errors;
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
    /// <summary>The message of a failed commit, the same in every stack.</summary>
    public const string NotCommitted = "the changes could not be committed: the database does not answer";

    public async Task<TResult> Handle(TCommand command, RequestHandlerDelegate<TResult> next, CancellationToken cancellationToken)
    {
        if (!unitOfWork.TryBegin())
        {
            return await next(cancellationToken);
        }
        TResult result;
        try
        {
            result = await next(cancellationToken);
        }
        catch
        {
            // A rollback must run for a cancelled caller too.
            await unitOfWork.RollbackAsync(CancellationToken.None);
            throw;
        }
        try
        {
            // CancellationToken.None: the work is done, and a caller that left now
            // must not undo it.
            await unitOfWork.CommitAsync(CancellationToken.None);
        }
        catch (Exception failure)
        {
            await unitOfWork.RollbackAsync(CancellationToken.None);
            // The commit may have reached the database: the caller is told it is
            // unavailable, never the database's address; the cause is for the log.
            throw new ErrorKindException(ErrorKind.Unavailable, NotCommitted, failure);
        }
        return result;
    }
}
