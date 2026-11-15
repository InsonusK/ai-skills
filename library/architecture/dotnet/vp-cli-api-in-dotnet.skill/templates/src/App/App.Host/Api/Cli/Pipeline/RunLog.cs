// Source: skill vp-cli-api-in-dotnet

using BuildingBlocks.Logging;
using Microsoft.Extensions.Logging;

namespace App.Host.Api.Cli.Pipeline;

/// <summary>
/// The run log of the command line: two records per run through the
/// composition root's logger - one when the arguments are parsed, with what
/// arrived as the payload, one when the run is answered, with the action and
/// the duration, whether it failed, and as the payload the output as printed;
/// a failed run repeats the arguments as given, so they are in the log even
/// when only failed records keep their payload, and even when the arguments
/// could not be parsed and no "cli call received" was written. The data goes
/// only as the payload attribute, so the base handler drops it and the logging
/// variation point masks it.
/// </summary>
public static class RunLog
{
    /// <summary>The first record of a run, once its arguments are parsed: the action, and the flags given as the payload.</summary>
    public static void Received(ILogger logger, string action, IReadOnlyDictionary<string, string> flags) =>
        logger.Write(LogLevel.Information, "cli call received",
            LogAttr.Of("action", action),
            LogAttr.Payload(new Dictionary<string, object?> { ["flags"] = flags }));

    /// <summary>The last record of a run: the action, the duration, failed, and as the payload the output as printed; on a failure also the arguments as given.</summary>
    public static void Answered(ILogger logger, string action, TimeSpan duration, bool failed, string output, IReadOnlyList<string>? args)
    {
        var payload = new Dictionary<string, object?> { ["output"] = output };
        if (args is not null)
        {
            payload["args"] = args;
        }
        logger.Write(LogLevel.Information, "cli call answered",
            LogAttr.Of("action", action),
            LogAttr.Of("duration", duration),
            LogAttr.Failed(failed),
            LogAttr.Payload(payload));
    }

    /// <summary>The record of an action that crashed: the fault, answered as a failure with no kind.</summary>
    public static void Crashed(ILogger logger, Exception crash) =>
        logger.Write(LogLevel.Error, "cli action crashed", crash);
}
