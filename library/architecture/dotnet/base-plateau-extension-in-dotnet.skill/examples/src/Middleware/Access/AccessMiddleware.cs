using MediatR;
using Shared.Access;

namespace Middleware.Access;

/// <summary>
/// The access check, the command middleware that filters by the command's
/// mark: a command marked <see cref="IRestricted"/> runs only for a caller
/// with its role - no caller is unauthenticated, a caller without the role is
/// permission denied; every other command passes unchanged. List it after the logging middleware, so a
/// rejected call is logged as failed.
/// </summary>
public sealed class AccessMiddleware<TCommand, TResult> : IPipelineBehavior<TCommand, TResult>
    where TCommand : notnull
{
    // The command's name in a rejection: the operation, as the log names it.
    private static readonly string Name = typeof(TCommand).Name.Replace("Command", "");

    public Task<TResult> Handle(TCommand command, RequestHandlerDelegate<TResult> next, CancellationToken cancellationToken)
    {
        if (command is not IRestricted restricted)
        {
            return next(cancellationToken);
        }
        var caller = CallerIdentity.Current ?? throw AccessErrors.NoCaller(Name);
        if (!caller.HasRole(restricted.RequiredRole))
        {
            throw AccessErrors.MissingRole(Name, restricted.RequiredRole);
        }
        return next(cancellationToken);
    }
}
