// Source: skill base-plateau-in-dotnet

using BuildingBlocks.Logging;
using Microsoft.Extensions.Logging;
using Shared.Errors;

namespace App.Host.Api.Entry;

/// <summary>
/// Runs the application's entry points - the startup command, an HTTP server,
/// a gRPC server, the health endpoints - together in one process, and turns
/// how they ended into the process exit code. It is what lets every variation
/// point attach by adding or replacing one <see cref="Entry"/> in the
/// "// [entries]" section of Program.cs: no variation point writes code to run
/// itself beside another.
/// </summary>
public static class EntryRunner
{
    /// <summary>Every entry ended without an error.</summary>
    public const int ExitOk = 0;

    /// <summary>The service failed, or a dependency is unavailable: a retry may help.</summary>
    public const int ExitFailure = 1;

    /// <summary>The request was rejected: its error has a kind other than unavailable.</summary>
    public const int ExitRejected = 2;

    /// <summary>
    /// Runs every entry at once and waits until the first one ends - its work
    /// done, its error, or <paramref name="stopping"/> cancelled on SIGTERM.
    /// Then it cancels the others, waits for them to stop, and returns the
    /// first error any entry ended with, or null. So one failed entry stops the
    /// whole process instead of leaving it half-alive, and a one-shot entry
    /// ends the process when it is done.
    /// </summary>
    public static async Task<Exception?> RunAllAsync(ILogger logger, CancellationToken stopping, params Entry[] entries)
    {
        using var stopAll = CancellationTokenSource.CreateLinkedTokenSource(stopping);
        // Task.Run: an entry that blocks before its first await still runs
        // beside the others.
        var running = entries.ToDictionary(entry => Task.Run(() => entry.RunAsync(stopAll.Token)), entry => entry);

        Exception? first = null;
        while (running.Count > 0)
        {
            var ended = await Task.WhenAny(running.Keys);
            var entry = running[ended];
            running.Remove(ended);
            var error = ErrorOf(ended, stopAll.IsCancellationRequested);
            if (error is not null)
            {
                if (!entry.ReportsErrors)
                {
                    logger.Write(LogLevel.Error, "entry point stopped with an error", error, LogAttr.Of("entry", entry.Name));
                }
                first ??= error;
            }
            await stopAll.CancelAsync(); // the first entry ended: stop the others
        }
        return first;
    }

    // The error an entry ended with; a cancellation after the stop was asked
    // for is how an entry stops, not an error.
    private static Exception? ErrorOf(Task ended, bool stopAsked) => ended switch
    {
        { IsCanceled: true } when stopAsked => null,
        { IsFaulted: true } when stopAsked && ended.Exception!.InnerException is OperationCanceledException => null,
        { IsFaulted: true } => ended.Exception!.InnerException,
        { IsCanceled: true } => new OperationCanceledException("the entry point was cancelled"),
        _ => null,
    };

    /// <summary>
    /// Maps the error <see cref="RunAllAsync"/> returned to the exit code by
    /// its kind (skill error-kinds): null is <see cref="ExitOk"/>; unavailable,
    /// and an error with no kind - a server that could not start - are
    /// <see cref="ExitFailure"/>; any other kind, which only a one-shot command
    /// returns, is <see cref="ExitRejected"/>.
    /// </summary>
    public static int ExitCode(Exception? error) => error switch
    {
        null => ExitOk,
        _ => ErrorKindException.KindOf(error) switch
        {
            null or ErrorKind.Unavailable => ExitFailure,
            _ => ExitRejected,
        },
    };
}
