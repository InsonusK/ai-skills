// Source: skill base-plateau-in-dotnet

using System.Diagnostics;
using BuildingBlocks.Logging;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Pipeline;

/// <summary>
/// A pipeline behavior - the decorator of every command handler: it calls the
/// next step and writes two records per call, so the log reads top-down as the
/// call ran - "command called" with the command as the payload, then "command
/// handled" with the duration and the result as the payload, or "command
/// failed" at warning level with the exception and the command again, so the
/// input of a failure is in the log even when only failed records keep their
/// payload. Whether the payload reaches the output is the provider's choice
/// (<see cref="JsonLogProvider"/>), never this behavior's.
///
/// <typeparamref name="TCommand"/> is the command type - the input of one
/// operation (for example <c>PlanItemsCommand</c>); <typeparamref name="TResult"/> is
/// the result its handler returns (for example <c>PlanItemsResult</c>).
/// MediatR closes the behavior for every command, so it is written once.
///
/// Every cross-cutting step (a unit of work, a metric, an access check, ...)
/// follows the same pattern: one behavior per file, listed in the
/// <see cref="CommandDecorators.AddCommandDecorators"/> call of the
/// "// [handlers]" section of Program.cs.
/// </summary>
public sealed class LoggingBehavior<TCommand, TResult>(ILogger<LoggingBehavior<TCommand, TResult>> logger)
    : IPipelineBehavior<TCommand, TResult>
    where TCommand : notnull
{
    // The command's name in the log: the operation, as every stack logs it.
    private static readonly string Name = OperationName(typeof(TCommand).Name);

    public async Task<TResult> Handle(TCommand command, RequestHandlerDelegate<TResult> next, CancellationToken cancellationToken)
    {
        var name = Name;
        logger.Write(LogLevel.Information, "command called", LogAttr.Of("command", name), LogAttr.Payload(command));
        var started = Stopwatch.GetTimestamp();
        TResult result;
        try
        {
            result = await next(cancellationToken);
        }
        catch (Exception error)
        {
            logger.Write(LogLevel.Warning, "command failed", error,
                LogAttr.Of("command", name), LogAttr.Of("duration", Stopwatch.GetElapsedTime(started)),
                LogAttr.Failed(true), LogAttr.Payload(command));
            throw;
        }
        logger.Write(LogLevel.Information, "command handled",
            LogAttr.Of("command", name), LogAttr.Of("duration", Stopwatch.GetElapsedTime(started)),
            LogAttr.Failed(false), LogAttr.Payload(result));
        return result;
    }

    // PlanItemsCommand -> PlanItems; a type without the suffix keeps its name.
    private static string OperationName(string type) =>
        type.EndsWith("Command", StringComparison.Ordinal) ? type[..^"Command".Length] : type;
}
